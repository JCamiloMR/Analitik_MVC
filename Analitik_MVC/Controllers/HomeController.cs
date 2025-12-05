using Analitik_MVC.Models;
using Analitik_MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
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
        private readonly AnalitikContext _context;

        public HomeController(ILogger<HomeController> logger, AnalitikContext context)
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

                    /***
                     * // Asegúrate de que el campo Id es tipo Guid en el modelo
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("Cookies", principal);
                    **/
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
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            // Validaciones y creación del usuario...
            return Json(new { success = true, name = model.FullName });
        }

    }
}
