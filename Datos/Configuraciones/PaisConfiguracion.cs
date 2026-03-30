using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicaVallarta.Datos.Configuraciones;

public class PaisConfiguracion : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> builder)
    {
        builder.ToTable("Paises");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CodigoMoneda).HasMaxLength(10).IsRequired();
        builder.Property(x => x.SimboloMoneda).HasMaxLength(10).IsRequired();
        builder.Property(x => x.FechaCreacion).HasColumnType("datetime2");
        builder.Property(x => x.FechaActualizacion).HasColumnType("datetime2");
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
