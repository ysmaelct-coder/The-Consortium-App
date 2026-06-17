using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Controllers
{
    public class HomeController(ApplicationDbContext context, ILogger<HomeController> logger) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<HomeController> _logger = logger;

        // 1. PÁGINA PRINCIPAL PÚBLICA (LANDING PAGE)
        
        [HttpGet]
        public IActionResult Index()
        {
            // Si el usuario ya tiene sesión activa, se salta la publicidad y va al Tablero
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Dashboard");
            }

            return View(); // Renderiza la nueva presentación comercial
        }

        // 2. TABLERO PRIVADO (TU LOGICA BASE INTACTA)
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            string? email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login", "Account");

            // REGLA: ADMIN GLOBAL
            if (email == "admin@admin.com")
            {
                ViewBag.TotalEmpresas = await _context.Empresas.CountAsync();
                ViewBag.TotalConsorcios = await _context.Consorcios.CountAsync();
                ViewBag.TotalUnidades = await _context.UnidadesFuncionales.CountAsync();
                ViewBag.UnidadesActivas = ViewBag.TotalUnidades;

                ViewBag.GastosMesActual = await _context.Gastos
                    .Where(g => g.FechaRegistro.Month == DateTime.Today.Month &&
                                g.FechaRegistro.Year == DateTime.Today.Year)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0m;

                ViewBag.UltimasUnidades = await _context.UnidadesFuncionales
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToListAsync();

                ViewBag.UltimosGastos = await _context.Gastos
                    .OrderByDescending(g => g.FechaRegistro)
                    .Take(5)
                    .ToListAsync();

                return View();
            }

            // REGLA: EMPRESA NORMAL
            int? empresaId = HttpContext.Session.GetInt32("EmpresaId");
            if (empresaId == null)
                return RedirectToAction("Login", "Account");

            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;

            var consorciosIds = await _context.Consorcios
                .Where(c => c.EmpresaId == empresaId.Value)
                .Select(c => c.Id)
                .ToListAsync();

            ViewBag.TotalConsorcios = consorciosIds.Count;

            ViewBag.TotalUnidades = await _context.UnidadesFuncionales
                .CountAsync(u => consorciosIds.Contains(u.ConsorcioId));

            ViewBag.UnidadesActivas = ViewBag.TotalUnidades;

            ViewBag.GastosMesActual = await _context.Gastos
                .Where(g => consorciosIds.Contains(g.ConsorcioId)
                         && g.FechaRegistro.Year == year
                         && g.FechaRegistro.Month == month)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;

            ViewBag.UltimasUnidades = await _context.UnidadesFuncionales
                .Where(u => consorciosIds.Contains(u.ConsorcioId))
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.UltimosGastos = await _context.Gastos
                .Where(g => g.UsuarioId != null)
                .Where(g => consorciosIds.Contains(g.ConsorcioId))
                .OrderByDescending(g => g.FechaRegistro)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}