using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarUY.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResumenesDiariosHistoricos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResumenesDiariosParques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ParqueSolarId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NombreParque = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    FechaLocal = table.Column<DateOnly>(type: "date", nullable: false),
                    PotenciaAcumuladaW = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadLecturas = table.Column<int>(type: "int", nullable: false),
                    EnergiaKWh = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PotenciaMaximaW = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fallas = table.Column<int>(type: "int", nullable: false),
                    UltimoEstado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UltimaFechaHoraUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumenesDiariosParques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResumenesDiariosParques_ParquesSolares_ParqueSolarId",
                        column: x => x.ParqueSolarId,
                        principalTable: "ParquesSolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ResumenesDiariosNodos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResumenDiarioParqueId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NodoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CodigoNodo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FechaLocal = table.Column<DateOnly>(type: "date", nullable: false),
                    PotenciaAcumuladaW = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadLecturas = table.Column<int>(type: "int", nullable: false),
                    EnergiaKWh = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PotenciaMaximaW = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fallas = table.Column<int>(type: "int", nullable: false),
                    UltimoEstado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UltimaFechaHoraUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumenesDiariosNodos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResumenesDiariosNodos_Nodos_NodoId",
                        column: x => x.NodoId,
                        principalTable: "Nodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResumenesDiariosNodos_ResumenesDiariosParques_ResumenDiarioP~",
                        column: x => x.ResumenDiarioParqueId,
                        principalTable: "ResumenesDiariosParques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ResumenesDiariosPaneles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResumenDiarioNodoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PanelId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CodigoSerie = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FechaLocal = table.Column<DateOnly>(type: "date", nullable: false),
                    PotenciaAcumuladaW = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadLecturas = table.Column<int>(type: "int", nullable: false),
                    EnergiaKWh = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PotenciaMaximaW = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fallas = table.Column<int>(type: "int", nullable: false),
                    UltimoEstado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UltimaFechaHoraUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumenesDiariosPaneles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResumenesDiariosPaneles_Paneles_PanelId",
                        column: x => x.PanelId,
                        principalTable: "Paneles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResumenesDiariosPaneles_ResumenesDiariosNodos_ResumenDiarioN~",
                        column: x => x.ResumenDiarioNodoId,
                        principalTable: "ResumenesDiariosNodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesDiariosNodos_NodoId_FechaLocal",
                table: "ResumenesDiariosNodos",
                columns: new[] { "NodoId", "FechaLocal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesDiariosNodos_ResumenDiarioParqueId",
                table: "ResumenesDiariosNodos",
                column: "ResumenDiarioParqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesDiariosPaneles_PanelId_FechaLocal",
                table: "ResumenesDiariosPaneles",
                columns: new[] { "PanelId", "FechaLocal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesDiariosPaneles_ResumenDiarioNodoId",
                table: "ResumenesDiariosPaneles",
                column: "ResumenDiarioNodoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesDiariosParques_ParqueSolarId_FechaLocal",
                table: "ResumenesDiariosParques",
                columns: new[] { "ParqueSolarId", "FechaLocal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResumenesDiariosPaneles");

            migrationBuilder.DropTable(
                name: "ResumenesDiariosNodos");

            migrationBuilder.DropTable(
                name: "ResumenesDiariosParques");
        }
    }
}
