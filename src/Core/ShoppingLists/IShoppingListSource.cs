using KitchenPC.Recipes;

namespace KitchenPC.ShoppingLists
{
   public interface IShoppingListSource
   {
      ShoppingListItem GetItem();
   }
}