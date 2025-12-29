# PLANTILLA EXCEL OFICIAL - CLARIDATA

## 📋 ESPECIFICACIÓN TÉCNICA DE LA PLANTILLA

**Versión:** 1.0  
**Formato:** .xlsx (Excel 2013+)  
**Nombre archivo esperado:** `claridata_datos_YYYYMM_[empresa].xlsx`  
**Tamaño máximo:** 10 MB  
**Encoding:** UTF-8  
**Zona horaria:** America/Bogota (UTC-5)  
**Moneda:** COP (Peso Colombiano)  

---

## 🗂️ ESTRUCTURA: 4 HOJAS INTERCONECTADAS

```
┌─────────────────────────────────────────────────┐
│ CLARIDATA_DATOS_202501_ACME.xlsx                │
├─────────────────────────────────────────────────┤
│ [1] PRODUCTOS          (maestro)                 │
│ [2] INVENTARIO         (maestro)                 │
│ [3] VENTAS             (transaccional)           │
│ [4] FINANCIEROS        (transaccional)           │
└─────────────────────────────────────────────────┘
```

### **Orden de procesamiento obligatorio:**
1️⃣ **PRODUCTOS** (primero - es maestro)  
2️⃣ **INVENTARIO** (depende de PRODUCTOS)  
3️⃣ **VENTAS** (depende de PRODUCTOS e INVENTARIO)  
4️⃣ **FINANCIEROS** (independiente pero complementario)  

---

---

# 🔷 HOJA 1: PRODUCTOS

## Descripción
Catálogo maestro de productos y servicios. **OBLIGATORIA**. Una fila = un SKU único.

## Estructura de Columnas

| # | Columna | Tipo | Obligatorio | Longitud | Rango/Formato | Validaciones | Notas |
|---|---------|------|-------------|----------|---------------|--------------|-------|
| A | `codigo_producto` | Texto | ✅ SÍ | 1-50 | Alfanumérico | Único dentro de la empresa, sin espacios, no puede iniciar con número | ID único por empresa |
| B | `nombre` | Texto | ✅ SÍ | 1-255 | Texto libre | No vacío, sin caracteres especiales, máx. 255 caracteres | Nombre comercial |
| C | `categoria` | Texto | ❌ NO | 1-100 | Predefinido | Debe existir en "Categorías permitidas" | Ej: "Uniformes", "Casual", "Formal" |
| D | `subcategoria` | Texto | ❌ NO | 1-100 | Texto libre | Sólo si categoría está llena | Refinamiento de categoría |
| E | `marca` | Texto | ❌ NO | 1-100 | Texto libre | Alfanumérico | Fabricante |
| F | `modelo` | Texto | ❌ NO | 1-100 | Texto libre | Alfanumérico | Variante del producto |
| G | `precio_venta` | Número | ✅ SÍ | - | $$ Decimal (2 decimales) | > 0, máximo 999,999,999.99 | Precio unitario al público |
| H | `costo_unitario` | Número | ❌ NO | - | $$ Decimal (2 decimales) | >= 0, máximo 999,999,999.99 | Costo de compra al proveedor |
| I | `precio_sugerido` | Número | ❌ NO | - | $$ Decimal (2 decimales) | >= `costo_unitario` | Recomendación de precio |
| J | `unidad_medida` | Texto | ✅ SÍ | 1-50 | Predefinido | Debe estar en lista permitida | Ej: "unidad", "kg", "metro", "litro", "caja" |
| K | `peso_kg` | Número | ❌ NO | - | Decimal (3 decimales) | >= 0 | Peso en kilogramos |
| L | `volumen_m3` | Número | ❌ NO | - | Decimal (3 decimales) | >= 0 | Volumen en metros cúbicos |
| M | `codigo_barras` | Texto | ❌ NO | 8-14 | Numérico | Si se proporciona, debe cumplir EAN-13 | Código de barras |
| N | `codigo_qr` | Texto | ❌ NO | 1-255 | Texto libre | Cualquier valor | URL o código QR |
| O | `es_servicio` | Booleano | ❌ NO | - | VERDADERO/FALSO | Si TRUE: `requiere_inventario` debe ser FALSE | Flag para servicios |
| P | `requiere_inventario` | Booleano | ✅ SÍ | - | VERDADERO/FALSO | Si FALSE: no necesita seguimiento de stock | Por defecto: VERDADERO |
| Q | `activo` | Booleano | ✅ SÍ | - | VERDADERO/FALSO | TRUE o FALSE | Productos inactivos no aparecen en dashboards |
| R | `descripcion` | Texto largo | ❌ NO | 1-1000 | Texto libre | Máximo 1000 caracteres | Detalles adicionales |

## Categorías Permitidas (Predefinidas)
```
- Uniformes
- Casual
- Formal
- Deportivo
- Accesorios
- Calzado
- Otro
```

## Unidades de Medida Permitidas
```
- unidad
- kg
- gramo
- metro
- centímetro
- litro
- mililitro
- caja
- docena
```

## Reglas de Validación

### **Validaciones de Estructura**
- ❌ Columna A (código_producto) no puede ser vacía
- ❌ Columna B (nombre) no puede ser vacía
- ❌ Columna G (precio_venta) no puede ser vacía
- ❌ Columna J (unidad_medida) no puede ser vacía
- ❌ Columna P (requiere_inventario) no puede ser vacía
- ❌ Columna Q (activo) no puede ser vacía

### **Validaciones de Formato**
- ❌ `codigo_producto` debe ser alfanumérico, sin espacios (ej: PROD-001, P001, PROD001)
- ❌ `precio_venta` y `costo_unitario` deben ser números con máximo 2 decimales
- ❌ `costo_unitario` NO puede ser mayor que `precio_venta`
- ❌ `unidad_medida` debe estar en la lista permitida
- ❌ `es_servicio` = TRUE → `requiere_inventario` debe ser FALSE
- ❌ `codigo_barras` si se proporciona, debe tener 8-14 dígitos numéricos

### **Validaciones de Consistencia**
- ❌ No pueden haber códigos duplicados en la misma carga
- ❌ Si un producto ya existe en la BD, se actualiza (no se duplica)
- ❌ `categoria` (si se proporciona) debe estar en lista permitida

---

## 📌 EJEMPLO DE FILA VÁLIDA

| código_producto | nombre | categoría | marca | precio_venta | costo_unitario | unidad_medida | peso_kg | requiere_inventario | activo |
|---|---|---|---|---|---|---|---|---|---|
| PANT-001 | Pantalón Formal Negro | Formal | TallerXYZ | 89,500 | 35,000 | unidad | 0.45 | VERDADERO | VERDADERO |
| CAMI-002 | Camisa Casual Azul | Casual | MarcaABC | 54,900 | 22,000 | unidad | 0.25 | VERDADERO | VERDADERO |
| SVC-001 | Servicio de Alteración | Servicios | Interno | 25,000 | 5,000 | unidad | - | FALSO | VERDADERO |

---

## ❌ EJEMPLOS DE ERRORES (Rechazados)

| Error | Razón | Solución |
|-------|-------|----------|
| Código vacío | Obligatorio | Asignar código único |
| PANT 001 | Contiene espacio | Usar PANT-001 o PANT001 |
| precio_venta = 0 | Debe ser > 0 | Ingresar precio válido |
| costo_unitario > precio_venta | Lógica inconsistente | Ajustar costo o precio |
| unidad_medida = "unidades" | No está en lista | Cambiar a "unidad" |
| es_servicio=VERDADERO + requiere_inventario=VERDADERO | Conflicto lógico | Si es servicio, poner FALSE |

---

---

# 🔷 HOJA 2: INVENTARIO

## Descripción
Estado actual del stock por producto. **OPCIONAL pero recomendada**. Una fila = un producto con seguimiento.

## Estructura de Columnas

| # | Columna | Tipo | Obligatorio | Longitud | Rango/Formato | Validaciones | Notas |
|---|---------|------|-------------|----------|---------------|--------------|-------|
| A | `codigo_producto` | Texto | ✅ SÍ | 1-50 | Alfanumérico | Debe existir en hoja PRODUCTOS | FK a PRODUCTOS |
| B | `cantidad_disponible` | Número | ✅ SÍ | - | Entero >= 0 | >= 0 | Unidades en stock |
| C | `cantidad_reservada` | Número | ❌ NO | - | Entero >= 0 | >= 0, <= cantidad_disponible | Unidades apartadas |
| D | `cantidad_en_transito` | Número | ❌ NO | - | Entero >= 0 | >= 0 | Unidades en compra |
| E | `stock_minimo` | Número | ❌ NO | - | Entero >= 0 | >= 0 | Umbral de alerta |
| F | `stock_maximo` | Número | ❌ NO | - | Entero >= 0 | >= `stock_minimo` si se proporciona | Máximo recomendado |
| G | `punto_reorden` | Número | ❌ NO | - | Entero >= 0 | >= `stock_minimo` | Cantidad para ordenar |
| H | `ubicacion` | Texto | ❌ NO | 1-100 | Texto libre | Alfanumérico | Almacén/zona (ej: "Almacén A", "Pasillo 3") |
| I | `pasillo` | Texto | ❌ NO | 1-20 | Alfanumérico | Sin espacios | Ej: "A1", "B2", "C3" |
| J | `estante` | Texto | ❌ NO | 1-20 | Alfanumérico | Sin espacios | Ej: "01", "02", "03" |
| K | `nivel` | Texto | ❌ NO | 1-20 | Alfanumérico | Sin espacios | Ej: "1", "2", "3" (altura) |
| L | `lote_actual` | Texto | ❌ NO | 1-50 | Alfanumérico | Sin espacios | Número de lote |
| M | `fecha_vencimiento` | Fecha | ❌ NO | - | YYYY-MM-DD | >= hoy | Fecha de caducidad |
| N | `dias_alerta_vencimiento` | Número | ❌ NO | - | Entero 1-365 | Entre 1 y 365 | Días previos para alerta |

## Reglas de Validación

### **Validaciones de Estructura**
- ❌ Columna A (codigo_producto) no puede ser vacía
- ❌ Columna B (cantidad_disponible) no puede ser vacía

### **Validaciones de Formato**
- ❌ `codigo_producto` debe existir en hoja PRODUCTOS
- ❌ `cantidad_disponible`, `cantidad_reservada`, `cantidad_en_transito` deben ser enteros >= 0
- ❌ `stock_maximo` >= `stock_minimo` (si ambos se proporcionan)
- ❌ `fecha_vencimiento` debe ser formato YYYY-MM-DD y >= fecha actual
- ❌ `dias_alerta_vencimiento` debe ser 1-365

### **Validaciones de Consistencia**
- ❌ `cantidad_reservada` no puede ser > `cantidad_disponible`
- ❌ No pueden haber códigos duplicados en esta hoja
- ❌ Solo se procesan productos que tienen `requiere_inventario = VERDADERO` en PRODUCTOS

---

## 📌 EJEMPLO DE FILA VÁLIDA

| código_producto | cantidad_disponible | stock_minimo | stock_maximo | ubicación | pasillo | estante | lote_actual | fecha_vencimiento |
|---|---|---|---|---|---|---|---|---|
| PANT-001 | 45 | 10 | 100 | Almacén A | A | 01 | LOTE-2025-001 | 2026-12-31 |
| CAMI-002 | 12 | 5 | 50 | Almacén B | B | 02 | LOTE-2025-002 | 2026-06-30 |

---

---

# 🔷 HOJA 3: VENTAS

## Descripción
Transacciones de venta. **OBLIGATORIA para dashboard de VENTAS**. Una fila = una orden de compra.

## Estructura de Columnas

### **Encabezado de Orden (obligatorios)**

| # | Columna | Tipo | Obligatorio | Longitud | Rango/Formato | Validaciones | Notas |
|---|---------|------|-------------|----------|---------------|--------------|-------|
| A | `numero_orden` | Texto | ✅ SÍ | 1-50 | Alfanumérico | Único por empresa y período | ID único de venta (ej: ORD-001, V001) |
| B | `numero_factura` | Texto | ❌ NO | 1-50 | Alfanumérico | Único si se proporciona | Número de factura emitida |
| C | `fecha_venta` | Fecha | ✅ SÍ | - | YYYY-MM-DD | Válida, no futura | Fecha de la transacción |
| D | `cliente_nombre` | Texto | ✅ SÍ | 1-255 | Texto libre | No vacío | Nombre del cliente |
| E | `cliente_documento` | Texto | ❌ NO | 1-50 | Numérico | Formato válido | Cédula/NIT cliente |
| F | `cliente_telefono` | Texto | ❌ NO | 1-20 | Numérico | Formato válido | Teléfono cliente |
| G | `cliente_email` | Texto | ❌ NO | 1-255 | Email | Formato válido | Email cliente |
| H | `cliente_direccion` | Texto | ❌ NO | 1-500 | Texto libre | Cualquier valor | Dirección entrega |
| I | `ciudad` | Texto | ❌ NO | 1-100 | Texto libre | Cualquier valor | Municipio |
| J | `monto_subtotal` | Número | ✅ SÍ | - | Decimal (2) | > 0 | Suma antes de descuentos/impuestos |
| K | `monto_descuento` | Número | ❌ NO | - | Decimal (2) | >= 0 | Descuento total |
| L | `monto_impuestos` | Número | ❌ NO | - | Decimal (2) | >= 0 | Impuestos (IVA, etc.) |
| M | `monto_total` | Número | ✅ SÍ | - | Decimal (2) | > 0 | subtotal - descuento + impuestos |
| N | `metodo_pago` | Texto | ✅ SÍ | 1-50 | Predefinido | Debe estar en lista permitida | Ej: "efectivo", "tarjeta", "transferencia" |
| O | `estado_pago` | Texto | ❌ NO | 1-50 | Predefinido | "pendiente" o "pagado" | Estado del pago |
| P | `vendedor` | Texto | ❌ NO | 1-100 | Texto libre | Cualquier valor | Nombre vendedor |
| Q | `canal_venta` | Texto | ❌ NO | 1-100 | Predefinido | Debe estar en lista permitida | Ej: "presencial", "online", "phone" |
| R | `estado` | Texto | ❌ NO | 1-50 | Predefinido | "completado" o "pendiente" | Estado de la orden |
| S | `notas` | Texto largo | ❌ NO | 1-1000 | Texto libre | Máximo 1000 caracteres | Observaciones |

### **Detalle de Productos (una columna por producto - opcional pero recomendada)**

Para cada línea de producto en la venta:

| Columna | Tipo | Obligatorio | Validaciones | Notas |
|---------|------|-------------|--------------|-------|
| `codigo_producto_{n}` | Texto | ❌ NO* | Debe existir en PRODUCTOS | Código del producto (ej: PANT-001) |
| `cantidad_{n}` | Número | ❌ NO* | > 0, entero o decimal | Cantidad vendida |
| `precio_unitario_{n}` | Número | ❌ NO* | > 0 | Precio por unidad |
| `descuento_porcentaje_{n}` | Número | ❌ NO | 0-100 | % descuento por línea |
| `subtotal_{n}` | Número | ❌ NO* | Calculado: cantidad * precio_unitario | Monto antes de impuestos |

*Si proporciona detalles, deben cumplirse estas validaciones. Si NO proporciona, la orden se registra sin detalles (menos detallada).

## Métodos de Pago Permitidos
```
- efectivo
- tarjeta
- transferencia
- credito
- cheque
```

## Canales de Venta Permitidos
```
- presencial
- online
- telefonico
- distribuidor
- otro
```

## Reglas de Validación

### **Validaciones de Estructura**
- ❌ `numero_orden` no puede ser vacío
- ❌ `fecha_venta` no puede ser vacía
- ❌ `cliente_nombre` no puede ser vacío
- ❌ `monto_total` no puede ser vacío
- ❌ `metodo_pago` no puede ser vacío

### **Validaciones de Formato**
- ❌ `numero_orden` debe ser único dentro de la empresa (no pueden repetirse en la carga)
- ❌ `fecha_venta` debe ser YYYY-MM-DD y no puede ser fecha futura
- ❌ `monto_subtotal`, `monto_descuento`, `monto_impuestos`, `monto_total` deben ser >= 0 con máximo 2 decimales
- ❌ `monto_total` = `monto_subtotal` - `monto_descuento` + `monto_impuestos` (validación matemática)
- ❌ `metodo_pago` debe estar en lista permitida
- ❌ `cliente_email` si se proporciona, debe cumplir formato email válido
- ❌ `cliente_documento` si se proporciona, debe ser numérico

### **Validaciones de Consistencia**
- ❌ Si proporciona `codigo_producto_{n}`, debe existir en hoja PRODUCTOS
- ❌ Si proporciona detalles de producto: `cantidad > 0`
- ❌ `monto_descuento` no puede ser > `monto_subtotal`
- ❌ No pueden haber órdenes duplicadas (mismo número_orden)

---

## 📌 EJEMPLO DE FILA VÁLIDA

| número_orden | fecha_venta | cliente_nombre | monto_subtotal | monto_descuento | monto_impuestos | monto_total | metodo_pago | estado_pago | vendedor | estado | código_producto_1 | cantidad_1 | precio_unitario_1 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| ORD-001 | 2025-01-15 | Juan Pérez | 150,000 | 15,000 | 21,600 | 156,600 | efectivo | pagado | Carlos | completado | PANT-001 | 2 | 75,000 |
| ORD-002 | 2025-01-16 | María López | 109,800 | 0 | 19,800 | 129,600 | tarjeta | pagado | Ana | completado | CAMI-002 | 2 | 54,900 |

---

---

# 🔷 HOJA 4: FINANCIEROS

## Descripción
Movimientos financieros (ingresos, gastos, costos). **OBLIGATORIA para dashboard FINANCIEROS**. Una fila = un movimiento.

## Estructura de Columnas

| # | Columna | Tipo | Obligatorio | Longitud | Rango/Formato | Validaciones | Notas |
|---|---------|------|-------------|----------|---------------|--------------|-------|
| A | `tipo_dato` | Texto | ✅ SÍ | - | Predefinido | Debe estar en lista permitida | "ingreso", "gasto", "costo", "inversion" |
| B | `categoria` | Texto | ✅ SÍ | 1-100 | Predefinido | Debe estar en lista permitida según `tipo_dato` | Ej: "Ventas" para ingreso, "Salarios" para gasto |
| C | `subcategoria` | Texto | ❌ NO | 1-100 | Texto libre | Refinamiento de categoría | Ej: "Ventas Online", "Servicio Técnico" |
| D | `concepto` | Texto | ✅ SÍ | 1-255 | Texto libre | No vacío | Descripción del movimiento |
| E | `monto` | Número | ✅ SÍ | - | Decimal (2) | > 0 | Monto del movimiento |
| F | `moneda` | Texto | ❌ NO | 3 | Predefinido | "COP", "USD", "EUR", etc. | Por defecto: COP |
| G | `fecha_registro` | Fecha | ✅ SÍ | - | YYYY-MM-DD | Válida, no futura | Fecha del movimiento |
| H | `fecha_pago` | Fecha | ❌ NO | - | YYYY-MM-DD | >= `fecha_registro` | Fecha efectiva (si aplica) |
| I | `numero_comprobante` | Texto | ❌ NO | 1-50 | Alfanumérico | Sin espacios | Recibo, factura, comprobante |
| J | `beneficiario` | Texto | ❌ NO | 1-255 | Texto libre | Cualquier valor | Quién recibe/paga |
| K | `observaciones` | Texto largo | ❌ NO | 1-1000 | Texto libre | Máximo 1000 caracteres | Notas adicionales |

## Tipos de Datos Permitidos
```
- ingreso     (Ingresos por ventas)
- gasto       (Gastos operacionales)
- costo       (Costo de productos vendidos)
- inversion   (Inversiones en activos)
```

## Categorías Permitidas por Tipo

### **INGRESO**
```
- Ventas
- Servicios
- Retorno de inversión
- Intereses
- Otros ingresos
```

### **GASTO**
```
- Salarios
- Servicios (arriendo, utilities, teléfono)
- Transporte
- Marketing
- Comisiones
- Otros gastos
```

### **COSTO**
```
- Costo de Bienes Vendidos (COGS)
- Materia Prima
- Mano de Obra Directa
- Otros costos
```

### **INVERSION**
```
- Activos Fijos
- Mejoras
- Tecnología
- Otros
```

## Reglas de Validación

### **Validaciones de Estructura**
- ❌ `tipo_dato` no puede ser vacío
- ❌ `categoria` no puede ser vacía
- ❌ `concepto` no puede ser vacío
- ❌ `monto` no puede ser vacío
- ❌ `fecha_registro` no puede ser vacía

### **Validaciones de Formato**
- ❌ `tipo_dato` debe estar en lista permitida
- ❌ `categoria` debe estar en lista permitida según `tipo_dato`
- ❌ `monto` debe ser número > 0 con máximo 2 decimales
- ❌ `fecha_registro` debe ser YYYY-MM-DD y no puede ser fecha futura
- ❌ `fecha_pago` (si se proporciona) debe ser >= `fecha_registro`
- ❌ `moneda` (si se proporciona) debe ser código válido (COP, USD, EUR, etc.)

### **Validaciones de Consistencia**
- ❌ No pueden haber comprobantes duplicados (mismo `numero_comprobante` en el período)
- ❌ Fechas coherentes: `fecha_pago` >= `fecha_registro`

---

## 📌 EJEMPLO DE FILAS VÁLIDAS

| tipo_dato | categoria | subcategoria | concepto | monto | fecha_registro | fecha_pago | numero_comprobante | beneficiario |
|---|---|---|---|---|---|---|---|---|
| ingreso | Ventas | Ventas Presenciales | Venta diaria 15/01 | 500,000 | 2025-01-15 | 2025-01-15 | FAC-001 | Caja |
| gasto | Salarios | Nómina | Pago salarios enero | 1,500,000 | 2025-01-31 | 2025-01-31 | NOMINA-01 | Empleados |
| costo | Costo de Bienes Vendidos | Materia Prima | Compra tela | 200,000 | 2025-01-10 | 2025-01-10 | OC-001 | Proveedor XYZ |
| inversion | Activos Fijos | Tecnología | Computadora nueva | 3,500,000 | 2025-01-20 | 2025-01-20 | REC-001 | Tech Store |

---

---

## 📊 MATRIZ DE RELACIONES

```
┌──────────────────────────────────────────────────────────┐
│                   CLARIDATA DATA MODEL                   │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  PRODUCTOS (Maestro)                                     │
│  ├─ código_producto (PK)                                │
│  ├─ nombre                                              │
│  ├─ precio_venta                                        │
│  └─ requiere_inventario                                │
│      │                                                  │
│      ├──→ INVENTARIO                                    │
│      │    ├─ código_producto (FK)                       │
│      │    ├─ cantidad_disponible                        │
│      │    ├─ stock_minimo                               │
│      │    └─ estado_stock                               │
│      │                                                  │
│      └──→ VENTAS → DETALLES_VENTA                       │
│           ├─ número_orden (PK)                          │
│           ├─ código_producto_{n} (FK)                   │
│           ├─ fecha_venta                                │
│           ├─ monto_total                                │
│           ├─ metodo_pago                                │
│           └─ estado                                     │
│                                                          │
│  FINANCIEROS (Independiente)                            │
│  ├─ tipo_dato                                           │
│  ├─ categoria                                           │
│  ├─ monto                                               │
│  ├─ fecha_registro                                      │
│  └─ observaciones                                       │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## 🎯 ORDEN DE CARGA OBLIGATORIO

```
1. VALIDAR ESTRUCTURA (archivo, formato, hojas)
   ↓
2. CARGAR Y VALIDAR PRODUCTOS
   └─ Error crítico → RECHAZAR TODO
   ↓
3. CARGAR Y VALIDAR INVENTARIO
   └─ Error crítico → RECHAZAR TODO
   ↓
4. CARGAR Y VALIDAR VENTAS
   └─ Error crítico → RECHAZAR TODO
   ↓
5. CARGAR Y VALIDAR FINANCIEROS
   └─ Error crítico → RECHAZAR TODO
   ↓
6. INSERTAR EN BD (transacción atómica)
   └─ Si falla: ROLLBACK completo
```

---

## 📋 MENSAJES DE ERROR EXPLÍCITOS (Para mostrar al usuario)

| Tipo de Error | Mensaje al Usuario | Solución |
|---|---|---|
| Archivo incorrecto | "El archivo no es Excel (.xlsx). Descarga la plantilla oficial." | Usar .xlsx |
| Archivo muy grande | "El archivo supera 10 MB. Máximo permitido: 10 MB. Tu archivo: X MB." | Reducir datos |
| Hojas faltantes | "Falta la hoja 'PRODUCTOS'. Hojas requeridas: PRODUCTOS, INVENTARIO, VENTAS, FINANCIEROS" | Crear hojas |
| Columnas faltantes | "Falta columna 'precio_venta' en hoja PRODUCTOS. Columnas obligatorias: código_producto, nombre, precio_venta, ..." | Agregar columnas |
| Código producto vacío | "Hoja PRODUCTOS, fila 5: Columna 'código_producto' está vacía. Esta columna es obligatoria." | Completar dato |
| Precio inválido | "Hoja PRODUCTOS, fila 7: 'precio_venta' = 0. Debe ser mayor a 0." | Corregir valor |
| Producto duplicado | "Hoja PRODUCTOS: Código 'PANT-001' está duplicado en filas 3 y 8. Los códigos deben ser únicos." | Cambiar código |
| Producto no existe | "Hoja INVENTARIO, fila 4: Código 'PANT-999' no existe en hoja PRODUCTOS." | Verificar código |
| Descuento > subtotal | "Hoja VENTAS, fila 6: monto_descuento (50,000) > monto_subtotal (40,000). Descuento no puede ser mayor al subtotal." | Corregir descuento |
| Total inconsistente | "Hoja VENTAS, fila 3: monto_total (156,600) ≠ subtotal (150,000) - descuento (15,000) + impuestos (21,600). Revisa el cálculo." | Recalcular |
| Fecha futura | "Hoja VENTAS, fila 2: fecha_venta = 2025-12-01 (futura). No se permiten fechas futuras." | Cambiar fecha |
| Email inválido | "Hoja VENTAS, fila 5: cliente_email = 'juan@.com'. Formato email inválido." | Corregir email |

---

## 📥 INSTRUCCIONES PARA EL USUARIO

### **Antes de subir:**
1. ✅ Descarga la plantilla oficial desde ClariData
2. ✅ Completa TODAS las columnas obligatorias (marcadas en negrita)
3. ✅ Respeta los tipos de datos (número, fecha, texto, etc.)
4. ✅ No cambies nombres de columnas ni hojas
5. ✅ Revisa fechas (formato YYYY-MM-DD, ej: 2025-01-15)
6. ✅ Verifica códigos únicos (no duplicados)
7. ✅ Asegúrate de que el archivo sea .xlsx (no .xls ni .csv)

### **Al subir:**
1. Click en "Seleccionar Archivo"
2. Elige tu archivo Excel
3. ClariData valida automáticamente
4. Si hay errores, verás el detalle exacto
5. Corrige y vuelve a intentar

### **Después de subir:**
1. Si es exitoso: Los datos aparecen en tus dashboards al instante
2. Si falla: Lee el error y corrije
3. Las cargas son aditivas (no reemplazan datos anteriores)

---

## 📧 SOPORTE

- **Descargar plantilla**: [Botón de descarga en interfaz]
- **Ver ejemplo completo**: [Link a archivo de ejemplo]
- **Preguntas**: support@claridata.co
- **Centro de ayuda**: docs.claridata.co/carga-datos
