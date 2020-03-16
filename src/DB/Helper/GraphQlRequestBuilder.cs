
using System;
using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.DB.Helper
{

    class GraphQlBuilderContext
    {
        private string tableName;
        private Dictionary<string, object> objectsInput;
        private List<string> returning;
        private string nameQuery;
        private string queryParams;
        
        private void CreateRoot()
        {
            objectsInput = new Dictionary<string, object>();
            returning = new List<string>();
        }
        
        public GraphQlBuilderContext(bool foo)
        {
            nameQuery = foo ? "MyMutation" :"MyQuery";
            queryParams = foo ? "mutation" :"query";
            CreateRoot();
        }

        public GraphQlBuilderContext Table(string tableName)
        {
            this.tableName = tableName;
            return this;
        }

        public GraphQlBuilderContext AppendObject<T>(string key, T value)
        {
            if (value is ValueType || value is string)
            {
                objectsInput.Add(key, value);
            } 
            
            return this;
        }

        public GraphQlBuilderContext AppendReturn(string name)
        {
            returning.Add(name);
            return this;
        }

        public GraphQlBuilderContext AppendReturns(IEnumerable<string> names)
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
            var head = $"{{\"query\":\"{queryParams} {nameQuery} {{ {tableName}(objects: {{ ";
            var last = $" }}) {{ returning {{  {ret} }}}}}}\", \"operationName\":\"{nameQuery}\"}}";
            
            var inputString = objectsInput.Select(InputProcess);
            var input = String.Join(" ", inputString);
            
            return head + input + last;
        }
    }

    class GraphQlRequestBuilder
    {
        public static GraphQlBuilderContext CreateQuery(bool isMutation = true)
        {
            return new GraphQlBuilderContext(isMutation);
        }
    }
}
