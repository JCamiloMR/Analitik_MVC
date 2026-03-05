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
        [FromQuery] string tipoFilter = "2y")
    {
        try
        {
            Guid empresaId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value!); //este es el ide empresa

            // Validar empresa
            var empresaExists = await _context.Empresas.AnyAsync(e => e.Id == empresaId && e.Activa);
            if (!empresaExists)
            {
                return NotFound(new { error = "Empresa no encontrada o inactiva" });
            }

            // Calcular rango de fechas según filtro
            var fechaHasta = DateTime.UtcNow;
            var fechaDesde = tipoFilter switch
            {
                "7d" => fechaHasta.AddDays(-7),
                "30d" => fechaHasta.AddDays(-30),
                "90d" => fechaHasta.AddDays(-90),
                "2y" => fechaHasta.AddYears(-2),
                _ => fechaHasta.AddDays(-30)
            };

            // ===== VENTAS =====
            var ventasQuery = _context.Ventas
                .Where(v => v.EmpresaId == empresaId && v.FechaVenta >= fechaDesde && v.FechaVenta <= fechaHasta);

            var ventasData = await ventasQuery.ToListAsync();

            var totalIngresos = ventasData.Sum(v => v.MontoTotal);
            var totalOrdenes = ventasData.Count;
            var promedioOrden = totalOrdenes > 0 ? totalIngresos / totalOrdenes : 0;

            // Calcular crecimiento (comparar con período anterior)
            var periodoAnteriorDesde = fechaDesde.AddDays(-(fechaHasta - fechaDesde).TotalDays);
            var ventasAnterior = await _context.Ventas
                .Where(v => v.EmpresaId == empresaId && v.FechaVenta >= periodoAnteriorDesde && v.FechaVenta < fechaDesde)
                .SumAsync(v => v.MontoTotal);

            var crecimientoVentas = ventasAnterior > 0
                ? ((totalIngresos - ventasAnterior) / ventasAnterior) * 100
                : 0;

            var ventasPorMesData = await _context.Ventas
                .Where(v => v.EmpresaId == empresaId &&
                            v.FechaVenta >= DateTime.UtcNow.AddMonths(-6))
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
                .Where(v => v.EmpresaId == empresaId && v.FechaVenta >= fechaDesde)
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new
                {
                    cliente = g.Key,
                    ventas = g.Sum(v => v.MontoTotal)
                })
                .OrderByDescending(x => x.ventas)
                .Take(5)
                .ToListAsync();

            // ===== INVENTARIO =====
            var inventarios = await _context.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.Producto.EmpresaId == empresaId)
                .ToListAsync();

            var stockTotal = inventarios.Sum(i => i.CantidadDisponible * i.Producto.PrecioVenta);
            var productosActivos = await _context.Productos
                .Where(p => p.EmpresaId == empresaId && p.Activo)
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

            // ===== FINANCIEROS =====
            var financieros = await _context.DatosFinancieros
                .Where(f => f.EmpresaId == empresaId && 
                           f.FechaRegistro >= DateOnly.FromDateTime(fechaDesde))
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
                .Where(f => f.EmpresaId == empresaId && f.Anio == DateTime.UtcNow.Year)
                .GroupBy(f => new { f.Mes, f.TipoDato })
                .Select(g => new
                {
                    mes = g.Key.Mes,
                    tipo = g.Key.TipoDato,
                    monto = g.Sum(f => f.Monto)
                })
                .ToListAsync();

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
                    topClientes = topClientes
                },
                inventario = new
                {
                    stockTotal = stockTotal,
                    productosActivos = productosActivos,
                    productosCriticos = productosCriticos,
                    inventarioPorCategoria = inventarioPorCategoria
                },
                financieros = new
                {
                    ingresos = ingresos,
                    gastos = gastos,
                    utilidad = utilidad,
                    rentabilidad = Math.Round(rentabilidad, 1),
                    flujoCajaPorMes = flujoCajaPorMes
                },
                operaciones = new
                {
                    margenPromedio = Math.Round(margenPromedio, 1),
                    diasPromedioEntrega = Math.Round(diasPromedioEntrega, 1),
                    eficienciaGeneral = 87.3 // Placeholder - calcular según lógica de negocio
                },
                metadata = new
                {
                    empresaId = empresaId,
                    fechaDesde = fechaDesde,
                    fechaHasta = fechaHasta,
                    filtroAplicado = tipoFilter,
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
