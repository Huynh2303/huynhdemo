using Demo_web_MVC.Models.ViewModel.Category;

namespace Demo_web_MVC.Models.ViewModel.Product
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Brand { get; set; }

        
        public DateTime CreatedAt { get; set; }
        public List<ProductVariantsViewModel>? Variants { get; set; }
        public List<string> imageUrl { get; set; } = new List<string>();
        public List <CategoryViewModel>? Categories { get; set; }
        public List<ProductViewModel> RelatedProducts { get; set; } = new();

    }
}