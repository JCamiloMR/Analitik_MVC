using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Analitik_MVC.Models;

public partial class AnalitikContext : DbContext
{
    public AnalitikContext()
    {
    }

    public AnalitikContext(DbContextOptions<AnalitikContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<ConfiguracionEmpresa> ConfiguracionEmpresas { get; set; }

    public virtual DbSet<CuentaEmpresa> CuentaEmpresas { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Suscripcione> Suscripciones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Analitik;Username=postgres;Password=Camilo1307");

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
            .HasPostgresEnum("tipo_sector_empresa", new[] { "comercio", "manufactura", "servicios", "tecnologia", "confecciones", "alimentos", "salud", "educacion", "otro" })
            .HasPostgresEnum("tipo_tamano_empresa", new[] { "micro", "pequena", "mediana" })
            .HasPostgresEnum("tipo_tema", new[] { "claro", "oscuro", "automatico" })
            .HasPostgresEnum("tipo_tendencia", new[] { "up", "down", "neutral" })
            .HasPostgresEnum("tipo_tendencia_producto", new[] { "creciente", "estable", "decreciente" })
            .HasPostgresExtension("btree_gin")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clientes_pkey");

            entity.ToTable("clientes");

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
            entity.Property(e => e.Sector)
                .HasConversion<string>() // enum <-> text
                .HasColumnType("tipo_sector_empresa") // enum de postgres
                .HasColumnName("sector");

            entity.Property(e => e.Tamano)
                .HasConversion<string>()
                .HasColumnType("tipo_tamano_empresa")
                .HasColumnName("tamano");

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
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) ||
                    property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc
                            ? v
                            : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                    ));
                }
            }
        }
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
