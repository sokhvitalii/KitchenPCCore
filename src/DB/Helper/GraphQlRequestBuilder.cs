
using System;
using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.DB.Helper
{
    public class MutationBuilderContext
    {
        private string tableName;
        private Dictionary<string, object> objectsInput;
        private List<string> returning;

        private void CreateRoot()
        {
            objectsInput = new Dictionary<string, object>();
            returning = new List<string>();
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
        
        string InputProcess(KeyValuePair<string, object> pair) => pair.Value is string ? pair.Key + ": \\\"" + pair.Value + "\\\"" : pair.Key + ": " + pair.Value;
        
        public string Result()
        {
            var ret = String.Join(" ", returning);
            var head = $"{{\"query\":\"mutation MyMutation {{ {tableName}(objects: {{ ";
            var last = $" }}) {{ returning {{  {ret} }}}}}}\", \"operationName\":\"MyMutation\"}}";
            
            var inputString = objectsInput.Select(InputProcess);
            var input = String.Join(" ", inputString);
            
            return head + input + last;
        }
    }
    
    
    public class ConditionType
    {
        public object Value { get; set; }
        public string Key { get; set; }
        public string Label;
        public string labelCondition;
        private static List<string> lables;

        static ConditionType()
        {
            lables = new List<string>
            {
                "eq", "like"
            };
        }
        
        public ConditionType(string key, object value, string label)
        {
            if (lables.Contains(label))
            {
                Value = value;
                Key = key;
                labelCondition = $"_{label}";
                Label = label;
            }
        }
    }

    public class QueryBuilderContext
    {
        private string tableName;
        private ConditionType objectsInput;
        private List<string> returning;

        private void CreateRoot()
        {
            returning = new List<string>();
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
                objectsInput = value;
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
        
        string InputProcess(ConditionType pair) => 
            pair.Value is string ? pair.Key + $": {{ {pair.labelCondition}: \\\"" + pair.Value + "\\\"" : pair.Key + $": {{ {pair.labelCondition}" + pair.Value;
        
        public string Result()
        {
            var ret = String.Join(" ", returning);
            var condition = "";
            if ( objectsInput?.labelCondition != null)
            {
                condition = "(where: {" + InputProcess(objectsInput)  + "}})";
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
