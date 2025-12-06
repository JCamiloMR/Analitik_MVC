using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Conexiones a BD externas para ETL
/// </summary>
public partial class FuentesDato
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public FuenteDatos TipoFuente { get; set; }

    public string? Host { get; set; }

    public int? Puerto { get; set; }

    public string? NombreBaseDatos { get; set; }

    public string? Usuario { get; set; }

    public string? PasswordEncriptado { get; set; }

    public bool? SslHabilitado { get; set; }

    public string? CertificadoSsl { get; set; }

    public string? ParametrosConexion { get; set; }

    public EstadoConexion EstadoConexion { get; set; }

    public DateTime? UltimaConexion { get; set; }

    public string? UltimoError { get; set; }

    public int? IntentosConexionFallidos { get; set; }

    public bool? SincronizacionAutomatica { get; set; }

    public string? FrecuenciaSincronizacion { get; set; }

    public DateTime? UltimaSincronizacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activa { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<ImportacionesDato> ImportacionesDatos { get; set; } = new List<ImportacionesDato>();
}
