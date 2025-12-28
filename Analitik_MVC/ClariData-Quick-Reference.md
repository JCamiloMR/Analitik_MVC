# ⚡ QUICK REFERENCE CARD - CLARIDATA ETL

**Para desarrolladores C#** - Referencia rápida de validaciones y parsers  
**Imprime o guarda en favoritos** 📌

---

## 🔴 VALIDACIONES OBLIGATORIAS (RECHAZO TOTAL)

### **Nivel 1: Archivo**
```csharp
✗ Extensión ≠ ".xlsx"                          → "No es Excel"
✗ Tamaño > 10 MB                                → "Archivo muy grande"
✗ Archivo corrupto/inválido                     → "Excel corrupto"
```

### **Nivel 2: Estructura**
```csharp
✗ Falta hoja "PRODUCTOS"                       → "Faltan hojas"
✗ Falta hoja "INVENTARIO"
✗ Falta hoja "VENTAS"
✗ Falta hoja "FINANCIEROS"
✗ Falta columna obligatoria                     → "Faltan columnas"
✗ Hoja sin datos (solo encabezado)              → "Hoja vacía"
```

### **Nivel 3: Datos (PRODUCTOS)**
```csharp
✗ codigo_producto vacío                         → RECHAZAR FILA
✗ nombre vacío                                  → RECHAZAR FILA
✗ precio_venta vacío                            → RECHAZAR FILA
✗ unidad_medida vacío                           → RECHAZAR FILA
✗ requiere_inventario vacío                     → RECHAZAR FILA
✗ activo vacío                                  → RECHAZAR FILA

✗ codigo_producto duplicado (en carga)          → RECHAZAR FILA
✗ codigo_producto NO es alfanumérico            → RECHAZAR FILA
✗ codigo_producto empieza con número            → RECHAZAR FILA
✗ precio_venta <= 0                             → RECHAZAR FILA
✗ precio_venta NO es número                     → RECHAZAR FILA
✗ costo_unitario > precio_venta                 → RECHAZAR FILA
✗ es_servicio=true + requiere_inventario=true  → RECHAZAR FILA
✗ unidad_medida NO en lista permitida           → RECHAZAR FILA
```

### **Nivel 3: Datos (INVENTARIO)**
```csharp
✗ codigo_producto vacío                         → RECHAZAR FILA
✗ cantidad_disponible vacío                     → RECHAZAR FILA
✗ codigo_producto NO existe en PRODUCTOS        → RECHAZAR FILA
✗ cantidad_disponible < 0                       → RECHAZAR FILA
✗ cantidad_reservada > cantidad_disponible      → RECHAZAR FILA
✗ stock_maximo < stock_minimo                   → RECHAZAR FILA
✗ fecha_vencimiento < HOY()                     → RECHAZAR FILA
```

### **Nivel 3: Datos (VENTAS)**
```csharp
✗ numero_orden vacío                            → RECHAZAR FILA
✗ fecha_venta vacío                             → RECHAZAR FILA
✗ cliente_nombre vacío                          → RECHAZAR FILA
✗ monto_total vacío                             → RECHAZAR FILA
✗ metodo_pago vacío                             → RECHAZAR FILA

✗ numero_orden duplicado (en carga)             → RECHAZAR FILA
✗ numero_orden duplicado (en BD)                → RECHAZAR FILA
✗ fecha_venta es futura                         → RECHAZAR FILA
✗ metodo_pago NO en lista permitida             → RECHAZAR FILA
✗ cliente_email inválido (si se proporciona)    → RECHAZAR FILA
✗ monto_total ≠ subtotal - descuento + impuesto → RECHAZAR FILA
✗ monto_descuento > monto_subtotal              → RECHAZAR FILA
✗ codigo_producto NO existe en PRODUCTOS        → RECHAZAR FILA
```

### **Nivel 3: Datos (FINANCIEROS)**
```csharp
✗ tipo_dato vacío                               → RECHAZAR FILA
✗ categoria vacío                               → RECHAZAR FILA
✗ concepto vacío                                → RECHAZAR FILA
✗ monto vacío                                   → RECHAZAR FILA
✗ fecha_registro vacío                          → RECHAZAR FILA

✗ tipo_dato NO en ["ingreso", "gasto", ...]    → RECHAZAR FILA
✗ categoria NO válida para tipo_dato            → RECHAZAR FILA
✗ monto <= 0                                    → RECHAZAR FILA
✗ fecha_registro es futura                      → RECHAZAR FILA
✗ fecha_pago < fecha_registro                   → RECHAZAR FILA
```

---

## 🟡 ADVERTENCIAS (PERMITE CONTINUAR)

```csharp
⚠️  Categoría no reconocida                      → Registrar, no ERROR
⚠️  Código ya existe en BD                       → Actualizar, no INSERT
⚠️  Producto no encontrado (en VENTAS)           → Saltar línea, no ERROR
⚠️  Código no existe en PRODUCTOS (para INV)     → Saltar, generar alerta
⚠️  Producto ya vencido (fecha_vencimiento)      → Permitir, alertar usuario
```

---

## 🎯 PARSERS - PSEUDOCÓDIGO RÁPIDO

### **ParseDate(object valor)**
```csharp
// Intentar en orden:
1. "YYYY-MM-DD"    (ISO - preferido)
2. "DD/MM/YYYY"    (Colombia)
3. "MM/DD/YYYY"    (USA)
4. DateTime.Parse(automatico)

SI TODAS FALLAN → throw ValidationException
```

### **ParseCurrency(object valor)**
```csharp
// Remover símbolos: $, COP, USD, espacios
// SI tiene . y , → último es decimal
// PARSEAR como InvariantCulture (punto decimal)
// REDONDEAR a 2 decimales

RETURN Math.Round(resultado, 2)
```

### **ParseBoolean(object valor)**
```csharp
VERDADERO ← ["VERDADERO", "TRUE", "V", "SÍ", "SI", "S", "YES", "Y", "1", "ACTIVO"]
FALSO    ← ["FALSO", "FALSE", "F", "NO", "N", "0", "INACTIVO"]
```

### **ParseInt(object valor)**
```csharp
SI valor = NULL → 0
INTENTA Int.Parse(valor.ToString().Trim())
EXCEPCIÓN → throw ValidationException
```

### **ParseDecimal(object valor)**
```csharp
// Mismo que ParseCurrency pero sin redondeo a 2 decimales
RETURN Decimal.Parse(texto)
```

### **ValidateEmail(string email)**
```csharp
REGEX: ^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$
SI NO cumple → throw ValidationException
```

### **NormalizeText(string texto)**
```csharp
1. .Trim()
2. Remover espacios múltiples (.Replace("  ", " "))
3. Remover caracteres de control (\x00-\x1F)
4. .ToUpper() para códigos
```

---

## 📋 LISTAS PERMITIDAS

### **Métodos de Pago**
```
efectivo, tarjeta, transferencia, credito, cheque
```

### **Canales de Venta**
```
presencial, online, telefonico, distribuidor, otro
```

### **Unidades de Medida**
```
unidad, kg, gramo, metro, centímetro, 
litro, mililitro, caja, docena
```

### **Categorías Productos**
```
Uniformes, Casual, Formal, Deportivo, Accesorios, 
Calzado, Otro
```

### **Tipos Financieros**
```
ingreso, gasto, costo, inversion
```

### **Categorías por Tipo Financiero**

**INGRESO:**
```
Ventas, Servicios, Retorno inversión, Intereses, Otros ingresos
```

**GASTO:**
```
Salarios, Servicios, Transporte, Marketing, Comisiones, Otros gastos
```

**COSTO:**
```
Costo Bienes Vendidos, Materia Prima, Mano Obra Directa, Otros costos
```

**INVERSION:**
```
Activos Fijos, Mejoras, Tecnología, Otros
```

---

## 🔄 ORDEN DE PROCESAMIENTO

```
┌─────────────────────────────────────┐
│ 1. ValidarArchivoExcel()            │ ← Si falla: RECHAZAR TODO
├─────────────────────────────────────┤
│ 2. ValidarEstructuraHojas()         │ ← Si falla: RECHAZAR TODO
├─────────────────────────────────────┤
│ 3. LeerYMapearProductos()           │ ← Si falla: RECHAZAR TODO
├─────────────────────────────────────┤
│ 4. LeerYMapearInventario()          │ ← Si falla: RECHAZAR TODO
├─────────────────────────────────────┤
│ 5. LeerYMapearVentas()              │ ← Si falla: RECHAZAR TODO
├─────────────────────────────────────┤
│ 6. LeerYMapearFinancieros()         │ ← Si falla: RECHAZAR TODO
├─────────────────────────────────────┤
│ 7. BeginTransaction(SERIALIZABLE)   │ ← Punto de no retorno
│    - InsertarProductos()            │
│    - InsertarInventario()           │
│    - InsertarVentas()               │
│    - InsertarFinancieros()          │
│    COMMIT o ROLLBACK                │
├─────────────────────────────────────┤
│ 8. RegistrarCargataBD()             │ ← Log en tabla
├─────────────────────────────────────┤
│ 9. GenerarReporte()                 │ ← JSON/HTML al usuario
└─────────────────────────────────────┘
```

---

## 💾 TRANSACCIÓN ATÓMICA (C# Entity Framework Core)

```csharp
using (var transaction = await _db.Database
    .BeginTransactionAsync(IsolationLevel.Serializable))
{
    try
    {
        // Fase 1: Productos
        await InsertProductos(productosDTO);
        
        // Fase 2: Inventario
        await InsertInventario(inventarioDTO);
        
        // Fase 3: Ventas
        await InsertVentas(ventasDTO, detallesDTO);
        
        // Fase 4: Financieros
        await InsertFinancieros(financierosDTO);
        
        // Commit si TODO está OK
        await transaction.CommitAsync();
        return new { success = true };
    }
    catch (Exception ex)
    {
        // ROLLBACK automático si hay error
        await transaction.RollbackAsync();
        return new { success = false, error = ex.Message };
    }
}
```

---

## 📊 MENSAJE DE ERROR AL USUARIO (Template JSON)

```json
{
  "exitoso": false,
  "tipo_error": "validacion_datos",
  "mensaje_usuario": "El archivo tiene errores que deben corregirse",
  "errores": [
    {
      "fila": 5,
      "columna": "precio_venta",
      "error": "Debe ser número mayor a 0",
      "valor_encontrado": "-150",
      "sugerencia": "Ingresa un precio positivo (ej: 89500)"
    }
  ],
  "resumen": {
    "total_errores": 1,
    "total_advertencias": 0,
    "filas_procesadas": 100
  },
  "instrucciones": "1. Corrige los errores listados. 2. Vuelve a subir."
}
```

---

## 🧮 FÓRMULAS DE VALIDACIÓN

### **Venta coherente**
```
monto_total = (monto_subtotal - monto_descuento + monto_impuestos)
Tolerancia: ±0.01 (un centavo por errores de redondeo)
```

### **Margen de producto**
```
margen_porcentaje = ((precio_venta - costo_unitario) / costo_unitario) * 100
```

### **Disponibilidad real**
```
cantidad_disponible_real = cantidad_disponible - cantidad_reservada
```

---

## 📈 PERFORMANCE TARGETS

```
Archivo 1 MB   (500 registros)   → 3-5 segundos
Archivo 5 MB   (2500 registros)  → 15-20 segundos
Archivo 10 MB  (5000 registros)  → 30-45 segundos
```

---

## 🛑 CASOS EDGE CASE (MANEJAR)

```csharp
// Email con caracteres especiales
"juan+promo@empresa.co"  ← Válido

// Código con espacios
"PANT 001"  ← NORMALIZAR a "PANT001"

// Precio con puntos y comas
"$1.234.567,50"  ← PARSEAR a 1234567.50

// Fecha con barra invertida
"15\01\2025"  ← RECHAZO, malformado

// Duplicado en carga
Fila 3: PANT-001
Fila 8: PANT-001  ← RECHAZAR FILA 8

// Documento de cliente
"1.234.567-8" (con puntos y guión)  ← NORMALIZAR a "12345678"

// Categoría con espacios
" Uniformes " (espacios antes/después)  ← NORMALIZAR a "Uniformes"

// Boolean con variantes
"VERDADERO", "true", "V", "Sí"  ← TODAS = TRUE
```

---

## 🎯 CHECKLIST PRE-IMPLEMENTACIÓN

- [ ] Descargar y revisar 3 documentos (Plantilla, Pseudocódigo, Decisiones)
- [ ] Crear DTOs (ProductoDTO, InventarioDTO, VentaDTO, FinancieroDTO)
- [ ] Implementar parsers (ParseDate, ParseCurrency, ParseBoolean, etc.)
- [ ] Implementar validadores (por tipo de dato)
- [ ] Crear ExcelValidationService (9 métodos)
- [ ] Crear DatabaseLoaderService (upsert + transacciones)
- [ ] Crear tests unitarios (mínimo 30 casos)
- [ ] Crear tests de integración (carga completa)
- [ ] Documentar API endpoints (3 nuevos)

---

## 📞 REFERENCIAS RÁPIDAS

- **Pseudocódigo detallado:** `ClariData-Pseudocodigo-ETL.md`
- **Plantilla Excel completa:** `ClariData-Plantilla-Excel.md`
- **Decisiones arquitectónicas:** `ClariData-Decisiones-Diseno.md`
- **Resumen ejecutivo:** `ClariData-Resumen-Ejecutivo.md`

---

**Imprime esta tarjeta. Tenla a mano mientras codeas. 🚀**
