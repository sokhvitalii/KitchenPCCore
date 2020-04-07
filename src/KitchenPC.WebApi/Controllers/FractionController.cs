using System;
using System.Linq;
using System.Text.Json;
using KitchenPC.WebApi.Common;
using KitchenPC.WebApi.Model;
using KitchenPC.WebApi.Model.error;
using Microsoft.AspNetCore.Mvc;

namespace KitchenPC.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FractionController: ControllerBase
    {

        [HttpPost]
        public IActionResult Post(FractionRequest request)
        {
            var jsonHelper = new JsonHelper();
            try
            {
                var map = Fractions.GetMap();
                var result = map
                    .Where(x => x.Key.StartsWith(request.Fraction))
                    .Select(x => new FractionResponse(x.Key, x.Value))
                    .ToList();
                
                return Ok(JsonSerializer.Serialize(result, jsonHelper.Options));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(JsonSerializer.Serialize(new ResponseError(e.Message),  jsonHelper.Options));
            }
        }
    }
}