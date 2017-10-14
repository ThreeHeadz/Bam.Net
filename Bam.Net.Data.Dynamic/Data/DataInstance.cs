using Bam.Net.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Data.Dynamic.Data
{
    [Serializable]
    public class DataInstance: RepoData
    {
        public string TypeName { get; set; }
        /// <summary>
        /// The Sha1 of the original json or yaml data
        /// </summary>
        public string DocumentHash { get; set; }
        public virtual List<DataInstancePropertyValue> Properties { get; set; }
    }
}
