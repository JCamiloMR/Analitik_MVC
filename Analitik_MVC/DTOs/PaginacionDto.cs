namespace Analitik_MVC.DTOs
{
    public class PaginacionDto
    {
        public int PaginaActual { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}
