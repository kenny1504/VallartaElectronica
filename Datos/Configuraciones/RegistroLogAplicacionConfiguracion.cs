using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicaVallarta.Datos.Configuraciones;

public class RegistroLogAplicacionConfiguracion : IEntityTypeConfiguration<RegistroLogAplicacion>
{
    public void Configure(EntityTypeBuilder<RegistroLogAplicacion> builder)
    {
        builder.ToTable("RegistrosLogsAplicacion", "Auditoria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FechaUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.Nivel).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Categoria).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Mensaje).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.DetalleExcepcion).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Propiedades).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Ruta).HasMaxLength(512);
        builder.Property(x => x.MetodoHttp).HasMaxLength(12);
        builder.Property(x => x.Usuario).HasMaxLength(120);
        builder.Property(x => x.TraceIdentifier).HasMaxLength(128);
        builder.Property(x => x.Ambiente).HasMaxLength(80);

        builder.HasIndex(x => x.FechaUtc);
        builder.HasIndex(x => new { x.Nivel, x.FechaUtc });
        builder.HasIndex(x => x.Categoria);
        builder.HasIndex(x => x.TraceIdentifier);
    }
}
