/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Net.Data;

namespace Bam.Net.CoreServices.Data.Daos
{
    public class ConfigurationCollection: DaoCollection<ConfigurationColumns, Configuration>
    { 
		public ConfigurationCollection(){}
		public ConfigurationCollection(Database db, DataTable table, Dao dao = null, string rc = null) : base(db, table, dao, rc) { }
		public ConfigurationCollection(DataTable table, Dao dao = null, string rc = null) : base(table, dao, rc) { }
		public ConfigurationCollection(Query<ConfigurationColumns, Configuration> q, Dao dao = null, string rc = null) : base(q, dao, rc) { }
		public ConfigurationCollection(Database db, Query<ConfigurationColumns, Configuration> q, bool load) : base(db, q, load) { }
		public ConfigurationCollection(Query<ConfigurationColumns, Configuration> q, bool load) : base(q, load) { }
    }
}