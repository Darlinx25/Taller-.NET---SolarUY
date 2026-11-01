using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarUY.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAlertasPanel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasPanel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ParqueSolarId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NombreParque = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    NodoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CodigoNodo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PanelId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CodigoSerie = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FechaInicioUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaFinUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasPanel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertasPanel_Nodos_NodoId",
                        column: x => x.NodoId,
                        principalTable: "Nodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertasPanel_Paneles_PanelId",
                        column: x => x.PanelId,
                        principalTable: "Paneles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertasPanel_ParquesSolares_ParqueSolarId",
                        column: x => x.ParqueSolarId,
                        principalTable: "ParquesSolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasPanel_FechaFinUtc",
                table: "AlertasPanel",
                column: "FechaFinUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasPanel_NodoId",
                table: "AlertasPanel",
                column: "NodoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasPanel_PanelId_FechaInicioUtc",
                table: "AlertasPanel",
                columns: new[] { "PanelId", "FechaInicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasPanel_ParqueSolarId",
                table: "AlertasPanel",
                column: "ParqueSolarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasPanel");
        }
    }
}
