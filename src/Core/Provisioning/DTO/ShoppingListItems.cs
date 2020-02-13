using System;

namespace KitchenPC.Data.DTO
{
   public class ShoppingListItems
   {
      public Guid ItemId { get; set; }
      public String Raw { get; set; }
      public float? Qty { get; set; }
      public Units? Unit { get; set; }
      public string UserId { get; set; }
      public Guid? IngredientId { get; set; }
      public Guid? RecipeId { get; set; }
      public Guid? ShoppingListId { get; set; }
      public bool CrossedOut { get; set; }
   }
}