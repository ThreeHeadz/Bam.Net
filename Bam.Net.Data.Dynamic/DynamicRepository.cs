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
    public class DynamicRepository : FsRepository
    {
        public DynamicRepository(DynamicTypeDataRepository descriptorRepository, DataSettings settings) : base(settings)
        {
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
        public DynamicTypeDataRepository DynamicTypeDataRepository { get; set; }
        public DirectoryInfo JsonDirectory { get; set; }
        public BackgroundThreadQueue<DataFile> JsonFileProcessor { get; }
        protected override string DataDirectoryName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
                    if (childType == typeof(JObject))
                    {
                        SetDynamicTypePropertyDescriptor(new DynamicTypePropertyDescriptor
                        {
                            DynamicTypeId = descriptor.Id,
                            ParentTypeName = descriptor.TypeName,
                            PropertyName = key.ToString(),
                            PropertyType = childTypeName
                        });
                    }
                    else if (childType == typeof(JArray))
                    {
                        SetDynamicTypePropertyDescriptor(new DynamicTypePropertyDescriptor
                        {
                            DynamicTypeId = descriptor.Id,
                            ParentTypeName = descriptor.TypeName,
                            PropertyName = key.ToString(),
                            PropertyType = $"arrayOf({childTypeName})"
                        });
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
                        foreach (JObject obj in (JArray)value)
                        {
                            SaveJObjectData(childTypeName, rootHash, instanceHash, obj);
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
                                                    Value = value
                                                }
                                            );
                    }
                }
            }

            return DynamicTypeDataRepository.Save(data);
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
            Dictionary<object, object> childValueDictionary = value.ToObject<Dictionary<object, object>>();
            SaveTypeDescriptor(typeName, childValueDictionary);
            SaveDataInstance(rootHash, parentHash, typeName, childValueDictionary);
        }
    }
}
