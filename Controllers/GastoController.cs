using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
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
    public class GastoController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // Helpers de Sesión Optimizados y Seguros
        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");
        private string? GetUserEmail() => HttpContext.Session.GetString("UserEmail");

        // Helper: Obtener consorcios filtrados estrictamente por Empresa logueada
        private async Task<List<Consorcio>> GetMisConsorciosAsync(int empresaId)
        {
            return await _context.Consorcios
                .Where(c => c.EmpresaId == empresaId)
                .OrderBy(c => c.Nombre)
                .AsNoTracking()
                .ToListAsync();
        }

        // 1. LISTAR (INDEX) - PRIVADO POR EMPRESA
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var emailActual = GetUserEmail();
            var empresaId = GetEmpresaId();

            if (string.IsNullOrEmpty(emailActual) || empresaId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var misConsorciosIds = await _context.Consorcios
                .Where(c => c.EmpresaId == empresaId.Value)
                .Select(c => c.Id)
                .ToListAsync();

            var gastos = await _context.Gastos
                .Include(g => g.Consorcio)
                .Where(g => misConsorciosIds.Contains(g.ConsorcioId))
                .OrderByDescending(g => g.FechaRegistro)
                .ToListAsync();

            return View(gastos);
        }

        // 2. FORMULARIO CREAR (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var empresaId = GetEmpresaId();

            if (empresaId is null)
                return RedirectToAction("Login", "Account");

            ViewBag.Consorcios = await GetMisConsorciosAsync(empresaId.Value);

            return View();
        }

        // 3. PROCESAR CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Gasto gasto, IFormFile? archivoFactura)
        {
            var emailActual = GetUserEmail();
            var empresaId = GetEmpresaId();

            if (string.IsNullOrEmpty(emailActual) || empresaId == null)
                return RedirectToAction("Login", "Account");

            bool consorcioValido = await _context.Consorcios
                .AnyAsync(c => c.Id == gasto.ConsorcioId && c.EmpresaId == empresaId.Value);

            if (!consorcioValido)
                ModelState.AddModelError("ConsorcioId", "Seleccione un consorcio válido de su lista.");

            if (archivoFactura is not null && archivoFactura.Length > 0)
            {
                var ext = Path.GetExtension(archivoFactura.FileName).ToLowerInvariant();
                var permitidos = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

                if (!permitidos.Contains(ext))
                    ModelState.AddModelError("", "Solo se permiten PDF o imágenes.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Consorcios = await GetMisConsorciosAsync(empresaId.Value);
                return View(gasto);
            }

            if (archivoFactura is not null && archivoFactura.Length > 0)
            {
                var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivoFactura.FileName)}";
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "facturas");

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var ruta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await archivoFactura.CopyToAsync(stream);
                }

                gasto.ArchivoFactura = nombreArchivo;
            }

            gasto.FechaRegistro = DateTime.Now;

            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 4. ELIMINAR (DELETE)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null)
                return RedirectToAction("Login", "Account");

            var gasto = await _context.Gastos
                .Include(g => g.Consorcio)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gasto is null)
                return NotFound();

            if (gasto.Consorcio?.EmpresaId != empresaId.Value)
                return Forbid();

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 5. EXPORTAR PDF REFACTORIZADO CON DISEÑO PREMIUM CORPORATIVO
        [HttpGet]
        public async Task<IActionResult> ExportPdf(int? consorcioId, int? year, int? month)
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null)
                return RedirectToAction("Login", "Account");

            int y = year ?? DateTime.Today.Year;
            int m = month ?? DateTime.Today.Month;
            string periodoStr = $"{y}-{m:D2}";

            var misConsorcios = await GetMisConsorciosAsync(empresaId.Value);
            var misConsorciosIds = misConsorcios.Select(c => c.Id).ToList();

            var query = _context.Gastos
                .Include(g => g.Consorcio)
                .Where(g => misConsorciosIds.Contains(g.ConsorcioId))
                .AsQueryable();

            query = query.Where(g => g.FechaRegistro.Year == y && g.FechaRegistro.Month == m);

            string consorcioTitulo = "Todos los consorcios";

            if (consorcioId is > 0)
            {
                if (!misConsorciosIds.Contains(consorcioId.Value))
                {
                    return Forbid();
                }

                query = query.Where(g => g.ConsorcioId == consorcioId.Value);
                consorcioTitulo = misConsorcios.FirstOrDefault(c => c.Id == consorcioId.Value)?.Nombre ?? consorcioTitulo;
            }

            var gastos = await query.OrderByDescending(g => g.FechaRegistro).ToListAsync();
            var total = gastos.Sum(g => g.Monto);

            // Armado del PDF con espaciado controlado y fuentes legibles (Sintaxis compatible)
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Encabezado Corporativo elegante
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Contreras & Asociados").FontSize(22).Bold().FontColor("#0f172a");
                            col.Item().Text("Administración Integral de Consorcios").FontSize(10).Italic().FontColor("#64748b");
                            col.Item().PaddingTop(5).Text($"Consorcio: {consorcioTitulo}").FontSize(11).Bold().FontColor("#475569");
                        });

                        row.ConstantItem(160).Column(col =>
                        {
                            col.Item().Background("#f1f5f9").Padding(10).Column(innerCol =>
                            {
                                innerCol.Item().Text("REPORTE DE GASTOS").FontSize(9).Bold().FontColor("#1e293b");
                                innerCol.Item().Text($"Período: {periodoStr}").FontSize(11).Bold().FontColor("#0d6efd");
                                innerCol.Item().Text($"Fecha Emisión: {DateTime.Today.ToShortDateString()}").FontSize(8).FontColor("#64748b");
                            });
                        });
                    });

                    // Tabla de contenido central con anchos explícitos para que no se encima nada
                    page.Content().PaddingTop(25).Column(col =>
                    {
                        col.Item().Background("#0f172a").Padding(12).Row(row =>
                        {
                            row.RelativeItem().Text("TOTAL DE GASTOS REGISTRADOS:").Bold().FontColor(Colors.White).FontSize(11);
                            row.ConstantItem(150).Text($"$ {total:N2}").Bold().FontColor(Colors.White).FontSize(11).AlignRight();
                        });

                        col.Item().PaddingTop(20).Text("Detalle Analítico de Movimientos Financieros").FontSize(12).Bold().FontColor("#1e293b");

                        col.Item().PaddingTop(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);  // Fecha
                                columns.RelativeColumn(2);   // Concepto / Descripción
                                columns.RelativeColumn((float)1.5); // Categoría
                                columns.ConstantColumn(120); // Monto fijo amplio para millones sin romperse
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Fecha").Bold().FontColor("#475569");
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Concepto / Descripción").Bold().FontColor("#475569");
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Categoría").Bold().FontColor("#475569");
                                header.Cell().Background("#f8fafc").Padding(8).BorderBottom(2).BorderColor("#cbd5e1").Text("Monto").Bold().FontColor("#475569").AlignRight();
                            });

                            foreach (var g in gastos)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text(g.FechaRegistro.ToString("dd/MM/yyyy")).FontColor("#334155");
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text(g.Concepto).FontColor("#1e293b");
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text(g.Categoria.ToString()).FontColor("#475569");
                                tabla.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text($"$ {g.Monto:N2}").Bold().FontColor("#0f172a").AlignRight();
                            }
                        });
                    });

                    // Pie de Página corporativo
                    page.Footer().BorderTop(1).BorderColor("#cbd5e1").PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text("The Consortium App - Control de Auditoría Interna de Gastos.").FontSize(8).FontColor("#94a3b8");
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Página ").FontSize(8).FontColor("#94a3b8");
                            x.CurrentPageNumber().FontSize(8).FontColor("#94a3b8");
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Gastos_{consorcioTitulo.Replace(" ", "_")}_{periodoStr}.pdf");
        }
    }
}