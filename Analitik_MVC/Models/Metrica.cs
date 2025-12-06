using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Valores calculados de KPIs predefinidos
/// </summary>
public partial class Metrica
{
    public Guid Id { get; set; }

    public Guid DashboardId { get; set; }

    public Guid CatalogoMetricaId { get; set; }

    public string? Unidad { get; set; }

    public decimal? ValorNumerico { get; set; }

    public string? ValorTexto { get; set; }

    public decimal? VariacionPorcentaje { get; set; }

    public decimal? VariacionValor { get; set; }

    public Tendencia Tendencia { get; set; }

    public Periodo Periodo { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public DateTime FechaCalculo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CatalogoMetrica CatalogoMetrica { get; set; } = null!;

    public virtual Dashboard Dashboard { get; set; } = null!;
}
