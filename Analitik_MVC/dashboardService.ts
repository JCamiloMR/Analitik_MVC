export interface VentaPorMes {
  mes: string;
  ventas: number;
}

export interface TopCliente {
  cliente: string;
  ventas: number;
}

export interface FlujoCajaPorMes {
  anio?: number;
  mes: number;
  tipo: number | string;
  monto: number;
}

export interface DashboardSummary {
  ventas: {
    total: number;
    ordenes: number;
    promedioOrden: number;
    margen: number;
    crecimientoPorcentaje: number;
    tendencia: string;
    ventasPorMes: VentaPorMes[];
    topClientes: TopCliente[];
    ventasPorCategoria?: Array<{ categoria: string; ventas: number }>;
    pedidosRecientes?: Array<{ id: string; cliente: string; monto: number; categoria: string; estado: string }>;
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
    flujoCajaPorMes: FlujoCajaPorMes[];
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

export async function getDashboardSummary(
  empresaId: string,
  timeFilter: string = '30d'
): Promise<DashboardSummary> {
  const queryEmpresaId = encodeURIComponent(empresaId);
  const queryTimeFilter = encodeURIComponent(timeFilter);

  const response = await fetch(
    `/api/dashboard/summary?empresaId=${queryEmpresaId}&timeFilter=${queryTimeFilter}&tipoFilter=${queryTimeFilter}`,
    {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include'
    }
  );

  if (!response.ok) {
    if (response.status === 404) {
      throw new Error('Empresa no encontrada o inactiva');
    }

    if (response.status === 500) {
      throw new Error('Error interno del servidor');
    }

    throw new Error(`Error HTTP: ${response.status}`);
  }

  return response.json() as Promise<DashboardSummary>;
}

export function handleDashboardError(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return 'Error desconocido al cargar datos del dashboard';
}
