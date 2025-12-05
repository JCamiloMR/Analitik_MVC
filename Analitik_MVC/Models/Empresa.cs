using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// PyMEs registradas - Entidad central del sistema
/// </summary>
public partial class Empresa
{
    public Guid Id { get; set; }

    public string NombreComercial { get; set; } = null!;

    public string? RazonSocial { get; set; }

    /// <summary>
    /// Número de Identificación Tributaria (único en Colombia)
    /// </summary>
    public string? Nit { get; set; }

    public string DirectorNombreCompleto { get; set; } = null!;

    public string? DirectorCargo { get; set; }

    public string? DirectorTelefono { get; set; }

    public string? DirectorEmailSecundario { get; set; }

    public string? DirectorDocumento { get; set; }

    public string? LogoUrl { get; set; }

    public string? DescripcionEmpresa { get; set; }

    public string? DireccionFiscal { get; set; }

    public string? Ciudad { get; set; }

    public string? Departamento { get; set; }

    public string? Pais { get; set; }

    public string? CodigoPostal { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activa { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ConfiguracionEmpresa? ConfiguracionEmpresa { get; set; }

    public virtual CuentaEmpresa? CuentaEmpresa { get; set; }

    public virtual ICollection<Suscripcione> Suscripciones { get; set; } = new List<Suscripcione>();
}
