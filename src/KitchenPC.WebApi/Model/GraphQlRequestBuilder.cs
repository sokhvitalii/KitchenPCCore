/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenPC.WebApi.Model
{

    class QlContext
    {
        private string tableName;
        private Dictionary<string, object> objectsInput;
        private List<string> returning;
        
        private void CreateRoot()
        {
            objectsInput = new Dictionary<string, object>();
            returning = new List<string>();
        }
        
        public QlContext(bool foo)
        {
            CreateRoot();
        }

        public QlContext Table(string tableName)
        {
            this.tableName = tableName;
            return this;
        }

        public QlContext AppendObject<T>(string key, T value)
        {
            if (value is ValueType || value is string)
            {
                objectsInput.Add(key, value);
            } 
            
            return this;
        }

        public QlContext AppendReturn(string name)
        {
            returning.Add(name);
            return this;
        }

        public QlContext AppendReturns(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                AppendReturn(name);
            }
            return this;
        }

        public string Result()
        {
            var ret = String.Join(" ", returning);
            var head = $"{{\"query\":\"mutation MyMutation {{ {tableName}(objects: {{ ";
            var last = $" }}) {{ returning {{  {ret} }}}}}}\", \"operationName\":\"MyMutation\"}}";
            var inputString = objectsInput
                .Where(x => x.Value is string)
                .Select(x => x.Key + ": \\\"" + x.Value + "\\\"");
            var inputOther = objectsInput
                .Where(x => x.Value is ValueType)
                .Select(x => x.Key + ": " + x.Value);

            var input = String.Join(" ", inputString.Concat(inputOther));
            
            return head + input + last;
        }
    }

    class GraphQlRequestBuilder
    {

        public static QlContext CreateMutable()
        {
            return new QlContext(true);
        }
    }
}
*/
