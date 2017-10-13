using System;
using System.Collections.Generic;
using System.Text;
using Bam.Net.Data;

namespace Bam.Net.Data.Dynamic.Data.Dao
{
    public class DataInstancePropertyValueColumns: QueryFilter<DataInstancePropertyValueColumns>, IFilterToken
    {
        public DataInstancePropertyValueColumns() { }
        public DataInstancePropertyValueColumns(string columnName)
            : base(columnName)
        { }
		
		public DataInstancePropertyValueColumns KeyColumn
		{
			get
			{
				return new DataInstancePropertyValueColumns("Id");
			}
		}	

				
        public DataInstancePropertyValueColumns Id
        {
            get
            {
                return new DataInstancePropertyValueColumns("Id");
            }
        }
        public DataInstancePropertyValueColumns Uuid
        {
            get
            {
                return new DataInstancePropertyValueColumns("Uuid");
            }
        }
        public DataInstancePropertyValueColumns Cuid
        {
            get
            {
                return new DataInstancePropertyValueColumns("Cuid");
            }
        }
        public DataInstancePropertyValueColumns PropertyName
        {
            get
            {
                return new DataInstancePropertyValueColumns("PropertyName");
            }
        }

        public DataInstancePropertyValueColumns DataInstanceId
        {
            get
            {
                return new DataInstancePropertyValueColumns("DataInstanceId");
            }
        }

		protected internal Type TableType
		{
			get
			{
				return typeof(DataInstancePropertyValue);
			}
		}

		public string Operator { get; set; }

        public override string ToString()
        {
            return base.ColumnName;
        }
	}
}