using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheConsortiumApp.Migrations
{
    /// <inheritdoc />
    public partial class CrearLiquidacionExpensasFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiquidacionesExpensa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsorcioId = table.Column<int>(type: "int", nullable: false),
                    Periodo = table.Column<int>(type: "int", nullable: false),
                    TotalGastos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionesExpensa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidacionesExpensa_Consorcios_ConsorcioId",
                        column: x => x.ConsorcioId,
                        principalTable: "Consorcios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidacionesExpensaDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiquidacionExpensaId = table.Column<int>(type: "int", nullable: false),
                    UnidadFuncionalId = table.Column<int>(type: "int", nullable: false),
                    Coeficiente = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoCalculado = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionesExpensaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidacionesExpensaDetalle_LiquidacionesExpensa_LiquidacionExpensaId",
                        column: x => x.LiquidacionExpensaId,
                        principalTable: "LiquidacionesExpensa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidacionesExpensaDetalle_UnidadesFuncionales_UnidadFuncionalId",
                        column: x => x.UnidadFuncionalId,
                        principalTable: "UnidadesFuncionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionesExpensa_ConsorcioId",
                table: "LiquidacionesExpensa",
                column: "ConsorcioId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionesExpensaDetalle_LiquidacionExpensaId",
                table: "LiquidacionesExpensaDetalle",
                column: "LiquidacionExpensaId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionesExpensaDetalle_UnidadFuncionalId",
                table: "LiquidacionesExpensaDetalle",
                column: "UnidadFuncionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiquidacionesExpensaDetalle");

            migrationBuilder.DropTable(
                name: "LiquidacionesExpensa");
        }
    }
}
