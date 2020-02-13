using System;

namespace KitchenPC.Modeler
{
   public struct RatingBinding
   {
      public string UserId { get; set; }
      public Guid RecipeId { get; set; }
      public Int16 Rating { get; set; }
   }
}