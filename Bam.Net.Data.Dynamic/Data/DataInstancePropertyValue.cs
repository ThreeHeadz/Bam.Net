using Bam.Net.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Data.Dynamic.Data
{
    [Serializable]
    public class DataInstancePropertyValue: RepoData
    {
        public long DataInstanceId { get; set; }
        public virtual DataInstance DataInstance { get; set; }
        public string DocumentHash { get; set; }
        public string PropertyName { get; set; }
        public virtual object Value { get; set; }
    }
}
