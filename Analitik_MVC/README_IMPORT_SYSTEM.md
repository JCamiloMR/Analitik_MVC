# Sistema ETL de Importación de Datos - ClariData

## ?? Resumen

Sistema completo de **Extracción, Transformación y Carga (ETL)** para importar datos desde archivos Excel a la base de datos PostgreSQL. Implementa validación en 3 niveles, transacciones atómicas y logging completo.

## ??? Arquitectura Implementada

### Componentes Principales



## ?? Estructura de Archivos Creados

### DTOs (Data Transfer Objects)
```
Analitik_MVC/DTOs/Import/
??? ProductoDTO.cs              - Representa producto de Excel
??? InventarioDTO.cs            - Representa inventario de Excel
??? VentaDTO.cs                 - Representa venta de Excel
??? DetalleVentaDTO.cs          - Representa detalle de venta
??? FinancieroDTO.cs            - Representa dato financiero
??? ErrorValidacion.cs          - Estructura de error de validación
??? ValidationResult.cs         - Resultado de validación
??? ImportResultDTO.cs          - Resultado completo de importación
??? ImportReportDTO.cs          - Reporte detallado de importación
```

### Services
```
Analitik_MVC/Services/
??? Data/
?   ??? DataTransformationService.cs   - Parsers (fecha, moneda, booleano)
??? Excel/
?   ??? ExcelValidationService.cs      - Validación de archivo y estructura
?   ??? ExcelReaderService.cs          - Lectura y mapeo de hojas Excel
??? Database/
    ??? DatabaseLoaderService.cs       - Carga atómica con transacciones
    ??? ImportLogService.cs            - Registro de logs de importación
```

### Controllers
```
Analitik_MVC/Controllers/
??? ImportController.cs                - Endpoints de API
```

## ?? API Endpoints

### 1. POST /api/import/excel
**Importa archivo Excel completo**

**Request:**
```http
POST /api/import/excel
Content-Type: multipart/form-data

archivo: [archivo.xlsx]
empresaId: "uuid-de-empresa"
```

**Response (Éxito):**
```json
{
  "exitoso": true,
  "mensaje": "? Carga exitosa: 250 registros importados en 3.45 segundos",
  "importId": "uuid-de-importacion",
  "advertencias": [
    "Fila 5: Categoría 'Deportivo' no reconocida"
  ],
  "resumen": {
    "registrosProcesados": 250,
    "productosInsertados": 100,
    "productosActualizados": 10,
    "inventariosInsertados": 85,
    "ventasInsertadas": 50,
    "financierosInsertados": 30,
    "duracionSegundos": 3.45
  }
}
```

**Response (Error):**
```json
{
  "exitoso": false,
  "mensaje": "Errores en hoja PRODUCTOS. Corrígelos y vuelve a intentar.",
  "importId": "uuid-de-importacion",
  "errores": [
    {
      "fila": 5,
      "columna": "precio_venta",
      "error": "Debe ser mayor a 0",
      "valorEncontrado": "-150",
      "sugerencia": "Ingresa un precio positivo (ej: 89500)",
      "tipoDatoEsperado": "decimal"
    }
  ]
}
```

### 2. GET /api/import/status/{importId}
**Consulta estado de una importación**

**Response:**
```json
{
  "id": "uuid-de-importacion",
  "estado": "completado",
  "faseActual": "completado",
  "progresoPorcentaje": 100,
  "registrosCargados": 250,
  "registrosRechazados": 0,
  "fechaImportacion": "2025-01-15T10:30:00Z",
  "duracionSegundos": 3
}
```

### 3. GET /api/import/report/{importId}
**Descarga reporte detallado de importación**

### 4. GET /api/import/history?empresaId={id}&pagina=1&tamano=20
**Lista historial de importaciones**

## ? Validaciones Implementadas

### Nivel 1: Archivo
- ? Extensión `.xlsx` obligatoria
- ? Tamaño máximo 10 MB
- ? Archivo Excel válido (no corrupto)

### Nivel 2: Estructura
- ? 4 hojas obligatorias: PRODUCTOS, INVENTARIO, VENTAS, FINANCIEROS
- ? Columnas obligatorias presentes en cada hoja
- ? Hojas no vacías (al menos una fila de datos)

### Nivel 3: Datos (50+ validaciones)

#### PRODUCTOS (18 columnas)
- Código único, alfanumérico, no inicia con número
- Precio > 0, costo <= precio
- Unidad de medida en lista permitida
- Lógica: si `es_servicio=true` ? `requiere_inventario=false`
- Categorías validadas (con advertencia si no reconocida)

#### INVENTARIO (14 columnas)
- Código debe existir en PRODUCTOS
- Cantidad >= 0
- Cantidad reservada <= cantidad disponible
- Stock máximo >= stock mínimo
- Fecha vencimiento no puede ser pasada

#### VENTAS (18+ columnas)
- Número de orden único
- Fecha no futura
- Método de pago en lista permitida
- Montos coherentes: `total = subtotal - descuento + impuestos`
- Validación de email si se proporciona
- Productos en detalles deben existir

#### FINANCIEROS (11 columnas)
- Tipo en lista: ingreso, gasto, costo, inversión
- Categoría válida según tipo
- Monto > 0
- Fecha no futura

## ?? Flujo de Procesamiento

```
1. Usuario sube archivo desde React component
   ?
2. Validación de archivo (extensión, tamaño)
   ? ? Si falla ? RECHAZAR TODO
3. Validación de estructura (hojas, columnas)
   ? ? Si falla ? RECHAZAR TODO
4. Lectura y mapeo de PRODUCTOS
   ? ? Si falla ? RECHAZAR TODO
5. Lectura y mapeo de INVENTARIO (valida refs a PRODUCTOS)
   ? ? Si falla ? RECHAZAR TODO
6. Lectura y mapeo de VENTAS
   ? ? Si falla ? RECHAZAR TODO
7. Lectura y mapeo de FINANCIEROS
   ? ? Si falla ? RECHAZAR TODO
8. TRANSACCIÓN ATÓMICA (SERIALIZABLE)
   ?? Upsert PRODUCTOS (insert o update si existe)
   ?? Insert INVENTARIO
   ?? Insert VENTAS + detalles
   ?? Insert FINANCIEROS
   ?? COMMIT (todo) o ROLLBACK (nada)
   ? ? Si falla ? ROLLBACK + Log de error
9. Registro en tabla importaciones_datos
   ?
10. Respuesta al usuario con resumen
```

## ??? Configuración e Instalación

### 1. Paquetes NuGet Agregados
```xml
<PackageReference Include="ClosedXML" Version="0.104.2" />
```

### 2. Servicios Registrados en Program.cs
```csharp
builder.Services.AddScoped<ExcelValidationService>();
builder.Services.AddScoped<ExcelReaderService>();
builder.Services.AddScoped<DataTransformationService>();
builder.Services.AddScoped<DatabaseLoaderService>();
builder.Services.AddScoped<ImportLogService>();
```

### 3. Restaurar Paquetes
```bash
dotnet restore
```

### 4. Compilar
```bash
dotnet build
```

## ?? Parsers Implementados

### ParseExcelDate(object valor)
Soporta múltiples formatos:
- `2025-01-15` (ISO - preferido)
- `15/01/2025` (Colombia DD/MM/YYYY)
- `01/15/2025` (USA MM/DD/YYYY)
- Parse automático de Excel

### ParseCurrency(object valor)
Maneja:
- Símbolos: `$`, `COP`, `USD`, `EUR`
- Separadores: `1,234.56` o `1.234,56`
- Redondeo automático a 2 decimales

### ParseBoolean(object valor)
Reconoce:
- **True:** VERDADERO, TRUE, V, SÍ, SI, S, YES, Y, 1, ACTIVO
- **False:** FALSO, FALSE, F, NO, N, 0, INACTIVO

### NormalizeText(string texto)
- Trim de espacios
- Remover espacios múltiples
- Eliminar caracteres de control

### ValidateCodigo(string codigo)
- Solo alfanumérico
- No inicia con número
- Permite guiones y guiones bajos

## ?? Seguridad y Transacciones

### Transacción SERIALIZABLE
```csharp
using var transaction = await _dbContext.Database
    .BeginTransactionAsync(IsolationLevel.Serializable);

try {
    // Todas las operaciones
    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync(); // Todo o nada
}
catch {
    await transaction.RollbackAsync(); // Rollback completo
    throw;
}
```

### Características
- ? **Atomicidad:** Todo o nada
- ? **Consistencia:** Validaciones antes de BD
- ? **Aislamiento:** SERIALIZABLE (máximo nivel)
- ? **Durabilidad:** Logs en BD

## ?? Logs y Auditoría

### Tabla: importaciones_datos
Registra:
- ID de importación
- Empresa
- Nombre de archivo
- Hash SHA-256 (detección de duplicados)
- Estado: en_proceso, completado, fallido
- Fase: extraccion, transformacion, carga, error
- Registros procesados/cargados/rechazados
- Errores (JSON detallado)
- Advertencias
- Duración en segundos

### Consulta de Logs
```csharp
// Por ID
var reporte = await _importLogService.GenerarReporteAsync(importacionId);

// Historial
GET /api/import/history?empresaId={id}&pagina=1
```

## ?? Casos de Uso

### 1. Carga mensual de datos
Usuario descarga plantilla, llena datos del mes y los carga.

### 2. Actualización de productos
Si un código ya existe, se actualiza (upsert automático).

### 3. Migración inicial
Carga masiva de datos históricos en una sola transacción.

### 4. Integración con sistemas externos
API puede consumirse desde otras aplicaciones.

## ?? Manejo de Errores

### Tipos de Error

#### 1. Error de Archivo
```json
{
  "fila": 0,
  "columna": "Archivo",
  "error": "El archivo no es Excel (.xlsx)",
  "sugerencia": "Descarga la plantilla oficial"
}
```

#### 2. Error de Estructura
```json
{
  "fila": 0,
  "columna": "PRODUCTOS",
  "error": "Falta la hoja PRODUCTOS",
  "sugerencia": "Usa la plantilla oficial"
}
```

#### 3. Error de Datos
```json
{
  "fila": 5,
  "columna": "precio_venta",
  "error": "Debe ser mayor a 0",
  "valorEncontrado": "-150",
  "sugerencia": "Ingresa un precio positivo",
  "tipoDatoEsperado": "decimal"
}
```

### Estrategia de Manejo
- **Errores bloqueantes:** Detienen el proceso, rollback completo
- **Advertencias:** Permiten continuar pero se registran en logs

## ?? Performance

### Tamaños Recomendados
| Tamaño | Registros | Tiempo Estimado |
|--------|-----------|-----------------|
| 1 MB   | ~500      | 3-5 segundos    |
| 5 MB   | ~2,500    | 15-20 segundos  |
| 10 MB  | ~5,000    | 30-45 segundos  |

### Optimizaciones Implementadas
- Transacciones en lote (no registro por registro)
- Upsert eficiente con diccionarios
- Parseo en memoria (no I/O por fila)

## ?? Testing Recomendado

### Test Unitarios
```csharp
[Fact]
public void ParseCurrency_ConFormatoColombiano_DebeConvertir()
{
    var service = new DataTransformationService();
    var resultado = service.ParseCurrency("$1.234.567,50");
    Assert.Equal(1234567.50m, resultado);
}
```

### Test de Integración
```csharp
[Fact]
public async Task ImportarExcel_ConArchivoValido_DebeCargar()
{
    // Arrange
    var archivo = CrearArchivoExcelPrueba();
    
    // Act
    var resultado = await _controller.ImportarExcel(archivo, empresaId);
    
    // Assert
    Assert.True(resultado.Exitoso);
}
```

## ?? Documentación Relacionada

- [ClariData-Plantilla-Excel.md](./ClariData-Plantilla-Excel.md) - Especificación de plantilla
- [ClariData-Pseudocodigo-ETL.md](./ClariData-Pseudocodigo-ETL.md) - Algoritmos detallados
- [ClariData-Decisiones-Diseno.md](./ClariData-Decisiones-Diseno.md) - Decisiones arquitectónicas
- [ClariData-Quick-Reference.md](./ClariData-Quick-Reference.md) - Referencia rápida

## ?? Próximos Pasos (Fase 2.5)

1. **Validaciones Avanzadas**
   - Margen mínimo de ganancia
   - Stock de seguridad
   - Límites por método de pago

2. **Imputación de Datos**
   - Auto-crear clientes si no existen
   - Categorías por defecto
   - Códigos auto-generados

3. **Reportes Descargables**
   - Excel con errores marcados
   - PDF de resumen de carga

4. **Caché de Validaciones**
   - Redis para listas permitidas
   - Performance 10x más rápido

5. **API Programada**
   - Cargas automáticas (CRON)
   - SFTP integration

## ? Preguntas Frecuentes

### ¿Puedo cargar solo una hoja?
No, las 4 hojas son obligatorias. Puedes dejar hojas con solo encabezados si no tienes datos.

### ¿Qué pasa si un producto ya existe?
Se actualiza automáticamente (upsert). Los cambios se pueden rastrear en logs.

### ¿Puedo cargar más de 10 MB?
No actualmente. Divide tu archivo en múltiples cargas mensuales.

### ¿Los errores detallan qué corregir?
Sí, cada error incluye: fila, columna, valor encontrado, sugerencia.

### ¿Se puede recuperar de un error?
Sí, la transacción hace rollback completo. Corriges y vuelves a intentar.

## ?? Soporte

Para problemas o dudas:
- Revisa los logs en `importaciones_datos`
- Usa `GET /api/import/report/{id}` para ver detalles
- Consulta la documentación técnica en archivos `.md`

---

**Implementado por:** GitHub Copilot  
**Fecha:** Enero 2025  
**Estado:** ? Compilación exitosa, listo para testing  
**Versión:** 1.0.0
