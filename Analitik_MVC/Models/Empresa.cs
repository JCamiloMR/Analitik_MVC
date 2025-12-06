using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

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

    public SectorEmpresa Sector { get; set; }

    public TamanoEmpresa Tamano { get; set; }

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

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual ICollection<Categoria> Categoria { get; set; } = new List<Categoria>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ConfiguracionEmpresa? ConfiguracionEmpresa { get; set; }

    public virtual ICollection<ConsumoIaMensual> ConsumoIaMensuals { get; set; } = new List<ConsumoIaMensual>();

    public virtual ICollection<ConversacionesIum> ConversacionesIa { get; set; } = new List<ConversacionesIum>();

    public virtual CuentaEmpresa? CuentaEmpresa { get; set; }

    public virtual ICollection<Dashboard> Dashboards { get; set; } = new List<Dashboard>();

    public virtual ICollection<DatosFinanciero> DatosFinancieros { get; set; } = new List<DatosFinanciero>();

    public virtual ICollection<FuentesDato> FuentesDatos { get; set; } = new List<FuentesDato>();

    public virtual ICollection<ImportacionesDato> ImportacionesDatos { get; set; } = new List<ImportacionesDato>();

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<RecomendacionesIum> RecomendacionesIa { get; set; } = new List<RecomendacionesIum>();

    public virtual ICollection<Suscripcione> Suscripciones { get; set; } = new List<Suscripcione>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
