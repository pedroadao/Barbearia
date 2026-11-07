using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Barbearia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdStatusToUsuarioAndCalendario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_status",
                table: "usuario",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "id_status",
                table: "calendario",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                "INSERT INTO status (id, descricao) VALUES (1, 'Ativo'), (2, 'Excluído') ON CONFLICT (id) DO NOTHING;");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_status",
                table: "usuario",
                column: "id_status");

            migrationBuilder.CreateIndex(
                name: "IX_calendario_id_status",
                table: "calendario",
                column: "id_status");

            migrationBuilder.AddForeignKey(
                name: "FK_calendario_status_id_status",
                table: "calendario",
                column: "id_status",
                principalTable: "status",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_status_id_status",
                table: "usuario",
                column: "id_status",
                principalTable: "status",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calendario_status_id_status",
                table: "calendario");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_status_id_status",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "IX_usuario_id_status",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "IX_calendario_id_status",
                table: "calendario");

            migrationBuilder.DropColumn(
                name: "id_status",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "id_status",
                table: "calendario");
        }
    }
}
