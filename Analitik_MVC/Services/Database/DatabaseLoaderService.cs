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
                resumen.FinancierosInsertados = await CargarFinancieros(financieros, empresaId);
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
        int insertados = 0;

        foreach (var dto in ventasDTO)
        {
            // Verificar si ya existe
            var existente = await _dbContext.Ventas
                .FirstOrDefaultAsync(v => v.NumeroOrden == dto.NumeroOrden && v.EmpresaId == empresaId);

            if (existente != null)
            {
                _logger.LogWarning("Venta {NumeroOrden} ya existe, saltando...", dto.NumeroOrden);
                continue;
            }

            var nuevaVenta = new Venta
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                NumeroOrden = dto.NumeroOrden,
                NumeroFactura = dto.NumeroFactura,
                FechaVenta = dto.FechaVenta,
                ClienteNombre = dto.ClienteNombre,
                ClienteDocumento = dto.ClienteDocumento,
                ClienteTelefono = dto.ClienteTelefono,
                ClienteEmail = dto.ClienteEmail,
                ClienteDireccion = dto.ClienteDireccion,
                MontoSubtotal = dto.MontoSubtotal,
                MontoDescuento = dto.MontoDescuento ?? 0,
                MontoImpuestos = dto.MontoImpuestos ?? 0,
                MontoTotal = dto.MontoTotal,
                MetodoPago = Enum.Parse<MetodoPago>(dto.MetodoPago, true),
                EstadoPago = dto.EstadoPago ?? "pendiente",
                Vendedor = dto.Vendedor,
                CanalVenta = dto.CanalVenta,
                Estado = Enum.Parse<EstadoVenta>(dto.Estado ?? "completado", true),
                Notas = dto.Notas,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Ventas.AddAsync(nuevaVenta);
            insertados++;
        }

        return insertados;
    }

    /// <summary>
    /// Carga datos financieros
    /// </summary>
    private async Task<int> CargarFinancieros(List<FinancieroDTO> financierosDTO, Guid empresaId)
    {
        int insertados = 0;

        foreach (var dto in financierosDTO)
        {
            var nuevoFinanciero = new DatosFinanciero
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
                NumeroComprobante = dto.NumeroComprobante,
                Beneficiario = dto.Beneficiario,
                Periodo = Periodo.Mensual, // Por defecto
                Anio = dto.FechaRegistro.Year,
                Mes = dto.FechaRegistro.Month,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.DatosFinancieros.AddAsync(nuevoFinanciero);
            insertados++;
        }

        return insertados;
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
