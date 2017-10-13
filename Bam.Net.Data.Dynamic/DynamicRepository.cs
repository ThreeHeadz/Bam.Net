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
    public class DynamicRepository : FsRepository
    {
        public DynamicRepository(DaoRepository descriptorRepository, DataSettings settings) : base(settings)
        {
            FileProcessor = new BackgroundThreadQueue<DataFile>()
            {
                Process = (df) =>
                {
                    ProcessJsonFile(df.TypeName, df.FileInfo);
                }
            };
        }
        public DaoRepository Repository { get; set; }
        public DirectoryInfo JsonDirectory { get; set; }
        public BackgroundThreadQueue<DataFile> FileProcessor { get; }
        protected override string DataDirectoryName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void SaveJson(string typeName, string json)
        {
            string filePath = Path.Combine(JsonDirectory.FullName, $"{json.Sha1()}.json").GetNextFileName();
            json.SafeWriteToFile(filePath);
            FileProcessor.Enqueue(new DataFile { FileInfo = new FileInfo(filePath), TypeName = typeName });
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
                if (value != null)
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
                    else if (childType == typeof(JArray))
                    {
                        foreach (JObject obj in (JArray)value)
                        {

                        }
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
            DynamicTypeDescriptor descriptor = EnsureDescriptor(typeName);

            Type type = valueDictionary.ToDynamicType(typeName, false);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                SetDynamicTypePropertyDescriptor(descriptor.Id, new DynamicTypePropertyDescriptor { PropertyName = prop.Name });                
            }

            return Repository.Retrieve<DynamicTypeDescriptor>(descriptor.Id);
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

        protected override object PerformCreate(Type type, object toCreate)
        {
            throw new NotImplementedException();
        }

        protected override bool PerformDelete(Type type, object toDelete)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> Query<T>(Func<T, bool> query)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> Query(Type type, Func<object, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override object Retrieve(Type objectType, long id)
        {
            throw new NotImplementedException();
        }

        public override object Retrieve(Type objectType, string uuid)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> RetrieveAll(Type type)
        {
            throw new NotImplementedException();
        }

        public override object Update(Type type, object toUpdate)
        {
            throw new NotImplementedException();
        }

        public override bool Delete<T>(T toDelete)
        {
            throw new NotImplementedException();
        }

        object _typeDescriptorLock = new object();
        private DynamicTypeDescriptor EnsureDescriptor(string typeName)
        {
            lock (_typeDescriptorLock)
            {
                DynamicTypeDescriptor descriptor = Repository.Query<DynamicTypeDescriptor>(td => td.TypeName == typeName).FirstOrDefault();
                if (descriptor == null)
                {
                    descriptor = new DynamicTypeDescriptor()
                    {
                        TypeName = typeName
                    };

                    descriptor = Repository.Save(descriptor);
                }
                return descriptor;
            }
        }

        private void SetDynamicTypePropertyDescriptor(long typeId, DynamicTypePropertyDescriptor prop)
        {
            DynamicTypePropertyDescriptor retrieved = Repository.Query<DynamicTypePropertyDescriptor>(
                Filter.Where(nameof(DynamicTypePropertyDescriptor.PropertyName)) == prop.PropertyName &&
                Filter.Where(nameof(DynamicTypePropertyDescriptor.DynamicTypeId)) == typeId
            ).FirstOrDefault();
            if(retrieved == null)
            {
                Repository.Save(new DynamicTypePropertyDescriptor { DynamicTypeId = typeId, PropertyName = prop.PropertyName });
            }
        }
    }
}
