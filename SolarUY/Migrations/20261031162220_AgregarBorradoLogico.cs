using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarUY.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBorradoLogico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "ParquesSolares",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "ParquesSolares",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Paneles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Paneles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Nodos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Nodos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Inversores",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Inversores",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "ParquesSolares");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "ParquesSolares");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Paneles");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Paneles");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Nodos");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Nodos");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Inversores");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Inversores");
        }
    }
}
