using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicaVallarta.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLogsAplicacionEnBaseDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Auditoria");

            migrationBuilder.CreateTable(
                name: "RegistrosLogsAplicacion",
                schema: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DetalleExcepcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Propiedades = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ruta = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    MetodoHttp = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TraceIdentifier = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Ambiente = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosLogsAplicacion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosLogsAplicacion_Categoria",
                schema: "Auditoria",
                table: "RegistrosLogsAplicacion",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosLogsAplicacion_FechaUtc",
                schema: "Auditoria",
                table: "RegistrosLogsAplicacion",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosLogsAplicacion_Nivel_FechaUtc",
                schema: "Auditoria",
                table: "RegistrosLogsAplicacion",
                columns: new[] { "Nivel", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosLogsAplicacion_TraceIdentifier",
                schema: "Auditoria",
                table: "RegistrosLogsAplicacion",
                column: "TraceIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosLogsAplicacion",
                schema: "Auditoria");
        }
    }
}
