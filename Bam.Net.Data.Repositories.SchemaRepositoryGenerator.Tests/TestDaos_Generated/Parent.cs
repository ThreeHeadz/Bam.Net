/*
	This file was generated and should not be modified directly
*/
// Model is Table
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Bam.Net;
using Bam.Net.Data;
using Bam.Net.Data.Qi;

namespace Bam.Net.Data.Repositories.Tests.ClrTypes.Daos
{
	// schema = TestDaoSchema
	// connection Name = TestDaoSchema
	[Serializable]
	[Bam.Net.Data.Table("Parent", "TestDaoSchema")]
	public partial class Parent: Dao
	{
		public Parent():base()
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public Parent(DataRow data)
			: base(data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public Parent(Database db)
			: base(db)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public Parent(Database db, DataRow data)
			: base(db, data)
		{
			this.SetKeyColumnName();
			this.SetChildren();
		}

		public static implicit operator Parent(DataRow data)
		{
			return new Parent(data);
		}

		private void SetChildren()
		{

            this.ChildCollections.Add("Daughter_ParentId", new DaughterCollection(Database.GetQuery<DaughterColumns, Daughter>((c) => c.ParentId == GetLongValue("Id")), this, "ParentId"));	
            this.ChildCollections.Add("Son_ParentId", new SonCollection(Database.GetQuery<SonColumns, Son>((c) => c.ParentId == GetLongValue("Id")), this, "ParentId"));	
            this.ChildCollections.Add("HouseParent_ParentId", new HouseParentCollection(Database.GetQuery<HouseParentColumns, HouseParent>((c) => c.ParentId == GetLongValue("Id")), this, "ParentId"));							
            this.ChildCollections.Add("Parent_HouseParent_House",  new XrefDaoCollection<HouseParent, House>(this, false));
				
		}

	// property:Id, columnName:Id	
	[Bam.Net.Exclude]
	[Bam.Net.Data.KeyColumn(Name="Id", DbDataType="BigInt", MaxLength="19")]
	public long? Id
	{
		get
		{
			return GetLongValue("Id");
		}
		set
		{
			SetValue("Id", value);
		}
	}

	// property:Uuid, columnName:Uuid	
	[Bam.Net.Data.Column(Name="Uuid", DbDataType="VarChar", MaxLength="4000", AllowNull=false)]
	public string Uuid
	{
		get
		{
			return GetStringValue("Uuid");
		}
		set
		{
			SetValue("Uuid", value);
		}
	}

	// property:Name, columnName:Name	
	[Bam.Net.Data.Column(Name="Name", DbDataType="VarChar", MaxLength="4000", AllowNull=true)]
	public string Name
	{
		get
		{
			return GetStringValue("Name");
		}
		set
		{
			SetValue("Name", value);
		}
	}

	// property:Created, columnName:Created	
	[Bam.Net.Data.Column(Name="Created", DbDataType="DateTime", MaxLength="8", AllowNull=true)]
	public DateTime? Created
	{
		get
		{
			return GetDateTimeValue("Created");
		}
		set
		{
			SetValue("Created", value);
		}
	}



				

	[Exclude]	
	public DaughterCollection DaughtersByParentId
	{
		get
		{
			if (this.IsNew)
			{
				throw new InvalidOperationException("The current instance of type({0}) hasn't been saved and will have no child collections, call Save() or Save(Database) first."._Format(this.GetType().Name));
			}

			if(!this.ChildCollections.ContainsKey("Daughter_ParentId"))
			{
				SetChildren();
			}

			var c = (DaughterCollection)this.ChildCollections["Daughter_ParentId"];
			if(!c.Loaded)
			{
				c.Load(Database);
			}
			return c;
		}
	}
	
	[Exclude]	
	public SonCollection SonsByParentId
	{
		get
		{
			if (this.IsNew)
			{
				throw new InvalidOperationException("The current instance of type({0}) hasn't been saved and will have no child collections, call Save() or Save(Database) first."._Format(this.GetType().Name));
			}

			if(!this.ChildCollections.ContainsKey("Son_ParentId"))
			{
				SetChildren();
			}

			var c = (SonCollection)this.ChildCollections["Son_ParentId"];
			if(!c.Loaded)
			{
				c.Load(Database);
			}
			return c;
		}
	}
	
	[Exclude]	
	public HouseParentCollection HouseParentsByParentId
	{
		get
		{
			if (this.IsNew)
			{
				throw new InvalidOperationException("The current instance of type({0}) hasn't been saved and will have no child collections, call Save() or Save(Database) first."._Format(this.GetType().Name));
			}

			if(!this.ChildCollections.ContainsKey("HouseParent_ParentId"))
			{
				SetChildren();
			}

			var c = (HouseParentCollection)this.ChildCollections["HouseParent_ParentId"];
			if(!c.Loaded)
			{
				c.Load(Database);
			}
			return c;
		}
	}
			


		// Xref       
        public XrefDaoCollection<HouseParent, House> Houses
        {
            get
            {			
				if (this.IsNew)
				{
					throw new InvalidOperationException("The current instance of type({0}) hasn't been saved and will have no child collections, call Save() or Save(Database) first."._Format(this.GetType().Name));
				}

				if(!this.ChildCollections.ContainsKey("Parent_HouseParent_House"))
				{
					SetChildren();
				}

				var xref = (XrefDaoCollection<HouseParent, House>)this.ChildCollections["Parent_HouseParent_House"];
				if(!xref.Loaded)
				{
					xref.Load(Database);
				}

				return xref;
            }
        }		/// <summary>
		/// Gets a query filter that should uniquely identify
		/// the current instance.  The default implementation
		/// compares the Id/key field to the current instance's.
		/// </summary> 
		public override IQueryFilter GetUniqueFilter()
		{
			if(UniqueFilterProvider != null)
			{
				return UniqueFilterProvider();
			}
			else
			{
				var colFilter = new ParentColumns();
				return (colFilter.KeyColumn == IdValue);
			}			
		}

		/// <summary>
		/// Return every record in the Parent table.
		/// </summary>
		/// <param name="database">
		/// The database to load from or null
		/// </param>
		public static ParentCollection LoadAll(Database database = null)
		{
			SqlStringBuilder sql = new SqlStringBuilder();
			sql.Select<Parent>();
			Database db = database ?? Db.For<Parent>();
			var results = new ParentCollection(sql.GetDataTable(db));
			results.Database = db;
			return results;
		}

		public static async Task BatchAll(int batchSize, Action<IEnumerable<Parent>> batchProcessor, Database database = null)
		{
			await Task.Run(async ()=>
			{
				ParentColumns columns = new ParentColumns();
				var orderBy = Order.By<ParentColumns>(c => c.KeyColumn, SortOrder.Ascending);
				var results = Top(batchSize, (c) => c.KeyColumn > 0, orderBy, database);
				while(results.Count > 0)
				{
					await Task.Run(()=>
					{
						batchProcessor(results);
					});
					long topId = results.Select(d => d.Property<long>(columns.KeyColumn.ToString())).ToArray().Largest();
					results = Top(batchSize, (c) => c.KeyColumn > topId, orderBy, database);
				}
			});			
		}	 

		public static async Task BatchQuery(int batchSize, QueryFilter filter, Action<IEnumerable<Parent>> batchProcessor, Database database = null)
		{
			await BatchQuery(batchSize, (c) => filter, batchProcessor, database);			
		}

		public static async Task BatchQuery(int batchSize, WhereDelegate<ParentColumns> where, Action<IEnumerable<Parent>> batchProcessor, Database database = null)
		{
			await Task.Run(async ()=>
			{
				ParentColumns columns = new ParentColumns();
				var orderBy = Order.By<ParentColumns>(c => c.KeyColumn, SortOrder.Ascending);
				var results = Top(batchSize, where, orderBy, database);
				while(results.Count > 0)
				{
					await Task.Run(()=>
					{ 
						batchProcessor(results);
					});
					long topId = results.Select(d => d.Property<long>(columns.KeyColumn.ToString())).ToArray().Largest();
					results = Top(batchSize, (ParentColumns)where(columns) && columns.KeyColumn > topId, orderBy, database);
				}
			});			
		}

		public static Parent GetById(int id, Database database = null)
		{
			return GetById((long)id, database);
		}

		public static Parent GetById(long id, Database database = null)
		{
			return OneWhere(c => c.KeyColumn == id, database);
		}

		public static Parent GetByUuid(string uuid, Database database = null)
		{
			return OneWhere(c => Bam.Net.Data.Query.Where("Uuid") == uuid, database);
		}

		public static Parent GetByCuid(string cuid, Database database = null)
		{
			return OneWhere(c => Bam.Net.Data.Query.Where("Cuid") == cuid, database);
		}

		public static ParentCollection Query(QueryFilter filter, Database database = null)
		{
			return Where(filter, database);
		}
				
		public static ParentCollection Where(QueryFilter filter, Database database = null)
		{
			WhereDelegate<ParentColumns> whereDelegate = (c) => filter;
			return Where(whereDelegate, database);
		}

		/// <summary>
		/// Execute a query and return the results. 
		/// </summary>
		/// <param name="where">A Func delegate that recieves a ParentColumns 
		/// and returns a QueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="db"></param>
		public static ParentCollection Where(Func<ParentColumns, QueryFilter<ParentColumns>> where, OrderBy<ParentColumns> orderBy = null, Database database = null)
		{
			database = database ?? Db.For<Parent>();
			return new ParentCollection(database.GetQuery<ParentColumns, Parent>(where, orderBy), true);
		}
		
		/// <summary>
		/// Execute a query and return the results. 
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="db"></param>
		public static ParentCollection Where(WhereDelegate<ParentColumns> where, Database database = null)
		{		
			database = database ?? Db.For<Parent>();
			var results = new ParentCollection(database, database.GetQuery<ParentColumns, Parent>(where), true);
			return results;
		}
		   
		/// <summary>
		/// Execute a query and return the results. 
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database"></param>
		public static ParentCollection Where(WhereDelegate<ParentColumns> where, OrderBy<ParentColumns> orderBy = null, Database database = null)
		{		
			database = database ?? Db.For<Parent>();
			var results = new ParentCollection(database, database.GetQuery<ParentColumns, Parent>(where, orderBy), true);
			return results;
		}

		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of 
		/// one of the methods that take a delegate of type
		/// WhereDelegate&lt;ParentColumns&gt;.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static ParentCollection Where(QiQuery where, Database database = null)
		{
			var results = new ParentCollection(database, Select<ParentColumns>.From<Parent>().Where(where, database));
			return results;
		}
				
		/// <summary>
		/// Get one entry matching the specified filter.  If none exists 
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		public static Parent GetOneWhere(QueryFilter where, Database database = null)
		{
			var result = OneWhere(where, database);
			if(result == null)
			{
				result = CreateFromFilter(where, database);
			}

			return result;
		}

		/// <summary>
		/// Execute a query that should return only one result.  If more
		/// than one result is returned a MultipleEntriesFoundException will 
		/// be thrown.  
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static Parent OneWhere(QueryFilter where, Database database = null)
		{
			WhereDelegate<ParentColumns> whereDelegate = (c) => where;
			var result = Top(1, whereDelegate, database);
			return OneOrThrow(result);
		}

		/// <summary>
		/// Get one entry matching the specified filter.  If none exists 
		/// one will be created; success will depend on the nullability
		/// of the specified columns.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static Parent GetOneWhere(WhereDelegate<ParentColumns> where, Database database = null)
		{
			var result = OneWhere(where, database);
			if(result == null)
			{
				ParentColumns c = new ParentColumns();
				IQueryFilter filter = where(c); 
				result = CreateFromFilter(filter, database);
			}

			return result;
		}

		/// <summary>
		/// Execute a query that should return only one result.  If more
		/// than one result is returned a MultipleEntriesFoundException will 
		/// be thrown.  This method is most commonly used to retrieve a
		/// single Parent instance by its Id/Key value
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="database"></param>
		public static Parent OneWhere(WhereDelegate<ParentColumns> where, Database database = null)
		{
			var result = Top(1, where, database);
			return OneOrThrow(result);
		}
					 
		/// <summary>
		/// This method is intended to respond to client side Qi queries.
		/// Use of this method from .Net should be avoided in favor of 
		/// one of the methods that take a delegate of type
		/// WhereDelegate<ParentColumns>.
		/// </summary>
		/// <param name="where"></param>
		/// <param name="database"></param>
		public static Parent OneWhere(QiQuery where, Database database = null)
		{
			var results = Top(1, where, database);
			return OneOrThrow(results);
		}

		/// <summary>
		/// Execute a query and return the first result.  This method will issue a sql TOP clause so only the 
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="database"></param>
		public static Parent FirstOneWhere(WhereDelegate<ParentColumns> where, Database database = null)
		{
			var results = Top(1, where, database);
			if(results.Count > 0)
			{
				return results[0];
			}
			else
			{
				return null;
			}
		}
		
		/// <summary>
		/// Execute a query and return the first result.  This method will issue a sql TOP clause so only the 
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="database"></param>
		public static Parent FirstOneWhere(WhereDelegate<ParentColumns> where, OrderBy<ParentColumns> orderBy, Database database = null)
		{
			var results = Top(1, where, orderBy, database);
			if(results.Count > 0)
			{
				return results[0];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Shortcut for Top(1, where, orderBy, database)
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="database"></param>
		public static Parent FirstOneWhere(QueryFilter where, OrderBy<ParentColumns> orderBy = null, Database database = null)
		{
			WhereDelegate<ParentColumns> whereDelegate = (c) => where;
			var results = Top(1, whereDelegate, orderBy, database);
			if(results.Count > 0)
			{
				return results[0];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Execute a query and return the specified number
		/// of values. This method will issue a sql TOP clause so only the 
		/// specified number of values will be returned.
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this 
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="database"></param>
		public static ParentCollection Top(int count, WhereDelegate<ParentColumns> where, Database database = null)
		{
			return Top(count, where, null, database);
		}

		/// <summary>
		/// Execute a query and return the specified number of values.  This method
		/// will issue a sql TOP clause so only the specified number of values
		/// will be returned.
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this 
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="database"></param>
		public static ParentCollection Top(int count, WhereDelegate<ParentColumns> where, OrderBy<ParentColumns> orderBy, Database database = null)
		{
			ParentColumns c = new ParentColumns();
			IQueryFilter filter = where(c);         
			
			Database db = database ?? Db.For<Parent>();
			QuerySet query = GetQuerySet(db); 
			query.Top<Parent>(count);
			query.Where(filter);

			if(orderBy != null)
			{
				query.OrderBy<ParentColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<ParentCollection>(0);
			results.Database = db;
			return results;
		}

		public static ParentCollection Top(int count, QueryFilter where, Database database)
		{
			return Top(count, where, null, database);
		}
		/// <summary>
		/// Execute a query and return the specified number of values.  This method
		/// will issue a sql TOP clause so only the specified number of values
		/// will be returned.
		/// of values
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this 
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A QueryFilter used to filter the 
		/// results
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="db"></param>
		public static ParentCollection Top(int count, QueryFilter where, OrderBy<ParentColumns> orderBy = null, Database database = null)
		{
			Database db = database ?? Db.For<Parent>();
			QuerySet query = GetQuerySet(db);
			query.Top<Parent>(count);
			query.Where(where);

			if(orderBy != null)
			{
				query.OrderBy<ParentColumns>(orderBy);
			}

			query.Execute(db);
			var results = query.Results.As<ParentCollection>(0);
			results.Database = db;
			return results;
		}

		/// <summary>
		/// Execute a query and return the specified number of values.  This method
		/// will issue a sql TOP clause so only the specified number of values
		/// will be returned.
		/// of values
		/// </summary>
		/// <param name="count">The number of values to return.
		/// This value is used in the sql query so no more than this 
		/// number of values will be returned by the database.
		/// </param>
		/// <param name="where">A QueryFilter used to filter the 
		/// results
		/// </param>
		/// <param name="orderBy">
		/// Specifies what column and direction to order the results by
		/// </param>
		/// <param name="db"></param>
		public static ParentCollection Top(int count, QiQuery where, Database database = null)
		{
			Database db = database ?? Db.For<Parent>();
			QuerySet query = GetQuerySet(db);
			query.Top<Parent>(count);
			query.Where(where);
			query.Execute(db);
			var results = query.Results.As<ParentCollection>(0);
			results.Database = db;
			return results;
		}

		/// <summary>
		/// Return the count of Parents
		/// </summary>
		public static long Count(Database database = null)
        {
			Database db = database ?? Db.For<Parent>();
            QuerySet query = GetQuerySet(db);
            query.Count<Parent>();
            query.Execute(db);
            return (long)query.Results[0].DataRow[0];
        }

		/// <summary>
		/// Execute a query and return the number of results
		/// </summary>
		/// <param name="where">A WhereDelegate that recieves a ParentColumns 
		/// and returns a IQueryFilter which is the result of any comparisons
		/// between ParentColumns and other values
		/// </param>
		/// <param name="db"></param>
		public static long Count(WhereDelegate<ParentColumns> where, Database database = null)
		{
			ParentColumns c = new ParentColumns();
			IQueryFilter filter = where(c) ;

			Database db = database ?? Db.For<Parent>();
			QuerySet query = GetQuerySet(db);	 
			query.Count<Parent>();
			query.Where(filter);	  
			query.Execute(db);
			return query.Results.As<CountResult>(0).Value;
		}

		private static Parent CreateFromFilter(IQueryFilter filter, Database database = null)
		{
			Database db = database ?? Db.For<Parent>();			
			var dao = new Parent();
			filter.Parameters.Each(p=>
			{
				dao.Property(p.ColumnName, p.Value);
			});
			dao.Save(db);
			return dao;
		}
		
		private static Parent OneOrThrow(ParentCollection c)
		{
			if(c.Count == 1)
			{
				return c[0];
			}
			else if(c.Count > 1)
			{
				throw new MultipleEntriesFoundException();
			}

			return null;
		}

	}
}																								
