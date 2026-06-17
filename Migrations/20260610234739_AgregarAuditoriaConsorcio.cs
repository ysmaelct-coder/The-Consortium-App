using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheConsortiumApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuditoriaConsorcio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstaAlquilada",
                table: "UnidadesFuncionales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NombreInquilino",
                table: "UnidadesFuncionales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "Gastos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ArchivoFactura",
                table: "Gastos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "CantidadDepartamentos",
                table: "Consorcios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPorEmail",
                table: "Consorcios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Consorcios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaAlquilada",
                table: "UnidadesFuncionales");

            migrationBuilder.DropColumn(
                name: "NombreInquilino",
                table: "UnidadesFuncionales");

            migrationBuilder.DropColumn(
                name: "CantidadDepartamentos",
                table: "Consorcios");

            migrationBuilder.DropColumn(
                name: "CreadoPorEmail",
                table: "Consorcios");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Consorcios");

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "Gastos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ArchivoFactura",
                table: "Gastos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
