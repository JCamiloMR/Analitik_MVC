namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para detalle de venta (líneas de productos)
/// </summary>
public class DetalleVentaDTO
{
    public string NumeroOrden { get; set; } = null!;
    public string CodigoProducto { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal? DescuentoPorcentaje { get; set; }
    public decimal Subtotal { get; set; }
    
    // Metadatos
    public Guid EmpresaId { get; set; }
    public int FilaOrigen { get; set; }
}
