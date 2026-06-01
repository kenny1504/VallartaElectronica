using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicaVallarta.Datos.Configuraciones;

public class PublicidadConfiguracion : IEntityTypeConfiguration<Publicidad>
{
    public void Configure(EntityTypeBuilder<Publicidad> builder)
    {
        builder.ToTable("Publicidades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(500);
        builder.Property(x => x.TipoRecurso).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.UrlRecurso).HasMaxLength(512).IsRequired();
        builder.Property(x => x.DuracionSegundos).IsRequired();
        builder.Property(x => x.Orden).IsRequired();
        builder.Property(x => x.EstaActivo).HasColumnName("Activo");
        builder.Property(x => x.FechaInicio).HasColumnType("datetime2");
        builder.Property(x => x.FechaFin).HasColumnType("datetime2");
        builder.Property(x => x.FechaCreacion).HasColumnType("datetime2");
        builder.Property(x => x.FechaActualizacion).HasColumnType("datetime2");
        builder.HasIndex(x => new { x.EstaActivo, x.Orden });
        builder.HasIndex(x => new { x.FechaInicio, x.FechaFin });
    }
}
