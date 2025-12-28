using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Analitik_MVC.Data;
using Analitik_MVC.DTOs.Import;
using Analitik_MVC.Enums;
using Analitik_MVC.Models;

namespace Analitik_MVC.Services.Database;

/// <summary>
/// Servicio para registro de logs de importación
/// Mantiene historial completo de cargas para auditoría
/// </summary>
public class ImportLogService
{
    private readonly AnalitikDbContext _dbContext;
    private readonly ILogger<ImportLogService> _logger;

    public ImportLogService(
        AnalitikDbContext dbContext,
        ILogger<ImportLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Registra una importación en la base de datos
    /// </summary>
    public async Task<Guid> RegistrarImportacionAsync(
        Guid empresaId,
        string nombreArchivo,
        long tamanoArchivo,
        Stream archivoStream)
    {
        var importacion = new ImportacionesDato
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            TipoFuente = FuenteDatos.Excel,
            TipoDatos = DatosImportacion.Mixto,
            NombreArchivo = nombreArchivo,
            TamanoArchivo = tamanoArchivo,
            HashArchivo = await CalcularHashArchivoAsync(archivoStream),
            Estado = EstadoImportacion.EnProceso,
            FaseActual = FaseEtl.Extraccion,
            ProgresoPorcentaje = 0,
            FechaImportacion = DateTime.UtcNow,
            FechaInicioEtl = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbContext.ImportacionesDatos.AddAsync(importacion);
        await _dbContext.SaveChangesAsync();

        return importacion.Id;
    }

    /// <summary>
    /// Actualiza el estado de una importación
    /// </summary>
    public async Task ActualizarEstadoImportacionAsync(
        Guid importacionId,
        EstadoImportacion estado,
        FaseEtl fase,
        decimal? progreso = null,
        string? errores = null)
    {
        var importacion = await _dbContext.ImportacionesDatos.FindAsync(importacionId);
        if (importacion == null)
        {
            _logger.LogWarning("Importación {Id} no encontrada", importacionId);
            return;
        }

        importacion.Estado = estado;
        importacion.FaseActual = fase;
        if (progreso.HasValue)
            importacion.ProgresoPorcentaje = progreso.Value;

        if (!string.IsNullOrWhiteSpace(errores))
        {
            importacion.ErroresCarga = errores;
        }

        importacion.UpdatedAt = DateTime.UtcNow;

        _dbContext.ImportacionesDatos.Update(importacion);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Registra la finalización exitosa de una importación
    /// </summary>
    public async Task RegistrarCargaExitosaAsync(
        Guid importacionId,
        ResumenImportacion resumen,
        List<string>? advertencias = null)
    {
        var importacion = await _dbContext.ImportacionesDatos.FindAsync(importacionId);
        if (importacion == null) return;

        importacion.Estado = EstadoImportacion.Completado;
        importacion.FaseActual = FaseEtl.Completado;
        importacion.ProgresoPorcentaje = 100;
        importacion.FechaFinEtl = DateTime.UtcNow;
        importacion.DuracionSegundos = (int)resumen.DuracionSegundos;

        importacion.RegistrosExtraidos = resumen.RegistrosProcesados;
        importacion.RegistrosTransformados = resumen.RegistrosProcesados;
        importacion.RegistrosCargados = resumen.RegistrosProcesados;
        importacion.RegistrosRechazados = 0;

        // Serializar resumen completo
        var resultadoCarga = JsonSerializer.Serialize(new
        {
            exitoso = true,
            resumen = resumen,
            advertencias = advertencias ?? new List<string>()
        }, new JsonSerializerOptions { WriteIndented = true });

        importacion.ResultadoCarga = resultadoCarga;

        if (advertencias?.Any() == true)
        {
            importacion.Advertencias = JsonSerializer.Serialize(advertencias);
        }

        importacion.UpdatedAt = DateTime.UtcNow;

        _dbContext.ImportacionesDatos.Update(importacion);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Importación {Id} completada: {Registros} registros en {Duracion}s",
            importacionId,
            resumen.RegistrosProcesados,
            resumen.DuracionSegundos);
    }

    /// <summary>
    /// Registra una importación fallida con errores
    /// </summary>
    public async Task RegistrarCargaFallidaAsync(
        Guid importacionId,
        List<ErrorValidacion> errores,
        FaseEtl faseError,
        string mensajeError)
    {
        var importacion = await _dbContext.ImportacionesDatos.FindAsync(importacionId);
        if (importacion == null) return;

        importacion.Estado = EstadoImportacion.Fallido;
        importacion.FaseActual = faseError;
        importacion.FechaFinEtl = DateTime.UtcNow;
        importacion.DuracionSegundos = (int)(DateTime.UtcNow - importacion.FechaInicioEtl!.Value).TotalSeconds;

        // Clasificar errores por fase
        var erroresJson = JsonSerializer.Serialize(errores, new JsonSerializerOptions { WriteIndented = true });

        switch (faseError)
        {
            case FaseEtl.Extraccion:
                importacion.ErroresExtraccion = erroresJson;
                break;
            case FaseEtl.Transformacion:
                importacion.ErroresTransformacion = erroresJson;
                break;
            case FaseEtl.Carga:
                importacion.ErroresCarga = erroresJson;
                break;
        }

        var resultadoCarga = JsonSerializer.Serialize(new
        {
            exitoso = false,
            mensaje = mensajeError,
            fase_error = faseError.ToString(),
            total_errores = errores.Count,
            errores = errores
        }, new JsonSerializerOptions { WriteIndented = true });

        importacion.ResultadoCarga = resultadoCarga;
        importacion.UpdatedAt = DateTime.UtcNow;

        _dbContext.ImportacionesDatos.Update(importacion);
        await _dbContext.SaveChangesAsync();

        _logger.LogError(
            "Importación {Id} fallida en fase {Fase}: {Errores} errores",
            importacionId,
            faseError,
            errores.Count);
    }

    /// <summary>
    /// Genera reporte detallado de una importación
    /// </summary>
    public async Task<ImportReportDTO?> GenerarReporteAsync(Guid importacionId)
    {
        var importacion = await _dbContext.ImportacionesDatos.FindAsync(importacionId);
        if (importacion == null) return null;

        var reporte = new ImportReportDTO
        {
            ImportId = importacion.Id,
            FechaCarga = importacion.FechaImportacion,
            EmpresaId = importacion.EmpresaId,
            Estado = importacion.Estado.ToString(),
            NombreArchivo = importacion.NombreArchivo,
            ResultadoDetallado = importacion.ResultadoCarga ?? "Sin resultado",
            ErroresPorHoja = new Dictionary<string, List<ErrorValidacion>>()
        };

        // Parsear errores por hoja
        if (!string.IsNullOrWhiteSpace(importacion.ErroresExtraccion))
        {
            try
            {
                var errores = JsonSerializer.Deserialize<List<ErrorValidacion>>(importacion.ErroresExtraccion);
                if (errores != null)
                    reporte.ErroresPorHoja["Extraccion"] = errores;
            }
            catch { }
        }

        if (!string.IsNullOrWhiteSpace(importacion.ErroresTransformacion))
        {
            try
            {
                var errores = JsonSerializer.Deserialize<List<ErrorValidacion>>(importacion.ErroresTransformacion);
                if (errores != null)
                    reporte.ErroresPorHoja["Transformacion"] = errores;
            }
            catch { }
        }

        // Parsear resumen
        if (!string.IsNullOrWhiteSpace(importacion.ResultadoCarga))
        {
            try
            {
                var resultado = JsonSerializer.Deserialize<JsonElement>(importacion.ResultadoCarga);
                if (resultado.TryGetProperty("resumen", out var resumenElement))
                {
                    reporte.Resumen = JsonSerializer.Deserialize<ResumenImportacion>(resumenElement.GetRawText())
                        ?? new ResumenImportacion();
                }
            }
            catch { }
        }

        return reporte;
    }

    /// <summary>
    /// Calcula hash SHA-256 del archivo para detección de duplicados
    /// </summary>
    private async Task<string> CalcularHashArchivoAsync(Stream archivoStream)
    {
        archivoStream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(archivoStream);
        archivoStream.Position = 0;
        
        return Convert.ToHexString(hashBytes);
    }
}
