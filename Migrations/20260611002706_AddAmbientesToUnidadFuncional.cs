using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheConsortiumApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAmbientesToUnidadFuncional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ambientes",
                table: "UnidadesFuncionales",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ambientes",
                table: "UnidadesFuncionales");
        }
    }
}
