using Analitik_MVC.DTOs.Import;
using Analitik_MVC.Services.Data;
using ClosedXML.Excel;

namespace Analitik_MVC.Services.Excel;

/// <summary>
/// Servicio para leer y mapear datos desde Excel a DTOs
/// Procesa las 4 hojas: PRODUCTOS, INVENTARIO, VENTAS, FINANCIEROS
/// </summary>
public class ExcelReaderService
{
    private readonly ILogger<ExcelReaderService> _logger;
    private readonly DataTransformationService _transformationService;
    private readonly ExcelValidationService _validationService;

    // Listas permitidas (hardcoded según especificación)
    private readonly string[] _unidadesMedidaPermitidas = { 
        "unidad", "kg", "gramo", "metro", "centímetro", "litro", "mililitro", "caja", "docena" 
    };
    
    private readonly string[] _categoriasPermitidas = { 
        "uniformes", "casual", "formal", "deportivo", "accesorios", "calzado", "otro" 
    };

    private readonly string[] _metodosPagoPermitidos = { 
        "efectivo", "tarjeta", "transferencia", "credito", "cheque" 
    };

    private readonly string[] _canalesVentaPermitidos = { 
        "presencial", "online", "telefonico", "distribuidor", "otro" 
    };

    private readonly string[] _tiposFinancierosPermitidos = { 
        "ingreso", "gasto", "costo", "inversion" 
    };

    private readonly Dictionary<string, string[]> _categoriasFinancieras = new()
    {
        ["ingreso"] = new[] { "ventas", "servicios", "retorno inversión", "intereses", "otros ingresos" },
        ["gasto"] = new[] { "salarios", "servicios", "transporte", "marketing", "comisiones", "otros gastos" },
        ["costo"] = new[] { "costo bienes vendidos", "materia prima", "mano obra directa", "otros costos" },
        ["inversion"] = new[] { "activos fijos", "mejoras", "tecnología", "otros" }
    };

    public ExcelReaderService(
        ILogger<ExcelReaderService> logger,
        DataTransformationService transformationService,
        ExcelValidationService validationService)
    {
        _logger = logger;
        _transformationService = transformationService;
        _validationService = validationService;
    }

    /// <summary>
    /// Lee y mapea hoja PRODUCTOS
    /// </summary>
    public (List<ProductoDTO> Productos, ValidationResult Validacion) LeerYMapearProductos(
        IXLWorksheet hoja, Guid empresaId)
    {
        var productos = new List<ProductoDTO>();
        var errores = new List<ErrorValidacion>();
        var advertencias = new List<string>();
        var codigosVistos = new HashSet<string>();

        // Obtener índices de columnas
        var indices = ObtenerIndicesColumnasProductos(hoja);
        if (indices == null)
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "PRODUCTOS",
                Error = "No se pudieron mapear las columnas de PRODUCTOS",
                Sugerencia = "Verifica que los nombres de columnas coincidan con la plantilla"
            });
            return (productos, ValidationResult.Failure(errores));
        }

        // Iterar filas (desde fila 2)
        var filas = hoja.RowsUsed().Skip(1); // Saltar encabezado
        int numeroFila = 2;

        foreach (var fila in filas)
        {
            try
            {
                // Leer valores
                var codigoProducto = fila.Cell(indices.CodigoProducto).GetString();
                var nombre = fila.Cell(indices.Nombre).GetString();
                var precioVentaRaw = fila.Cell(indices.PrecioVenta).Value;
                var unidadMedida = fila.Cell(indices.UnidadMedida).GetString();
                var requiereInventarioRaw = fila.Cell(indices.RequiereInventario).Value;
                var activoRaw = fila.Cell(indices.Activo).Value;

                // VALIDACIÓN: Campos obligatorios vacíos
                if (string.IsNullOrWhiteSpace(codigoProducto))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "codigo_producto",
                        Error = "Campo obligatorio vacío",
                        Sugerencia = "Ingresa un código único (ej: PROD-001)"
                    });
                    numeroFila++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "nombre",
                        Error = "Campo obligatorio vacío",
                        Sugerencia = "Ingresa el nombre del producto"
                    });
                    numeroFila++;
                    continue;
                }

                // Normalizar código
                codigoProducto = _transformationService.NormalizeCodigo(codigoProducto);

                // VALIDACIÓN: Formato código
                if (!_transformationService.ValidateCodigo(codigoProducto))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "codigo_producto",
                        Error = "Código debe ser alfanumérico, sin espacios, no iniciar con número",
                        ValorEncontrado = codigoProducto,
                        Sugerencia = "Use formato: PROD-001, PANT001, etc."
                    });
                    numeroFila++;
                    continue;
                }

                // VALIDACIÓN: Duplicados en carga
                if (codigosVistos.Contains(codigoProducto))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "codigo_producto",
                        Error = $"Código duplicado: {codigoProducto}. Ya aparece en fila anterior",
                        ValorEncontrado = codigoProducto,
                        Sugerencia = "Cada código debe ser único"
                    });
                    numeroFila++;
                    continue;
                }
                codigosVistos.Add(codigoProducto);

                // Parsear precio_venta
                decimal precioVenta;
                try
                {
                    precioVenta = _transformationService.ParseCurrency(precioVentaRaw);
                    if (precioVenta <= 0)
                    {
                        errores.Add(new ErrorValidacion
                        {
                            Fila = numeroFila,
                            Columna = "precio_venta",
                            Error = "Debe ser mayor a 0",
                            ValorEncontrado = precioVenta.ToString(),
                            Sugerencia = "Ingresa un precio positivo (ej: 89500)"
                        });
                        numeroFila++;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "precio_venta",
                        Error = "Formato inválido",
                        ValorEncontrado = precioVentaRaw.ToString(),
                        Sugerencia = "Use formato numérico (ej: 89500 o 89,500.00)",
                        TipoDatoEsperado = "decimal"
                    });
                    numeroFila++;
                    continue;
                }

                // Parsear costo_unitario (opcional)
                decimal? costoUnitario = null;
                if (indices.CostoUnitario.HasValue)
                {
                    var costoRaw = fila.Cell(indices.CostoUnitario.Value).Value;
                    if (!string.IsNullOrWhiteSpace(costoRaw.ToString()))
                    {
                        try
                        {
                            costoUnitario = _transformationService.ParseCurrency(costoRaw);
                            if (costoUnitario > precioVenta)
                            {
                                errores.Add(new ErrorValidacion
                                {
                                    Fila = numeroFila,
                                    Columna = "costo_unitario",
                                    Error = "No puede ser mayor que precio_venta",
                                    ValorEncontrado = $"{costoUnitario} (precio: {precioVenta})",
                                    Sugerencia = "Ajusta el costo o el precio de venta"
                                });
                                numeroFila++;
                                continue;
                            }
                        }
                        catch { costoUnitario = null; }
                    }
                }

                // VALIDACIÓN: Unidad de medida
                unidadMedida = unidadMedida?.Trim().ToLower() ?? "";
                if (!_unidadesMedidaPermitidas.Contains(unidadMedida))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "unidad_medida",
                        Error = $"Debe estar en lista permitida: {string.Join(", ", _unidadesMedidaPermitidas)}",
                        ValorEncontrado = unidadMedida,
                        Sugerencia = "Usa una unidad de la lista"
                    });
                    numeroFila++;
                    continue;
                }

                // Parsear booleanos
                bool requiereInventario = _transformationService.ParseBoolean(requiereInventarioRaw);
                bool activo = _transformationService.ParseBoolean(activoRaw);

                // Parsear es_servicio (opcional)
                bool esServicio = false;
                if (indices.EsServicio.HasValue)
                {
                    var esServicioRaw = fila.Cell(indices.EsServicio.Value).Value;
                    esServicio = _transformationService.ParseBoolean(esServicioRaw);
                }

                // VALIDACIÓN: Lógica es_servicio + requiere_inventario
                if (esServicio && requiereInventario)
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Error = "Si es_servicio=VERDADERO, requiere_inventario debe ser FALSO",
                        Sugerencia = "Los servicios no manejan inventario"
                    });
                    numeroFila++;
                    continue;
                }

                // Parsear campos opcionales
                var categoria = indices.Categoria.HasValue 
                    ? fila.Cell(indices.Categoria.Value).GetString()?.Trim() 
                    : null;
                
                // Validar categoría (solo advertencia si no está en lista)
                if (!string.IsNullOrWhiteSpace(categoria))
                {
                    if (!_categoriasPermitidas.Contains(categoria.ToLower()))
                    {
                        advertencias.Add($"Fila {numeroFila}: Categoría '{categoria}' no reconocida. Será registrada sin clasificación.");
                    }
                }

                // Crear DTO
                var producto = new ProductoDTO
                {
                    CodigoProducto = codigoProducto,
                    Nombre = _transformationService.NormalizeText(nombre),
                    PrecioVenta = precioVenta,
                    UnidadMedida = unidadMedida,
                    RequiereInventario = requiereInventario,
                    Activo = activo,
                    CostoUnitario = costoUnitario,
                    Categoria = categoria,
                    Subcategoria = indices.Subcategoria.HasValue 
                        ? fila.Cell(indices.Subcategoria.Value).GetString()?.Trim() 
                        : null,
                    Marca = indices.Marca.HasValue 
                        ? fila.Cell(indices.Marca.Value).GetString()?.Trim() 
                        : null,
                    Modelo = indices.Modelo.HasValue 
                        ? fila.Cell(indices.Modelo.Value).GetString()?.Trim() 
                        : null,
                    PrecioSugerido = indices.PrecioSugerido.HasValue 
                        ? ParseDecimalSafe(fila.Cell(indices.PrecioSugerido.Value).Value) 
                        : null,
                    PesoKg = indices.PesoKg.HasValue 
                        ? ParseDecimalSafe(fila.Cell(indices.PesoKg.Value).Value) 
                        : null,
                    VolumenM3 = indices.VolumenM3.HasValue 
                        ? ParseDecimalSafe(fila.Cell(indices.VolumenM3.Value).Value) 
                        : null,
                    CodigoBarras = indices.CodigoBarras.HasValue 
                        ? fila.Cell(indices.CodigoBarras.Value).GetString()?.Trim() 
                        : null,
                    CodigoQr = indices.CodigoQr.HasValue 
                        ? fila.Cell(indices.CodigoQr.Value).GetString()?.Trim() 
                        : null,
                    EsServicio = esServicio,
                    Descripcion = indices.Descripcion.HasValue 
                        ? fila.Cell(indices.Descripcion.Value).GetString()?.Trim() 
                        : null,
                    EmpresaId = empresaId,
                    FilaOrigen = numeroFila
                };

                productos.Add(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando fila {Fila} de PRODUCTOS", numeroFila);
                errores.Add(new ErrorValidacion
                {
                    Fila = numeroFila,
                    Error = $"Error inesperado: {ex.Message}",
                    Sugerencia = "Verifica el formato de los datos en esta fila"
                });
            }

            numeroFila++;
        }

        if (errores.Any())
            return (productos, ValidationResult.Failure(errores, advertencias));

        return (productos, new ValidationResult { IsSuccess = true, Warnings = advertencias });
    }

    // Helper para parsear decimales de forma segura
    private decimal? ParseDecimalSafe(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return null;

        try
        {
            return _transformationService.ParseDecimal(value);
        }
        catch
        {
            return null;
        }
    }

    // Clase helper para índices de columnas de PRODUCTOS
    private class IndicesProductos
    {
        public int CodigoProducto { get; set; }
        public int Nombre { get; set; }
        public int PrecioVenta { get; set; }
        public int UnidadMedida { get; set; }
        public int RequiereInventario { get; set; }
        public int Activo { get; set; }
        public int? Categoria { get; set; }
        public int? Subcategoria { get; set; }
        public int? Marca { get; set; }
        public int? Modelo { get; set; }
        public int? CostoUnitario { get; set; }
        public int? PrecioSugerido { get; set; }
        public int? PesoKg { get; set; }
        public int? VolumenM3 { get; set; }
        public int? CodigoBarras { get; set; }
        public int? CodigoQr { get; set; }
        public int? EsServicio { get; set; }
        public int? Descripcion { get; set; }
    }

    private IndicesProductos? ObtenerIndicesColumnasProductos(IXLWorksheet hoja)
    {
        var indices = new IndicesProductos
        {
            CodigoProducto = _validationService.GetColumnIndex(hoja, "codigo_producto") ?? -1,
            Nombre = _validationService.GetColumnIndex(hoja, "nombre") ?? -1,
            PrecioVenta = _validationService.GetColumnIndex(hoja, "precio_venta") ?? -1,
            UnidadMedida = _validationService.GetColumnIndex(hoja, "unidad_medida") ?? -1,
            RequiereInventario = _validationService.GetColumnIndex(hoja, "requiere_inventario") ?? -1,
            Activo = _validationService.GetColumnIndex(hoja, "activo") ?? -1,
            Categoria = _validationService.GetColumnIndex(hoja, "categoria"),
            Subcategoria = _validationService.GetColumnIndex(hoja, "subcategoria"),
            Marca = _validationService.GetColumnIndex(hoja, "marca"),
            Modelo = _validationService.GetColumnIndex(hoja, "modelo"),
            CostoUnitario = _validationService.GetColumnIndex(hoja, "costo_unitario"),
            PrecioSugerido = _validationService.GetColumnIndex(hoja, "precio_sugerido"),
            PesoKg = _validationService.GetColumnIndex(hoja, "peso_kg"),
            VolumenM3 = _validationService.GetColumnIndex(hoja, "volumen_m3"),
            CodigoBarras = _validationService.GetColumnIndex(hoja, "codigo_barras"),
            CodigoQr = _validationService.GetColumnIndex(hoja, "codigo_qr"),
            EsServicio = _validationService.GetColumnIndex(hoja, "es_servicio"),
            Descripcion = _validationService.GetColumnIndex(hoja, "descripcion")
        };

        // Verificar que campos obligatorios existan
        if (indices.CodigoProducto == -1 || indices.Nombre == -1 || indices.PrecioVenta == -1 ||
            indices.UnidadMedida == -1 || indices.RequiereInventario == -1 || indices.Activo == -1)
        {
            return null;
        }

        return indices;
    }

    /// <summary>
    /// Lee y mapea hoja INVENTARIO
    /// </summary>
    public (List<InventarioDTO> Inventarios, ValidationResult Validacion) LeerYMapearInventario(
        IXLWorksheet hoja, List<ProductoDTO> productosValidos, Guid empresaId)
    {
        var inventarios = new List<InventarioDTO>();
        var errores = new List<ErrorValidacion>();
        var advertencias = new List<string>();

        // Crear índice de códigos válidos
        var codigosValidos = productosValidos.Select(p => p.CodigoProducto).ToHashSet();

        // Obtener índices de columnas
        var idxCodigo = _validationService.GetColumnIndex(hoja, "codigo_producto") ?? -1;
        var idxCantidad = _validationService.GetColumnIndex(hoja, "cantidad_disponible") ?? -1;

        if (idxCodigo == -1 || idxCantidad == -1)
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "INVENTARIO",
                Error = "No se pudieron mapear columnas obligatorias",
                Sugerencia = "Verifica codigo_producto y cantidad_disponible"
            });
            return (inventarios, ValidationResult.Failure(errores));
        }

        // Campos opcionales
        var idxReservada = _validationService.GetColumnIndex(hoja, "cantidad_reservada");
        var idxTransito = _validationService.GetColumnIndex(hoja, "cantidad_en_transito");
        var idxStockMin = _validationService.GetColumnIndex(hoja, "stock_minimo");
        var idxStockMax = _validationService.GetColumnIndex(hoja, "stock_maximo");
        var idxFechaVto = _validationService.GetColumnIndex(hoja, "fecha_vencimiento");
        var idxUbicacion = _validationService.GetColumnIndex(hoja, "ubicacion");

        var filas = hoja.RowsUsed().Skip(1);
        int numeroFila = 2;

        foreach (var fila in filas)
        {
            try
            {
                var codigo = _transformationService.NormalizeCodigo(fila.Cell(idxCodigo).GetString());
                var cantidadRaw = fila.Cell(idxCantidad).Value;

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "codigo_producto",
                        Error = "Campo obligatorio vacío"
                    });
                    numeroFila++;
                    continue;
                }

                // Validar que código existe en PRODUCTOS
                if (!codigosValidos.Contains(codigo))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "codigo_producto",
                        Error = $"Código '{codigo}' no existe en hoja PRODUCTOS",
                        ValorEncontrado = codigo,
                        Sugerencia = "Verifica que el producto esté en hoja PRODUCTOS"
                    });
                    numeroFila++;
                    continue;
                }

                int cantidad = _transformationService.ParseInt(cantidadRaw);
                if (cantidad < 0)
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "cantidad_disponible",
                        Error = "No puede ser negativo",
                        ValorEncontrado = cantidad.ToString()
                    });
                    numeroFila++;
                    continue;
                }

                var inventario = new InventarioDTO
                {
                    CodigoProducto = codigo,
                    CantidadDisponible = cantidad,
                    CantidadReservada = idxReservada.HasValue ? ParseIntSafe(fila.Cell(idxReservada.Value).Value) : null,
                    CantidadEnTransito = idxTransito.HasValue ? ParseIntSafe(fila.Cell(idxTransito.Value).Value) : null,
                    StockMinimo = idxStockMin.HasValue ? ParseIntSafe(fila.Cell(idxStockMin.Value).Value) : null,
                    StockMaximo = idxStockMax.HasValue ? ParseIntSafe(fila.Cell(idxStockMax.Value).Value) : null,
                    FechaVencimiento = idxFechaVto.HasValue ? ParseDateSafe(fila.Cell(idxFechaVto.Value).Value) : null,
                    Ubicacion = idxUbicacion.HasValue ? fila.Cell(idxUbicacion.Value).GetString()?.Trim() : null,
                    EmpresaId = empresaId,
                    FilaOrigen = numeroFila
                };

                inventarios.Add(inventario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando fila {Fila} de INVENTARIO", numeroFila);
                errores.Add(new ErrorValidacion
                {
                    Fila = numeroFila,
                    Error = $"Error inesperado: {ex.Message}"
                });
            }

            numeroFila++;
        }

        if (errores.Any())
            return (inventarios, ValidationResult.Failure(errores, advertencias));

        return (inventarios, new ValidationResult { IsSuccess = true, Warnings = advertencias });
    }

    private int? ParseIntSafe(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return null;
        try
        {
            return _transformationService.ParseInt(value);
        }
        catch
        {
            return null;
        }
    }

    private DateTime? ParseDateSafe(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return null;
        try
        {
            return _transformationService.ParseExcelDate(value);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Lee y mapea hoja VENTAS
    /// </summary>
    public (List<VentaDTO> Ventas, ValidationResult Validacion) LeerYMapearVentas(
        IXLWorksheet hoja, List<ProductoDTO> productosValidos, Guid empresaId)
    {
        var ventas = new List<VentaDTO>();
        var errores = new List<ErrorValidacion>();
        var advertencias = new List<string>();
        var ordenesVistas = new HashSet<string>();

        // Validar índices obligatorios
        var idxOrden = _validationService.GetColumnIndex(hoja, "numero_orden") ?? -1;
        var idxFecha = _validationService.GetColumnIndex(hoja, "fecha_venta") ?? -1;
        var idxCliente = _validationService.GetColumnIndex(hoja, "cliente_nombre") ?? -1;
        var idxTotal = _validationService.GetColumnIndex(hoja, "monto_total") ?? -1;
        var idxMetodo = _validationService.GetColumnIndex(hoja, "metodo_pago") ?? -1;

        if (idxOrden == -1 || idxFecha == -1 || idxCliente == -1 || idxTotal == -1 || idxMetodo == -1)
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "VENTAS",
                Error = "Faltan columnas obligatorias",
                Sugerencia = "Verifica numero_orden, fecha_venta, cliente_nombre, monto_total, metodo_pago"
            });
            return (ventas, ValidationResult.Failure(errores));
        }

        var filas = hoja.RowsUsed().Skip(1);
        int numeroFila = 2;

        foreach (var fila in filas)
        {
            try
            {
                var numeroOrden = _transformationService.NormalizeCodigo(fila.Cell(idxOrden).GetString());
                var fechaRaw = fila.Cell(idxFecha).Value;
                var cliente = fila.Cell(idxCliente).GetString();
                var totalRaw = fila.Cell(idxTotal).Value;
                var metodoPago = fila.Cell(idxMetodo).GetString()?.Trim().ToLower();

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(numeroOrden))
                {
                    errores.Add(new ErrorValidacion { Fila = numeroFila, Columna = "numero_orden", Error = "Campo obligatorio vacío" });
                    numeroFila++;
                    continue;
                }

                if (ordenesVistas.Contains(numeroOrden))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "numero_orden",
                        Error = $"Orden duplicada: {numeroOrden}",
                        ValorEncontrado = numeroOrden
                    });
                    numeroFila++;
                    continue;
                }
                ordenesVistas.Add(numeroOrden);

                var fechaVenta = _transformationService.ParseExcelDate(fechaRaw);
                if (!fechaVenta.HasValue)
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "fecha_venta",
                        Error = "Fecha inválida",
                        ValorEncontrado = fechaRaw.ToString()
                    });
                    numeroFila++;
                    continue;
                }

                if (fechaVenta.Value > DateTime.Now)
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "fecha_venta",
                        Error = "No se permiten fechas futuras",
                        ValorEncontrado = fechaVenta.Value.ToString("yyyy-MM-dd")
                    });
                    numeroFila++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(metodoPago) || !_metodosPagoPermitidos.Contains(metodoPago))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "metodo_pago",
                        Error = $"Debe ser: {string.Join(", ", _metodosPagoPermitidos)}",
                        ValorEncontrado = metodoPago
                    });
                    numeroFila++;
                    continue;
                }

                var montoTotal = _transformationService.ParseCurrency(totalRaw);

                var venta = new VentaDTO
                {
                    NumeroOrden = numeroOrden,
                    FechaVenta = fechaVenta.Value,
                    ClienteNombre = _transformationService.NormalizeText(cliente),
                    MontoTotal = montoTotal,
                    MetodoPago = metodoPago,
                    EmpresaId = empresaId,
                    FilaOrigen = numeroFila
                };

                ventas.Add(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando fila {Fila} de VENTAS", numeroFila);
                errores.Add(new ErrorValidacion { Fila = numeroFila, Error = $"Error: {ex.Message}" });
            }

            numeroFila++;
        }

        if (errores.Any())
            return (ventas, ValidationResult.Failure(errores, advertencias));

        return (ventas, new ValidationResult { IsSuccess = true, Warnings = advertencias });
    }

    /// <summary>
    /// Lee y mapea hoja FINANCIEROS
    /// </summary>
    public (List<FinancieroDTO> Financieros, ValidationResult Validacion) LeerYMapearFinancieros(
        IXLWorksheet hoja, Guid empresaId)
    {
        var financieros = new List<FinancieroDTO>();
        var errores = new List<ErrorValidacion>();
        var advertencias = new List<string>();

        var idxTipo = _validationService.GetColumnIndex(hoja, "tipo_dato") ?? -1;
        var idxCategoria = _validationService.GetColumnIndex(hoja, "categoria") ?? -1;
        var idxConcepto = _validationService.GetColumnIndex(hoja, "concepto") ?? -1;
        var idxMonto = _validationService.GetColumnIndex(hoja, "monto") ?? -1;
        var idxFecha = _validationService.GetColumnIndex(hoja, "fecha_registro") ?? -1;

        if (idxTipo == -1 || idxCategoria == -1 || idxConcepto == -1 || idxMonto == -1 || idxFecha == -1)
        {
            errores.Add(new ErrorValidacion
            {
                Fila = 0,
                Columna = "FINANCIEROS",
                Error = "Faltan columnas obligatorias"
            });
            return (financieros, ValidationResult.Failure(errores));
        }

        var filas = hoja.RowsUsed().Skip(1);
        int numeroFila = 2;

        foreach (var fila in filas)
        {
            try
            {
                var tipo = fila.Cell(idxTipo).GetString()?.Trim().ToLower();
                var categoria = fila.Cell(idxCategoria).GetString()?.Trim().ToLower();
                var concepto = fila.Cell(idxConcepto).GetString();
                var montoRaw = fila.Cell(idxMonto).Value;
                var fechaRaw = fila.Cell(idxFecha).Value;

                if (string.IsNullOrWhiteSpace(tipo) || !_tiposFinancierosPermitidos.Contains(tipo))
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "tipo_dato",
                        Error = $"Debe ser: {string.Join(", ", _tiposFinancierosPermitidos)}",
                        ValorEncontrado = tipo
                    });
                    numeroFila++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(categoria))
                {
                    errores.Add(new ErrorValidacion { Fila = numeroFila, Columna = "categoria", Error = "Campo obligatorio vacío" });
                    numeroFila++;
                    continue;
                }

                var monto = _transformationService.ParseCurrency(montoRaw);
                if (monto <= 0)
                {
                    errores.Add(new ErrorValidacion
                    {
                        Fila = numeroFila,
                        Columna = "monto",
                        Error = "Debe ser mayor a 0",
                        ValorEncontrado = monto.ToString()
                    });
                    numeroFila++;
                    continue;
                }

                var fecha = _transformationService.ParseExcelDate(fechaRaw);
                if (!fecha.HasValue)
                {
                    errores.Add(new ErrorValidacion { Fila = numeroFila, Columna = "fecha_registro", Error = "Fecha inválida" });
                    numeroFila++;
                    continue;
                }

                var financiero = new FinancieroDTO
                {
                    TipoDato = tipo,
                    Categoria = categoria,
                    Concepto = concepto,
                    Monto = monto,
                    FechaRegistro = fecha.Value,
                    EmpresaId = empresaId,
                    FilaOrigen = numeroFila
                };

                financieros.Add(financiero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando fila {Fila} de FINANCIEROS", numeroFila);
                errores.Add(new ErrorValidacion { Fila = numeroFila, Error = $"Error: {ex.Message}" });
            }

            numeroFila++;
        }

        if (errores.Any())
            return (financieros, ValidationResult.Failure(errores, advertencias));

        return (financieros, new ValidationResult { IsSuccess = true, Warnings = advertencias });
    }
}
