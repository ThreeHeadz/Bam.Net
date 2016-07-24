/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using Bam.Net.Incubation;
using Bam.Net;
using Bam.Net.Data;
using Bam.Net.Data;
using Npgsql;

namespace Bam.Net.Data
{
    public class NpgsqlParameterBuilder: ParameterBuilder
    {
        public override DbParameter BuildParameter(string name, object value)
        {
            return new NpgsqlParameter($"@{name}", value);
        }
        public override DbParameter BuildParameter(IParameterInfo c)
        {
            string parameterName = string.Format("@{0}{1}", c.ColumnName, c.Number);
            object value = c.Value;
            if (value is DateTime || value is DateTime?)
            {
                value = new Instant((DateTime)value).ToDateTime();
            }
            return new NpgsqlParameter(parameterName, value);
        }
    }
}
