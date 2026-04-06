using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicaVallarta.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRegistroConsultaAnalitica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Analitica");

            migrationBuilder.CreateTable(
                name: "RegistrosConsultasAnalitica",
                schema: "Analitica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaConsultaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaisId = table.Column<int>(type: "int", nullable: true),
                    NombrePais = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RegionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NombreRegion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SucursalId = table.Column<int>(type: "int", nullable: true),
                    NombreSucursal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TasaCambioRangoId = table.Column<int>(type: "int", nullable: true),
                    RangoMontoDesdeUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RangoMontoHastaUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DescripcionRango = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    MontoConsultadoUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ResultadoObtenido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TasaCambioAplicada = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    EsExitosa = table.Column<bool>(type: "bit", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IpCliente = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IdiomaNavegador = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RutaOrigen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Referer = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TiempoRespuestaMs = table.Column<long>(type: "bigint", nullable: false),
                    IdentificadorSesionAnonima = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MetodoHttp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosConsultasAnalitica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosConsultasAnalitica_Paises_PaisId",
                        column: x => x.PaisId,
                        principalTable: "Paises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosConsultasAnalitica_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosConsultasAnalitica_TasasCambioRango_TasaCambioRangoId",
                        column: x => x.TasaCambioRangoId,
                        principalTable: "TasasCambioRango",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosConsultasAnalitica_DescripcionRango",
                schema: "Analitica",
                table: "RegistrosConsultasAnalitica",
                column: "DescripcionRango");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosConsultasAnalitica_FechaConsultaUtc",
                schema: "Analitica",
                table: "RegistrosConsultasAnalitica",
                column: "FechaConsultaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosConsultasAnalitica_FechaConsultaUtc_EsExitosa",
                schema: "Analitica",
                table: "RegistrosConsultasAnalitica",
                columns: new[] { "FechaConsultaUtc", "EsExitosa" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosConsultasAnalitica_IdentificadorSesionAnonima",
                schema: "Analitica",
                table: "RegistrosConsultasAnalitica",
                column: "IdentificadorSesionAnonima");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosConsultasAnalitica_PaisId_FechaConsultaUtc",
                schema: "Analitica",
                table: "RegistrosConsultasAnalitica",
                columns: new[] { "PaisId", "FechaConsultaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosConsultasAnalitica_SucursalId_FechaConsultaUtc",
                schema: "Analitica",
                table: "RegistrosConsultasAnalitica",
                columns: new[] { "SucursalId", "FechaConsultaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosConsultasAnalitica_TasaCambioRangoId_FechaConsultaUtc",
                schema: "Analitica",
                table: "RegistrosConsultasAnalitica",
                columns: new[] { "TasaCambioRangoId", "FechaConsultaUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosConsultasAnalitica",
                schema: "Analitica");
        }
    }
}
