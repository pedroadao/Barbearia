using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbearia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMetodoETabelaToLogAlteracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "metodo",
                table: "log_alteracao",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tabela",
                table: "log_alteracao",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "metodo",
                table: "log_alteracao");

            migrationBuilder.DropColumn(
                name: "tabela",
                table: "log_alteracao");
        }
    }
}
