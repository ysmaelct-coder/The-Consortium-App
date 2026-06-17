using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Controllers
{
    public class ConsorcioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsorcioController> _logger;

        public ConsorcioController(ApplicationDbContext context, ILogger<ConsorcioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper: obtener EmpresaId desde sesión
        private int? GetEmpresaId()
        {
            return HttpContext.Session.GetInt32("EmpresaId");
        }

        // GET: Consorcio
        public async Task<IActionResult> Index()
        {
            try
            {
                int? empresaId = HttpContext.Session.GetInt32("EmpresaId");

                if (empresaId == null)
                    return RedirectToAction("Login", "Account");

                var consorcios = await _context.Consorcios
                    .Where(c => c.EmpresaId == empresaId.Value)
                    .ToListAsync();

                return View(consorcios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consorcios in Index action");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar los consorcios.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Consorcio/Create
        public IActionResult Create()
        {
            var empresaId = GetEmpresaId();
            if (empresaId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: Consorcio/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("Nombre,Direccion,Localidad")] Consorcio consorcio)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == null)
                return RedirectToAction("Login", "Account");
            consorcio.CreadoPorEmail = HttpContext.Session.GetString("UserEmail");
            consorcio.FechaCreacion = DateTime.Now;

            // set FK obligatoria
            consorcio.EmpresaId = empresaId.Value;

            if (!ModelState.IsValid)
                return View(consorcio);

            try
            {
                _context.Add(consorcio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consorcio");
                TempData["ErrorMessage"] = "Ocurrió un error al crear el consorcio.";
                return View(consorcio);
            }
        }

        // GET: Consorcio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == null)
                return RedirectToAction("Login", "Account");

            if (id == null) return NotFound();

            var consorcio = await _context.Consorcios
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && m.EmpresaId == empresaId.Value);

            if (consorcio == null) return NotFound();
            return View(consorcio);
        }

        // GET: Consorcio/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == null)
                return RedirectToAction("Login", "Account");

            if (id == null) return NotFound();

            var consorcio = await _context.Consorcios
                .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId.Value);

            if (consorcio == null) return NotFound();
            return View(consorcio);
        }

        // POST: Consorcio/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Direccion,Localidad")] Consorcio consorcio)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == null)
                return RedirectToAction("Login", "Account");

            if (id != consorcio.Id) return NotFound();

            // asegurar FK
            consorcio.EmpresaId = empresaId.Value;

            if (!ModelState.IsValid) return View(consorcio);

            try
            {
                _context.Update(consorcio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Consorcios.AnyAsync(e => e.Id == consorcio.Id && e.EmpresaId == empresaId.Value))
                    return NotFound();

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing consorcio");
                TempData["ErrorMessage"] = "Ocurrió un error al editar el consorcio.";
                return View(consorcio);
            }
        }

        // GET: Consorcio/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == null)
                return RedirectToAction("Login", "Account");

            if (id == null) return NotFound();

            var consorcio = await _context.Consorcios
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && m.EmpresaId == empresaId.Value);

            if (consorcio == null) return NotFound();
            return View(consorcio);
        }

        // POST: Consorcio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var consorcio = await _context.Consorcios
                    .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId.Value);

                if (consorcio != null)
                {
                    _context.Consorcios.Remove(consorcio);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting consorcio");
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el consorcio.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}