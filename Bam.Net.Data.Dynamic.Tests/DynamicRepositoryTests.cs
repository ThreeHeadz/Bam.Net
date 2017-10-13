using Bam.Net.Testing;
using Bam.Net.Testing.Unit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bam.Net.Data.Repositories;

namespace Bam.Net.Data.Dynamic.Tests
{

    [Serializable]
    public class DynamicRepositoryTests: CommandLineTestInterface
    {
        [UnitTest]
        public void CanReadUnc()
        {
            JObject jobj = (JObject)JsonConvert.DeserializeObject("\\\\core\\data\\events\\github\\24745fe6efe498f79b3b165be27b1feb69a851d0.json".SafeReadFile());
            Dictionary<object, object> dic = jobj.ToObject<Dictionary<object, object>>();
            List<object> convertKeys = new List<object>();
            foreach(object key in dic.Keys)
            {
                object val = dic[key];
                Type type = null;
                if(val != null)
                {
                    type = val.GetType();
                }
                string typeName = type == null ? "null" : type.Name;
                OutLineFormat("{0}: Type = {1}", key, typeName);
                if(type == typeof(JObject))
                {
                    convertKeys.Add(key);                    
                }
            }

            convertKeys.Each(k => dic[k] = ((JObject)dic[k]).ToObject<Dictionary<object, object>>());
            
            Type dynamicType = dic.ToDynamicType("GitEvent");
            foreach(PropertyInfo prop in dynamicType.GetProperties())
            {
                OutLineFormat("{0}: {1}", ConsoleColor.Cyan, prop.Name, prop.PropertyType.Name);
            }
        }

        // save descriptor
        [UnitTest]
        public void SaveDescriptorTest()
        {
            JObject jobj = (JObject)JsonConvert.DeserializeObject("\\\\core\\data\\events\\github\\24745fe6efe498f79b3b165be27b1feb69a851d0.json".SafeReadFile());
            Dictionary<object, object> dic = jobj.ToObject<Dictionary<object, object>>();

        }
        // save child descriptors
        // save data
        // save child data
    }
}
