using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

public partial class Cliente
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string CodigoCliente { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? DocumentoIdentificacion { get; set; }

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string? TelefonoAlternativo { get; set; }

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    public string? Departamento { get; set; }

    public string? CodigoPostal { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaUltimaCompra { get; set; }

    public decimal? TotalCompras { get; set; }

    public int? NumeroCompras { get; set; }

    public bool Activo { get; set; }

    public string? Notas { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
