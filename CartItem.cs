namespace PhoneStore.Web.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        
        // Thuộc tính tính thành tiền cho từng sản phẩm
        public decimal TotalPrice => Price * Quantity;
    }
}
