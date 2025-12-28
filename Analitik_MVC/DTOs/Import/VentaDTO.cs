using Analitik_MVC.Enums;

namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para importación de ventas desde Excel
/// Mapea directamente con hoja VENTAS
/// </summary>
public class VentaDTO
{
    // Obligatorios
    public string NumeroOrden { get; set; } = null!;
    public DateTime FechaVenta { get; set; }
    public string ClienteNombre { get; set; } = null!;
    public decimal MontoTotal { get; set; }
    public string MetodoPago { get; set; } = null!;
    
    // Opcionales
    public string? NumeroFactura { get; set; }
    public string? ClienteDocumento { get; set; }
    public string? ClienteTelefono { get; set; }
    public string? ClienteEmail { get; set; }
    public string? ClienteDireccion { get; set; }
    public string? Ciudad { get; set; }
    public decimal MontoSubtotal { get; set; }
    public decimal? MontoDescuento { get; set; }
    public decimal? MontoImpuestos { get; set; }
    public string? EstadoPago { get; set; }
    public string? Vendedor { get; set; }
    public string? CanalVenta { get; set; }
    public string? Estado { get; set; }
    public string? Notas { get; set; }
    
    // Detalles de productos (opcional)
    public List<DetalleVentaDTO> Detalles { get; set; } = new();
    
    // Metadatos
    public Guid EmpresaId { get; set; }
    public int FilaOrigen { get; set; }
}
