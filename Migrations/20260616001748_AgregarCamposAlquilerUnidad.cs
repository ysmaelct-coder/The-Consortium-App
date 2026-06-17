using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheConsortiumApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposAlquilerUnidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchivoContrato",
                table: "UnidadesFuncionales",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivoContrato",
                table: "UnidadesFuncionales");
        }
    }
}
