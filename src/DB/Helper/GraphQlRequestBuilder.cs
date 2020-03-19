
using System;
using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.DB.Helper
{

    public class MutationSingleObject
    {
        public Dictionary<string, object> objectsInput;
        
         
        public MutationSingleObject()
        {
            objectsInput = new Dictionary<string, object>();
        }

        public MutationSingleObject AppendObject<T>(string key, T value)
        {
            if (value is ValueType || value is string)
            {
                objectsInput.Add(key, value);
            }
            
            return this;
        }
    }
    
    public class MutationBuilderContext
    {
        private string tableName;
        private Dictionary<string, object> objectsInput;
        private List<MutationSingleObject> bulkInput;
        private List<string> returning;

        private void CreateRoot()
        {
            objectsInput = new Dictionary<string, object>();
            returning = new List<string>();
            bulkInput = new List<MutationSingleObject>();
        }
        
        public MutationBuilderContext()
        {
            CreateRoot();
        }

        public MutationBuilderContext Table(string tableName)
        {
            this.tableName = tableName;
            return this;
        }

        public MutationBuilderContext AppendObject<T>(string key, T value)
        {
            if (value is ValueType || value is string)
            {
                objectsInput.Add(key, value);
            } 
            
            return this;
        }
        
        public MutationBuilderContext AppendObject(MutationSingleObject value)
        {
            bulkInput.Add(value);
            return this;
        }

        public MutationBuilderContext AppendReturn(string name)
        {
            returning.Add(name);
            return this;
        }

        public MutationBuilderContext AppendReturns(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                AppendReturn(name);
            }
            return this;
        }
        
        string InputProcess(KeyValuePair<string, object> pair)
        {
            var res = pair.Value is string ?
                pair.Key + ": \\\"" + pair.Value + "\\\"" : 
                pair.Key + ": " + pair.Value;

            return res;
        }

        public string Result()
        {
            var ret = String.Join(" ", returning);
            var head = $"{{\"query\":\"mutation MyMutation {{ {tableName}(objects: {{ ";
            var last = $" }}) {{ returning {{  {ret} }}}}}}\", \"operationName\":\"MyMutation\"}}";
            
            var inputString = objectsInput.Select(InputProcess);
            var input = String.Join(" ", inputString);
            
            return head + input + last;
        }
        
        public string BulkResult()
        {
            // id: 10, recipe_id: "b", tag_id: 10
            // objects: [{id: 10, recipe_id: "b", tag_id: 10}, {id: 10, recipe_id: "", tag_id: 10}]
            // bulkInput
            var ret = String.Join(" ", returning);
            var head = $"{{\"query\":\"mutation MyMutation {{ {tableName}(objects: [";
            var last = $" ]) {{ returning {{  {ret} }}}}}}\", \"operationName\":\"MyMutation\"}}";

            var inputString = 
                bulkInput.Select(x => "{" + String.Join(", ", x.objectsInput.Select(InputProcess)) + "}");
            
            var input = String.Join(", ", inputString);
            
            return head + input + last;
        }
    }
    
    
    public class ConditionType
    {
        public object Value { get; set; }
        public string Key { get; set; }
        public string Label;
        
        public ConditionType(string key, object value, string label)
        {
            Value = value;
            Key = key;
            Label = label;
        }
    }

    public class QueryBuilderContext
    {
        private string tableName;
        private List<ConditionType> objectsInput;
        private List<string> returning;

        private void CreateRoot()
        {
            returning = new List<string>();
            objectsInput = new List<ConditionType>();
        }
        
        public QueryBuilderContext()
        {
            CreateRoot();
        }

        public QueryBuilderContext Table(string tableName)
        {
            this.tableName = tableName;
            return this;
        }

        public QueryBuilderContext AppendCondition(ConditionType value)
        {
            if (value.Value is ValueType || value.Value is string)
            {
                objectsInput.Add(value);
            } 
            
            return this;
        }

        public QueryBuilderContext AppendReturn(string name)
        {
            returning.Add(name);
            return this;
        }

        public QueryBuilderContext AppendReturns(IEnumerable<ConditionType> names)
        {
            foreach (var name in names)
            {
                AppendCondition(name);
            }
            return this;
        }
        
        string InputProcess(ConditionType pair)
        {
            var prepare = 
                pair.Value is string ? 
                    pair.Key + $": {{ {pair.Label}: \\\"" + pair.Value + "\\\"" : 
                    pair.Key + $": {{ {pair.Label}" + pair.Value;
            return "{" + prepare + "}}";
        }

        public string SingleResult()
        {
            var ret = String.Join(" ", returning);
            var condition = "";
            if (objectsInput.Any())
            {
                condition = "(where:" + InputProcess(objectsInput.First()) + ")";
            }
            return $"{{\"query\":\"query MyQuery {{ {tableName}{condition} {{  {ret} }}}}\", \"operationName\":\"MyQuery\"}}";
        }
        
            
        public string BulkResult(string conditionType)
        {
            var ret = String.Join(" ", returning);
            var condition = "";
            if (objectsInput.Any())
            {
                condition = $"(where: {{ {conditionType}: [" +  String.Join(", ", objectsInput.Select(InputProcess))  + "]})";
            }
            return $"{{\"query\":\"query MyQuery {{ {tableName}{condition} {{  {ret} }}}}\", \"operationName\":\"MyQuery\"}}";
        }
    }


    public class GraphQlRequestBuilder
    {
        public static MutationBuilderContext CreateMutation()
        {
            return new MutationBuilderContext();
        }
        
        
        public static QueryBuilderContext CreateQuery()
        {
            return new QueryBuilderContext();
        }
    }
}
