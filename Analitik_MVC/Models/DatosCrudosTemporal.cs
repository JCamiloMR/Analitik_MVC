using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// Almacenamiento temporal durante ETL (se limpia después)
/// </summary>
public partial class DatosCrudosTemporal
{
    public Guid Id { get; set; }

    public Guid ImportacionId { get; set; }

    public int NumeroFila { get; set; }

    public string DatosJson { get; set; } = null!;

    public string? EstadoProcesamiento { get; set; }

    public string? ErroresValidacion { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ImportacionesDato Importacion { get; set; } = null!;
}
