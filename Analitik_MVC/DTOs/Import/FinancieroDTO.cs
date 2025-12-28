using Analitik_MVC.Enums;

namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para importación de datos financieros desde Excel
/// Mapea directamente con hoja FINANCIEROS
/// </summary>
public class FinancieroDTO
{
    // Obligatorios
    public string TipoDato { get; set; } = null!; // ingreso, gasto, costo, inversion
    public string Categoria { get; set; } = null!;
    public string Concepto { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateTime FechaRegistro { get; set; }
    
    // Opcionales
    public string? Subcategoria { get; set; }
    public string? Moneda { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? NumeroComprobante { get; set; }
    public string? Beneficiario { get; set; }
    public string? Observaciones { get; set; }
    
    // Metadatos
    public Guid EmpresaId { get; set; }
    public int FilaOrigen { get; set; }
}
