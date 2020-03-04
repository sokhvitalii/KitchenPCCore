using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Context.Fluent;
using KitchenPC.Modeler;
using KitchenPC.ShoppingLists;
using KitchenPC.WebApi.Model;
using Microsoft.AspNetCore.Mvc;
using IngredientUsage = KitchenPC.Ingredients.IngredientUsage;

namespace KitchenPC.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecommendationEngineController : ControllerBase
    {
        
        private List<PantryItem> SetItemToRemove(IEnumerator<ShoppingListItem> query)
        {
            var ingredientUsages = new List<PantryItem>();
            while (query.MoveNext())
            {
                ShoppingListItem item = query.Current;
                var usage = new IngredientUsage(item.Ingredient, null, null, null);
                ingredientUsages.Add(new PantryItem(usage));
            }

            return ingredientUsages;
        }
        
        
        [HttpPost]
        public IActionResult Post(string request)
        {
            
            var context = new DataBaseConnection(new AuthIdentity("Vetal", "")).Context.Context;
            var sList = context.ShoppingLists.LoadAll.WithItems.List();
            var pantryItems = sList.SelectMany(x => SetItemToRemove(x.GetEnumerator())).ToArray();
            
            var userProfile = context.GetUserProfiles(new AuthIdentity("Vetal", ""));
            userProfile.Pantry = pantryItems;
            
            
            var ee = context.Modeler.WithProfile(userProfile).Generate();


            var yy = ee;
            
            
            
            
            return Ok();
        }
    }
}