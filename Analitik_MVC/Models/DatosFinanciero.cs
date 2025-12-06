using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Información financiera
/// </summary>
public partial class DatosFinanciero
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public TipoDatoFinanciero TipoDato { get; set; }

    public string Categoria { get; set; } = null!;

    public string? Subcategoria { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Monto { get; set; }

    public string? Moneda { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public DateOnly? FechaPago { get; set; }

    public Periodo Periodo { get; set; }

    public int? Anio { get; set; }

    public int? Mes { get; set; }

    public string? ComprobanteUrl { get; set; }

    public string? NumeroComprobante { get; set; }

    public string? Beneficiario { get; set; }

    public string? Observaciones { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
