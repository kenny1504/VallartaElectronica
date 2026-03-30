using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicaVallarta.Datos.Configuraciones;

public class SucursalConfiguracion : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("Sucursales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(x => x.FechaCreacion).HasColumnType("datetime2");
        builder.Property(x => x.FechaActualizacion).HasColumnType("datetime2");
        builder.HasIndex(x => new { x.PaisId, x.Nombre }).IsUnique();
        builder.HasIndex(x => new { x.PaisId, x.EstaActivo });
        builder.HasOne(x => x.Pais).WithMany(x => x.Sucursales).HasForeignKey(x => x.PaisId).OnDelete(DeleteBehavior.Restrict);
    }
}
