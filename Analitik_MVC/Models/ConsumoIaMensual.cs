using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// Control mensual de consumo y costos de OpenAI API
/// </summary>
public partial class ConsumoIaMensual
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public int Anio { get; set; }

    public int Mes { get; set; }

    public int? TotalConversaciones { get; set; }

    public int? TotalMensajes { get; set; }

    public int? TotalTokens { get; set; }

    public decimal? TotalCostoUsd { get; set; }

    public int? LimiteTokensMes { get; set; }

    public decimal? LimiteCostoMesUsd { get; set; }

    public bool? LimiteAlcanzado { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
