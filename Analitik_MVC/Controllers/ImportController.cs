using Analitik_MVC.Data;
using Analitik_MVC.DTOs.Import;
using Analitik_MVC.Enums;
using Analitik_MVC.Services.Data;
using Analitik_MVC.Services.Database;
using Analitik_MVC.Services.Excel;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Analitik_MVC.Controllers;

/// <summary>
/// Controlador para importación de datos desde Excel
/// Implementa pipeline ETL completo con validación en 3 niveles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly ILogger<ImportController> _logger;
    private readonly AnalitikDbContext _dbContext;
    private readonly ExcelValidationService _excelValidationService;
    private readonly ExcelReaderService _excelReaderService;
    private readonly DataTransformationService _transformationService;
    private readonly DatabaseLoaderService _databaseLoaderService;
    private readonly ImportLogService _importLogService;

    public ImportController(
        ILogger<ImportController> logger,
        AnalitikDbContext dbContext,
        ExcelValidationService excelValidationService,
        ExcelReaderService excelReaderService,
        DataTransformationService transformationService,
        DatabaseLoaderService databaseLoaderService,
        ImportLogService importLogService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _excelValidationService = excelValidationService;
        _excelReaderService = excelReaderService;
        _transformationService = transformationService;
        _databaseLoaderService = databaseLoaderService;
        _importLogService = importLogService;
    }

    /// <summary>
    /// Endpoint principal: POST /api/import/excel
    /// Recibe archivo Excel y lo procesa completamente
    /// </summary>
    [HttpPost("excel")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB límite
    public async Task<IActionResult> ImportarExcel([FromForm] IFormFile archivo, [FromForm] Guid empresaId)
    {
        _logger.LogInformation("Iniciando importación de Excel para empresa {EmpresaId}", empresaId);

        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new ImportResultDTO
            {
                Exitoso = false,
                Mensaje = "No se recibió ningún archivo",
                Errores = new List<ErrorValidacion>
                {
                    new ErrorValidacion
                    {
                        Fila = 0,
                        Columna = "Archivo",
                        Error = "Archivo no encontrado",
                        Sugerencia = "Asegúrate de adjuntar un archivo Excel (.xlsx)"
                    }
                }
            });
        }

        Guid? importacionId = null;

        try
        {
            // ======================================
            // NIVEL 1: VALIDACIÓN DE ARCHIVO
            // ======================================
            using var archivoStream = archivo.OpenReadStream();
            
            var validacionArchivo = _excelValidationService.ValidarArchivoExcel(
                archivoStream, 
                archivo.FileName, 
                archivo.Length);

            if (!validacionArchivo.IsSuccess)
            {
                return BadRequest(new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = "El archivo no pasó la validación inicial",
                    Errores = validacionArchivo.Errors
                });
            }

            // Registrar importación en BD (log inicial)
            archivoStream.Position = 0;
            importacionId = await _importLogService.RegistrarImportacionAsync(
                empresaId,
                archivo.FileName,
                archivo.Length,
                archivoStream);

            _logger.LogInformation("Importación {ImportId} registrada", importacionId);

            // ======================================
            // NIVEL 2: VALIDACIÓN DE ESTRUCTURA
            // ======================================
            archivoStream.Position = 0;
            XLWorkbook workbook;
            try
            {
                workbook = new XLWorkbook(archivoStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir workbook");
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    new List<ErrorValidacion>
                    {
                        new ErrorValidacion
                        {
                            Fila = 0,
                            Columna = "Archivo",
                            Error = $"Error al abrir Excel: {ex.Message}"
                        }
                    },
                    FaseEtl.Extraccion,
                    "Error al abrir archivo Excel");

                return BadRequest(new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = "Error al abrir archivo Excel",
                    ImportId = importacionId,
                    Errores = new List<ErrorValidacion>
                    {
                        new ErrorValidacion
                        {
                            Fila = 0,
                            Columna = "Archivo",
                            Error = ex.Message,
                            Sugerencia = "Verifica que el archivo no esté corrupto"
                        }
                    }
                });
            }

            var validacionEstructura = _excelValidationService.ValidarEstructuraHojas(workbook);
            if (!validacionEstructura.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionEstructura.Errors,
                    FaseEtl.Extraccion,
                    "Error en estructura de hojas");

                return BadRequest(new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = "El archivo no cumple con la estructura requerida",
                    ImportId = importacionId,
                    Errores = validacionEstructura.Errors,
                    Advertencias = validacionEstructura.Warnings
                });
            }

            // Actualizar progreso
            await _importLogService.ActualizarEstadoImportacionAsync(
                importacionId.Value,
                EstadoImportacion.EnProceso,
                FaseEtl.Transformacion,
                progreso: 25);

            // ======================================
            // NIVEL 3: LECTURA Y MAPEO DE DATOS
            // ======================================
            var erroresGlobales = new List<ErrorValidacion>();
            var advertenciasGlobales = new List<string>();

            // 3.1 Leer PRODUCTOS
            var hojaProductos = workbook.Worksheet("PRODUCTOS");
            var (productos, validacionProductos) = _excelReaderService.LeerYMapearProductos(hojaProductos, empresaId);
            
            if (!validacionProductos.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionProductos.Errors,
                    FaseEtl.Transformacion,
                    "Errores en hoja PRODUCTOS");

                return BadRequest(new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = "Errores en hoja PRODUCTOS. Corrígelos y vuelve a intentar.",
                    ImportId = importacionId,
                    Errores = validacionProductos.Errors,
                    Advertencias = validacionProductos.Warnings
                });
            }

            advertenciasGlobales.AddRange(validacionProductos.Warnings);

            // Actualizar progreso
            await _importLogService.ActualizarEstadoImportacionAsync(
                importacionId.Value,
                EstadoImportacion.EnProceso,
                FaseEtl.Transformacion,
                progreso: 40);

            // 3.2 Leer INVENTARIO
            var hojaInventario = workbook.Worksheet("INVENTARIO");
            var (inventarios, validacionInventario) = _excelReaderService.LeerYMapearInventario(
                hojaInventario, 
                productos, 
                empresaId);

            if (!validacionInventario.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionInventario.Errors,
                    FaseEtl.Transformacion,
                    "Errores en hoja INVENTARIO");

                return BadRequest(new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = "Errores en hoja INVENTARIO. Corrígelos y vuelve a intentar.",
                    ImportId = importacionId,
                    Errores = validacionInventario.Errors,
                    Advertencias = validacionInventario.Warnings
                });
            }

            advertenciasGlobales.AddRange(validacionInventario.Warnings);

            // Actualizar progreso
            await _importLogService.ActualizarEstadoImportacionAsync(
                importacionId.Value,
                EstadoImportacion.EnProceso,
                FaseEtl.Transformacion,
                progreso: 55);

            // 3.3 Leer VENTAS
            var hojaVentas = workbook.Worksheet("VENTAS");
            var (ventas, validacionVentas) = _excelReaderService.LeerYMapearVentas(
                hojaVentas, 
                productos, 
                empresaId);

            if (!validacionVentas.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionVentas.Errors,
                    FaseEtl.Transformacion,
                    "Errores en hoja VENTAS");

                return BadRequest(new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = "Errores en hoja VENTAS. Corrígelos y vuelve a intentar.",
                    ImportId = importacionId,
                    Errores = validacionVentas.Errors,
                    Advertencias = validacionVentas.Warnings
                });
            }

            advertenciasGlobales.AddRange(validacionVentas.Warnings);

            // Actualizar progreso
            await _importLogService.ActualizarEstadoImportacionAsync(
                importacionId.Value,
                EstadoImportacion.EnProceso,
                FaseEtl.Transformacion,
                progreso: 70);

            // 3.4 Leer FINANCIEROS
            var hojaFinancieros = workbook.Worksheet("FINANCIEROS");
            var (financieros, validacionFinancieros) = _excelReaderService.LeerYMapearFinancieros(
                hojaFinancieros, 
                empresaId);

            if (!validacionFinancieros.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionFinancieros.Errors,
                    FaseEtl.Transformacion,
                    "Errores en hoja FINANCIEROS");

                return BadRequest(new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = "Errores en hoja FINANCIEROS. Corrígelos y vuelve a intentar.",
                    ImportId = importacionId,
                    Errores = validacionFinancieros.Errors,
                    Advertencias = validacionFinancieros.Warnings
                });
            }

            advertenciasGlobales.AddRange(validacionFinancieros.Warnings);

            // ======================================
            // NIVEL 4: CARGA ATÓMICA A BD
            // ======================================
            await _importLogService.ActualizarEstadoImportacionAsync(
                importacionId.Value,
                EstadoImportacion.EnProceso,
                FaseEtl.Carga,
                progreso: 85);

            ResumenImportacion resumen;
            try
            {
                resumen = await _databaseLoaderService.CargarDatosAtomicamente(
                    productos,
                    inventarios,
                    ventas,
                    financieros,
                    empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en carga atómica de datos");
                
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    new List<ErrorValidacion>
                    {
                        new ErrorValidacion
                        {
                            Fila = 0,
                            Error = $"Error al cargar datos: {ex.Message}",
                            Sugerencia = "Contacta soporte si el problema persiste"
                        }
                    },
                    FaseEtl.Carga,
                    $"Error en carga: {ex.Message}");

                return StatusCode(500, new ImportResultDTO
                {
                    Exitoso = false,
                    Mensaje = $"Error al cargar datos en base de datos: {ex.Message}",
                    ImportId = importacionId,
                    Errores = new List<ErrorValidacion>
                    {
                        new ErrorValidacion
                        {
                            Fila = 0,
                            Error = ex.Message
                        }
                    }
                });
            }

            // ======================================
            // NIVEL 5: REGISTRO DE LOGS Y RESPUESTA
            // ======================================
            await _importLogService.RegistrarCargaExitosaAsync(
                importacionId.Value,
                resumen,
                advertenciasGlobales);

            _logger.LogInformation(
                "Importación {ImportId} completada exitosamente: {Registros} registros",
                importacionId,
                resumen.RegistrosProcesados);

            return Ok(new ImportResultDTO
            {
                Exitoso = true,
                Mensaje = $"? Carga exitosa: {resumen.RegistrosProcesados} registros importados en {resumen.DuracionSegundos:F2} segundos",
                ImportId = importacionId,
                Advertencias = advertenciasGlobales,
                Resumen = resumen
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en importación");

            if (importacionId.HasValue)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    new List<ErrorValidacion>
                    {
                        new ErrorValidacion
                        {
                            Fila = 0,
                            Error = $"Error inesperado: {ex.Message}"
                        }
                    },
                    FaseEtl.Error,
                    ex.Message);
            }

            return StatusCode(500, new ImportResultDTO
            {
                Exitoso = false,
                Mensaje = $"Error inesperado: {ex.Message}",
                ImportId = importacionId,
                Errores = new List<ErrorValidacion>
                {
                    new ErrorValidacion
                    {
                        Fila = 0,
                        Error = ex.Message,
                        Sugerencia = "Contacta soporte técnico"
                    }
                }
            });
        }
    }

    /// <summary>
    /// GET /api/import/status/{importId}
    /// Consulta el estado de una importación
    /// </summary>
    [HttpGet("status/{importId}")]
    public async Task<IActionResult> GetEstadoImportacion(Guid importId)
    {
        var importacion = await _dbContext.ImportacionesDatos
            .Where(i => i.Id == importId)
            .Select(i => new
            {
                i.Id,
                i.Estado,
                i.FaseActual,
                i.ProgresoPorcentaje,
                i.RegistrosCargados,
                i.RegistrosRechazados,
                i.FechaImportacion,
                i.FechaFinEtl,
                i.DuracionSegundos,
                i.ResultadoCarga
            })
            .FirstOrDefaultAsync();

        if (importacion == null)
        {
            return NotFound(new { mensaje = "Importación no encontrada" });
        }

        return Ok(importacion);
    }

    /// <summary>
    /// GET /api/import/report/{importId}
    /// Descarga reporte detallado de una importación
    /// </summary>
    [HttpGet("report/{importId}")]
    public async Task<IActionResult> GetReporteImportacion(Guid importId)
    {
        var reporte = await _importLogService.GenerarReporteAsync(importId);
        
        if (reporte == null)
        {
            return NotFound(new { mensaje = "Reporte no encontrado" });
        }

        return Ok(reporte);
    }

    /// <summary>
    /// GET /api/import/history
    /// Lista historial de importaciones de una empresa
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistorialImportaciones([FromQuery] Guid empresaId, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        var importaciones = await _dbContext.ImportacionesDatos
            .Where(i => i.EmpresaId == empresaId)
            .OrderByDescending(i => i.FechaImportacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(i => new
            {
                i.Id,
                i.NombreArchivo,
                i.Estado,
                i.FechaImportacion,
                i.RegistrosCargados,
                i.DuracionSegundos
            })
            .ToListAsync();

        var total = await _dbContext.ImportacionesDatos
            .CountAsync(i => i.EmpresaId == empresaId);

        return Ok(new
        {
            datos = importaciones,
            total = total,
            pagina = pagina,
            tamano = tamano,
            totalPaginas = (int)Math.Ceiling(total / (double)tamano)
        });
    }
}
