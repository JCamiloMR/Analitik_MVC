using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Dashboards fijos (4 tipos) - NO personalizables
/// </summary>
public partial class Dashboard
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public TipoDashboard TipoDashboard { get; set; }

    public bool Activo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<Metrica> Metricas { get; set; } = new List<Metrica>();
}
