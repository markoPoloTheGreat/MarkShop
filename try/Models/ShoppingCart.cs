namespace MarkShop.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int? CustomerId {  get; set; }
        public Customer? Customer { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public bool IsCheckedOut { get; set; } = false;
    }
    public class CartItem
    {
        public int Id { get; set; }
       // public int cartId { get; set; }
        public int ProductId {get; set; }
        public int Quantity { get; set; }
    }
}
