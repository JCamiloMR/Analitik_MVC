# PSEUDOCÓDIGO COMPLETO - PIPELINE ETL CLARIDATA

**Versión:** 1.0  
**Target Language:** C# (fácil adaptación)  
**Paradigma:** Modular, layer-based, transaction-safe  
**Fecha:** Enero 2025  

---

## 📋 ÍNDICE DE FUNCIONES

1. **ENTRADA & VALIDACIÓN INICIAL**
   - ValidarArchivoExcel
   - ValidarEstructuraHojas
   - ValidarDimensionArchivo

2. **LECTURA & PARSEO**
   - LeerHojaExcel
   - MapearFilasAObjetos
   - NormalizarDatos

3. **VALIDACIÓN DE NEGOCIO**
   - ValidarProductos
   - ValidarInventario
   - ValidarVentas
   - ValidarFinancieros

4. **TRANSFORMACIÓN & IMPUTACIÓN**
   - TransformarFechas
   - NormalizarValoresMonetarios
   - NormalizarValoresNumericos
   - ImputarValoresFaltantes

5. **PROCESAMIENTO & MAPEO A BD**
   - MapearAEntidades
   - ResolverRelaciones
   - GenerarInserts

6. **CARGA ATÓMICA A BD**
   - IniciarTransaccion
   - InsertarProductos
   - InsertarInventario
   - InsertarVentas
   - InsertarFinancieros
   - CommitOuRollback

7. **MANEJO DE ERRORES & LOGS**
   - RegistrarError
   - RegistrarAdvertencia
   - GenerarReporteCargas

---

## 🔶 NIVEL 1: VALIDACIÓN INICIAL

### **Función: ValidarArchivoExcel**

```
ENTRADA:
  archivo: File (objeto del upload del usuario)
  empresaId: UUID

SALIDA:
  resultado: {
    exitoso: boolean,
    error?: string,
    archivoValidado?: FileStream
  }

LÓGICA:
  1. Verificar extensión
     SI archivo.extension ≠ ".xlsx"
       RETORNAR { 
         exitoso: false, 
         error: "El archivo no es Excel (.xlsx). Descarga la plantilla oficial."
       }
     FIN SI

  2. Verificar tamaño
     SI archivo.tamano > 10 * 1024 * 1024  // 10 MB
       RETORNAR {
         exitoso: false,
         error: "El archivo supera 10 MB. Máximo permitido: 10 MB. Tu archivo: " + 
                 (archivo.tamano / (1024 * 1024)).toFixed(2) + " MB."
       }
     FIN SI

  3. Verificar que es Excel válido
     INTENTA
       workbook = OpenWorkbook(archivo.stream)
     EXCEPCIÓN error
       RETORNAR {
         exitoso: false,
         error: "Archivo corrupto o no es Excel válido. Intenta descargarlo nuevamente."
       }
     FIN INTENTA

  4. Retornar éxito
     RETORNAR {
       exitoso: true,
       archivoValidado: archivo.stream
     }
FIN FUNCIÓN
```

---

### **Función: ValidarEstructuraHojas**

```
ENTRADA:
  workbook: ExcelWorkbook

SALIDA:
  resultado: {
    exitoso: boolean,
    hojas?: Map<string, boolean>,
    error?: string
  }

LÓGICA:
  1. Definir hojas requeridas
     hojas_esperadas = ["PRODUCTOS", "INVENTARIO", "VENTAS", "FINANCIEROS"]

  2. Obtener hojas existentes en workbook
     hojas_actuales = workbook.GetSheetNames()

  3. Validar presencia de hojas
     hojas_faltantes = []
     PARA CADA hoja EN hojas_esperadas
       SI hoja NO está EN hojas_actuales
         hojas_faltantes.Add(hoja)
       FIN SI
     FIN PARA

  4. Si faltan hojas
     SI hojas_faltantes.Count > 0
       RETORNAR {
         exitoso: false,
         error: "Faltan las hojas: " + JOIN(hojas_faltantes, ", ") + 
                ". Hojas requeridas: " + JOIN(hojas_esperadas, ", ")
       }
     FIN SI

  5. Validar encabezados de cada hoja
     PARA CADA hoja EN hojas_esperadas
       resultado_columnas = ValidarColumnasHoja(workbook.GetSheet(hoja), hoja)
       SI NO resultado_columnas.exitoso
         RETORNAR resultado_columnas
       FIN SI
     FIN PARA

  6. Retornar éxito
     RETORNAR { exitoso: true }
FIN FUNCIÓN
```

---

### **Función: ValidarColumnasHoja**

```
ENTRADA:
  hoja: ExcelSheet,
  nombreHoja: string

SALIDA:
  resultado: {
    exitoso: boolean,
    columnasObligatorias?: string[],
    columnasEncontradas?: string[],
    error?: string
  }

LÓGICA:
  1. Definir columnas obligatorias por hoja
     SELECCIONAR nombreHoja
       CASO "PRODUCTOS":
         columnas_obligatorias = [
           "codigo_producto", "nombre", "precio_venta", 
           "unidad_medida", "requiere_inventario", "activo"
         ]
       
       CASO "INVENTARIO":
         columnas_obligatorias = [
           "codigo_producto", "cantidad_disponible"
         ]
       
       CASO "VENTAS":
         columnas_obligatorias = [
           "numero_orden", "fecha_venta", "cliente_nombre",
           "monto_total", "metodo_pago"
         ]
       
       CASO "FINANCIEROS":
         columnas_obligatorias = [
           "tipo_dato", "categoria", "concepto", "monto", "fecha_registro"
         ]
     FIN SELECCIONAR

  2. Obtener encabezados de la hoja
     encabezados = hoja.GetRow(1).GetValues()
     encabezados_normalizados = encabezados.ToLower().Trim()

  3. Validar presencia de obligatorias
     columnas_faltantes = []
     PARA CADA col EN columnas_obligatorias
       SI col NO está EN encabezados_normalizados
         columnas_faltantes.Add(col)
       FIN SI
     FIN PARA

  4. Si faltan columnas
     SI columnas_faltantes.Count > 0
       RETORNAR {
         exitoso: false,
         error: "Hoja '" + nombreHoja + "': Faltan columnas obligatorias: " + 
                JOIN(columnas_faltantes, ", ") + 
                ". Columnas encontradas: " + JOIN(encabezados, ", ")
       }
     FIN SI

  5. Validar que hoja no esté vacía
     SI hoja.GetRowCount() <= 1  // solo encabezado
       RETORNAR {
         exitoso: false,
         error: "Hoja '" + nombreHoja + "' no contiene datos."
       }
     FIN SI

  6. Retornar éxito
     RETORNAR { exitoso: true }
FIN FUNCIÓN
```

---

## 🔶 NIVEL 2: LECTURA & PARSEO

### **Función: LeerYMapearProductos**

```
ENTRADA:
  hoja_productos: ExcelSheet,
  empresaId: UUID

SALIDA:
  {
    exitoso: boolean,
    productos: List<ProductoDTO>,
    errores: List<ErrorValidacion>,
    advertencias: List<string>
  }

DONDE ProductoDTO = {
  codigo_producto: string,
  nombre: string,
  categoria: string?,
  subcategoria: string?,
  marca: string?,
  modelo: string?,
  precio_venta: decimal,
  costo_unitario: decimal?,
  precio_sugerido: decimal?,
  unidad_medida: string,
  peso_kg: decimal?,
  volumen_m3: decimal?,
  codigo_barras: string?,
  codigo_qr: string?,
  es_servicio: boolean,
  requiere_inventario: boolean,
  activo: boolean,
  descripcion: string?,
  empresaId: UUID,
  fila_origen: int
}

LÓGICA:
  1. Inicializar colecciones
     productos = []
     errores = []
     advertencias = []
     codigos_vistos = Set<string>()

  2. Obtener encabezados y crear índices
     encabezados = hoja.GetRow(1).ToLower()
     idx_codigo = encabezados.IndexOf("codigo_producto")
     idx_nombre = encabezados.IndexOf("nombre")
     idx_categoria = encabezados.IndexOf("categoria")
     // ... resto de índices
     
  3. Iterar filas (desde fila 2)
     PARA fila = 2 A hoja.GetRowCount()
       
       3a. Leer valores de celda
           codigo = hoja[fila, idx_codigo].Value?.ToString().Trim()
           nombre = hoja[fila, idx_nombre].Value?.ToString().Trim()
           precio_venta = hoja[fila, idx_precio_venta].Value
           // ... resto de campos

       3b. VALIDACIÓN: Campos obligatorios vacíos
           SI codigo.IsNullOrEmpty()
             errores.Add({
               fila: fila,
               columna: "codigo_producto",
               error: "Campo obligatorio vacío",
               valor_encontrado: codigo
             })
             CONTINUAR  // Saltar esta fila
           FIN SI

           SI nombre.IsNullOrEmpty()
             errores.Add({
               fila: fila,
               columna: "nombre",
               error: "Campo obligatorio vacío"
             })
             CONTINUAR
           FIN SI

           SI precio_venta.IsNullOrEmpty() OR NO EsNumero(precio_venta)
             errores.Add({
               fila: fila,
               columna: "precio_venta",
               error: "Debe ser número mayor a 0",
               valor_encontrado: precio_venta
             })
             CONTINUAR
           FIN SI

       3c. VALIDACIÓN: Formato códigos
           SI NOT EsAlfanumerico(codigo) OR codigo.StartsWith(numero)
             errores.Add({
               fila: fila,
               columna: "codigo_producto",
               error: "Debe ser alfanumérico, sin espacios, sin iniciar con número",
               valor_encontrado: codigo
             })
             CONTINUAR
           FIN SI

       3d. VALIDACIÓN: Duplicados en carga
           SI codigo EN codigos_vistos
             errores.Add({
               fila: fila,
               columna: "codigo_producto",
               error: "Código duplicado. Ya aparece en fila anterior.",
               valor_encontrado: codigo
             })
             CONTINUAR
           FIN SI
           codigos_vistos.Add(codigo)

       3e. VALIDACIÓN: Rango valores numéricos
           precio_venta_num = ParseDecimal(precio_venta)
           SI precio_venta_num <= 0
             errores.Add({
               fila: fila,
               columna: "precio_venta",
               error: "Debe ser mayor a 0",
               valor_encontrado: precio_venta
             })
             CONTINUAR
           FIN SI

           SI costo_unitario NO NULL
             costo_num = ParseDecimal(costo_unitario)
             SI costo_num > precio_venta_num
               errores.Add({
                 fila: fila,
                 columna: "costo_unitario",
                 error: "No puede ser mayor que precio_venta",
                 valor_encontrado: costo_unitario + " (precio: " + precio_venta + ")"
               })
               CONTINUAR
             FIN SI
           FIN SI

       3f. VALIDACIÓN: Categoría en lista permitida
           SI categoria NO NULL
             categorias_permitidas = ["Uniformes", "Casual", "Formal", "Deportivo", ...]
             SI categoria NO EN categorias_permitidas
               advertencias.Add("Fila " + fila + ": Categoría '" + categoria + 
                               "' no reconocida. Será registrada pero sin clasificación.")
             FIN SI
           FIN SI

       3g. VALIDACIÓN: Unidad de medida válida
           unidades_permitidas = ["unidad", "kg", "gramo", "metro", "litro", ...]
           SI unidad_medida NO EN unidades_permitidas
             errores.Add({
               fila: fila,
               columna: "unidad_medida",
               error: "Debe estar en lista permitida: " + JOIN(unidades_permitidas),
               valor_encontrado: unidad_medida
             })
             CONTINUAR
           FIN SI

       3h. VALIDACIÓN: Coherencia lógica
           es_servicio = ParseBoolean(hoja[fila, idx_es_servicio])
           requiere_inventario = ParseBoolean(hoja[fila, idx_requiere_inventario])
           
           SI es_servicio = true AND requiere_inventario = true
             errores.Add({
               fila: fila,
               error: "Si es_servicio=VERDADERO, requiere_inventario debe ser FALSO"
             })
             CONTINUAR
           FIN SI

       3i. Transformar datos
           // Normalizar espacios
           codigo = codigo.ToUpper()
           nombre = NormalizarTexto(nombre)

           // Parsear números
           precio_venta_decimal = ParseDecimal(precio_venta)
           costo_unitario_decimal = SI costo_unitario? ENTONCES ParseDecimal(costo_unitario) SINO NULL
           peso_kg_decimal = SI peso_kg? ENTONCES ParseDecimal(peso_kg) SINO NULL

           // Valores booleanos
           requiere_inventario_bool = ParseBoolean(requiere_inventario)
           activo_bool = ParseBoolean(activo)
           es_servicio_bool = ParseBoolean(es_servicio)

       3j. Crear DTO
           producto = NEW ProductoDTO {
             codigo_producto = codigo,
             nombre = nombre,
             categoria = categoria,
             subcategoria = subcategoria,
             marca = marca?.Trim(),
             modelo = modelo?.Trim(),
             precio_venta = precio_venta_decimal,
             costo_unitario = costo_unitario_decimal,
             precio_sugerido = SI precio_sugerido? ENTONCES ParseDecimal(precio_sugerido) SINO NULL,
             unidad_medida = unidad_medida,
             peso_kg = peso_kg_decimal,
             volumen_m3 = SI volumen_m3? ENTONCES ParseDecimal(volumen_m3) SINO NULL,
             codigo_barras = codigo_barras?.Trim(),
             codigo_qr = codigo_qr?.Trim(),
             es_servicio = es_servicio_bool,
             requiere_inventario = requiere_inventario_bool,
             activo = activo_bool,
             descripcion = descripcion?.Trim(),
             empresaId = empresaId,
             fila_origen = fila
           }

           productos.Add(producto)

     FIN PARA (siguiente fila)

  4. VALIDACIÓN POST-LECTURA: Verificar duplicados por código en BD
     codigos_existentes = BD.Productos
       .Where(p => p.empresaId = empresaId AND codigos_en_carga.Contains(p.codigo))
       .Select(p => p.codigo)
       .ToList()
     
     PARA CADA codigo EN codigos_existentes
       advertencias.Add("Código '" + codigo + "' ya existe en BD. Será actualizado.")
     FIN PARA

  5. Retornar resultado
     SI errores.Count > 0
       RETORNAR {
         exitoso: false,
         productos: [],
         errores: errores
       }
     FIN SI

     RETORNAR {
       exitoso: true,
       productos: productos,
       errores: [],
       advertencias: advertencias
     }
FIN FUNCIÓN
```

---

### **Función: LeerYMapearInventario**

```
ENTRADA:
  hoja_inventario: ExcelSheet,
  productos_cargados: List<ProductoDTO>,
  empresaId: UUID

SALIDA:
  {
    exitoso: boolean,
    inventarios: List<InventarioDTO>,
    errores: List<ErrorValidacion>,
    advertencias: List<string>
  }

DONDE InventarioDTO = {
  codigo_producto: string,
  cantidad_disponible: int,
  cantidad_reservada: int?,
  cantidad_en_transito: int?,
  stock_minimo: int?,
  stock_maximo: int?,
  punto_reorden: int?,
  ubicacion: string?,
  pasillo: string?,
  estante: string?,
  nivel: string?,
  lote_actual: string?,
  fecha_vencimiento: DateTime?,
  dias_alerta_vencimiento: int?,
  empresaId: UUID,
  fila_origen: int
}

LÓGICA:
  1. Inicializar colecciones
     inventarios = []
     errores = []
     advertencias = []

  2. Crear índice de códigos válidos
     codigos_validos = productos_cargados
       .Where(p => p.requiere_inventario = true)
       .Select(p => p.codigo_producto)
       .ToHashSet()

  3. Iterar filas (desde fila 2)
     PARA fila = 2 A hoja_inventario.GetRowCount()
       
       3a. Leer valores
           codigo = hoja[fila, idx_codigo].Value?.ToString().Trim().ToUpper()
           cantidad_disponible = hoja[fila, idx_cantidad].Value
           cantidad_reservada = hoja[fila, idx_reservada].Value?
           stock_minimo = hoja[fila, idx_stock_minimo].Value?
           stock_maximo = hoja[fila, idx_stock_maximo].Value?
           fecha_vencimiento = hoja[fila, idx_fecha_vto].Value?
           // ... resto

       3b. VALIDACIÓN: Código obligatorio
           SI codigo.IsNullOrEmpty()
             errores.Add({fila, "codigo_producto", "Campo obligatorio vacío"})
             CONTINUAR
           FIN SI

       3c. VALIDACIÓN: Código existe en PRODUCTOS
           SI codigo NO EN codigos_validos
             INTENTA
               // Buscar en BD por si fue creado antes
               prod_existente = BD.Productos
                 .FirstOrDefault(p => p.codigo = codigo AND p.empresaId = empresaId)
               SI prod_existente = NULL
                 errores.Add({
                   fila,
                   "codigo_producto",
                   "Código '" + codigo + "' no existe en hoja PRODUCTOS"
                 })
                 CONTINUAR
               FIN SI
             EXCEPCIÓN
               errores.Add({fila, "codigo_producto", "Error validando código"})
               CONTINUAR
             FIN INTENTA
           FIN SI

       3d. VALIDACIÓN: Cantidad disponible
           SI cantidad_disponible.IsNullOrEmpty() OR NOT EsNumero(cantidad_disponible)
             errores.Add({
               fila,
               "cantidad_disponible",
               "Debe ser número entero >= 0"
             })
             CONTINUAR
           FIN SI
           
           cantidad_num = ParseInt(cantidad_disponible)
           SI cantidad_num < 0
             errores.Add({
               fila,
               "cantidad_disponible",
               "No puede ser negativo"
             })
             CONTINUAR
           FIN SI

       3e. VALIDACIÓN: Cantidad reservada <= disponible
           SI cantidad_reservada NOT NULL
             cant_res_num = ParseInt(cantidad_reservada)
             SI cant_res_num > cantidad_num
               errores.Add({
                 fila,
                 "cantidad_reservada",
                 "No puede ser mayor que cantidad_disponible (" + cantidad_num + ")"
               })
               CONTINUAR
             FIN SI
           FIN SI

       3f. VALIDACIÓN: Stock máximo >= stock mínimo
           SI stock_maximo NOT NULL AND stock_minimo NOT NULL
             SI ParseInt(stock_maximo) < ParseInt(stock_minimo)
               errores.Add({
                 fila,
                 "stock_maximo",
                 "No puede ser menor que stock_minimo"
               })
               CONTINUAR
             FIN SI
           FIN SI

       3g. VALIDACIÓN: Fecha vencimiento
           SI fecha_vencimiento NOT NULL
             fecha_vto_parsed = ParseDate(fecha_vencimiento)
             SI fecha_vto_parsed < HOY()
               advertencias.Add("Fila " + fila + ": Producto '" + codigo + 
                               "' ya vencido (vencimiento: " + fecha_vto_parsed + ")")
             FIN SI
           FIN SI

       3h. Normalizar y crear DTO
           inventario = NEW InventarioDTO {
             codigo_producto = codigo,
             cantidad_disponible = ParseInt(cantidad_disponible),
             cantidad_reservada = cantidad_reservada? ParseInt(cantidad_reservada) : 0,
             cantidad_en_transito = cantidad_en_transito? ParseInt(cantidad_en_transito) : 0,
             stock_minimo = stock_minimo? ParseInt(stock_minimo) : 0,
             stock_maximo = stock_maximo? ParseInt(stock_maximo) : NULL,
             punto_reorden = punto_reorden? ParseInt(punto_reorden) : NULL,
             ubicacion = ubicacion?.Trim(),
             pasillo = pasillo?.Trim()?.ToUpper(),
             estante = estante?.Trim()?.ToUpper(),
             nivel = nivel?.Trim(),
             lote_actual = lote_actual?.Trim(),
             fecha_vencimiento = fecha_vencimiento? ParseDate(fecha_vencimiento) : NULL,
             dias_alerta_vencimiento = dias_alerta? ParseInt(dias_alerta) : 30,
             empresaId = empresaId,
             fila_origen = fila
           }

           inventarios.Add(inventario)

     FIN PARA

  4. Retornar resultado
     SI errores.Count > 0
       RETORNAR { exitoso: false, errores: errores }
     FIN SI

     RETORNAR {
       exitoso: true,
       inventarios: inventarios,
       advertencias: advertencias
     }
FIN FUNCIÓN
```

---

### **Función: LeerYMapearVentas**

```
ENTRADA:
  hoja_ventas: ExcelSheet,
  productos_cargados: List<ProductoDTO>,
  empresaId: UUID

SALIDA:
  {
    exitoso: boolean,
    ventas: List<VentaDTO>,
    detalles_venta: List<DetalleVentaDTO>,
    errores: List<ErrorValidacion>,
    advertencias: List<string>
  }

DONDE VentaDTO = {
  numero_orden: string,
  numero_factura: string?,
  fecha_venta: DateTime,
  cliente_nombre: string,
  cliente_documento: string?,
  cliente_telefono: string?,
  cliente_email: string?,
  cliente_direccion: string?,
  ciudad: string?,
  monto_subtotal: decimal,
  monto_descuento: decimal,
  monto_impuestos: decimal,
  monto_total: decimal,
  metodo_pago: enum,
  estado_pago: string,
  vendedor: string?,
  canal_venta: string?,
  estado: enum,
  notas: string?,
  empresaId: UUID,
  fila_origen: int
}

DONDE DetalleVentaDTO = {
  numero_orden: string,
  codigo_producto: string,
  cantidad: decimal,
  precio_unitario: decimal,
  descuento_porcentaje: decimal?,
  subtotal: decimal,
  empresaId: UUID,
  fila_origen: int
}

LÓGICA:
  1. Inicializar
     ventas = []
     detalles = []
     errores = []
     advertencias = []
     ordenes_vistas = Set<string>()
     codigos_validos = productos_cargados.Select(p => p.codigo_producto).ToHashSet()
     metodos_permitidos = ["efectivo", "tarjeta", "transferencia", "credito", "cheque"]

  2. PARA fila = 2 A hoja_ventas.GetRowCount()
       
       2a. Leer valores encabezado orden
           numero_orden = hoja[fila, idx_orden].Value?.ToString().Trim().ToUpper()
           fecha_venta = hoja[fila, idx_fecha].Value
           cliente_nombre = hoja[fila, idx_cliente].Value?.ToString().Trim()
           monto_subtotal = hoja[fila, idx_subtotal].Value
           monto_descuento = hoja[fila, idx_descuento].Value?
           monto_impuestos = hoja[fila, idx_impuestos].Value?
           monto_total = hoja[fila, idx_total].Value
           metodo_pago = hoja[fila, idx_metodo].Value?.ToString().Trim().ToLower()
           // ... resto de campos

       2b. VALIDACIONES OBLIGATORIAS
           errores_fila = ValidarOrdenVenta(
             numero_orden, fecha_venta, cliente_nombre,
             monto_total, metodo_pago, fila
           )
           SI errores_fila.Count > 0
             errores.AddRange(errores_fila)
             CONTINUAR
           FIN SI

       2c. VALIDACIÓN: Orden única
           SI numero_orden EN ordenes_vistas
             errores.Add({
               fila,
               "numero_orden",
               "Número de orden duplicado: " + numero_orden
             })
             CONTINUAR
           FIN SI
           ordenes_vistas.Add(numero_orden)

       2d. VALIDACIÓN: Método pago válido
           SI metodo_pago NO EN metodos_permitidos
             errores.Add({
               fila,
               "metodo_pago",
               "Debe ser uno de: " + JOIN(metodos_permitidos),
               valor_encontrado: metodo_pago
             })
             CONTINUAR
           FIN SI

       2e. VALIDACIÓN: Montos coherentes
           monto_sub = ParseDecimal(monto_subtotal)
           monto_desc = monto_descuento? ParseDecimal(monto_descuento) : 0
           monto_imp = monto_impuestos? ParseDecimal(monto_impuestos) : 0
           monto_tot = ParseDecimal(monto_total)

           monto_calculado = monto_sub - monto_desc + monto_imp

           SI ABS(monto_tot - monto_calculado) > 0.01  // tolerancia 1 centavo
             errores.Add({
               fila,
               "monto_total",
               "Inconsistencia: total (" + monto_tot + ") ≠ subtotal (" + monto_sub + 
               ") - descuento (" + monto_desc + ") + impuestos (" + monto_imp + ")",
               formula: "Esperado = " + monto_calculado
             })
             CONTINUAR
           FIN SI

           SI monto_desc > monto_sub
             errores.Add({
               fila,
               "monto_descuento",
               "No puede ser mayor que monto_subtotal"
             })
             CONTINUAR
           FIN SI

       2f. VALIDACIÓN: Email válido (si se proporciona)
           SI cliente_email NOT NULL
             SI NOT EsEmailValido(cliente_email)
               errores.Add({
                 fila,
                 "cliente_email",
                 "Formato email inválido",
                 valor_encontrado: cliente_email
               })
               CONTINUAR
             FIN SI
           FIN SI

       2g. VALIDACIÓN: Fecha no futura
           fecha_parsed = ParseDate(fecha_venta)
           SI fecha_parsed > HOY()
             errores.Add({
               fila,
               "fecha_venta",
               "No se permiten fechas futuras",
               valor_encontrado: fecha_venta
             })
             CONTINUAR
           FIN SI

       2h. Leer detalles de productos (opcional)
           detalles_orden = []
           PARA col = idx_producto_1 A idx_producto_MAX (columnas dinámicas)
             codigo_prod = hoja[fila, col].Value?.ToString().Trim().ToUpper()
             SI codigo_prod.IsNullOrEmpty()
               SALIR  // Sin más productos en esta orden
             FIN SI

             cantidad = hoja[fila, col+1].Value  // siguiente columna
             precio_unit = hoja[fila, col+2].Value  // siguiente columna
             desc_porc = hoja[fila, col+3].Value?  // siguiente columna

             // VALIDACIONES de detalle
             SI codigo_prod NO EN codigos_validos
               advertencias.Add("Fila " + fila + ": Código '" + codigo_prod + 
                               "' no existe en PRODUCTOS. Línea ignorada.")
               CONTINUAR
             FIN SI

             SI NOT EsNumero(cantidad) OR ParseDecimal(cantidad) <= 0
               errores.Add({fila, "cantidad", "Debe ser > 0"})
               CONTINUAR
             FIN SI

             detalle = NEW DetalleVentaDTO {
               numero_orden = numero_orden,
               codigo_producto = codigo_prod,
               cantidad = ParseDecimal(cantidad),
               precio_unitario = ParseDecimal(precio_unit),
               descuento_porcentaje = desc_porc? ParseDecimal(desc_porc) : 0,
               subtotal = ParseDecimal(cantidad) * ParseDecimal(precio_unit),
               empresaId = empresaId,
               fila_origen = fila
             }

             detalles_orden.Add(detalle)

           FIN PARA

       2i. Crear venta
           venta = NEW VentaDTO {
             numero_orden = numero_orden,
             numero_factura = numero_factura?.Trim(),
             fecha_venta = ParseDate(fecha_venta),
             cliente_nombre = cliente_nombre,
             cliente_documento = cliente_documento?.Trim(),
             cliente_telefono = cliente_telefono?.Trim(),
             cliente_email = cliente_email?.Trim(),
             cliente_direccion = cliente_direccion?.Trim(),
             ciudad = ciudad?.Trim(),
             monto_subtotal = monto_sub,
             monto_descuento = monto_desc,
             monto_impuestos = monto_imp,
             monto_total = monto_tot,
             metodo_pago = ParseEnum(metodo_pago),
             estado_pago = estado_pago?.Trim()?.ToLower() ?? "pendiente",
             vendedor = vendedor?.Trim(),
             canal_venta = canal_venta?.Trim()?.ToLower(),
             estado = "completado",
             notas = notas?.Trim(),
             empresaId = empresaId,
             fila_origen = fila
           }

           ventas.Add(venta)
           SI detalles_orden.Count > 0
             detalles.AddRange(detalles_orden)
           FIN SI

     FIN PARA

  3. Retornar resultado
     SI errores.Count > 0
       RETORNAR { exitoso: false, errores: errores }
     FIN SI

     RETORNAR {
       exitoso: true,
       ventas: ventas,
       detalles_venta: detalles,
       advertencias: advertencias
     }
FIN FUNCIÓN
```

---

### **Función: LeerYMapearFinancieros**

```
ENTRADA:
  hoja_financieros: ExcelSheet,
  empresaId: UUID

SALIDA:
  {
    exitoso: boolean,
    financieros: List<FinancieroDTO>,
    errores: List<ErrorValidacion>,
    advertencias: List<string>
  }

DONDE FinancieroDTO = {
  tipo_dato: enum,  // "ingreso", "gasto", "costo", "inversion"
  categoria: string,
  subcategoria: string?,
  concepto: string,
  monto: decimal,
  moneda: string,
  fecha_registro: DateTime,
  fecha_pago: DateTime?,
  numero_comprobante: string?,
  beneficiario: string?,
  observaciones: string?,
  empresaId: UUID,
  fila_origen: int
}

LÓGICA:
  1. Inicializar
     financieros = []
     errores = []
     advertencias = []
     tipos_permitidos = ["ingreso", "gasto", "costo", "inversion"]
     categorias_por_tipo = {
       "ingreso": ["Ventas", "Servicios", "Retorno inversión", ...],
       "gasto": ["Salarios", "Servicios", "Transporte", ...],
       "costo": ["Costo Bienes Vendidos", "Materia Prima", ...],
       "inversion": ["Activos Fijos", "Mejoras", "Tecnología", ...]
     }

  2. PARA fila = 2 A hoja_financieros.GetRowCount()
       
       2a. Leer valores
           tipo_dato = hoja[fila, idx_tipo].Value?.ToString().Trim().ToLower()
           categoria = hoja[fila, idx_categoria].Value?.ToString().Trim()
           subcategoria = hoja[fila, idx_subcategoria].Value?.ToString().Trim()?
           concepto = hoja[fila, idx_concepto].Value?.ToString().Trim()
           monto = hoja[fila, idx_monto].Value
           fecha_registro = hoja[fila, idx_fecha].Value
           fecha_pago = hoja[fila, idx_fecha_pago].Value?
           // ... resto

       2b. VALIDACIÓN: Campos obligatorios
           SI tipo_dato.IsNullOrEmpty() OR concepto.IsNullOrEmpty() OR 
              categoria.IsNullOrEmpty() OR monto.IsNullOrEmpty() OR
              fecha_registro.IsNullOrEmpty()
             errores.Add({fila, error: "Faltan campos obligatorios"})
             CONTINUAR
           FIN SI

       2c. VALIDACIÓN: Tipo válido
           SI tipo_dato NO EN tipos_permitidos
             errores.Add({
               fila,
               "tipo_dato",
               "Debe ser: " + JOIN(tipos_permitidos),
               valor_encontrado: tipo_dato
             })
             CONTINUAR
           FIN SI

       2d. VALIDACIÓN: Categoría válida para tipo
           categorias_validas = categorias_por_tipo[tipo_dato]
           SI categoria NO EN categorias_validas
             errores.Add({
               fila,
               "categoria",
               "No válida para tipo '" + tipo_dato + "'. Válidas: " + 
               JOIN(categorias_validas),
               valor_encontrado: categoria
             })
             CONTINUAR
           FIN SI

       2e. VALIDACIÓN: Monto
           SI NOT EsNumero(monto)
             errores.Add({fila, "monto", "Debe ser número"})
             CONTINUAR
           FIN SI

           monto_num = ParseDecimal(monto)
           SI monto_num <= 0
             errores.Add({
               fila,
               "monto",
               "Debe ser mayor a 0",
               valor_encontrado: monto
             })
             CONTINUAR
           FIN SI

       2f. VALIDACIÓN: Fechas
           fecha_reg = ParseDate(fecha_registro)
           SI fecha_reg > HOY()
             errores.Add({
               fila,
               "fecha_registro",
               "No se permiten fechas futuras"
             })
             CONTINUAR
           FIN SI

           SI fecha_pago NOT NULL
             fecha_pago_parsed = ParseDate(fecha_pago)
             SI fecha_pago_parsed < fecha_reg
               errores.Add({
                 fila,
                 "fecha_pago",
                 "No puede ser anterior a fecha_registro"
               })
               CONTINUAR
             FIN SI
           FIN SI

       2g. Crear DTO
           financiero = NEW FinancieroDTO {
             tipo_dato = ParseEnum(tipo_dato),
             categoria = categoria,
             subcategoria = subcategoria,
             concepto = concepto,
             monto = monto_num,
             moneda = moneda?.Trim() ?? "COP",
             fecha_registro = fecha_reg,
             fecha_pago = fecha_pago? ParseDate(fecha_pago) : NULL,
             numero_comprobante = numero_comprobante?.Trim(),
             beneficiario = beneficiario?.Trim(),
             observaciones = observaciones?.Trim(),
             empresaId = empresaId,
             fila_origen = fila
           }

           financieros.Add(financiero)

     FIN PARA

  3. Retornar
     SI errores.Count > 0
       RETORNAR { exitoso: false, errores: errores }
     FIN SI

     RETORNAR {
       exitoso: true,
       financieros: financieros
     }
FIN FUNCIÓN
```

---

## 🔶 NIVEL 3: CARGA ATÓMICA A BASE DE DATOS

### **Función: CargarDatosABD**

```
ENTRADA:
  productos: List<ProductoDTO>,
  inventarios: List<InventarioDTO>,
  ventas: List<VentaDTO>,
  detalles_venta: List<DetalleVentaDTO>,
  financieros: List<FinancieroDTO>,
  empresaId: UUID

SALIDA:
  {
    exitoso: boolean,
    resultado_carga: {
      productos_insertados: int,
      productos_actualizados: int,
      inventario_insertado: int,
      ventas_insertadas: int,
      detalles_insertados: int,
      financieros_insertados: int,
      fecha_carga: DateTime,
      duracion_segundos: float
    },
    error?: string
  }

LÓGICA:
  1. Iniciar transacción
     INTENTA
       transaccion = BD.BeginTransaction(IsolationLevel.Serializable)
       inicio_tiempo = AHORA()

  2. Insertar/Actualizar PRODUCTOS
     resultados_prod = CargarProductosABD(productos, empresaId, transaccion)
     SI NOT resultados_prod.exitoso
       transaccion.Rollback()
       RETORNAR { exitoso: false, error: resultados_prod.error }
     FIN SI

  3. Insertar/Actualizar INVENTARIO
     resultados_inv = CargarInventarioABD(inventarios, empresaId, transaccion)
     SI NOT resultados_inv.exitoso
       transaccion.Rollback()
       RETORNAR { exitoso: false, error: resultados_inv.error }
     FIN SI

  4. Insertar VENTAS Y DETALLES
     resultados_ventas = CargarVentasABD(
       ventas, 
       detalles_venta, 
       empresaId, 
       transaccion
     )
     SI NOT resultados_ventas.exitoso
       transaccion.Rollback()
       RETORNAR { exitoso: false, error: resultados_ventas.error }
     FIN SI

  5. Insertar FINANCIEROS
     resultados_fin = CargarFinancierosABD(financieros, empresaId, transaccion)
     SI NOT resultados_fin.exitoso
       transaccion.Rollback()
       RETORNAR { exitoso: false, error: resultados_fin.error }
     FIN SI

  6. Commit transacción
     transaccion.Commit()
     fin_tiempo = AHORA()
     duracion = (fin_tiempo - inicio_tiempo).TotalSeconds

  7. Registrar carga completada
     RegistrarCargataBD({
       empresa_id: empresaId,
       estado: "completado",
       fecha: AHORA(),
       registros_cargados: productos.Count + inventarios.Count + ventas.Count + financieros.Count,
       duracion_segundos: duracion
     })

  8. Retornar éxito
     RETORNAR {
       exitoso: true,
       resultado_carga: {
         productos_insertados: resultados_prod.insertados,
         productos_actualizados: resultados_prod.actualizados,
         inventario_insertado: resultados_inv.insertados,
         ventas_insertadas: resultados_ventas.ventas,
         detalles_insertados: resultados_ventas.detalles,
         financieros_insertados: resultados_fin.insertados,
         fecha_carga: AHORA(),
         duracion_segundos: duracion
       }
     }

  EXCEPCIONES:
     EXCEPCIÓN error tipo ConstraintViolationException
       transaccion.Rollback()
       RETORNAR {
         exitoso: false,
         error: "Violación de integridad: " + error.Message
       }

     EXCEPCIÓN error tipo DatabaseException
       transaccion.Rollback()
       RETORNAR {
         exitoso: false,
         error: "Error de base de datos: " + error.Message
       }

     EXCEPCIÓN error tipo Exception
       INTENTA transaccion.Rollback()
       RETORNAR {
         exitoso: false,
         error: "Error inesperado: " + error.Message
       }

FIN FUNCIÓN
```

---

### **Función: CargarProductosABD** (Helper)

```
ENTRADA:
  productos: List<ProductoDTO>,
  empresaId: UUID,
  transaccion: DbTransaction

SALIDA:
  {
    exitoso: boolean,
    insertados: int,
    actualizados: int,
    error?: string
  }

LÓGICA:
  1. Inicializar contadores
     insertados = 0
     actualizados = 0

  2. PARA CADA producto EN productos
       
       2a. Verificar si existe
           producto_existente = BD.Productos.FirstOrDefault(
             p => p.empresaId = empresaId AND 
                  p.codigo_producto = producto.codigo_producto
           )

       2b. SI existe: ACTUALIZAR
           SI producto_existente NOT NULL
             producto_existente.nombre = producto.nombre
             producto_existente.precio_venta = producto.precio_venta
             producto_existente.costo_unitario = producto.costo_unitario
             producto_existente.categoria = producto.categoria
             // ... actualizar resto de campos
             producto_existente.updated_at = AHORA()

             BD.SaveChanges()  // Dentro de la transacción
             actualizados++

       2c. SI NO existe: INSERTAR
           SINO
             nuevo_producto = NEW Producto {
               id = UUID_Generate(),
               empresa_id = empresaId,
               codigo_producto = producto.codigo_producto,
               nombre = producto.nombre,
               // ... copiar resto de campos
               created_at = AHORA(),
               updated_at = AHORA()
             }

             BD.Productos.Add(nuevo_producto)
             BD.SaveChanges()  // Dentro de la transacción
             insertados++

           FIN SI

     FIN PARA

  3. Retornar resultado
     RETORNAR {
       exitoso: true,
       insertados: insertados,
       actualizados: actualizados
     }

  EXCEPCIÓN error
    RETORNAR {
      exitoso: false,
      error: "Error cargando productos: " + error.Message
    }
FIN FUNCIÓN
```

---

## 🔶 NIVEL 4: MANEJO DE ERRORES & LOGS

### **Función: RegistrarCargataBD**

```
ENTRADA:
  importacion_data: {
    empresa_id: UUID,
    estado: enum,  // "en_proceso", "completado", "fallido"
    archivo_nombre: string,
    archivo_hash: string,
    tipo_datos: string,  // "ventas", "inventario", etc.
    registros_cargados: int,
    registros_rechazados: int,
    errores: List<string>,
    advertencias: List<string>,
    duracion_segundos: float,
    fecha: DateTime,
    usuario_id: UUID?
  }

LÓGICA:
  INTENTA
    registro = NEW ImportacionDatos {
      id = UUID_Generate(),
      empresa_id = importacion_data.empresa_id,
      nombre_archivo = importacion_data.archivo_nombre,
      hash_archivo = importacion_data.archivo_hash,
      tipo_datos = importacion_data.tipo_datos,
      estado = importacion_data.estado,
      registros_cargados = importacion_data.registros_cargados,
      registros_rechazados = importacion_data.registros_rechazados,
      errores_extraccion = JsonConvert.SerializeObject(importacion_data.errores),
      advertencias = JsonConvert.SerializeObject(importacion_data.advertencias),
      duracion_segundos = importacion_data.duracion_segundos,
      fecha_importacion = importacion_data.fecha,
      created_at = AHORA(),
      updated_at = AHORA()
    }

    BD.ImportacionesDatos.Add(registro)
    BD.SaveChanges()

    RETORNAR true

  EXCEPCIÓN error
    LOG_ERROR("Error registrando importación", error)
    RETORNAR false
FIN FUNCIÓN
```

---

### **Función: GenerarReporteCargas**

```
ENTRADA:
  resultado_completo: {
    exitoso: boolean,
    archivo_nombre: string,
    timestamp: DateTime,
    empresa_id: UUID,
    errores: List<ErrorValidacion>,
    advertencias: List<string>,
    resultado_carga?: {
      productos_insertados: int,
      ... (del resultado de carga)
    }
  }

SALIDA:
  reporte: string (HTML o JSON)

LÓGICA:
  1. Crear estructura JSON/HTML
     reporte = {
       titulo: "Reporte de Carga ClariData",
       fecha: resultado_completo.timestamp,
       archivo: resultado_completo.archivo_nombre,
       estado_general: resultado_completo.exitoso ? "✅ EXITOSO" : "❌ FALLIDO",
       resumen: {
         total_errores: resultado_completo.errores.Count,
         total_advertencias: resultado_completo.advertencias.Count
       }
     }

  2. SI exitoso
       reporte.detalle_carga = {
         productos_insertados: resultado_completo.resultado_carga.productos_insertados,
         productos_actualizados: resultado_completo.resultado_carga.productos_actualizados,
         inventarios_insertados: resultado_completo.resultado_carga.inventario_insertado,
         ventas_insertadas: resultado_completo.resultado_carga.ventas_insertadas,
         financieros_insertados: resultado_completo.resultado_carga.financieros_insertados,
         duracion_segundos: resultado_completo.resultado_carga.duracion_segundos
       }

  3. SI hay errores
       reporte.errores = PARA CADA error EN resultado_completo.errores
         {
           fila: error.fila,
           columna: error.columna,
           mensaje: error.error,
           valor_encontrado: error.valor_encontrado
         }

  4. SI hay advertencias
       reporte.advertencias = resultado_completo.advertencias

  5. Retornar reporte
     RETORNAR JsonConvert.SerializeObject(reporte, Formatting.Indented)
FIN FUNCIÓN
```

---

## 📊 FLUJO GENERAL (ORQUESTACIÓN)

```
┌─────────────────────────────────────────────────────────────────┐
│ ENTRADA: Usuario sube archivo Excel                              │
└─────────────────────────────────────────────────────────────────┘
                            ↓
        ┌──────────────────────────────────────┐
        │ 1️⃣  ValidarArchivoExcel()             │
        │    - Extensión .xlsx                 │
        │    - Tamaño < 10 MB                  │
        │    - Archivo válido                  │
        └──────────────────────────────────────┘
                  ❌ ERROR → RECHAZAR
                            ↓
        ┌──────────────────────────────────────┐
        │ 2️⃣  ValidarEstructuraHojas()          │
        │    - 4 hojas presentes                │
        │    - Columnas correctas               │
        │    - No vacías                        │
        └──────────────────────────────────────┘
                  ❌ ERROR → RECHAZAR
                            ↓
        ┌──────────────────────────────────────┐
        │ 3️⃣  LeerYMapearProductos()            │
        │    - Parse de datos                  │
        │    - Validaciones de negocio         │
        │    - Crear DTOs                      │
        └──────────────────────────────────────┘
                  ❌ ERROR → RECHAZAR
                            ↓
        ┌──────────────────────────────────────┐
        │ 4️⃣  LeerYMapearInventario()           │
        │    - Validar referencias             │
        │    - Consistencia stock              │
        └──────────────────────────────────────┘
                  ❌ ERROR → RECHAZAR
                            ↓
        ┌──────────────────────────────────────┐
        │ 5️⃣  LeerYMapearVentas()               │
        │    - Validar órdenes                 │
        │    - Coherencia montos               │
        └──────────────────────────────────────┘
                  ❌ ERROR → RECHAZAR
                            ↓
        ┌──────────────────────────────────────┐
        │ 6️⃣  LeerYMapearFinancieros()          │
        │    - Validar categorías              │
        │    - Fechas consistentes             │
        └──────────────────────────────────────┘
                  ❌ ERROR → RECHAZAR
                            ↓
        ┌──────────────────────────────────────┐
        │ 7️⃣  CargarDatosABD()                  │
        │    - Transacción Serializable        │
        │    - Insertar/Actualizar en orden    │
        │    - COMMIT o ROLLBACK               │
        └──────────────────────────────────────┘
                  ❌ ERROR → ROLLBACK
                            ↓
        ┌──────────────────────────────────────┐
        │ 8️⃣  RegistrarCargataBD()              │
        │    - Guardar log                     │
        │    - Historial de carga              │
        └──────────────────────────────────────┘
                            ↓
        ┌──────────────────────────────────────┐
        │ 9️⃣  GenerarReporteCargas()            │
        │    - JSON/HTML con resultados        │
        │    - Errores detallados (si aplica)  │
        │    - Advertencias                    │
        └──────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ SALIDA: Reporte + Datos en BD                                   │
│         Dashboards actualizados en tiempo real                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## ⚙️ PSEUDOCÓDIGO TRANSVERSAL - UTILIDADES

### **Función: ParseDate**

```
ENTRADA: valor: object

SALIDA: DateTime o NULL

LÓGICA:
  SI valor IS NULL OR valor IS DBNull
    RETORNAR NULL
  FIN SI

  texto = valor.ToString().Trim()

  FORMATOS ESPERADOS (en orden de intento):
  1. "YYYY-MM-DD" (ISO estándar) ← PREFERIDO
  2. "DD/MM/YYYY" (formato local Colombia)
  3. "MM/DD/YYYY" (formato USA)
  4. "YYYY/MM/DD"

  INTENTA
    // Intentar ISO
    SI DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha1)
      RETORNAR fecha1
    FIN SI

    // Intentar DD/MM/YYYY
    SI DateTime.TryParseExact(texto, "dd/MM/yyyy", new CultureInfo("es-CO"), DateTimeStyles.None, out var fecha2)
      RETORNAR fecha2
    FIN SI

    // Intentar parse automático
    SI DateTime.TryParse(texto, out var fecha3)
      RETORNAR fecha3
    FIN SI

    // Si nada funciona
    LANZAR EXCEPCIÓN "Formato de fecha no reconocido: " + texto

  EXCEPCIÓN error
    RETORNAR NULL
FIN FUNCIÓN
```

### **Función: ParseDecimal**

```
ENTRADA: valor: object

SALIDA: decimal

LÓGICA:
  SI valor IS NULL OR valor IS DBNull
    RETORNAR 0m
  FIN SI

  texto = valor.ToString().Trim()

  // Remover símbolos de moneda comunes
  texto = texto.Replace("$", "")
              .Replace("COP", "")
              .Replace("USD", "")
              .Replace(",", "")  // separador de miles
              .Trim()

  // EN COLOMBIA: punto (.) es decimal, coma (,) es miles
  // Pero Excel puede enviar ambos formatos
  // Lógica: si hay dos separadores, el último es decimal
  
  SI texto.Contains(".") AND texto.Contains(",")
    ultimo_punto = texto.LastIndexOf(".")
    ultima_coma = texto.LastIndexOf(",")
    
    SI ultimo_punto > ultima_coma
      // Punto es decimal: 1.000,50 → 1000.50
      texto = texto.Replace(".", "").Replace(",", ".")
    SINO
      // Coma es decimal: 1,000.50 → 1000.50
      texto = texto.Replace(",", "")
    FIN SI
  FIN SI

  INTENTA
    decimal resultado = Decimal.Parse(texto, new CultureInfo("en-US"))
    RETORNAR resultado
  EXCEPCIÓN error
    RETORNAR 0m
FIN FUNCIÓN
```

### **Función: ParseBoolean**

```
ENTRADA: valor: object

SALIDA: boolean

LÓGICA:
  SI valor IS NULL OR valor IS DBNull
    RETORNAR false
  FIN SI

  texto = valor.ToString().Trim().ToUpper()

  RETORNAR SELECCIONAR texto
    CASO "VERDADERO", "TRUE", "V", "SÍ", "SI", "S", "YES", "Y", "1", "ACTIVO", "ACTIVA"
      RETORNAR true
    
    CASO "FALSO", "FALSE", "F", "NO", "N", "0", "INACTIVO", "INACTIVA"
      RETORNAR false
    
    DEFAULT
      RETORNAR false
FIN FUNCIÓN
```

### **Función: EsEmailValido**

```
ENTRADA: email: string

SALIDA: boolean

LÓGICA:
  PATRÓN_REGEX = "^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$"
  
  RETORNAR Regex.IsMatch(email, PATRÓN_REGEX)
FIN FUNCIÓN
```

### **Función: NormalizarTexto**

```
ENTRADA: texto: string

SALIDA: string

LÓGICA:
  SI texto IS NULL
    RETORNAR ""
  FIN SI

  // Trim espacios
  texto = texto.Trim()

  // Remover espacios múltiples
  MIENTRAS texto.Contains("  ")
    texto = texto.Replace("  ", " ")
  FIN MIENTRAS

  // Remover caracteres de control
  texto = Regex.Replace(texto, @"[\x00-\x1F]", "")

  RETORNAR texto
FIN FUNCIÓN
```

---

## 📋 RESUMEN ARQUITECTURA

| Aspecto | Detalles |
|---------|----------|
| **Paradigma** | Modular, layer-based |
| **Entrada** | 1 archivo Excel (.xlsx) |
| **Validación** | 3 niveles (estructura, formato, negocio) |
| **Transformación** | Normalización, imputación, mapping |
| **Carga** | Transacción atómica (Serializable) |
| **Rollback** | Completo si hay error |
| **Logs** | Historial de cargas en BD |
| **Errores** | Explícitos, por fila y columna |
| **Target Code** | C# (Entity Framework Core + PostgreSQL) |

---

## 🎯 SIGUIENTES PASOS (FASE 2.5)

1. **Implementar en C#** con Entity Framework Core
2. **Agregar validaciones de negocio avanzadas** (restricciones complejas)
3. **Implementar caché de validaciones** (listas de categorías, etc.)
4. **Soporte para imputación de datos** (llenar vacíos inteligentemente)
5. **Interfaz de reintento de cargas** (para errores no bloqueantes)
6. **Reportes descargables** (Excel con errores marcados)
7. **API para cargas programadas** (no solo manuales)
8. **Webhooks para notificaciones** (post-carga a IA)
