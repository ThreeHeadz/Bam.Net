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
using Bam.Net.Data.Dynamic.Data.Dao.Repository;

namespace Bam.Net.Data.Dynamic
{
    public class DynamicTypeManager 
    {
        public DynamicTypeManager(DynamicTypeDataRepository descriptorRepository, DataSettings settings) 
        {
            DataSettings = settings;
            JsonDirectory = settings.GetDataDirectory(nameof(DynamicTypeManager));
            if (!JsonDirectory.Exists)
            {
                JsonDirectory.Create();
            }
            descriptorRepository.EnsureDaoAssemblyAndSchema();
            DynamicTypeDataRepository = descriptorRepository;
            JsonFileProcessor = new BackgroundThreadQueue<DataFile>()
            {
                Process = (df) =>
                {
                    ProcessJsonFile(df.TypeName, df.FileInfo);
                }
            };
        }
        public DataSettings DataSettings { get; set; }
        public DynamicTypeDataRepository DynamicTypeDataRepository { get; set; }
        public DirectoryInfo JsonDirectory { get; set; }
        public BackgroundThreadQueue<DataFile> JsonFileProcessor { get; }

        public void SaveJson(string typeName, string json)
        {
            string filePath = Path.Combine(JsonDirectory.FullName, $"{json.Sha1()}.json").GetNextFileName();
            json.SafeWriteToFile(filePath);
            JsonFileProcessor.Enqueue(new DataFile { FileInfo = new FileInfo(filePath), TypeName = typeName });
        }

        protected void ProcessJsonFile(string typeName, FileInfo jsonFile)
        {
            // read the json
            string json = jsonFile.ReadAllText();
            string rootHash = json.Sha1();
            JObject jobj = (JObject)JsonConvert.DeserializeObject(json);
            Dictionary<object, object> valueDictionary = jobj.ToObject<Dictionary<object, object>>();
            SaveRootData(rootHash, typeName, valueDictionary);
        }
 
        protected void SaveRootData(string rootHash, string typeName, Dictionary<object, object> valueDictionary)
        {
            // 1. save parent descriptor
            SaveTypeDescriptor(typeName, valueDictionary);
            // 2. save data
            SaveDataInstance(rootHash, typeName, valueDictionary);            
        }

        /// <summary>
        /// Save a DynamicTypeDescriptor for the specified values
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="valueDictionary"></param>
        /// <returns></returns>
        protected DynamicTypeDescriptor SaveTypeDescriptor(string typeName, Dictionary<object, object> valueDictionary)
        {
            DynamicTypeDescriptor descriptor = EnsureDescriptor(typeName);

            foreach (object key in valueDictionary.Keys)
            {
                object value = valueDictionary[key];
                if (value != null)
                {
                    Type childType = value.GetType();
                    string childTypeName = $"{typeName}.{key}";
                    DynamicTypePropertyDescriptor propertyDescriptor = new DynamicTypePropertyDescriptor
                    {
                        DynamicTypeId = descriptor.Id,
                        ParentTypeName = descriptor.TypeName,
                        PropertyName = key.ToString(),
                    };

                    if (childType == typeof(JObject) || childType == typeof(Dictionary<object, object>))
                    {
                        propertyDescriptor.PropertyType = childTypeName;
                        SetDynamicTypePropertyDescriptor(propertyDescriptor);
                        Dictionary<object, object> data = value as Dictionary<object, object>;
                        if(data is null)
                        {
                            data = ((JObject)value).ToObject<Dictionary<object, object>>();
                        }
                        SaveTypeDescriptor(childTypeName, data);
                    }
                    else if (childType == typeof(JArray) || childType.IsArray)
                    {
                        propertyDescriptor.PropertyType = $"arrayOf({childTypeName})";
                        SetDynamicTypePropertyDescriptor(propertyDescriptor);

                        foreach (object obj in (IEnumerable)value)
                        {
                            Dictionary<object, object> data = new Dictionary<object, object>();
                            if(obj is JObject jobj)
                            {
                                data = jobj.ToObject<Dictionary<object, object>>();
                                SaveTypeDescriptor(childTypeName, data);
                            }
                        }
                    }
                    else
                    {
                        SetDynamicTypePropertyDescriptor(propertyDescriptor);
                    }
                }
            }

            return DynamicTypeDataRepository.Retrieve<DynamicTypeDescriptor>(descriptor.Id);
        }
        protected DataInstance SaveDataInstance(string rootHash, string typeName, Dictionary<object, object> valueDictionary)
        {
            return SaveDataInstance(rootHash, rootHash, typeName, valueDictionary);
        }
        static Dictionary<string, object> _parentLocks = new Dictionary<string, object>();
        protected DataInstance SaveDataInstance(string rootHash, string parentHash, string typeName, Dictionary<object, object> valueDictionary)
        {
            string instanceHash = valueDictionary.ToJson().Sha1();
            DataInstance data = DynamicTypeDataRepository.DataInstancesWhere(di => di.RootHash == rootHash && di.ParentHash == parentHash && di.TypeName == typeName).FirstOrDefault();
            if(data == null)
            {
                _parentLocks.AddMissing(parentHash, new object());
                lock (_parentLocks[parentHash])
                {
                    data = DynamicTypeDataRepository.Save(new DataInstance
                    {
                        TypeName = typeName,
                        RootHash = rootHash,
                        ParentHash = parentHash,
                        Properties = new List<DataInstancePropertyValue>()
                    });
                }
            }
            
            foreach(object key in valueDictionary.Keys)
            {
                object value = valueDictionary[key];
                if (value != null)
                {
                    Type childType = value.GetType();
                    string childTypeName = $"{typeName}.{key}";
                    // 3. for each property where the type is JObject
                    //      - repeat from 1
                    if (childType == typeof(JObject))
                    {
                        SaveJObjectData(childTypeName, rootHash, instanceHash, (JObject)value);
                    }
                    // 4. for each property where the type is JArray
                    //      foreach object in jarray
                    //          - repeat from 1
                    else if (childType == typeof(JArray))
                    {
                        foreach (object obj in (JArray)value)
                        {
                            if(obj is JObject jobj)
                            {
                                SaveJObjectData(childTypeName, rootHash, instanceHash, jobj);
                            }
                            else
                            {
                                data.Properties.Add(new DataInstancePropertyValue
                                {
                                    RootHash = rootHash,
                                    InstanceHash = instanceHash,
                                    ParentTypeName = typeName,
                                    PropertyName = key.ToString(),
                                    Value = obj.ToString()
                                });
                            }                           
                        }
                    }
                    else
                    {
                        data.Properties.Add(new DataInstancePropertyValue
                        {
                            RootHash = rootHash,
                            InstanceHash = instanceHash,
                            ParentTypeName = typeName,
                            PropertyName = key.ToString(),
                            Value = value.ToString()
                        });
                    }
                }
            }

            return DynamicTypeDataRepository.Save(data);
        }

        
        object _typeDescriptorLock = new object();
        private DynamicTypeDescriptor EnsureDescriptor(string typeName)
        {
            lock (_typeDescriptorLock)
            {
                DynamicTypeDescriptor descriptor = DynamicTypeDataRepository.Query<DynamicTypeDescriptor>(td => td.TypeName == typeName).FirstOrDefault();
                if (descriptor == null)
                {
                    descriptor = new DynamicTypeDescriptor()
                    {
                        TypeName = typeName
                    };

                    descriptor = DynamicTypeDataRepository.Save(descriptor);
                }
                return descriptor;
            }
        }

        static Dictionary<int, object> _dynamicTypePropertyLocks = new Dictionary<int, object>();
        private void SetDynamicTypePropertyDescriptor(DynamicTypePropertyDescriptor prop)
        {
            int hashCode = prop.GetHashCode();
            _dynamicTypePropertyLocks.AddMissing(hashCode, new object());
            lock (_dynamicTypePropertyLocks[hashCode])
            {
                DynamicTypePropertyDescriptor retrieved = DynamicTypeDataRepository.DynamicTypePropertyDescriptorsWhere(pd =>
                    pd.DynamicTypeId == prop.DynamicTypeId &&
                    pd.ParentTypeName == prop.ParentTypeName &&
                    pd.PropertyType == prop.PropertyType &&
                    pd.PropertyName == prop.PropertyName).FirstOrDefault();

                if (retrieved == null)
                {
                    DynamicTypeDataRepository.Save(prop);
                }
            }
        }
        private void SaveJObjectData(string typeName, string rootHash, string parentHash, JObject value)
        {
            Dictionary<object, object> valueDictionary = value.ToObject<Dictionary<object, object>>();
            SaveObjectData(typeName, rootHash, parentHash, valueDictionary);
        }

        private void SaveObjectData(string typeName, string rootHash, string parentHash, Dictionary<object, object> valueDictionary)
        {
            SaveTypeDescriptor(typeName, valueDictionary);
            SaveDataInstance(rootHash, parentHash, typeName, valueDictionary);
        }
    }
}
