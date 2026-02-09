namespace Analitik_MVC.DTOs
{
    public class ProductPreviewDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public List<string> OrderNumbers { get; set; } = new List<string>();
        public List<decimal> OrderTotals { get; set; } = new List<decimal>();
        public int Stock { get; set; }
    }

}
