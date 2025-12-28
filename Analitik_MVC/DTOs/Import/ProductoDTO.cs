namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para importación de productos desde Excel
/// Mapea directamente con hoja PRODUCTOS
/// </summary>
public class ProductoDTO
{
    // Obligatorios
    public string CodigoProducto { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public decimal PrecioVenta { get; set; }
    public string UnidadMedida { get; set; } = null!;
    public bool RequiereInventario { get; set; }
    public bool Activo { get; set; }
    
    // Opcionales
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public decimal? CostoUnitario { get; set; }
    public decimal? PrecioSugerido { get; set; }
    public decimal? PesoKg { get; set; }
    public decimal? VolumenM3 { get; set; }
    public string? CodigoBarras { get; set; }
    public string? CodigoQr { get; set; }
    public bool EsServicio { get; set; }
    public string? Descripcion { get; set; }
    
    // Metadatos
    public Guid EmpresaId { get; set; }
    public int FilaOrigen { get; set; }
}
