using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarUY.Migrations
{
    /// <inheritdoc />
    public partial class EstadoNodoEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE `Nodos` SET `Estado` = 'Activo' " +
                "WHERE LOWER(TRIM(`Estado`)) IN ('prendido', 'activo');");

            migrationBuilder.Sql(
                "UPDATE `Nodos` SET `Estado` = 'Inactivo' " +
                "WHERE LOWER(TRIM(`Estado`)) IN ('inactivo', 'enfalla', 'en falla');");

            migrationBuilder.Sql(
                "UPDATE `Nodos` SET `Estado` = 'EnMantenimiento' " +
                "WHERE LOWER(TRIM(`Estado`)) IN ('enmantenimiento', 'en mantenimiento', 'mantenimiento');");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Nodos_Estado",
                table: "Nodos",
                sql: "`Estado` IN ('Activo', 'Inactivo', 'EnMantenimiento')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Nodos_Estado",
                table: "Nodos");
        }
    }
}
