using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheConsortiumApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdToGastoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
    name: "UsuarioId",
    table: "Gastos",
    type: "int",
    nullable: true);


            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Consorcios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_UsuarioId",
                table: "Gastos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Consorcios_UsuarioId",
                table: "Consorcios",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consorcios_Usuarios_UsuarioId",
                table: "Consorcios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_Usuarios_UsuarioId",
                table: "Gastos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consorcios_Usuarios_UsuarioId",
                table: "Consorcios");

            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_Usuarios_UsuarioId",
                table: "Gastos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Gastos_UsuarioId",
                table: "Gastos");

            migrationBuilder.DropIndex(
                name: "IX_Consorcios_UsuarioId",
                table: "Consorcios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Consorcios");
        }
    }
}
