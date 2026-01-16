using System;

namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para reporte detallado de importación
/// Usado por el endpoint GET /api/import/report/{id}
/// </summary>
public class ImportReportDTO
{
    public Guid ImportId { get; set; }
    public DateTimeOffset FechaCarga { get; set; }
    public Guid EmpresaId { get; set; }
    public string Estado { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string ResultadoDetallado { get; set; } = null!;
    public Dictionary<string, List<ErrorValidacion>> ErroresPorHoja { get; set; } = new();
    public ResumenImportacion Resumen { get; set; } = null!;
}
