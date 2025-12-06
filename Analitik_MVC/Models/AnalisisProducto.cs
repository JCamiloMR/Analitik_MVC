using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Métricas de productos
/// </summary>
public partial class AnalisisProducto
{
    public Guid Id { get; set; }

    public Guid ProductoId { get; set; }

    public int? DiasSinMovimiento { get; set; }

    public DateTime? FechaUltimoMovimiento { get; set; }

    public decimal? ValorInmovilizado { get; set; }

    public decimal? IndiceRotacion { get; set; }

    public int? PeriodoRotacionDias { get; set; }

    public int? VentasUltimos30Dias { get; set; }

    public int? VentasUltimos90Dias { get; set; }

    public int? VentasUltimos365Dias { get; set; }

    public decimal? IngresosUltimos30Dias { get; set; }

    public decimal? IngresosUltimos90Dias { get; set; }

    public TendenciaProducto Tendencia { get; set; }

    public decimal? VariacionVentasPorcentaje { get; set; }

    public decimal? VentaPromedioDiaria { get; set; }

    public decimal? VentaPromedioSemanal { get; set; }

    public int? DiasStockDisponible { get; set; }

    public char? ClasificacionAbc { get; set; }

    public decimal? ContribucionVentasPorcentaje { get; set; }

    public DateTime FechaCalculo { get; set; }

    public DateTime? ProximaActualizacion { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
