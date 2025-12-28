using System.Globalization;
using System.Text.RegularExpressions;
using Analitik_MVC.DTOs.Import;

namespace Analitik_MVC.Services.Data;

/// <summary>
/// Servicio para transformación y parseo de datos desde Excel
/// Maneja conversión de fechas, monedas, booleanos y textos
/// </summary>
public class DataTransformationService
{
    /// <summary>
    /// Parsea fechas desde Excel en múltiples formatos
    /// Prioridad: ISO (YYYY-MM-DD), DD/MM/YYYY (Colombia), MM/DD/YYYY (USA)
    /// </summary>
    public DateTime? ParseExcelDate(object? valor)
    {
        if (valor == null) return null;

        string texto = valor.ToString()!.Trim();
        if (string.IsNullOrWhiteSpace(texto)) return null;

        // Intentar ISO primero (más rápido y preferido)
        if (DateTime.TryParseExact(texto, "yyyy-MM-dd", 
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var isoDate))
            return isoDate;

        // Intentar DD/MM/YYYY (Colombia)
        if (DateTime.TryParseExact(texto, "dd/MM/yyyy", 
            CultureInfo.GetCultureInfo("es-CO"), DateTimeStyles.None, out var colDate))
            return colDate;

        // Intentar MM/DD/YYYY (USA)
        if (DateTime.TryParseExact(texto, "MM/dd/yyyy", 
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var usaDate))
            return usaDate;

        // Intentar parse automático
        if (DateTime.TryParse(texto, out var fecha))
            return fecha;

        throw new ValidationException($"Formato de fecha no reconocido: {texto}. Use YYYY-MM-DD, DD/MM/YYYY o MM/DD/YYYY");
    }

    /// <summary>
    /// Parsea valores monetarios desde Excel
    /// Maneja múltiples formatos: $1,234.56 o 1.234,56 (Colombia)
    /// Siempre devuelve con 2 decimales
    /// </summary>
    public decimal ParseCurrency(object? valor)
    {
        if (valor == null) return 0m;

        string texto = valor.ToString()!
            .Trim()
            .Replace("$", "")
            .Replace("COP", "")
            .Replace("USD", "")
            .Replace("EUR", "")
            .Replace(" ", "");

        if (string.IsNullOrWhiteSpace(texto)) return 0m;

        // Detectar separadores: si hay . y ,, el último es decimal
        if (texto.Contains(".") && texto.Contains(","))
        {
            int lastDot = texto.LastIndexOf(".");
            int lastComma = texto.LastIndexOf(",");

            if (lastDot > lastComma)
            {
                // 1.000,50 ? punto es miles, coma es decimal
                texto = texto.Replace(".", "").Replace(",", ".");
            }
            else
            {
                // 1,000.50 ? coma es miles, punto es decimal
                texto = texto.Replace(",", "");
            }
        }
        else if (texto.Contains(",") && texto.Count(c => c == ',') == 1)
        {
            // Solo una coma: es decimal (formato europeo)
            texto = texto.Replace(",", ".");
        }
        else if (texto.Contains("."))
        {
            // Solo punto: puede ser miles o decimal
            // Si hay más de un punto, es miles
            if (texto.Count(c => c == '.') > 1)
            {
                texto = texto.Replace(".", "");
            }
        }

        if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return Math.Round(result, 2);
        }

        throw new ValidationException($"Monto inválido: {valor}. Use formato numérico (ej: 89500 o 89,500.00)");
    }

    /// <summary>
    /// Parsea valores booleanos desde Excel
    /// Soporta múltiples representaciones en español e inglés
    /// </summary>
    public bool ParseBoolean(object? valor)
    {
        if (valor == null) return false;

        string texto = valor.ToString()!.Trim().ToUpper();

        // Valores verdaderos
        string[] valoresVerdaderos = { 
            "VERDADERO", "TRUE", "V", "SÍ", "SI", "S", "YES", "Y", "1", "ACTIVO", "ACTIVA" 
        };
        if (valoresVerdaderos.Contains(texto))
            return true;

        // Valores falsos
        string[] valoresFalsos = { 
            "FALSO", "FALSE", "F", "NO", "N", "0", "INACTIVO", "INACTIVA" 
        };
        if (valoresFalsos.Contains(texto))
            return false;

        // Por defecto: falso
        return false;
    }

    /// <summary>
    /// Parsea enteros desde Excel
    /// </summary>
    public int ParseInt(object? valor)
    {
        if (valor == null) return 0;

        string texto = valor.ToString()!.Trim();
        if (string.IsNullOrWhiteSpace(texto)) return 0;

        // Remover separadores de miles
        texto = texto.Replace(",", "").Replace(".", "").Replace(" ", "");

        if (int.TryParse(texto, out var result))
            return result;

        throw new ValidationException($"Número entero inválido: {valor}");
    }

    /// <summary>
    /// Parsea decimales desde Excel (sin redondeo automático)
    /// </summary>
    public decimal ParseDecimal(object? valor)
    {
        if (valor == null) return 0m;

        string texto = valor.ToString()!.Trim();
        if (string.IsNullOrWhiteSpace(texto)) return 0m;

        // Usar misma lógica que ParseCurrency pero sin redondeo
        texto = texto.Replace(" ", "");

        if (texto.Contains(".") && texto.Contains(","))
        {
            int lastDot = texto.LastIndexOf(".");
            int lastComma = texto.LastIndexOf(",");

            if (lastDot > lastComma)
                texto = texto.Replace(".", "").Replace(",", ".");
            else
                texto = texto.Replace(",", "");
        }
        else if (texto.Contains(","))
        {
            texto = texto.Replace(",", ".");
        }

        if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new ValidationException($"Número decimal inválido: {valor}");
    }

    /// <summary>
    /// Valida formato de email
    /// </summary>
    public bool ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        // Patrón regex para email
        string pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    /// <summary>
    /// Normaliza texto: trim, espacios múltiples, caracteres de control
    /// </summary>
    public string NormalizeText(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        // Trim espacios
        texto = texto.Trim();

        // Remover espacios múltiples
        while (texto.Contains("  "))
            texto = texto.Replace("  ", " ");

        // Remover caracteres de control
        texto = Regex.Replace(texto, @"[\x00-\x1F]", "");

        return texto;
    }

    /// <summary>
    /// Normaliza código (para códigos de producto, orden, etc.)
    /// Convierte a mayúsculas y remueve espacios
    /// </summary>
    public string NormalizeCodigo(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return string.Empty;

        return codigo.Trim().ToUpper().Replace(" ", "");
    }

    /// <summary>
    /// Valida que un código sea alfanumérico y no inicie con número
    /// </summary>
    public bool ValidateCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return false;

        // No debe iniciar con número
        if (char.IsDigit(codigo[0])) return false;

        // Solo alfanumérico y guiones
        return Regex.IsMatch(codigo, @"^[A-Za-z][A-Za-z0-9\-_]*$");
    }
}

/// <summary>
/// Excepción personalizada para errores de validación
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
