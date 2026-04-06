using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicaVallarta.Datos.Configuraciones;

public class RegistroConsultaAnaliticaConfiguracion : IEntityTypeConfiguration<RegistroConsultaAnalitica>
{
    public void Configure(EntityTypeBuilder<RegistroConsultaAnalitica> builder)
    {
        builder.ToTable("RegistrosConsultasAnalitica", "Analitica");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FechaConsultaUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.NombrePais).HasMaxLength(120);
        builder.Property(x => x.RegionId).HasMaxLength(50);
        builder.Property(x => x.NombreRegion).HasMaxLength(150);
        builder.Property(x => x.NombreSucursal).HasMaxLength(150);
        builder.Property(x => x.DescripcionRango).HasMaxLength(120);
        builder.Property(x => x.MontoConsultadoUsd).HasPrecision(18, 2);
        builder.Property(x => x.ResultadoObtenido).HasPrecision(18, 2);
        builder.Property(x => x.TasaCambioAplicada).HasPrecision(18, 6);
        builder.Property(x => x.RangoMontoDesdeUsd).HasPrecision(18, 2);
        builder.Property(x => x.RangoMontoHastaUsd).HasPrecision(18, 2);
        builder.Property(x => x.IpCliente).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1024);
        builder.Property(x => x.IdiomaNavegador).HasMaxLength(128);
        builder.Property(x => x.RutaOrigen).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Referer).HasMaxLength(1024);
        builder.Property(x => x.IdentificadorSesionAnonima).HasMaxLength(64);
        builder.Property(x => x.MetodoHttp).HasMaxLength(10);
        builder.Property(x => x.MensajeError).HasMaxLength(1024);
        builder.Property(x => x.FechaCreacion).HasColumnType("datetime2");
        builder.Property(x => x.FechaActualizacion).HasColumnType("datetime2");

        builder.HasIndex(x => x.FechaConsultaUtc);
        builder.HasIndex(x => new { x.FechaConsultaUtc, x.EsExitosa });
        builder.HasIndex(x => new { x.PaisId, x.FechaConsultaUtc });
        builder.HasIndex(x => new { x.SucursalId, x.FechaConsultaUtc });
        builder.HasIndex(x => new { x.TasaCambioRangoId, x.FechaConsultaUtc });
        builder.HasIndex(x => x.IdentificadorSesionAnonima);
        builder.HasIndex(x => x.DescripcionRango);

        builder.HasOne(x => x.Pais)
            .WithMany(x => x.RegistrosConsultasAnalitica)
            .HasForeignKey(x => x.PaisId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sucursal)
            .WithMany(x => x.RegistrosConsultasAnalitica)
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TasaCambioRango)
            .WithMany(x => x.RegistrosConsultasAnalitica)
            .HasForeignKey(x => x.TasaCambioRangoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
