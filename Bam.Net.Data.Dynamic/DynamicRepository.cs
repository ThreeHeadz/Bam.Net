using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net.Data.Repositories;
using System.Collections;
using System.IO;
using Bam.Net.Data.Dynamic.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace Bam.Net.Data.Dynamic
{
    public class DynamicRepository : ICrudProvider
    {        
        public DynamicRepository(Repository descriptorRepository)
        {
            FileProcessor = new BackgroundThreadQueue<FileInfo>()
            {
                Process = (fi) =>
                {

                }
            };
        }
        public Repository Repository { get; set; }
        public DirectoryInfo JsonDirectory { get; set; }
        public BackgroundThreadQueue<FileInfo> FileProcessor { get; }
        public void SaveJson(string json)
        {
            string filePath = Path.Combine(JsonDirectory.FullName, $"{json.Sha1()}.json").GetNextFileName();
            json.SafeWriteToFile(filePath);
            FileProcessor.Enqueue(new FileInfo(filePath));
        }

        protected void ProcessJsonFile(string typeName, FileInfo jsonFile)
        {
            // read the json
            string json = jsonFile.ReadAllText();
            string dataSha1 = json.Sha1();
            JObject jobj = (JObject)JsonConvert.DeserializeObject(json);
            Dictionary<object, object> valueDictionary = jobj.ToObject<Dictionary<object, object>>();
            // 1. save parent descriptor
            SaveTypeDescriptor(typeName, valueDictionary);
            // 2. save data
            SaveDataInstance(typeName, dataSha1, valueDictionary);
            
            foreach (object key in valueDictionary.Keys)
            {
                object value = valueDictionary[key];
                if(value != null)
                {
                    Type childType = value.GetType();
                    // 3. for each property where the type is JObject
                    //      - repeat from 1
                    if (childType == typeof(JObject))
                    {
                        string childTypeName = $"{typeName}_{key}";
                        Dictionary<object, object> childValueDictionary = ((JObject)value).ToObject<Dictionary<object, object>>();
                        SaveTypeDescriptor(childTypeName, childValueDictionary);
                        SaveDataInstance(childTypeName, dataSha1, childValueDictionary);
                    }
                    // 4. for each property where the type is JArray
                    //      foreach object in jarray
                    //          - repeat from 1
                    else if(childType == typeof(JArray))
                    {
                        // test how best to access values of JArray
                        throw new NotImplementedException();
                    }
                }
            }
        }

        /// <summary>
        /// Save the primitive property types in the specified
        /// JObject as the specified typeName
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="valueDictionary"></param>
        /// <returns></returns>
        protected DynamicTypeDescriptor SaveTypeDescriptor(string typeName, Dictionary<object, object> valueDictionary)
        {
            DynamicTypeDescriptor descriptor = new DynamicTypeDescriptor()
            {
                TypeName = typeName,
                Properties = new List<DynamicTypePropertyDescriptor>()
            };
            descriptor = Repository.Save(descriptor);
            Type type = valueDictionary.ToDynamicType(typeName, false);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                descriptor.Properties.Add(new DynamicTypePropertyDescriptor { PropertyName = prop.Name });
            }
            return Repository.Save(descriptor);            
        }
        
        protected DataInstance SaveDataInstance(string typeName, string sha1, Dictionary<object, object> valueDictionary)
        {
            DataInstance data = new DataInstance
            {
                TypeName = typeName,
                DataId = sha1,
                Properties = new List<DataInstancePropertyValue>()
            };
            data = Repository.Save(data);
            Type dynamicType = valueDictionary.ToDynamicType(typeName, true);
            foreach(PropertyInfo prop in dynamicType.GetProperties())
            {
                data.Properties.Add(new DataInstancePropertyValue { PropertyName = prop.Name, Value = valueDictionary[prop.Name] });
            }
            return Repository.Save(data);
        }

        public object[] TypesFromYaml(string yaml)
        {
            throw new NotImplementedException();
        }

        public void SaveData(object[] data)
        {
            throw new NotImplementedException();
        }

        public object Create(object toCreate)
        {
            throw new NotImplementedException();
        }

        public bool Delete(object toDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> Query(Type type, Dictionary<string, object> queryParameters)
        {
            throw new NotImplementedException();
        }

        public object Retrieve(Type objectType, string identifier)
        {
            throw new NotImplementedException();
        }

        public object Save(object toSave)
        {
            throw new NotImplementedException();
        }

        public IEnumerable SaveCollection(IEnumerable values)
        {
            throw new NotImplementedException();
        }

        public object Update(object toUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
