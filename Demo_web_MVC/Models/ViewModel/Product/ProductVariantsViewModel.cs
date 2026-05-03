namespace Demo_web_MVC.Models.ViewModel.Product
{
    public class ProductVariantsViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string? Size { get; set; }

        public string? Color { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }
        public List<string> ImageUrlsVariants { get; set; } = new List<string>();
    }
}