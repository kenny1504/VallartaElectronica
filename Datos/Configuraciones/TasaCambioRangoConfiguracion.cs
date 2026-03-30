using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicaVallarta.Datos.Configuraciones;

public class TasaCambioRangoConfiguracion : IEntityTypeConfiguration<TasaCambioRango>
{
    public void Configure(EntityTypeBuilder<TasaCambioRango> builder)
    {
        builder.ToTable("TasasCambioRango");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MontoDesdeUsd).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.MontoHastaUsd).HasPrecision(18, 2);
        builder.Property(x => x.TasaCambio).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.FechaTasa).HasColumnType("date").IsRequired();
        builder.Property(x => x.FechaCreacion).HasColumnType("datetime2");
        builder.Property(x => x.FechaActualizacion).HasColumnType("datetime2");
        builder.HasIndex(x => new { x.PaisId, x.SucursalId, x.FechaTasa, x.EstaActivo });
        builder.HasIndex(x => new { x.PaisId, x.SucursalId, x.FechaTasa, x.MontoDesdeUsd, x.MontoHastaUsd });
        builder.HasOne(x => x.Pais).WithMany(x => x.TasasCambioRango).HasForeignKey(x => x.PaisId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sucursal).WithMany(x => x.TasasCambioRango).HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Restrict);
    }
}
