using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Historial de movimientos de inventario
/// </summary>
public partial class MovimientosInventario
{
    public Guid Id { get; set; }

    public Guid InventarioId { get; set; }

    public Guid EmpresaId { get; set; }

    public MovimientoInventario TipoMovimiento { get; set; }

    public int Cantidad { get; set; }

    public int CantidadAnterior { get; set; }

    public int CantidadNueva { get; set; }

    public string Motivo { get; set; } = null!;

    public string? Referencia { get; set; }

    public string? NumeroDocumento { get; set; }

    public Guid? VentaId { get; set; }

    public string? Lote { get; set; }

    public string? UbicacionOrigen { get; set; }

    public string? UbicacionDestino { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string? Observaciones { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual Inventario Inventario { get; set; } = null!;

    public virtual Venta? Venta { get; set; }
}
