namespace Analitik_MVC.DTOs.Import;

/// <summary>
/// Resultado de la validación de datos
/// </summary>
public class ValidationResult
{
    public bool IsSuccess { get; set; }
    public List<ErrorValidacion> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public static ValidationResult Success() => new ValidationResult { IsSuccess = true };
    
    public static ValidationResult Failure(List<ErrorValidacion> errors, List<string>? warnings = null)
    {
        return new ValidationResult 
        { 
            IsSuccess = false, 
            Errors = errors,
            Warnings = warnings ?? new List<string>()
        };
    }
}
