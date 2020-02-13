using System;
using KitchenPC.ShoppingLists;

namespace KitchenPC.Data.DTO
{
   public class ShoppingLists
   {
      public Guid ShoppingListId { get; set; }
      public string UserId { get; set; }
      public String Title { get; set; }
      public int PlanId { get; set; }

      public static ShoppingList ToShoppingList(ShoppingLists dtoList)
      {
         return new ShoppingList
         {
            Id = dtoList.ShoppingListId,
            PlanId = dtoList.PlanId,
            Title = dtoList.Title
         };
      }
   }
}