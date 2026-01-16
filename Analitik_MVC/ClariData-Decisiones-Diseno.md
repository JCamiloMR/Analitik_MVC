# DOCUMENTO DE DECISIONES DE DISEÑO - CLARIDATA

**Documento:** Decisiones Arquitectónicas + Recomendaciones  
**Fecha:** Enero 2025  
**Audiencia:** Equipo técnico (C#, Frontend, DevOps)  
**Versión:** 1.0  

---

## 📊 MATRIZ DE DECISIONES

### **D1: Validación Estricta (Rechazo Total vs. Parcial)**

**Decisión:** ✅ RECHAZO TOTAL (Modo Estricto)

**Razón:**
- Los usuarios de PyMEs necesitan certeza: "Mis datos están 100% correctos o no entran"
- Evita inconsistencias parciales que generan confusión en dashboards
- Facilita debugging: el usuario ve exactamente qué está mal
- Proporciona garantía: si se carga, es porque TODO es válido

**Alternativa rechazada:**
- Carga parcial (errores no bloqueantes): Genera datos inconsistentes y confusión

**Impacto:**
- UX: Mensajes de error explícitos y detallados (fila + columna + valor + solución)
- BD: No requiere tabla de "registros rechazados"
- BI: Dashboards siempre confiables

**Implementación en C#:**
```csharp
// Modelo de validación
public class ValidationResult {
    public bool IsSuccess { get; set; }
    public List<ValidationError> Errors { get; set; }
    public List<string> Warnings { get; set; }
}

// Si hay errores: no proceder
if (!validationResult.IsSuccess) {
    return BadRequest(new {
        mensaje = "Carga rechazada: hay errores que deben corregirse",
        errores = validationResult.Errors,
        instrucciones = "Revisa cada error en la columna especificada y reinténtalo"
    });
}
```

---

### **D2: Orden de Procesamiento Obligatorio**

**Decisión:** ✅ SECUENCIAL (PRODUCTOS → INVENTARIO → VENTAS → FINANCIEROS)

**Razón:**
- **Integridad referencial:** Productos primero (son maestros)
- **Dependencias:** Inventario depende de Productos; Ventas de ambos
- **Debugging:** Si hay error, sabes exactamente en qué fase
- **Transacción:** Más fácil manejar rollback en orden inverso

**Diagrama de Dependencias:**
```
        PRODUCTOS (Maestro)
           ↙      ↘
    INVENTARIO   VENTAS ← Depende de PRODUCTOS
           ↓         ↓
           ← RELACIONES →
    
    FINANCIEROS (Independiente)
    (Solo valida su propia estructura)
```

**Implementación:**
```csharp
// Orquestador en C#
public async Task<ImportResultDto> ProcessExcelImportAsync(
    Stream excelFile, 
    Guid empresaId)
{
    using (var transaction = await _db.Database.BeginTransactionAsync()) {
        try {
            // Fase 1: Productos
            var productosResult = await ValidateAndLoadProductos(excelFile, empresaId);
            if (!productosResult.IsSuccess) 
                throw new ImportException("Error en PRODUCTOS");

            // Fase 2: Inventario
            var inventarioResult = await ValidateAndLoadInventario(
                excelFile, 
                productosResult.LoadedProducts, 
                empresaId);
            if (!inventarioResult.IsSuccess) 
                throw new ImportException("Error en INVENTARIO");

            // Fase 3: Ventas
            var ventasResult = await ValidateAndLoadVentas(
                excelFile, 
                productosResult.LoadedProducts, 
                empresaId);
            if (!ventasResult.IsSuccess) 
                throw new ImportException("Error en VENTAS");

            // Fase 4: Financieros
            var financierosResult = await ValidateAndLoadFinancieros(excelFile, empresaId);
            if (!financierosResult.IsSuccess) 
                throw new ImportException("Error en FINANCIEROS");

            // Commit si todo OK
            await transaction.CommitAsync();
            return new ImportResultDto { Success = true, ... };
        }
        catch (Exception ex) {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

---

### **D3: Transacciones Atómicas (Serializable)**

**Decisión:** ✅ AISLAMIENTO SERIALIZABLE

**Razón:**
- Una carga es "todo o nada" → Serializable es necesario
- Evita race conditions con lecturas concurrentes de dashboards
- PostgreSQL lo soporta eficientemente
- El tamaño típico (< 10 MB = ~1000 registros) no genera problemas de deadlock

**Alternativa rechazada:**
- RepeatableRead: Permite anomalías de fantasma en datos históricos
- ReadCommitted: Puede causar inconsistencias en relaciones

**Nivel de Aislamiento Actual:**
```sql
-- En PostgreSQL (Entity Framework)
BEGIN ISOLATION LEVEL SERIALIZABLE;
  -- Inserciones
COMMIT;
```

**En C#:**
```csharp
using (var transaction = _db.Database.BeginTransaction(
    System.Data.IsolationLevel.Serializable))
{
    // Todo o nada
    await _db.SaveChangesAsync();
    await transaction.CommitAsync();
}
```

---

### **D4: Manejo de Duplicados**

**Decisión:** ✅ DETECTAR + ACTUALIZAR (Upsert)

**Razón:**
- El usuario carga mensualmente (Enero, Febrero, etc.)
- Si el código de producto YA existe → actualizar (no duplicar)
- Mantiene histórico de cambios sin redundancia
- Expected behavior en sistemas de BI

**Tabla para tracking:**
```sql
CREATE TABLE audit_cambios_productos (
    id UUID PRIMARY KEY,
    producto_id UUID REFERENCES productos(id),
    campo_modificado VARCHAR(100),
    valor_anterior TEXT,
    valor_nuevo TEXT,
    fecha_cambio TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    fecha_carga_id UUID REFERENCES importaciones_datos(id)
);
```

**Implementación en C#:**
```csharp
// Helper: Upsert de productos
private async Task UpsertProducto(ProductoDTO dto, Guid empresaId) {
    var existente = await _db.Productos.FirstOrDefaultAsync(p => 
        p.EmpresaId == empresaId && 
        p.CodigoProducto == dto.codigo_producto);

    if (existente != null) {
        // Registrar cambios en audit
        foreach (var prop in typeof(Producto).GetProperties()) {
            var oldValue = prop.GetValue(existente);
            var newValue = prop.GetValue(dto);
            
            if (!Equals(oldValue, newValue)) {
                _db.AuditCambios.Add(new AuditCambio {
                    ProductoId = existente.Id,
                    CampoModificado = prop.Name,
                    ValorAnterior = oldValue?.ToString(),
                    ValorNuevo = newValue?.ToString()
                });
            }
        }
        
        // Actualizar
        existente.Nombre = dto.nombre;
        existente.PrecioVenta = dto.precio_venta;
        // ... resto de campos
    } else {
        // Insertar nuevo
        var nuevo = new Producto { ... };
        _db.Productos.Add(nuevo);
    }
    
    await _db.SaveChangesAsync();
}
```

---

### **D5: Normalización de Fechas**

**Decisión:** ✅ FORMATO ISO (YYYY-MM-DD) como estándar, pero aceptar múltiples formatos

**Razón:**
- ISO es estándar internacional para datos
- Excel puede enviar en múltiples formatos según locale
- Código maneja: ISO, DD/MM/YYYY, MM/DD/YYYY automáticamente
- PostgreSQL siempre almacena en ISO

**Formatos Aceptados (en orden de intento):**
1. `2025-01-15` (ISO - preferido)
2. `15/01/2025` (Colombia DD/MM/YYYY)
3. `01/15/2025` (USA MM/DD/YYYY)
4. Excel OLE Automation date → parsed automáticamente

**Implementación:**
```csharp
public static DateTime ParseExcelDate(object cellValue) {
    if (cellValue == null) return default;
    
    string texto = cellValue.ToString().Trim();
    
    // Intentar ISO primero (más rápido)
    if (DateTime.TryParseExact(texto, "yyyy-MM-dd", 
        CultureInfo.InvariantCulture, DateTimeStyles.None, out var isoDate))
        return isoDate;
    
    // Intentar DD/MM/YYYY (Colombia)
    if (DateTime.TryParseExact(texto, "dd/MM/yyyy", 
        CultureInfo.GetCultureInfo("es-CO"), DateTimeStyles.None, out var colDate))
        return colDate;
    
    // Intentar MM/DD/YYYY (USA)
    if (DateTime.TryParseExact(texto, "MM/dd/yyyy", 
        CultureInfo.InvariantCulture, DateTimeStyles.None, out var usaDate))
        return usaDate;
    
    throw new ValidationException($"Fecha no válida: {texto}");
}
```

---

### **D6: Normalización de Montos (Decimales)**

**Decisión:** ✅ PARSEO FLEXIBLE + VALIDACIÓN ESTRICTA

**Razón:**
- Excel puede enviar: 1000, 1.000, 1,000 (diferentes locales)
- COP es moneda local de Colombia
- PostgreSQL NUMERIC(15,2) almacena centavos exactos
- Redondeo: solo 2 decimales (centavos)

**Lógica de Normalización:**
```
Entrada: "$ 1.234.567,50" (Colombia)
Limpieza: "1234567,50"
Conversión: "1234567.50" (cambiar coma por punto)
Parse: decimal 1234567.50
Validación: >= 0, <= 999999999.99
BD: NUMERIC(15,2) → exacto
```

**Implementación:**
```csharp
public static decimal ParseCurrency(object cellValue) {
    if (cellValue == null) return 0m;
    
    string texto = cellValue.ToString()
        .Trim()
        .Replace("$", "")
        .Replace("COP", "")
        .Replace(" ", "");
    
    // Detectar separadores: si hay . y ,, el último es decimal
    if (texto.Contains(".") && texto.Contains(",")) {
        int lastDot = texto.LastIndexOf(".");
        int lastComma = texto.LastIndexOf(",");
        
        if (lastDot > lastComma) {
            // 1.000,50 → punto es miles, coma es decimal
            texto = texto.Replace(".", "").Replace(",", ".");
        } else {
            // 1,000.50 → coma es miles, punto es decimal
            texto = texto.Replace(",", "");
        }
    } else if (texto.Contains(",") && texto.Count(c => c == ',') == 1) {
        // Solo una coma: es decimal
        texto = texto.Replace(",", ".");
    }
    
    if (decimal.TryParse(texto, CultureInfo.InvariantCulture, out var result)) {
        return Math.Round(result, 2);  // Redondear a 2 decimales
    }
    
    throw new ValidationException($"Monto inválido: {cellValue}");
}
```

---

### **D7: Logs de Carga**

**Decisión:** ✅ TABLA DEDICADA `importaciones_datos` + JSON para detalles

**Razón:**
- Historial completo de qué se cargó, cuándo, por quién
- Facilita auditoría y debugging
- Soporte para reintentos/recuperación
- Permite análisis de problemas comunes

**Tabla:**
```sql
CREATE TABLE importaciones_datos (
    id UUID PRIMARY KEY,
    empresa_id UUID NOT NULL REFERENCES empresas(id),
    tipo_datos VARCHAR(100),  -- "ventas", "inventario", etc.
    nombre_archivo VARCHAR(255) NOT NULL,
    tamaño_archivo BIGINT,
    hash_archivo VARCHAR(64),
    
    estado tipo_estado_importacion,  -- "en_proceso", "completado", "fallido"
    fase_actual tipo_fase_etl,  -- "extraccion", "transformacion", etc.
    
    registros_extraidos INTEGER DEFAULT 0,
    registros_transformados INTEGER DEFAULT 0,
    registros_cargados INTEGER DEFAULT 0,
    registros_rechazados INTEGER DEFAULT 0,
    
    errores_extraccion JSONB DEFAULT '[]',
    errores_transformacion JSONB DEFAULT '[]',
    errores_carga JSONB DEFAULT '[]',
    advertencias JSONB DEFAULT '[]',
    
    fecha_importacion TIMESTAMPTZ DEFAULT NOW(),
    fecha_inicio_etl TIMESTAMPTZ,
    fecha_fin_etl TIMESTAMPTZ,
    duracion_segundos INTEGER,
    
    resultado_carga JSONB,  -- Resultado final con stats
    log_completo TEXT,
    
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);
```

**Estructura JSON de errores:**
```json
{
  "errores": [
    {
      "fila": 5,
      "columna": "precio_venta",
      "tipo_error": "validacion_formato",
      "mensaje": "Debe ser número mayor a 0",
      "valor_encontrado": "-150",
      "tipo_dato_esperado": "decimal"
    }
  ],
  "advertencias": [
    "Código 'PANT-001' ya existe en BD. Será actualizado."
  ],
  "resumen": {
    "total_errores": 2,
    "total_advertencias": 1,
    "filas_procesadas": 100
  }
}
```

---

### **D8: Notificaciones Post-Carga**

**Decisión:** ✅ WEBHOOK ASINCRÓNICO → Sistema de IA (Phase 2.5)

**Razón:**
- No bloquea la respuesta al usuario
- La IA puede procesar datos después de cargarse
- Permite análisis en background

**Implementación Futura:**
```csharp
// En background job (Hangfire/Quartz)
private async Task NotifyAIAboutNewDataAsync(Guid empresaId, ImportResultDto resultado) {
    var aiContext = new AIContextDto {
        empresaId = empresaId,
        datoCargado = resultado.tipo_datos,
        registrosCargados = resultado.registros_cargados,
        fecha = DateTime.UtcNow,
        datos_summary = ObtenerSummaryDatos(empresaId)
    };
    
    await _aiService.GenerateRecommendationsAsync(aiContext);
}
```

---

---

## 🎯 RECOMENDACIONES ARQUITECTÓNICAS

### **R1: Caché de Validaciones**

**Implementar:** Redis para listas de categorías, unidades, métodos de pago

```csharp
// Al iniciar aplicación
var categoriasPermitidas = new[] { 
    "Uniformes", "Casual", "Formal", "Deportivo", ... 
};
await _cache.SetAsync("claridata:categorias", categoriasPermitidas, 
    expiration: TimeSpan.FromHours(24));

// En validación
var categorias = await _cache.GetAsync("claridata:categorias");
if (categoriaUsuario NOT IN categorias) 
    ALERTA pero NO ERROR;  // Solo advertencia
```

**Beneficio:** Validación 10x más rápida en archivos grandes

---

### **R2: Validación de Negocio Avanzada (Fase 2.5)**

**Implementar después:**
- Margen mínimo de ganancia (precio_venta >= costo * factor)
- Stock de seguridad (cantidad_disponible >= stock_minimo)
- Coherencia de fechas en ventas (fecha_entrega >= fecha_venta)
- Límites de montos por método de pago (efectivo ≤ límite diario)

---

### **R3: Imputación de Datos Faltantes (Fase 2.5)**

**Estrategias:**
- Categoría faltante → "Sin clasificar"
- Costo faltante → 0 (permitido para servicios)
- Email faltante → vacío (permitido)
- Margen calculado automáticamente (si costo y precio están)

---

### **R4: Validación de Relaciones Cruzadas (Fase 2.5)**

```csharp
// Ejemplo: Cliente en venta debe existir o ser creado
var cliente = await _db.Clientes.FirstOrDefaultAsync(c => 
    c.EmpresaId == empresaId && c.Nombre == venta.cliente_nombre);

if (cliente == null) {
    // Crear cliente automáticamente si no existe
    cliente = new Cliente {
        EmpresaId = empresaId,
        CodigoCliente = GenerarCodigoUnico(),
        Nombre = venta.cliente_nombre,
        Documento = venta.cliente_documento,
        // ...
    };
    _db.Clientes.Add(cliente);
}

venta.ClienteId = cliente.Id;
```

---

### **R5: Interfaz de Reintento (Phase 2.5)**

**Permitir:** User corrige solo las filas con error, no todo el archivo

```
Flujo:
1. Carga falla con 3 errores en filas 5, 12, 15
2. Usuario descarga archivo de errores (Excel con errores marcados)
3. Corrige solo esas filas
4. Sube nuevamente (solo esas filas)
5. Sistema valida incrementalmente
6. ✅ Carga completada
```

---

### **R6: Reportes Descargables (Phase 2.5)**

**Generar:** Excel con:
- Hoja "Resumen" (total cargado, errores, advertencias)
- Hoja "Productos Cargados" (listado completo)
- Hoja "Errores" (fila + columna + solución)
- Gráficos de distribución por categoría

---

### **R7: API Programada para Cargas (Phase 2.5)**

**Endpoint para integraciones:**
```
POST /api/v1/import/schedule
{
    "empresa_id": "uuid",
    "tipo_carga": "mensual",
    "frecuencia": "CRON: 0 1 1 * *",  // 1 AM primer día mes
    "fuente_datos": "sftp://proveedor.com/datos/",
    "archivo_patron": "sales_*.xlsx"
}
```

**Ventaja:** Cargas automáticas sin intervención del usuario

---

### **R8: Monitoreo & Alertas**

**Dashboard de Importaciones:**
- Última carga: fecha, estado, # registros
- Tendencia de errores (gráfico)
- Archivos fallidos + razones
- SLA: 99.9% de cargas exitosas

**Alertas:**
- 🔴 Carga fallida → Email al admin
- 🟡 Archivo muy grande (> 9 MB) → Aviso de proximidad al límite
- 🟢 Carga exitosa → Notificación en app

---

---

## 🚀 ROADMAP: FASES DE IMPLEMENTACIÓN

### **FASE 1 (ACTUAL)** ✅
- [x] Diseño plantilla Excel
- [x] Pseudocódigo pipeline ETL
- [x] Validaciones básicas (estructura, formato, obligatorios)

### **FASE 2** (2-3 semanas)
- [ ] Implementar en C# (validadores, parsers, loaders)
- [ ] Integración con PostgreSQL
- [ ] Manejo de transacciones
- [ ] Logs en `importaciones_datos`
- [ ] UI: Componente drag-drop + progreso
- [ ] Reportes básicos

### **FASE 2.5** (3-4 semanas)
- [ ] Validaciones de negocio avanzadas
- [ ] Imputación de datos
- [ ] Validación de relaciones cruzadas
- [ ] Interfaz de reintento incremental
- [ ] Reportes descargables (Excel)
- [ ] Webhooks para IA

### **FASE 3** (4-5 semanas)
- [ ] API programada para cargas automáticas
- [ ] Caché de validaciones (Redis)
- [ ] Monitoreo & alertas
- [ ] Soporte para múltiples proveedores de datos (no solo Excel)
- [ ] Dashboard de importaciones

---

---

## 📌 DECISIONES POR CONFIRMAR (Si algo cambió)

Si después de esta documentación necesitas ajustar algo, aquí están los puntos críticos:

| Aspecto | Decisión Actual | ¿Cambiar? |
|---------|-----------------|-----------|
| Validación | Estricta (rechazo total) | ✓ |
| Aislamiento BD | Serializable | ✓ |
| Duplicados | Upsert (actualizar) | ✓ |
| Formato fechas | ISO + flexible | ✓ |
| Moneda | COP (ajustable) | ✓ |
| Tamaño máximo | 10 MB | ✓ |
| Reutilización cliente | Auto-crear si no existe | ✓ |

---

---

## 🔧 CHECKLIST IMPLEMENTACIÓN (Para desarrollador C#)

### **Validación & Parseo**
- [ ] Clase `ExcelValidationService`
- [ ] Función `ParseExcelDate()`
- [ ] Función `ParseCurrency()`
- [ ] Función `ParseBoolean()`
- [ ] Función `ValidateEmail()`
- [ ] Función `NormalizeText()`

### **Modelos DTO**
- [ ] `ProductoDTO`
- [ ] `InventarioDTO`
- [ ] `VentaDTO`
- [ ] `DetalleVentaDTO`
- [ ] `FinancieroDTO`
- [ ] `ImportResultDTO`

### **Servicios**
- [ ] `ExcelReaderService`
- [ ] `DataValidationService`
- [ ] `DataTransformationService`
- [ ] `
- [ ] LoaderService`
- [ ] `ImportLogService`

### **Controllers**
- [ ] POST `/api/import/excel` (upload)
- [ ] GET `/api/import/status/{id}` (check status)
- [ ] GET `/api/import/history` (listar cargas)
- [ ] GET `/api/import/download-errors/{id}` (descargar reporte)

### **Pruebas**
- [ ] Unit tests para cada parser
- [ ] Unit tests para validaciones
- [ ] Integration tests para carga completa
- [ ] Test con archivos edge case (errores, duplicados, etc.)

---

## 📄 PLANTILLAS DE ERRORES PARA MOSTRAR AL USUARIO

**Usar estos mensajes exactos en respuesta HTTP:**

```json
{
  "exitoso": false,
  "tipo_error": "validacion_archivo",
  "mensaje_usuario": "El archivo no es válido. Por favor, revisa lo siguiente:",
  "errores": [
    {
      "fila": 5,
      "columna": "precio_venta",
      "error": "Debe ser número mayor a 0",
      "valor_encontrado": "-150",
      "sugerencia": "Ingresa un precio positivo"
    },
    {
      "fila": 8,
      "columna": "codigo_producto",
      "error": "Código duplicado",
      "valor_encontrado": "PANT-001",
      "sugerencia": "Cambia el código o revisa si el producto ya existe"
    }
  ],
  "resumen": {
    "total_errores": 2,
    "total_advertencias": 0,
    "filas_procesadas": 100
  },
  "instrucciones": "1. Corrige los errores listados. 2. Vuelve a descargar la plantilla si es necesario. 3. Intenta subir nuevamente.",
  "help_url": "https://docs.claridata.co/ayuda-carga-datos"
}
```

---

## 🎬 CONCLUSIÓN

Este documento define:

✅ **QUÉ:** Estructura Excel de 4 hojas con 20+ validaciones  
✅ **CÓMO:** Pipeline ETL modular en 8 fases  
✅ **POR QUÉ:** Decisiones arquitectónicas con trade-offs explícitos  
✅ **CUÁNDO:** Roadmap de 3 fases de implementación  

Listo para transformar en C# + PostgreSQL.

**Próximo paso:** Crear PR con implementación de Fase 2.

