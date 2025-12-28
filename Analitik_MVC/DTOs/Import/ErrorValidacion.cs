namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// DTO para representar errores de validación
/// Incluye información detallada para mostrar al usuario
/// </summary>
public class ErrorValidacion
{
    public int Fila { get; set; }
    public string Columna { get; set; } = null!;
    public string Error { get; set; } = null!;
    public string? ValorEncontrado { get; set; }
    public string? Sugerencia { get; set; }
    public string? TipoDatoEsperado { get; set; }
}
