using Analitik_MVC.DTOs.Import;
using ClosedXML.Excel;

namespace Analitik_MVC.Services.Excel;

/// <summary>
/// Servicio para validación de archivos Excel
/// Valida estructura, formato y presencia de hojas/columnas
/// </summary>
public class ExcelValidationService
{
    private readonly ILogger<ExcelValidationService> _logger;

    // Hojas requeridas
    private readonly string[] _hojasRequeridas = { "PRODUCTOS", "INVENTARIO", "VENTAS", "FINANCIEROS" };

    // Columnas obligatorias por hoja
    private readonly Dictionary<string, string[]> _columnasObligatorias = new()
    {
        ["PRODUCTOS"] = new[] { "codigo_producto", "nombre", "precio_venta", "unidad_medida", "requiere_inventario", "activo" },
        ["INVENTARIO"] = new[] { "codigo_producto", "cantidad_disponible" },
        ["VENTAS"] = new[] { "numero_orden", "fecha_venta", "cliente_nombre", "monto_total", "metodo_pago" },
        ["FINANCIEROS"] = new[] { "tipo_dato", "categoria", "concepto", "monto", "fecha_registro" }
    };

    public ExcelValidationService(ILogger<ExcelValidationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Valida que el archivo sea Excel válido
    /// </summary>
    public ValidationResult ValidarArchivoExcel(Stream archivoStream, string nombreArchivo, long tamanoArchivo)
    {
        var errores = new List<ErrorValidacion>();

        // VALIDACIÓN 1: Extensión
        if (!nombreArchivo.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "Archivo",
                Error = "El archivo no es Excel (.xlsx)",
                ValorEncontrado = Path.GetExtension(nombreArchivo),
                Sugerencia = "Descarga la plantilla oficial en formato .xlsx"
            });
            return ValidationResult.Failure(errores);
        }

        // VALIDACIÓN 2: Tamaño (10 MB máximo)
        const long maxTamano = 10 * 1024 * 1024; // 10 MB
        if (tamanoArchivo > maxTamano)
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "Archivo",
                Error = $"El archivo supera 10 MB. Máximo permitido: 10 MB. Tu archivo: {tamanoArchivo / (1024.0 * 1024.0):F2} MB",
                Sugerencia = "Reduce el tamaño del archivo o divide los datos en múltiples cargas"
            });
            return ValidationResult.Failure(errores);
        }

        // VALIDACIÓN 3: Archivo Excel válido (puede abrirse)
        try
        {
            archivoStream.Position = 0;
            using var workbook = new XLWorkbook(archivoStream);
            
            // Verificar que tiene al menos una hoja
            if (workbook.Worksheets.Count == 0)
            {
                errores.Add(new ErrorValidacion
                {
                    Fila = 0,
                    Columna = "Archivo",
                    Error = "El archivo Excel no contiene hojas",
                    Sugerencia = "Usa la plantilla oficial de ClariData"
                });
                return ValidationResult.Failure(errores);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir archivo Excel");
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "Archivo",
                Error = "Archivo corrupto o no es Excel válido",
                ValorEncontrado = ex.Message,
                Sugerencia = "Intenta descargarlo nuevamente o crea uno desde la plantilla oficial"
            });
            return ValidationResult.Failure(errores);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valida la estructura de hojas del Excel
    /// </summary>
    public ValidationResult ValidarEstructuraHojas(XLWorkbook workbook)
    {
        var errores = new List<ErrorValidacion>();
        var advertencias = new List<string>();

        // Obtener nombres de hojas actuales (normalizado a mayúsculas)
        var hojasActuales = workbook.Worksheets
            .Select(w => w.Name.Trim().ToUpper())
            .ToList();

        // VALIDACIÓN 1: Presencia de hojas requeridas
        var hojasFaltantes = _hojasRequeridas
            .Where(h => !hojasActuales.Contains(h.ToUpper()))
            .ToList();

        if (hojasFaltantes.Any())
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "Hojas",
                Error = $"Faltan las hojas: {string.Join(", ", hojasFaltantes)}",
                ValorEncontrado = string.Join(", ", hojasActuales),
                Sugerencia = $"Hojas requeridas: {string.Join(", ", _hojasRequeridas)}"
            });
            return ValidationResult.Failure(errores, advertencias);
        }

        // VALIDACIÓN 2: Validar columnas de cada hoja
        foreach (var nombreHoja in _hojasRequeridas)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault(w => 
                w.Name.Trim().Equals(nombreHoja, StringComparison.OrdinalIgnoreCase));

            if (worksheet == null) continue;

            var resultadoColumnas = ValidarColumnasHoja(worksheet, nombreHoja);
            if (!resultadoColumnas.IsSuccess)
            {
                errores.AddRange(resultadoColumnas.Errors);
                advertencias.AddRange(resultadoColumnas.Warnings);
            }
        }

        if (errores.Any())
            return ValidationResult.Failure(errores, advertencias);

        return ValidationResult.Success();
    }

    /// <summary>
    /// Valida que una hoja tenga las columnas obligatorias
    /// </summary>
    public ValidationResult ValidarColumnasHoja(IXLWorksheet hoja, string nombreHoja)
    {
        var errores = new List<ErrorValidacion>();
        var advertencias = new List<string>();

        // Obtener columnas obligatorias para esta hoja
        if (!_columnasObligatorias.TryGetValue(nombreHoja.ToUpper(), out var columnasRequeridas))
        {
            advertencias.Add($"Hoja '{nombreHoja}' no reconocida en validación de columnas");
            return ValidationResult.Failure(new List<ErrorValidacion>(), advertencias);
        }

        // Obtener encabezados de la primera fila (normalizado a minúsculas)
        var primeraFila = hoja.FirstRowUsed();
        if (primeraFila == null)
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = nombreHoja,
                Error = $"Hoja '{nombreHoja}' está vacía (no tiene encabezados)",
                Sugerencia = "Agrega al menos una fila de encabezados"
            });
            return ValidationResult.Failure(errores);
        }

        var encabezados = primeraFila.CellsUsed()
            .Select(c => c.GetString().Trim().ToLower())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (!encabezados.Any())
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 1,
                Columna = nombreHoja,
                Error = $"Hoja '{nombreHoja}' no tiene encabezados válidos",
                Sugerencia = "La primera fila debe contener nombres de columnas"
            });
            return ValidationResult.Failure(errores);
        }

        // VALIDACIÓN: Columnas faltantes
        var columnasFaltantes = columnasRequeridas
            .Where(col => !encabezados.Contains(col.ToLower()))
            .ToList();

        if (columnasFaltantes.Any())
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 1,
                Columna = nombreHoja,
                Error = $"Hoja '{nombreHoja}': Faltan columnas obligatorias: {string.Join(", ", columnasFaltantes)}",
                ValorEncontrado = string.Join(", ", encabezados),
                Sugerencia = "Descarga la plantilla oficial y verifica los nombres de columnas"
            });
            return ValidationResult.Failure(errores);
        }

        // VALIDACIÓN: Hoja sin datos (solo encabezado)
        var segundaFila = hoja.Row(2);
        if (segundaFila == null || !segundaFila.CellsUsed().Any())
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 2,
                Columna = nombreHoja,
                Error = $"Hoja '{nombreHoja}' no contiene datos (solo encabezados)",
                Sugerencia = "Agrega al menos una fila de datos"
            });
            return ValidationResult.Failure(errores);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Obtiene el índice de una columna por su nombre
    /// </summary>
    public int? GetColumnIndex(IXLWorksheet hoja, string nombreColumna)
    {
        var primeraFila = hoja.FirstRowUsed();
        if (primeraFila == null) return null;

        int columnIndex = 1;
        foreach (var celda in primeraFila.CellsUsed())
        {
            if (celda.GetString().Trim().Equals(nombreColumna, StringComparison.OrdinalIgnoreCase))
                return columnIndex;
            columnIndex++;
        }

        return null;
    }
}
