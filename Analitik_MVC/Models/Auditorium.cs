using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;

namespace Analitik_MVC.Models;

/// <summary>
/// Auditoría completa del sistema
/// </summary>
public partial class Auditorium
{
    public Guid Id { get; set; }

    public Guid? CuentaEmpresaId { get; set; }

    public Guid EmpresaId { get; set; }

    public AccionAuditoria Accion { get; set; }

    public string TablaAfectada { get; set; } = null!;

    public Guid RegistroId { get; set; }

    public string? DatosAnteriores { get; set; }

    public string? DatosNuevos { get; set; }

    public string? CambiosDetectados { get; set; }

    public DateTime Timestamp { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Descripcion { get; set; }

    public virtual CuentaEmpresa? CuentaEmpresa { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
