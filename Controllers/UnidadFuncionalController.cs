using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TheConsortiumApp.Data;
using TheConsortiumApp.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TheConsortiumApp.Controllers
{
    public class UnidadFuncionalController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context 
            = context;

        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");
        private string? GetUserEmail() => HttpContext.Session.GetString("UserEmail");

        // LISTAR UNIDADES - FILTRADO ESTRICTAMENTE POR EMPRESA LOGUEADA
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

            var unidades = await _context.UnidadesFuncionales
                .Include(u => u.Consorcio)
                .Where(u => misConsorciosIds.Contains(u.ConsorcioId))
                .ToListAsync();

            return View(unidades);
        }

        // FORM CREAR (GET)
        public async Task<IActionResult> Create()
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null) return RedirectToAction("Login", "Account");

            ViewBag.Consorcios = await _context.Consorcios
                .Where(c => c.EmpresaId == empresaId.Value)
                .OrderBy(c => c.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return View();
        }

        // GUARDAR CREACIÓN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnidadFuncional unidad)
        {
            var emailActual = GetUserEmail();
            var empresaId = GetEmpresaId();

            if (string.IsNullOrEmpty(emailActual) || empresaId == null)
                return RedirectToAction("Login", "Account");

            bool consorcioValido = await _context.Consorcios
                .AnyAsync(c => c.Id == unidad.ConsorcioId && c.EmpresaId == empresaId.Value);

            if (!consorcioValido)
            {
                ModelState.AddModelError("ConsorcioId", "Seleccione un consorcio válido.");
            }

            if (unidad.EstaAlquilada)
            {
                if (string.IsNullOrWhiteSpace(unidad.NombreInquilino))
                {
                    ModelState.AddModelError("NombreInquilino", "El nombre del inquilino es obligatorio si la unidad está alquilada.");
                }
                if (unidad.ArchivoInquilino == null || unidad.ArchivoInquilino.Length == 0)
                {
                    ModelState.AddModelError("", "Debe adjuntar obligatoriamente el documento del contrato.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Consorcios = await _context.Consorcios
                    .Where(c => c.EmpresaId == empresaId.Value)
                    .OrderBy(c => c.Nombre)
                    .AsNoTracking()
                    .ToListAsync();
                return View(unidad);
            }

            if (unidad.EstaAlquilada && unidad.ArchivoInquilino != null && unidad.ArchivoInquilino.Length > 0)
            {
                var nombreArchivoUnico = $"Contrato_{Guid.NewGuid()}{Path.GetExtension(unidad.ArchivoInquilino.FileName)}";
                var carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contratos");

                if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivoUnico);
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await unidad.ArchivoInquilino.CopyToAsync(stream);
                }

                unidad.ArchivoContrato = nombreArchivoUnico;
            }

            unidad.Coeficiente = 0m;
            _context.UnidadesFuncionales.Add(unidad);
            await _context.SaveChangesAsync();

            await RecalcularCoeficientesConsorcioAsync(unidad.ConsorcioId);
            return RedirectToAction(nameof(Index));
        }

        // FORM EDICIÓN / GESTIÓN DE ALQUILER (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null) return RedirectToAction("Login", "Account");

            var unidad = await _context.UnidadesFuncionales
                .Include(u => u.Consorcio)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidad == null) return NotFound();
            if (unidad.Consorcio?.EmpresaId != empresaId.Value) return Forbid();

            return View(unidad);
        }

        // GUARDAR EDICIÓN / GESTIÓN DE ALQUILER (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UnidadFuncional unidad)
        {
            if (id != unidad.Id) return NotFound();

            var empresaId = GetEmpresaId();
            if (empresaId is null) return RedirectToAction("Login", "Account");

            var unidadEnDb = await _context.UnidadesFuncionales
                .Include(u => u.Consorcio)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidadEnDb == null) return NotFound();
            if (unidadEnDb.Consorcio?.EmpresaId != empresaId.Value) return Forbid();

            if (unidad.EstaAlquilada && string.IsNullOrWhiteSpace(unidad.NombreInquilino))
            {
                ModelState.AddModelError("NombreInquilino", "El nombre del inquilino es obligatorio.");
            }

            if (!ModelState.IsValid) return View(unidad);

            try
            {
                unidadEnDb.EstaAlquilada = unidad.EstaAlquilada;

                if (unidad.EstaAlquilada)
                {
                    unidadEnDb.NombreInquilino = unidad.NombreInquilino;

                    if (unidad.ArchivoInquilino != null && unidad.ArchivoInquilino.Length > 0)
                    {
                        var nombreArchivoUnico = $"Contrato_{Guid.NewGuid()}{Path.GetExtension(unidad.ArchivoInquilino.FileName)}";
                        var carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contratos");

                        if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                        var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivoUnico);
                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            await unidad.ArchivoInquilino.CopyToAsync(stream);
                        }

                        if (!string.IsNullOrEmpty(unidadEnDb.ArchivoContrato))
                        {
                            var rutaArchivoViejo = Path.Combine(carpetaDestino, unidadEnDb.ArchivoContrato);
                            if (System.IO.File.Exists(rutaArchivoViejo)) System.IO.File.Delete(rutaArchivoViejo);
                        }

                        unidadEnDb.ArchivoContrato = nombreArchivoUnico;
                    }
                }
                else
                {
                    unidadEnDb.NombreInquilino = null;
                    unidadEnDb.ArchivoContrato = null;
                }

                _context.Update(unidadEnDb);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ocurrió un error al guardar los cambios.");
                return View(unidad);
            }
        }

        // ELIMINAR (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null) return RedirectToAction("Login", "Account");

            var unidad = await _context.UnidadesFuncionales
                .Include(u => u.Consorcio)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidad == null) return NotFound();
            if (unidad.Consorcio?.EmpresaId != empresaId.Value) return Forbid();

            int consorcioIdAfectado = unidad.ConsorcioId;

            if (!string.IsNullOrEmpty(unidad.ArchivoContrato))
            {
                var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contratos", unidad.ArchivoContrato);
                if (System.IO.File.Exists(rutaArchivo)) System.IO.File.Delete(rutaArchivo);
            }

            _context.UnidadesFuncionales.Remove(unidad);
            await _context.SaveChangesAsync();

            await RecalcularCoeficientesConsorcioAsync(consorcioIdAfectado);
            return RedirectToAction(nameof(Index));
        }

        // MOTOR DE REDISTRIBUCIÓN AUTOMÁTICA
        private async Task RecalcularCoeficientesConsorcioAsync(int consorcioId)
        {
            var unidades = await _context.UnidadesFuncionales
                .Where(u => u.ConsorcioId == consorcioId)
                .ToListAsync();

            if (!unidades.Any()) return;

            int totalAmbientesEdificio = unidades.Select(u => (int)u.Ambientes).Sum();
            if (totalAmbientesEdificio == 0) return;

            foreach (var u in unidades)
            {
                int ambientesUnidad = (int)u.Ambientes;
                decimal porcentajeCalculado = ((decimal)ambientesUnidad / (decimal)totalAmbientesEdificio) * 100m;
                u.Coeficiente = Math.Round(porcentajeCalculado, 2);
            }

            decimal sumaVerificacion = unidades.Sum(u => u.Coeficiente);
            decimal diferencia = 100m - sumaVerificacion;

            if (diferencia != 0m && unidades.Any())
            {
                unidades.First().Coeficiente += diferencia;
            }

            await _context.SaveChangesAsync();
        }

        // GET: UnidadFuncional/CargarPago/5
        public async Task<IActionResult> CargarPago(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null) return RedirectToAction("Login", "Account");

            var unidad = await _context.UnidadesFuncionales
                .Include(u => u.Consorcio)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidad == null) return NotFound();

            // Validación de seguridad estricta para la carga de comprobantes
            if (unidad.Consorcio?.EmpresaId != empresaId.Value) return Forbid();

            ViewBag.UnidadInfo = $"{unidad.PisoDepto} - Propietario: {unidad.NombrePropietario}";

            var nuevoPago = new ComprobantePago { UnidadFuncionalId = id };
            return View(nuevoPago);
        }

        public ApplicationDbContext Get_context1()
        {
            return _context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CargarPago(ComprobantePago pago)
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null) return RedirectToAction("Login", "Account");

            // Validar propiedad de la unidad antes de guardar el pago
            var unidad = await _context.UnidadesFuncionales
                .Include(u => u.Consorcio)
                .FirstOrDefaultAsync(u => u.Id == pago.UnidadFuncionalId);

            if (unidad == null) return NotFound();
            if (unidad.Consorcio?.EmpresaId != empresaId.Value) return Forbid();

            pago.Id = 0;

            if (!ModelState.IsValid)
            {
                ViewBag.UnidadInfo = $"{unidad.PisoDepto} - Propietario: {unidad.NombrePropietario}";
                return View(pago);
            }

            var carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes_mensuales");
            if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

            // 1. Procesar Comprobante de Alquiler
            if (pago.InputArchivoAlquiler != null && pago.InputArchivoAlquiler.Length > 0)
            {
                var nombreUnico = $"Alquiler_{pago.Periodo}_{Guid.NewGuid()}{Path.GetExtension(pago.InputArchivoAlquiler.FileName)}";
                using (var stream = new FileStream(Path.Combine(carpetaDestino, nombreUnico), FileMode.Create))
                {
                    await pago.InputArchivoAlquiler.CopyToAsync(stream);
                }
                pago.ArchivoAlquiler = nombreUnico;
            }

            // 2. Procesar Comprobante de Expensas
            if (pago.InputArchivoExpensas != null && pago.InputArchivoExpensas.Length > 0)
            {
                var nombreUnico = $"Expensas_{pago.Periodo}_{Guid.NewGuid()}{Path.GetExtension(pago.InputArchivoExpensas.FileName)}";
                using (var stream = new FileStream(Path.Combine(carpetaDestino, nombreUnico), FileMode.Create))
                {
                    await pago.InputArchivoExpensas.CopyToAsync(stream);
                }
                pago.ArchivoExpensas = nombreUnico;
            }

            _context.ComprobantesPagos.Add(pago);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> VerPagos(int id)
        {
            var empresaId = GetEmpresaId();
            if (empresaId is null) return RedirectToAction("Login", "Account");

            // Buscamos la unidad cargando su lista de pagos e incluyendo el consorcio
            var unidad = await _context.UnidadesFuncionales
                .Include(u => u.Consorcio)
                .Include(u => u.ComprobantesPagos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unidad == null) return NotFound();

            // Seguridad estricta anti-hackeos de URL
            if (unidad.Consorcio?.EmpresaId != empresaId.Value) return Forbid();

            return View(unidad);
        }
    } 
} 