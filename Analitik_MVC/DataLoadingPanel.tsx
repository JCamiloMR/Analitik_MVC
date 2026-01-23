import { useState, useEffect, useRef } from 'react';
import { motion } from 'motion/react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { Progress } from './ui/progress';
import { Skeleton } from './ui/skeleton';
import { FileSpreadsheet, Upload, FolderOpen, Eye, RotateCcw, AlertTriangle, CheckCircle2, XCircle, X, Download, Calendar, Clock, Database, TrendingUp } from 'lucide-react';

// ===================================================================
// TYPES / INTERFACES
// ===================================================================

interface ErrorValidacion {
  fila: number;
  columna: string;
  error: string;
  valorEncontrado?: string;
  sugerencia?: string;
  tipoDatoEsperado?: string;
}

interface ImportResumen {
  registrosProcesados: number;
  productosInsertados: number;
  productosActualizados: number;
  inventariosInsertados: number;
  ventasInsertadas: number;
  financierosInsertados: number;
  duracionSegundos: number;
  totalErrores?: number;
  totalAdvertencias?: number;
}

interface ImportResult {
  exitoso: boolean;
  mensaje: string;
  importId?: string;
  resumen?: ImportResumen;
  errores?: ErrorValidacion[];
  advertencias?: string[];
}

interface RecentImport {
  id: string;
  nombreArchivo: string;
  estado: string;
  fechaImportacion: string;
  registrosCargados: number;
  duracionSegundos?: number;
}

// Interfaz para los datos importados (vista tabular)
interface DatosImportados {
  metadata: {
    importId: string;
    nombreArchivo: string;
    fechaImportacion: string;
    estado: string;
    registrosCargados: number;
  };
  productos: Array<{
    codigoProducto: string;
    nombre: string;
    categoria: string | null;
    marca: string | null;
    precioVenta: number;
    costoUnitario: number | null;
    unidadMedida: string;
    activo: boolean;
    requiereInventario: boolean;
  }>;
  inventario: Array<{
    codigoProducto: string;
    nombreProducto: string;
    cantidadDisponible: number;
    cantidadReservada: number | null;
    stockMinimo: number | null;
    stockMaximo: number | null;
    ubicacion: string | null;
    estadoStock: string;
  }>;
  ventas: Array<{
    numeroOrden: string;
    fechaVenta: string;
    clienteNombre: string;
    montoSubtotal: number;
    montoDescuento: number | null;
    montoImpuestos: number | null;
    montoTotal: number;
    metodoPago: string;
    estado: string;
  }>;
  financieros: Array<{
    tipoDato: string;
    categoria: string;
    concepto: string;
    monto: number;
    fechaRegistro: string;
    beneficiario: string | null;
  }>;
}

// ===================================================================
// COMPONENTE PRINCIPAL
// ===================================================================

export function DataLoadingPanel() {
  // File upload state
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadStatus, setUploadStatus] = useState<'idle' | 'uploading' | 'success' | 'error'>('idle');

  // Import result state
  const [importResult, setImportResult] = useState<ImportResult | null>(null);

  // Loading state
  const [loadingImports, setLoadingImports] = useState(true);
  const [recentImports, setRecentImports] = useState<RecentImport[]>([]);

  // Modal de datos importados (vista tabular)
  const [showDataModal, setShowDataModal] = useState(false);
  const [datosImportados, setDatosImportados] = useState<DatosImportados | null>(null);
  const [loadingDatos, setLoadingDatos] = useState(false);
  const [tablaActiva, setTablaActiva] = useState<'productos' | 'inventario' | 'ventas' | 'financieros'>('productos');

  // Drag & drop state
  const [dragActive, setDragActive] = useState(false);

  // Refs
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Company ID (obtener del localStorage o props)
  const [empresaId] = useState<string>(
    localStorage.getItem('empresaId') || '00000000-0000-0000-0000-000000000000'
  );

  // API base URL
  const [apiBaseUrl] = useState<string>(
    import.meta.env.VITE_API_URL || 'http://localhost:5000'
  );

  // Progress interval ref
  const progressIntervalRef = useRef<number | null>(null);

  // ===================================================================
  // EFFECTS
  // ===================================================================

  // Load recent imports on mount
  useEffect(() => {
    loadRecentImports();
  }, [empresaId]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (progressIntervalRef.current) {
        clearInterval(progressIntervalRef.current);
      }
    };
  }, []);

  // ===================================================================
  // VALIDATION
  // ===================================================================

  /**
   * Validar archivo antes de subir
   */
  const validateFileBeforeUpload = (file: File): { valid: boolean; error?: string } => {
    // Validar extensión
    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      return {
        valid: false,
        error: `❌ El archivo debe ser Excel (.xlsx). Tu archivo: ${file.name}`
      };
    }

    // Validar tamaño (10 MB)
    const maxSizeBytes = 10 * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      return {
        valid: false,
        error: `❌ El archivo es muy grande (${(file.size / (1024 * 1024)).toFixed(2)} MB). Máximo: 10 MB`
      };
    }

    // Validar tipo MIME
    const validMimeTypes = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'application/octet-stream'
    ];
    if (file.type && !validMimeTypes.includes(file.type)) {
      return {
        valid: false,
        error: '❌ El archivo no es un Excel válido. Descarga la plantilla oficial.'
      };
    }

    return { valid: true };
  };

  // ===================================================================
  // API FUNCTIONS
  // ===================================================================

  /**
   * Cargar historial de importaciones desde el backend
   */
  const loadRecentImports = async () => {
    try {
      setLoadingImports(true);
      
      const response = await fetch(
        `/api/import/history?empresaId=${empresaId}&pagina=1&tamano=5`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      if (response.ok) {
        const data = await response.json();
        setRecentImports(data.datos || []);
      } else {
        console.warn('No se pudo cargar historial de importaciones');
        setRecentImports([]);
      }
    } catch (error) {
      console.error('Error loading imports:', error);
      setRecentImports([]);
    } finally {
      setLoadingImports(false);
    }
  };

  /**
   * Manejar subida de archivo al backend
   */
  const handleFileUpload = async (file: File) => {
    // 1. Validar antes de enviar
    const validation = validateFileBeforeUpload(file);
    if (!validation.valid) {
      setImportResult({
        exitoso: false,
        mensaje: validation.error!,
        errores: []
      });
      setUploadStatus('error');
      return;
    }

    // 2. Preparar estado
    setSelectedFile(file);
    setIsUploading(true);
    setUploadProgress(10);
    setUploadStatus('uploading');
    setImportResult(null);

    try {
      // 3. Construir FormData
      const formData = new FormData();
      formData.append('archivo', file);
      formData.append('empresaId', empresaId);

      // 4. Simular progreso mientras se envía (10-90%)
      progressIntervalRef.current = window.setInterval(() => {
        setUploadProgress(prev => {
          if (prev >= 90) {
            if (progressIntervalRef.current) {
              clearInterval(progressIntervalRef.current);
              progressIntervalRef.current = null;
            }
            return 90;
          }
          return prev + Math.random() * 15;
        });
      }, 300);

      // 5. Enviar POST al backend
      const response = await fetch(`/api/import/excel`, {
        method: 'POST',
        body: formData
        // NO incluir Content-Type - el navegador lo establece automáticamente con boundary
      });

      if (progressIntervalRef.current) {
        clearInterval(progressIntervalRef.current);
        progressIntervalRef.current = null;
      }
      setUploadProgress(95);

      // 6. Parsear respuesta
      const data: ImportResult = await response.json();

      // 7. Actualizar progreso final
      setUploadProgress(100);

      // 8. Manejar respuesta
      if (response.ok && data.exitoso) {
        // ✅ ÉXITO
        setImportResult(data);
        setUploadStatus('success');
        
        // Recargar historial de importaciones
        setTimeout(() => {
          loadRecentImports();
        }, 1000);
      } else {
        // ❌ ERROR (con detalles)
        setImportResult(data);
        setUploadStatus('error');
      }

    } catch (error: any) {
      // ❌ ERROR DE RED
      console.error('Error uploading file:', error);
      
      if (progressIntervalRef.current) {
        clearInterval(progressIntervalRef.current);
        progressIntervalRef.current = null;
      }
      setUploadProgress(0);
      setUploadStatus('error');
      
      setImportResult({
        exitoso: false,
        mensaje: `❌ Error de conexión: ${error.message || 'No se pudo conectar al servidor. Verifica que el backend está corriendo.'}`,
        errores: []
      });
    } finally {
      setIsUploading(false);
    }
  };

  // ===================================================================
  // EVENT HANDLERS
  // ===================================================================

  /**
   * Handlers para drag & drop
   */
  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true);
    } else if (e.type === "dragleave") {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileUpload(e.dataTransfer.files[0]);
    }
  };

  /**
   * Handler para click en área de upload
   */
  const handleFileInputClick = () => {
    fileInputRef.current?.click();
  };

  /**
   * Handler para cambio en input file
   */
  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      handleFileUpload(file);
      // Reset input para permitir subir el mismo archivo nuevamente
      e.target.value = '';
    }
  };

  /**
   * Resetear estado para nueva carga
   */
  const resetUploadState = () => {
    setImportResult(null);
    setUploadProgress(0);
    setUploadStatus('idle');
    setSelectedFile(null);
  };

  /**
   * Cargar datos importados (vista tabular tipo Excel)
   */
  const handleVerDatosImportados = async (importId: string) => {
    setLoadingDatos(true);
    setShowDataModal(true);
    setTablaActiva('productos'); // Resetear pestaña activa
    
    try {
      const response = await fetch(`/api/import/report/${importId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const data: DatosImportados = await response.json();
        setDatosImportados(data);
      } else {
        console.error('Error al cargar datos:', response.statusText);
        setDatosImportados(null);
      }
    } catch (error) {
      console.error('Error loading data:', error);
      setDatosImportados(null);
    } finally {
      setLoadingDatos(false);
    }
  };

  // ===================================================================
  // UTILITY FUNCTIONS
  // ===================================================================

  /**
   * Formatear fecha relativa
   */
  const formatRelativeDate = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Hace un momento';
    if (diffMins < 60) return `Hace ${diffMins} minuto${diffMins > 1 ? 's' : ''}`;
    if (diffHours < 24) return `Hace ${diffHours} hora${diffHours > 1 ? 's' : ''}`;
    if (diffDays === 1) return 'Hace 1 día';
    if (diffDays < 7) return `Hace ${diffDays} días`;
    if (diffDays < 30) return `Hace ${Math.floor(diffDays / 7)} semana${Math.floor(diffDays / 7) > 1 ? 's' : ''}`;
    return date.toLocaleDateString('es-CO');
  };

  /**
   * Obtener estado badge para tabla
   */
  const getEstadoBadge = (estado: string) => {
    switch (estado.toLowerCase()) {
      case 'completado':
        return <Badge className="bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-400 hover:bg-green-100 dark:hover:bg-green-900/30 border-green-200 dark:border-green-800">Exitoso</Badge>;
      case 'fallido':
        return <Badge variant="destructive" className="dark:bg-red-900/30 dark:text-red-400 dark:border-red-800">Fallido</Badge>;
      case 'en_proceso':
        return <Badge variant="outline" className="border-blue-300 dark:border-blue-700 text-blue-700 dark:text-blue-400">En Proceso</Badge>;
      default:
        return <Badge variant="outline">{estado}</Badge>;
    }
  };

  // ===================================================================
  // RENDER
  // ===================================================================

  return (
    <div className="min-h-full bg-slate-50 dark:bg-[#111827] transition-colors duration-200">
      {/* Modal de Datos Importados (Vista Tabular Tipo Excel) */}
      {showDataModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4 overflow-hidden">
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.95 }}
            className="bg-white dark:bg-[#1E293B] rounded-lg shadow-2xl w-full max-w-7xl h-[90vh] flex flex-col"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-6 border-b border-slate-200 dark:border-[#374151]">
              <div className="flex items-center space-x-3">
                <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-lg">
                  <FileSpreadsheet className="w-6 h-6 text-blue-600 dark:text-blue-400" />
                </div>
                <div>
                  <h2 className="text-2xl font-bold text-slate-900 dark:text-[#F1F5F9]">
                    Datos Importados
                  </h2>
                  {datosImportados && (
                    <p className="text-sm text-slate-600 dark:text-[#E5E7EB] mt-1">
                      {datosImportados.metadata.nombreArchivo} • {datosImportados.metadata.registrosCargados} registros
                    </p>
                  )}
                </div>
              </div>
              <button
                onClick={() => setShowDataModal(false)}
                className="p-2 hover:bg-slate-100 dark:hover:bg-[#334155] rounded-lg transition-colors"
              >
                <X className="w-6 h-6 text-slate-600 dark:text-[#E5E7EB]" />
              </button>
            </div>

            {/* Pestañas */}
            <div className="flex space-x-1 p-4 border-b border-slate-200 dark:border-[#374151] bg-slate-50 dark:bg-[#273043]">
              {['productos', 'inventario', 'ventas', 'financieros'].map((tab) => (
                <button
                  key={tab}
                  onClick={() => setTablaActiva(tab as any)}
                  className={`px-4 py-2 rounded-md font-medium transition-colors ${
                    tablaActiva === tab
                      ? 'bg-blue-600 text-white'
                      : 'bg-white dark:bg-[#1E293B] text-slate-700 dark:text-[#E5E7EB] hover:bg-slate-100 dark:hover:bg-[#334155]'
                  }`}
                >
                  {tab.charAt(0).toUpperCase() + tab.slice(1)} {datosImportados && `(${(datosImportados as any)[tab]?.length || 0})`}
                </button>
              ))}
            </div>

            {/* Body con Tabla */}
            <div className="flex-1 overflow-auto p-6">
              {loadingDatos ? (
                <div className="flex flex-col items-center justify-center h-full space-y-4">
                  <div className="w-16 h-16 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
                  <p className="text-slate-600 dark:text-[#E5E7EB]">Cargando datos importados...</p>
                </div>
              ) : datosImportados ? (
                <>
                  {/* Tabla de Productos */}
                  {tablaActiva === 'productos' && (
                    <div className="overflow-x-auto">
                      <table className="w-full border-collapse">
                        <thead>
                          <tr className="bg-slate-100 dark:bg-[#273043] border-b-2 border-slate-300 dark:border-[#374151]">
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Código</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Nombre</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Categoría</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Marca</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Precio Venta</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Costo</th>
                            <th className="text-center py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Unidad</th>
                            <th className="text-center py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9]">Activo</th>
                          </tr>
                        </thead>
                        <tbody>
                          {datosImportados.productos.map((producto, idx) => (
                            <tr key={idx} className="border-b border-slate-200 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155]">
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] font-mono text-sm text-slate-900 dark:text-[#F1F5F9]">{producto.codigoProducto}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9]">{producto.nombre}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-600 dark:text-[#E5E7EB]">{producto.categoria || '-'}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-600 dark:text-[#E5E7EB]">{producto.marca || '-'}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right font-semibold text-green-600 dark:text-green-400">
                                ${producto.precioVenta.toLocaleString('es-CO')}
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right text-slate-600 dark:text-[#E5E7EB]">
                                ${producto.costoUnitario?.toLocaleString('es-CO') || '-'}
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-center text-xs text-slate-600 dark:text-[#E5E7EB]">{producto.unidadMedida}</td>
                              <td className="py-3 px-4 text-center">
                                {producto.activo ? (
                                  <span className="inline-block w-2 h-2 bg-green-500 rounded-full"></span>
                                ) : (
                                  <span className="inline-block w-2 h-2 bg-red-500 rounded-full"></span>
                                )}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      {datosImportados.productos.length === 0 && (
                        <div className="text-center py-12 text-slate-500 dark:text-[#E5E7EB]">
                          <FileSpreadsheet className="w-12 h-12 mx-auto mb-3 opacity-50" />
                          <p>No hay productos en esta importación</p>
                        </div>
                      )}
                    </div>
                  )}

                  {/* Tabla de Inventario */}
                  {tablaActiva === 'inventario' && (
                    <div className="overflow-x-auto">
                      <table className="w-full border-collapse">
                        <thead>
                          <tr className="bg-slate-100 dark:bg-[#273043] border-b-2 border-slate-300 dark:border-[#374151]">
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Código</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Producto</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Disponible</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Reservada</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Stock Min</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Stock Max</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Ubicación</th>
                            <th className="text-center py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9]">Estado</th>
                          </tr>
                        </thead>
                        <tbody>
                          {datosImportados.inventario.map((item, idx) => (
                            <tr key={idx} className="border-b border-slate-200 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155]">
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] font-mono text-sm text-slate-900 dark:text-[#F1F5F9]">{item.codigoProducto}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9]">{item.nombreProducto}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right font-semibold text-blue-600 dark:text-blue-400">{item.cantidadDisponible}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right text-slate-600 dark:text-[#E5E7EB]">{item.cantidadReservada || 0}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right text-slate-600 dark:text-[#E5E7EB]">{item.stockMinimo || '-'}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right text-slate-600 dark:text-[#E5E7EB]">{item.stockMaximo || '-'}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-600 dark:text-[#E5E7EB]">{item.ubicacion || '-'}</td>
                              <td className="py-3 px-4 text-center">
                                <Badge className={`${
                                  item.estadoStock === 'normal' ? 'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-400' :
                                  item.estadoStock === 'bajo' ? 'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-400' :
                                  'bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-400'
                                }`}>
                                  {item.estadoStock}
                                </Badge>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      {datosImportados.inventario.length === 0 && (
                        <div className="text-center py-12 text-slate-500 dark:text-[#E5E7EB]">
                          <Database className="w-12 h-12 mx-auto mb-3 opacity-50" />
                          <p>No hay inventario en esta importación</p>
                        </div>
                      )}
                    </div>
                  )}

                  {/* Tabla de Ventas */}
                  {tablaActiva === 'ventas' && (
                    <div className="overflow-x-auto">
                      <table className="w-full border-collapse">
                        <thead>
                          <tr className="bg-slate-100 dark:bg-[#273043] border-b-2 border-slate-300 dark:border-[#374151]">
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">N° Orden</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Fecha</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Cliente</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Subtotal</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Descuento</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Impuestos</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Total</th>
                            <th className="text-center py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Pago</th>
                            <th className="text-center py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9]">Estado</th>
                          </tr>
                        </thead>
                        <tbody>
                          {datosImportados.ventas.map((venta, idx) => (
                            <tr key={idx} className="border-b border-slate-200 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155]">
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] font-mono text-sm text-slate-900 dark:text-[#F1F5F9]">{venta.numeroOrden}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-sm text-slate-600 dark:text-[#E5E7EB]">
                                {new Date(venta.fechaVenta).toLocaleDateString('es-CO')}
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9]">{venta.clienteNombre}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right text-slate-600 dark:text-[#E5E7EB]">${venta.montoSubtotal.toLocaleString('es-CO')}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right text-orange-600 dark:text-orange-400">
                                {venta.montoDescuento ? `-$${venta.montoDescuento.toLocaleString('es-CO')}` : '-'}
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right text-slate-600 dark:text-[#E5E7EB]">
                                {venta.montoImpuestos ? `+$${venta.montoImpuestos.toLocaleString('es-CO')}` : '-'}
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right font-bold text-green-600 dark:text-green-400">
                                ${venta.montoTotal.toLocaleString('es-CO')}
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-center text-xs text-slate-600 dark:text-[#E5E7EB]">{venta.metodoPago}</td>
                              <td className="py-3 px-4 text-center">
                                <Badge className={venta.estado === 'completado' 
                                  ? 'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-400' 
                                  : 'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-400'
                                }>
                                  {venta.estado}
                                </Badge>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      {datosImportados.ventas.length === 0 && (
                        <div className="text-center py-12 text-slate-500 dark:text-[#E5E7EB]">
                          <TrendingUp className="w-12 h-12 mx-auto mb-3 opacity-50" />
                          <p>No hay ventas en esta importación</p>
                        </div>
                      )}
                    </div>
                  )}

                  {/* Tabla de Financieros */}
                  {tablaActiva === 'financieros' && (
                    <div className="overflow-x-auto">
                      <table className="w-full border-collapse">
                        <thead>
                          <tr className="bg-slate-100 dark:bg-[#273043] border-b-2 border-slate-300 dark:border-[#374151]">
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Tipo</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Categoría</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Concepto</th>
                            <th className="text-right py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Monto</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9] border-r border-slate-200 dark:border-[#374151]">Fecha</th>
                            <th className="text-left py-3 px-4 font-semibold text-slate-900 dark:text-[#F1F5F9]">Beneficiario</th>
                          </tr>
                        </thead>
                        <tbody>
                          {datosImportados.financieros.map((item, idx) => (
                            <tr key={idx} className="border-b border-slate-200 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155]">
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151]">
                                <Badge className={`${
                                  item.tipoDato === 'ingreso' ? 'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-400' :
                                  item.tipoDato === 'gasto' ? 'bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-400' :
                                  'bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-400'
                                }`}>
                                  {item.tipoDato}
                                </Badge>
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9]">{item.categoria}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9]">{item.concepto}</td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-right font-bold">
                                <span className={item.tipoDato === 'ingreso' ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}>
                                  {item.tipoDato === 'ingreso' ? '+' : '-'}${item.monto.toLocaleString('es-CO')}
                                </span>
                              </td>
                              <td className="py-3 px-4 border-r border-slate-200 dark:border-[#374151] text-sm text-slate-600 dark:text-[#E5E7EB]">
                                {new Date(item.fechaRegistro).toLocaleDateString('es-CO')}
                              </td>
                              <td className="py-3 px-4 text-slate-600 dark:text-[#E5E7EB]">{item.beneficiario || '-'}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      {datosImportados.financieros.length === 0 && (
                        <div className="text-center py-12 text-slate-500 dark:text-[#E5E7EB]">
                          <Database className="w-12 h-12 mx-auto mb-3 opacity-50" />
                          <p>No hay datos financieros en esta importación</p>
                        </div>
                      )}
                    </div>
                  )}
                </>
              ) : (
                <div className="flex flex-col items-center justify-center h-full space-y-4">
                  <XCircle className="w-16 h-16 text-red-600 dark:text-red-400" />
                  <p className="text-slate-600 dark:text-[#E5E7EB] text-center">
                    No se pudieron cargar los datos importados.
                  </p>
                </div>
              )}
            </div>

            {/* Footer */}
            <div className="flex justify-between items-center p-6 border-t border-slate-200 dark:border-[#374151] bg-slate-50 dark:bg-[#273043]">
              <div className="text-sm text-slate-600 dark:text-[#E5E7EB]">
                {datosImportados && (
                  <span>Mostrando hasta 100 registros más recientes por tabla</span>
                )}
              </div>
              <Button
                variant="outline"
                onClick={() => setShowDataModal(false)}
                className="border-slate-300 dark:border-[#374151] text-slate-700 dark:text-[#E5E7EB]"
              >
                Cerrar
              </Button>
            </div>
          </motion.div>
        </div>
      )}

      {/* Header */}
      <div className="bg-white dark:bg-[#1E293B] border-b border-slate-200 dark:border-[#374151] px-4 py-4 sm:px-6 lg:px-8 transition-colors duration-200">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-[#F1F5F9] break-words">
            Importa tus datos
          </h1>
          <p className="text-sm sm:text-base text-slate-600 dark:text-[#E5E7EB] mt-1">
            Sube archivos Excel para importar tus datos
          </p>
        </div>
      </div>

      {loadingImports ? (
        // ===================================================================
        // LOADING SKELETON
        // ===================================================================
        <div className="p-4 sm:p-6 lg:p-8 space-y-6 sm:space-y-8">
          <section className="grid grid-cols-1 gap-6 sm:gap-8">
            <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
              <CardHeader>
                <div className="flex items-center space-x-3 mb-2">
                  <Skeleton className="w-6 h-6 rounded" />
                  <Skeleton className="h-6 w-48" />
                </div>
                <Skeleton className="h-4 w-64" />
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="border-2 border-dashed border-slate-300 dark:border-[#374151] rounded-lg p-8 transition-colors duration-200">
                  <div className="text-center space-y-4">
                    <Skeleton className="w-16 h-16 mx-auto rounded-lg" />
                    <div className="space-y-2">
                      <Skeleton className="h-5 w-56 mx-auto" />
                      <Skeleton className="h-4 w-40 mx-auto" />
                    </div>
                    <Skeleton className="h-10 w-32 mx-auto rounded-md" />
                  </div>
                </div>
                <div className="text-center space-y-2">
                  <Skeleton className="h-4 w-64 mx-auto" />
                  <Skeleton className="h-4 w-48 mx-auto" />
                </div>
              </CardContent>
            </Card>
          </section>

          <section>
            <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
              <CardHeader>
                <Skeleton className="h-6 w-48 mb-2" />
                <Skeleton className="h-4 w-72" />
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-slate-200 dark:border-[#374151]">
                        <th className="text-left py-3 px-4"><Skeleton className="h-4 w-20" /></th>
                        <th className="text-left py-3 px-4"><Skeleton className="h-4 w-16" /></th>
                        <th className="text-left py-3 px-4"><Skeleton className="h-4 w-20" /></th>
                        <th className="text-left py-3 px-4"><Skeleton className="h-4 w-16" /></th>
                        <th className="text-left py-3 px-4"><Skeleton className="h-4 w-20" /></th>
                      </tr>
                    </thead>
                    <tbody>
                      {Array.from({ length: 5 }).map((_, index) => (
                        <tr key={index} className="border-b border-slate-100 dark:border-[#374151]">
                          <td className="py-3 px-4"><Skeleton className="h-4 w-40" /></td>
                          <td className="py-3 px-4"><Skeleton className="h-6 w-20 rounded-full" /></td>
                          <td className="py-3 px-4"><Skeleton className="h-4 w-24" /></td>
                          <td className="py-3 px-4"><Skeleton className="h-6 w-20 rounded-full" /></td>
                          <td className="py-3 px-4">
                            <div className="flex space-x-2">
                              <Skeleton className="h-8 w-16 rounded-md" />
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          </section>
        </div>
      ) : (
        // ===================================================================
        // MAIN CONTENT
        // ===================================================================
        <div className="p-6 lg:p-8 space-y-8">
          {/* Excel Upload Section */}
          <section>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.3 }}
            >
              <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
                <CardHeader>
                  <CardTitle className="flex items-center space-x-3 text-slate-900 dark:text-[#F1F5F9]">
                    <FileSpreadsheet className="w-6 h-6 text-blue-600 dark:text-[#60A5FA]" />
                    <span>Importar desde Excel</span>
                  </CardTitle>
                  <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">
                    Sube archivos Excel (.xlsx) para importar tus datos rápidamente
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-6">
                  {/* Drag & Drop Zone */}
                  <div
                    className={`border-2 border-dashed rounded-lg p-8 text-center transition-all cursor-pointer ${
                      dragActive 
                        ? 'border-blue-500 dark:border-[#60A5FA] bg-blue-50 dark:bg-[#334155] scale-105' 
                        : 'border-slate-300 dark:border-[#374151] hover:border-slate-400 dark:hover:border-[#60A5FA] hover:bg-slate-50 dark:hover:bg-[#334155]'
                    } ${isUploading ? 'pointer-events-none opacity-50' : ''}`}
                    onDragEnter={handleDrag}
                    onDragLeave={handleDrag}
                    onDragOver={handleDrag}
                    onDrop={handleDrop}
                    onClick={!isUploading ? handleFileInputClick : undefined}
                  >
                    <div className="space-y-4">
                      <FolderOpen className="w-16 h-16 text-blue-600 dark:text-[#60A5FA] mx-auto" />
                      <div>
                        <p className="text-lg font-medium text-slate-900 dark:text-[#F1F5F9]">
                          {dragActive ? 'Suelta tu archivo aquí' : 'Arrastra y suelta tu archivo aquí'}
                        </p>
                        <p className="text-slate-500 dark:text-[#E5E7EB] mt-1">o haz clic para seleccionar un archivo</p>
                      </div>
                      {!isUploading && (
                        <Button 
                          variant="outline"
                          className="border-slate-300 dark:border-[#374151] text-slate-700 dark:text-[#E5E7EB] hover:bg-slate-50 dark:hover:bg-[#334155]"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleFileInputClick();
                          }}
                        >
                          <Upload className="w-4 h-4 mr-2" />
                          Seleccionar Archivo
                        </Button>
                      )}
                      <input
                        ref={fileInputRef}
                        type="file"
                        accept=".xlsx"
                        onChange={handleFileInputChange}
                        className="hidden"
                      />
                    </div>
                  </div>

                  {/* Selected File Info */}
                  {selectedFile && !importResult && (
                    <div className="flex items-center justify-between p-3 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
                      <div className="flex items-center space-x-3">
                        <FileSpreadsheet className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                        <div>
                          <p className="text-sm font-medium text-slate-900 dark:text-[#F1F5F9]">{selectedFile.name}</p>
                          <p className="text-xs text-slate-600 dark:text-[#E5E7EB]">
                            {(selectedFile.size / (1024 * 1024)).toFixed(2)} MB
                          </p>
                        </div>
                      </div>
                      {uploadStatus === 'uploading' && (
                        <div className="flex items-center space-x-2">
                          <span className="text-sm text-blue-700 dark:text-blue-400">Subiendo...</span>
                          <div className="w-5 h-5 border-2 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
                        </div>
                      )}
                    </div>
                  )}

                  {/* Upload Progress */}
                  {isUploading && (
                    <motion.div 
                      className="space-y-2"
                      initial={{ opacity: 0, height: 0 }}
                      animate={{ opacity: 1, height: 'auto' }}
                      transition={{ duration: 0.2 }}
                    >
                      <div className="flex justify-between text-sm text-slate-700 dark:text-[#E5E7EB]">
                        <span>Procesando archivo...</span>
                        <span>{Math.round(uploadProgress)}%</span>
                      </div>
                      <Progress value={uploadProgress} className="w-full" />
                      <p className="text-xs text-slate-500 dark:text-[#E5E7EB] text-center">
                        {uploadProgress < 30 && 'Validando estructura...'}
                        {uploadProgress >= 30 && uploadProgress < 60 && 'Leyendo datos...'}
                        {uploadProgress >= 60 && uploadProgress < 90 && 'Validando registros...'}
                        {uploadProgress >= 90 && uploadProgress < 100 && 'Cargando a base de datos...'}
                        {uploadProgress === 100 && 'Completado!'}
                      </p>
                    </motion.div>
                  )}

                  {/* Supported Formats */}
                  <div className="text-center text-sm text-slate-500 dark:text-[#E5E7EB]">
                    <p>✅ Formato soportado: <strong>.xlsx</strong> (Excel 2013+)</p>
                    <p>📊 Tamaño máximo: <strong>10 MB</strong> (~5,000 registros)</p>
                    <p className="mt-2 text-xs">
                      <a href="/plantilla-claridata.xlsx" className="text-blue-600 dark:text-blue-400 hover:underline" download>
                        📥 Descargar plantilla oficial
                      </a>
                    </p>
                  </div>
                </CardContent>
              </Card>
            </motion.div>
          </section>

          {/* MOSTRAR RESULTADO DE UPLOAD */}
          {importResult && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.3 }}
            >
              <Card className={`border-2 ${
                importResult.exitoso 
                  ? 'border-green-500 dark:border-green-700 bg-green-50 dark:bg-green-900/20' 
                  : 'border-red-500 dark:border-red-700 bg-red-50 dark:bg-red-900/20'
              }`}>
                <CardHeader>
                  <CardTitle className={`flex items-center space-x-2 ${
                    importResult.exitoso ? 'text-green-700 dark:text-green-400' : 'text-red-700 dark:text-red-400'
                  }`}>
                    {importResult.exitoso ? (
                      <>
                        <CheckCircle2 className="w-6 h-6" />
                        <span>✅ Importación Exitosa</span>
                      </>
                    ) : (
                      <>
                        <XCircle className="w-6 h-6" />
                        <span>❌ Errores en Importación</span>
                      </>
                    )}
                  </CardTitle>
                  <CardDescription className={importResult.exitoso ? 'text-green-800 dark:text-green-300' : 'text-red-800 dark:text-red-300'}>
                    {importResult.mensaje}
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* RESUMEN SI ES EXITOSO */}
                  {importResult.exitoso && importResult.resumen && (
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                      <div className="bg-white dark:bg-[#1E293B] p-3 rounded-lg border border-green-200 dark:border-green-800">
                        <p className="text-xs text-gray-600 dark:text-gray-400">Registros Procesados</p>
                        <p className="text-2xl font-bold text-green-700 dark:text-green-400">
                          {importResult.resumen.registrosProcesados}
                        </p>
                      </div>
                      <div className="bg-white dark:bg-[#1E293B] p-3 rounded-lg border border-green-200 dark:border-green-800">
                        <p className="text-xs text-gray-600 dark:text-gray-400">Duración</p>
                        <p className="text-2xl font-bold text-green-700 dark:text-green-400">
                          {importResult.resumen.duracionSegundos.toFixed(2)}s
                        </p>
                      </div>
                      <div className="bg-white dark:bg-[#1E293B] p-3 rounded-lg border border-green-200 dark:border-green-800">
                        <p className="text-xs text-gray-600 dark:text-gray-400">Productos Insertados</p>
                        <p className="text-2xl font-bold text-green-700 dark:text-green-400">
                          {importResult.resumen.productosInsertados}
                        </p>
                      </div>
                      <div className="bg-white dark:bg-[#1E293B] p-3 rounded-lg border border-green-200 dark:border-green-800">
                        <p className="text-xs text-gray-600 dark:text-gray-400">Productos Actualizados</p>
                        <p className="text-2xl font-bold text-green-700 dark:text-green-400">
                          {importResult.resumen.productosActualizados}
                        </p>
                      </div>
                      <div className="bg-white dark:bg-[#1E293B] p-3 rounded-lg border border-green-200 dark:border-green-800">
                        <p className="text-xs text-gray-600 dark:text-gray-400">Ventas Insertadas</p>
                        <p className="text-2xl font-bold text-green-700 dark:text-green-400">
                          {importResult.resumen.ventasInsertadas}
                        </p>
                      </div>
                      <div className="bg-white dark:bg-[#1E293B] p-3 rounded-lg border border-green-200 dark:border-green-800">
                        <p className="text-xs text-gray-600 dark:text-gray-400">Inventario</p>
                        <p className="text-2xl font-bold text-green-700 dark:text-green-400">
                          {importResult.resumen.inventariosInsertados}
                        </p>
                      </div>
                    </div>
                  )}

                  {/* ERRORES SI FALLÓ */}
                  {!importResult.exitoso && importResult.errores && importResult.errores.length > 0 && (
                    <div className="space-y-3">
                      <h3 className="font-semibold text-red-700 dark:text-red-400 flex items-center space-x-2">
                        <AlertTriangle className="w-5 h-5" />
                        <span>Errores Encontrados ({importResult.errores.length}):</span>
                      </h3>
                      <div className="overflow-y-auto max-h-96 space-y-2">
                        {importResult.errores.map((error, idx) => (
                          <div
                            key={idx}
                            className="bg-white dark:bg-[#1E293B] p-4 rounded-lg border border-red-200 dark:border-red-800 text-sm"
                          >
                            <div className="flex justify-between items-start mb-2">
                              <span className="font-mono font-bold text-red-600 dark:text-red-400 text-xs bg-red-100 dark:bg-red-900/30 px-2 py-1 rounded">
                                Fila {error.fila} • Columna "{error.columna}"
                              </span>
                              {error.tipoDatoEsperado && (
                                <span className="text-xs text-gray-500 dark:text-gray-400">
                                  Tipo esperado: {error.tipoDatoEsperado}
                                </span>
                              )}
                            </div>
                            <p className="text-red-700 dark:text-red-300 mb-2">
                              <strong>Error:</strong> {error.error}
                            </p>
                            {error.valorEncontrado && (
                              <p className="text-gray-700 dark:text-gray-300 mb-2">
                                <strong>Valor encontrado:</strong> <code className="bg-gray-100 dark:bg-gray-800 px-1 py-0.5 rounded">{error.valorEncontrado}</code>
                              </p>
                            )}
                            {error.sugerencia && (
                              <p className="text-blue-700 dark:text-blue-400 mt-2 p-2 bg-blue-50 dark:bg-blue-900/20 rounded border-l-4 border-blue-500">
                                <strong>💡 Sugerencia:</strong> {error.sugerencia}
                              </p>
                            )}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* ADVERTENCIAS */}
                  {importResult.advertencias && importResult.advertencias.length > 0 && (
                    <div className="mt-4 p-4 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
                      <h3 className="font-semibold text-yellow-700 dark:text-yellow-400 mb-2 flex items-center space-x-2">
                        <AlertTriangle className="w-5 h-5" />
                        <span>⚠️ Advertencias ({importResult.advertencias.length}):</span>
                      </h3>
                      <ul className="list-disc list-inside text-sm text-yellow-700 dark:text-yellow-300 space-y-1">
                        {importResult.advertencias.map((adv, idx) => (
                          <li key={idx}>{adv}</li>
                        ))}
                      </ul>
                    </div>
                  )}

                  {/* BOTONES DE ACCIÓN */}
                  <div className="flex flex-wrap gap-2 mt-6 pt-4 border-t border-slate-200 dark:border-slate-700">
                    {importResult.exitoso && importResult.importId && (
                      <>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleVerDatosImportados(importResult.importId!)}
                          className="flex items-center space-x-2"
                        >
                          <Eye className="w-4 h-4" />
                          <span>Ver Datos Importados</span>
                        </Button>
                        <Button
                          variant="default"
                          size="sm"
                          onClick={resetUploadState}
                          className="flex items-center space-x-2 bg-green-600 hover:bg-green-700"
                        >
                          <Upload className="w-4 h-4" />
                          <span>Cargar Otro Archivo</span>
                        </Button>
                      </>
                    )}
                    {!importResult.exitoso && (
                      <Button
                        variant="default"
                        size="sm"
                        onClick={resetUploadState}
                        className="flex items-center space-x-2 bg-red-600 hover:bg-red-700"
                      >
                        <RotateCcw className="w-4 h-4" />
                        <span>Reintentar Carga</span>
                      </Button>
                    )}
                  </div>
                </CardContent>
              </Card>
            </motion.div>
          )}

          {/* Recent Imports */}
          <section>
            <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
              <CardHeader>
                <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Importaciones Recientes</CardTitle>
                <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">
                  Historial de tus últimas cargas de datos
                </CardDescription>
              </CardHeader>
              <CardContent>
                {recentImports.length === 0 ? (
                  <div className="text-center py-8 text-slate-500 dark:text-[#E5E7EB]">
                    <FileSpreadsheet className="w-12 h-12 mx-auto mb-3 opacity-50" />
                    <p>No hay importaciones recientes</p>
                    <p className="text-sm mt-1">Sube tu primer archivo Excel para comenzar</p>
                  </div>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-slate-200 dark:border-[#374151]">
                          <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Archivo</th>
                          <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Fecha</th>
                          <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Registros</th>
                          <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Estado</th>
                          <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Acciones</th>
                        </tr>
                      </thead>
                      <tbody>
                        {recentImports.map((item, index) => (
                          <motion.tr 
                            key={item.id || index} 
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ duration: 0.2, delay: index * 0.05 }}
                            className="border-b border-slate-100 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155] transition-colors duration-200"
                          >
                            <td className="py-3 px-4 font-medium text-slate-900 dark:text-[#F1F5F9]">
                              <div className="flex items-center space-x-2">
                                <FileSpreadsheet className="w-4 h-4 text-blue-600 dark:text-blue-400" />
                                <span className="truncate max-w-xs">{item.nombreArchivo}</span>
                              </div>
                            </td>
                            <td className="py-3 px-4 text-slate-600 dark:text-[#E5E7EB] text-sm">
                              {formatRelativeDate(item.fechaImportacion)}
                            </td>
                            <td className="py-3 px-4 text-slate-600 dark:text-[#E5E7EB]">
                              {item.registrosCargados || 0}
                            </td>
                            <td className="py-3 px-4">
                              {getEstadoBadge(item.estado)}
                            </td>
                            <td className="py-3 px-4">
                              <div className="flex space-x-2">
                                <Button 
                                  variant="ghost" 
                                  size="sm" 
                                  className="flex items-center space-x-1 hover:bg-slate-100 dark:hover:bg-[#334155] text-slate-700 dark:text-[#E5E7EB] transition-colors duration-200"
                                  onClick={() => handleVerDatosImportados(item.id)}
                                >
                                  <Eye className="w-4 h-4" />
                                  <span>Ver Datos</span>
                                </Button>
                              </div>
                            </td>
                          </motion.tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>
          </section>
        </div>
      )}
    </div>
  );
}