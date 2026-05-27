using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;

namespace TheConsortiumApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Empresa> _hasher = new();

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Si ya hay sesión, lo mandamos al Home
            if (HttpContext.Session.GetString("UserEmail") != null)
                return RedirectToAction("Index", "Home");

            return View(new Empresa());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Empresa empresa, string password)
        {
            // Validación del modelo (RazonSocial, Cuit, Email, etc.)
            // OJO: empresa.PasswordHash es Required en tu modelo, lo seteamos abajo
            ModelState.Remove(nameof(Empresa.PasswordHash));

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");

            // Email único (no duplicar empresas con el mismo correo)
            var emailNormalizado = (empresa.Email ?? "").Trim();
            empresa.Email = emailNormalizado;

            if (await _context.Empresas.AnyAsync(e => e.Email == empresa.Email))
                ModelState.AddModelError(nameof(Empresa.Email), "Ya existe una empresa registrada con ese email.");

            if (!ModelState.IsValid)
                return View(empresa);

            // Hash real de password (queda guardado en PasswordHash)
            empresa.PasswordHash = _hasher.HashPassword(empresa, password);

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            // Auto-login después de registrar
            HttpContext.Session.SetString("UserEmail", empresa.Email);
            HttpContext.Session.SetInt32("EmpresaId", empresa.Id);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Si no hay empresas registradas, forzamos registro primero
            if (!_context.Empresas.Any())
                return RedirectToAction("Register");

            // Si ya hay sesión, al dashboard
            if (HttpContext.Session.GetString("UserEmail") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            email = (email ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email y contraseña son obligatorios.";
                return View();
            }

            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Email == email);
            if (empresa == null)
            {
                ViewBag.Error = "No existe una empresa registrada con ese email.";
                return View();
            }

            // Verificar hash
            var result = _hasher.VerifyHashedPassword(empresa, empresa.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Email o contraseña incorrectos.";
                return View();
            }

            HttpContext.Session.SetString("UserEmail", empresa.Email);
            HttpContext.Session.SetInt32("EmpresaId", empresa.Id);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}