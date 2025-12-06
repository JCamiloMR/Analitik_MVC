using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Preferencias empresariales
/// </summary>
public partial class ConfiguracionEmpresa
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Tema Tema { get; set; }

    public Idioma Idioma { get; set; }

    public string ZonaHoraria { get; set; } = null!;

    public string Moneda { get; set; } = null!;

    public string FormatoFecha { get; set; } = null!;

    public string FormatoNumero { get; set; } = null!;

    public int? PrimeraDiaSemana { get; set; }

    public bool NotificacionesEmailReportes { get; set; }

    public bool NotificacionesEmailAlertas { get; set; }

    public bool NotificacionesPush { get; set; }

    public bool NotificacionesApp { get; set; }

    public bool NotificacionesMarketing { get; set; }

    public bool NotificacionesSeguridad { get; set; }

    public FrecuenciaReporte FrecuenciaReportes { get; set; }

    public string? ConfiguracionPrivacidad { get; set; }

    public string? ConfiguracionReportes { get; set; }

    public string? ConfiguracionAlertas { get; set; }

    public string? ConfiguracionAvanzada { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
