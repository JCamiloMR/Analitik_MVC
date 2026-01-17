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
        Stream archivoStream, 
        DateTime? utcNow)
    {
        var nowUtc = utcNow ?? DateTime.UtcNow;

        var importacion = new ImportacionesDatosLogs
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            TipoDatos = "mixto",
            NombreArchivo = nombreArchivo,
            TamanoArchivo = tamanoArchivo,
            HashArchivo = await CalcularHashArchivoAsync(archivoStream),
            Estado = "en_proceso",
            FaseActual = "extraccion",
            ProgresoPorcentaje = 0,

            FechaImportacion = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc),
            FechaInicioEtl = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc),
            CreatedAt = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc)
        };


        await _dbContext.ImportacionesDatosLogs.AddAsync(importacion);
        await _dbContext.SaveChangesAsync();

        return importacion.Id;
    }

    /// <summary>
    /// Actualiza el estado de una importación
    /// </summary>
    public async Task ActualizarEstadoImportacionAsync(
        Guid importacionId,
        string estado,
        string fase,
        decimal? progreso = null,
        string? errores = null)
    {
        var importacion = await _dbContext.ImportacionesDatosLogs.FindAsync(importacionId);
        if (importacion == null)
        {
            _logger.LogWarning("Importación {Id} no encontrada", importacionId);
            return;
        }

        importacion.Estado = estado;
        importacion.FaseActual = fase;
        if (progreso.HasValue)
            importacion.ProgresoPorcentaje = (int)progreso.Value;

        if (!string.IsNullOrWhiteSpace(errores))
        {
            importacion.ErroresCarga = errores;
        }

        importacion.UpdatedAt = DateTime.UtcNow;

        _dbContext.ImportacionesDatosLogs.Update(importacion);
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
        var importacion = await _dbContext.ImportacionesDatosLogs.FindAsync(importacionId);
        if (importacion == null) return;

        importacion.Estado = "completado";
        importacion.FaseActual = "completado";
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

        _dbContext.ImportacionesDatosLogs.Update(importacion);
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
        string faseError,
        string mensajeError)
    {
        var importacion = await _dbContext.ImportacionesDatosLogs.FindAsync(importacionId);
        if (importacion == null) return;

        importacion.Estado = "fallido";
        importacion.FaseActual = faseError;
        importacion.FechaFinEtl = DateTime.UtcNow;

        // Calcular duración usando DateTime
        if (importacion.FechaInicioEtl.HasValue)
        {
            importacion.DuracionSegundos = (int)(DateTime.UtcNow - importacion.FechaInicioEtl.Value).TotalSeconds;
        }

        importacion.UpdatedAt = DateTime.UtcNow;

        // Clasificar errores por fase
        var erroresJson = JsonSerializer.Serialize(errores, new JsonSerializerOptions { WriteIndented = true });

        switch (faseError.ToLower())
        {
            case "extraccion":
                importacion.ErroresExtraccion = erroresJson;
                break;
            case "transformacion":
                importacion.ErroresTransformacion = erroresJson;
                break;
            case "carga":
                importacion.ErroresCarga = erroresJson;
                break;
        }

        var resultadoCarga = JsonSerializer.Serialize(new
        {
            exitoso = false,
            mensaje = mensajeError,
            fase_error = faseError,
            total_errores = errores.Count,
            errores = errores
        }, new JsonSerializerOptions { WriteIndented = true });

        importacion.ResultadoCarga = resultadoCarga;
        importacion.FechaFinEtl = DateTime.UtcNow;

        if (importacion.FechaInicioEtl.HasValue)
        {
            importacion.FechaInicioEtl =
                DateTime.SpecifyKind(importacion.FechaInicioEtl.Value, DateTimeKind.Utc);

            importacion.DuracionSegundos =
                (int)(DateTime.UtcNow - importacion.FechaInicioEtl.Value).TotalSeconds;
        }

        importacion.UpdatedAt = DateTime.UtcNow;

        _dbContext.ImportacionesDatosLogs.Update(importacion);

        var entry = _dbContext.Entry(importacion);

        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.ClrType == typeof(DateTime) && prop.CurrentValue is DateTime dt)
            {
                _logger.LogError(
                    "Fecha PROBLEMA: {Prop} = {Value} | Kind = {Kind}",
                    prop.Metadata.Name,
                    dt,
                    dt.Kind
                );
            }
        }

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
        var importacion = await _dbContext.ImportacionesDatosLogs.FindAsync(importacionId);
        if (importacion == null) return null;

        var reporte = new ImportReportDTO
        {
            ImportId = importacion.Id,
            FechaCarga = importacion.FechaImportacion,
            EmpresaId = importacion.EmpresaId,
            Estado = importacion.Estado,
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
