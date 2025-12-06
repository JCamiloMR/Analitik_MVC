using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Gestión de ETL - Extracción, Transformación y Carga
/// </summary>
public partial class ImportacionesDato
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? FuenteDatosId { get; set; }

    public FuenteDatos TipoFuente { get; set; }

    public DatosImportacion TipoDatos { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string? ArchivoUrl { get; set; }

    public long? TamanoArchivo { get; set; }

    public string? HashArchivo { get; set; }

    /// <summary>
    /// Mapeo entre columnas Excel/CSV y campos BD
    /// </summary>
    public string? MapeoColumnas { get; set; }

    public string? ReglasTransformacion { get; set; }

    public EstadoImportacion Estado { get; set; }

    public decimal? ProgresoPorcentaje { get; set; }

    public FaseEtl FaseActual { get; set; }

    public int? RegistrosExtraidos { get; set; }

    public int? RegistrosTransformados { get; set; }

    public int? RegistrosCargados { get; set; }

    public int? RegistrosRechazados { get; set; }

    public string? ResultadoCarga { get; set; }

    public DateTime FechaImportacion { get; set; }

    public DateTime? FechaInicioEtl { get; set; }

    public DateTime? FechaFinEtl { get; set; }

    public int? DuracionSegundos { get; set; }

    public string? ErroresExtraccion { get; set; }

    public string? ErroresTransformacion { get; set; }

    public string? ErroresCarga { get; set; }

    public string? Advertencias { get; set; }

    public string? LogEtl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DatosCrudosTemporal> DatosCrudosTemporals { get; set; } = new List<DatosCrudosTemporal>();

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual FuentesDato? FuenteDatos { get; set; }
}
