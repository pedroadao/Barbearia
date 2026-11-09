using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbearia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveFilterToCalendarioUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_calendario_id_barbeiro_data_hora_inicio",
                table: "calendario");

            migrationBuilder.CreateIndex(
                name: "IX_calendario_id_barbeiro_data_hora_inicio",
                table: "calendario",
                columns: new[] { "id_barbeiro", "data", "hora_inicio" },
                unique: true,
                filter: "id_status = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_calendario_id_barbeiro_data_hora_inicio",
                table: "calendario");

            migrationBuilder.CreateIndex(
                name: "IX_calendario_id_barbeiro_data_hora_inicio",
                table: "calendario",
                columns: new[] { "id_barbeiro", "data", "hora_inicio" },
                unique: true);
        }
    }
}
