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
using Bam.Net.Data.Dynamic.Data.Dao.Repository;
using Bam.Net.Data.Dynamic.Data;

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

        public class TestDynamicRepository: DynamicRepository
        {
            public TestDynamicRepository(DynamicTypeDataRepository descriptorRepository, DataSettings settings) : base(descriptorRepository, settings)
            {
            }

            public void TestSaveTypDescriptor(string typeName, Dictionary<object, object> data)
            {
                SaveTypeDescriptor(typeName, data);
            }
            public void TestSaveData(string sha1, string typeName, Dictionary<object, object> data)
            {
                SaveRootData(sha1, typeName, data);
            }
        }

        // save descriptor
        [UnitTest]
        public void SaveDescriptorDoesntDuplicte()
        {
            TestDynamicRepository testRepo = new TestDynamicRepository(new DynamicTypeDataRepository(), DataSettings.Default);            
            JObject jobj = (JObject)JsonConvert.DeserializeObject(new { Name = "some name", Arr = new object[] { new { Fromage = "gooey" } } }.ToJson());
            Dictionary<object, object> data = jobj.ToObject<Dictionary<object, object>>();
            string testTypeName = "test_typeName";
            testRepo.DynamicTypeDataRepository.DeleteWhere<DynamicTypeDescriptor>(new { TypeName = testTypeName });
            DynamicTypeDescriptor descriptor = testRepo.DynamicTypeDataRepository.DynamicTypeDescriptorsWhere(d => d.TypeName == testTypeName).FirstOrDefault();
            Expect.IsNull(descriptor);

            testRepo.TestSaveTypDescriptor(testTypeName, data);
            int count = testRepo.DynamicTypeDataRepository.DynamicTypeDescriptorsWhere(d => d.TypeName == testTypeName).Count();

            Expect.IsTrue(count == 1);
            testRepo.TestSaveTypDescriptor(testTypeName, data);
            count = testRepo.DynamicTypeDataRepository.DynamicTypeDescriptorsWhere(d => d.TypeName == testTypeName).Count();
            Expect.IsTrue(count == 1);
        }
        // save child descriptors
        [UnitTest]
        public void SaveDataSavesTypes()
        {
            string testTypeName = nameof(SaveDataSavesTypes).RandomLetters(5);
            SetupTestData(testTypeName, out TestDynamicRepository testRepo, out Dictionary<object, object> data);

            // Test
            testRepo.TestSaveData("sha1NotUsedForThisTest", testTypeName, data);
            List<DynamicTypeDescriptor> descriptors = testRepo.DynamicTypeDataRepository.DynamicTypeDescriptorsWhere(d => d.Id > 0).ToList();
            Expect.IsTrue(descriptors.Count == 3);
            descriptors.Each(d => OutLineFormat("{0}: {1}", d.Id, d.TypeName, ConsoleColor.Cyan));
        }

        // save data
        [UnitTest]
        public void SaveDataSavesInstance()
        {
            string testTypeName = nameof(SaveDataSavesInstance).RandomLetters(5);
            SetupTestData(testTypeName, out TestDynamicRepository testRepo, out Dictionary<object, object> data);

            testRepo.TestSaveData("sha1NotUsedForThisTest", testTypeName, data);
            List<DataInstance> datas = testRepo.DynamicTypeDataRepository.DataInstancesWhere(d => d.TypeName == testTypeName).ToList();
            Expect.IsTrue(datas.Count == 1);
            Expect.IsTrue(datas[0].TypeName.Equals(testTypeName));
            datas[0].Properties.Each(p => OutLine($"{p.PropertyName} = {p.Value}"));
        }
        // save child data

        private static void SetupTestData(string typeName, out TestDynamicRepository repo, out Dictionary<object, object> data)
        {
            // setup test data
            TestDynamicRepository testRepo = new TestDynamicRepository(new DynamicTypeDataRepository(), DataSettings.Default);
            JObject jobj = (JObject)JsonConvert.DeserializeObject(new
            {
                Name = "some name",
                ChildObjectProp = new
                {
                    Name = "child name",
                    ChildProp = "only child has this"
                },
                ChildArrProp = new object[] 
                {
                    new
                    {
                        Fromage = "gooey"
                    }
                }
            }.ToJson());

            data = jobj.ToObject<Dictionary<object, object>>();

            // clear existing entries if any
            testRepo.DynamicTypeDataRepository.DynamicTypeDescriptorsWhere(d => d.Id > 0).Each(d => testRepo.DynamicTypeDataRepository.Delete(d));
            testRepo.DynamicTypeDataRepository.DataInstancesWhere(d => d.Id > 0).Each(d => testRepo.DynamicTypeDataRepository.Delete(d));
            
            // make sure the type isn't in the repo 
            DynamicTypeDescriptor descriptor = testRepo.DynamicTypeDataRepository.DynamicTypeDescriptorsWhere(d => d.TypeName == typeName).FirstOrDefault();
            Expect.IsNull(descriptor);

            repo = testRepo;
        }
    }
}
