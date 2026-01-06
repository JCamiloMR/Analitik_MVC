using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analitik_MVC.Models;

/// <summary>
/// Modelo para registro de importaciones de datos desde Excel
/// </summary>
[Table("importaciones_datos")]
public class ImportacionDatos
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("empresa_id")]
    public Guid EmpresaId { get; set; }

    // ===================================================================
    // METADATOS DEL ARCHIVO
    // ===================================================================

    [Required]
    [MaxLength(255)]
    [Column("nombre_archivo")]
    public string NombreArchivo { get; set; } = string.Empty;

    [Column("tamano_archivo")]
    public long? TamanoArchivo { get; set; }

    [MaxLength(64)]
    [Column("hash_archivo")]
    public string? HashArchivo { get; set; }

    [MaxLength(100)]
    [Column("tipo_datos")]
    public string? TipoDatos { get; set; }

    // ===================================================================
    // ESTADO DE LA IMPORTACIÓN
    // ===================================================================

    [Required]
    [MaxLength(50)]
    [Column("estado")]
    public string Estado { get; set; } = "en_proceso"; // en_proceso, completado, fallido

    [MaxLength(50)]
    [Column("fase_actual")]
    public string? FaseActual { get; set; } // extraccion, transformacion, carga, error

    [Column("progreso_porcentaje")]
    public int ProgresoPorcentaje { get; set; } = 0;

    // ===================================================================
    // CONTADORES DE REGISTROS
    // ===================================================================

    [Column("registros_extraidos")]
    public int RegistrosExtraidos { get; set; } = 0;

    [Column("registros_transformados")]
    public int RegistrosTransformados { get; set; } = 0;

    [Column("registros_cargados")]
    public int RegistrosCargados { get; set; } = 0;

    [Column("registros_rechazados")]
    public int RegistrosRechazados { get; set; } = 0;

    // ===================================================================
    // ERRORES Y ADVERTENCIAS (JSONB)
    // ===================================================================

    [Column("errores_extraccion", TypeName = "jsonb")]
    public string? ErroresExtraccion { get; set; } = "[]";

    [Column("errores_transformacion", TypeName = "jsonb")]
    public string? ErroresTransformacion { get; set; } = "[]";

    [Column("errores_carga", TypeName = "jsonb")]
    public string? ErroresCarga { get; set; } = "[]";

    [Column("advertencias", TypeName = "jsonb")]
    public string? Advertencias { get; set; } = "[]";

    // ===================================================================
    // FECHAS DE PROCESO
    // ===================================================================

    [Column("fecha_importacion")]
    public DateTime FechaImportacion { get; set; } = DateTime.UtcNow;

    [Column("fecha_inicio_etl")]
    public DateTime? FechaInicioEtl { get; set; }

    [Column("fecha_fin_etl")]
    public DateTime? FechaFinEtl { get; set; }

    [Column("duracion_segundos")]
    public int? DuracionSegundos { get; set; }

    // ===================================================================
    // RESULTADO FINAL
    // ===================================================================

    [Column("resultado_carga", TypeName = "jsonb")]
    public string? ResultadoCarga { get; set; }

    [Column("log_completo")]
    public string? LogCompleto { get; set; }

    // ===================================================================
    // AUDITORÍA
    // ===================================================================

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===================================================================
    // NAVEGACIÓN
    // ===================================================================

    [ForeignKey(nameof(EmpresaId))]
    public virtual Empresa? Empresa { get; set; }
}
