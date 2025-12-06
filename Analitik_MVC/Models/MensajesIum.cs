using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Mensajes de IA consumiendo OpenAI API
/// </summary>
public partial class MensajesIum
{
    public Guid Id { get; set; }

    public Guid ConversacionId { get; set; }

    public TipoMensaje TipoMensaje { get; set; }

    public string Contenido { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public string? Metadata { get; set; }

    /// <summary>
    /// Total tokens (prompt + completion)
    /// </summary>
    public int? TokensUsados { get; set; }

    /// <summary>
    /// Costo según pricing OpenAI
    /// </summary>
    public decimal? CostoEstimadoUsd { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ConversacionesIum Conversacion { get; set; } = null!;
}
