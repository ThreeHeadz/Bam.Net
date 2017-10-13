using Bam.Net.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Data.Dynamic.Data
{
    [Serializable]
    public class DynamicTypePropertyDescriptor: RepoData
    {
        public long DynamicTypeId { get; set; }
        public virtual DynamicTypeDescriptor DynamicType { get; set; }
        public string PropertyName { get; set; }

        public override int GetHashCode()
        {
            return $"{DynamicTypeId}:{PropertyName}".ToSha1Int();
        }

        public override bool Equals(object obj)
        {
            if(obj is DynamicTypePropertyDescriptor d)
            {
                return d.DynamicTypeId == DynamicTypeId && d.PropertyName.Equals(PropertyName);
            }
            return false;
        }
    }
}
