using System;
using System.Collections.Generic;
using Analitik_MVC.Enums;
using Analitik_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Analitik_MVC.Data;

public partial class AnalitikDbContext : DbContext
{
    public AnalitikDbContext()
    {
    }

    public AnalitikDbContext(DbContextOptions<AnalitikDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnalisisProducto> AnalisisProductos { get; set; }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<CatalogoMetrica> CatalogoMetricas { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<ConfiguracionEmpresa> ConfiguracionEmpresas { get; set; }

    public virtual DbSet<ConsumoIaMensual> ConsumoIaMensuals { get; set; }

    public virtual DbSet<ConversacionesIum> ConversacionesIa { get; set; }

    public virtual DbSet<CuentaEmpresa> CuentaEmpresas { get; set; }

    public virtual DbSet<Dashboard> Dashboards { get; set; }

    public virtual DbSet<DatosCrudosTemporal> DatosCrudosTemporals { get; set; }

    public virtual DbSet<DatosFinanciero> DatosFinancieros { get; set; }

    public virtual DbSet<DetallesVentum> DetallesVenta { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<FuentesDato> FuentesDatos { get; set; }

    public virtual DbSet<ImportacionesDato> ImportacionesDatos { get; set; }

    public virtual DbSet<ImportacionesDatosLogs> ImportacionesDatosLogs { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<MensajesIum> MensajesIa { get; set; }

    public virtual DbSet<Metrica> Metricas { get; set; }

    public virtual DbSet<MovimientosInventario> MovimientosInventarios { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<RecomendacionesIum> RecomendacionesIa { get; set; }

    public virtual DbSet<Suscripcione> Suscripciones { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    public virtual DbSet<VistaEmpresaCompletum> VistaEmpresaCompleta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Analitik;Username=postgres;Password=Camilo1307;Include Error Detail=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("tipo_accion_auditoria", new[] { "crear", "actualizar", "eliminar" })
            .HasPostgresEnum("tipo_cliente", new[] { "b2b", "b2c", "eventual" })
            .HasPostgresEnum("tipo_dashboard", new[] { "ventas", "financieros", "inventarios", "operaciones" })
            .HasPostgresEnum("tipo_dato_financiero", new[] { "ingreso", "gasto", "costo", "inversion" })
            .HasPostgresEnum("tipo_datos_importacion", new[] { "ventas", "productos", "inventario", "clientes", "financieros", "mixto" })
            .HasPostgresEnum("tipo_estado_conexion", new[] { "conectado", "desconectado", "error", "probando" })
            .HasPostgresEnum("tipo_estado_importacion", new[] { "en_proceso", "completado", "fallido", "cancelado" })
            .HasPostgresEnum("tipo_estado_recomendacion", new[] { "nueva", "vista", "aplicada", "descartada" })
            .HasPostgresEnum("tipo_estado_stock", new[] { "normal", "bajo", "critico", "exceso" })
            .HasPostgresEnum("tipo_estado_suscripcion", new[] { "activa", "suspendida", "cancelada", "trial" })
            .HasPostgresEnum("tipo_estado_venta", new[] { "pendiente", "completado", "cancelado", "devuelto" })
            .HasPostgresEnum("tipo_fase_etl", new[] { "extraccion", "transformacion", "carga", "completado", "error" })
            .HasPostgresEnum("tipo_frecuencia_reporte", new[] { "diaria", "semanal", "mensual", "nunca" })
            .HasPostgresEnum("tipo_fuente_datos", new[] { "postgresql", "mysql", "mariadb", "sqlserver", "oracle", "excel", "csv", "api", "manual" })
            .HasPostgresEnum("tipo_idioma", new[] { "es", "en", "pt" })
            .HasPostgresEnum("tipo_mensaje", new[] { "usuario", "ia" })
            .HasPostgresEnum("tipo_metodo_pago", new[] { "efectivo", "tarjeta", "transferencia", "credito", "cheque" })
            .HasPostgresEnum("tipo_metrica", new[] { "ventas", "financiero", "inventario", "operacional" })
            .HasPostgresEnum("tipo_movimiento_inventario", new[] { "entrada", "salida", "ajuste", "devolucion", "transferencia" })
            .HasPostgresEnum("tipo_notificacion", new[] { "alerta", "info", "exito", "advertencia" })
            .HasPostgresEnum("tipo_periodo", new[] { "diario", "semanal", "mensual", "trimestral", "anual" })
            .HasPostgresEnum("tipo_plan_suscripcion", new[] { "basico", "profesional", "empresarial" })
            .HasPostgresEnum("tipo_prioridad", new[] { "baja", "media", "alta", "critica" })
            .HasPostgresEnum("tipo_recomendacion", new[] { "alerta", "oportunidad", "optimizacion", "tendencia" })
            .HasPostgresEnum<SectorEmpresa>("tipo_sector_empresa")
            .HasPostgresEnum<TamanoEmpresa>("tipo_tamano_empresa")    
            .HasPostgresEnum("tipo_tema", new[] { "claro", "oscuro", "automatico" })
            .HasPostgresEnum("tipo_tendencia", new[] { "up", "down", "neutral" })
            .HasPostgresEnum("tipo_tendencia_producto", new[] { "creciente", "estable", "decreciente" })
            .HasPostgresExtension("btree_gin")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp");



        modelBuilder.Entity<AnalisisProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("analisis_productos_pkey");

            entity.ToTable("analisis_productos", tb => tb.HasComment("Métricas de productos"));

            entity.HasIndex(e => e.ProductoId, "analisis_productos_producto_id_key").IsUnique();

            entity.HasIndex(e => e.DiasSinMovimiento, "idx_analisis_dias_sin_mov").IsDescending();

            entity.HasIndex(e => e.ProductoId, "idx_analisis_producto");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ClasificacionAbc)
                .HasMaxLength(1)
                .HasColumnName("clasificacion_abc");
            entity.Property(e => e.ContribucionVentasPorcentaje)
                .HasPrecision(5, 2)
                .HasColumnName("contribucion_ventas_porcentaje");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DiasSinMovimiento)
                .HasDefaultValue(0)
                .HasColumnName("dias_sin_movimiento");
            entity.Property(e => e.DiasStockDisponible).HasColumnName("dias_stock_disponible");
            entity.Property(e => e.FechaCalculo)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_calculo");
            entity.Property(e => e.FechaUltimoMovimiento).HasColumnName("fecha_ultimo_movimiento");
            entity.Property(e => e.IndiceRotacion)
                .HasPrecision(10, 2)
                .HasColumnName("indice_rotacion");
            entity.Property(e => e.IngresosUltimos30Dias)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("ingresos_ultimos_30_dias");
            entity.Property(e => e.IngresosUltimos90Dias)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("ingresos_ultimos_90_dias");
            entity.Property(e => e.Tendencia)
                .HasConversion<string>()
                .HasColumnName("tendencia");
            entity.Property(e => e.PeriodoRotacionDias)
                .HasDefaultValue(90)
                .HasColumnName("periodo_rotacion_dias");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.ProximaActualizacion).HasColumnName("proxima_actualizacion");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValorInmovilizado)
                .HasPrecision(15, 2)
                .HasColumnName("valor_inmovilizado");
            entity.Property(e => e.VariacionVentasPorcentaje)
                .HasPrecision(5, 2)
                .HasColumnName("variacion_ventas_porcentaje");
            entity.Property(e => e.VentaPromedioDiaria)
                .HasPrecision(10, 2)
                .HasColumnName("venta_promedio_diaria");
            entity.Property(e => e.VentaPromedioSemanal)
                .HasPrecision(10, 2)
                .HasColumnName("venta_promedio_semanal");
            entity.Property(e => e.VentasUltimos30Dias)
                .HasDefaultValue(0)
                .HasColumnName("ventas_ultimos_30_dias");
            entity.Property(e => e.VentasUltimos365Dias)
                .HasDefaultValue(0)
                .HasColumnName("ventas_ultimos_365_dias");
            entity.Property(e => e.VentasUltimos90Dias)
                .HasDefaultValue(0)
                .HasColumnName("ventas_ultimos_90_dias");

            entity.HasOne(d => d.Producto).WithOne(p => p.AnalisisProducto)
                .HasForeignKey<AnalisisProducto>(d => d.ProductoId)
                .HasConstraintName("analisis_productos_producto_id_fkey");
        });

        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("auditoria_pkey");

            entity.ToTable("auditoria", tb => tb.HasComment("Auditoría completa del sistema"));

            entity.HasIndex(e => e.CuentaEmpresaId, "idx_auditoria_cuenta");

            entity.HasIndex(e => e.EmpresaId, "idx_auditoria_empresa");

            entity.HasIndex(e => e.RegistroId, "idx_auditoria_registro");

            entity.HasIndex(e => e.TablaAfectada, "idx_auditoria_tabla");

            entity.HasIndex(e => e.Timestamp, "idx_auditoria_timestamp").IsDescending();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CambiosDetectados)
                .HasColumnType("jsonb")
                .HasColumnName("cambios_detectados");
            entity.Property(e => e.CuentaEmpresaId).HasColumnName("cuenta_empresa_id");
            entity.Property(e => e.Accion)
                .HasConversion<string>()
                .HasColumnName("accion");
            entity.Property(e => e.DatosAnteriores)
                .HasColumnType("jsonb")
                .HasColumnName("datos_anteriores");
            entity.Property(e => e.DatosNuevos)
                .HasColumnType("jsonb")
                .HasColumnName("datos_nuevos");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.RegistroId).HasColumnName("registro_id");
            entity.Property(e => e.TablaAfectada)
                .HasMaxLength(100)
                .HasColumnName("tabla_afectada");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("timestamp");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");

            entity.HasOne(d => d.CuentaEmpresa).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.CuentaEmpresaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("auditoria_cuenta_empresa_id_fkey");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("auditoria_empresa_id_fkey");
        });

        modelBuilder.Entity<CatalogoMetrica>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("catalogo_metricas_pkey");

            entity.ToTable("catalogo_metricas", tb => tb.HasComment("Catálogo de KPIs predefinidos (fijos)"));

            entity.HasIndex(e => e.CodigoMetrica, "catalogo_metricas_codigo_metrica_key").IsUnique();

            entity.HasIndex(e => e.Activa, "idx_catalogo_activa").HasFilter("(activa = true)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.TipoDashboard)
                .HasConversion<string>()
                .HasColumnName("tipo_dashboard");
            entity.Property(e => e.TipoMetrica)
                .HasConversion<string>()
                .HasColumnName("tipo_metrica");
            entity.Property(e => e.CodigoMetrica)
                .HasMaxLength(50)
                .HasColumnName("codigo_metrica");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FormulaSql).HasColumnName("formula_sql");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasColumnName("icono");
            entity.Property(e => e.NombreMetrica)
                .HasMaxLength(100)
                .HasColumnName("nombre_metrica");
            entity.Property(e => e.Orden)
                .HasDefaultValue(0)
                .HasColumnName("orden");
            entity.Property(e => e.Unidad)
                .HasMaxLength(50)
                .HasColumnName("unidad");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categorias_pkey");

            entity.ToTable("categorias", tb => tb.HasComment("Categorías de productos"));

            entity.HasIndex(e => new { e.EmpresaId, e.CodigoCategoria }, "codigo_categoria_unico").IsUnique();

            entity.HasIndex(e => e.Activa, "idx_categorias_activa").HasFilter("(activa = true)");

            entity.HasIndex(e => e.EmpresaId, "idx_categorias_empresa");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CategoriaPadreId).HasColumnName("categoria_padre_id");
            entity.Property(e => e.CodigoCategoria)
                .HasMaxLength(50)
                .HasColumnName("codigo_categoria");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasColumnName("icono");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden)
                .HasDefaultValue(0)
                .HasColumnName("orden");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CategoriaPadre).WithMany(p => p.InverseCategoriaPadre)
                .HasForeignKey(d => d.CategoriaPadreId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("categorias_categoria_padre_id_fkey");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Categoria)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("categorias_empresa_id_fkey");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clientes_pkey");

            entity.ToTable("clientes", tb => tb.HasComment("Base de clientes"));

            entity.HasIndex(e => new { e.EmpresaId, e.CodigoCliente }, "codigo_cliente_unico").IsUnique();

            entity.HasIndex(e => e.Activo, "idx_clientes_activo").HasFilter("(activo = true)");

            entity.HasIndex(e => e.CodigoCliente, "idx_clientes_codigo");

            entity.HasIndex(e => e.EmpresaId, "idx_clientes_empresa");

            entity.HasIndex(e => e.Nombre, "idx_clientes_nombre_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.TipoCliente)
                .HasConversion<string>()
                .HasColumnName("tipo_cliente");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");
            entity.Property(e => e.CodigoCliente)
                .HasMaxLength(50)
                .HasColumnName("codigo_cliente");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(20)
                .HasColumnName("codigo_postal");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .HasColumnName("departamento");
            entity.Property(e => e.Direccion).HasColumnName("direccion");
            entity.Property(e => e.DocumentoIdentificacion)
                .HasMaxLength(50)
                .HasColumnName("documento_identificacion");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FechaUltimaCompra).HasColumnName("fecha_ultima_compra");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.Notas).HasColumnName("notas");
            entity.Property(e => e.NumeroCompras)
                .HasDefaultValue(0)
                .HasColumnName("numero_compras");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.TelefonoAlternativo)
                .HasMaxLength(20)
                .HasColumnName("telefono_alternativo");
            entity.Property(e => e.TotalCompras)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_compras");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("clientes_empresa_id_fkey");
        });

        modelBuilder.Entity<ConfiguracionEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("configuracion_empresa_pkey");

            entity.ToTable("configuracion_empresa", tb => tb.HasComment("Preferencias empresariales"));

            entity.HasIndex(e => e.EmpresaId, "configuracion_empresa_empresa_id_key").IsUnique();

            entity.HasIndex(e => e.EmpresaId, "idx_configuracion_empresa");

            entity.HasIndex(e => e.ConfiguracionPrivacidad, "idx_configuracion_privacidad").HasMethod("gin");

            entity.HasIndex(e => e.ConfiguracionReportes, "idx_configuracion_reportes").HasMethod("gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ConfiguracionAlertas)
                .HasDefaultValueSql("'{\"stock_minimo_global\": 10, \"dias_vencimiento_alerta\": 30}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("configuracion_alertas");
            entity.Property(e => e.ConfiguracionAvanzada)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("configuracion_avanzada");
            entity.Property(e => e.ConfiguracionPrivacidad)
                .HasDefaultValueSql("'{\"analytics\": true, \"compartir_datos\": false, \"reportes_errores\": true, \"visibilidad_perfil\": \"privado\"}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("configuracion_privacidad");
            entity.Property(e => e.ConfiguracionReportes)
                .HasDefaultValueSql("'{\"incluir_graficos\": true, \"formato_exportacion\": \"pdf\", \"incluir_datos_brutos\": false}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("configuracion_reportes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Tema)
                .HasConversion<string>()
                .HasColumnName("tema");
            entity.Property(e => e.Idioma)
                .HasConversion<string>()
                .HasColumnName("idioma");
            entity.Property(e => e.FrecuenciaReportes)
                .HasConversion<string>()
                .HasColumnName("frecuencia_reportes");
            entity.Property(e => e.FormatoFecha)
                .HasMaxLength(20)
                .HasDefaultValueSql("'DD/MM/YYYY'::character varying")
                .HasColumnName("formato_fecha");
            entity.Property(e => e.FormatoNumero)
                .HasMaxLength(20)
                .HasDefaultValueSql("'es-CO'::character varying")
                .HasColumnName("formato_numero");
            entity.Property(e => e.Moneda)
                .HasMaxLength(10)
                .HasDefaultValueSql("'COP'::character varying")
                .HasColumnName("moneda");
            entity.Property(e => e.NotificacionesApp)
                .HasDefaultValue(true)
                .HasColumnName("notificaciones_app");
            entity.Property(e => e.NotificacionesEmailAlertas)
                .HasDefaultValue(true)
                .HasColumnName("notificaciones_email_alertas");
            entity.Property(e => e.NotificacionesEmailReportes)
                .HasDefaultValue(true)
                .HasColumnName("notificaciones_email_reportes");
            entity.Property(e => e.NotificacionesMarketing)
                .HasDefaultValue(false)
                .HasColumnName("notificaciones_marketing");
            entity.Property(e => e.NotificacionesPush)
                .HasDefaultValue(true)
                .HasColumnName("notificaciones_push");
            entity.Property(e => e.NotificacionesSeguridad)
                .HasDefaultValue(true)
                .HasColumnName("notificaciones_seguridad");
            entity.Property(e => e.PrimeraDiaSemana)
                .HasDefaultValue(1)
                .HasColumnName("primera_dia_semana");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.ZonaHoraria)
                .HasMaxLength(50)
                .HasDefaultValueSql("'America/Bogota'::character varying")
                .HasColumnName("zona_horaria");

            entity.HasOne(d => d.Empresa).WithOne(p => p.ConfiguracionEmpresa)
                .HasForeignKey<ConfiguracionEmpresa>(d => d.EmpresaId)
                .HasConstraintName("configuracion_empresa_empresa_id_fkey");
        });

        modelBuilder.Entity<ConsumoIaMensual>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("consumo_ia_mensual_pkey");

            entity.ToTable("consumo_ia_mensual", tb => tb.HasComment("Control mensual de consumo y costos de OpenAI API"));

            entity.HasIndex(e => e.EmpresaId, "idx_consumo_ia_empresa");

            entity.HasIndex(e => new { e.Anio, e.Mes }, "idx_consumo_ia_periodo");

            entity.HasIndex(e => new { e.EmpresaId, e.Anio, e.Mes }, "periodo_unico").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.LimiteAlcanzado)
                .HasDefaultValue(false)
                .HasColumnName("limite_alcanzado");
            entity.Property(e => e.LimiteCostoMesUsd)
                .HasPrecision(10, 2)
                .HasColumnName("limite_costo_mes_usd");
            entity.Property(e => e.LimiteTokensMes).HasColumnName("limite_tokens_mes");
            entity.Property(e => e.Mes).HasColumnName("mes");
            entity.Property(e => e.TotalConversaciones)
                .HasDefaultValue(0)
                .HasColumnName("total_conversaciones");
            entity.Property(e => e.TotalCostoUsd)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_costo_usd");
            entity.Property(e => e.TotalMensajes)
                .HasDefaultValue(0)
                .HasColumnName("total_mensajes");
            entity.Property(e => e.TotalTokens)
                .HasDefaultValue(0)
                .HasColumnName("total_tokens");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ConsumoIaMensuals)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("consumo_ia_mensual_empresa_id_fkey");
        });

        modelBuilder.Entity<ConversacionesIum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conversaciones_ia_pkey");

            entity.ToTable("conversaciones_ia", tb => tb.HasComment("Sesiones de chat con IA (OpenAI API)"));

            entity.HasIndex(e => e.Activa, "idx_conversaciones_activa").HasFilter("(activa = true)");

            entity.HasIndex(e => e.EmpresaId, "idx_conversaciones_empresa");

            entity.HasIndex(e => e.FechaUltimaActualizacion, "idx_conversaciones_fecha").IsDescending();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Archivada)
                .HasDefaultValue(false)
                .HasColumnName("archivada");
            entity.Property(e => e.Contexto)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("contexto");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Etiquetas)
                .HasDefaultValueSql("ARRAY[]::character varying[]")
                .HasColumnType("character varying(100)[]")
                .HasColumnName("etiquetas");
            entity.Property(e => e.Favorita)
                .HasDefaultValue(false)
                .HasColumnName("favorita");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaUltimaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_ultima_actualizacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .HasColumnName("titulo");
            entity.Property(e => e.UltimoMensaje).HasColumnName("ultimo_mensaje");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ConversacionesIa)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("conversaciones_ia_empresa_id_fkey");
        });

        modelBuilder.Entity<CuentaEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cuenta_empresa_pkey");

            entity.ToTable("cuenta_empresa", tb => tb.HasComment("Cuenta única de autenticación por empresa (1:1 con empresas)"));

            entity.HasIndex(e => e.Email, "cuenta_empresa_email_key").IsUnique();

            entity.HasIndex(e => e.EmpresaId, "cuenta_empresa_empresa_id_key").IsUnique();

            entity.HasIndex(e => e.Activa, "idx_cuenta_empresa_activa").HasFilter("(activa = true)");

            entity.HasIndex(e => e.Email, "idx_cuenta_empresa_email");

            entity.HasIndex(e => e.EmpresaId, "idx_cuenta_empresa_empresa");

            entity.HasIndex(e => e.TokenRecuperacion, "idx_cuenta_empresa_token_recuperacion").HasFilter("(token_recuperacion IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.BloqueadaHasta).HasColumnName("bloqueada_hasta");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerificadoEn).HasColumnName("email_verificado_en");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.EsOwner)
                .HasDefaultValue(true)
                .HasColumnName("es_owner");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaUltimoCambioPassword).HasColumnName("fecha_ultimo_cambio_password");
            entity.Property(e => e.IntentosFallidos)
                .HasDefaultValue(0)
                .HasColumnName("intentos_fallidos");
            entity.Property(e => e.IpUltimaSesion)
                .HasMaxLength(45)
                .HasColumnName("ip_ultima_sesion");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(255)
                .HasColumnName("nombre_completo");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RefreshTokenExpiracion).HasColumnName("refresh_token_expiracion");
            entity.Property(e => e.RefreshTokenHash)
                .HasMaxLength(255)
                .HasColumnName("refresh_token_hash");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .HasDefaultValueSql("'admin'::character varying")
                .HasColumnName("rol");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.TokenExpiracion).HasColumnName("token_expiracion");
            entity.Property(e => e.TokenRecuperacion)
                .HasMaxLength(255)
                .HasColumnName("token_recuperacion");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(255)
                .HasColumnName("ubicacion");
            entity.Property(e => e.UltimaSesion).HasColumnName("ultima_sesion");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Verificada)
                .HasDefaultValue(false)
                .HasColumnName("verificada");

            entity.HasOne(d => d.Empresa).WithOne(p => p.CuentaEmpresa)
                .HasForeignKey<CuentaEmpresa>(d => d.EmpresaId)
                .HasConstraintName("cuenta_empresa_empresa_id_fkey");
        });

        modelBuilder.Entity<Dashboard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dashboards_pkey");

            entity.ToTable("dashboards", tb => tb.HasComment("Dashboards fijos (4 tipos) - NO personalizables"));

            entity.HasIndex(e => e.Activo, "idx_dashboards_activo").HasFilter("(activo = true)");

            entity.HasIndex(e => e.EmpresaId, "idx_dashboards_empresa");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.TipoDashboard)
                .HasConversion<string>()
                .HasColumnName("tipo_dashboard");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Dashboards)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("dashboards_empresa_id_fkey");
        });

        modelBuilder.Entity<DatosCrudosTemporal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("datos_crudos_temporal_pkey");

            entity.ToTable("datos_crudos_temporal", tb => tb.HasComment("Almacenamiento temporal durante ETL (se limpia después)"));

            entity.HasIndex(e => e.EstadoProcesamiento, "idx_datos_crudos_estado");

            entity.HasIndex(e => e.ImportacionId, "idx_datos_crudos_importacion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DatosJson)
                .HasColumnType("jsonb")
                .HasColumnName("datos_json");
            entity.Property(e => e.ErroresValidacion)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("errores_validacion");
            entity.Property(e => e.EstadoProcesamiento)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pendiente'::character varying")
                .HasColumnName("estado_procesamiento");
            entity.Property(e => e.ImportacionId).HasColumnName("importacion_id");
            entity.Property(e => e.NumeroFila).HasColumnName("numero_fila");

            entity.HasOne(d => d.Importacion).WithMany(p => p.DatosCrudosTemporals)
                .HasForeignKey(d => d.ImportacionId)
                .HasConstraintName("datos_crudos_temporal_importacion_id_fkey");
        });

        modelBuilder.Entity<DatosFinanciero>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("datos_financieros_pkey");

            entity.ToTable("datos_financieros", tb => tb.HasComment("Información financiera"));

            entity.HasIndex(e => new { e.Anio, e.Mes }, "idx_datos_financieros_anio_mes");

            entity.HasIndex(e => e.EmpresaId, "idx_datos_financieros_empresa");

            entity.HasIndex(e => e.FechaRegistro, "idx_datos_financieros_fecha").IsDescending();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Anio)
                .HasComputedColumnSql("EXTRACT(year FROM fecha_registro)", true)
                .HasColumnName("anio");
            entity.Property(e => e.Beneficiario)
                .HasMaxLength(255)
                .HasColumnName("beneficiario");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");
            entity.Property(e => e.ComprobanteUrl)
                .HasMaxLength(500)
                .HasColumnName("comprobante_url");
            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .HasColumnName("concepto");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.TipoDato)
                .HasConversion<string>()
                .HasColumnName("tipo_dato");
            entity.Property(e => e.Periodo)
                .HasConversion<string>()
                .HasColumnName("periodo");
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Mes)
                .HasComputedColumnSql("EXTRACT(month FROM fecha_registro)", true)
                .HasColumnName("mes");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Moneda)
                .HasMaxLength(10)
                .HasDefaultValueSql("'COP'::character varying")
                .HasColumnName("moneda");
            entity.Property(e => e.Monto)
                .HasPrecision(15, 2)
                .HasColumnName("monto");
            entity.Property(e => e.NumeroComprobante)
                .HasMaxLength(50)
                .HasColumnName("numero_comprobante");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.Subcategoria)
                .HasMaxLength(100)
                .HasColumnName("subcategoria");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.DatosFinancieros)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("datos_financieros_empresa_id_fkey");
        });

        modelBuilder.Entity<DetallesVentum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("detalles_venta_pkey");

            entity.ToTable("detalles_venta", tb => tb.HasComment("Líneas de productos por venta"));

            entity.HasIndex(e => e.ProductoId, "idx_detalles_producto");

            entity.HasIndex(e => e.VentaId, "idx_detalles_venta");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasPrecision(10, 3)
                .HasColumnName("cantidad");
            entity.Property(e => e.CantidadDevuelta)
                .HasPrecision(10, 3)
                .HasDefaultValueSql("0")
                .HasColumnName("cantidad_devuelta");
            entity.Property(e => e.CostoTotal)
                .HasPrecision(15, 2)
                .HasColumnName("costo_total");
            entity.Property(e => e.CostoUnitario)
                .HasPrecision(12, 2)
                .HasColumnName("costo_unitario");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DescuentoMonto)
                .HasPrecision(12, 2)
                .HasColumnName("descuento_monto");
            entity.Property(e => e.DescuentoPorcentaje)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("descuento_porcentaje");
            entity.Property(e => e.ImpuestoMonto)
                .HasPrecision(12, 2)
                .HasColumnName("impuesto_monto");
            entity.Property(e => e.ImpuestoPorcentaje)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("impuesto_porcentaje");
            entity.Property(e => e.Notas).HasColumnName("notas");
            entity.Property(e => e.PrecioOriginal)
                .HasPrecision(12, 2)
                .HasColumnName("precio_original");
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(12, 2)
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Subtotal)
                .HasPrecision(15, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasPrecision(15, 2)
                .HasColumnName("total");
            entity.Property(e => e.VentaId).HasColumnName("venta_id");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("detalles_venta_producto_id_fkey");

            entity.HasOne(d => d.Venta).WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("detalles_venta_venta_id_fkey");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("empresas_pkey");

            entity.ToTable("empresas", tb => tb.HasComment("PyMEs registradas - Entidad central del sistema"));

            entity.HasIndex(e => e.Nit, "empresas_nit_key").IsUnique();

            entity.HasIndex(e => e.Activa, "idx_empresas_activa").HasFilter("(activa = true)");

            entity.HasIndex(e => new { e.Ciudad, e.Departamento }, "idx_empresas_ciudad_departamento");

            entity.HasIndex(e => e.Nit, "idx_empresas_nit")
                .IsUnique()
                .HasFilter("(nit IS NOT NULL)");

            entity.HasIndex(e => e.NombreComercial, "idx_empresas_nombre_comercial_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Sector)
                .HasColumnName("sector")
                .HasColumnType("tipo_sector_empresa")
                .IsRequired();

            entity.Property(e => e.Tamano)
                .HasColumnName("tamano")
                .HasColumnType("tipo_tamano_empresa")
                .IsRequired();
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(20)
                .HasColumnName("codigo_postal");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Antioquia'::character varying")
                .HasColumnName("departamento");
            entity.Property(e => e.DescripcionEmpresa).HasColumnName("descripcion_empresa");
            entity.Property(e => e.DireccionFiscal).HasColumnName("direccion_fiscal");
            entity.Property(e => e.DirectorCargo)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Director General'::character varying")
                .HasColumnName("director_cargo");
            entity.Property(e => e.DirectorDocumento)
                .HasMaxLength(50)
                .HasColumnName("director_documento");
            entity.Property(e => e.DirectorEmailSecundario)
                .HasMaxLength(255)
                .HasColumnName("director_email_secundario");
            entity.Property(e => e.DirectorNombreCompleto)
                .HasMaxLength(255)
                .HasColumnName("director_nombre_completo");
            entity.Property(e => e.DirectorTelefono)
                .HasMaxLength(20)
                .HasColumnName("director_telefono");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Nit)
                .HasMaxLength(50)
                .HasComment("Número de Identificación Tributaria (único en Colombia)")
                .HasColumnName("nit");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(255)
                .HasColumnName("nombre_comercial");
            entity.Property(e => e.Pais)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Colombia'::character varying")
                .HasColumnName("pais");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(255)
                .HasColumnName("razon_social");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<FuentesDato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fuentes_datos_pkey");

            entity.ToTable("fuentes_datos", tb => tb.HasComment("Conexiones a BD externas para ETL"));

            entity.HasIndex(e => e.Activa, "idx_fuentes_activa").HasFilter("(activa = true)");

            entity.HasIndex(e => e.EmpresaId, "idx_fuentes_empresa");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CertificadoSsl).HasColumnName("certificado_ssl");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.EstadoConexion)
                .HasConversion<string>()
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FrecuenciaSincronizacion)
                .HasMaxLength(50)
                .HasColumnName("frecuencia_sincronizacion");
            entity.Property(e => e.Host)
                .HasMaxLength(255)
                .HasColumnName("host");
            entity.Property(e => e.IntentosConexionFallidos)
                .HasDefaultValue(0)
                .HasColumnName("intentos_conexion_fallidos");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.NombreBaseDatos)
                .HasMaxLength(100)
                .HasColumnName("nombre_base_datos");
            entity.Property(e => e.ParametrosConexion)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("parametros_conexion");
            entity.Property(e => e.PasswordEncriptado)
                .HasMaxLength(500)
                .HasColumnName("password_encriptado");
            entity.Property(e => e.Puerto).HasColumnName("puerto");
            entity.Property(e => e.SincronizacionAutomatica)
                .HasDefaultValue(false)
                .HasColumnName("sincronizacion_automatica");
            entity.Property(e => e.SslHabilitado)
                .HasDefaultValue(false)
                .HasColumnName("ssl_habilitado");
            entity.Property(e => e.TipoFuente)
                .HasConversion<string>()
                .HasColumnName("tipo_fuente");
            entity.Property(e => e.UltimaConexion).HasColumnName("ultima_conexion");
            entity.Property(e => e.UltimaSincronizacion).HasColumnName("ultima_sincronizacion");
            entity.Property(e => e.UltimoError).HasColumnName("ultimo_error");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Usuario)
                .HasMaxLength(100)
                .HasColumnName("usuario");

            entity.HasOne(d => d.Empresa).WithMany(p => p.FuentesDatos)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("fuentes_datos_empresa_id_fkey");
        });

        modelBuilder.Entity<ImportacionesDato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("importaciones_datos_pkey");

            entity.ToTable("importaciones_datos", tb => tb.HasComment("Gestión de ETL - Extracción, Transformación y Carga"));

            entity.HasIndex(e => e.EmpresaId, "idx_importaciones_empresa");

            entity.HasIndex(e => e.FechaImportacion, "idx_importaciones_fecha").IsDescending();

            entity.HasIndex(e => e.FuenteDatosId, "idx_importaciones_fuente");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Advertencias)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("advertencias");
            entity.Property(e => e.ArchivoUrl)
                .HasMaxLength(500)
                .HasColumnName("archivo_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DuracionSegundos).HasColumnName("duracion_segundos");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasColumnName("estado");
            entity.Property(e => e.ErroresCarga)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("errores_carga");
            entity.Property(e => e.ErroresExtraccion)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("errores_extraccion");
            entity.Property(e => e.ErroresTransformacion)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("errores_transformacion");
            entity.Property(e => e.FaseActual)
                .HasConversion<string>()
                .HasColumnName("fase_actual");
            entity.Property(e => e.FechaFinEtl).HasColumnName("fecha_fin_etl").HasColumnType("timestamp with time zone"); ;
            entity.Property(e => e.FechaImportacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_importacion");
            entity.Property(e => e.FechaInicioEtl).HasColumnName("fecha_inicio_etl").HasColumnType("timestamp with time zone"); ;
            entity.Property(e => e.FuenteDatosId).HasColumnName("fuente_datos_id");
            entity.Property(e => e.HashArchivo)
                .HasMaxLength(64)
                .HasColumnName("hash_archivo");
            entity.Property(e => e.LogEtl).HasColumnName("log_etl");
            entity.Property(e => e.MapeoColumnas)
                .HasComment("Mapeo entre columnas Excel/CSV y campos BD")
                .HasColumnType("jsonb")
                .HasColumnName("mapeo_columnas");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.ProgresoPorcentaje)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("progreso_porcentaje");
            entity.Property(e => e.RegistrosCargados)
                .HasDefaultValue(0)
                .HasColumnName("registros_cargados");
            entity.Property(e => e.RegistrosExtraidos)
                .HasDefaultValue(0)
                .HasColumnName("registros_extraidos");
            entity.Property(e => e.RegistrosRechazados)
                .HasDefaultValue(0)
                .HasColumnName("registros_rechazados");
            entity.Property(e => e.RegistrosTransformados)
                .HasDefaultValue(0)
                .HasColumnName("registros_transformados");
            entity.Property(e => e.ReglasTransformacion)
                .HasColumnType("jsonb")
                .HasColumnName("reglas_transformacion");
            entity.Property(e => e.ResultadoCarga)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("resultado_carga");
            entity.Property(e => e.TamanoArchivo).HasColumnName("tamano_archivo");
            entity.Property(e => e.TipoDatos)
                .HasConversion<string>()
                .HasColumnName("tipo_datos");
            entity.Property(e => e.TipoFuente)
                .HasConversion<string>()
                .HasColumnName("tipo_fuente");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ImportacionesDatos)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("importaciones_datos_empresa_id_fkey");

            entity.HasOne(d => d.FuenteDatos).WithMany(p => p.ImportacionesDatos)
                .HasForeignKey(d => d.FuenteDatosId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("importaciones_datos_fuente_datos_id_fkey");
        });

        // Nueva tabla para logs simplificados de import
        modelBuilder.Entity<ImportacionesDatosLogs>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("importacion_datos_logs_pkey");

            entity.ToTable("importacion_datos_logs", tb => tb.HasComment("Logs simplificados de importaciones Excel"));

            entity.HasIndex(e => e.EmpresaId, "idx_import_logs_empresa");
            entity.HasIndex(e => e.FechaImportacion, "idx_import_logs_fecha").IsDescending();
            entity.HasIndex(e => e.Estado, "idx_import_logs_estado");
            entity.HasIndex(e => e.HashArchivo, "idx_import_logs_hash");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.EmpresaId)
                .IsRequired()
                .HasColumnName("empresa_id");
            entity.Property(e => e.NombreArchivo)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.TamanoArchivo)
                .HasColumnName("tamano_archivo");
            entity.Property(e => e.HashArchivo)
                .HasMaxLength(64)
                .HasColumnName("hash_archivo");
            entity.Property(e => e.TipoDatos)
                .HasMaxLength(100)
                .HasColumnName("tipo_datos");
            entity.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValueSql("'en_proceso'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.FaseActual)
                .HasMaxLength(50)
                .HasColumnName("fase_actual");
            entity.Property(e => e.ProgresoPorcentaje)
                .HasDefaultValue(0)
                .HasColumnName("progreso_porcentaje");
            entity.Property(e => e.RegistrosExtraidos)
                .HasDefaultValue(0)
                .HasColumnName("registros_extraidos");
            entity.Property(e => e.RegistrosTransformados)
                .HasDefaultValue(0)
                .HasColumnName("registros_transformados");
            entity.Property(e => e.RegistrosCargados)
                .HasDefaultValue(0)
                .HasColumnName("registros_cargados");
            entity.Property(e => e.RegistrosRechazados)
                .HasDefaultValue(0)
                .HasColumnName("registros_rechazados");
            entity.Property(e => e.ErroresExtraccion)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnName("errores_extraccion");
            entity.Property(e => e.ErroresTransformacion)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnName("errores_transformacion");
            entity.Property(e => e.ErroresCarga)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnName("errores_carga");
            entity.Property(e => e.Advertencias)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnName("advertencias");
            entity.Property(e => e.FechaImportacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_importacion");
            entity.Property(e => e.FechaInicioEtl)
                .HasColumnName("fecha_inicio_etl");
            entity.Property(e => e.FechaFinEtl)
                .HasColumnName("fecha_fin_etl");
            entity.Property(e => e.DuracionSegundos)
                .HasColumnName("duracion_segundos");
            entity.Property(e => e.ResultadoCarga)
                .HasColumnType("jsonb")
                .HasColumnName("resultado_carga");
            entity.Property(e => e.LogCompleto)
                .HasColumnName("log_completo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa)
                .WithMany()
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_import_logs_empresa");
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventario_pkey");

            entity.ToTable("inventario", tb => tb.HasComment("Control de stock"));

            entity.HasIndex(e => new { e.CantidadDisponible, e.StockMinimo }, "idx_inventario_alerta_bajo").HasFilter("((alertas_activadas = true) AND (cantidad_disponible < stock_minimo))");

            entity.HasIndex(e => e.ProductoId, "idx_inventario_producto");

            entity.HasIndex(e => e.ProductoId, "inventario_producto_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AlertasActivadas)
                .HasDefaultValue(true)
                .HasColumnName("alertas_activadas");
            entity.Property(e => e.CantidadDisponible)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_disponible");
            entity.Property(e => e.CantidadEnTransito)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_en_transito");
            entity.Property(e => e.CantidadReservada)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_reservada");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DiasAlertaVencimiento)
                .HasDefaultValue(30)
                .HasColumnName("dias_alerta_vencimiento");
            entity.Property(e => e.EstadoStock)
                .HasConversion<string>()
                .HasColumnName("estado_stock");
            entity.Property(e => e.Estante)
                .HasMaxLength(20)
                .HasColumnName("estante");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.LoteActual)
                .HasMaxLength(50)
                .HasColumnName("lote_actual");
            entity.Property(e => e.Nivel)
                .HasMaxLength(20)
                .HasColumnName("nivel");
            entity.Property(e => e.Pasillo)
                .HasMaxLength(20)
                .HasColumnName("pasillo");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.PuntoReorden).HasColumnName("punto_reorden");
            entity.Property(e => e.StockMaximo).HasColumnName("stock_maximo");
            entity.Property(e => e.StockMinimo)
                .HasDefaultValue(0)
                .HasColumnName("stock_minimo");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(100)
                .HasColumnName("ubicacion");
            entity.Property(e => e.UltimaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("ultima_actualizacion");
            entity.Property(e => e.UltimaEntrada).HasColumnName("ultima_entrada");
            entity.Property(e => e.UltimaSalida).HasColumnName("ultima_salida");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Producto).WithOne(p => p.Inventario)
                .HasForeignKey<Inventario>(d => d.ProductoId)
                .HasConstraintName("inventario_producto_id_fkey");
        });

        modelBuilder.Entity<MensajesIum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mensajes_ia_pkey");

            entity.ToTable("mensajes_ia", tb => tb.HasComment("Mensajes de IA consumiendo OpenAI API"));

            entity.HasIndex(e => e.ConversacionId, "idx_mensajes_conversacion");

            entity.HasIndex(e => e.Timestamp, "idx_mensajes_timestamp").IsDescending();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.ConversacionId).HasColumnName("conversacion_id");
            entity.Property(e => e.TipoMensaje)
                .HasConversion<string>()
                .HasColumnName("tipo_mensaje");
            entity.Property(e => e.CostoEstimadoUsd)
                .HasPrecision(10, 6)
                .HasComment("Costo según pricing OpenAI")
                .HasColumnName("costo_estimado_usd");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{\"modelo\": null, \"provider\": \"openai\", \"costo_usd\": null, \"max_tokens\": null, \"temperatura\": null, \"tokens_total\": null, \"finish_reason\": null, \"tokens_prompt\": null, \"tokens_completion\": null, \"tiempo_respuesta_ms\": null}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("timestamp");
            entity.Property(e => e.TokensUsados)
                .HasComment("Total tokens (prompt + completion)")
                .HasColumnName("tokens_usados");

            entity.HasOne(d => d.Conversacion).WithMany(p => p.MensajesIa)
                .HasForeignKey(d => d.ConversacionId)
                .HasConstraintName("mensajes_ia_conversacion_id_fkey");
        });

        modelBuilder.Entity<Metrica>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("metricas_pkey");

            entity.ToTable("metricas", tb => tb.HasComment("Valores calculados de KPIs predefinidos"));

            entity.HasIndex(e => e.CatalogoMetricaId, "idx_metricas_catalogo");

            entity.HasIndex(e => e.DashboardId, "idx_metricas_dashboard");

            entity.HasIndex(e => e.FechaCalculo, "idx_metricas_fecha").IsDescending();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CatalogoMetricaId).HasColumnName("catalogo_metrica_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DashboardId).HasColumnName("dashboard_id");
            entity.Property(e => e.Tendencia)
                .HasConversion<string>()
                .HasColumnName("tendencia");
            entity.Property(e => e.Periodo)
                .HasConversion<string>()
                .HasColumnName("periodo");
            entity.Property(e => e.FechaCalculo)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_calculo");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Unidad)
                .HasMaxLength(50)
                .HasColumnName("unidad");
            entity.Property(e => e.ValorNumerico)
                .HasPrecision(15, 2)
                .HasColumnName("valor_numerico");
            entity.Property(e => e.ValorTexto)
                .HasMaxLength(255)
                .HasColumnName("valor_texto");
            entity.Property(e => e.VariacionPorcentaje)
                .HasPrecision(5, 2)
                .HasColumnName("variacion_porcentaje");
            entity.Property(e => e.VariacionValor)
                .HasPrecision(15, 2)
                .HasColumnName("variacion_valor");

            entity.HasOne(d => d.CatalogoMetrica).WithMany(p => p.Metricas)
                .HasForeignKey(d => d.CatalogoMetricaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("metricas_catalogo_metrica_id_fkey");

            entity.HasOne(d => d.Dashboard).WithMany(p => p.Metricas)
                .HasForeignKey(d => d.DashboardId)
                .HasConstraintName("metricas_dashboard_id_fkey");
        });

        modelBuilder.Entity<MovimientosInventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("movimientos_inventario_pkey");

            entity.ToTable("movimientos_inventario", tb => tb.HasComment("Historial de movimientos de inventario"));

            entity.HasIndex(e => e.EmpresaId, "idx_movimientos_empresa");

            entity.HasIndex(e => e.FechaMovimiento, "idx_movimientos_fecha").IsDescending();

            entity.HasIndex(e => e.InventarioId, "idx_movimientos_inventario");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.CantidadAnterior).HasColumnName("cantidad_anterior");
            entity.Property(e => e.CantidadNueva).HasColumnName("cantidad_nueva");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.TipoMovimiento)
                .HasConversion<string>()
                .HasColumnName("tipo_movimiento");
            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_movimiento");
            entity.Property(e => e.InventarioId).HasColumnName("inventario_id");
            entity.Property(e => e.Lote)
                .HasMaxLength(50)
                .HasColumnName("lote");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .HasColumnName("motivo");
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(50)
                .HasColumnName("numero_documento");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.Referencia)
                .HasMaxLength(100)
                .HasColumnName("referencia");
            entity.Property(e => e.UbicacionDestino)
                .HasMaxLength(100)
                .HasColumnName("ubicacion_destino");
            entity.Property(e => e.UbicacionOrigen)
                .HasMaxLength(100)
                .HasColumnName("ubicacion_origen");
            entity.Property(e => e.VentaId).HasColumnName("venta_id");

            entity.HasOne(d => d.Empresa).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("movimientos_inventario_empresa_id_fkey");

            entity.HasOne(d => d.Inventario).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.InventarioId)
                .HasConstraintName("movimientos_inventario_inventario_id_fkey");

            entity.HasOne(d => d.Venta).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("movimientos_inventario_venta_id_fkey");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notificaciones_pkey");

            entity.ToTable("notificaciones", tb => tb.HasComment("Notificaciones empresariales"));

            entity.HasIndex(e => e.EmpresaId, "idx_notificaciones_empresa");

            entity.HasIndex(e => e.FechaCreacion, "idx_notificaciones_fecha").IsDescending();

            entity.HasIndex(e => e.Leida, "idx_notificaciones_leida");

            entity.HasIndex(e => new { e.EmpresaId, e.Leida, e.FechaCreacion }, "idx_notificaciones_no_leidas")
                .IsDescending(false, false, true)
                .HasFilter("((leida = false) AND (archivada = false))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AccionTexto)
                .HasMaxLength(100)
                .HasColumnName("accion_texto");
            entity.Property(e => e.AccionUrl)
                .HasMaxLength(500)
                .HasColumnName("accion_url");
            entity.Property(e => e.Archivada)
                .HasDefaultValue(false)
                .HasColumnName("archivada");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.FechaArchivado).HasColumnName("fecha_archivado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaLectura).HasColumnName("fecha_lectura");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasColumnName("icono");
            entity.Property(e => e.Leida)
                .HasDefaultValue(false)
                .HasColumnName("leida");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.TipoNotificacion)
                .HasConversion<string>()
                .HasColumnName("tipo_notificacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .HasColumnName("titulo");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("notificaciones_empresa_id_fkey");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productos_pkey");

            entity.ToTable("productos", tb => tb.HasComment("Catálogo de productos y servicios"));

            entity.HasIndex(e => new { e.EmpresaId, e.CodigoProducto }, "codigo_producto_unico").IsUnique();

            entity.HasIndex(e => e.Activo, "idx_productos_activo").HasFilter("(activo = true)");

            entity.HasIndex(e => e.CategoriaId, "idx_productos_categoria");

            entity.HasIndex(e => e.EmpresaId, "idx_productos_empresa");

            entity.HasIndex(e => e.Nombre, "idx_productos_nombre_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(100)
                .HasColumnName("codigo_barras");
            entity.Property(e => e.CodigoProducto)
                .HasMaxLength(50)
                .HasColumnName("codigo_producto");
            entity.Property(e => e.CodigoQr)
                .HasMaxLength(255)
                .HasColumnName("codigo_qr");
            entity.Property(e => e.Comprable)
                .HasDefaultValue(true)
                .HasColumnName("comprable");
            entity.Property(e => e.CostoUnitario)
                .HasPrecision(12, 2)
                .HasColumnName("costo_unitario");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.EsServicio)
                .HasDefaultValue(false)
                .HasColumnName("es_servicio");
            entity.Property(e => e.Especificaciones)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("especificaciones");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .HasColumnName("imagen_url");
            entity.Property(e => e.ImagenesAdicionales)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("imagenes_adicionales");
            entity.Property(e => e.Marca)
                .HasMaxLength(100)
                .HasColumnName("marca");
            entity.Property(e => e.MargenPorcentaje)
                .HasPrecision(5, 2)
                .HasComputedColumnSql("\nCASE\n    WHEN (costo_unitario > (0)::numeric) THEN (((precio_venta - costo_unitario) / costo_unitario) * (100)::numeric)\n    ELSE NULL::numeric\nEND", true)
                .HasColumnName("margen_porcentaje");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Modelo)
                .HasMaxLength(100)
                .HasColumnName("modelo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.PesoKg)
                .HasPrecision(10, 3)
                .HasColumnName("peso_kg");
            entity.Property(e => e.PrecioSugerido)
                .HasPrecision(12, 2)
                .HasColumnName("precio_sugerido");
            entity.Property(e => e.PrecioVenta)
                .HasPrecision(12, 2)
                .HasColumnName("precio_venta");
            entity.Property(e => e.RequiereInventario)
                .HasDefaultValue(true)
                .HasColumnName("requiere_inventario");
            entity.Property(e => e.Subcategoria)
                .HasMaxLength(100)
                .HasColumnName("subcategoria");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(50)
                .HasDefaultValueSql("'unidad'::character varying")
                .HasColumnName("unidad_medida");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Vendible)
                .HasDefaultValue(true)
                .HasColumnName("vendible");
            entity.Property(e => e.VolumenM3)
                .HasPrecision(10, 3)
                .HasColumnName("volumen_m3");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("productos_categoria_id_fkey");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Productos)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("productos_empresa_id_fkey");
        });

        modelBuilder.Entity<RecomendacionesIum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recomendaciones_ia_pkey");

            entity.ToTable("recomendaciones_ia", tb => tb.HasComment("Insights y sugerencias generadas por IA"));

            entity.HasIndex(e => e.EmpresaId, "idx_recomendaciones_empresa");

            entity.HasIndex(e => e.FechaGeneracion, "idx_recomendaciones_fecha").IsDescending();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AccionesSugeridas).HasColumnName("acciones_sugeridas");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EfectividadPorcentaje)
                .HasPrecision(5, 2)
                .HasColumnName("efectividad_porcentaje");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasColumnName("estado");
            entity.Property(e => e.FechaAplicacion).HasColumnName("fecha_aplicacion");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.FechaVista).HasColumnName("fecha_vista");
            entity.Property(e => e.ImpactoEstimado)
                .HasMaxLength(100)
                .HasColumnName("impacto_estimado");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Prioridad)
                .HasConversion<string>()
                .HasColumnName("prioridad");
            entity.Property(e => e.Resultados).HasColumnName("resultados");
            entity.Property(e => e.TipoRecomendacion)
                .HasConversion<string>()
                .HasColumnName("tipo_recomendacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .HasColumnName("titulo");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Empresa).WithMany(p => p.RecomendacionesIa)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("recomendaciones_ia_empresa_id_fkey");
        });

        modelBuilder.Entity<Suscripcione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("suscripciones_pkey");

            entity.ToTable("suscripciones", tb => tb.HasComment("Gestión de planes SaaS y facturación"));

            entity.HasIndex(e => e.EmpresaId, "idx_suscripciones_empresa");

            entity.HasIndex(e => new { e.FechaInicio, e.FechaFin }, "idx_suscripciones_vigencia");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AlmacenamientoGb)
                .HasDefaultValue(5)
                .HasColumnName("almacenamiento_gb");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DescuentoAplicado)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("descuento_aplicado");
            entity.Property(e => e.DiasTrialRestantes).HasColumnName("dias_trial_restantes");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaProximoCobro).HasColumnName("fecha_proximo_cobro");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Plan)
                .HasConversion<string>()
                .HasColumnName("plan");
            entity.Property(e => e.PrecioFinal)
                .HasPrecision(10, 2)
                .HasComputedColumnSql("(precio_mensual * ((1)::numeric - (descuento_aplicado / (100)::numeric)))", true)
                .HasColumnName("precio_final");
            entity.Property(e => e.PrecioMensual)
                .HasPrecision(10, 2)
                .HasColumnName("precio_mensual");
            entity.Property(e => e.RenovacionAutomatica)
                .HasDefaultValue(true)
                .HasColumnName("renovacion_automatica");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsuariosPermitidos)
                .HasDefaultValue(1)
                .HasColumnName("usuarios_permitidos");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Suscripciones)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("suscripciones_empresa_id_fkey");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ventas_pkey");

            entity.ToTable("ventas", tb => tb.HasComment("Transacciones de venta"));

            entity.HasIndex(e => e.ClienteId, "idx_ventas_cliente");

            entity.HasIndex(e => e.EmpresaId, "idx_ventas_empresa");

            entity.HasIndex(e => e.FechaVenta, "idx_ventas_fecha").IsDescending();

            entity.HasIndex(e => e.NumeroOrden, "idx_ventas_numero_orden");

            entity.HasIndex(e => new { e.EmpresaId, e.NumeroOrden }, "orden_unica_por_empresa").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CanalVenta)
                .HasMaxLength(50)
                .HasColumnName("canal_venta");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");
            entity.Property(e => e.ClienteDireccion).HasColumnName("cliente_direccion");
            entity.Property(e => e.ClienteDocumento)
                .HasMaxLength(50)
                .HasColumnName("cliente_documento");
            entity.Property(e => e.ClienteEmail)
                .HasMaxLength(255)
                .HasColumnName("cliente_email");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ClienteNombre)
                .HasMaxLength(255)
                .HasColumnName("cliente_nombre");
            entity.Property(e => e.ClienteTelefono)
                .HasMaxLength(20)
                .HasColumnName("cliente_telefono");
            entity.Property(e => e.CostoTotal)
                .HasPrecision(15, 2)
                .HasColumnName("costo_total");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasColumnName("estado");
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pendiente'::character varying")
                .HasColumnName("estado_pago");
            entity.Property(e => e.FechaEntrega).HasColumnName("fecha_entrega");
            entity.Property(e => e.FechaEntregaEstimada).HasColumnName("fecha_entrega_estimada");
            entity.Property(e => e.FechaVenta)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_venta");
            entity.Property(e => e.MargenBruto)
                .HasPrecision(15, 2)
                .HasColumnName("margen_bruto");
            entity.Property(e => e.MetodoPago)
                .HasConversion<string>()
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.MontoDescuento)
                .HasPrecision(15, 2)
                .HasColumnName("monto_descuento");
            entity.Property(e => e.MontoImpuestos)
                .HasPrecision(15, 2)
                .HasColumnName("monto_impuestos");
            entity.Property(e => e.MontoSubtotal)
                .HasPrecision(15, 2)
                .HasColumnName("monto_subtotal");
            entity.Property(e => e.MontoTotal)
                .HasPrecision(15, 2)
                .HasColumnName("monto_total");
            entity.Property(e => e.Notas).HasColumnName("notas");
            entity.Property(e => e.NotasInternas).HasColumnName("notas_internas");
            entity.Property(e => e.NumeroFactura)
                .HasMaxLength(50)
                .HasColumnName("numero_factura");
            entity.Property(e => e.NumeroOrden)
                .HasMaxLength(50)
                .HasColumnName("numero_orden");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Vendedor)
                .HasMaxLength(100)
                .HasColumnName("vendedor");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Venta)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ventas_cliente_id_fkey");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Venta)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("ventas_empresa_id_fkey");
        });

        modelBuilder.Entity<VistaEmpresaCompletum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vista_empresa_completa");

            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");
            entity.Property(e => e.CuentaActiva).HasColumnName("cuenta_activa");
            entity.Property(e => e.CuentaAvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("cuenta_avatar_url");
            entity.Property(e => e.CuentaBio).HasColumnName("cuenta_bio");
            entity.Property(e => e.CuentaDisplayName)
                .HasMaxLength(100)
                .HasColumnName("cuenta_display_name");
            entity.Property(e => e.CuentaEmail)
                .HasMaxLength(255)
                .HasColumnName("cuenta_email");
            entity.Property(e => e.CuentaEsOwner).HasColumnName("cuenta_es_owner");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.CuentaNombreCompleto)
                .HasMaxLength(255)
                .HasColumnName("cuenta_nombre_completo");
            entity.Property(e => e.CuentaRol)
                .HasMaxLength(50)
                .HasColumnName("cuenta_rol");
            entity.Property(e => e.CuentaTelefono)
                .HasMaxLength(20)
                .HasColumnName("cuenta_telefono");
            entity.Property(e => e.CuentaUbicacion)
                .HasMaxLength(255)
                .HasColumnName("cuenta_ubicacion");
            entity.Property(e => e.CuentaVerificada).HasColumnName("cuenta_verificada");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .HasColumnName("departamento");
            entity.Property(e => e.DirectorCargo)
                .HasMaxLength(100)
                .HasColumnName("director_cargo");
            entity.Property(e => e.DirectorNombreCompleto)
                .HasMaxLength(255)
                .HasColumnName("director_nombre_completo");
            entity.Property(e => e.EmpresaActiva).HasColumnName("empresa_activa");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.FechaFinSuscripcion).HasColumnName("fecha_fin_suscripcion");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.Moneda)
                .HasMaxLength(10)
                .HasColumnName("moneda");
            entity.Property(e => e.Nit)
                .HasMaxLength(50)
                .HasColumnName("nit");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(255)
                .HasColumnName("nombre_comercial");
            entity.Property(e => e.NotificacionesPendientes).HasColumnName("notificaciones_pendientes");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(255)
                .HasColumnName("razon_social");
            entity.Property(e => e.UltimaSesion).HasColumnName("ultima_sesion");
            entity.Property(e => e.ZonaHoraria)
                .HasMaxLength(50)
                .HasColumnName("zona_horaria");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
