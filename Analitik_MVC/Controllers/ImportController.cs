using Analitik_MVC.Data;
using Analitik_MVC.DTOs;
using Analitik_MVC.DTOs.Import;
using Analitik_MVC.Enums;
using Analitik_MVC.Models;
using Analitik_MVC.Services.Data;
using Analitik_MVC.Services.Database;
using Analitik_MVC.Services.Excel;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
    public async Task<IActionResult> ImportarExcel([FromForm] IFormFile archivo)
    {
        Guid empresaId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        Guid id_empresa = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value!);
        _logger.LogInformation($"Iniciando importación de Excel para empresa {empresaId}");

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
                archivoStream,
                DateTime.UtcNow); // Cambiado de DateTime a DateTime

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
                    FaseEtl.Extraccion.ToString(),
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
                    FaseEtl.Extraccion.ToString(),
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
                EstadoImportacion.EnProceso.ToString(),
                FaseEtl.Transformacion.ToString(),
                progreso: 25);

            // ======================================
            // NIVEL 3: LECTURA Y MAPEO DE DATOS
            // ======================================
            var erroresGlobales = new List<ErrorValidacion>();
            var advertenciasGlobales = new List<string>();

            // 3.1 Leer PRODUCTOS
            var hojaProductos = workbook.Worksheet("PRODUCTOS");
            var (productos, validacionProductos) = _excelReaderService.LeerYMapearProductos(hojaProductos, id_empresa);

            if (!validacionProductos.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionProductos.Errors,
                    FaseEtl.Transformacion.ToString(),
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
                EstadoImportacion.EnProceso.ToString(),
                FaseEtl.Transformacion.ToString(),
                progreso: 40);

            // 3.2 Leer INVENTARIO
            var hojaInventario = workbook.Worksheet("INVENTARIO");
            var (inventarios, validacionInventario) = _excelReaderService.LeerYMapearInventario(
                hojaInventario,
                productos,
                id_empresa);

            if (!validacionInventario.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionInventario.Errors,
                    FaseEtl.Transformacion.ToString(),
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
                EstadoImportacion.EnProceso.ToString(),
                FaseEtl.Transformacion.ToString(),
                progreso: 55);

            // 3.3 Leer VENTAS
            var hojaVentas = workbook.Worksheet("VENTAS");
            var (ventas, validacionVentas) = _excelReaderService.LeerYMapearVentas(
                hojaVentas,
                productos,
                id_empresa);

            if (!validacionVentas.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionVentas.Errors,
                    FaseEtl.Transformacion.ToString(),
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
                EstadoImportacion.EnProceso.ToString(),
                FaseEtl.Transformacion.ToString(),
                progreso: 70);

            // 3.4 Leer FINANCIEROS
            var hojaFinancieros = workbook.Worksheet("FINANCIEROS");
            var (financieros, validacionFinancieros) = _excelReaderService.LeerYMapearFinancieros(
                hojaFinancieros,
                id_empresa);

            if (!validacionFinancieros.IsSuccess)
            {
                await _importLogService.RegistrarCargaFallidaAsync(
                    importacionId.Value,
                    validacionFinancieros.Errors,
                    FaseEtl.Transformacion.ToString(),
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
                EstadoImportacion.EnProceso.ToString(),
                FaseEtl.Carga.ToString(),
                progreso: 85);

            ResumenImportacion resumen;
            try
            {
                resumen = await _databaseLoaderService.CargarDatosAtomicamente(
                    productos,
                    inventarios,
                    ventas,
                    financieros,
                    id_empresa);
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
                    FaseEtl.Carga.ToString(),
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
                    FaseEtl.Error.ToString(),
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
    /// Retorna los DATOS IMPORTADOS en formato tabular para visualización tipo Excel
    /// CON PAGINACIÓN Y FILTRADO POR IMPORTACIÓN ESPECÍFICA
    /// </summary>
    [HttpGet("report/{importId}")]
    public async Task<IActionResult> GetReporteImportacion(
        Guid importId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? tipoHoja = null)
    {
        // Validar parámetros de paginación
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1 || tamanoPagina > 100) tamanoPagina = 10;

        var importacion = await _dbContext.ImportacionesDatos
            .Where(i => i.Id == importId)
            .Select(i => new
            {
                i.Id,
                i.NombreArchivo,
                i.FechaImportacion,
                i.Estado,
                i.EmpresaId,
                i.RegistrosCargados,
                i.TipoDatos,
                i.FechaInicioEtl,
                i.FechaFinEtl
            })
            .FirstOrDefaultAsync();

        if (importacion == null)
        {
            return NotFound(new { mensaje = "Importación no encontrada" });
        }

        // Calcular ventana de tiempo de la importación (±5 segundos para margen)
        var fechaInicio = importacion.FechaInicioEtl?.AddSeconds(-5) ?? importacion.FechaImportacion.AddSeconds(-5);
        var fechaFin = importacion.FechaFinEtl?.AddSeconds(5) ?? DateTime.UtcNow;

        // Respuesta base con metadata
        var response = new
        {
            metadata = new
            {
                importId = importacion.Id,
                nombreArchivo = importacion.NombreArchivo,
                fechaImportacion = importacion.FechaImportacion,
                estado = importacion.Estado,
                registrosCargados = importacion.RegistrosCargados,
                tipoDatos = importacion.TipoDatos
            },
            paginacion = new
            {
                paginaActual = pagina,
                tamanoPagina = tamanoPagina,
                totalRegistros = 0,
                totalPaginas = 0
            },
            datos = new Dictionary<string, object>()
        };

        // Si se especifica tipo de hoja, solo traer esa hoja
        var tipoHojaNormalizado = tipoHoja?.ToLower();

        // PRODUCTOS
        if (tipoHojaNormalizado == null || tipoHojaNormalizado == "productos")
        {
            var queryProductos = _dbContext.Productos
                .Where(p => p.EmpresaId == importacion.EmpresaId &&
                           p.CreatedAt >= fechaInicio &&
                           p.CreatedAt <= fechaFin)
                .OrderByDescending(p => p.CreatedAt);

            var totalProductos = await queryProductos.CountAsync();
            var productos = await queryProductos
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(p => new
                {
                    p.CodigoProducto,
                    p.Nombre,
                    p.Categoria,
                    p.Subcategoria,
                    p.Marca,
                    p.PrecioVenta,
                    p.CostoUnitario,
                    p.UnidadMedida,
                    p.Activo,
                    p.RequiereInventario,
                    p.CreatedAt
                })
                .ToListAsync();

            response.datos["productos"] = new
            {
                registros = productos,
                total = totalProductos,
                totalPaginas = (int)Math.Ceiling(totalProductos / (double)tamanoPagina)
            };

            if (tipoHojaNormalizado == "productos")
            {
                response.paginacion.GetType().GetProperty("totalRegistros")?.SetValue(response.paginacion, totalProductos);
                response.paginacion.GetType().GetProperty("totalPaginas")?.SetValue(response.paginacion, (int)Math.Ceiling(totalProductos / (double)tamanoPagina));
            }
        }

        // INVENTARIO
        if (tipoHojaNormalizado == null || tipoHojaNormalizado == "inventario")
        {
            var queryInventario = _dbContext.Inventarios
                .Where(i => i.Producto.EmpresaId == importacion.EmpresaId &&
                           i.CreatedAt >= fechaInicio &&
                           i.CreatedAt <= fechaFin)
                .OrderByDescending(i => i.CreatedAt);

            var totalInventario = await queryInventario.CountAsync();
            var inventario = await queryInventario
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(i => new
                {
                    CodigoProducto = i.Producto.CodigoProducto,
                    NombreProducto = i.Producto.Nombre,
                    i.CantidadDisponible,
                    i.CantidadReservada,
                    i.StockMinimo,
                    i.StockMaximo,
                    i.Ubicacion,
                    EstadoStock = i.EstadoStock.ToString(),
                    i.CreatedAt
                })
                .ToListAsync();

            response.datos["inventario"] = new
            {
                registros = inventario,
                total = totalInventario,
                totalPaginas = (int)Math.Ceiling(totalInventario / (double)tamanoPagina)
            };

            if (tipoHojaNormalizado == "inventario")
            {
                response.paginacion.GetType().GetProperty("totalRegistros")?.SetValue(response.paginacion, totalInventario);
                response.paginacion.GetType().GetProperty("totalPaginas")?.SetValue(response.paginacion, (int)Math.Ceiling(totalInventario / (double)tamanoPagina));
            }
        }

        // VENTAS
        if (tipoHojaNormalizado == null || tipoHojaNormalizado == "ventas")
        {
            var queryVentas = _dbContext.Ventas
                .Where(v => v.EmpresaId == importacion.EmpresaId &&
                           v.CreatedAt >= fechaInicio &&
                           v.CreatedAt <= fechaFin)
                .OrderByDescending(v => v.FechaVenta);

            var totalVentas = await queryVentas.CountAsync();
            var ventas = await queryVentas
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(v => new
                {
                    v.NumeroOrden,
                    v.FechaVenta,
                    v.ClienteNombre,
                    v.MontoSubtotal,
                    v.MontoDescuento,
                    v.MontoImpuestos,
                    v.MontoTotal,
                    MetodoPago = v.MetodoPago.ToString(),
                    Estado = v.Estado.ToString(),
                    v.CreatedAt
                })
                .ToListAsync();

            response.datos["ventas"] = new
            {
                registros = ventas,
                total = totalVentas,
                totalPaginas = (int)Math.Ceiling(totalVentas / (double)tamanoPagina)
            };

            if (tipoHojaNormalizado == "ventas")
            {
                response.paginacion.GetType().GetProperty("totalRegistros")?.SetValue(response.paginacion, totalVentas);
                response.paginacion.GetType().GetProperty("totalPaginas")?.SetValue(response.paginacion, (int)Math.Ceiling(totalVentas / (double)tamanoPagina));
            }
        }

        // FINANCIEROS
        if (tipoHojaNormalizado == null || tipoHojaNormalizado == "financieros")
        {
            var queryFinancieros = _dbContext.DatosFinancieros
                .Where(f => f.EmpresaId == importacion.EmpresaId &&
                           f.CreatedAt >= fechaInicio &&
                           f.CreatedAt <= fechaFin)
                .OrderByDescending(f => f.FechaRegistro);

            var totalFinancieros = await queryFinancieros.CountAsync();
            var financieros = await queryFinancieros
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(f => new
                {
                    TipoDato = f.TipoDato.ToString(),
                    f.Categoria,
                    f.Concepto,
                    f.Monto,
                    f.FechaRegistro,
                    f.Beneficiario,
                    f.CreatedAt
                })
                .ToListAsync();

            response.datos["financieros"] = new
            {
                registros = financieros,
                total = totalFinancieros,
                totalPaginas = (int)Math.Ceiling(totalFinancieros / (double)tamanoPagina)
            };

            if (tipoHojaNormalizado == "financieros")
            {
                response.paginacion.GetType().GetProperty("totalRegistros")?.SetValue(response.paginacion, totalFinancieros);
                response.paginacion.GetType().GetProperty("totalPaginas")?.SetValue(response.paginacion, (int)Math.Ceiling(totalFinancieros / (double)tamanoPagina));
            }
        }

        return Ok(response);
    }

    /// <summary>
    /// GET /api/import/history
    /// Lista historial de importaciones de una empresa
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistorialImportaciones([FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        Guid empresaId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

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

    [Authorize]
    [HttpGet("preview")]
    public IActionResult GetPreview()
    {
        var preview = _dbContext.Productos
    .Where(p => p.EmpresaId == new Guid("922096db-178b-457f-bcc3-d217761f093e") && p.Activo)
    .Select(p => new ProductPreviewDTO
    {
        ProductId = p.Id,
        ProductName = p.Nombre,
        ProductCode = p.CodigoProducto,
        // todas las órdenes asociadas al producto
        OrderNumbers = p.DetallesVenta
                        .Select(dv => dv.Venta.NumeroOrden)
                        .Distinct()
                        .ToList(),
        // totales por línea de venta sumados
        OrderTotals = p.DetallesVenta
                        .Select(dv => dv.Total)
                        .ToList(),
        Stock = p.Inventario != null ? p.Inventario.CantidadDisponible : 0
    })
    .OrderBy(p => p.ProductName)
    .ToList();

        return Ok(new
        {
            success = true,
            message = $"Vista previa cargada correctamente and {User.FindFirstValue(ClaimTypes.Name)}",
            data = preview
        });
    }
}
