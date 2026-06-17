using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TheConsortiumApp.Controllers
{
    public class ReportesController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // Helper seguro para obtener la Empresa desde la Sesión
        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");

        // GET: /Reportes?consorcioId=1&year=2026&month=5
        [HttpGet]
        public async Task<IActionResult> Index(int? consorcioId, int? year, int? month)
        {
            // Validar sesión de la Empresa
            int? empresaId = GetEmpresaId();
            if (empresaId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 1) Parámetros de fecha (si no vienen, uso mes/año actual)
            int y = year ?? DateTime.Today.Year;
            int m = month ?? DateTime.Today.Month;
            int periodo = y * 100 + m;

            // 2) DROPDOWN BLINDADO: Traer SOLO los consorcios que le pertenecen a esta empresa
            var consorcios = await _context.Consorcios
                .Where(c => c.EmpresaId == empresaId.Value) // <-- Clave del aislamiento privado
                .OrderBy(c => c.Nombre)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Consorcios = consorcios;

            // Si no eligió consorcio y la empresa tiene consorcios, agarro el primero de SU lista
            if (consorcioId == null && consorcios.Count > 0)
            {
                consorcioId = consorcios[0].Id;
            }

            // VALIDACIÓN DE SEGURIDAD EXTREMA
            if (consorcioId != null && !consorcios.Any(c => c.Id == consorcioId.Value))
            {
                return Forbid(); // Bloqueo 403
            }

            ViewBag.ConsorcioIdSeleccionado = consorcioId ?? 0;
            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.Periodo = periodo;

            if (consorcioId == null)
            {
                ViewBag.TotalGastos = 0m;
                ViewBag.Historico = new List<LiquidacionExpensa>();
                return View(new List<LiquidacionExpensaDetalle>());
            }

            // 3) Histórico filtrado exclusivamente por el consorcio de este usuario
            ViewBag.Historico = await _context.LiquidacionesExpensa
                .Where(l => l.ConsorcioId == consorcioId.Value)
                .OrderByDescending(l => l.Periodo)
                .AsNoTracking()
                .ToListAsync();

            // 4) Buscar liquidación existente para ese consorcio y período
            var liquidacion = await _context.LiquidacionesExpensa
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.ConsorcioId == consorcioId.Value && l.Periodo == periodo);

            if (liquidacion == null)
            {
                ViewBag.TotalGastos = 0m;
                return View(new List<LiquidacionExpensaDetalle>());
            }

            ViewBag.TotalGastos = liquidacion.TotalGastos;

            // 5) Traer detalles para mostrar vinculados a la liquidación validada
            var detalles = await _context.LiquidacionesExpensaDetalle
                .Include(d => d.UnidadFuncional)
                .Where(d => d.LiquidacionExpensaId == liquidacion.Id)
                .AsNoTracking()
                .ToListAsync();

            return View(detalles);
        }

        // POST: /Reportes/Generar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar(int consorcioId, int year, int month)
        {
            int? empresaId = GetEmpresaId();
            if (empresaId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            bool esMio = await _context.Consorcios
                .AnyAsync(c => c.Id == consorcioId && c.EmpresaId == empresaId.Value);

            if (!esMio)
            {
                return Forbid();
            }

            int periodo = year * 100 + month;

            var unidades = await _context.UnidadesFuncionales
                .Where(u => u.ConsorcioId == consorcioId)
                .ToListAsync();

            if (unidades.Count == 0)
            {
                TempData["Error"] = "El consorcio no tiene unidades funcionales registradas.";
                return RedirectToAction("Index", new { consorcioId, year, month });
            }

            decimal sumaCoeficientes = unidades.Sum(u => u.Coeficiente);
            if (Math.Abs(sumaCoeficientes - 100m) > 0.5m && sumaCoeficientes > 0)
            {
                if (Math.Abs(sumaCoeficientes - 1m) > 0.05m)
                {
                    TempData["Error"] = $"La suma de los coeficientes del consorcio debe dar 100%. Actualmente suma: {sumaCoeficientes}%";
                    return RedirectToAction("Index", new { consorcioId, year, month });
                }
            }

            decimal totalGastos = await _context.Gastos
                .Where(g => g.ConsorcioId == consorcioId
                         && g.FechaRegistro.Year == year
                         && g.FechaRegistro.Month == month)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;

            var existente = await _context.LiquidacionesExpensa
                .Include(l => l.Detalles)
                .FirstOrDefaultAsync(l => l.ConsorcioId == consorcioId && l.Periodo == periodo);

            if (existente != null)
            {
                _context.LiquidacionesExpensaDetalle.RemoveRange(existente.Detalles);
                _context.LiquidacionesExpensa.Remove(existente);
                await _context.SaveChangesAsync();
            }

            var liquidacion = new LiquidacionExpensa
            {
                ConsorcioId = consorcioId,
                Periodo = periodo,
                TotalGastos = totalGastos,
                FechaGeneracion = DateTime.Now
            };

            _context.LiquidacionesExpensa.Add(liquidacion);
            await _context.SaveChangesAsync();

            foreach (var u in unidades)
            {
                decimal factor = sumaCoeficientes > 2m ? (u.Coeficiente / 100m) : u.Coeficiente;
                decimal montoCalculado = Math.Round(totalGastos * factor, 2);

                _context.LiquidacionesExpensaDetalle.Add(new LiquidacionExpensaDetalle
                {
                    LiquidacionExpensaId = liquidacion.Id,
                    UnidadFuncionalId = u.Id,
                    Coeficiente = u.Coeficiente,
                    MontoCalculado = montoCalculado
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { consorcioId, year, month });
        }

        // GET: /Reportes/DescargarPdf?consorcioId=1&periodo=202605
        [HttpGet]
        public async Task<IActionResult> DescargarPdf(int consorcioId, int periodo)
        {
            int? empresaId = GetEmpresaId();
            if (empresaId == null) return RedirectToAction("Login", "Account");

            var consorcio = await _context.Consorcios
                .FirstOrDefaultAsync(c => c.Id == consorcioId && c.EmpresaId == empresaId.Value);

            if (consorcio == null) return Forbid();

            var liquidacion = await _context.LiquidacionesExpensa
                .FirstOrDefaultAsync(l => l.ConsorcioId == consorcioId && l.Periodo == periodo);

            if (liquidacion == null) return NotFound("No se encontró la liquidación para este período.");

            var detalles = await _context.LiquidacionesExpensaDetalle
                .Include(d => d.UnidadFuncional)
                .Where(d => d.LiquidacionExpensaId == liquidacion.Id)
                .ToListAsync();

            byte[] pdfBytes = GenerarPdfLiquidacion(consorcio, detalles, periodo, liquidacion.TotalGastos);

            string nombreArchivo = $"Expensas_{consorcio.Nombre}_{periodo}.pdf";
            return File(pdfBytes, "application/pdf", nombreArchivo);
        }

        // METODO PRIVADO: DISEÑO CORPORATIVO PREMIUM DE QUESTPDF
        private static byte[] GenerarPdfLiquidacion(Consorcio consorcio, List<LiquidacionExpensaDetalle> detalles, int periodo, decimal totalGastos)
        {
            string anio = periodo.ToString().Substring(0, 4);
            string mesNum = periodo.ToString().Substring(4, 2);
            string[] meses = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];
            string nombreMes = meses[int.Parse(mesNum) - 1];

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // 1) Encabezado de la Firma
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Contreras & Asociados").FontSize(22).Bold().FontColor("#0f172a");
                            col.Item().Text("Administración Integral de Consorcios").FontSize(10).Italic().FontColor("#64748b");
                            col.Item().Text($"Dirección: {consorcio.Direccion} - {consorcio.Localidad}").FontSize(9).FontColor("#475569");
                        });

                        row.ConstantItem(160).Column(col =>
                        {
                            col.Item().Background("#f1f5f9").Padding(10).Column(innerCol =>
                            {
                                innerCol.Item().Text("LIQUIDACIÓN").FontSize(9).Bold().FontColor("#1e293b");
                                innerCol.Item().Text($"Período: {nombreMes} {anio}").FontSize(11).Bold().FontColor("#0d6efd");
                                innerCol.Item().Text($"Fecha: {DateTime.Today.ToShortDateString()}").FontSize(8).FontColor("#64748b");
                            });
                        });
                    });

                    // 2) Tabla Central de Prorrateo
                    page.Content().PaddingTop(25).Column(col =>
                    {
                        col.Item().Background("#0f172a").Padding(12).Row(row =>
                        {
                            row.RelativeItem().Text("GASTO TOTAL CONSOLIDADO DEL MES:").Bold().FontColor(Colors.White);
                            row.ConstantItem(150).Text($"$ {totalGastos:N2}").Bold().FontColor(Colors.White).AlignRight();
                        });

                        col.Item().PaddingTop(20).Text("Distribución de Expensas por Coeficiente").FontSize(12).Bold().FontColor("#1e293b");

                        col.Item().PaddingTop(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);  // U.F.
                                columns.RelativeColumn();    // Propietario
                                columns.ConstantColumn(100); // Ambientes
                                columns.ConstantColumn(90);  // Coeficiente
                                columns.ConstantColumn(120); // Monto
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Unidad").Bold().FontColor("#475569");
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Copropietario / Inquilino").Bold().FontColor("#475569");
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Ambientes").Bold().FontColor("#475569");
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Coeficiente").Bold().FontColor("#475569").AlignRight();
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Monto Expensa").Bold().FontColor("#475569").AlignRight();
                            });

                            foreach (var detalle in detalles)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text($"{detalle.UnidadFuncional?.PisoDepto}").FontColor("#334155");
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text($"{detalle.UnidadFuncional?.NombrePropietario}").FontColor("#1e293b");
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text($"{detalle.UnidadFuncional?.Ambientes}").FontColor("#475569");
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text($"{detalle.Coeficiente:N2} %").AlignRight();
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text($"$ {detalle.MontoCalculado:N2}").Bold().FontColor("#0f172a").AlignRight();
                            }
                        });
                    });

                    // 3) Pie de Página
                    page.Footer().BorderTop(1).BorderColor("#cbd5e1").PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text("The Consortium App - Reporte Oficial de Prorrateo Legal.").FontSize(8).FontColor("#94a3b8");
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Página ").FontSize(8).FontColor("#94a3b8");
                            x.CurrentPageNumber().FontSize(8).FontColor("#94a3b8");
                        });
                    });
                });
            });

            return documento.GeneratePdf();
        }
    }
}