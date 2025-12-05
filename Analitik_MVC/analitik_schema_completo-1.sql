-- ============================================================================
-- ANALITIK - MODELO DE BASE DE DATOS COMPLETO Y FINAL
-- Sistema de Análisis de Datos para PyMEs Colombianas
-- ============================================================================
-- Versión: 4.0 FINAL
-- Backend: Node.js/Express
-- Base de Datos: PostgreSQL 15+
-- Modelo: Corporativo - Cuenta única por empresa
-- Características:
--   ✓ Dashboards NO personalizables (4 tipos fijos)
--   ✓ KPIs y gráficos predefinidos
--   ✓ ETL interno para importación de datos
--   ✓ OpenAI API con control de tokens y costos
-- Fecha: Octubre 29, 2025
-- ============================================================================

-- Configuración inicial
SET client_encoding = 'UTF8';
SET timezone = 'America/Bogota';
SET search_path = public, pg_catalog;

-- Extensiones necesarias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "btree_gin";

-- ============================================================================
-- TIPOS ENUMERADOS (ENUMS)
-- ============================================================================

CREATE TYPE tipo_sector_empresa AS ENUM (
    'comercio', 'manufactura', 'servicios', 'tecnologia', 
    'confecciones', 'alimentos', 'salud', 'educacion', 'otro'
);

CREATE TYPE tipo_tamano_empresa AS ENUM ('micro', 'pequena', 'mediana');
CREATE TYPE tipo_tema AS ENUM ('claro', 'oscuro', 'automatico');
CREATE TYPE tipo_idioma AS ENUM ('es', 'en', 'pt');
CREATE TYPE tipo_dashboard AS ENUM ('ventas', 'financieros', 'inventarios', 'operaciones');
CREATE TYPE tipo_metrica AS ENUM ('ventas', 'financiero', 'inventario', 'operacional');
CREATE TYPE tipo_periodo AS ENUM ('diario', 'semanal', 'mensual', 'trimestral', 'anual');
CREATE TYPE tipo_tendencia AS ENUM ('up', 'down', 'neutral');
CREATE TYPE tipo_cliente AS ENUM ('b2b', 'b2c', 'eventual');
CREATE TYPE tipo_estado_venta AS ENUM ('pendiente', 'completado', 'cancelado', 'devuelto');
CREATE TYPE tipo_metodo_pago AS ENUM ('efectivo', 'tarjeta', 'transferencia', 'credito', 'cheque');
CREATE TYPE tipo_movimiento_inventario AS ENUM ('entrada', 'salida', 'ajuste', 'devolucion', 'transferencia');
CREATE TYPE tipo_estado_stock AS ENUM ('normal', 'bajo', 'critico', 'exceso');
CREATE TYPE tipo_tendencia_producto AS ENUM ('creciente', 'estable', 'decreciente');
CREATE TYPE tipo_dato_financiero AS ENUM ('ingreso', 'gasto', 'costo', 'inversion');
CREATE TYPE tipo_mensaje AS ENUM ('usuario', 'ia');
CREATE TYPE tipo_recomendacion AS ENUM ('alerta', 'oportunidad', 'optimizacion', 'tendencia');
CREATE TYPE tipo_prioridad AS ENUM ('baja', 'media', 'alta', 'critica');
CREATE TYPE tipo_estado_recomendacion AS ENUM ('nueva', 'vista', 'aplicada', 'descartada');
CREATE TYPE tipo_notificacion AS ENUM ('alerta', 'info', 'exito', 'advertencia');
CREATE TYPE tipo_plan_suscripcion AS ENUM ('basico', 'profesional', 'empresarial');
CREATE TYPE tipo_estado_suscripcion AS ENUM ('activa', 'suspendida', 'cancelada', 'trial');
CREATE TYPE tipo_frecuencia_reporte AS ENUM ('diaria', 'semanal', 'mensual', 'nunca');
CREATE TYPE tipo_fuente_datos AS ENUM ('postgresql', 'mysql', 'mariadb', 'sqlserver', 'oracle', 'excel', 'csv', 'api', 'manual');
CREATE TYPE tipo_estado_importacion AS ENUM ('en_proceso', 'completado', 'fallido', 'cancelado');
CREATE TYPE tipo_estado_conexion AS ENUM ('conectado', 'desconectado', 'error', 'probando');
CREATE TYPE tipo_datos_importacion AS ENUM ('ventas', 'productos', 'inventario', 'clientes', 'financieros', 'mixto');
CREATE TYPE tipo_fase_etl AS ENUM ('extraccion', 'transformacion', 'carga', 'completado', 'error');
CREATE TYPE tipo_accion_auditoria AS ENUM ('crear', 'actualizar', 'eliminar');

-- ============================================================================
-- TABLA: empresas
-- ============================================================================

CREATE TABLE empresas (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    nombre_comercial VARCHAR(255) NOT NULL,
    razon_social VARCHAR(255),
    nit VARCHAR(50) UNIQUE,
    sector tipo_sector_empresa NOT NULL,
    tamano tipo_tamano_empresa NOT NULL,
    director_nombre_completo VARCHAR(255) NOT NULL,
    director_cargo VARCHAR(100) DEFAULT 'Director General',
    director_telefono VARCHAR(20),
    director_email_secundario VARCHAR(255),
    director_documento VARCHAR(50),
    logo_url VARCHAR(500),
    descripcion_empresa TEXT,
    direccion_fiscal TEXT,
    ciudad VARCHAR(100),
    departamento VARCHAR(100) DEFAULT 'Antioquia',
    pais VARCHAR(50) DEFAULT 'Colombia',
    codigo_postal VARCHAR(20),
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    activa BOOLEAN NOT NULL DEFAULT true,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT nit_valido CHECK (nit IS NULL OR LENGTH(nit) >= 5),
    CONSTRAINT telefono_director_valido CHECK (director_telefono IS NULL OR LENGTH(director_telefono) >= 7),
    CONSTRAINT email_secundario_valido CHECK (
        director_email_secundario IS NULL OR 
        director_email_secundario ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'
    )
);

CREATE UNIQUE INDEX idx_empresas_nit ON empresas(nit) WHERE nit IS NOT NULL;
CREATE INDEX idx_empresas_activa ON empresas(activa) WHERE activa = true;
CREATE INDEX idx_empresas_sector ON empresas(sector);
CREATE INDEX idx_empresas_ciudad_departamento ON empresas(ciudad, departamento);
CREATE INDEX idx_empresas_nombre_comercial_trgm ON empresas USING GIN(nombre_comercial gin_trgm_ops);

COMMENT ON TABLE empresas IS 'PyMEs registradas - Entidad central del sistema';
COMMENT ON COLUMN empresas.nit IS 'Número de Identificación Tributaria (único en Colombia)';

-- ============================================================================
-- TABLA: cuenta_empresa
-- ============================================================================

CREATE TABLE cuenta_empresa (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL UNIQUE REFERENCES empresas(id) ON DELETE CASCADE,
    email VARCHAR(255) NOT NULL UNIQUE,
    -- Campos enriquecidos para mapear el perfil usado en el frontend (Settings.profileForm / HomePage)
    nombre_completo VARCHAR(255),
    display_name VARCHAR(100),
    avatar_url VARCHAR(500),
    bio TEXT,
    ubicacion VARCHAR(255),
    telefono VARCHAR(20),
    password_hash VARCHAR(255) NOT NULL,
    activa BOOLEAN NOT NULL DEFAULT true,
    verificada BOOLEAN NOT NULL DEFAULT false,
    email_verificado_en TIMESTAMPTZ,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ultima_sesion TIMESTAMPTZ,
    ip_ultima_sesion VARCHAR(45),
    refresh_token_hash VARCHAR(255),
    refresh_token_expiracion TIMESTAMPTZ,
    token_recuperacion VARCHAR(255),
    token_expiracion TIMESTAMPTZ,
    intentos_fallidos INTEGER NOT NULL DEFAULT 0,
    bloqueada_hasta TIMESTAMPTZ,
    fecha_ultimo_cambio_password TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Rol y flags de usuario (una sola cuenta por empresa, por eso rol por cuenta)
    rol VARCHAR(50) DEFAULT 'admin',
    es_owner BOOLEAN NOT NULL DEFAULT true,
    
    CONSTRAINT email_valido CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT intentos_rango CHECK (intentos_fallidos >= 0 AND intentos_fallidos <= 10)
);

CREATE INDEX idx_cuenta_empresa_empresa ON cuenta_empresa(empresa_id);
CREATE INDEX idx_cuenta_empresa_email ON cuenta_empresa(email);
CREATE INDEX idx_cuenta_empresa_activa ON cuenta_empresa(activa) WHERE activa = true;
CREATE INDEX idx_cuenta_empresa_token_recuperacion ON cuenta_empresa(token_recuperacion) 
    WHERE token_recuperacion IS NOT NULL AND token_expiracion > CURRENT_TIMESTAMP;

COMMENT ON TABLE cuenta_empresa IS 'Cuenta única de autenticación por empresa (1:1 con empresas)';

-- ============================================================================
-- TABLA: configuracion_empresa
-- ============================================================================

CREATE TABLE configuracion_empresa (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL UNIQUE REFERENCES empresas(id) ON DELETE CASCADE,
    tema tipo_tema NOT NULL DEFAULT 'claro',
    idioma tipo_idioma NOT NULL DEFAULT 'es',
    zona_horaria VARCHAR(50) NOT NULL DEFAULT 'America/Bogota',
    moneda VARCHAR(10) NOT NULL DEFAULT 'COP',
    formato_fecha VARCHAR(20) NOT NULL DEFAULT 'DD/MM/YYYY',
    formato_numero VARCHAR(20) NOT NULL DEFAULT 'es-CO',
    primera_dia_semana INTEGER DEFAULT 1 CHECK (primera_dia_semana BETWEEN 0 AND 6),
    notificaciones_email_reportes BOOLEAN NOT NULL DEFAULT true,
    notificaciones_email_alertas BOOLEAN NOT NULL DEFAULT true,
    notificaciones_push BOOLEAN NOT NULL DEFAULT true,
    notificaciones_app BOOLEAN NOT NULL DEFAULT true,
    notificaciones_marketing BOOLEAN NOT NULL DEFAULT false,
    notificaciones_seguridad BOOLEAN NOT NULL DEFAULT true,
    frecuencia_reportes tipo_frecuencia_reporte NOT NULL DEFAULT 'semanal',
    configuracion_privacidad JSONB DEFAULT '{"visibilidad_perfil": "privado", "compartir_datos": false, "analytics": true, "reportes_errores": true}'::jsonb,
    configuracion_reportes JSONB DEFAULT '{"formato_exportacion": "pdf", "incluir_graficos": true, "incluir_datos_brutos": false}'::jsonb,
    configuracion_alertas JSONB DEFAULT '{"stock_minimo_global": 10, "dias_vencimiento_alerta": 30}'::jsonb,
    configuracion_avanzada JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_configuracion_empresa ON configuracion_empresa(empresa_id);
CREATE INDEX idx_configuracion_reportes ON configuracion_empresa USING GIN(configuracion_reportes);
CREATE INDEX idx_configuracion_privacidad ON configuracion_empresa USING GIN(configuracion_privacidad);

COMMENT ON TABLE configuracion_empresa IS 'Preferencias empresariales';

-- ============================================================================
-- TABLA: suscripciones
-- ============================================================================

CREATE TABLE suscripciones (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    plan tipo_plan_suscripcion NOT NULL,
    estado tipo_estado_suscripcion NOT NULL DEFAULT 'trial',
    fecha_inicio DATE NOT NULL DEFAULT CURRENT_DATE,
    fecha_fin DATE,
    fecha_proximo_cobro DATE,
    dias_trial_restantes INTEGER,
    precio_mensual NUMERIC(10,2) NOT NULL,
    descuento_aplicado NUMERIC(5,2) DEFAULT 0,
    precio_final NUMERIC(10,2) GENERATED ALWAYS AS (
        precio_mensual * (1 - descuento_aplicado / 100)
    ) STORED,
    metodo_pago VARCHAR(50),
    renovacion_automatica BOOLEAN NOT NULL DEFAULT true,
    usuarios_permitidos INTEGER DEFAULT 1,
    almacenamiento_gb INTEGER DEFAULT 5,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT precio_positivo CHECK (precio_mensual >= 0),
    CONSTRAINT descuento_valido CHECK (descuento_aplicado >= 0 AND descuento_aplicado <= 100),
    CONSTRAINT fechas_validas CHECK (fecha_fin IS NULL OR fecha_fin >= fecha_inicio)
);

CREATE INDEX idx_suscripciones_empresa ON suscripciones(empresa_id);
CREATE INDEX idx_suscripciones_estado ON suscripciones(estado);
CREATE INDEX idx_suscripciones_vigencia ON suscripciones(fecha_inicio, fecha_fin);

COMMENT ON TABLE suscripciones IS 'Gestión de planes SaaS y facturación';

-- ============================================================================
-- TABLA: clientes
-- ============================================================================

CREATE TABLE clientes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    codigo_cliente VARCHAR(50) NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    tipo_cliente tipo_cliente NOT NULL DEFAULT 'eventual',
    documento_identificacion VARCHAR(50),
    email VARCHAR(255),
    telefono VARCHAR(20),
    telefono_alternativo VARCHAR(20),
    direccion TEXT,
    ciudad VARCHAR(100),
    departamento VARCHAR(100),
    codigo_postal VARCHAR(20),
    fecha_registro TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_ultima_compra TIMESTAMPTZ,
    total_compras NUMERIC(15,2) DEFAULT 0,
    numero_compras INTEGER DEFAULT 0,
    activo BOOLEAN NOT NULL DEFAULT true,
    notas TEXT,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT codigo_cliente_unico UNIQUE(empresa_id, codigo_cliente),
    CONSTRAINT email_cliente_valido CHECK (
        email IS NULL OR 
        email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'
    )
);

CREATE INDEX idx_clientes_empresa ON clientes(empresa_id);
CREATE INDEX idx_clientes_codigo ON clientes(codigo_cliente);
CREATE INDEX idx_clientes_nombre_trgm ON clientes USING GIN(nombre gin_trgm_ops);
CREATE INDEX idx_clientes_activo ON clientes(activo) WHERE activo = true;

COMMENT ON TABLE clientes IS 'Base de clientes';

-- ============================================================================
-- TABLA: categorias
-- ============================================================================

CREATE TABLE categorias (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    codigo_categoria VARCHAR(50) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    categoria_padre_id UUID REFERENCES categorias(id) ON DELETE SET NULL,
    icono VARCHAR(50),
    color VARCHAR(20),
    orden INTEGER DEFAULT 0,
    activa BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT codigo_categoria_unico UNIQUE(empresa_id, codigo_categoria)
);

CREATE INDEX idx_categorias_empresa ON categorias(empresa_id);
CREATE INDEX idx_categorias_activa ON categorias(activa) WHERE activa = true;

COMMENT ON TABLE categorias IS 'Categorías de productos';

-- ============================================================================
-- TABLA: productos
-- ============================================================================

CREATE TABLE productos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    categoria_id UUID REFERENCES categorias(id) ON DELETE SET NULL,
    codigo_producto VARCHAR(50) NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    subcategoria VARCHAR(100),
    marca VARCHAR(100),
    modelo VARCHAR(100),
    precio_venta NUMERIC(12,2) NOT NULL,
    costo_unitario NUMERIC(12,2),
    precio_sugerido NUMERIC(12,2),
    margen_porcentaje NUMERIC(5,2) GENERATED ALWAYS AS (
        CASE WHEN costo_unitario > 0 
        THEN ((precio_venta - costo_unitario) / costo_unitario * 100)
        ELSE NULL END
    ) STORED,
    unidad_medida VARCHAR(50) NOT NULL DEFAULT 'unidad',
    peso_kg NUMERIC(10,3),
    volumen_m3 NUMERIC(10,3),
    codigo_barras VARCHAR(100),
    codigo_qr VARCHAR(255),
    es_servicio BOOLEAN NOT NULL DEFAULT false,
    requiere_inventario BOOLEAN NOT NULL DEFAULT true,
    vendible BOOLEAN NOT NULL DEFAULT true,
    comprable BOOLEAN NOT NULL DEFAULT true,
    imagen_url VARCHAR(500),
    imagenes_adicionales JSONB DEFAULT '[]'::jsonb,
    activo BOOLEAN NOT NULL DEFAULT true,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    especificaciones JSONB DEFAULT '{}'::jsonb,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT codigo_producto_unico UNIQUE(empresa_id, codigo_producto),
    CONSTRAINT precio_positivo CHECK (precio_venta >= 0),
    CONSTRAINT costo_positivo CHECK (costo_unitario IS NULL OR costo_unitario >= 0)
);

CREATE INDEX idx_productos_empresa ON productos(empresa_id);
CREATE INDEX idx_productos_categoria ON productos(categoria_id);
CREATE INDEX idx_productos_nombre_trgm ON productos USING GIN(nombre gin_trgm_ops);
CREATE INDEX idx_productos_activo ON productos(activo) WHERE activo = true;

COMMENT ON TABLE productos IS 'Catálogo de productos y servicios';

-- ============================================================================
-- TABLA: inventario
-- ============================================================================

CREATE TABLE inventario (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    producto_id UUID NOT NULL UNIQUE REFERENCES productos(id) ON DELETE CASCADE,
    cantidad_disponible INTEGER NOT NULL DEFAULT 0,
    cantidad_reservada INTEGER NOT NULL DEFAULT 0,
    cantidad_en_transito INTEGER NOT NULL DEFAULT 0,
    stock_minimo INTEGER NOT NULL DEFAULT 0,
    stock_maximo INTEGER,
    punto_reorden INTEGER,
    estado_stock tipo_estado_stock NOT NULL DEFAULT 'normal',
    ubicacion VARCHAR(100),
    pasillo VARCHAR(20),
    estante VARCHAR(20),
    nivel VARCHAR(20),
    lote_actual VARCHAR(50),
    fecha_vencimiento DATE,
    dias_alerta_vencimiento INTEGER DEFAULT 30,
    ultima_actualizacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ultima_entrada TIMESTAMPTZ,
    ultima_salida TIMESTAMPTZ,
    alertas_activadas BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT cantidad_disponible_no_negativa CHECK (cantidad_disponible >= 0),
    CONSTRAINT cantidad_reservada_valida CHECK (cantidad_reservada >= 0),
    CONSTRAINT stock_minimo_valido CHECK (stock_minimo >= 0)
);

CREATE INDEX idx_inventario_producto ON inventario(producto_id);
CREATE INDEX idx_inventario_estado ON inventario(estado_stock);
CREATE INDEX idx_inventario_alerta_bajo ON inventario(cantidad_disponible, stock_minimo) 
    WHERE alertas_activadas = true AND cantidad_disponible < stock_minimo;

COMMENT ON TABLE inventario IS 'Control de stock';

-- ============================================================================
-- TABLA: analisis_productos
-- ============================================================================

CREATE TABLE analisis_productos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    producto_id UUID NOT NULL UNIQUE REFERENCES productos(id) ON DELETE CASCADE,
    dias_sin_movimiento INTEGER DEFAULT 0,
    fecha_ultimo_movimiento TIMESTAMPTZ,
    valor_inmovilizado NUMERIC(15,2),
    indice_rotacion NUMERIC(10,2),
    periodo_rotacion_dias INTEGER DEFAULT 90,
    ventas_ultimos_30_dias INTEGER DEFAULT 0,
    ventas_ultimos_90_dias INTEGER DEFAULT 0,
    ventas_ultimos_365_dias INTEGER DEFAULT 0,
    ingresos_ultimos_30_dias NUMERIC(15,2) DEFAULT 0,
    ingresos_ultimos_90_dias NUMERIC(15,2) DEFAULT 0,
    tendencia tipo_tendencia_producto DEFAULT 'estable',
    variacion_ventas_porcentaje NUMERIC(5,2),
    venta_promedio_diaria NUMERIC(10,2),
    venta_promedio_semanal NUMERIC(10,2),
    dias_stock_disponible INTEGER,
    clasificacion_abc CHAR(1) CHECK (clasificacion_abc IN ('A', 'B', 'C')),
    contribucion_ventas_porcentaje NUMERIC(5,2),
    fecha_calculo TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    proxima_actualizacion TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_analisis_producto ON analisis_productos(producto_id);
CREATE INDEX idx_analisis_dias_sin_mov ON analisis_productos(dias_sin_movimiento DESC);

COMMENT ON TABLE analisis_productos IS 'Métricas de productos';

-- ============================================================================
-- TABLA: movimientos_inventario
-- ============================================================================

CREATE TABLE movimientos_inventario (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    inventario_id UUID NOT NULL REFERENCES inventario(id) ON DELETE CASCADE,
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tipo_movimiento tipo_movimiento_inventario NOT NULL,
    cantidad INTEGER NOT NULL,
    cantidad_anterior INTEGER NOT NULL,
    cantidad_nueva INTEGER NOT NULL,
    motivo VARCHAR(255) NOT NULL,
    referencia VARCHAR(100),
    numero_documento VARCHAR(50),
    venta_id UUID REFERENCES ventas(id) ON DELETE SET NULL,
    lote VARCHAR(50),
    ubicacion_origen VARCHAR(100),
    ubicacion_destino VARCHAR(100),
    fecha_movimiento TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    observaciones TEXT,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT cantidad_positiva CHECK (cantidad > 0)
);

CREATE INDEX idx_movimientos_inventario ON movimientos_inventario(inventario_id);
CREATE INDEX idx_movimientos_empresa ON movimientos_inventario(empresa_id);
CREATE INDEX idx_movimientos_tipo ON movimientos_inventario(tipo_movimiento);
CREATE INDEX idx_movimientos_fecha ON movimientos_inventario(fecha_movimiento DESC);

COMMENT ON TABLE movimientos_inventario IS 'Historial de movimientos de inventario';

-- ============================================================================
-- TABLA: ventas
-- ============================================================================

CREATE TABLE ventas (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    cliente_id UUID REFERENCES clientes(id) ON DELETE SET NULL,
    numero_orden VARCHAR(50) NOT NULL,
    numero_factura VARCHAR(50),
    cliente_nombre VARCHAR(255) NOT NULL,
    cliente_documento VARCHAR(50),
    cliente_telefono VARCHAR(20),
    cliente_email VARCHAR(255),
    cliente_direccion TEXT,
    monto_subtotal NUMERIC(15,2) NOT NULL,
    monto_descuento NUMERIC(15,2) NOT NULL DEFAULT 0,
    monto_impuestos NUMERIC(15,2) NOT NULL DEFAULT 0,
    monto_total NUMERIC(15,2) NOT NULL,
    costo_total NUMERIC(15,2),
    margen_bruto NUMERIC(15,2),
    categoria VARCHAR(100),
    canal_venta VARCHAR(50),
    vendedor VARCHAR(100),
    estado tipo_estado_venta NOT NULL DEFAULT 'pendiente',
    metodo_pago tipo_metodo_pago NOT NULL,
    estado_pago VARCHAR(50) DEFAULT 'pendiente',
    fecha_venta TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_entrega TIMESTAMPTZ,
    fecha_entrega_estimada TIMESTAMPTZ,
    notas TEXT,
    notas_internas TEXT,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT orden_unica_por_empresa UNIQUE(empresa_id, numero_orden),
    CONSTRAINT montos_positivos CHECK (
        monto_subtotal >= 0 AND 
        monto_descuento >= 0 AND
        monto_impuestos >= 0 AND 
        monto_total >= 0
    ),
    CONSTRAINT total_coherente CHECK (monto_total = monto_subtotal - monto_descuento + monto_impuestos)
);

CREATE INDEX idx_ventas_empresa ON ventas(empresa_id);
CREATE INDEX idx_ventas_cliente ON ventas(cliente_id);
CREATE INDEX idx_ventas_numero_orden ON ventas(numero_orden);
CREATE INDEX idx_ventas_estado ON ventas(estado);
CREATE INDEX idx_ventas_fecha ON ventas(fecha_venta DESC);
CREATE INDEX idx_ventas_fecha_mes ON ventas(DATE_TRUNC('month', fecha_venta));

COMMENT ON TABLE ventas IS 'Transacciones de venta';

-- ============================================================================
-- TABLA: detalles_venta
-- ============================================================================

CREATE TABLE detalles_venta (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    venta_id UUID NOT NULL REFERENCES ventas(id) ON DELETE CASCADE,
    producto_id UUID NOT NULL REFERENCES productos(id) ON DELETE RESTRICT,
    cantidad NUMERIC(10,3) NOT NULL,
    cantidad_devuelta NUMERIC(10,3) DEFAULT 0,
    precio_unitario NUMERIC(12,2) NOT NULL,
    precio_original NUMERIC(12,2),
    descuento_porcentaje NUMERIC(5,2) DEFAULT 0,
    descuento_monto NUMERIC(12,2) NOT NULL DEFAULT 0,
    subtotal NUMERIC(15,2) NOT NULL,
    impuesto_porcentaje NUMERIC(5,2) DEFAULT 0,
    impuesto_monto NUMERIC(12,2) NOT NULL DEFAULT 0,
    total NUMERIC(15,2) NOT NULL,
    costo_unitario NUMERIC(12,2),
    costo_total NUMERIC(15,2),
    notas TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT cantidad_positiva CHECK (cantidad > 0),
    CONSTRAINT precio_positivo CHECK (precio_unitario >= 0),
    CONSTRAINT subtotal_coherente CHECK (subtotal = (precio_unitario * cantidad) - descuento_monto),
    CONSTRAINT total_coherente CHECK (total = subtotal + impuesto_monto)
);

CREATE INDEX idx_detalles_venta ON detalles_venta(venta_id);
CREATE INDEX idx_detalles_producto ON detalles_venta(producto_id);

COMMENT ON TABLE detalles_venta IS 'Líneas de productos por venta';

-- ============================================================================
-- TABLA: datos_financieros
-- ============================================================================

CREATE TABLE datos_financieros (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tipo_dato tipo_dato_financiero NOT NULL,
    categoria VARCHAR(100) NOT NULL,
    subcategoria VARCHAR(100),
    concepto VARCHAR(255) NOT NULL,
    monto NUMERIC(15,2) NOT NULL,
    moneda VARCHAR(10) DEFAULT 'COP',
    fecha_registro DATE NOT NULL DEFAULT CURRENT_DATE,
    fecha_pago DATE,
    periodo tipo_periodo NOT NULL DEFAULT 'mensual',
    anio INTEGER GENERATED ALWAYS AS (EXTRACT(YEAR FROM fecha_registro)) STORED,
    mes INTEGER GENERATED ALWAYS AS (EXTRACT(MONTH FROM fecha_registro)) STORED,
    comprobante_url VARCHAR(500),
    numero_comprobante VARCHAR(50),
    beneficiario VARCHAR(255),
    observaciones TEXT,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT monto_positivo CHECK (monto >= 0)
);

CREATE INDEX idx_datos_financieros_empresa ON datos_financieros(empresa_id);
CREATE INDEX idx_datos_financieros_tipo ON datos_financieros(tipo_dato);
CREATE INDEX idx_datos_financieros_fecha ON datos_financieros(fecha_registro DESC);
CREATE INDEX idx_datos_financieros_anio_mes ON datos_financieros(anio, mes);

COMMENT ON TABLE datos_financieros IS 'Información financiera';

-- ============================================================================
-- TABLA: catalogo_metricas
-- Catálogo de KPIs predefinidos (NO personalizables)
-- ============================================================================

CREATE TABLE catalogo_metricas (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    codigo_metrica VARCHAR(50) NOT NULL UNIQUE,
    nombre_metrica VARCHAR(100) NOT NULL,
    tipo_dashboard tipo_dashboard NOT NULL,
    tipo_metrica tipo_metrica NOT NULL,
    descripcion TEXT,
    unidad VARCHAR(50),
    icono VARCHAR(50),
    formula_sql TEXT,
    orden INTEGER DEFAULT 0,
    activa BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_catalogo_tipo_dashboard ON catalogo_metricas(tipo_dashboard);
CREATE INDEX idx_catalogo_activa ON catalogo_metricas(activa) WHERE activa = true;

COMMENT ON TABLE catalogo_metricas IS 'Catálogo de KPIs predefinidos (fijos)';

-- Insertar métricas predefinidas
INSERT INTO catalogo_metricas (codigo_metrica, nombre_metrica, tipo_dashboard, tipo_metrica, unidad, icono, orden) VALUES
    ('ventas_total_periodo', 'Ventas Totales', 'ventas', 'ventas', 'COP', 'DollarSign', 1),
    ('ventas_num_transacciones', 'Número de Transacciones', 'ventas', 'ventas', 'transacciones', 'ShoppingCart', 2),
    ('ventas_ticket_promedio', 'Ticket Promedio', 'ventas', 'ventas', 'COP', 'TrendingUp', 3),
    ('ventas_crecimiento', 'Crecimiento vs Mes Anterior', 'ventas', 'ventas', '%', 'Percent', 4),
    ('inventario_productos_activos', 'Productos Activos', 'inventarios', 'inventario', 'productos', 'Package', 1),
    ('inventario_stock_bajo', 'Productos con Stock Bajo', 'inventarios', 'inventario', 'productos', 'AlertTriangle', 2),
    ('inventario_sin_movimiento_30d', 'Sin Movimiento (30 días)', 'inventarios', 'inventario', 'productos', 'Clock', 3),
    ('inventario_valor_total', 'Valor Total Inventario', 'inventarios', 'inventario', 'COP', 'DollarSign', 4),
    ('financiero_ingresos_mes', 'Ingresos del Mes', 'financieros', 'financiero', 'COP', 'TrendingUp', 1),
    ('financiero_gastos_mes', 'Gastos del Mes', 'financieros', 'financiero', 'COP', 'TrendingDown', 2),
    ('financiero_utilidad_neta', 'Utilidad Neta', 'financieros', 'financiero', 'COP', 'DollarSign', 3),
    ('financiero_margen_bruto', 'Margen Bruto', 'financieros', 'financiero', '%', 'Percent', 4),
    ('operaciones_pedidos_pendientes', 'Pedidos Pendientes', 'operaciones', 'operacional', 'pedidos', 'Clock', 1),
    ('operaciones_pedidos_completados', 'Pedidos Completados', 'operaciones', 'operacional', 'pedidos', 'CheckCircle', 2),
    ('operaciones_tiempo_entrega', 'Tiempo Promedio Entrega', 'operaciones', 'operacional', 'días', 'TrendingUp', 3),
    ('operaciones_tasa_cumplimiento', 'Tasa de Cumplimiento', 'operaciones', 'operacional', '%', 'Percent', 4);

-- ============================================================================
-- TABLA: dashboards
-- Dashboards fijos (4 tipos: NO personalizables)
-- ============================================================================

CREATE TABLE dashboards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tipo_dashboard tipo_dashboard NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT dashboard_unico_por_tipo UNIQUE(empresa_id, tipo_dashboard)
);

CREATE INDEX idx_dashboards_empresa ON dashboards(empresa_id);
CREATE INDEX idx_dashboards_tipo ON dashboards(tipo_dashboard);
CREATE INDEX idx_dashboards_activo ON dashboards(activo) WHERE activo = true;

COMMENT ON TABLE dashboards IS 'Dashboards fijos (4 tipos) - NO personalizables';

-- ============================================================================
-- TABLA: metricas
-- Valores calculados de KPIs predefinidos
-- ============================================================================

CREATE TABLE metricas (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    dashboard_id UUID NOT NULL REFERENCES dashboards(id) ON DELETE CASCADE,
    catalogo_metrica_id UUID NOT NULL REFERENCES catalogo_metricas(id) ON DELETE RESTRICT,
    unidad VARCHAR(50),
    valor_numerico NUMERIC(15,2),
    valor_texto VARCHAR(255),
    variacion_porcentaje NUMERIC(5,2),
    variacion_valor NUMERIC(15,2),
    tendencia tipo_tendencia NOT NULL DEFAULT 'neutral',
    periodo tipo_periodo NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    fecha_calculo TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT valor_requerido CHECK (valor_numerico IS NOT NULL OR valor_texto IS NOT NULL),
    CONSTRAINT metrica_unica_por_periodo UNIQUE(dashboard_id, catalogo_metrica_id, periodo, fecha_inicio)
);

CREATE INDEX idx_metricas_dashboard ON metricas(dashboard_id);
CREATE INDEX idx_metricas_catalogo ON metricas(catalogo_metrica_id);
CREATE INDEX idx_metricas_fecha ON metricas(fecha_calculo DESC);

COMMENT ON TABLE metricas IS 'Valores calculados de KPIs predefinidos';

-- ============================================================================
-- TABLA: conversaciones_ia
-- Sesiones de chat con IA (OpenAI API)
-- ============================================================================

CREATE TABLE conversaciones_ia (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    titulo VARCHAR(255) NOT NULL,
    ultimo_mensaje TEXT,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_ultima_actualizacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    activa BOOLEAN NOT NULL DEFAULT true,
    archivada BOOLEAN NOT NULL DEFAULT false,
    etiquetas VARCHAR(100)[] DEFAULT ARRAY[]::VARCHAR[],
    favorita BOOLEAN DEFAULT false,
    contexto JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_conversaciones_empresa ON conversaciones_ia(empresa_id);
CREATE INDEX idx_conversaciones_activa ON conversaciones_ia(activa) WHERE activa = true;
CREATE INDEX idx_conversaciones_fecha ON conversaciones_ia(fecha_ultima_actualizacion DESC);

COMMENT ON TABLE conversaciones_ia IS 'Sesiones de chat con IA (OpenAI API)';

-- ============================================================================
-- TABLA: mensajes_ia
-- Mensajes individuales en conversaciones
-- ============================================================================

CREATE TABLE mensajes_ia (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    conversacion_id UUID NOT NULL REFERENCES conversaciones_ia(id) ON DELETE CASCADE,
    tipo_mensaje tipo_mensaje NOT NULL,
    contenido TEXT NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB DEFAULT '{"modelo": null, "provider": "openai", "tokens_prompt": null, "tokens_completion": null, "tokens_total": null, "costo_usd": null, "temperatura": null, "max_tokens": null, "tiempo_respuesta_ms": null, "finish_reason": null}'::jsonb,
    tokens_usados INTEGER,
    costo_estimado_usd NUMERIC(10,6),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_mensajes_conversacion ON mensajes_ia(conversacion_id);
CREATE INDEX idx_mensajes_timestamp ON mensajes_ia(timestamp DESC);
CREATE INDEX idx_mensajes_tipo ON mensajes_ia(tipo_mensaje);

COMMENT ON TABLE mensajes_ia IS 'Mensajes de IA consumiendo OpenAI API';
COMMENT ON COLUMN mensajes_ia.tokens_usados IS 'Total tokens (prompt + completion)';
COMMENT ON COLUMN mensajes_ia.costo_estimado_usd IS 'Costo según pricing OpenAI';

-- ============================================================================
-- TABLA: consumo_ia_mensual
-- Control de consumo mensual de OpenAI API
-- ============================================================================

CREATE TABLE consumo_ia_mensual (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    anio INTEGER NOT NULL CHECK (anio >= 2024),
    mes INTEGER NOT NULL CHECK (mes BETWEEN 1 AND 12),
    total_conversaciones INTEGER DEFAULT 0,
    total_mensajes INTEGER DEFAULT 0,
    total_tokens INTEGER DEFAULT 0,
    total_costo_usd NUMERIC(10,2) DEFAULT 0,
    limite_tokens_mes INTEGER,
    limite_costo_mes_usd NUMERIC(10,2),
    limite_alcanzado BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT periodo_unico UNIQUE(empresa_id, anio, mes)
);

CREATE INDEX idx_consumo_ia_empresa ON consumo_ia_mensual(empresa_id);
CREATE INDEX idx_consumo_ia_periodo ON consumo_ia_mensual(anio, mes);

COMMENT ON TABLE consumo_ia_mensual IS 'Control mensual de consumo y costos de OpenAI API';

-- ============================================================================
-- TABLA: recomendaciones_ia
-- Insights generados automáticamente por IA
-- ============================================================================

CREATE TABLE recomendaciones_ia (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tipo_recomendacion tipo_recomendacion NOT NULL,
    prioridad tipo_prioridad NOT NULL DEFAULT 'media',
    titulo VARCHAR(255) NOT NULL,
    descripcion TEXT NOT NULL,
    impacto_estimado VARCHAR(100),
    acciones_sugeridas TEXT[],
    estado tipo_estado_recomendacion NOT NULL DEFAULT 'nueva',
    fecha_generacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_vencimiento TIMESTAMPTZ,
    fecha_vista TIMESTAMPTZ,
    fecha_aplicacion TIMESTAMPTZ,
    resultados TEXT,
    efectividad_porcentaje NUMERIC(5,2),
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_recomendaciones_empresa ON recomendaciones_ia(empresa_id);
CREATE INDEX idx_recomendaciones_estado ON recomendaciones_ia(estado);
CREATE INDEX idx_recomendaciones_fecha ON recomendaciones_ia(fecha_generacion DESC);

COMMENT ON TABLE recomendaciones_ia IS 'Insights y sugerencias generadas por IA';

-- ============================================================================
-- TABLA: fuentes_datos
-- Conexiones a bases de datos externas
-- ============================================================================

CREATE TABLE fuentes_datos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    tipo_fuente tipo_fuente_datos NOT NULL,
    host VARCHAR(255),
    puerto INTEGER,
    nombre_base_datos VARCHAR(100),
    usuario VARCHAR(100),
    password_encriptado VARCHAR(500),
    ssl_habilitado BOOLEAN DEFAULT false,
    certificado_ssl TEXT,
    parametros_conexion JSONB DEFAULT '{}'::jsonb,
    estado_conexion tipo_estado_conexion DEFAULT 'desconectado',
    ultima_conexion TIMESTAMPTZ,
    ultimo_error TEXT,
    intentos_conexion_fallidos INTEGER DEFAULT 0,
    sincronizacion_automatica BOOLEAN DEFAULT false,
    frecuencia_sincronizacion VARCHAR(50),
    ultima_sincronizacion TIMESTAMPTZ,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    activa BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT puerto_valido CHECK (puerto IS NULL OR (puerto > 0 AND puerto <= 65535))
);

CREATE INDEX idx_fuentes_empresa ON fuentes_datos(empresa_id);
CREATE INDEX idx_fuentes_tipo ON fuentes_datos(tipo_fuente);
CREATE INDEX idx_fuentes_activa ON fuentes_datos(activa) WHERE activa = true;

COMMENT ON TABLE fuentes_datos IS 'Conexiones a BD externas para ETL';

-- ============================================================================
-- TABLA: importaciones_datos
-- Registro de cargas de archivos (ETL)
-- ============================================================================

CREATE TABLE importaciones_datos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    fuente_datos_id UUID REFERENCES fuentes_datos(id) ON DELETE SET NULL,
    tipo_fuente tipo_fuente_datos NOT NULL,
    tipo_datos tipo_datos_importacion NOT NULL,
    nombre_archivo VARCHAR(255) NOT NULL,
    archivo_url VARCHAR(500),
    tamano_archivo BIGINT,
    hash_archivo VARCHAR(64),
    mapeo_columnas JSONB,
    reglas_transformacion JSONB,
    estado tipo_estado_importacion NOT NULL DEFAULT 'en_proceso',
    progreso_porcentaje NUMERIC(5,2) DEFAULT 0,
    fase_actual tipo_fase_etl DEFAULT 'extraccion',
    registros_extraidos INTEGER DEFAULT 0,
    registros_transformados INTEGER DEFAULT 0,
    registros_cargados INTEGER DEFAULT 0,
    registros_rechazados INTEGER DEFAULT 0,
    resultado_carga JSONB DEFAULT '{}'::jsonb,
    fecha_importacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_inicio_etl TIMESTAMPTZ,
    fecha_fin_etl TIMESTAMPTZ,
    duracion_segundos INTEGER,
    errores_extraccion JSONB DEFAULT '[]'::jsonb,
    errores_transformacion JSONB DEFAULT '[]'::jsonb,
    errores_carga JSONB DEFAULT '[]'::jsonb,
    advertencias JSONB DEFAULT '[]'::jsonb,
    log_etl TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT tamano_valido CHECK (tamano_archivo IS NULL OR tamano_archivo > 0),
    CONSTRAINT progreso_valido CHECK (progreso_porcentaje >= 0 AND progreso_porcentaje <= 100)
);

CREATE INDEX idx_importaciones_empresa ON importaciones_datos(empresa_id);
CREATE INDEX idx_importaciones_fuente ON importaciones_datos(fuente_datos_id);
CREATE INDEX idx_importaciones_estado ON importaciones_datos(estado);
CREATE INDEX idx_importaciones_fecha ON importaciones_datos(fecha_importacion DESC);

COMMENT ON TABLE importaciones_datos IS 'Gestión de ETL - Extracción, Transformación y Carga';
COMMENT ON COLUMN importaciones_datos.mapeo_columnas IS 'Mapeo entre columnas Excel/CSV y campos BD';
COMMENT ON COLUMN importaciones_datos.fase_actual IS 'Fase ETL: extraccion → transformacion → carga → completado';

-- ============================================================================
-- TABLA: datos_crudos_temporal
-- Almacenamiento temporal durante ETL
-- ============================================================================

CREATE TABLE datos_crudos_temporal (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    importacion_id UUID NOT NULL REFERENCES importaciones_datos(id) ON DELETE CASCADE,
    numero_fila INTEGER NOT NULL,
    datos_json JSONB NOT NULL,
    estado_procesamiento VARCHAR(50) DEFAULT 'pendiente',
    errores_validacion JSONB DEFAULT '[]'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_datos_crudos_importacion ON datos_crudos_temporal(importacion_id);
CREATE INDEX idx_datos_crudos_estado ON datos_crudos_temporal(estado_procesamiento);

COMMENT ON TABLE datos_crudos_temporal IS 'Almacenamiento temporal durante ETL (se limpia después)';

-- ============================================================================
-- TABLA: notificaciones
-- Sistema de notificaciones empresariales
-- ============================================================================

CREATE TABLE notificaciones (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tipo_notificacion tipo_notificacion NOT NULL,
    prioridad tipo_prioridad NOT NULL DEFAULT 'media',
    titulo VARCHAR(255) NOT NULL,
    descripcion TEXT NOT NULL,
    leida BOOLEAN NOT NULL DEFAULT false,
    archivada BOOLEAN NOT NULL DEFAULT false,
    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_lectura TIMESTAMPTZ,
    fecha_archivado TIMESTAMPTZ,
    accion_url VARCHAR(500),
    accion_texto VARCHAR(100),
    icono VARCHAR(50),
    color VARCHAR(20),
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_notificaciones_empresa ON notificaciones(empresa_id);
CREATE INDEX idx_notificaciones_leida ON notificaciones(leida);
CREATE INDEX idx_notificaciones_fecha ON notificaciones(fecha_creacion DESC);
CREATE INDEX idx_notificaciones_no_leidas ON notificaciones(empresa_id, leida, fecha_creacion DESC) 
    WHERE leida = false AND archivada = false;

COMMENT ON TABLE notificaciones IS 'Notificaciones empresariales';

-- ============================================================================
-- TABLA: auditoria
-- Registro completo de auditoría
-- ============================================================================

CREATE TABLE auditoria (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    cuenta_empresa_id UUID REFERENCES cuenta_empresa(id) ON DELETE SET NULL,
    empresa_id UUID NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tabla_afectada VARCHAR(100) NOT NULL,
    registro_id UUID NOT NULL,
    accion tipo_accion_auditoria NOT NULL,
    datos_anteriores JSONB,
    datos_nuevos JSONB,
    cambios_detectados JSONB,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    descripcion TEXT
);

CREATE INDEX idx_auditoria_cuenta ON auditoria(cuenta_empresa_id);
CREATE INDEX idx_auditoria_empresa ON auditoria(empresa_id);
CREATE INDEX idx_auditoria_tabla ON auditoria(tabla_afectada);
CREATE INDEX idx_auditoria_registro ON auditoria(registro_id);
CREATE INDEX idx_auditoria_timestamp ON auditoria(timestamp DESC);

COMMENT ON TABLE auditoria IS 'Auditoría completa del sistema';

-- ============================================================================
-- FUNCIONES Y TRIGGERS
-- ============================================================================

-- Función: Actualizar updated_at automáticamente
CREATE OR REPLACE FUNCTION actualizar_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Aplicar triggers a todas las tablas con updated_at
CREATE TRIGGER trigger_empresas_updated_at BEFORE UPDATE ON empresas FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_cuenta_empresa_updated_at BEFORE UPDATE ON cuenta_empresa FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_configuracion_empresa_updated_at BEFORE UPDATE ON configuracion_empresa FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_suscripciones_updated_at BEFORE UPDATE ON suscripciones FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_clientes_updated_at BEFORE UPDATE ON clientes FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_categorias_updated_at BEFORE UPDATE ON categorias FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_productos_updated_at BEFORE UPDATE ON productos FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_inventario_updated_at BEFORE UPDATE ON inventario FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_analisis_productos_updated_at BEFORE UPDATE ON analisis_productos FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_ventas_updated_at BEFORE UPDATE ON ventas FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_datos_financieros_updated_at BEFORE UPDATE ON datos_financieros FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_dashboards_updated_at BEFORE UPDATE ON dashboards FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_conversaciones_ia_updated_at BEFORE UPDATE ON conversaciones_ia FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_recomendaciones_ia_updated_at BEFORE UPDATE ON recomendaciones_ia FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_fuentes_datos_updated_at BEFORE UPDATE ON fuentes_datos FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();
CREATE TRIGGER trigger_importaciones_datos_updated_at BEFORE UPDATE ON importaciones_datos FOR EACH ROW EXECUTE FUNCTION actualizar_updated_at();

-- Función: Crear configuración y dashboards automáticos
CREATE OR REPLACE FUNCTION crear_configuracion_automatica()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO configuracion_empresa (empresa_id) VALUES (NEW.id);
    
    INSERT INTO dashboards (empresa_id, tipo_dashboard) VALUES
        (NEW.id, 'ventas'),
        (NEW.id, 'financieros'),
        (NEW.id, 'inventarios'),
        (NEW.id, 'operaciones');
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_crear_configuracion_empresa
    AFTER INSERT ON empresas
    FOR EACH ROW EXECUTE FUNCTION crear_configuracion_automatica();

-- Función: Crear inventario y análisis automáticos
CREATE OR REPLACE FUNCTION crear_inventario_automatico()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.requiere_inventario = true THEN
        INSERT INTO inventario (producto_id, cantidad_disponible, stock_minimo, estado_stock)
        VALUES (NEW.id, 0, 10, 'critico');
        
        INSERT INTO analisis_productos (producto_id, dias_sin_movimiento, periodo_rotacion_dias)
        VALUES (NEW.id, 0, 90);
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_crear_inventario_producto
    AFTER INSERT ON productos
    FOR EACH ROW EXECUTE FUNCTION crear_inventario_automatico();

-- Función: Actualizar inventario al registrar venta
CREATE OR REPLACE FUNCTION actualizar_inventario_venta()
RETURNS TRIGGER AS $$
DECLARE
    v_inventario_id UUID;
    v_cantidad_actual INTEGER;
    v_empresa_id UUID;
    v_producto_nombre VARCHAR(255);
BEGIN
    SELECT inv.id, inv.cantidad_disponible, p.empresa_id, p.nombre
    INTO v_inventario_id, v_cantidad_actual, v_empresa_id, v_producto_nombre
    FROM inventario inv
    JOIN productos p ON p.id = inv.producto_id
    WHERE inv.producto_id = NEW.producto_id;
    
    IF v_inventario_id IS NULL THEN
        RAISE EXCEPTION 'No existe inventario para el producto %', NEW.producto_id;
    END IF;
    
    IF v_cantidad_actual < NEW.cantidad::INTEGER THEN
        RAISE EXCEPTION 'Stock insuficiente para "%". Disponible: %, Solicitado: %', 
            v_producto_nombre, v_cantidad_actual, NEW.cantidad::INTEGER;
    END IF;
    
    UPDATE inventario 
    SET cantidad_disponible = cantidad_disponible - NEW.cantidad::INTEGER,
        ultima_actualizacion = CURRENT_TIMESTAMP,
        ultima_salida = CURRENT_TIMESTAMP,
        estado_stock = CASE 
            WHEN (cantidad_disponible - NEW.cantidad::INTEGER) = 0 THEN 'critico'::tipo_estado_stock
            WHEN (cantidad_disponible - NEW.cantidad::INTEGER) < stock_minimo THEN 'bajo'::tipo_estado_stock
            ELSE 'normal'::tipo_estado_stock
        END
    WHERE id = v_inventario_id;
    
    INSERT INTO movimientos_inventario (
        inventario_id, empresa_id, tipo_movimiento, cantidad, cantidad_anterior, cantidad_nueva,
        motivo, venta_id, referencia, fecha_movimiento
    ) VALUES (
        v_inventario_id, v_empresa_id, 'salida', NEW.cantidad::INTEGER, v_cantidad_actual,
        v_cantidad_actual - NEW.cantidad::INTEGER, 'Venta registrada', NEW.venta_id,
        (SELECT numero_orden FROM ventas WHERE id = NEW.venta_id), CURRENT_TIMESTAMP
    );
    
    UPDATE analisis_productos
    SET fecha_ultimo_movimiento = CURRENT_TIMESTAMP, dias_sin_movimiento = 0
    WHERE producto_id = NEW.producto_id;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_actualizar_inventario_venta
    AFTER INSERT ON detalles_venta
    FOR EACH ROW EXECUTE FUNCTION actualizar_inventario_venta();

-- Función: Actualizar totales de cliente
CREATE OR REPLACE FUNCTION actualizar_totales_cliente()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.estado = 'completado' AND NEW.cliente_id IS NOT NULL THEN
        UPDATE clientes
        SET total_compras = total_compras + NEW.monto_total,
            numero_compras = numero_compras + 1,
            fecha_ultima_compra = NEW.fecha_venta
        WHERE id = NEW.cliente_id;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_actualizar_totales_cliente
    AFTER INSERT ON ventas
    FOR EACH ROW EXECUTE FUNCTION actualizar_totales_cliente();

-- Función: Notificar stock bajo
CREATE OR REPLACE FUNCTION notificar_stock_bajo()
RETURNS TRIGGER AS $$
DECLARE
    v_producto_nombre VARCHAR(255);
    v_empresa_id UUID;
BEGIN
    IF NEW.cantidad_disponible < NEW.stock_minimo AND NEW.alertas_activadas = true THEN
        SELECT p.nombre, p.empresa_id 
        INTO v_producto_nombre, v_empresa_id
        FROM productos p
        WHERE p.id = NEW.producto_id;
        
        INSERT INTO notificaciones (
            empresa_id, tipo_notificacion, prioridad, titulo, descripcion, icono, accion_url
        ) VALUES (
            v_empresa_id,
            'alerta',
            CASE 
                WHEN NEW.cantidad_disponible = 0 THEN 'critica'::tipo_prioridad
                ELSE 'alta'::tipo_prioridad
            END,
            'Alerta de Stock Bajo',
            format('El inventario del producto "%s" está bajo. Disponible: %s unidades, Mínimo: %s unidades. Se recomienda reabastecer pronto.',
                v_producto_nombre, NEW.cantidad_disponible, NEW.stock_minimo),
            'AlertTriangle',
            '/dashboard-inventarios'
        );
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_notificar_stock_bajo
    AFTER UPDATE OF cantidad_disponible ON inventario
    FOR EACH ROW EXECUTE FUNCTION notificar_stock_bajo();

-- Función: Actualizar último mensaje conversación
CREATE OR REPLACE FUNCTION actualizar_ultimo_mensaje_conversacion()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE conversaciones_ia
    SET ultimo_mensaje = LEFT(NEW.contenido, 200),
        fecha_ultima_actualizacion = NEW.timestamp
    WHERE id = NEW.conversacion_id;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_actualizar_ultimo_mensaje
    AFTER INSERT ON mensajes_ia
    FOR EACH ROW EXECUTE FUNCTION actualizar_ultimo_mensaje_conversacion();

-- Función: Bloquear cuenta tras intentos fallidos
CREATE OR REPLACE FUNCTION bloquear_cuenta_intentos_fallidos()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.intentos_fallidos >= 5 AND OLD.intentos_fallidos < 5 THEN
        NEW.bloqueada_hasta := CURRENT_TIMESTAMP + INTERVAL '30 minutes';
        
        INSERT INTO notificaciones (empresa_id, tipo_notificacion, prioridad, titulo, descripcion, icono)
        SELECT empresa_id, 'advertencia', 'critica',
            'Cuenta bloqueada temporalmente',
            'Su cuenta ha sido bloqueada temporalmente por múltiples intentos de inicio de sesión fallidos. Se desbloqueará en 30 minutos.',
            'Shield'
        FROM cuenta_empresa WHERE id = NEW.id;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_bloquear_cuenta
    BEFORE UPDATE OF intentos_fallidos ON cuenta_empresa
    FOR EACH ROW EXECUTE FUNCTION bloquear_cuenta_intentos_fallidos();

-- Función: Actualizar consumo de IA
CREATE OR REPLACE FUNCTION actualizar_consumo_ia()
RETURNS TRIGGER AS $$
DECLARE
    v_anio INTEGER;
    v_mes INTEGER;
BEGIN
    IF NEW.tipo_mensaje = 'ia' AND NEW.tokens_usados IS NOT NULL THEN
        v_anio := EXTRACT(YEAR FROM NEW.timestamp);
        v_mes := EXTRACT(MONTH FROM NEW.timestamp);
        
        INSERT INTO consumo_ia_mensual (empresa_id, anio, mes, total_mensajes, total_tokens, total_costo_usd)
        SELECT c.empresa_id, v_anio, v_mes, 1, NEW.tokens_usados, NEW.costo_estimado_usd
        FROM conversaciones_ia c
        WHERE c.id = NEW.conversacion_id
        ON CONFLICT (empresa_id, anio, mes) 
        DO UPDATE SET
            total_mensajes = consumo_ia_mensual.total_mensajes + 1,
            total_tokens = consumo_ia_mensual.total_tokens + EXCLUDED.total_tokens,
            total_costo_usd = consumo_ia_mensual.total_costo_usd + COALESCE(EXCLUDED.total_costo_usd, 0),
            updated_at = CURRENT_TIMESTAMP;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_actualizar_consumo_ia
    AFTER INSERT ON mensajes_ia
    FOR EACH ROW EXECUTE FUNCTION actualizar_consumo_ia();

-- Función: Auditar cambios
CREATE OR REPLACE FUNCTION auditar_cambios()
RETURNS TRIGGER AS $$
DECLARE
    v_cuenta_empresa_id UUID;
    v_empresa_id UUID;
    v_cambios JSONB;
BEGIN
    BEGIN
        v_cuenta_empresa_id := current_setting('app.current_cuenta_id')::UUID;
    EXCEPTION WHEN OTHERS THEN
        v_cuenta_empresa_id := NULL;
    END;
    
    IF TG_OP = 'DELETE' THEN
        v_empresa_id := OLD.empresa_id;
    ELSE
        v_empresa_id := NEW.empresa_id;
    END IF;
    
    IF (TG_OP = 'DELETE') THEN
        INSERT INTO auditoria (cuenta_empresa_id, empresa_id, tabla_afectada, registro_id, accion, datos_anteriores)
        VALUES (v_cuenta_empresa_id, v_empresa_id, TG_TABLE_NAME, OLD.id, 'eliminar', row_to_json(OLD)::jsonb);
        RETURN OLD;
    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO auditoria (cuenta_empresa_id, empresa_id, tabla_afectada, registro_id, accion, datos_anteriores, datos_nuevos)
        VALUES (v_cuenta_empresa_id, v_empresa_id, TG_TABLE_NAME, NEW.id, 'actualizar', row_to_json(OLD)::jsonb, row_to_json(NEW)::jsonb);
        RETURN NEW;
    ELSIF (TG_OP = 'INSERT') THEN
        INSERT INTO auditoria (cuenta_empresa_id, empresa_id, tabla_afectada, registro_id, accion, datos_nuevos)
        VALUES (v_cuenta_empresa_id, v_empresa_id, TG_TABLE_NAME, NEW.id, 'crear', row_to_json(NEW)::jsonb);
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Aplicar auditoría a tablas críticas
CREATE TRIGGER trigger_auditar_ventas AFTER INSERT OR UPDATE OR DELETE ON ventas FOR EACH ROW EXECUTE FUNCTION auditar_cambios();
CREATE TRIGGER trigger_auditar_productos AFTER INSERT OR UPDATE OR DELETE ON productos FOR EACH ROW EXECUTE FUNCTION auditar_cambios();
CREATE TRIGGER trigger_auditar_clientes AFTER INSERT OR UPDATE OR DELETE ON clientes FOR EACH ROW EXECUTE FUNCTION auditar_cambios();
CREATE TRIGGER trigger_auditar_datos_financieros AFTER INSERT OR UPDATE OR DELETE ON datos_financieros FOR EACH ROW EXECUTE FUNCTION auditar_cambios();
CREATE TRIGGER trigger_auditar_empresas AFTER INSERT OR UPDATE OR DELETE ON empresas FOR EACH ROW EXECUTE FUNCTION auditar_cambios();

-- ============================================================================
-- PROCEDIMIENTOS ALMACENADOS
-- ============================================================================

-- Procedimiento: Calcular métricas de ventas
CREATE OR REPLACE PROCEDURE calcular_metricas_ventas(
    p_empresa_id UUID,
    p_periodo tipo_periodo,
    p_dashboard_id UUID
)
LANGUAGE plpgsql AS $$
DECLARE
    v_total_ventas NUMERIC(15,2);
    v_num_transacciones INTEGER;
    v_ticket_promedio NUMERIC(15,2);
    v_crecimiento_porcentaje NUMERIC(5,2);
    v_fecha_inicio DATE;
    v_fecha_fin DATE;
    v_periodo_anterior_inicio DATE;
    v_periodo_anterior_fin DATE;
    v_total_periodo_anterior NUMERIC(15,2);
BEGIN
    v_fecha_fin := CURRENT_DATE;
    
    CASE p_periodo
        WHEN 'diario' THEN
            v_fecha_inicio := CURRENT_DATE;
            v_periodo_anterior_inicio := CURRENT_DATE - INTERVAL '1 day';
            v_periodo_anterior_fin := CURRENT_DATE - INTERVAL '1 day';
        WHEN 'semanal' THEN
            v_fecha_inicio := CURRENT_DATE - INTERVAL '7 days';
            v_periodo_anterior_inicio := CURRENT_DATE - INTERVAL '14 days';
            v_periodo_anterior_fin := CURRENT_DATE - INTERVAL '8 days';
        WHEN 'mensual' THEN
            v_fecha_inicio := CURRENT_DATE - INTERVAL '1 month';
            v_periodo_anterior_inicio := CURRENT_DATE - INTERVAL '2 months';
            v_periodo_anterior_fin := CURRENT_DATE - INTERVAL '1 month' - INTERVAL '1 day';
        WHEN 'trimestral' THEN
            v_fecha_inicio := CURRENT_DATE - INTERVAL '3 months';
            v_periodo_anterior_inicio := CURRENT_DATE - INTERVAL '6 months';
            v_periodo_anterior_fin := CURRENT_DATE - INTERVAL '3 months' - INTERVAL '1 day';
        WHEN 'anual' THEN
            v_fecha_inicio := CURRENT_DATE - INTERVAL '1 year';
            v_periodo_anterior_inicio := CURRENT_DATE - INTERVAL '2 years';
            v_periodo_anterior_fin := CURRENT_DATE - INTERVAL '1 year' - INTERVAL '1 day';
    END CASE;
    
    SELECT 
        COALESCE(SUM(monto_total), 0),
        COUNT(*),
        COALESCE(AVG(monto_total), 0)
    INTO v_total_ventas, v_num_transacciones, v_ticket_promedio
    FROM ventas
    WHERE empresa_id = p_empresa_id
    AND fecha_venta::DATE BETWEEN v_fecha_inicio AND v_fecha_fin
    AND estado = 'completado';
    
    SELECT COALESCE(SUM(monto_total), 0)
    INTO v_total_periodo_anterior
    FROM ventas
    WHERE empresa_id = p_empresa_id
    AND fecha_venta::DATE BETWEEN v_periodo_anterior_inicio AND v_periodo_anterior_fin
    AND estado = 'completado';
    
    IF v_total_periodo_anterior > 0 THEN
        v_crecimiento_porcentaje := ((v_total_ventas - v_total_periodo_anterior) / v_total_periodo_anterior * 100);
    ELSE
        v_crecimiento_porcentaje := 100;
    END IF;
    
    INSERT INTO metricas (
        dashboard_id, catalogo_metrica_id, valor_numerico, unidad, variacion_porcentaje, tendencia, periodo, fecha_inicio, fecha_fin
    )
    SELECT
        p_dashboard_id,
        cm.id,
        CASE cm.codigo_metrica
            WHEN 'ventas_total_periodo' THEN v_total_ventas
            WHEN 'ventas_num_transacciones' THEN v_num_transacciones
            WHEN 'ventas_ticket_promedio' THEN v_ticket_promedio
            WHEN 'ventas_crecimiento' THEN v_crecimiento_porcentaje
        END,
        cm.unidad,
        v_crecimiento_porcentaje,
        CASE WHEN v_crecimiento_porcentaje > 0 THEN 'up'::tipo_tendencia 
             WHEN v_crecimiento_porcentaje < 0 THEN 'down'::tipo_tendencia
             ELSE 'neutral'::tipo_tendencia END,
        p_periodo,
        v_fecha_inicio,
        v_fecha_fin
    FROM catalogo_metricas cm
    WHERE cm.tipo_dashboard = 'ventas'
    AND cm.codigo_metrica IN ('ventas_total_periodo', 'ventas_num_transacciones', 'ventas_ticket_promedio', 'ventas_crecimiento')
    ON CONFLICT (dashboard_id, catalogo_metrica_id, periodo, fecha_inicio) 
    DO UPDATE SET
        valor_numerico = EXCLUDED.valor_numerico,
        variacion_porcentaje = EXCLUDED.variacion_porcentaje,
        tendencia = EXCLUDED.tendencia,
        fecha_calculo = CURRENT_TIMESTAMP;
END;
$$;

-- Procedimiento: Limpiar datos temporales ETL
CREATE OR REPLACE PROCEDURE limpiar_datos_temporales_etl(p_importacion_id UUID)
LANGUAGE plpgsql AS $$
BEGIN
    DELETE FROM datos_crudos_temporal WHERE importacion_id = p_importacion_id;
    RAISE NOTICE 'Datos temporales de importación % limpiados', p_importacion_id;
END;
$$;

-- Procedimiento: Validar suscripción activa
CREATE OR REPLACE FUNCTION validar_suscripcion_activa(p_empresa_id UUID)
RETURNS BOOLEAN AS $$
DECLARE
    v_tiene_suscripcion_activa BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1 
        FROM suscripciones 
        WHERE empresa_id = p_empresa_id 
        AND estado IN ('activa', 'trial')
        AND (fecha_fin IS NULL OR fecha_fin >= CURRENT_DATE)
    ) INTO v_tiene_suscripcion_activa;
    
    RETURN v_tiene_suscripcion_activa;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- VISTAS ÚTILES
-- ============================================================================

-- Vista: Empresa completa (para autenticación)
CREATE OR REPLACE VIEW vista_empresa_completa AS
SELECT 
    e.id AS empresa_id,
    e.nombre_comercial,
    e.razon_social,
    e.nit,
    e.sector,
    e.tamano,
    e.director_nombre_completo,
    e.director_cargo,
    e.logo_url,
    e.ciudad,
    e.departamento,
    e.activa AS empresa_activa,
    
    ce.id AS cuenta_id,
    ce.email AS cuenta_email,
    -- Campos de perfil expuestos para el frontend (Settings / HomePage / Sidebar)
    ce.nombre_completo AS cuenta_nombre_completo,
    ce.display_name AS cuenta_display_name,
    ce.avatar_url AS cuenta_avatar_url,
    ce.telefono AS cuenta_telefono,
    ce.rol AS cuenta_rol,
    ce.es_owner AS cuenta_es_owner,
    ce.bio AS cuenta_bio,
    ce.ubicacion AS cuenta_ubicacion,
    ce.activa AS cuenta_activa,
    ce.verificada AS cuenta_verificada,
    ce.ultima_sesion,
    
    conf.tema,
    conf.idioma,
    conf.zona_horaria,
    conf.moneda,
    
    s.plan AS plan_actual,
    s.estado AS estado_suscripcion,
    s.fecha_fin AS fecha_fin_suscripcion,
    
    (SELECT COUNT(*) FROM notificaciones n 
     WHERE n.empresa_id = e.id AND n.leida = false) AS notificaciones_pendientes
     
FROM empresas e
LEFT JOIN cuenta_empresa ce ON ce.empresa_id = e.id
LEFT JOIN configuracion_empresa conf ON conf.empresa_id = e.id
LEFT JOIN LATERAL (
    SELECT plan, estado, fecha_fin 
    FROM suscripciones 
    WHERE empresa_id = e.id 
    ORDER BY fecha_inicio DESC 
    LIMIT 1
) s ON true;

COMMENT ON VIEW vista_empresa_completa IS 'Vista completa para login y sesión';

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================

-- Análisis de la BD
ANALYZE;

-- Mensaje de confirmación
DO $$
BEGIN
    RAISE NOTICE '================================================================================';
    RAISE NOTICE '✅ Base de datos ANALITIK creada exitosamente';
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'Características:';
    RAISE NOTICE '   • Modelo corporativo: Cuenta única por empresa';
    RAISE NOTICE '   • Dashboards: 4 tipos fijos (Ventas, Financieros, Inventarios, Operaciones)';
    RAISE NOTICE '   • KPIs: Catálogo predefinido de 16 métricas';
    RAISE NOTICE '   • ETL: Sistema de importación con transformación de datos';
    RAISE NOTICE '   • IA: Control de consumo OpenAI API con tracking de tokens y costos';
    RAISE NOTICE '   • Auditoría: Registro completo de operaciones';
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'Compatible con: PostgreSQL 15+';
    RAISE NOTICE 'Backend: Node.js/Express';
    RAISE NOTICE 'Versión del modelo: 4.0 FINAL';
    RAISE NOTICE '================================================================================';
END $$;
