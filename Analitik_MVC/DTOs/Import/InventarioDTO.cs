namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para importación de inventario desde Excel
/// Mapea directamente con hoja INVENTARIO
/// </summary>
public class InventarioDTO
{
    // Obligatorios
    public string CodigoProducto { get; set; } = null!;
    public int CantidadDisponible { get; set; }
    
    // Opcionales
    public int? CantidadReservada { get; set; }
    public int? CantidadEnTransito { get; set; }
    public int? StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    public int? PuntoReorden { get; set; }
    public string? Ubicacion { get; set; }
    public string? Pasillo { get; set; }
    public string? Estante { get; set; }
    public string? Nivel { get; set; }
    public string? LoteActual { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? DiasAlertaVencimiento { get; set; }
    
    // Metadatos
    public Guid EmpresaId { get; set; }
    public int FilaOrigen { get; set; }
}
