using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicaVallarta.Datos.Configuraciones;

public class UsuarioAdministradorConfiguracion : IEntityTypeConfiguration<UsuarioAdministrador>
{
    public void Configure(EntityTypeBuilder<UsuarioAdministrador> builder)
    {
        builder.ToTable("UsuariosAdministradores");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NombreUsuario).HasMaxLength(80).IsRequired();
        builder.Property(x => x.NombreCompleto).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ClaveHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.FechaCreacion).HasColumnType("datetime2");
        builder.Property(x => x.FechaActualizacion).HasColumnType("datetime2");
        builder.HasIndex(x => x.NombreUsuario).IsUnique();
    }
}
