/*
	This file was generated and should not be modified directly
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Net.Data;

namespace Bam.Net.Data.Repositories.Tests.ClrTypes.Daos
{
    public class DaughterPagedQuery: PagedQuery<DaughterColumns, Daughter>
    { 
		public DaughterPagedQuery(DaughterColumns orderByColumn, DaughterQuery query, Database db = null) : base(orderByColumn, query, db) { }
    }
}