using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Controllers
{
    public class UnidadFuncionalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnidadFuncionalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTAR UNIDADES
        public async Task<IActionResult> Index()
        {
            var unidades = await _context.UnidadesFuncionales.ToListAsync();
            return View(unidades);
        }

        // FORM CREAR
        // FORM CREAR
        public async Task<IActionResult> Create()
        {
            ViewBag.Consorcios = await _context.Consorcios.ToListAsync();
            return View();
        }
        // GUARDAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnidadFuncional unidad)
        {
            // 🔥 Validación consorcio
            var consorcioExiste = await _context.Consorcios
                .AnyAsync(c => c.Id == unidad.ConsorcioId);

            if (!consorcioExiste)
            {
                ModelState.AddModelError("ConsorcioId", "Seleccione un consorcio válido");
            }

            // Si hay errores de validación, volvemos al form y recargamos el combo
            if (!ModelState.IsValid)
            {
                ViewBag.Consorcios = await _context.Consorcios.ToListAsync();
                return View(unidad);
            }
            // ✅ Valor temporal (se recalcula después)
            unidad.Coeficiente = 1;

            _context.UnidadesFuncionales.Add(unidad);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // ELIMINAR
        public async Task<IActionResult> Delete(int id)
        {
            var unidad = await _context.UnidadesFuncionales.FindAsync(id);

            if (unidad == null)
                return NotFound();

            _context.UnidadesFuncionales.Remove(unidad);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}