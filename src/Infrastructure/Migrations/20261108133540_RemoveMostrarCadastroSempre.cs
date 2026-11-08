using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbearia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMostrarCadastroSempre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mostrar_cadastro_sempre",
                table: "usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "mostrar_cadastro_sempre",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
