using System.Data;
using Analitik_MVC.Data;
using Analitik_MVC.DTOs.Import;
using Analitik_MVC.Models;
using Analitik_MVC.Enums;
using Microsoft.EntityFrameworkCore;

namespace Analitik_MVC.Services.Database;

/// <summary>
/// Servicio para carga atómica de datos en base de datos
/// Implementa transacción SERIALIZABLE (todo o nada)
/// </summary>
public class DatabaseLoaderService
{
    private readonly AnalitikDbContext _dbContext;
    private readonly ILogger<DatabaseLoaderService> _logger;

    public DatabaseLoaderService(
        AnalitikDbContext dbContext,
        ILogger<DatabaseLoaderService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Carga datos atómicamente en BD con transacción SERIALIZABLE
    /// </summary>
    public async Task<ResumenImportacion> CargarDatosAtomicamente(
        List<ProductoDTO> productos,
        List<InventarioDTO> inventarios,
        List<VentaDTO> ventas,
        List<FinancieroDTO> financieros,
        Guid empresaId)
    {
        var inicio = DateTime.UtcNow;
        var resumen = new ResumenImportacion();

        // Iniciar transacción SERIALIZABLE
        using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            // FASE 1: Cargar PRODUCTOS (con upsert)
            _logger.LogInformation("Cargando {Count} productos...", productos.Count);
            var (insertados, actualizados) = await CargarProductos(productos, empresaId);
            resumen.ProductosInsertados = insertados;
            resumen.ProductosActualizados = actualizados;

            // FASE 2: Cargar INVENTARIO
            if (inventarios.Any())
            {
                _logger.LogInformation("Cargando {Count} registros de inventario...", inventarios.Count);
                resumen.InventariosInsertados = await CargarInventario(inventarios, empresaId);
            }

            // FASE 3: Cargar VENTAS
            if (ventas.Any())
            {
                _logger.LogInformation("Cargando {Count} ventas...", ventas.Count);
                resumen.VentasInsertadas = await CargarVentas(ventas, empresaId);
            }

            // FASE 4: Cargar FINANCIEROS
            if (financieros.Any())
            {
                _logger.LogInformation("Cargando {Count} datos financieros...", financieros.Count);
                var(insertadosF, actualizadosF) = await CargarFinancieros(financieros, empresaId);
                resumen.FinancierosInsertados = insertadosF;
            }

            // COMMIT si todo OK
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var fin = DateTime.UtcNow;
            resumen.DuracionSegundos = (decimal)(fin - inicio).TotalSeconds;
            resumen.RegistrosProcesados = 
                resumen.ProductosInsertados + 
                resumen.ProductosActualizados + 
                resumen.InventariosInsertados + 
                resumen.VentasInsertadas + 
                resumen.FinancierosInsertados;

            _logger.LogInformation("Carga completada exitosamente en {Duracion}s", resumen.DuracionSegundos);

            return resumen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en carga atómica, ejecutando ROLLBACK");
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Carga productos con lógica Upsert (insert o update)
    /// </summary>
    private async Task<(int insertados, int actualizados)> CargarProductos(
        List<ProductoDTO> productosDTO, 
        Guid empresaId)
    {
        int insertados = 0;
        int actualizados = 0;

        // Obtener productos existentes en BD
        var codigosEnCarga = productosDTO.Select(p => p.CodigoProducto).ToList();
        var productosExistentes = await _dbContext.Productos
            .Where(p => p.EmpresaId == empresaId && codigosEnCarga.Contains(p.CodigoProducto))
            .ToDictionaryAsync(p => p.CodigoProducto);

        foreach (var dto in productosDTO)
        {
            if (productosExistentes.TryGetValue(dto.CodigoProducto, out var existente))
            {
                // ACTUALIZAR producto existente
                existente.Nombre = dto.Nombre;
                existente.Descripcion = dto.Descripcion;
                existente.Subcategoria = dto.Subcategoria;
                existente.Marca = dto.Marca;
                existente.Modelo = dto.Modelo;
                existente.PrecioVenta = dto.PrecioVenta;
                existente.CostoUnitario = dto.CostoUnitario;
                existente.PrecioSugerido = dto.PrecioSugerido;
                existente.MargenPorcentaje = CalcularMargen(dto.PrecioVenta, dto.CostoUnitario);
                existente.UnidadMedida = dto.UnidadMedida;
                existente.PesoKg = dto.PesoKg;
                existente.VolumenM3 = dto.VolumenM3;
                existente.CodigoBarras = dto.CodigoBarras;
                existente.CodigoQr = dto.CodigoQr;
                existente.EsServicio = dto.EsServicio;
                existente.RequiereInventario = dto.RequiereInventario;
                existente.Activo = dto.Activo;
                existente.UpdatedAt = DateTime.UtcNow;

                _dbContext.Productos.Update(existente);
                actualizados++;
            }
            else
            {
                // INSERTAR nuevo producto
                var nuevoProducto = new Producto
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    CodigoProducto = dto.CodigoProducto,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Subcategoria = dto.Subcategoria,
                    Marca = dto.Marca,
                    Modelo = dto.Modelo,
                    PrecioVenta = dto.PrecioVenta,
                    CostoUnitario = dto.CostoUnitario,
                    PrecioSugerido = dto.PrecioSugerido,
                    MargenPorcentaje = CalcularMargen(dto.PrecioVenta, dto.CostoUnitario),
                    UnidadMedida = dto.UnidadMedida,
                    PesoKg = dto.PesoKg,
                    VolumenM3 = dto.VolumenM3,
                    CodigoBarras = dto.CodigoBarras,
                    CodigoQr = dto.CodigoQr,
                    EsServicio = dto.EsServicio,
                    RequiereInventario = dto.RequiereInventario,
                    Vendible = true,
                    Comprable = true,
                    Activo = dto.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.Productos.AddAsync(nuevoProducto);
                insertados++;
            }
        }

        return (insertados, actualizados);
    }

    /// <summary>
    /// Carga inventario (solo INSERT, no actualiza)
    /// </summary>
    private async Task<int> CargarInventario(List<InventarioDTO> inventariosDTO, Guid empresaId)
    {
        int insertados = 0;

        // Mapear códigos a IDs de productos
        var codigosEnCarga = inventariosDTO.Select(i => i.CodigoProducto).Distinct().ToList();
        var productosMap = await _dbContext.Productos
            .Where(p => p.EmpresaId == empresaId && codigosEnCarga.Contains(p.CodigoProducto))
            .ToDictionaryAsync(p => p.CodigoProducto, p => p.Id);

        foreach (var dto in inventariosDTO)
        {
            if (!productosMap.TryGetValue(dto.CodigoProducto, out var productoId))
            {
                _logger.LogWarning("Producto {Codigo} no encontrado para inventario", dto.CodigoProducto);
                continue;
            }

            // Verificar si ya existe inventario para este producto
            var existente = await _dbContext.Inventarios
                .FirstOrDefaultAsync(i => i.ProductoId == productoId);

            if (existente != null)
            {
                // Actualizar inventario existente
                existente.CantidadDisponible = dto.CantidadDisponible;
                existente.CantidadReservada = dto.CantidadReservada ?? 0;
                existente.CantidadEnTransito = dto.CantidadEnTransito ?? 0;
                existente.StockMinimo = dto.StockMinimo ?? 0;
                existente.StockMaximo = dto.StockMaximo;
                existente.PuntoReorden = dto.PuntoReorden;
                existente.Ubicacion = dto.Ubicacion;
                existente.Pasillo = dto.Pasillo;
                existente.Estante = dto.Estante;
                existente.Nivel = dto.Nivel;
                existente.LoteActual = dto.LoteActual;
                existente.FechaVencimiento = dto.FechaVencimiento.HasValue 
                    ? DateOnly.FromDateTime(dto.FechaVencimiento.Value) 
                    : null;
                existente.DiasAlertaVencimiento = dto.DiasAlertaVencimiento ?? 30;
                existente.UpdatedAt = DateTime.UtcNow;

                _dbContext.Inventarios.Update(existente);
            }
            else
            {
                var nuevoInventario = new Inventario
                {
                    Id = Guid.NewGuid(),
                    ProductoId = productoId,
                    CantidadDisponible = dto.CantidadDisponible,
                    CantidadReservada = dto.CantidadReservada ?? 0,
                    CantidadEnTransito = dto.CantidadEnTransito ?? 0,
                    StockMinimo = dto.StockMinimo ?? 0,
                    StockMaximo = dto.StockMaximo,
                    PuntoReorden = dto.PuntoReorden,
                    Ubicacion = dto.Ubicacion,
                    Pasillo = dto.Pasillo,
                    Estante = dto.Estante,
                    Nivel = dto.Nivel,
                    LoteActual = dto.LoteActual,
                    FechaVencimiento = dto.FechaVencimiento.HasValue 
                        ? DateOnly.FromDateTime(dto.FechaVencimiento.Value) 
                        : null,
                    DiasAlertaVencimiento = dto.DiasAlertaVencimiento ?? 30,
                    UltimaActualizacion = DateTime.UtcNow,
                    EstadoStock = EstadoStock.Normal, // Por defecto
                    AlertasActivadas = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.Inventarios.AddAsync(nuevoInventario);
            }

            insertados++;
        }

        return insertados;
    }

    /// <summary>
    /// Carga ventas y sus detalles
    /// </summary>
    private async Task<int> CargarVentas(List<VentaDTO> ventasDTO, Guid empresaId)
    {
        int procesados = 0;

        // 1. Cargar ventas existentes de la empresa y crear diccionario por NumeroOrden
        var ventasExistentes = await _dbContext.Ventas
            .Where(v => v.EmpresaId == empresaId)
            .ToListAsync();

        var ventasExistentesMap = ventasExistentes
            .ToDictionary(v => v.NumeroOrden.Trim().ToUpper(), v => v);

        // 2. Trackear NumeroOrden que vienen en el Excel
        var ordenesEnCarga = new HashSet<string>(
            ventasDTO
                .Select(v => (v.NumeroOrden ?? string.Empty).Trim().ToUpper())
                .Where(o => !string.IsNullOrWhiteSpace(o))
        );

        // 3. Upsert (update si existe, insert si no existe)
        foreach (var dto in ventasDTO)
        {
            var numeroOrden = (dto.NumeroOrden ?? string.Empty).Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(numeroOrden))
                continue;

            var subtotal = dto.MontoSubtotal;
            var descuento = dto.MontoDescuento ?? 0;
            var impuestos = dto.MontoImpuestos ?? 0;

            if (subtotal < 0 || descuento < 0 || impuestos < 0)
            {
                _logger.LogWarning(
                    "Montos inválidos en orden {NumeroOrden}. Subtotal: {Subtotal}, Desc: {Desc}, Imp: {Imp}",
                    dto.NumeroOrden,
                    subtotal,
                    descuento,
                    impuestos);

                continue;
            }

            var totalCalculado = subtotal - descuento + impuestos;

            var fechaVentaUtc =
                dto.FechaVenta.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dto.FechaVenta, DateTimeKind.Utc)
                    : dto.FechaVenta.ToUniversalTime();

            if (ventasExistentesMap.TryGetValue(numeroOrden, out var existente))
            {
                // UPDATE
                existente.NumeroFactura = dto.NumeroFactura;
                existente.FechaVenta = fechaVentaUtc;
                existente.ClienteNombre = dto.ClienteNombre;
                existente.ClienteDocumento = dto.ClienteDocumento;
                existente.ClienteTelefono = dto.ClienteTelefono;
                existente.ClienteEmail = dto.ClienteEmail;
                existente.ClienteDireccion = dto.ClienteDireccion;
                existente.MontoSubtotal = subtotal;
                existente.MontoDescuento = descuento;
                existente.MontoImpuestos = impuestos;
                existente.MontoTotal = totalCalculado;
                existente.MetodoPago = Enum.Parse<MetodoPago>(dto.MetodoPago, true);
                existente.EstadoPago = dto.EstadoPago ?? "pendiente";
                existente.Estado = Enum.Parse<EstadoVenta>(dto.Estado ?? "completado", true);
                existente.Vendedor = dto.Vendedor;
                existente.CanalVenta = dto.CanalVenta;
                existente.Notas = dto.Notas;
                existente.UpdatedAt = DateTime.UtcNow;

                _dbContext.Ventas.Update(existente);
            }
            else
            {
                // INSERT
                var nuevaVenta = new Venta
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    NumeroOrden = numeroOrden,
                    NumeroFactura = dto.NumeroFactura,
                    FechaVenta = fechaVentaUtc,
                    ClienteNombre = dto.ClienteNombre,
                    ClienteDocumento = dto.ClienteDocumento,
                    ClienteTelefono = dto.ClienteTelefono,
                    ClienteEmail = dto.ClienteEmail,
                    ClienteDireccion = dto.ClienteDireccion,
                    MontoSubtotal = subtotal,
                    MontoDescuento = descuento,
                    MontoImpuestos = impuestos,
                    MontoTotal = totalCalculado,
                    MetodoPago = Enum.Parse<MetodoPago>(dto.MetodoPago, true),
                    EstadoPago = dto.EstadoPago ?? "pendiente",
                    Estado = Enum.Parse<EstadoVenta>(dto.Estado ?? "completado", true),
                    Vendedor = dto.Vendedor,
                    CanalVenta = dto.CanalVenta,
                    Notas = dto.Notas,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.Ventas.AddAsync(nuevaVenta);
            }

            procesados++;
        }

        // 4. Eliminar ventas que existen en BD pero no vienen en el nuevo Excel
        var ventasAEliminar = ventasExistentes
            .Where(v => !ordenesEnCarga.Contains(v.NumeroOrden.Trim().ToUpper()))
            .ToList();

        if (ventasAEliminar.Any())
        {
            var idsVentasAEliminar = ventasAEliminar.Select(v => v.Id).ToList();

            // Eliminar primero detalles_venta para respetar FK
            var detallesAEliminar = await _dbContext.DetallesVenta
                .Where(d => idsVentasAEliminar.Contains(d.VentaId))
                .ToListAsync();

            _dbContext.DetallesVenta.RemoveRange(detallesAEliminar);
            _dbContext.Ventas.RemoveRange(ventasAEliminar);
        }

        return procesados;
    }


    private string CrearKeyFinanciero(string tipo, string categoria, string concepto, DateTime fecha)
    {
        return $"{tipo?.Trim().ToLower()}|" +
               $"{categoria?.Trim().ToLower()}|" +
               $"{concepto?.Trim().ToLower()}|" +
               $"{fecha.Date:yyyyMMdd}";
    }

    /// <summary>
    /// Carga datos financieros
    /// </summary>
    private async Task<(int insertadosF, int actualizadosF)> CargarFinancieros(
    List<FinancieroDTO> financierosDTO, Guid empresaId)
    {
        int insertadosF = 0;
        int actualizadosF = 0;

        // 🔑 Obtener comprobantes válidos del Excel
        var comprobantes = financierosDTO
            .Where(x => !string.IsNullOrWhiteSpace(x.NumeroComprobante))
            .Select(x => x.NumeroComprobante.Trim())
            .Distinct()
            .ToList();

        // 🔍 Traer existentes por comprobante
        var existentes = await _dbContext.DatosFinancieros
            .Where(f => f.EmpresaId == empresaId && comprobantes.Contains(f.NumeroComprobante))
            .ToDictionaryAsync(f => f.NumeroComprobante);

        foreach (var dto in financierosDTO)
        {
            var comprobante = dto.NumeroComprobante?.Trim();

            if (!string.IsNullOrWhiteSpace(comprobante) &&
                existentes.TryGetValue(comprobante, out var existente))
            {
                // 🔄 UPDATE
                existente.TipoDato = Enum.Parse<TipoDatoFinanciero>(dto.TipoDato, true);
                existente.Categoria = dto.Categoria;
                existente.Subcategoria = dto.Subcategoria;
                existente.Concepto = dto.Concepto;
                existente.Observaciones = dto.Observaciones;
                existente.Monto = dto.Monto;
                existente.Moneda = dto.Moneda ?? "COP";
                existente.FechaRegistro = DateOnly.FromDateTime(dto.FechaRegistro);
                existente.FechaPago = dto.FechaPago.HasValue
                    ? DateOnly.FromDateTime(dto.FechaPago.Value)
                    : null;
                existente.NumeroComprobante = comprobante;
                existente.Beneficiario = dto.Beneficiario;
                existente.Anio = dto.FechaRegistro.Year;
                existente.Mes = dto.FechaRegistro.Month;
                existente.UpdatedAt = DateTime.UtcNow;

                _dbContext.DatosFinancieros.Update(existente);
                actualizadosF++;
            }
            else
            {
                // 🆕 INSERT
                var nuevo = new DatosFinanciero
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    TipoDato = Enum.Parse<TipoDatoFinanciero>(dto.TipoDato, true),
                    Categoria = dto.Categoria,
                    Subcategoria = dto.Subcategoria,
                    Concepto = dto.Concepto,
                    Observaciones = dto.Observaciones,
                    Monto = dto.Monto,
                    Moneda = dto.Moneda ?? "COP",
                    FechaRegistro = DateOnly.FromDateTime(dto.FechaRegistro),
                    FechaPago = dto.FechaPago.HasValue
                        ? DateOnly.FromDateTime(dto.FechaPago.Value)
                        : null,
                    NumeroComprobante = comprobante,
                    Beneficiario = dto.Beneficiario,
                    Periodo = Periodo.Mensual,
                    Anio = dto.FechaRegistro.Year,
                    Mes = dto.FechaRegistro.Month,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.DatosFinancieros.AddAsync(nuevo);
                insertadosF++;
            }
        }

        return (insertadosF, actualizadosF);
    }

    /// <summary>
    /// Calcula margen de ganancia
    /// </summary>
    private decimal? CalcularMargen(decimal precioVenta, decimal? costoUnitario)
    {
        if (!costoUnitario.HasValue || costoUnitario == 0)
            return null;

        return ((precioVenta - costoUnitario.Value) / costoUnitario.Value) * 100;
    }
}
