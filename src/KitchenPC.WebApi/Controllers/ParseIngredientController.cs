using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using KitchenPC.NLP;
using KitchenPC.WebApi.Common;
using KitchenPC.WebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace KitchenPC.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParseIngredientController: ControllerBase
    {
        
        [HttpPost]
        public IActionResult Post(ParseIngredients request)
        {

            var ctx = new DataBaseConnection(new AuthIdentity("systemUser", "")).Context.Context;
            var aSplit = request.Request.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            var list = new List<ParseIngredientsResponse>();

            foreach (var ingredient in aSplit)
            {
                try
                {
                    var result = ctx.Parser.Parse(ingredient);
                    if (result.Status == MatchResult.Match || result.Status == MatchResult.PartialMatch ||
                        result.Status == MatchResult.IncompatibleForm)
                    {
                        var res = new ParseIngredientsResponse();
                        res.Name = result.Usage.Ingredient.Name;
                        res.PossibleUnits.Add(result.Usage.Amount.Unit.ToString());
                        res.Quantity = result.Usage.Amount.SizeHigh;

                        list.Add(res); 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return list.Any()? (IActionResult) Ok(JsonSerializer.Serialize(list, JsonHelper.Options)): NoContent();
        }
    }
}