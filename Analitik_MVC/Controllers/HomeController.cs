using Analitik_MVC.Data;
using Analitik_MVC.Enums;
using Analitik_MVC.Models;
using Analitik_MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Analitik_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AnalitikDbContext _context;

        public HomeController(ILogger<HomeController> logger, AnalitikDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


            [HttpPost("Login")]
            public async Task<IActionResult> Login([FromBody] LoginViewModel model)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        return Json(new { success = false, error = "Datos inválidos" });
                    }

                    var user = await _context.CuentaEmpresas
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                    if (user == null)
                    {
                        return Json(new { success = false, error = "Credenciales incorrectas" });
                    }

                    if (user.PasswordHash != model.Password)
                    {
                        return Json(new { success = false, error = "Credenciales incorrectas" });
                    }

                    // Crear las claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.EmpresaId.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Rol!)
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("Cookies", principal);
                    return Json(new
                    {
                        success = true,
                        message = "Inicio de sesión exitoso",
                        user = new
                        {
                            id = user.Id,
                            email = user.Email,
                            nombre = user.NombreCompleto
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = "Error inesperado: " + ex.Message });
                }
            }


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Datos inválidos" });
            }

            // Validaciones manuales mínimas
            if (string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.NombreComercial))
            {
                return Json(new { success = false, error = "Campos obligatorios faltantes" });
            }

            // Email único
            var emailExiste = await _context.CuentaEmpresas
                .AnyAsync(c => c.Email.ToLower() == model.Email.ToLower());

            if (emailExiste)
            {
                return Json(new { success = false, error = "El email ya está registrado" });
            }

            // NIT único (si viene)
            if (!string.IsNullOrEmpty(model.Nit))
            {
                var nitExiste = await _context.Empresas
                    .AnyAsync(e => e.Nit == model.Nit);

                if (nitExiste)
                {
                    return Json(new { success = false, error = "El NIT ya está registrado" });
                }
            }

            // 🔐 TRANSACCIÓN
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Crear empresa
                var empresa = new Empresa
                {
                    NombreComercial = model.NombreComercial,
                    RazonSocial = model.RazonSocial,
                    Nit = model.Nit,
                    Sector = model.Sector,
                    Tamano = model.Tamano,
                    Ciudad = model.Ciudad,
                    DirectorNombreCompleto = model.DirectorNombreCompleto,
                    DirectorTelefono = model.DirectorTelefono,
                    Activa = true
                };

                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync(); // aquí ya existe empresa.Id

                // 2️⃣ Crear cuenta empresa
                var cuenta = new CuentaEmpresa
                {
                    EmpresaId = empresa.Id,
                    Email = model.Email,
                    NombreCompleto = model.DirectorNombreCompleto,
                    Telefono = model.DirectorTelefono,
                    PasswordHash = model.Password, // ⚠️ luego lo mejoras
                    Rol = "admin",
                    EsOwner = true,
                    Activa = true,
                    Verificada = false
                };

                _context.CuentaEmpresas.Add(cuenta);
                await _context.SaveChangesAsync();

                // 3️⃣ Commit
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Empresa y cuenta creadas correctamente"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return Json(new
                {
                    success = false,
                    error = $"{ex.Message}, {model.Sector}",
                    detail = ex.Message
                });
            }
        }

        [Authorize] // asegura que User esté lleno
        [HttpGet("Me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                authenticated = User.Identity?.IsAuthenticated ?? false,
                user = new
                {
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    empresaId = User.FindFirstValue(ClaimTypes.Name),
                    email = User.FindFirstValue(ClaimTypes.Email),
                    role = User.FindFirstValue(ClaimTypes.Role)
                }
            });
        }


        [HttpGet("LogOut")]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("Cookies");

            return RedirectToAction("Index", "Home");
        }


    }
}
