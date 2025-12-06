using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// Sesiones de chat con IA (OpenAI API)
/// </summary>
public partial class ConversacionesIum
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? UltimoMensaje { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaUltimaActualizacion { get; set; }

    public bool Activa { get; set; }

    public bool Archivada { get; set; }

    public List<string>? Etiquetas { get; set; }

    public bool? Favorita { get; set; }

    public string? Contexto { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<MensajesIum> MensajesIa { get; set; } = new List<MensajesIum>();
}
