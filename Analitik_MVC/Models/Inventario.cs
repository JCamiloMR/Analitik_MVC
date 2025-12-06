using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Control de stock
/// </summary>
public partial class Inventario
{
    public Guid Id { get; set; }

    public Guid ProductoId { get; set; }

    public int CantidadDisponible { get; set; }

    public int CantidadReservada { get; set; }

    public int CantidadEnTransito { get; set; }

    public int StockMinimo { get; set; }

    public int? StockMaximo { get; set; }

    public int? PuntoReorden { get; set; }

    public EstadoStock EstadoStock { get; set; }

    public string? Ubicacion { get; set; }

    public string? Pasillo { get; set; }

    public string? Estante { get; set; }

    public string? Nivel { get; set; }

    public string? LoteActual { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public int? DiasAlertaVencimiento { get; set; }

    public DateTime UltimaActualizacion { get; set; }

    public DateTime? UltimaEntrada { get; set; }

    public DateTime? UltimaSalida { get; set; }

    public bool AlertasActivadas { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();

    public virtual Producto Producto { get; set; } = null!;
}
