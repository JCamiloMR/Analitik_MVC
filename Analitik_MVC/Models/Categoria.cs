using System;
using System.Collections.Generic;

namespace Analitik_MVC.Models;

/// <summary>
/// Categorías de productos
/// </summary>
public partial class Categoria
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string CodigoCategoria { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public Guid? CategoriaPadreId { get; set; }

    public string? Icono { get; set; }

    public string? Color { get; set; }

    public int? Orden { get; set; }

    public bool Activa { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Categoria? CategoriaPadre { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<Categoria> InverseCategoriaPadre { get; set; } = new List<Categoria>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
