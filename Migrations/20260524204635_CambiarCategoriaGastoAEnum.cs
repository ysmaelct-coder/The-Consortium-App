using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheConsortiumApp.Migrations
{
    /// <inheritdoc />
    public partial class CambiarCategoriaGastoAEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Gastos]
SET [Categoria] =
    CASE 
        WHEN LOWER(LTRIM(RTRIM([Categoria]))) LIKE '%sueldo%' THEN '1'
        WHEN LOWER(LTRIM(RTRIM([Categoria]))) LIKE '%manten%' THEN '2'
        WHEN LOWER(LTRIM(RTRIM([Categoria]))) LIKE '%serv%' THEN '3'
        WHEN LOWER(LTRIM(RTRIM([Categoria]))) LIKE '%honor%' THEN '4'
        WHEN LOWER(LTRIM(RTRIM([Categoria]))) LIKE '%limp%' THEN '5'
        WHEN LOWER(LTRIM(RTRIM([Categoria]))) LIKE '%seguro%' THEN '6'
        ELSE '99'
    END
");

            migrationBuilder.AlterColumn<int>(
                name: "Categoria",
                table: "Gastos",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "Gastos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
