/**
 * Servicio para consumir API de Dashboard
 * NO ejecutar npm run build - este archivo se compilar� autom�ticamente
 */

// Interfaces para tipar la respuesta del API
export interface DashboardSummary {
  ventas: {
    total: number;
    ordenes: number;
    promedioOrden: number;
    margen: number;
    crecimientoPorcentaje: number;
    tendencia: string;
    ventasPorMes: Array<{ mes: string; ventas: number }>;
    topClientes: Array<{ cliente: string; ventas: number }>;
    ventasPorCategoria?: Array<{ category: string; ventas: number }>;
    pedidosRecientes?: Array<{ id: string; cliente: string; monto: string; categoria: string; estado: string }>;
  };
  inventario: {
    stockTotal: number;
    productosActivos: number;
    productosCriticos: number;
    inventarioPorCategoria: Array<{ categoria: string; nivel: number; status: string }>;
    movimientos?: Array<{ month: string; entradas: number; salidas: number }>;
    rotacion?: Array<{ producto: string; rotacion: number; type: string }>;
  };
  financieros: {
    ingresos: number;
    gastos: number;
    utilidad: number;
    rentabilidad: number;
    flujoCajaPorMes: Array<{ mes: number; tipo: string; monto: number }>;
    flujoCaja?: Array<{ month: string; entradas: number; salidas: number }>;
    distribucionGastos?: Array<{ categoria: string; monto: number }>;
    rentabilidadHistorica?: Array<{ month: string; rentabilidad: number }>;
    proyeccion?: Array<{ month: string; real: number | null; optimista: number | null; pesimista: number | null }>;
  };
  operaciones: {
    margenPromedio: number;
    diasPromedioEntrega: number;
    eficienciaGeneral: number;
    margenMensual?: Array<{ month: string; margen: number }>;
    eficienciaTurno?: Array<{ turno: string; eficiencia: number }>;
    desperdicio?: Array<{ material: string; desperdicio: number }>;
    tiemposEtapa?: Array<{ etapa: string; tiempo: number }>;
  };
  metadata: {
    empresaId: string;
    fechaDesde: string;
    fechaHasta: string;
    filtroAplicado: string;
    ultimaActualizacion: string;
  };
}

/**
 * Obtiene resumen consolidado del dashboard
 * @param empresaId - ID de la empresa (GUID)
 * @param timeFilter - Filtro de tiempo: "7d", "30d", "90d", "1y"
 * @returns Promise con datos del dashboard
 */
export async function getDashboardSummary(
  empresaId: string,
  timeFilter: string = "30d"
): Promise<DashboardSummary> {
  try {
    const queryEmpresaId = encodeURIComponent(empresaId);
    const queryTimeFilter = encodeURIComponent(timeFilter);

    const response = await fetch(
      `/api/dashboard/summary?empresaId=${queryEmpresaId}&timeFilter=${queryTimeFilter}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include", // Incluir cookies de autenticaci�n
      }
    );

    if (!response.ok) {
      if (response.status === 404) {
        throw new Error("Empresa no encontrada o inactiva");
      }
      if (response.status === 500) {
        throw new Error("Error interno del servidor");
      }
      throw new Error(`Error HTTP: ${response.status}`);
    }

    const data: DashboardSummary = await response.json();
    return data;
  } catch (error) {
    console.error("Error al obtener resumen de dashboard:", error);
    throw error;
  }
}

/**
 * Funci�n helper para manejar errores de forma consistente
 */
export function handleDashboardError(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }
  return "Error desconocido al cargar datos del dashboard";
}
