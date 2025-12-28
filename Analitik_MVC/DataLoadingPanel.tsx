import { useState, useEffect } from 'react';
import { motion } from 'motion/react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Progress } from './ui/progress';
import { Skeleton } from './ui/skeleton';
import { FileSpreadsheet, Database, Upload, FolderOpen, Eye, RotateCcw, AlertTriangle } from 'lucide-react';

export function DataLoadingPanel() {
  const [isLoading, setIsLoading] = useState(true);
  const [dragActive, setDragActive] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);
  const [dbConnection, setDbConnection] = useState({
    host: '',
    port: '5432',
    database: '',
    user: '',
    password: ''
  });
  const [isConnecting, setIsConnecting] = useState(false);

  // Simulate initial data loading
  useEffect(() => {
    const timer = setTimeout(() => {
      setIsLoading(false);
    }, 1500);
    return () => clearTimeout(timer);
  }, []);

  const recentImports = [
    { name: 'sales_data_2024.xlsx', type: 'Excel', size: '2.3 MB', date: 'Hace 2 horas', status: 'Success' },
    { name: 'customer_analytics.csv', type: 'CSV', size: '1.8 MB', date: 'Hace 1 día', status: 'Success' },
    { name: 'product_inventory.xlsx', type: 'Excel', size: '945 KB', date: 'Hace 2 días', status: 'Failed' },
    { name: 'monthly_reports.csv', type: 'CSV', size: '3.2 MB', date: 'Hace 3 días', status: 'Success' },
    { name: 'user_engagement.xlsx', type: 'Excel', size: '1.5 MB', date: 'Hace 1 semana', status: 'Success' }
  ];

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

  const handleFileUpload = async (file: File) => {
    setIsUploading(true);
    setUploadProgress(0);
    
    // Simulate upload progress
    const interval = setInterval(() => {
      setUploadProgress(prev => {
        if (prev >= 100) {
          clearInterval(interval);
          setIsUploading(false);
          return 100;
        }
        return prev + 10;
      });
    }, 200);
  };

  const handleDbConnect = async () => {
    setIsConnecting(true);
    // Simulate connection
    await new Promise(resolve => setTimeout(resolve, 2000));
    setIsConnecting(false);
  };

  const handleInputChange = (field: string, value: string) => {
    setDbConnection(prev => ({ ...prev, [field]: value }));
  };

  return (
    <div className="min-h-full bg-slate-50 dark:bg-[#111827] transition-colors duration-200">
      {/* Header */}
      <div className="bg-white dark:bg-[#1E293B] border-b border-slate-200 dark:border-[#374151] px-4 py-4 sm:px-6 lg:px-8 transition-colors duration-200">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-[#F1F5F9] break-words">Importa tus datos</h1>
          <p className="text-sm sm:text-base text-slate-600 dark:text-[#E5E7EB] mt-1">Sube archivos o conéctate a tu base de datos</p>
        </div>
      </div>

      {isLoading ? (
        <div className="p-4 sm:p-6 lg:p-8 space-y-6 sm:space-y-8">
          {/* Skeleton for Import Options */}
          <section className="grid grid-cols-1 lg:grid-cols-2 gap-6 sm:gap-8">
            {/* File Upload Card Skeleton */}
            <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
              <CardHeader>
                <div className="flex items-center space-x-3 mb-2">
                  <Skeleton className="w-6 h-6 rounded" />
                  <Skeleton className="h-6 w-48" />
                </div>
                <Skeleton className="h-4 w-64" />
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Drag and Drop Area Skeleton */}
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

                {/* Supported Formats Skeleton */}
                <div className="text-center space-y-2">
                  <Skeleton className="h-4 w-64 mx-auto" />
                  <Skeleton className="h-4 w-48 mx-auto" />
                </div>
              </CardContent>
            </Card>

            {/* Database Connection Card Skeleton */}
            <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
              <CardHeader>
                <div className="flex items-center space-x-3 mb-2">
                  <Skeleton className="w-6 h-6 rounded" />
                  <Skeleton className="h-6 w-56" />
                </div>
                <Skeleton className="h-4 w-72" />
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Connection Form Fields Skeleton */}
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Skeleton className="h-4 w-12" />
                      <Skeleton className="h-10 w-full rounded-md" />
                    </div>
                    <div className="space-y-2">
                      <Skeleton className="h-4 w-10" />
                      <Skeleton className="h-10 w-full rounded-md" />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-28" />
                    <Skeleton className="h-10 w-full rounded-md" />
                  </div>
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-10 w-full rounded-md" />
                  </div>
                  <div className="space-y-2">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-10 w-full rounded-md" />
                  </div>
                </div>
                <Skeleton className="h-10 w-full rounded-md" />
                <div className="text-center">
                  <Skeleton className="h-4 w-80 mx-auto" />
                </div>
              </CardContent>
            </Card>
          </section>

          {/* Recent Imports Table Skeleton */}
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
                        <th className="text-left py-3 px-4">
                          <Skeleton className="h-4 w-20" />
                        </th>
                        <th className="text-left py-3 px-4">
                          <Skeleton className="h-4 w-16" />
                        </th>
                        <th className="text-left py-3 px-4">
                          <Skeleton className="h-4 w-12" />
                        </th>
                        <th className="text-left py-3 px-4">
                          <Skeleton className="h-4 w-20" />
                        </th>
                        <th className="text-left py-3 px-4">
                          <Skeleton className="h-4 w-16" />
                        </th>
                        <th className="text-left py-3 px-4">
                          <Skeleton className="h-4 w-20" />
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {Array.from({ length: 5 }).map((_, index) => (
                        <tr key={index} className="border-b border-slate-100 dark:border-[#374151]">
                          <td className="py-3 px-4">
                            <Skeleton className="h-4 w-40" />
                          </td>
                          <td className="py-3 px-4">
                            <Skeleton className="h-6 w-16 rounded-full" />
                          </td>
                          <td className="py-3 px-4">
                            <Skeleton className="h-4 w-16" />
                          </td>
                          <td className="py-3 px-4">
                            <Skeleton className="h-4 w-24" />
                          </td>
                          <td className="py-3 px-4">
                            <Skeleton className="h-6 w-20 rounded-full" />
                          </td>
                          <td className="py-3 px-4">
                            <div className="flex space-x-2">
                              <Skeleton className="h-8 w-16 rounded-md" />
                              <Skeleton className="h-8 w-20 rounded-md" />
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
        <div className="p-6 lg:p-8 space-y-8">
        {/* Import Options */}
        <section className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Excel/CSV Upload */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
              <CardHeader>
                <CardTitle className="flex items-center space-x-3 text-slate-900 dark:text-[#F1F5F9]">
                  <FileSpreadsheet className="w-6 h-6 text-blue-600 dark:text-[#60A5FA]" />
                  <span>Importar desde Excel/CSV</span>
                </CardTitle>
                <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">
                  Sube archivos de hojas de cálculo para importar tus datos rápidamente
                </CardDescription>
              </CardHeader>
            <CardContent className="space-y-6">
              {/* Drag & Drop Zone */}
              <div
                className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
                  dragActive 
                    ? 'border-blue-500 dark:border-[#60A5FA] bg-blue-50 dark:bg-[#334155]' 
                    : 'border-slate-300 dark:border-[#374151] hover:border-slate-400 dark:hover:border-[#60A5FA]'
                }`}
                onDragEnter={handleDrag}
                onDragLeave={handleDrag}
                onDragOver={handleDrag}
                onDrop={handleDrop}
              >
                <div className="space-y-4">
                  <FolderOpen className="w-16 h-16 text-blue-600 dark:text-[#60A5FA] mx-auto" />
                  <div>
                    <p className="text-lg font-medium text-slate-900 dark:text-[#F1F5F9]">
                      {dragActive ? 'Suelta tu archivo aquí' : 'Arrastra y suelta tu archivo aquí'}
                    </p>
                    <p className="text-slate-500 dark:text-[#E5E7EB] mt-1">o haz clic para seleccionar un archivo</p>
                  </div>
                  <Button 
                    variant="outline"
                    className="border-slate-300 dark:border-[#374151] text-slate-700 dark:text-[#E5E7EB] hover:bg-slate-50 dark:hover:bg-[#334155]"
                    onClick={() => document.getElementById('fileInput')?.click()}
                  >
                    Seleccionar Archivo
                  </Button>
                  <input
                    id="fileInput"
                    type="file"
                    accept=".xlsx,.xls,.csv"
                    onChange={(e) => e.target.files && handleFileUpload(e.target.files[0])}
                    className="hidden"
                  />
                </div>
              </div>

              {/* Upload Progress */}
              {isUploading && (
                <motion.div 
                  className="space-y-2"
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                  transition={{ duration: 0.2 }}
                >
                  <div className="flex justify-between text-sm text-slate-700 dark:text-[#E5E7EB]">
                    <span>Subiendo...</span>
                    <span>{uploadProgress}%</span>
                  </div>
                  <Progress value={uploadProgress} className="w-full" />
                </motion.div>
              )}

              {/* Supported Formats */}
              <div className="text-center text-sm text-slate-500 dark:text-[#E5E7EB]">
                <p>Formatos soportados: .xlsx, .xls, .csv</p>
                <p>Tamaño máximo de archivo: 10MB</p>
              </div>
            </CardContent>
            </Card>
          </motion.div>

          {/* PostgreSQL Connection */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3, delay: 0.1 }}
          >
            <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
              <CardHeader>
                <CardTitle className="flex items-center space-x-3 text-slate-900 dark:text-[#F1F5F9]">
                  <Database className="w-6 h-6 text-blue-600 dark:text-[#60A5FA]" />
                  <span>Importar desde PostgreSQL</span>
                </CardTitle>
                <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">
                  Conéctate a tu base de datos PostgreSQL para sincronización de datos en tiempo real
                </CardDescription>
              </CardHeader>
            <CardContent className="space-y-6">
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="host" className="text-slate-700 dark:text-[#E5E7EB]">Servidor</Label>
                    <Input
                      id="host"
                      placeholder="localhost"
                      value={dbConnection.host}
                      onChange={(e) => handleInputChange('host', e.target.value)}
                      className="bg-white dark:bg-[#1E293B] border-slate-300 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9] placeholder:text-slate-400 dark:placeholder:text-[#94A3B8]"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="port" className="text-slate-700 dark:text-[#E5E7EB]">Puerto</Label>
                    <Input
                      id="port"
                      placeholder="5432"
                      value={dbConnection.port}
                      onChange={(e) => handleInputChange('port', e.target.value)}
                      className="bg-white dark:bg-[#1E293B] border-slate-300 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9] placeholder:text-slate-400 dark:placeholder:text-[#94A3B8]"
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="database" className="text-slate-700 dark:text-[#E5E7EB]">Nombre de Base de Datos</Label>
                  <Input
                    id="database"
                    placeholder="your_database"
                    value={dbConnection.database}
                    onChange={(e) => handleInputChange('database', e.target.value)}
                    className="bg-white dark:bg-[#1E293B] border-slate-300 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9] placeholder:text-slate-400 dark:placeholder:text-[#94A3B8]"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="user" className="text-slate-700 dark:text-[#E5E7EB]">Usuario</Label>
                  <Input
                    id="user"
                    placeholder="username"
                    value={dbConnection.user}
                    onChange={(e) => handleInputChange('user', e.target.value)}
                    className="bg-white dark:bg-[#1E293B] border-slate-300 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9] placeholder:text-slate-400 dark:placeholder:text-[#94A3B8]"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="password" className="text-slate-700 dark:text-[#E5E7EB]">Contraseña</Label>
                  <Input
                    id="password"
                    type="password"
                    placeholder="contraseña"
                    value={dbConnection.password}
                    onChange={(e) => handleInputChange('password', e.target.value)}
                    className="bg-white dark:bg-[#1E293B] border-slate-300 dark:border-[#374151] text-slate-900 dark:text-[#F1F5F9] placeholder:text-slate-400 dark:placeholder:text-[#94A3B8]"
                  />
                </div>
              </div>

              <Button 
                className="w-full bg-blue-600 hover:bg-blue-700 dark:bg-[#2563EB] dark:hover:bg-[#1D4ED8] text-white transition-colors duration-200"
                onClick={handleDbConnect}
                disabled={isConnecting || !dbConnection.host || !dbConnection.database}
              >
                {isConnecting ? (
                  <span className="flex items-center justify-center space-x-2">
                    <span className="flex space-x-1">
                      <span className="w-2 h-2 bg-white rounded-full animate-bounce"></span>
                      <span className="w-2 h-2 bg-white rounded-full animate-bounce" style={{ animationDelay: '0.1s' }}></span>
                      <span className="w-2 h-2 bg-white rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></span>
                    </span>
                    <span>Conectando...</span>
                  </span>
                ) : (
                  'Conectar y Sincronizar'
                )}
              </Button>

              <div className="text-center text-sm text-slate-500 dark:text-[#E5E7EB]">
                <div className="flex items-center justify-center space-x-2">
                  <AlertTriangle className="w-4 h-4 text-amber-600 dark:text-[#FACC15]" />
                  <p className="text-amber-700 dark:text-[#FACC15]">Asegúrate de que tu base de datos permita conexiones externas</p>
                </div>
              </div>
            </CardContent>
            </Card>
          </motion.div>
        </section>

        {/* Recent Imports */}
        <section>
          <Card className="bg-white dark:bg-[#273043] border-slate-200 dark:border-[#374151] transition-colors duration-200">
            <CardHeader>
              <CardTitle className="text-slate-900 dark:text-[#F1F5F9]">Importaciones de Datos Recientes</CardTitle>
              <CardDescription className="text-slate-600 dark:text-[#E5E7EB]">Historial de tus cargas de datos y conexiones recientes</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-slate-200 dark:border-[#374151]">
                      <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Nombre de Archivo</th>
                      <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Tipo</th>
                      <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Tamaño</th>
                      <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Importado</th>
                      <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Estado</th>
                      <th className="text-left py-3 px-4 font-medium text-slate-600 dark:text-[#E5E7EB]">Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {recentImports.map((item, index) => (
                      <motion.tr 
                        key={index} 
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.2, delay: index * 0.05 }}
                        className="border-b border-slate-100 dark:border-[#374151] hover:bg-slate-50 dark:hover:bg-[#334155] transition-colors duration-200"
                      >
                        <td className="py-3 px-4 font-medium text-slate-900 dark:text-[#F1F5F9]">{item.name}</td>
                        <td className="py-3 px-4">
                          <Badge variant="outline" className="border-slate-300 dark:border-[#374151] text-slate-700 dark:text-[#E5E7EB]">{item.type}</Badge>
                        </td>
                        <td className="py-3 px-4 text-slate-600 dark:text-[#E5E7EB]">{item.size}</td>
                        <td className="py-3 px-4 text-slate-500 dark:text-[#E5E7EB] text-sm">{item.date}</td>
                        <td className="py-3 px-4">
                          <Badge 
                            variant={item.status === 'Success' ? 'default' : 'destructive'}
                            className={item.status === 'Success' ? 'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-400 hover:bg-green-100 dark:hover:bg-green-900/30 border-green-200 dark:border-green-800' : 'dark:bg-red-900/30 dark:text-red-400 dark:border-red-800'}
                          >
                            {item.status === 'Success' ? 'Exitoso' : 'Fallido'}
                          </Badge>
                        </td>
                        <td className="py-3 px-4">
                          <div className="flex space-x-2">
                            <Button variant="ghost" size="sm" className="flex items-center space-x-1 hover:bg-slate-100 dark:hover:bg-[#334155] text-slate-700 dark:text-[#E5E7EB] transition-colors duration-200">
                              <Eye className="w-4 h-4" />
                              <span>Ver</span>
                            </Button>
                            <Button variant="ghost" size="sm" className="flex items-center space-x-1 hover:bg-slate-100 dark:hover:bg-[#334155] text-slate-700 dark:text-[#E5E7EB] transition-colors duration-200">
                              <RotateCcw className="w-4 h-4" />
                              <span>Reimportar</span>
                            </Button>
                          </div>
                        </td>
                      </motion.tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </section>
        </div>
      )}
    </div>
  );
}