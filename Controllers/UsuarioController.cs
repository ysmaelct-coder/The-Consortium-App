using Microsoft.AspNetCore.Mvc;
using TheConsortiumApp.Data;
using Microsoft.EntityFrameworkCore;


public class UsuarioController : Controller
{
    private readonly ApplicationDbContext _context;
    public UsuarioController (ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        return View(usuarios);
    }
}
