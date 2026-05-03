namespace Demo_web_MVC.Models.ViewModel.Oder
{
    public class OderViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int  Quatity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.Now;
        public List<OderItemViewModel> Items { get; set; } = new();
    }
}
