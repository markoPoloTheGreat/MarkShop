namespace MarkShop.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int CostumerId {  get; set; }
        public List<CartItem> Items { get; set; } = new();
        public bool IsCheckedOut { get; set; } = false;
    }
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId {get; set; }
        public int Quantity { get; set; }
    }
}
