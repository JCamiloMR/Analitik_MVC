using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Insights y sugerencias generadas por IA
/// </summary>
public partial class RecomendacionesIum
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public TipoRecomendacion TipoRecomendacion { get; set; }

    public Prioridad Prioridad { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? ImpactoEstimado { get; set; }

    public List<string>? AccionesSugeridas { get; set; }

    public EstadoRecomendacion Estado { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public DateTime? FechaVista { get; set; }

    public DateTime? FechaAplicacion { get; set; }

    public string? Resultados { get; set; }

    public decimal? EfectividadPorcentaje { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
