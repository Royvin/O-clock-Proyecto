using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Oclock.Models;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Oclock.Data;

public partial class By5rqco0trg7fpqgnpvmContext : DbContext
{
    public By5rqco0trg7fpqgnpvmContext()
    {
    }

    public By5rqco0trg7fpqgnpvmContext(DbContextOptions<By5rqco0trg7fpqgnpvmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bono> Bonos { get; set; }

    public virtual DbSet<Capacitacion> Capacitacions { get; set; }

    public virtual DbSet<Documento> Documentos { get; set; }

    public virtual DbSet<Expediente> Expedientes { get; set; }

    public virtual DbSet<Feriado> Feriados { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<PoliticaBono> PoliticaBonos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Solicitud> Solicituds { get; set; }

    public virtual DbSet<TipoDocumento> TipoDocumentos { get; set; }

    public virtual DbSet<TipoSolicitud> TipoSolicituds { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioHorario> UsuarioHorarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=by5rqco0trg7fpqgnpvm-mysql.services.clever-cloud.com;database=by5rqco0trg7fpqgnpvm;user=uhtuhx1j5cjbucsm;password=ctjpVDBOslwnwliD6fMQ;sslmode=Required", ServerVersion.Parse("8.4.2-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Bono>(entity =>
        {
            entity.HasKey(e => e.IdBono).HasName("PRIMARY");

            entity.ToTable("BONO");

            entity.HasIndex(e => e.IdPoliticaBono, "fk_bono_politica");

            entity.HasIndex(e => e.IdUsuario, "fk_bono_usuario");

            entity.Property(e => e.IdBono).HasColumnName("id_bono");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCumplidos).HasColumnName("fecha_cumplidos");
            entity.Property(e => e.FechaOtorgado).HasColumnName("fecha_otorgado");
            entity.Property(e => e.IdPoliticaBono).HasColumnName("id_politica_bono");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NombreBono)
                .HasMaxLength(100)
                .HasColumnName("nombre_bono");
            entity.Property(e => e.Periodo)
                .HasMaxLength(50)
                .HasColumnName("periodo");

            entity.HasOne(d => d.IdPoliticaBonoNavigation).WithMany(p => p.Bonos)
                .HasForeignKey(d => d.IdPoliticaBono)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_bono_politica");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Bonos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_bono_usuario");
        });

        modelBuilder.Entity<Capacitacion>(entity =>
        {
            entity.HasKey(e => e.IdCapacitacion).HasName("PRIMARY");

            entity.ToTable("CAPACITACION");

            entity.HasIndex(e => e.IdUsuario, "fk_capacitacion_usuario");

            entity.Property(e => e.IdCapacitacion).HasColumnName("id_capacitacion");
            entity.Property(e => e.Certificado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("certificado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Horas).HasColumnName("horas");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Institucion)
                .HasMaxLength(200)
                .HasColumnName("institucion");
            entity.Property(e => e.NombreCurso)
                .HasMaxLength(200)
                .HasColumnName("nombre_curso");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Capacitacions)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_capacitacion_usuario");
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.HasKey(e => e.IdDocumento).HasName("PRIMARY");

            entity.ToTable("DOCUMENTO");

            entity.HasIndex(e => e.IdTipoDocumento, "fk_documento_tipo");

            entity.HasIndex(e => e.IdUsuario, "fk_documento_usuario");

            entity.Property(e => e.IdDocumento).HasColumnName("id_documento");
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_subida");
            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .HasColumnName("ruta_archivo");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("fk_documento_tipo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_documento_usuario");
        });

        modelBuilder.Entity<Expediente>(entity =>
        {
            entity.HasKey(e => e.IdExpediente).HasName("PRIMARY");

            entity.ToTable("EXPEDIENTE");

            entity.HasIndex(e => e.Cedula, "cedula").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "fk_expediente_usuario");

            entity.Property(e => e.IdExpediente).HasColumnName("id_expediente");
            entity.Property(e => e.Cedula)
                .HasMaxLength(50)
                .HasColumnName("cedula");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");
            entity.Property(e => e.ContactoEmergencia)
                .HasMaxLength(200)
                .HasColumnName("contacto_emergencia");
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .HasColumnName("direccion");
            entity.Property(e => e.Estado)
                .HasMaxLength(100)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoCivil)
                .HasMaxLength(50)
                .HasColumnName("estado_civil");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.TelefonoEmergencia)
                .HasMaxLength(20)
                .HasColumnName("telefono_emergencia");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Expedientes)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_expediente_usuario");
        });

        modelBuilder.Entity<Feriado>(entity =>
        {
            entity.HasKey(e => e.IdFeriado).HasName("PRIMARY");

            entity.ToTable("FERIADO");

            entity.Property(e => e.IdFeriado).HasColumnName("id_feriado");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.EsLaborable)
                .HasDefaultValueSql("'0'")
                .HasColumnName("es_laborable");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.IdHorario).HasName("PRIMARY");

            entity.ToTable("HORARIO");

            entity.Property(e => e.IdHorario).HasColumnName("id_horario");
            entity.Property(e => e.Dias)
                .HasMaxLength(50)
                .HasColumnName("dias");
            entity.Property(e => e.HoraEntrada)
                .HasColumnType("time")
                .HasColumnName("hora_entrada");
            entity.Property(e => e.HoraSalida)
                .HasColumnType("time")
                .HasColumnName("hora_salida");
            entity.Property(e => e.NombreHorario)
                .HasMaxLength(100)
                .HasColumnName("nombre_horario");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PRIMARY");

            entity.ToTable("MARCA");

            entity.HasIndex(e => e.IdUsuario, "fk_marca_usuario");

            entity.Property(e => e.IdMarca)
                .ValueGeneratedOnAdd()
                .HasColumnName("id_marca");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Comentario)
                .HasColumnType("text")
                .HasColumnName("comentario");
            entity.Property(e => e.HoraEntrada)
                .HasColumnType("time")
                .HasColumnName("hora_entrada");
            entity.Property(e => e.HoraSalida)
                .HasColumnType("time")
                .HasColumnName("hora_salida");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Ubicancia)
                .HasMaxLength(200)
                .HasColumnName("ubicancia");
            entity.Property(e => e.Fecha)
                .HasColumnType("date")
                .HasColumnName("fecha")
                .HasDefaultValueSql("CURRENT_DATE");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Marcas)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_marca_usuario");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PRIMARY");

            entity.ToTable("NOTIFICACION");

            entity.HasIndex(e => e.IdUsuario, "fk_notificacion_usuario");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.FechaNotificacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_notificacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leida)
                .HasDefaultValueSql("'0'")
                .HasColumnName("leida");
            entity.Property(e => e.Mensaje)
                .HasColumnType("text")
                .HasColumnName("mensaje");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_notificacion_usuario");
        });

        modelBuilder.Entity<PoliticaBono>(entity =>
        {
            entity.HasKey(e => e.IdPoliticaBono).HasName("PRIMARY");

            entity.ToTable("POLITICA_BONO");

            entity.Property(e => e.IdPoliticaBono).HasColumnName("id_politica_bono");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.DiasAcumulados).HasColumnName("dias_acumulados");
            entity.Property(e => e.MesesBono).HasColumnName("meses_bono");
            entity.Property(e => e.NombrePolitica)
                .HasMaxLength(100)
                .HasColumnName("nombre_politica");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PRIMARY");

            entity.ToTable("ROL");

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .HasColumnName("nombre_rol");
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PRIMARY");

            entity.ToTable("SOLICITUD");

            entity.HasIndex(e => e.IdTipoSolicitud, "fk_solicitud_tipo");

            entity.HasIndex(e => e.IdUsuario, "fk_solicitud_usuario");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.DescripcionEstado)
                .HasColumnType("text")
                .HasColumnName("descripcion_estado");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaSolicitud).HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdTipoSolicitud).HasColumnName("id_tipo_solicitud");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdTipoSolicitudNavigation).WithMany(p => p.Solicituds)
                .HasForeignKey(d => d.IdTipoSolicitud)
                .HasConstraintName("fk_solicitud_tipo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Solicituds)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_solicitud_usuario");
        });

        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasName("PRIMARY");

            entity.ToTable("TIPO_DOCUMENTO");

            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreTipo)
                .HasMaxLength(100)
                .HasColumnName("nombre_tipo");
            entity.Property(e => e.Obligatorio)
                .HasDefaultValueSql("'0'")
                .HasColumnName("obligatorio");
        });

        modelBuilder.Entity<TipoSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdTipoSolicitud).HasName("PRIMARY");

            entity.ToTable("TIPO_SOLICITUD");

            entity.Property(e => e.IdTipoSolicitud).HasColumnName("id_tipo_solicitud");
            entity.Property(e => e.NombreSolicitud)
                .HasMaxLength(100)
                .HasColumnName("nombre_solicitud");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.IdRol, "id_rol_idx");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .HasColumnName("apellido");
            entity.Property(e => e.Contraseña)
                .HasMaxLength(255)
                .HasColumnName("contraseña");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasColumnName("estado");
            entity.Property(e => e.FechaContratacion).HasColumnName("fecha_contratacion");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("id_rol");
        });

        modelBuilder.Entity<UsuarioHorario>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioHorario).HasName("PRIMARY");

            entity.ToTable("USUARIO_HORARIO");

            entity.HasIndex(e => e.IdHorario, "fk_usuario_horario_horario");

            entity.HasIndex(e => e.IdUsuario, "fk_usuario_horario_usuario");

            entity.Property(e => e.IdUsuarioHorario).HasColumnName("id_usuario_horario");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.IdHorario).HasColumnName("id_horario");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdHorarioNavigation).WithMany(p => p.UsuarioHorarios)
                .HasForeignKey(d => d.IdHorario)
                .HasConstraintName("fk_usuario_horario_horario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuarioHorarios)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_usuario_horario_usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}