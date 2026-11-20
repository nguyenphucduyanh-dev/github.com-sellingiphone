namespace PhoneStore.WinForms.Models
{
   public class ProductItem
   {
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
      public string Brand { get; set; } = string.Empty;
      public decimal Price { get; set; }
      public int Stock { get; set; }
   }
}
