using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ PANEL ADMIN
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserEmail") != "admin@admin.com")
            {
                return RedirectToAction("Index", "Home");
            }

            var empresas = await _context.Empresas
                .Include(e => e.Consorcios)
                .ToListAsync();

            return View(empresas);
        }

        // ✅ ELIMINAR EMPRESA
        public async Task<IActionResult> Delete(int id)
        {
            var empresa = await _context.Empresas
                .Include(e => e.Consorcios)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (empresa == null)
                return NotFound();

            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var empresa = await _context.Empresas
                .Include(e => e.Consorcios)
                    .ThenInclude(c => c.UnidadesFuncionales)
                .Include(e => e.Consorcios)
                    .ThenInclude(c => c.Gastos)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (empresa == null)
                return NotFound();

            return View(empresa);
        }
    }
}