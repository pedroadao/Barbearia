using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbearia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUserIdAndMostrarCadastroSempre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auth_user_id",
                table: "usuario",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "mostrar_cadastro_sempre",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_auth_user_id",
                table: "usuario",
                column: "auth_user_id",
                unique: true,
                filter: "auth_user_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuario_auth_user_id",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "auth_user_id",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "mostrar_cadastro_sempre",
                table: "usuario");
        }
    }
}
