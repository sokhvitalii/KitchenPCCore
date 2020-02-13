using System;

namespace KitchenPC.Data.DTO
{
   public class Favorites
   {
      public Guid FavoriteId { get; set; }
      public string UserId { get; set; }
      public Guid RecipeId { get; set; }
      public Guid? MenuId { get; set; }
   }
}