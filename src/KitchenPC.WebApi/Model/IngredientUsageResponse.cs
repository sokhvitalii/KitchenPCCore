using System;
using KitchenPC.Ingredients;

namespace KitchenPC.WebApi.Model
{
    public class AmountResponse
    {
        public Single? SizeLow { get; }
        public Single SizeHigh { get; }
        public Units Unit { get; }


        public AmountResponse(Amount? amount)
        {
            SizeLow = amount.SizeLow; 
            SizeHigh = amount.SizeHigh; 
            Unit = amount.Unit;
        }

        public AmountResponse()
        {
        }
    }   
    
    public class IngredientFormResponse
    {
        
        public Guid FormId { get; }
        public Guid IngredientId { get; }
        public Units FormUnitType { get; }
        public string FormDisplayName { get; }
        public string FormUnitName { get; }
        public int ConversionMultiplier { get; }
        public AmountResponse FormAmount { get; }

        public IngredientFormResponse(IngredientForm? form)
        {
            FormId = form.FormId; 
            IngredientId = form.IngredientId;
            FormUnitType = form.FormUnitType; 
            FormDisplayName = form.FormDisplayName;
            FormUnitName = form.FormUnitName;
            ConversionMultiplier = form.ConversionMultiplier;
            FormAmount = form.FormAmount != null ? new AmountResponse(form.FormAmount) : new AmountResponse();
        }

        public IngredientFormResponse()
        {
        }
    } 
        
    public class IngredientResponse
    {
        public Guid Id { get; }
        public String Name { get; }
        public UnitType ConversionType { get; }
        public String UnitName { get; }
        public Weight UnitWeight { get; }
        

        public IngredientResponse(Ingredient ing)
        {
            Id = ing.Id;
            Name = ing.Name;
            ConversionType = ing.ConversionType;
            UnitName = ing.UnitName;
            UnitWeight = ing.UnitWeight;
        }

        public IngredientResponse()
        {
        }
    } 
    
    public class IngredientUsageResponse
    {
        public IngredientResponse Ingredient { get; }
        public IngredientFormResponse Form { get; }
        public AmountResponse Amount { get; }
        public string PrepNote { get; }
        public string Section { get; }

        public IngredientUsageResponse(IngredientUsage ingredientUsage)
        {
            Ingredient = ingredientUsage.Ingredient != null?  new IngredientResponse(ingredientUsage.Ingredient) : new IngredientResponse();
            Form = ingredientUsage.Form != null?  new IngredientFormResponse(ingredientUsage.Form) : new IngredientFormResponse();
            Amount = ingredientUsage.Amount != null?  new AmountResponse(ingredientUsage.Amount) : new AmountResponse();
            PrepNote = ingredientUsage.PrepNote;
            Section = ingredientUsage.Section;
        }

        public IngredientUsageResponse()
        {
        }
    }
}