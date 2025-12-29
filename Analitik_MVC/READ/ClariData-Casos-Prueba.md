# 🧪 CASOS DE PRUEBA - CLARIDATA ETL

**Para QA** - Matriz completa de validaciones  
**Status:** Listos para automatizar  

---

## 📊 ESTRUCTURA DE CASOS DE PRUEBA

```
TOTAL CASOS: 87 pruebas
├─ NIVEL 1 (Validación Archivo): 5 casos
├─ NIVEL 2 (Validación Estructura): 8 casos
├─ NIVEL 3a (Validación PRODUCTOS): 18 casos
├─ NIVEL 3b (Validación INVENTARIO): 12 casos
├─ NIVEL 3c (Validación VENTAS): 14 casos
├─ NIVEL 3d (Validación FINANCIEROS): 12 casos
├─ NIVEL 4 (Transacciones): 6 casos
├─ NIVEL 5 (Edge Cases): 8 casos
└─ NIVEL 6 (Performance): 4 casos
```

---

## 🔷 NIVEL 1: VALIDACIÓN ARCHIVO (5 casos)

### **TC-1.1: Archivo con extensión incorrecta**
```
Descripción: Usuario sube .xls en lugar de .xlsx
Datos entrada: archivo.xls (5 MB válido)
Esperado: ❌ RECHAZAR
Mensaje: "El archivo no es Excel (.xlsx). Descarga la plantilla oficial."
Validación: Verificar extensión en objeto File
```

### **TC-1.2: Archivo vacío**
```
Descripción: Usuario sube archivo vacío
Datos entrada: archivo.xlsx (0 bytes)
Esperado: ❌ RECHAZAR
Mensaje: "Archivo corrupto o no es Excel válido"
Validación: Verificar tamaño > 0
```

### **TC-1.3: Archivo > 10 MB**
```
Descripción: Usuario sube archivo muy grande
Datos entrada: archivo.xlsx (15 MB)
Esperado: ❌ RECHAZAR
Mensaje: "El archivo supera 10 MB. Máximo permitido: 10 MB. Tu archivo: 15.00 MB"
Validación: Verificar tamaño <= 10 * 1024 * 1024
```

### **TC-1.4: Archivo corrupto (no es Excel válido)**
```
Descripción: Usuario sube archivo .xlsx que no es Excel (ej: imagen renombrada)
Datos entrada: imagen.jpg renombrado a .xlsx
Esperado: ❌ RECHAZAR
Mensaje: "Archivo corrupto o no es Excel válido. Intenta descargarlo nuevamente."
Validación: Intentar abrir con OpenXML, capturar excepción
```

### **TC-1.5: Archivo .csv en lugar de .xlsx**
```
Descripción: Usuario sube .csv (no se acepta formato antiguo)
Datos entrada: datos.csv
Esperado: ❌ RECHAZAR
Mensaje: "El archivo no es Excel (.xlsx)"
Validación: Verificar extension exacta
```

---

## 🔷 NIVEL 2: VALIDACIÓN ESTRUCTURA (8 casos)

### **TC-2.1: Faltan hojas (solo PRODUCTOS)**
```
Descripción: Usuario sube archivo con solo hoja PRODUCTOS
Datos entrada: archivo.xlsx (solo 1 hoja)
Esperado: ❌ RECHAZAR TODO
Mensaje: "Faltan las hojas: INVENTARIO, VENTAS, FINANCIEROS. Hojas requeridas: PRODUCTOS, INVENTARIO, VENTAS, FINANCIEROS"
Validación: Verificar workbook.GetSheetNames()
```

### **TC-2.2: Hojas duplicadas (PRODUCTOS aparece 2 veces)**
```
Descripción: Usuario duplicó accidentalmente hoja PRODUCTOS
Datos entrada: archivo.xlsx (PRODUCTOS x2, INVENTARIO, VENTAS, FINANCIEROS)
Esperado: ❌ RECHAZAR TODO
Mensaje: "Hoja PRODUCTOS duplicada. Solo se acepta una de cada tipo."
Validación: Contar ocurrencias de cada nombre de hoja
```

### **TC-2.3: Hoja renombrada (PROD en lugar de PRODUCTOS)**
```
Descripción: Usuario renombró hoja PRODUCTOS a PROD
Datos entrada: archivo.xlsx (PROD, INVENTARIO, VENTAS, FINANCIEROS)
Esperado: ❌ RECHAZAR TODO
Mensaje: "Falta la hoja 'PRODUCTOS'"
Validación: Case-sensitive en nombres
```

### **TC-2.4: Falta columna obligatoria (precio_venta en PRODUCTOS)**
```
Descripción: Usuario eliminó columna precio_venta
Datos entrada: PRODUCTOS sin precio_venta
Esperado: ❌ RECHAZAR TODO
Mensaje: "Hoja 'PRODUCTOS': Faltan columnas obligatorias: precio_venta. Columnas encontradas: codigo_producto, nombre, ..."
Validación: Verificar presencia en encabezados
```

### **TC-2.5: Hoja vacía (INVENTARIO sin datos)**
```
Descripción: Usuario creó INVENTARIO pero no agregó filas de datos
Datos entrada: INVENTARIO solo encabezado (fila 1)
Esperado: ❌ RECHAZAR TODO (o permitir vacío si es opcional)
Nota: Verificar si INVENTARIO es obligatoria o opcional
```

### **TC-2.6: Columnas en orden diferente**
```
Descripción: Usuario cambia orden de columnas (ej: nombre antes que codigo)
Datos entrada: PRODUCTOS con columnas reordenadas
Esperado: ✅ ACEPTAR (orden no importa, solo nombres)
Validación: Mapear por nombre, no por posición
```

### **TC-2.7: Columnas adicionales (no especificadas)**
```
Descripción: Usuario agrega columnas extras (ej: "descuento_proveedor")
Datos entrada: PRODUCTOS + columnas extras
Esperado: ✅ ACEPTAR (ignorar columnas extra)
Validación: Solo procesar columnas conocidas
```

### **TC-2.8: Encabezado con mayúsculas/minúsculas inconsistentes**
```
Descripción: Encabezado es "CODIGO_PRODUCTO" en lugar de "codigo_producto"
Datos entrada: PRODUCTOS con encabezados en MAYÚSCULAS
Esperado: ✅ ACEPTAR (normalizar a minúsculas)
Validación: .ToLower() en comparación
```

---

## 🔷 NIVEL 3a: VALIDACIÓN PRODUCTOS (18 casos)

### **TC-3a.1: código_producto vacío**
```
Fila: 5
Columna: codigo_producto
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 5: Columna 'codigo_producto' está vacía. Esta columna es obligatoria."
```

### **TC-3a.2: nombre vacío**
```
Fila: 7
Columna: nombre
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 7: Columna 'nombre' está vacía. Esta columna es obligatoria."
```

### **TC-3a.3: precio_venta vacío**
```
Fila: 10
Columna: precio_venta
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 10: Columna 'precio_venta' está vacía. Esta columna es obligatoria."
```

### **TC-3a.4: código_producto con espacios**
```
Fila: 3
Columna: codigo_producto
Valor: "PANT 001"
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 3: 'codigo_producto' = 'PANT 001'. Debe ser alfanumérico sin espacios."
```

### **TC-3a.5: código_producto empieza con número**
```
Fila: 4
Columna: codigo_producto
Valor: "001PANT"
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 4: 'codigo_producto' = '001PANT'. No puede iniciar con número."
```

### **TC-3a.6: código_producto duplicado en carga**
```
Fila 2: PANT-001
Fila 8: PANT-001
Esperado: ❌ RECHAZAR FILA 8
Error: "Hoja PRODUCTOS: Código 'PANT-001' está duplicado en filas 2 y 8. Los códigos deben ser únicos."
```

### **TC-3a.7: precio_venta = 0**
```
Fila: 6
Columna: precio_venta
Valor: 0
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 6: 'precio_venta' = 0. Debe ser mayor a 0."
```

### **TC-3a.8: precio_venta negativo**
```
Fila: 9
Columna: precio_venta
Valor: -150
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 9: 'precio_venta' = -150. Debe ser mayor a 0."
```

### **TC-3a.9: precio_venta NO es número**
```
Fila: 12
Columna: precio_venta
Valor: "ciento cincuenta"
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 12: 'precio_venta' = 'ciento cincuenta'. Debe ser número."
```

### **TC-3a.10: costo_unitario > precio_venta**
```
Fila: 5
precio_venta: 100
costo_unitario: 150
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 5: 'costo_unitario' (150) no puede ser mayor que 'precio_venta' (100)."
```

### **TC-3a.11: unidad_medida no en lista permitida**
```
Fila: 8
Columna: unidad_medida
Valor: "bulto"
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 8: 'unidad_medida' = 'bulto'. Debe estar en lista permitida: unidad, kg, gramo, ..."
```

### **TC-3a.12: es_servicio=VERDADERO + requiere_inventario=VERDADERO**
```
Fila: 11
es_servicio: VERDADERO
requiere_inventario: VERDADERO
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 11: Si es_servicio=VERDADERO, requiere_inventario debe ser FALSO."
```

### **TC-3a.13: categoria no en lista permitida (solo advertencia)**
```
Fila: 3
Columna: categoria
Valor: "Accesorios Deportivos"
Esperado: ⚠️ PERMITIR + ADVERTENCIA
Advertencia: "Hoja PRODUCTOS, fila 3: Categoría 'Accesorios Deportivos' no reconocida. Será registrada pero sin clasificación."
```

### **TC-3a.14: código_producto ya existe en BD (update, no error)**
```
Fila: 2
codigo_producto: PANT-001
Dato: Ya existe en BD con precio_venta anterior = 79,500
Nuevo valor: precio_venta = 89,500
Esperado: ✅ ACTUALIZAR (no error, solo advertencia)
Advertencia: "Código 'PANT-001' ya existe en BD. Será actualizado con nuevos valores."
```

### **TC-3a.15: Nombre muy largo (> 255 caracteres)**
```
Fila: 4
Columna: nombre
Valor: "Lorem ipsum..." (256+ caracteres)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 4: 'nombre' excede 255 caracteres (encontrados: 300)"
```

### **TC-3a.16: Precio con formato monetario**
```
Fila: 7
Columna: precio_venta
Valor: "$ 89.500"
Esperado: ✅ ACEPTAR (normalizar a 89500)
Validación: ParseCurrency() debe manejar $, COP, puntos, comas
```

### **TC-3a.17: Costo vacío (es opcional)**
```
Fila: 2
Columna: costo_unitario
Valor: (vacío)
Esperado: ✅ ACEPTAR (permite NULL)
Validación: Columna es opcional
```

### **TC-3a.18: Descripción muy larga (> 1000 caracteres)**
```
Fila: 9
Columna: descripcion
Valor: "Lorem ipsum..." (1500 caracteres)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja PRODUCTOS, fila 9: 'descripcion' excede 1000 caracteres"
```

---

## 🔷 NIVEL 3b: VALIDACIÓN INVENTARIO (12 casos)

### **TC-3b.1: código_producto no existe en PRODUCTOS**
```
Fila: 5
Columna: codigo_producto
Valor: "PROD-999"
PRODUCTOS contiene: PANT-001, CAMI-002, ...
Esperado: ❌ RECHAZAR FILA
Error: "Hoja INVENTARIO, fila 5: Código 'PROD-999' no existe en hoja PRODUCTOS."
```

### **TC-3b.2: cantidad_disponible negativa**
```
Fila: 3
Columna: cantidad_disponible
Valor: -10
Esperado: ❌ RECHAZAR FILA
Error: "Hoja INVENTARIO, fila 3: 'cantidad_disponible' = -10. Debe ser >= 0."
```

### **TC-3b.3: cantidad_reservada > cantidad_disponible**
```
Fila: 8
cantidad_disponible: 20
cantidad_reservada: 25
Esperado: ❌ RECHAZAR FILA
Error: "Hoja INVENTARIO, fila 8: 'cantidad_reservada' (25) no puede ser mayor que 'cantidad_disponible' (20)."
```

### **TC-3b.4: stock_maximo < stock_minimo**
```
Fila: 6
stock_minimo: 50
stock_maximo: 30
Esperado: ❌ RECHAZAR FILA
Error: "Hoja INVENTARIO, fila 6: 'stock_maximo' (30) no puede ser menor que 'stock_minimo' (50)."
```

### **TC-3b.5: fecha_vencimiento es pasada**
```
Fila: 4
Columna: fecha_vencimiento
Valor: "2024-01-01" (pasada)
Hoy: 2025-01-27
Esperado: ⚠️ PERMITIR + ADVERTENCIA
Advertencia: "Hoja INVENTARIO, fila 4: Producto ya vencido. Fecha de vencimiento: 2024-01-01"
```

### **TC-3b.6: cantidad_disponible vacío**
```
Fila: 7
Columna: cantidad_disponible
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja INVENTARIO, fila 7: Columna 'cantidad_disponible' está vacía. Esta columna es obligatoria."
```

### **TC-3b.7: cantidad con decimales (debe ser entero)**
```
Fila: 2
Columna: cantidad_disponible
Valor: 45.5
Esperado: ❌ RECHAZAR FILA
Error: "Hoja INVENTARIO, fila 2: 'cantidad_disponible' = 45.5. Debe ser número entero (sin decimales)."
```

### **TC-3b.8: Producto para el cual requiere_inventario=FALSE**
```
Fila: 10
codigo_producto: SVC-001 (Servicio, requiere_inventario=FALSE)
Esperado: ⚠️ PERMITIR + ADVERTENCIA (o SALTAR)
Advertencia: "Hoja INVENTARIO, fila 10: Código 'SVC-001' es un servicio. No se necesita seguimiento de inventario."
```

### **TC-3b.9: código_producto duplicado en INVENTARIO**
```
Fila 2: PANT-001
Fila 9: PANT-001
Esperado: ❌ RECHAZAR FILA 9
Error: "Hoja INVENTARIO: Código 'PANT-001' está duplicado en filas 2 y 9. Cada producto debe tener una sola fila."
```

### **TC-3b.10: Fecha vencimiento futura válida**
```
Fila: 5
Columna: fecha_vencimiento
Valor: "2026-12-31"
Esperado: ✅ ACEPTAR
```

### **TC-3b.11: Ubicación con caracteres especiales**
```
Fila: 3
Columna: ubicacion
Valor: "Almacén #2 - Pasillo A/B"
Esperado: ✅ ACEPTAR (permitir caracteres especiales en ubicación)
```

### **TC-3b.12: Pasillo con espacios**
```
Fila: 6
Columna: pasillo
Valor: "A 1"
Esperado: ❌ RECHAZAR FILA (o NORMALIZAR a "A1")
Error: "Hoja INVENTARIO, fila 6: 'pasillo' debe ser alfanumérico sin espacios"
```

---

## 🔷 NIVEL 3c: VALIDACIÓN VENTAS (14 casos)

### **TC-3c.1: número_orden duplicado en carga**
```
Fila 3: ORD-001
Fila 12: ORD-001
Esperado: ❌ RECHAZAR FILA 12
Error: "Hoja VENTAS: Número de orden 'ORD-001' está duplicado en filas 3 y 12."
```

### **TC-3c.2: número_orden vacío**
```
Fila: 5
Columna: numero_orden
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 5: Columna 'numero_orden' está vacía. Esta columna es obligatoria."
```

### **TC-3c.3: fecha_venta es futura**
```
Fila: 8
Columna: fecha_venta
Valor: "2025-12-31" (futuro)
Hoy: 2025-01-27
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 8: fecha_venta = 2025-12-31 (futura). No se permiten fechas futuras."
```

### **TC-3c.4: monto_total inconsistente con cálculo**
```
Fila: 6
monto_subtotal: 150,000
monto_descuento: 15,000
monto_impuestos: 21,600
monto_total: 156,700 (DEBERÍA ser 156,600)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 6: monto_total (156,700) ≠ subtotal (150,000) - descuento (15,000) + impuestos (21,600). Esperado: 156,600"
```

### **TC-3c.5: monto_descuento > monto_subtotal**
```
Fila: 4
monto_subtotal: 100,000
monto_descuento: 120,000
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 4: monto_descuento (120,000) no puede ser mayor que monto_subtotal (100,000)."
```

### **TC-3c.6: metodo_pago no válido**
```
Fila: 10
Columna: metodo_pago
Valor: "bitcoin"
Permitidos: efectivo, tarjeta, transferencia, credito, cheque
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 10: metodo_pago = 'bitcoin'. Debe ser uno de: efectivo, tarjeta, transferencia, credito, cheque"
```

### **TC-3c.7: cliente_email inválido**
```
Fila: 7
Columna: cliente_email
Valor: "juan@.com"
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 7: cliente_email = 'juan@.com'. Formato email inválido."
```

### **TC-3c.8: monto_total vacío**
```
Fila: 9
Columna: monto_total
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 9: Columna 'monto_total' está vacía. Esta columna es obligatoria."
```

### **TC-3c.9: código_producto no existe en detalle**
```
Fila: 12
Detalle: codigo_producto_1 = "PROD-999"
PRODUCTOS contiene: PANT-001, CAMI-002, ...
Esperado: ⚠️ ADVERTENCIA (saltar línea, no error total)
Advertencia: "Hoja VENTAS, fila 12: Código 'PROD-999' no existe en PRODUCTOS. Línea del producto ignorada."
```

### **TC-3c.10: Cantidad en detalle <= 0**
```
Fila: 5
Detalle: cantidad_1 = 0
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 5: Cantidad en detalle debe ser > 0"
```

### **TC-3c.11: cliente_nombre vacío**
```
Fila: 3
Columna: cliente_nombre
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 3: Columna 'cliente_nombre' está vacía. Esta columna es obligatoria."
```

### **TC-3c.12: Venta con montos válidos pero con 1 centavo de diferencia**
```
Fila: 11
monto_total calculado: 156,600.50
monto_total ingresado: 156,600.51 (diferencia: 0.01)
Tolerancia: ±0.01
Esperado: ✅ ACEPTAR (dentro de tolerancia de redondeo)
```

### **TC-3c.13: Cliente email válido con caracteres especiales**
```
Fila: 6
Columna: cliente_email
Valor: "juan+promo@empresa.co"
Esperado: ✅ ACEPTAR (+ es válido en emails)
```

### **TC-3c.14: número_orden ya existe en BD (permitir overwrite?)**
```
Fila: 2
numero_orden: ORD-001
BD contiene: ORD-001 (de carga anterior)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja VENTAS, fila 2: número_orden 'ORD-001' ya existe en BD. No se permiten duplicados entre períodos."
Nota: Verificar si se permite overwrite
```

---

## 🔷 NIVEL 3d: VALIDACIÓN FINANCIEROS (12 casos)

### **TC-3d.1: tipo_dato no válido**
```
Fila: 5
Columna: tipo_dato
Valor: "ingreso_venta"
Permitidos: ingreso, gasto, costo, inversion
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 5: tipo_dato = 'ingreso_venta'. Debe ser: ingreso, gasto, costo, inversion"
```

### **TC-3d.2: categoria no válida para tipo_dato**
```
Fila: 8
tipo_dato: ingreso
categoria: "Salarios" (válido solo para gasto)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 8: categoría 'Salarios' no válida para tipo 'ingreso'. Válidas: Ventas, Servicios, ..."
```

### **TC-3d.3: monto <= 0**
```
Fila: 3
Columna: monto
Valor: 0
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 3: monto = 0. Debe ser mayor a 0."
```

### **TC-3d.4: monto negativo**
```
Fila: 6
Columna: monto
Valor: -50000
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 6: monto = -50000. Debe ser mayor a 0."
```

### **TC-3d.5: fecha_registro es futura**
```
Fila: 10
Columna: fecha_registro
Valor: "2025-12-31" (futura)
Hoy: 2025-01-27
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 10: fecha_registro = 2025-12-31 (futura). No se permiten fechas futuras."
```

### **TC-3d.6: fecha_pago < fecha_registro**
```
Fila: 7
fecha_registro: "2025-01-20"
fecha_pago: "2025-01-10" (anterior)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 7: fecha_pago (2025-01-10) no puede ser anterior a fecha_registro (2025-01-20)"
```

### **TC-3d.7: concepto vacío**
```
Fila: 4
Columna: concepto
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 4: Columna 'concepto' está vacía. Esta columna es obligatoria."
```

### **TC-3d.8: categoria vacía**
```
Fila: 9
Columna: categoria
Valor: (vacío)
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 9: Columna 'categoria' está vacía. Esta columna es obligatoria."
```

### **TC-3d.9: monto no es número**
```
Fila: 12
Columna: monto
Valor: "cincuenta mil"
Esperado: ❌ RECHAZAR FILA
Error: "Hoja FINANCIEROS, fila 12: monto = 'cincuenta mil'. Debe ser número."
```

### **TC-3d.10: moneda válida diferente a COP**
```
Fila: 5
Columna: moneda
Valor: "USD"
Esperado: ✅ ACEPTAR (aunque sea USD, se acepta)
Validación: Reconocer COP, USD, EUR, etc.
```

### **TC-3d.11: numero_comprobante duplicado**
```
Fila 3: numero_comprobante = "FAC-001"
Fila 10: numero_comprobante = "FAC-001"
Esperado: ❌ RECHAZAR FILA 10
Error: "Hoja FINANCIEROS: Comprobante 'FAC-001' está duplicado en filas 3 y 10."
```

### **TC-3d.12: Fecha válida, monto válido, todo ok**
```
Fila: 2
tipo_dato: ingreso
categoria: Ventas
concepto: "Venta diaria 15/01"
monto: 500,000
fecha_registro: "2025-01-15"
Esperado: ✅ ACEPTAR
```

---

## 🔷 NIVEL 4: TRANSACCIONES (6 casos)

### **TC-4.1: Carga exitosa (commit)**
```
Descripción: Archivo válido completo, todas las hojas con datos válidos
Esperado: ✅ COMMIT
Verificar:
- Productos insertados en tabla
- Inventarios insertados
- Ventas + detalles insertados
- Financieros insertados
- Importaciones_datos registrado
```

### **TC-4.2: Error en Fase 3 (ROLLBACK)**
```
Descripción: Archivo válido hasta VENTAS, pero error en monto_total inconsistente
Esperado: ❌ ROLLBACK
Verificar:
- NO se insertaron productos
- NO se insertaron inventarios
- BD sigue como antes del upload
- Importaciones_datos registra estado FALLIDO
```

### **TC-4.3: Error en Fase 4 (ROLLBACK)**
```
Descripción: Archivo válido hasta FINANCIEROS, error en categoría
Esperado: ❌ ROLLBACK
Verificar:
- TODOS los inserts se revierten
- BD intacta
```

### **TC-4.4: Actualización de producto existente**
```
Descripción: Carga con código_producto que ya existe en BD
Esperado: ✅ COMMIT + UPDATE (no INSERT)
Verificar:
- Registro updated_at actualizado
- Audit de cambios registrado
- Valor anterior guardado
```

### **TC-4.5: Transacción concurrente (2 uploads simultáneos)**
```
Descripción: Usuario A y Usuario B suben archivos al mismo tiempo
Aislamiento: SERIALIZABLE
Esperado: ✅ Una completa primero, luego la otra (no interferences)
Verificar:
- No hay corrupción de datos
- Ambas cargas completadas correctamente
```

### **TC-4.6: Carga fallida, reintento exitoso**
```
Descripción: Primer intento falla, usuario corrige y reintenta
Esperado: ✅ Segundo intento: COMMIT
Verificar:
- Importaciones_datos registra ambos intentos
- Resultado final: COMPLETADO
```

---

## 🔷 NIVEL 5: EDGE CASES (8 casos)

### **TC-5.1: Valor numérico con múltiples puntos y comas**
```
Entrada: "$ 1.234.567,50"
Esperado: ✅ ParseCurrency() → 1234567.50
```

### **TC-5.2: Fecha en múltiples formatos en mismo archivo**
```
Fila 3: "2025-01-15" (ISO)
Fila 5: "15/01/2025" (DD/MM)
Fila 7: "01/15/2025" (MM/DD)
Esperado: ✅ ACEPTAR todos (ParseDate normaliza)
```

### **TC-5.3: Boolean con variantes**
```
Fila 2: VERDADERO
Fila 5: true
Fila 8: V
Fila 12: Sí
Esperado: ✅ ACEPTAR todos = TRUE
```

### **TC-5.4: Texto con espacios antes/después**
```
Valor: "  Juan Pérez  " (espacios)
Esperado: ✅ NORMALIZAR → "Juan Pérez"
```

### **TC-5.5: Email con punto al final**
```
Valor: "juan@empresa.co." (punto extra)
Esperado: ❌ RECHAZAR (email inválido)
```

### **TC-5.6: Código con caracteres especiales**
```
Valor: "PANT-001_V2"
Esperado: ✅ ACEPTAR (alfanumérico + guión + guión bajo)
```

### **TC-5.7: Descripción con saltos de línea**
```
Valor: "Pantalón formal\nColor: negro\nTalla: 32"
Esperado: ✅ ACEPTAR (permitir multi-línea)
```

### **TC-5.8: Producto con precio muy alto**
```
Valor: 999,999,999.99 (máximo permitido)
Esperado: ✅ ACEPTAR
```

---

## 🔷 NIVEL 6: PERFORMANCE (4 casos)

### **TC-6.1: Archivo 1 MB (500 registros)**
```
Tiempo máximo: 5 segundos
Verificar:
- Carga completa
- Datos correctos
- Sin timeout
```

### **TC-6.2: Archivo 5 MB (2500 registros)**
```
Tiempo máximo: 20 segundos
Verificar:
- Carga completa
- Performance aceptable
- BD responde
```

### **TC-6.3: Archivo 10 MB (5000 registros)**
```
Tiempo máximo: 45 segundos
Verificar:
- Carga completa
- Performance límite pero aceptable
```

### **TC-6.4: Carga repetida (mismos datos 3 veces)**
```
Intento 1: Carga exitosa (INSERT)
Intento 2: Carga exitosa (UPDATE)
Intento 3: Carga exitosa (UPDATE)
Verificar:
- Cada intento respeta orden de procesamiento
- No hay degradación de performance
- Datos quedan consistentes
```

---

## 📊 MATRIZ DE EJECUCIÓN

```
PRIORIDAD ALTA (Ejecutar primero):
├─ TC-1.1, TC-1.3, TC-1.4      (Validación archivo)
├─ TC-2.1, TC-2.4, TC-2.5      (Validación estructura)
├─ TC-3a.1, TC-3a.4, TC-3a.6   (Validación PRODUCTOS)
├─ TC-3b.1, TC-3b.3            (Validación INVENTARIO)
├─ TC-3c.1, TC-3c.4, TC-3c.5   (Validación VENTAS)
├─ TC-3d.1, TC-3d.2, TC-3d.5   (Validación FINANCIEROS)
└─ TC-4.1, TC-4.2              (Transacciones críticas)

PRIORIDAD MEDIA:
├─ TC-1.2, TC-1.5
├─ TC-2.2, TC-2.3, TC-2.6, TC-2.7, TC-2.8
├─ TC-3a.2, TC-3a.3, TC-3a.7, TC-3a.8, TC-3a.9, TC-3a.10
└─ Resto de TC-3b, TC-3c, TC-3d

PRIORIDAD BAJA (Nice to have):
├─ TC-5.x (Edge cases especiales)
└─ TC-6.x (Performance)
```

---

## ✅ CHECKLIST QA

- [ ] Crear test automation en framework elegido (NUnit, xUnit, etc.)
- [ ] Generar archivos Excel de prueba para cada caso
- [ ] Ejecutar pruebas manuales primero
- [ ] Automatizar los 30 casos de Prioridad Alta + Media
- [ ] Crear reporte de cobertura
- [ ] Documentar bugs encontrados
- [ ] Verificar mensajes de error exactos
- [ ] Validar performance en BD con ~5000 registros
- [ ] Sign-off antes de deploy a producción
