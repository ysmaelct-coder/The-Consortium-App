using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Controllers
{
    public class ReportesController : Controller
    {
        // ✅ ESTE CAMPO ES EL QUE TE FALTABA (por eso _context estaba en rojo)
        private readonly ApplicationDbContext _context;

        // ✅ UN SOLO CONSTRUCTOR con DI (inyección)
        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Reportes?consorcioId=1&year=2026&month=5
        public async Task<IActionResult> Index(int? consorcioId, int? year, int? month)
        {
            // 1) parámetros (si no vienen, uso mes/año actual)
            int y = year ?? DateTime.Today.Year;
            int m = month ?? DateTime.Today.Month;
            int periodo = y * 100 + m;

            // 2) dropdown de consorcios
            var consorcios = await _context.Consorcios
                .OrderBy(c => c.Nombre)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Consorcios = consorcios;

            // Si no eligió consorcio, agarro el primero (si existe)
            if (consorcioId == null && consorcios.Count > 0)
                consorcioId = consorcios[0].Id;

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

            // 3) Histórico (✅ ESTO ES LO QUE ME PREGUNTASTE “DÓNDE PEGARLO”)
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
                // aún no generaste expensas para ese período
                ViewBag.TotalGastos = 0m;
                return View(new List<LiquidacionExpensaDetalle>());
            }

            ViewBag.TotalGastos = liquidacion.TotalGastos;

            // 5) Traer detalles para mostrar (unidad + propietario + % + monto)
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
            int periodo = year * 100 + month;

            // Total de gastos del período para ese consorcio
            decimal totalGastos = await _context.Gastos
                .Where(g => g.ConsorcioId == consorcioId
                         && g.FechaRegistro.Year == year
                         && g.FechaRegistro.Month == month)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;

            // Unidades del consorcio
            var unidades = await _context.UnidadesFuncionales
                .Where(u => u.ConsorcioId == consorcioId)
                .ToListAsync();

            // Si ya existía liquidación para ese período, borramos y recreamos
            var existente = await _context.LiquidacionesExpensa
                .Include(l => l.Detalles)
                .FirstOrDefaultAsync(l => l.ConsorcioId == consorcioId && l.Periodo == periodo);

            if (existente != null)
            {
                _context.LiquidacionesExpensaDetalle.RemoveRange(existente.Detalles);
                _context.LiquidacionesExpensa.Remove(existente);
                await _context.SaveChangesAsync();
            }

            // Crear liquidación nueva
            var liquidacion = new LiquidacionExpensa
            {
                ConsorcioId = consorcioId,
                Periodo = periodo,
                TotalGastos = totalGastos,
                FechaGeneracion = DateTime.Now
            };

            _context.LiquidacionesExpensa.Add(liquidacion);
            await _context.SaveChangesAsync();

            // Crear detalles por unidad (monto = totalGastos * coeficiente)
            foreach (var u in unidades)
            {
                _context.LiquidacionesExpensaDetalle.Add(new LiquidacionExpensaDetalle
                {
                    LiquidacionExpensaId = liquidacion.Id,
                    UnidadFuncionalId = u.Id,
                    Coeficiente = u.Coeficiente,
                    MontoCalculado = Math.Round(totalGastos * u.Coeficiente, 2)
                });
            }

            await _context.SaveChangesAsync();

            // volver al reporte filtrado
            return RedirectToAction("Index", new { consorcioId, year, month });
        }
    }
}