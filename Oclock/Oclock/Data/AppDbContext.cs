using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Oclock.Models;

namespace Oclock.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BONO> BONOs { get; set; }

    public virtual DbSet<CAPACITACION> CAPACITACIONs { get; set; }

    public virtual DbSet<DOCUMENTO> DOCUMENTOs { get; set; }

    public virtual DbSet<EXPEDIENTE> EXPEDIENTEs { get; set; }

    public virtual DbSet<FERIADO> FERIADOs { get; set; }

    public virtual DbSet<HORARIO> HORARIOs { get; set; }

    public virtual DbSet<MARCA> MARCAs { get; set; }

    public virtual DbSet<NOTIFICACION> NOTIFICACIONs { get; set; }

    public virtual DbSet<POLITICA_BONO> POLITICA_BONOs { get; set; }

    public virtual DbSet<ROL> ROLs { get; set; }

    public virtual DbSet<SOLICITUD> SOLICITUDs { get; set; }

    public virtual DbSet<TIPO_DOCUMENTO> TIPO_DOCUMENTOs { get; set; }

    public virtual DbSet<TIPO_SOLICITUD> TIPO_SOLICITUDs { get; set; }

    public virtual DbSet<USUARIO> USUARIOs { get; set; }

    public virtual DbSet<USUARIO_HORARIO> USUARIO_HORARIOs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<BONO>(entity =>
        {
            entity.HasKey(e => e.id_bono).HasName("PRIMARY");

            entity.ToTable("BONO");

            entity.HasIndex(e => e.id_politica_bono, "fk_bono_politica");

            entity.HasIndex(e => e.id_usuario, "fk_bono_usuario");

            entity.Property(e => e.descripcion).HasColumnType("text");
            entity.Property(e => e.nombre_bono).HasMaxLength(100);
            entity.Property(e => e.periodo).HasMaxLength(50);

            entity.HasOne(d => d.id_politica_bonoNavigation).WithMany(p => p.BONOs)
                .HasForeignKey(d => d.id_politica_bono)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_bono_politica");

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.BONOs)
                .HasForeignKey(d => d.id_usuario)
                .HasConstraintName("fk_bono_usuario");
        });

        modelBuilder.Entity<CAPACITACION>(entity =>
        {
            entity.HasKey(e => e.id_capacitacion).HasName("PRIMARY");

            entity.ToTable("CAPACITACION");

            entity.HasIndex(e => e.id_usuario, "fk_capacitacion_usuario");

            entity.Property(e => e.certificado).HasDefaultValueSql("'0'");
            entity.Property(e => e.institucion).HasMaxLength(200);
            entity.Property(e => e.nombre_curso).HasMaxLength(200);

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.CAPACITACIONs)
                .HasForeignKey(d => d.id_usuario)
                .HasConstraintName("fk_capacitacion_usuario");
        });

        modelBuilder.Entity<DOCUMENTO>(entity =>
        {
            entity.HasKey(e => e.id_documento).HasName("PRIMARY");

            entity.ToTable("DOCUMENTO");

            entity.HasIndex(e => e.id_tipo_documento, "fk_documento_tipo");

            entity.HasIndex(e => e.id_usuario, "fk_documento_usuario");

            entity.Property(e => e.fecha_subida)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.nombre_archivo).HasMaxLength(255);
            entity.Property(e => e.ruta_archivo).HasMaxLength(500);

            entity.HasOne(d => d.id_tipo_documentoNavigation).WithMany(p => p.DOCUMENTOs)
                .HasForeignKey(d => d.id_tipo_documento)
                .HasConstraintName("fk_documento_tipo");

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.DOCUMENTOs)
                .HasForeignKey(d => d.id_usuario)
                .HasConstraintName("fk_documento_usuario");
        });

        modelBuilder.Entity<EXPEDIENTE>(entity =>
        {
            entity.HasKey(e => e.id_expediente).HasName("PRIMARY");

            entity.ToTable("EXPEDIENTE");

            entity.HasIndex(e => e.cedula, "cedula").IsUnique();

            entity.HasIndex(e => e.id_usuario, "fk_expediente_usuario");

            entity.Property(e => e.cedula).HasMaxLength(50);
            entity.Property(e => e.ciudad).HasMaxLength(100);
            entity.Property(e => e.contacto_emergencia).HasMaxLength(200);
            entity.Property(e => e.direccion).HasMaxLength(300);
            entity.Property(e => e.estado).HasMaxLength(100);
            entity.Property(e => e.estado_civil).HasMaxLength(50);
            entity.Property(e => e.telefono_emergencia).HasMaxLength(20);

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.EXPEDIENTEs)
                .HasForeignKey(d => d.id_usuario)
                .HasConstraintName("fk_expediente_usuario");
        });

        modelBuilder.Entity<FERIADO>(entity =>
        {
            entity.HasKey(e => e.id_feriado).HasName("PRIMARY");

            entity.ToTable("FERIADO");

            entity.Property(e => e.descripcion).HasColumnType("text");
            entity.Property(e => e.es_laborable).HasDefaultValueSql("'0'");
            entity.Property(e => e.nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<HORARIO>(entity =>
        {
            entity.HasKey(e => e.id_horario).HasName("PRIMARY");

            entity.ToTable("HORARIO");

            entity.Property(e => e.dias).HasMaxLength(50);
            entity.Property(e => e.hora_entrada).HasColumnType("time");
            entity.Property(e => e.hora_salida).HasColumnType("time");
            entity.Property(e => e.nombre_horario).HasMaxLength(100);
        });

        modelBuilder.Entity<MARCA>(entity =>
        {
            entity.HasKey(e => e.id_marca).HasName("PRIMARY");

            entity.ToTable("MARCA");

            entity.Property(e => e.id_marca).ValueGeneratedOnAdd();
            entity.Property(e => e.comentario).HasColumnType("text");
            entity.Property(e => e.hora_entrada).HasColumnType("time");
            entity.Property(e => e.hora_salida).HasColumnType("time");
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.ubicancia).HasMaxLength(200);

            entity.HasOne(d => d.id_marcaNavigation).WithOne(p => p.MARCA)
                .HasForeignKey<MARCA>(d => d.id_marca)
                .HasConstraintName("fk_marca_usuario");
        });

        modelBuilder.Entity<NOTIFICACION>(entity =>
        {
            entity.HasKey(e => e.id_notificacion).HasName("PRIMARY");

            entity.ToTable("NOTIFICACION");

            entity.HasIndex(e => e.id_usuario, "fk_notificacion_usuario");

            entity.Property(e => e.fecha_notificacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.leida).HasDefaultValueSql("'0'");
            entity.Property(e => e.mensaje).HasColumnType("text");

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.NOTIFICACIONs)
                .HasForeignKey(d => d.id_usuario)
                .HasConstraintName("fk_notificacion_usuario");
        });

        modelBuilder.Entity<POLITICA_BONO>(entity =>
        {
            entity.HasKey(e => e.id_politica_bono).HasName("PRIMARY");

            entity.ToTable("POLITICA_BONO");

            entity.Property(e => e.activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.descripcion).HasColumnType("text");
            entity.Property(e => e.nombre_politica).HasMaxLength(100);
        });

        modelBuilder.Entity<ROL>(entity =>
        {
            entity.HasKey(e => e.id_rol).HasName("PRIMARY");

            entity.ToTable("ROL");

            entity.Property(e => e.descripcion).HasColumnType("text");
            entity.Property(e => e.nombre_rol).HasMaxLength(50);
        });

        modelBuilder.Entity<SOLICITUD>(entity =>
        {
            entity.HasKey(e => e.id_solicitud).HasName("PRIMARY");

            entity.ToTable("SOLICITUD");

            entity.HasIndex(e => e.id_tipo_solicitud, "fk_solicitud_tipo");

            entity.HasIndex(e => e.id_usuario, "fk_solicitud_usuario");

            entity.Property(e => e.descripcion).HasColumnType("text");
            entity.Property(e => e.descripcion_estado).HasColumnType("text");
            entity.Property(e => e.estado).HasMaxLength(50);

            entity.HasOne(d => d.id_tipo_solicitudNavigation).WithMany(p => p.SOLICITUDs)
                .HasForeignKey(d => d.id_tipo_solicitud)
                .HasConstraintName("fk_solicitud_tipo");

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.SOLICITUDs)
                .HasForeignKey(d => d.id_usuario)
                .HasConstraintName("fk_solicitud_usuario");
        });

        modelBuilder.Entity<TIPO_DOCUMENTO>(entity =>
        {
            entity.HasKey(e => e.id_tipo_documento).HasName("PRIMARY");

            entity.ToTable("TIPO_DOCUMENTO");

            entity.Property(e => e.descripcion).HasColumnType("text");
            entity.Property(e => e.nombre_tipo).HasMaxLength(100);
            entity.Property(e => e.obligatorio).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<TIPO_SOLICITUD>(entity =>
        {
            entity.HasKey(e => e.id_tipo_solicitud).HasName("PRIMARY");

            entity.ToTable("TIPO_SOLICITUD");

            entity.Property(e => e.nombre_solicitud).HasMaxLength(100);
        });

        modelBuilder.Entity<USUARIO>(entity =>
        {
            entity.HasKey(e => e.id_usuario).HasName("PRIMARY");

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.email, "email").IsUnique();

            entity.Property(e => e.activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.apellido).HasMaxLength(100);
            entity.Property(e => e.contraseña).HasMaxLength(255);
            entity.Property(e => e.email).HasMaxLength(150);
            entity.Property(e => e.estado).HasMaxLength(50);
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<USUARIO_HORARIO>(entity =>
        {
            entity.HasKey(e => e.id_usuario_horario).HasName("PRIMARY");

            entity.ToTable("USUARIO_HORARIO");

            entity.HasIndex(e => e.id_horario, "fk_usuario_horario_horario");

            entity.HasIndex(e => e.id_usuario, "fk_usuario_horario_usuario");

            entity.HasOne(d => d.id_horarioNavigation).WithMany(p => p.USUARIO_HORARIOs)
                .HasForeignKey(d => d.id_horario)
                .HasConstraintName("fk_usuario_horario_horario");

            entity.HasOne(d => d.id_usuarioNavigation).WithMany(p => p.USUARIO_HORARIOs)
                .HasForeignKey(d => d.id_usuario)
                .HasConstraintName("fk_usuario_horario_usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
