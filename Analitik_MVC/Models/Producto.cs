using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// Catálogo de productos y servicios
/// </summary>
public partial class Producto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? CategoriaId { get; set; }

    public string CodigoProducto { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Subcategoria { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal? CostoUnitario { get; set; }

    public decimal? PrecioSugerido { get; set; }

    public decimal? MargenPorcentaje { get; set; }

    public string UnidadMedida { get; set; } = null!;

    public decimal? PesoKg { get; set; }

    public decimal? VolumenM3 { get; set; }

    public string? CodigoBarras { get; set; }

    public string? CodigoQr { get; set; }

    public bool EsServicio { get; set; }

    public bool RequiereInventario { get; set; }

    public bool Vendible { get; set; }

    public bool Comprable { get; set; }

    public string? ImagenUrl { get; set; }

    public string? ImagenesAdicionales { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string? Especificaciones { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AnalisisProducto? AnalisisProducto { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual ICollection<DetallesVentum> DetallesVenta { get; set; } = new List<DetallesVentum>();

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual Inventario? Inventario { get; set; }
}
