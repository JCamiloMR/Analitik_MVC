using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Notificaciones empresariales
/// </summary>
public partial class Notificacione
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public TipoNotificacion TipoNotificacion { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public bool Leida { get; set; }

    public bool Archivada { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaLectura { get; set; }

    public DateTime? FechaArchivado { get; set; }

    public string? AccionUrl { get; set; }

    public string? AccionTexto { get; set; }

    public string? Icono { get; set; }

    public string? Color { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
