namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para el resultado completo de una importación
/// Contiene información de éxito, errores y estadísticas
/// </summary>
public class ImportResultDTO
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = null!;
    public Guid? ImportId { get; set; }
    public List<ErrorValidacion> Errores { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
    public ResumenImportacion? Resumen { get; set; }
}

public class ResumenImportacion
{
    public int RegistrosProcesados { get; set; }
    public int TotalErrores { get; set; }
    public int TotalAdvertencias { get; set; }
    public int ProductosInsertados { get; set; }
    public int ProductosActualizados { get; set; }
    public int InventariosInsertados { get; set; }
    public int VentasInsertadas { get; set; }
    public int FinancierosInsertados { get; set; }
    public decimal DuracionSegundos { get; set; }
}
