using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

public partial class VistaEmpresaCompletum
{
    public Guid? EmpresaId { get; set; }

    public string? NombreComercial { get; set; }

    public string? RazonSocial { get; set; }

    public string? Nit { get; set; }

    public string? DirectorNombreCompleto { get; set; }

    public string? DirectorCargo { get; set; }

    public string? LogoUrl { get; set; }

    public string? Ciudad { get; set; }

    public string? Departamento { get; set; }

    public bool? EmpresaActiva { get; set; }

    public Guid? CuentaId { get; set; }

    public string? CuentaEmail { get; set; }

    public string? CuentaNombreCompleto { get; set; }

    public string? CuentaDisplayName { get; set; }

    public string? CuentaAvatarUrl { get; set; }

    public string? CuentaTelefono { get; set; }

    public string? CuentaRol { get; set; }

    public bool? CuentaEsOwner { get; set; }

    public string? CuentaBio { get; set; }

    public string? CuentaUbicacion { get; set; }

    public bool? CuentaActiva { get; set; }

    public bool? CuentaVerificada { get; set; }

    public DateTime? UltimaSesion { get; set; }

    public string? ZonaHoraria { get; set; }

    public string? Moneda { get; set; }

    public DateOnly? FechaFinSuscripcion { get; set; }

    public long? NotificacionesPendientes { get; set; }
}
