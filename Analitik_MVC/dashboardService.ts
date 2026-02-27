/**
 * Servicio para consumir API de Dashboard
 * NO ejecutar npm run build - este archivo se compilará automáticamente
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
  };
  inventario: {
    stockTotal: number;
    productosActivos: number;
    productosCriticos: number;
    inventarioPorCategoria: Array<{ categoria: string; nivel: number; status: string }>;
  };
  financieros: {
    ingresos: number;
    gastos: number;
    utilidad: number;
    rentabilidad: number;
    flujoCajaPorMes: Array<{ mes: number; tipo: string; monto: number }>;
  };
  operaciones: {
    margenPromedio: number;
    diasPromedioEntrega: number;
    eficienciaGeneral: number;
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
 * @param tipoFilter - Filtro de tiempo: "7d", "30d", "90d", "1y"
 * @returns Promise con datos del dashboard
 */
export async function getDashboardSummary(
  empresaId: string,
  tipoFilter: string = "30d"
): Promise<DashboardSummary> {
  try {
    const response = await fetch(
      `/api/dashboard/summary?empresaId=${empresaId}&tipoFilter=${tipoFilter}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include", // Incluir cookies de autenticación
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
 * Función helper para manejar errores de forma consistente
 */
export function handleDashboardError(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }
  return "Error desconocido al cargar datos del dashboard";
}
