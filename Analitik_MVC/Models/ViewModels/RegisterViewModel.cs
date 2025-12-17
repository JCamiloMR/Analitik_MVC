namespace Analitik_MVC.Models.ViewModels
{
    public class RegisterViewModel
    {
        // Empresa
        public string NombreComercial { get; set; }
        public string RazonSocial { get; set; }
        public string Nit { get; set; }
        public string Sector { get; set; }
        public string Tamano { get; set; }
        public string Ciudad { get; set; }
        public string DirectorNombreCompleto { get; set; }
        public string DirectorTelefono { get; set; }

        // Cuenta
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Password { get; set; }
    }

}
