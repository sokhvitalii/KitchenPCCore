using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KitchenPC.WebApi.Model
{
  
    
    public class DataPlanItems
    {
        [JsonPropertyName("plan_id")]
        public Guid PlanId { get; set; }
        public Guid Id { get; set; }
        public int Servings { get; set; }
        public DateTime? date { get; set; }
       
        public DataPlanItems()
        {
        }
    }

    public class PlanItemsFromGq
    {
        [JsonPropertyName("plan_item")]
        public List<DataPlanItems> PlanItems { get; set; }

        public PlanItemsFromGq()
        {

        }
    }
    
    public class PlanItemsResponseFromGq
    {
        public PlanItemsFromGq Data { get; set; }

        public PlanItemsResponseFromGq()
        {

        }
    }
}