import { getDashboardSummary, handleDashboardError, DashboardSummary } from "./dashboardService";
import React, { useState, useEffect } from 'react';
import { motion } from 'motion/react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { Badge } from './ui/badge';
import { Skeleton } from './ui/skeleton';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from './ui/tooltip';
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip as RechartsTooltip,
    ResponsiveContainer,
    BarChart,
    Bar,
    Cell,
    Legend,
    Area,
    AreaChart
} from 'recharts';
import {
    DollarSign,
    TrendingUp,
    Target,
    Wallet,
    Package,
    AlertTriangle,
    Archive,
    Activity,
    Clock,
    AlertCircle,
    ShoppingCart,
    Percent
} from 'lucide-react';

interface DashboardProps {
    type: string;
    isDarkMode?: boolean;
}

export function Dashboard({ type = 'ventas', isDarkMode = false }: DashboardProps) {
    const [timeFilter, setTimeFilter] = useState('7d');
    const [departmentFilter, setDepartmentFilter] = useState(type);
    const [empresaId, setEmpresaId] = useState<string>('');
    const [data, setData] = useState<DashboardSummary | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Chart axis colors - dark in light mode, light in dark mode
    const axisColor = isDarkMode ? '#E5E7EB' : '#4B5563';

    useEffect(() => {
        setDepartmentFilter(type);
    }, [type]);

    useEffect(() => {
        const obtenerEmpresaId = async () => {
            try {
                const response = await fetch('/Me', {
                    method: 'GET',
                    credentials: 'include'
                });

                if (!response.ok) {
                    throw new Error('No se pudo obtener la sesión del usuario');
                }

                const payload = await response.json();
                const id = payload?.user?.empresaId;

                if (!id) {
                    throw new Error('No se encontró el empresaId en la sesión');
                }

                setEmpresaId(id);
            } catch (err) {
                setError(handleDashboardError(err));
            }
        };

        obtenerEmpresaId();
    }, []);

    useEffect(() => {
        if (!empresaId) {
            return;
        }

        const fetchDashboard = async () => {
            try {
                setIsLoading(true);
                setError(null);

                const response = await getDashboardSummary(empresaId, timeFilter);
                setData(response);

            } catch (err) {
                setError(handleDashboardError(err));
            } finally {
                setIsLoading(false);
            }
        };

        fetchDashboard();
    }, [empresaId, timeFilter]);


    // ==================== VENTAS DATA ====================
    // Los datasets mock fueron removidos. Los datos reales se obtienen desde `data` (DashboardSummary)
    // Reemplazos en uso más abajo: ventasPorMesData, topClientesData, ventasPorCategoriaData, pedidosRecientesData
    // inventarioPorCategoriaData, inventarioMovimientosData, inventarioRotacionData
    // margenMensualData, eficienciaTurnoData, desperdicioData, tiemposEtapaData
    // distribucionGastosData, rentabilidadHistoricaData, proyeccionData, flujoCajaArray


    // ==================== OPERACIONES DATA ====================
    // ==================== FINANCIEROS DATA ====================
    const getKPIData = () => {
        switch (departmentFilter) {
            case 'ventas':
                return [
                    {
                        title: 'Ingresos Totales Mes',
                        value: formatCurrency(data?.ventas.total ?? 0),
                        change: '+12.3% vs mes anterior',
                        trend: 'up',
                        icon: DollarSign,
                        description: 'Dinero que entró por ventas este mes, sin devoluciones.',
                        linkText: 'Ver gráfico de tendencia',
                        relatedChart: 'trend'
                    },
                    {
                        title: 'Valor promedio por pedido',
                        value: formatCurrency(data?.ventas.promedioOrden ?? 0),
                        change: '+5.2% vs mes anterior',
                        trend: 'up',
                        icon: ShoppingCart,
                        description: 'Promedio de cuánto paga cada cliente por pedido.',
                        linkText: 'Ver gráfico de clientes',
                        relatedChart: 'clients'
                    },
                    {
                        title: 'Pedidos facturados (ventas)',
                        value: `${data?.ventas.ordenes ?? 0} órdenes`,
                        change: '+8.1% vs mes anterior',
                        trend: 'up',
                        icon: TrendingUp,
                        description: 'Cantidad de ventas cerradas y cobradas.',
                        linkText: 'Ver tabla de pedidos recientes',
                        relatedChart: 'orders'
                    },
                    {
                        title: 'Crecimiento Mensual',
                        value: `${data?.ventas.crecimientoPorcentaje ?? 0}%`,
                        trend: data?.ventas.tendencia === "up" ? "up" : "down",
                        change: 'vs objetivo: +10%',
                        icon: Percent,
                        description: 'Cambio de ingresos frente al mes pasado.',
                        linkText: 'Ver gráfico de tendencia',
                        relatedChart: 'trend'
                    }
                ];
            case 'inventarios':
                return [
                    {
                        title: 'Stock Total',
                        value: formatCurrency(data?.inventario.stockTotal ?? 0),
                        change: `${data?.inventario.productosActivos ?? 0} productos activos`,
                        trend: 'up',
                        icon: Package,
                        description: 'Valor total del inventario actual.',
                        linkText: 'Ver gráfico por categoría',
                        relatedChart: 'inventory-level'
                    },
                    {
                        title: 'Productos Activos',
                        value: `${data?.inventario.productosActivos ?? 0}`,
                        change: 'En inventario',
                        trend: 'up',
                        icon: Archive,
                        description: 'Cantidad total de productos disponibles.',
                        linkText: 'Ver entradas y salidas',
                        relatedChart: 'movements'
                    },
                    {
                        title: 'Productos Críticos',
                        value: `${data?.inventario.productosCriticos ?? 0}`,
                        change: 'Requiere atención',
                        trend: data?.inventario.productosCriticos ? 'down' : 'up',
                        icon: AlertTriangle,
                        description: 'Productos con bajo nivel de stock.',
                        linkText: 'Ver rotación',
                        relatedChart: 'rotation'
                    },
                    {
                        title: 'Valor Total Inventario',
                        value: formatCurrency(data?.inventario.stockTotal ?? 0),
                        change: 'Valor acumulado',
                        trend: 'up',
                        icon: DollarSign,
                        description: 'Valor financiero total almacenado.',
                        linkText: 'Ver detalle',
                        relatedChart: 'inventory-level'
                    }
                ];
            case 'operaciones':
                return [
                    {
                        title: 'Eficiencia General',
                        value: `${data?.operaciones.eficienciaGeneral ?? 0}%`,
                        change: 'Buena',
                        trend: 'up',
                        icon: Activity,
                        description: 'Qué tan cerca estamos del rendimiento esperado.',
                        linkText: 'Ver gráfico de eficiencia',
                        relatedChart: 'efficiency'
                    },
                    {
                        title: 'Tiempo desde pedido a entrega',
                        value: `${data?.operaciones.diasPromedioEntrega ?? 0} días`,
                        change: 'vs meta: 3 días',
                        trend: 'up',
                        icon: Clock,
                        description: 'Días promedio desde que se pide hasta que se entrega.',
                        linkText: 'Ver gráfico de tiempos por etapa',
                        relatedChart: 'times'
                    },
                    {
                        title: 'Desperdicios',
                        value: formatCurrency(desperdicioTotal),
                        change: desperdicioData.length > 0 ? `${desperdicioData.length} materiales analizados` : 'Sin datos para el período',
                        trend: desperdicioTotal > 0 ? 'down' : 'up',
                        icon: AlertCircle,
                        description: 'Lo que se pierde en materiales y su costo.',
                        linkText: 'Ver detalle de desperdicio',
                        relatedChart: 'waste'
                    },
                    {
                        title: 'Margen Promedio',
                        value: `${data?.operaciones.margenPromedio ?? 0}%`,
                        change: '+2.1% vs mes anterior',
                        trend: 'up',
                        icon: TrendingUp,
                        description: 'Ganancia promedio después de costos operativos.',
                        linkText: 'Ver gráfico mensual',
                        relatedChart: 'margins'
                    }
                ];
            case 'financieros':
                return [
                    {
                        title: 'Flujo de Caja',
                        value: formatCurrency(data?.financieros.utilidad ?? 0),
                        change: 'Positivo',
                        trend: 'up',
                        icon: DollarSign,
                        description: 'Entró más dinero del que salió este mes.',
                        linkText: 'Ver gráfico de flujo mensual',
                        relatedChart: 'cash-flow'
                    },
                    {
                        title: 'Rentabilidad Real',
                        value: `${data?.financieros.rentabilidad ?? 0}%`,
                        change: 'vs meta: 20%',
                        trend: 'up',
                        icon: TrendingUp,
                        description: 'Porcentaje de ganancia sobre los ingresos.',
                        linkText: 'Ver gráfico de rentabilidad',
                        relatedChart: 'profitability'
                    },
                    {
                        title: 'Punto Equilibrio',
                        value: formatCurrency(data?.financieros.ingresos ?? 0),
                        change: 'Alcanzado día 18',
                        trend: 'up',
                        icon: Target,
                        description: 'Ventas necesarias para cubrir todos los costos.',
                        linkText: 'Ver gráfico de gastos',
                        relatedChart: 'expenses'
                    },
                    {
                        title: 'Capital Trabajo',
                        value: formatCurrency(data?.financieros.gastos ?? 0),
                        change: '45 días cobertura',
                        trend: 'up',
                        icon: Wallet,
                        description: 'Días que podemos operar con lo disponible.',
                        linkText: 'Ver proyección de flujo',
                        relatedChart: 'projection'
                    }
                ];
            default:
                return [];
        }
    };

    const getDepartmentTitle = () => {
        switch (departmentFilter) {
            case 'ventas': return 'Panel de Ventas';
            case 'inventarios': return 'Panel de Inventarios';
            case 'operaciones': return 'Panel de Operaciones';
            case 'financieros': return 'Panel Financiero';
            default: return 'Panel de Control';
        }
    };

    const PlaceholderSinDatos = ({ mensaje = 'Sin datos disponibles para este período' }: { mensaje?: string }) => (
        <div className="h-[300px] flex items-center justify-center text-sm text-slate-600 dark:text-[#E5E7EB]">
            {mensaje}
        </div>
    );

    const ventasPorMesData = data?.ventas.ventasPorMes ?? [];
    const topClientesData = data?.ventas.topClientes ?? [];
    const ventasPorCategoriaData = data?.ventas.ventasPorCategoria ?? [];
    const pedidosRecientesData = data?.ventas.pedidosRecientes ?? [];
    const inventarioPorCategoriaData = data?.inventario.inventarioPorCategoria ?? [];
    const inventarioMovimientosData = data?.inventario.movimientos ?? [];
    const inventarioRotacionData = data?.inventario.rotacion ?? [];
    const productosMenorRotacion = inventarioRotacionData
        .filter((item: NonNullable<DashboardSummary['inventario']['rotacion']>[number]) => item.type === 'bottom')
        .slice(0, 5);
    const margenMensualData = data?.operaciones.margenMensual ?? [];
    const eficienciaTurnoData = data?.operaciones.eficienciaTurno ?? [];
    const desperdicioData = data?.operaciones.desperdicio ?? [];
    const tiemposEtapaData = data?.operaciones.tiemposEtapa ?? [];
    const distribucionGastosData = data?.financieros.distribucionGastos ?? [];
    const rentabilidadHistoricaData = data?.financieros.rentabilidadHistorica ?? [];
    const proyeccionData = data?.financieros.proyeccion ?? [];
    const desperdicioTotal = desperdicioData.reduce(
        (total: number, item: NonNullable<DashboardSummary['operaciones']['desperdicio']>[number]) => total + item.desperdicio,
        0
    );

    const flujoCajaTransformado = (data?.financieros.flujoCajaPorMes ?? []).reduce<Record<string, { month: string; entradas: number; salidas: number }>>(
        (acc: Record<string, { month: string; entradas: number; salidas: number }>, item: NonNullable<DashboardSummary['financieros']['flujoCajaPorMes']>[number]) => {
        const anio = item.anio ?? new Date().getFullYear();
        const key = `${anio}-${item.mes}`;

        if (!acc[key]) {
            acc[key] = {
                month: `${item.mes}/${anio}`,
                entradas: 0,
                salidas: 0
            };
        }

        const tipoNormalizado =
            typeof item.tipo === 'string' ? item.tipo.toLowerCase() : item.tipo;

        if (tipoNormalizado === 0 || tipoNormalizado === 'ingreso') {
            acc[key].entradas += item.monto;
        } else {
            acc[key].salidas += item.monto;
        }

        return acc;
    }, {});

    const flujoCajaArray = Object.values(flujoCajaTransformado);

    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('es-CO', {
            style: 'currency',
            currency: 'COP',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(value);
    };

    const scrollToChart = (chartId: string) => {
        const element = document.getElementById(chartId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });

            // Add highlight animation
            element.classList.add('ring-4', 'ring-blue-500', 'ring-opacity-50');
            setTimeout(() => {
                element.classList.remove('ring-4', 'ring-blue-500', 'ring-opacity-50');
            }, 2000);
        }
    };

    const CustomTooltip = ({ active, payload, label, formatter }: any) => {
        if (active && payload && payload.length) {
            return (
                <div className="bg-white dark:bg-[#273043] p-3 border border-slate-200 dark:border-[#374151] rounded-lg shadow-lg">
                    <p className="font-medium text-slate-900 dark:text-[#F1F5F9] mb-2">{label}</p>
                    {payload.map((entry: any, index: number) => (
                        <p key={index} style={{ color: entry.color }} className="text-sm">
                            {entry.name}: {formatter ? formatter(entry.value) : entry.value}
                        </p>
                    ))}
                </div>
            );
        }
        return null;
    };

    return (
        <div className="min-h-full bg-slate-50 dark:bg-[#111827] transition-colors duration-200">
            {/* Header */}
            <div className="bg-white dark:bg-[#1E293B] border-b border-slate-200 dark:border-[#374151] px-4 py-4 sm:px-6 lg:px-8 transition-colors duration-200">
                <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
                    <div className="min-w-0 flex-1">
                        <h1 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-[#F1F5F9] break-words">{getDepartmentTitle()}</h1>
                        <p className="text-sm sm:text-base text-slate-600 dark:text-[#E5E7EB] mt-1">Monitorea tus indicadores clave</p>
                    </div>
                    <div className="flex items-center gap-2 sm:gap-4 flex-wrap">
                        <Select value={timeFilter} onValueChange={setTimeFilter}>
                            <SelectTrigger className="w-full sm:w-32 transition-all duration-150 hover:scale-105 text-xs sm:text-sm">
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="7d">Últimos 7 días</SelectItem>
                                <SelectItem value="30d">Últimos 30 días</SelectItem>
                                <SelectItem value="90d">Últimos 3 meses</SelectItem>
                                <SelectItem value="1y">Último año</SelectItem>
                            </SelectContent>
                        </Select>
                        <Select value={departmentFilter} onValueChange={setDepartmentFilter}>
                            <SelectTrigger className="w-full sm:w-40 transition-all duration-150 hover:scale-105 text-xs sm:text-sm">
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="ventas">Ventas</SelectItem>
                                <SelectItem value="inventarios">Inventarios</SelectItem>
                                <SelectItem value="operaciones">Operaciones</SelectItem>
                                <SelectItem value="financieros">Financieros</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                </div>
            </div>

            <div className="p-4 sm:p-6 lg:p-8 space-y-6 sm:space-y-8">
                {/* KPI Cards */}
                <section>
                    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 sm:gap-6">
                        {isLoading ? (
                            // Skeleton Loading State
                            Array.from({ length: 4 }).map((_, index) => (
                                <Card key={index} className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151]">
                                    <CardContent className="p-6">
                                        <div className="flex items-center justify-between">
                                            <div className="flex-1">
                                                <Skeleton className="h-4 w-24 mb-2" />
                                                <Skeleton className="h-8 w-32 mb-2" />
                                                <Skeleton className="h-4 w-16" />
                                            </div>
                                            <Skeleton className="w-8 h-8 rounded-full" />
                                        </div>
                                    </CardContent>
                                </Card>
                            ))
                        ) : (
                            <TooltipProvider>
                                {getKPIData().map((kpi, index) => (
                                    <motion.div
                                        key={index}
                                        initial={{ opacity: 0, y: 20 }}
                                        animate={{ opacity: 1, y: 0 }}
                                        transition={{ duration: 0.3, delay: index * 0.05 }}
                                    >
                                        <Tooltip>
                                            <TooltipTrigger asChild>
                                                <div className="cursor-pointer">
                                                    <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg hover:scale-105 transition-all duration-200">
                                                        <CardContent className="p-4 sm:p-6">
                                                            <div className="flex items-start justify-between gap-2">
                                                                <div className="min-w-0 flex-1">
                                                                    <p className="text-xs sm:text-sm font-medium text-slate-500 dark:text-[#E5E7EB] mb-1 break-words leading-tight">{kpi.title}</p>
                                                                    <p className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-[#F1F5F9] break-words">{kpi.value}</p>
                                                                    <div className={`text-xs sm:text-sm font-medium mt-2 ${kpi.trend === 'up' ? 'text-green-600 dark:text-[#22C55E]' : 'text-yellow-600 dark:text-[#FACC15]'
                                                                        }`}>
                                                                        {kpi.trend === 'up' ? '↗' : '⚠'} {kpi.change}
                                                                    </div>
                                                                </div>
                                                                <kpi.icon className="w-7 h-7 sm:w-8 sm:h-8 text-blue-600 dark:text-[#60A5FA] flex-shrink-0" />
                                                            </div>
                                                        </CardContent>
                                                    </Card>
                                                </div>
                                            </TooltipTrigger>
                                            <TooltipContent className="max-w-xs">
                                                <p className="mb-2">{kpi.description}</p>
                                                <button
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        scrollToChart(kpi.relatedChart);
                                                    }}
                                                    className="text-blue-600 dark:text-[#60A5FA] hover:underline text-sm font-medium"
                                                >
                                                    {kpi.linkText}
                                                </button>
                                            </TooltipContent>
                                        </Tooltip>
                                    </motion.div>
                                ))}
                            </TooltipProvider>
                        )}
                    </div>
                </section>

                {/* Charts Section - 2x2 Grid */}
                {departmentFilter === 'ventas' && (
                    <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        {isLoading ? (
                            Array.from({ length: 4 }).map((_, index) => (
                                <Card key={index} className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151]">
                                    <CardHeader>
                                        <Skeleton className="h-6 w-32 mb-2" />
                                        <Skeleton className="h-4 w-48" />
                                    </CardHeader>
                                    <CardContent>
                                        <Skeleton className="h-[300px] w-full" />
                                    </CardContent>
                                </Card>
                            ))
                        ) : (
                            <>
                                {/* Tendencia de Ventas (12 meses) */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.2 }}
                                >
                                    <Card id="trend" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Tendencia de Ventas (12 meses)</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Ventas mensuales del período seleccionado</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {ventasPorMesData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <LineChart data={ventasPorMesData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="mes" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `$${value / 1000}K`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `$${value.toLocaleString()} COP`} />} />
                                                        <Legend />
                                                        <Line type="monotone" dataKey="ventas" stroke="#2563eb" strokeWidth={3} name="Ventas" dot={{ fill: '#2563eb', strokeWidth: 2, r: 4 }} connectNulls={false} />
                                                    </LineChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Top 5 Clientes */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.25 }}
                                >
                                    <Card id="clients" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Top 5 Clientes por compras</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Clientes con mayores compras del periodo</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {topClientesData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={topClientesData} layout="vertical">
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis type="number" stroke={axisColor} tickFormatter={(value) => `$${value / 1000}K`} />
                                                        <YAxis dataKey="cliente" type="category" width={130} stroke={axisColor} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `$${value.toLocaleString()} COP`} />} />
                                                        <Bar dataKey="ventas" fill="#2563eb" name="Ventas" radius={[0, 4, 4, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Ventas por Categoría */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.3 }}
                                >
                                    <Card id="category" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Ventas por Categoría</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Distribución de ventas por tipo de producto</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {ventasPorCategoriaData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={ventasPorCategoriaData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="categoria" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `$${value / 1000}K`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `$${value.toLocaleString()} COP`} />} />
                                                        <Bar dataKey="ventas" fill="#3B82F6" name="Ventas" radius={[4, 4, 0, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Pedidos recientes - Table */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.35 }}
                                >
                                    <Card id="orders" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Pedidos recientes (últimas 10 ventas)</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Últimas órdenes procesadas en tiempo real</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            <div className="overflow-x-auto max-h-[300px] overflow-y-auto">
                                                <table className="w-full">
                                                    <thead className="sticky top-0 bg-white dark:bg-[#273043]">
                                                        <tr className="border-b border-slate-200 dark:border-[#374151]">
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">ID</th>
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">Cliente</th>
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">Monto</th>
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">Categoría</th>
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">Estado</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        {pedidosRecientesData.length === 0 && (
                                                            <tr>
                                                                <td colSpan={5} className="py-6 text-center text-sm text-slate-600 dark:text-[#E5E7EB]">
                                                                    Sin datos disponibles para este período
                                                                </td>
                                                            </tr>
                                                        )}
                                                        {pedidosRecientesData.map((order, index) => (
                                                            <tr key={index} className="border-b border-slate-100 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155] transition-colors duration-150">
                                                                <td className="py-2 px-2 text-sm text-blue-600 dark:text-[#60A5FA] font-medium">{order.id}</td>
                                                                <td className="py-2 px-2 text-sm text-slate-900 dark:text-[#F1F5F9]">{order.cliente}</td>
                                                                <td className="py-2 px-2 text-sm font-medium text-slate-900 dark:text-[#F1F5F9]">{order.monto}</td>
                                                                <td className="py-2 px-2 text-sm text-slate-600 dark:text-[#E5E7EB]">{order.categoria}</td>
                                                                <td className="py-2 px-2 text-sm">
                                                                    <Badge
                                                                        variant={order.estado === 'Completado' ? 'default' : order.estado === 'Pendiente' ? 'secondary' : 'default'}
                                                                        className={
                                                                            order.estado === 'Completado'
                                                                                ? 'bg-green-100 dark:bg-[#22C55E]/20 text-green-800 dark:text-[#22C55E]'
                                                                                : order.estado === 'Pendiente'
                                                                                    ? 'bg-yellow-100 dark:bg-[#FACC15]/20 text-yellow-800 dark:text-[#FACC15]'
                                                                                    : 'bg-blue-100 dark:bg-[#60A5FA]/20 text-blue-800 dark:text-[#60A5FA]'
                                                                        }
                                                                    >
                                                                        {order.estado}
                                                                    </Badge>
                                                                </td>
                                                            </tr>
                                                        ))}
                                                    </tbody>
                                                </table>
                                            </div>
                                        </CardContent>
                                    </Card>
                                </motion.div>
                            </>
                        )}
                    </section>
                )}

                {departmentFilter === 'inventarios' && (
                    <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        {isLoading ? (
                            Array.from({ length: 4 }).map((_, index) => (
                                <Card key={index} className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151]">
                                    <CardHeader>
                                        <Skeleton className="h-6 w-32 mb-2" />
                                        <Skeleton className="h-4 w-48" />
                                    </CardHeader>
                                    <CardContent>
                                        <Skeleton className="h-[300px] w-full" />
                                    </CardContent>
                                </Card>
                            ))
                        ) : (
                            <>
                                {/* Nivel de Inventarios por Categoría */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.2 }}
                                >
                                    <Card id="inventory-level" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Nivel de Inventarios por Categoría</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Niveles de stock con indicadores de semáforo</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {inventarioPorCategoriaData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={inventarioPorCategoriaData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="categoria" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `${value} unidades`} />} />
                                                        <Bar dataKey="nivel" name="Nivel" radius={[4, 4, 0, 0]}>
                                                            {inventarioPorCategoriaData.map((entry, index) => (
                                                                <Cell
                                                                    key={`cell-${index}`}
                                                                    fill={entry.status === 'high' ? '#EF4444' : entry.status === 'low' ? '#22C55E' : '#3B82F6'}
                                                                />
                                                            ))}
                                                        </Bar>
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Productos de Menor Rotación - Table */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.25 }}
                                >
                                    <Card id="low-rotation" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Productos de Menor Rotación</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Inventario con menor velocidad de salida según el contrato real</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            <div className="overflow-x-auto max-h-[300px] overflow-y-auto">
                                                <table className="w-full">
                                                    <thead className="sticky top-0 bg-white dark:bg-[#273043]">
                                                        <tr className="border-b border-slate-200 dark:border-[#374151]">
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">Producto</th>
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">Rotación</th>
                                                            <th className="text-left py-2 px-2 text-xs font-medium text-slate-600 dark:text-[#E5E7EB]">Tipo</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        {productosMenorRotacion.length === 0 ? (
                                                            <tr>
                                                                <td colSpan={3} className="py-6 text-center text-sm text-slate-600 dark:text-[#E5E7EB]">
                                                                    Sin datos disponibles para este período
                                                                </td>
                                                            </tr>
                                                        ) : (
                                                            productosMenorRotacion.map((product, index) => (
                                                                <tr key={index} className="border-b border-slate-100 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155] transition-colors duration-150">
                                                                    <td className="py-2 px-2 text-sm text-slate-900 dark:text-[#F1F5F9]">{product.producto}</td>
                                                                    <td className="py-2 px-2 text-sm font-medium text-slate-900 dark:text-[#F1F5F9]">{product.rotacion}x</td>
                                                                    <td className="py-2 px-2 text-sm">
                                                                        <Badge variant="secondary" className="bg-slate-100 dark:bg-[#334155] text-slate-700 dark:text-[#E5E7EB]">
                                                                            Bajo
                                                                        </Badge>
                                                                    </td>
                                                                </tr>
                                                            ))
                                                        )}
                                                    </tbody>
                                                </table>
                                            </div>
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Movimientos de Inventario */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.3 }}
                                >
                                    <Card id="movements" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Movimientos de Inventario</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Entradas y salidas de inventario por mes</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {inventarioMovimientosData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={inventarioMovimientosData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="month" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `${value} unidades`} />} />
                                                        <Legend />
                                                        <Bar dataKey="entradas" fill="#22C55E" name="Entradas" radius={[4, 4, 0, 0]} />
                                                        <Bar dataKey="salidas" fill="#EF4444" name="Salidas" radius={[4, 4, 0, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Rotación por Producto */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.35 }}
                                >
                                    <Card id="rotation" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Rotación por Producto (Top/Bottom 10)</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Comparación de productos con mejor y peor rotación</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {inventarioRotacionData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={inventarioRotacionData} layout="vertical">
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis type="number" stroke={axisColor} />
                                                        <YAxis dataKey="producto" type="category" width={120} stroke={axisColor} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `${value}x rotación`} />} />
                                                        <Bar dataKey="rotacion" name="Rotación" radius={[0, 4, 4, 0]}>
                                                            {inventarioRotacionData.map((entry: any, index: number) => (
                                                                <Cell key={`cell-${index}`} fill={entry.type === 'top' ? '#22C55E' : '#EF4444'} />
                                                            ))}
                                                        </Bar>
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>
                            </>
                        )}
                    </section>
                )}

                {departmentFilter === 'operaciones' && (
                    <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        {isLoading ? (
                            Array.from({ length: 4 }).map((_, index) => (
                                <Card key={index} className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151]">
                                    <CardHeader>
                                        <Skeleton className="h-6 w-32 mb-2" />
                                        <Skeleton className="h-4 w-48" />
                                    </CardHeader>
                                    <CardContent>
                                        <Skeleton className="h-[300px] w-full" />
                                    </CardContent>
                                </Card>
                            ))
                        ) : (
                            <>
                                {/* Evolución del Margen Mensual */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.2 }}
                                >
                                    <Card id="margins" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Evolución del Margen Mensual</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Tendencia del margen promedio por mes</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {margenMensualData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <LineChart data={margenMensualData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="month" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `${value}%`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `${value}%`} />} />
                                                        <Line type="monotone" dataKey="margen" stroke="#2563eb" strokeWidth={3} name="Margen Promedio" dot={{ fill: '#2563eb', strokeWidth: 2, r: 4 }} />
                                                    </LineChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Eficiencia por turno */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.25 }}
                                >
                                    <Card id="efficiency" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Eficiencia por turno</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Eficiencia operativa por turno de trabajo</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {eficienciaTurnoData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={eficienciaTurnoData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="turno" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `${value}%`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `${value}%`} />} />
                                                        <Bar dataKey="eficiencia" fill="#3B82F6" name="Eficiencia" radius={[4, 4, 0, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Detalle de Desperdicio */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.3 }}
                                >
                                    <Card id="waste" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Detalle de Desperdicio</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Desperdicios por material con valor monetario</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {desperdicioData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={desperdicioData} layout="vertical">
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis type="number" stroke={axisColor} tickFormatter={(value) => `$${value / 1000}K`} />
                                                        <YAxis dataKey="material" type="category" width={100} stroke={axisColor} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => formatCurrency(value)} />} />
                                                        <Bar dataKey="desperdicio" fill="#EF4444" name="Desperdicio" radius={[0, 4, 4, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Tiempos por etapa del proceso */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.35 }}
                                >
                                    <Card id="times" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Tiempos por etapa del proceso</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Tiempo promedio en cada etapa operativa</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {tiemposEtapaData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={tiemposEtapaData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="etapa" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `${value}d`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `${value} días`} />} />
                                                        <Bar dataKey="tiempo" fill="#3B82F6" name="Tiempo" radius={[4, 4, 0, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>
                            </>
                        )}
                    </section>
                )}

                {departmentFilter === 'financieros' && (
                    <section className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        {isLoading ? (
                            Array.from({ length: 4 }).map((_, index) => (
                                <Card key={index} className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151]">
                                    <CardHeader>
                                        <Skeleton className="h-6 w-32 mb-2" />
                                        <Skeleton className="h-4 w-48" />
                                    </CardHeader>
                                    <CardContent>
                                        <Skeleton className="h-[300px] w-full" />
                                    </CardContent>
                                </Card>
                            ))
                        ) : (
                            <>
                                {/* Flujo de Caja Mensual */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.2 }}
                                >
                                    <Card id="cash-flow" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Flujo de Caja Mensual</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Comparación de entradas y salidas de efectivo</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {flujoCajaArray.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={flujoCajaArray}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="month" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `$${value / 1000}K`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => formatCurrency(value)} />} />
                                                        <Legend />
                                                        <Bar dataKey="entradas" fill="#22C55E" name="Entradas" radius={[4, 4, 0, 0]} />
                                                        <Bar dataKey="salidas" fill="#EF4444" name="Salidas" radius={[4, 4, 0, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Distribución de Gastos */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.25 }}
                                >
                                    <Card id="expenses" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Distribución de Gastos (mes)</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Principales categorías de gastos del periodo</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {distribucionGastosData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <BarChart data={distribucionGastosData} layout="vertical">
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis type="number" stroke={axisColor} tickFormatter={(value) => `$${value / 1000}K`} />
                                                        <YAxis dataKey="categoria" type="category" width={100} stroke={axisColor} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => formatCurrency(value)} />} />
                                                        <Bar dataKey="monto" fill="#3B82F6" name="Monto" radius={[0, 4, 4, 0]} />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Evolución Rentabilidad (12 meses) */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.3 }}
                                >
                                    <Card id="profitability" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Evolución Rentabilidad (12 meses)</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Tendencia de rentabilidad con área sombreada</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {rentabilidadHistoricaData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <AreaChart data={rentabilidadHistoricaData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="month" stroke={axisColor} angle={-45} textAnchor="end" height={80} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `${value}%`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => `${value}%`} />} />
                                                        <defs>
                                                            <linearGradient id="colorRentabilidad" x1="0" y1="0" x2="0" y2="1">
                                                                <stop offset="5%" stopColor="#2563eb" stopOpacity={0.8} />
                                                                <stop offset="95%" stopColor="#2563eb" stopOpacity={0.1} />
                                                            </linearGradient>
                                                        </defs>
                                                        <Area type="monotone" dataKey="rentabilidad" stroke="#2563eb" strokeWidth={3} fill="url(#colorRentabilidad)" name="Rentabilidad" />
                                                    </AreaChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>

                                {/* Proyección Próximos 3 Meses */}
                                <motion.div
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.3, delay: 0.35 }}
                                >
                                    <Card id="projection" className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] hover:shadow-lg transition-all duration-200 rounded-xl">
                                        <CardHeader>
                                            <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Proyección Próximos 3 Meses</CardTitle>
                                            <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Datos históricos y proyección con escenarios</CardDescription>
                                        </CardHeader>
                                        <CardContent>
                                            {proyeccionData.length === 0 ? (
                                                <PlaceholderSinDatos />
                                            ) : (
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <LineChart data={proyeccionData}>
                                                        <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                        <XAxis dataKey="month" stroke={axisColor} />
                                                        <YAxis stroke={axisColor} tickFormatter={(value) => `$${value / 1000}K`} />
                                                        <RechartsTooltip content={<CustomTooltip formatter={(value: number) => formatCurrency(value)} />} />
                                                        <Legend />
                                                        <Line type="monotone" dataKey="real" stroke="#2563eb" strokeWidth={3} name="Real" dot={{ fill: '#2563eb', strokeWidth: 2, r: 4 }} connectNulls={false} />
                                                        <Line type="monotone" dataKey="optimista" stroke="#22C55E" strokeWidth={2} strokeDasharray="5 5" name="Optimista" dot={{ fill: '#22C55E', strokeWidth: 2, r: 4 }} connectNulls={false} />
                                                        <Line type="monotone" dataKey="pesimista" stroke="#EF4444" strokeWidth={2} strokeDasharray="5 5" name="Pesimista" dot={{ fill: '#EF4444', strokeWidth: 2, r: 4 }} connectNulls={false} />
                                                    </LineChart>
                                                </ResponsiveContainer>
                                            )}
                                        </CardContent>
                                    </Card>
                                </motion.div>
                            </>
                        )}
                    </section>
                )}
            </div>
        </div>
    );
}