using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Transacciones de venta
/// </summary>
public partial class Venta
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? ClienteId { get; set; }

    public string NumeroOrden { get; set; } = null!;

    public string? NumeroFactura { get; set; }

    public string ClienteNombre { get; set; } = null!;

    public string? ClienteDocumento { get; set; }

    public string? ClienteTelefono { get; set; }

    public string? ClienteEmail { get; set; }

    public string? ClienteDireccion { get; set; }

    public decimal MontoSubtotal { get; set; }

    public decimal MontoDescuento { get; set; }

    public decimal MontoImpuestos { get; set; }

    public decimal MontoTotal { get; set; }

    public decimal? CostoTotal { get; set; }

    public decimal? MargenBruto { get; set; }

    public string? Categoria { get; set; }

    public string? CanalVenta { get; set; }

    public string? Vendedor { get; set; }

    public EstadoVenta Estado { get; set; }

    public MetodoPago MetodoPago { get; set; }

    public string? EstadoPago { get; set; }

    public DateTime FechaVenta { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public DateTime? FechaEntregaEstimada { get; set; }

    public string? Notas { get; set; }

    public string? NotasInternas { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<DetallesVentum> DetallesVenta { get; set; } = new List<DetallesVentum>();

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();
}
