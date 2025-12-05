using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// Cuenta única de autenticación por empresa (1:1 con empresas)
/// </summary>
public partial class CuentaEmpresa
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Email { get; set; } = null!;

    public string? NombreCompleto { get; set; }

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string? Ubicacion { get; set; }

    public string? Telefono { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool Activa { get; set; }

    public bool Verificada { get; set; }

    public DateTime? EmailVerificadoEn { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? UltimaSesion { get; set; }

    public string? IpUltimaSesion { get; set; }

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiracion { get; set; }

    public string? TokenRecuperacion { get; set; }

    public DateTime? TokenExpiracion { get; set; }

    public int IntentosFallidos { get; set; }

    public DateTime? BloqueadaHasta { get; set; }

    public DateTime? FechaUltimoCambioPassword { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Rol { get; set; }

    public bool EsOwner { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
