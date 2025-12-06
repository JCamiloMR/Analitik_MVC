using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Catálogo de KPIs predefinidos (fijos)
/// </summary>
public partial class CatalogoMetrica
{
    public Guid Id { get; set; }

    public string CodigoMetrica { get; set; } = null!;

    public string NombreMetrica { get; set; } = null!;

    public TipoDashboard TipoDashboard { get; set; }

    public TipoMetrica TipoMetrica { get; set; }

    public string? Descripcion { get; set; }

    public string? Unidad { get; set; }

    public string? Icono { get; set; }

    public string? FormulaSql { get; set; }

    public int? Orden { get; set; }

    public bool Activa { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Metrica> Metricas { get; set; } = new List<Metrica>();
}
