using Analitik_MVC.Data;
using Analitik_MVC.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Analitik_MVC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AnalitikDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AnalitikDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene resumen consolidado de todos los dashboards
    /// GET /api/dashboard/summary?empresaId={guid}&tipoFilter=30d
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary(
        [FromQuery] string? timeFilter = null,
        [FromQuery] string? tipoFilter = null,
        [FromQuery] Guid? empresaId = null)
    {
        try
        {
            Guid empresaIdFinal = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value ?? empresaId?.ToString()!); //este es el ide empresa

            var filtroTiempo = string.IsNullOrWhiteSpace(timeFilter) ? tipoFilter : timeFilter;
            if (string.IsNullOrWhiteSpace(filtroTiempo))
                filtroTiempo = "30d";

            // Validar empresa
            var empresaExists = await _context.Empresas.AnyAsync(e => e.Id == empresaIdFinal && e.Activa);
            if (!empresaExists)
            {
                return NotFound(new { error = "Empresa no encontrada o inactiva" });
            }

            // Calcular rango de fechas según filtro
            var fechaHasta = DateTime.UtcNow;
            var fechaDesde = filtroTiempo switch
            {
                "7d" => fechaHasta.AddDays(-7),
                "30d" => fechaHasta.AddDays(-30),
                "90d" => fechaHasta.AddDays(-90),
                "1y" => fechaHasta.AddYears(-1),
                "2y" => fechaHasta.AddYears(-2),
                _ => fechaHasta.AddDays(-30)
            };

            var fechaDesdeDateOnly = DateOnly.FromDateTime(fechaDesde);
            var fechaHastaDateOnly = DateOnly.FromDateTime(fechaHasta);

            // ===== VENTAS =====
            var ventasQuery = _context.Ventas
                .AsNoTracking()
                .Where(v => v.EmpresaId == empresaIdFinal && v.FechaVenta >= fechaDesde && v.FechaVenta <= fechaHasta);

            var ventasData = await ventasQuery.ToListAsync();

            var totalIngresos = ventasData.Sum(v => v.MontoTotal);
            var totalOrdenes = ventasData.Count;
            var promedioOrden = totalOrdenes > 0 ? totalIngresos / totalOrdenes : 0;

            // Calcular crecimiento (comparar con período anterior)
            var periodoAnteriorDesde = fechaDesde.AddDays(-(fechaHasta - fechaDesde).TotalDays);
            var ventasAnterior = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.EmpresaId == empresaIdFinal && v.FechaVenta >= periodoAnteriorDesde && v.FechaVenta < fechaDesde)
                .SumAsync(v => v.MontoTotal);

            var crecimientoVentas = ventasAnterior > 0
                ? ((totalIngresos - ventasAnterior) / ventasAnterior) * 100
                : 0;

            var ventasPorMesData = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.EmpresaId == empresaIdFinal &&
                            v.FechaVenta >= fechaDesde &&
                            v.FechaVenta <= fechaHasta)
                .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Ventas = g.Sum(v => v.MontoTotal)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var ventasPorMes = ventasPorMesData
                .Select(x => new
                {
                    mes = $"{x.Year}-{x.Month:D2}",
                    ventas = x.Ventas
                })
                .ToList();

            // Top 5 clientes
            var topClientes = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.EmpresaId == empresaIdFinal && v.FechaVenta >= fechaDesde)
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new
                {
                    cliente = g.Key,
                    ventas = g.Sum(v => v.MontoTotal)
                })
                .OrderByDescending(x => x.ventas)
                .Take(5)
                .ToListAsync();

            var ventasPorCategoria = await _context.DetallesVenta
                .AsNoTracking()
                .Where(d => d.Venta.EmpresaId == empresaIdFinal &&
                            d.Venta.FechaVenta >= fechaDesde &&
                            d.Venta.FechaVenta <= fechaHasta)
                .GroupBy(d => !string.IsNullOrWhiteSpace(d.Producto.Subcategoria)
                    ? d.Producto.Subcategoria!
                    : (!string.IsNullOrWhiteSpace(d.Venta.Categoria) ? d.Venta.Categoria! : "Sin categoría"))
                .Select(g => new
                {
                    categoria = g.Key,
                    ventas = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.ventas)
                .Take(8)
                .ToListAsync();

            var pedidosRecientes = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.EmpresaId == empresaIdFinal)
                .OrderByDescending(v => v.FechaVenta)
                .Take(10)
                .Select(v => new
                {
                    id = v.NumeroOrden,
                    cliente = v.ClienteNombre,
                    monto = v.MontoTotal,
                    categoria = !string.IsNullOrWhiteSpace(v.Categoria) ? v.Categoria : "Sin categoría",
                    estado = v.Estado.ToString()
                })
                .ToListAsync();

            // ===== INVENTARIO =====
            var inventarios = await _context.Inventarios
                .AsNoTracking()
                .Include(i => i.Producto)
                .Where(i => i.Producto.EmpresaId == empresaIdFinal)
                .ToListAsync();

            var stockTotal = inventarios.Sum(i => i.CantidadDisponible * i.Producto.PrecioVenta);
            var productosActivos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.EmpresaId == empresaIdFinal && p.Activo)
                .CountAsync();

            var productosCriticos = inventarios
                .Count(i => i.CantidadDisponible <= i.StockMinimo);

            // Nivel de inventario por categoría
            var inventarioPorCategoria = inventarios
                .Where(i => !string.IsNullOrEmpty(i.Producto.Subcategoria))
                .GroupBy(i => i.Producto.Subcategoria)
                .Select(g => new
                {
                    categoria = g.Key,
                    nivel = g.Sum(i => i.CantidadDisponible),
                    status = g.Average(i => (double)i.CantidadDisponible / (i.StockMinimo > 0 ? i.StockMinimo : 1)) > 2 ? "high" : "normal"
                })
                .Take(5)
                .ToList();

            var movimientosInventarioMensual = await _context.MovimientosInventarios
                .AsNoTracking()
                .Where(m => m.EmpresaId == empresaIdFinal && m.FechaMovimiento >= fechaDesde && m.FechaMovimiento <= fechaHasta)
                .GroupBy(m => new { m.FechaMovimiento.Year, m.FechaMovimiento.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    entradas = g.Sum(x =>
                        x.TipoMovimiento == MovimientoInventario.Entrada ||
                        x.TipoMovimiento == MovimientoInventario.Devolucion ||
                        (x.TipoMovimiento == MovimientoInventario.Ajuste && x.Cantidad > 0)
                            ? x.Cantidad
                            : 0),
                    salidas = g.Sum(x =>
                        x.TipoMovimiento == MovimientoInventario.Salida ||
                        x.TipoMovimiento == MovimientoInventario.Transferencia ||
                        (x.TipoMovimiento == MovimientoInventario.Ajuste && x.Cantidad < 0)
                            ? Math.Abs(x.Cantidad)
                            : 0)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var movimientos = movimientosInventarioMensual
                .Select(x => new
                {
                    month = $"{x.Year}-{x.Month:D2}",
                    entradas = x.entradas,
                    salidas = x.salidas
                })
                .ToList();

            var ventasPorProducto = await _context.DetallesVenta
                .AsNoTracking()
                .Where(d => d.Venta.EmpresaId == empresaIdFinal && d.Venta.FechaVenta >= fechaDesde && d.Venta.FechaVenta <= fechaHasta)
                .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
                .Select(g => new
                {
                    g.Key.ProductoId,
                    producto = g.Key.Nombre,
                    cantidadVendida = g.Sum(x => x.Cantidad)
                })
                .ToListAsync();

            var stockPorProducto = inventarios
                .GroupBy(i => i.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.CantidadDisponible));

            var rotacionCalculada = ventasPorProducto
                .Select(v =>
                {
                    stockPorProducto.TryGetValue(v.ProductoId, out var stockActual);
                    var baseStock = stockActual <= 0 ? 1 : stockActual;
                    var rot = (double)(v.cantidadVendida / baseStock);
                    return new
                    {
                        producto = v.producto,
                        rotacion = Math.Round(rot, 2)
                    };
                })
                .OrderByDescending(x => x.rotacion)
                .ToList();

            var rotacionTop = rotacionCalculada
                .Take(5)
                .Select(x => new { x.producto, x.rotacion, type = "top" });

            var rotacionBottom = rotacionCalculada
                .OrderBy(x => x.rotacion)
                .Take(5)
                .Select(x => new { x.producto, x.rotacion, type = "bottom" });

            var rotacion = rotacionTop.Concat(rotacionBottom).ToList();

            // ===== FINANCIEROS =====
            var financieros = await _context.DatosFinancieros
                .AsNoTracking()
                .Where(f => f.EmpresaId == empresaIdFinal &&
                           f.FechaRegistro >= fechaDesdeDateOnly &&
                           f.FechaRegistro <= fechaHastaDateOnly)
                .ToListAsync();

            var ingresos = financieros
                .Where(f => f.TipoDato == TipoDatoFinanciero.Ingreso)
                .Sum(f => f.Monto);

            var gastos = financieros
                .Where(f => f.TipoDato == TipoDatoFinanciero.Gasto)
                .Sum(f => f.Monto);

            var utilidad = ingresos - gastos;
            var rentabilidad = ingresos > 0 ? (utilidad / ingresos) * 100 : 0;

            // Flujo de caja por mes
            var flujoCajaPorMes = await _context.DatosFinancieros
                .AsNoTracking()
                .Where(f => f.EmpresaId == empresaIdFinal &&
                            f.FechaRegistro >= fechaDesdeDateOnly &&
                            f.FechaRegistro <= fechaHastaDateOnly)
                .GroupBy(f => new { f.Anio, f.Mes, f.TipoDato })
                .Select(g => new
                {
                    anio = g.Key.Anio,
                    mes = g.Key.Mes,
                    tipo = g.Key.TipoDato,
                    monto = g.Sum(f => f.Monto)
                })
                .OrderBy(x => x.anio)
                .ThenBy(x => x.mes)
                .ToListAsync();

            var distribucionGastos = financieros
                .Where(f => f.TipoDato == TipoDatoFinanciero.Gasto)
                .GroupBy(f => string.IsNullOrWhiteSpace(f.Categoria) ? "Sin categoría" : f.Categoria)
                .Select(g => new
                {
                    categoria = g.Key,
                    monto = g.Sum(x => x.Monto)
                })
                .OrderByDescending(x => x.monto)
                .ToList();

            var rentabilidadHistorica = financieros
                .GroupBy(f => new { f.Anio, f.Mes })
                .Select(g =>
                {
                    var ingresosMes = g.Where(x => x.TipoDato == TipoDatoFinanciero.Ingreso).Sum(x => x.Monto);
                    var gastosMes = g.Where(x => x.TipoDato == TipoDatoFinanciero.Gasto).Sum(x => x.Monto);
                    var rentMes = ingresosMes > 0 ? ((ingresosMes - gastosMes) / ingresosMes) * 100 : 0;
                    return new
                    {
                        year = g.Key.Anio,
                        month = g.Key.Mes,
                        monthLabel = $"{g.Key.Anio}-{g.Key.Mes:D2}",
                        rentabilidad = Math.Round(rentMes, 2),
                        utilidad = ingresosMes - gastosMes
                    };
                })
                .OrderBy(x => x.year)
                .ThenBy(x => x.month)
                .ToList();

            var proyeccion = new List<object>();
            var historicoProyeccion = rentabilidadHistorica
                .TakeLast(3)
                .ToList();

            foreach (var item in historicoProyeccion)
            {
                proyeccion.Add(new
                {
                    month = item.monthLabel,
                    real = item.utilidad,
                    optimista = (decimal?)null,
                    pesimista = (decimal?)null
                });
            }

            if (historicoProyeccion.Any())
            {
                var promedioUtilidad = historicoProyeccion.Average(x => x.utilidad);
                var ultimoRegistro = historicoProyeccion.Last();
                var ultimaFechaHistorica = new DateTime(ultimoRegistro.year ?? fechaHasta.Year, ultimoRegistro.month ?? fechaHasta.Month, 1);

                for (var i = 1; i <= 3; i++)
                {
                    var mesFuturo = ultimaFechaHistorica.AddMonths(i);
                    proyeccion.Add(new
                    {
                        month = $"{mesFuturo:yyyy-MM}",
                        real = (decimal?)null,
                        optimista = Math.Round(promedioUtilidad * 1.10m, 2),
                        pesimista = Math.Round(promedioUtilidad * 0.90m, 2)
                    });
                }
            }

            // ===== OPERACIONES =====
            var margenPromedio = ventasData.Any() && ventasData.Any(v => v.MargenBruto.HasValue)
                ? ventasData.Where(v => v.MargenBruto.HasValue).Average(v => v.MargenBruto!.Value)
                : 0;

            // Calcular días promedio de entrega
            var ventasConEntrega = ventasData
                .Where(v => v.FechaEntrega.HasValue)
                .ToList();

            var diasPromedioEntrega = ventasConEntrega.Any()
                ? ventasConEntrega.Average(v => (v.FechaEntrega!.Value - v.FechaVenta).TotalDays)
                : 0;

            var margenMensual = ventasData
                .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                .Select(g =>
                {
                    var totalMes = g.Sum(x => x.MontoTotal);
                    var costoMes = g.Sum(x => x.CostoTotal ?? 0);
                    var margenMes = totalMes > 0
                        ? ((totalMes - costoMes) / totalMes) * 100
                        : g.Where(x => x.MargenBruto.HasValue).Select(x => x.MargenBruto!.Value).DefaultIfEmpty(0).Average();
                    return new
                    {
                        month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        margen = Math.Round(margenMes, 2)
                    };
                })
                .OrderBy(x => x.month)
                .ToList();

            var eficienciaTurno = ventasData
                .GroupBy(v => v.FechaVenta.Hour < 12 ? "Mañana" : v.FechaVenta.Hour < 18 ? "Tarde" : "Noche")
                .Select(g => new
                {
                    turno = g.Key,
                    eficiencia = Math.Round(g.Any()
                        ? (g.Count(x => x.Estado == EstadoVenta.Completado) * 100.0m) / g.Count()
                        : 0, 2)
                })
                .OrderBy(x => x.turno)
                .ToList();

            var desperdicio = await _context.DetallesVenta
                .AsNoTracking()
                .Where(d => d.Venta.EmpresaId == empresaIdFinal &&
                            d.Venta.FechaVenta >= fechaDesde &&
                            d.Venta.FechaVenta <= fechaHasta &&
                            (d.CantidadDevuelta ?? 0) > 0)
                .GroupBy(d => !string.IsNullOrWhiteSpace(d.Producto.Subcategoria) ? d.Producto.Subcategoria! : d.Producto.Nombre)
                .Select(g => new
                {
                    material = g.Key,
                    desperdicio = g.Sum(x => (x.CantidadDevuelta ?? 0) * x.PrecioUnitario)
                })
                .OrderByDescending(x => x.desperdicio)
                .Take(10)
                .ToListAsync();

            var tiemposEtapa = new List<object>();
            if (diasPromedioEntrega > 0)
            {
                tiemposEtapa = new List<object>
                {
                    new { etapa = "Pedido", tiempo = Math.Round((decimal)diasPromedioEntrega * 0.10m, 2) },
                    new { etapa = "Producción", tiempo = Math.Round((decimal)diasPromedioEntrega * 0.55m, 2) },
                    new { etapa = "Control Calidad", tiempo = Math.Round((decimal)diasPromedioEntrega * 0.15m, 2) },
                    new { etapa = "Entrega", tiempo = Math.Round((decimal)diasPromedioEntrega * 0.20m, 2) }
                };
            }

            // ===== RESPUESTA CONSOLIDADA =====
            var respuesta = new
            {
                ventas = new
                {
                    total = totalIngresos,
                    ordenes = totalOrdenes,
                    promedioOrden = promedioOrden,
                    margen = margenPromedio,
                    crecimientoPorcentaje = Math.Round(crecimientoVentas, 1),
                    tendencia = crecimientoVentas >= 0 ? "up" : "down",
                    ventasPorMes = ventasPorMes,
                    topClientes = topClientes,
                    ventasPorCategoria = ventasPorCategoria,
                    pedidosRecientes = pedidosRecientes
                },
                inventario = new
                {
                    stockTotal = stockTotal,
                    productosActivos = productosActivos,
                    productosCriticos = productosCriticos,
                    inventarioPorCategoria = inventarioPorCategoria,
                    movimientos = movimientos,
                    rotacion = rotacion
                },
                financieros = new
                {
                    ingresos = ingresos,
                    gastos = gastos,
                    utilidad = utilidad,
                    rentabilidad = Math.Round(rentabilidad, 1),
                    flujoCajaPorMes = flujoCajaPorMes,
                    distribucionGastos = distribucionGastos,
                    rentabilidadHistorica = rentabilidadHistorica.Select(x => new { month = x.monthLabel, rentabilidad = x.rentabilidad }),
                    proyeccion = proyeccion
                },
                operaciones = new
                {
                    margenPromedio = Math.Round(margenPromedio, 1),
                    diasPromedioEntrega = Math.Round(diasPromedioEntrega, 1),
                    eficienciaGeneral = 87.3, // Placeholder - calcular según lógica de negocio
                    margenMensual = margenMensual,
                    eficienciaTurno = eficienciaTurno,
                    desperdicio = desperdicio,
                    tiemposEtapa = tiemposEtapa
                },
                metadata = new
                {
                    empresaId = empresaIdFinal,
                    fechaDesde = fechaDesde,
                    fechaHasta = fechaHasta,
                    filtroAplicado = filtroTiempo,
                    ultimaActualizacion = DateTime.UtcNow
                }
            };

            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }
}
