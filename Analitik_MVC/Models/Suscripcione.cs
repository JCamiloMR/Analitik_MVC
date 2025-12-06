using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Gestión de planes SaaS y facturación
/// </summary>
public partial class Suscripcione
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public PlanSuscripcion Plan { get; set; }

    public EstadoSuscripcion Estado { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public DateOnly? FechaProximoCobro { get; set; }

    public int? DiasTrialRestantes { get; set; }

    public decimal PrecioMensual { get; set; }

    public decimal? DescuentoAplicado { get; set; }

    public decimal? PrecioFinal { get; set; }

    public string? MetodoPago { get; set; }

    public bool RenovacionAutomatica { get; set; }

    public int? UsuariosPermitidos { get; set; }

    public int? AlmacenamientoGb { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
