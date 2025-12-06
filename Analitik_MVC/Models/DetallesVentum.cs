using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// Líneas de productos por venta
/// </summary>
public partial class DetallesVentum
{
    public Guid Id { get; set; }

    public Guid VentaId { get; set; }

    public Guid ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public decimal? CantidadDevuelta { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? PrecioOriginal { get; set; }

    public decimal? DescuentoPorcentaje { get; set; }

    public decimal DescuentoMonto { get; set; }

    public decimal Subtotal { get; set; }

    public decimal? ImpuestoPorcentaje { get; set; }

    public decimal ImpuestoMonto { get; set; }

    public decimal Total { get; set; }

    public decimal? CostoUnitario { get; set; }

    public decimal? CostoTotal { get; set; }

    public string? Notas { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual Venta Venta { get; set; } = null!;
}
