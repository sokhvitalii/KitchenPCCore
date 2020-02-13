using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Text;
using FluentNHibernate.Cfg.Db;
using KitchenPC;
using KitchenPC.Context;
using KitchenPC.DB;
using KitchenPC.DB.Search;
using Microsoft.AspNetCore.Mvc;
using Parser.Parser;
using System.Text.Json;
using System.Threading.Tasks;
using KitchenPC.Context.Fluent;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using WebApplication2.Model;

namespace WebApplication2.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class FooController: ControllerBase
    {
        [HttpGet]
        public JsonResult Get()
        {
            var res = new ParseJson();
            Console.WriteLine("2222222222222222222222 = " + new JsonResult(res.RecipesFromJson));
            return new JsonResult(res.RecipesFromJson);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ShoppingListController: ControllerBase
    {

        private string GetToken(IHeaderDictionary headers)
        {
            var token = headers.SingleOrDefault(x => x.Key == "Authorization").Value.ToString();
            return token.Substring("Bearer ".Length);
        }

        private string GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token).Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
        }

        [HttpPost]
        public JsonResult Post(ShoppingListEvent request)
        {
            /*
            var defaultToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImU1YTkxZDlmMzlmYTRkZTI1NGExZTg5ZGYwMGYwNWI3ZTI0OGI5ODUiLCJ0eXAiOiJKV1QifQ.eyJuYW1lIjoi0JLQsNC70LXRgNC40Y8g0JzQsNC60LDRgNC10L3QutC-IiwicGljdHVyZSI6Imh0dHBzOi8vZ3JhcGguZmFjZWJvb2suY29tLzI5MzUyMTI4MDk4NDY4ODgvcGljdHVyZSIsImh0dHBzOi8vaGFzdXJhLmlvL2p3dC9jbGFpbXMiOnsieC1oYXN1cmEtZGVmYXVsdC1yb2xlIjoiYW5vbnltb3VzIiwieC1oYXN1cmEtYWxsb3dlZC1yb2xlcyI6WyJ1c2VyIiwiYW5vbnltb3VzIl0sIngtaGFzdXJhLW9yZy1pZCI6IjEyMyIsIngtaGFzdXJhLWN1c3RvbSI6ImN1c3RvbS12YWx1ZSIsIngtaGFzdXJhLXVzZXItaWQiOiJTME1iRDhoa2o5ZFFhcTVQYlFneW5IUjBHdzAyIn0sImlzcyI6Imh0dHBzOi8vc2VjdXJldG9rZW4uZ29vZ2xlLmNvbS9mZ2wyLTYzZGQzIiwiYXVkIjoiZmdsMi02M2RkMyIsImF1dGhfdGltZSI6MTU3OTY4MTM5NSwidXNlcl9pZCI6IlMwTWJEOGhrajlkUWFxNVBiUWd5bkhSMEd3MDIiLCJzdWIiOiJTME1iRDhoa2o5ZFFhcTVQYlFneW5IUjBHdzAyIiwiaWF0IjoxNTc5NjgxMzk1LCJleHAiOjE1Nzk2ODQ5OTUsImVtYWlsIjoicGFuYWNlYUB0ZS5uZXQudWEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImZpcmViYXNlIjp7ImlkZW50aXRpZXMiOnsiZmFjZWJvb2suY29tIjpbIjI5MzUyMTI4MDk4NDY4ODgiXSwiZW1haWwiOlsicGFuYWNlYUB0ZS5uZXQudWEiXX0sInNpZ25faW5fcHJvdmlkZXIiOiJmYWNlYm9vay5jb20ifX0.AVXxUJT6muVrmQNX7maG69pWOb2je-UMFqfYqauKCPkex3qFhdJ0YHDtGsQ_qA5VnOMwBmK2j9yeF5DFpQqZVsHtu12cwygJEy--Df7RuUYRz2TL50EBS2pij8qUUL0TTUJnLrDUQfP466MG0hdvj7wLvy6y8FbTFzBIinXdoVI5DUuF3imueGm_SVCCs3-a58PtUH-zKEjlbLVjOyLU3r_Q9DNitDKrUucXJGbhC-NMlPW75ANH9brS0oP147el_Lb-9hBHzU3flanr6iWMIC_Fi5WzlOiP7-SuUc-u3Mp5iSPukS-EBvJLXN_HUi2UJx7ri1bB3HKlp4waImx5tg";
            var token = GetToken(Request.Headers);
            var userId = GetUserIdFromToken(token);
            */


            /*
            var connection = new DataBaseConnection(new AuthIdentity(request.Event.SessionVariables.HasuraUserId, ""));


            var ee = connection.Context.Context.ShoppingLists.Load(ShoppingList.FromId(request.Event.Data.New.Recipeid)).List();

            var recipes = connection.Context.Context.Recipes
                .Load(Recipe.FromId(request.Event.Data.New.Recipeid)) // Weeknight Cheese Quiche
                .WithMethod
                .WithUserRating
                .List();
                
                FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

            var foo = new ShoppingListAdder
            {
                Recipes = recipes,
                Ingredients = recipes.SelectMany(x => x.Ingredients.Select(j => j.Ingredient)).ToList(),
                Usages = recipes.SelectMany(x => x.Ingredients).ToList()
            };

            var created2 = connection.Context.Context.ShoppingLists.Create.AddItems(foo).Commit();
            
           
            var cc = connection.Context.Context.Modeler;
            */
            
            
            Console.WriteLine("request = " + new JsonResult(request));
            return new JsonResult(request);
        }
    }
}