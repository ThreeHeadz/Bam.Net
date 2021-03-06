/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using Bam.Net;
using Bam.Net.Incubation;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace Bam.Net.Data
{
    /// <summary>
    /// Data Access Object
    /// </summary>
    public abstract class Dao : ICommittable, IHasDataRow
    {
        static Dictionary<string, string> _proxiedConnectionNames;

        static Dao()
        {
            _proxiedConnectionNames = new Dictionary<string, string>();
            PostConstructActions = new Dictionary<Type, Action<Dao>>();
        }

        public Dao()
        {
            Initialize();
        }

        public Dao(Database database)
        {
            Database = database;
            Initialize();
        }

        public Dao(DataRow row)
            : this()
        {
            DataRow = row;
            IsNew = false;
        }

        public Dao(Database database, DataRow row)
        {
            Database = database;
            DataRow = row;
            Initialize();
        }

        static volatile Incubator _proxyTypeProvider;
        static object _proxyTypeProviderLock = new object();

        [Obsolete("There is no known logical use for this and it should be removed unless one can be identified.")]
        public static Incubator ProxyTypeProvider
        {
            get
            {
                if (_proxyTypeProvider == null)
                {
                    lock (_proxyTypeProviderLock)
                    {
                        if (_proxyTypeProvider == null)
                        {
                            _proxyTypeProvider = Incubator.Default;
                        }
                    }
                }

                return _proxyTypeProvider;
            }

            set
            {
                _proxyTypeProvider = value;
            }
        }

        Dictionary<string, ILoadable> _childCollections;
        /// <summary>
        /// Actions keyed by type to take after contruction
        /// </summary>
        public static Dictionary<Type, Action<Dao>> PostConstructActions { get; set; }

        /// <summary>
        /// Instantiate all dao types found in the assembly that contains the
        /// specified type and place them into the Incubator.Default
        /// </summary>
        /// <param name="daoSibling"></param>
        public static void RegisterDaoTypes(Type daoSibling)
        {
            RegisterDaoTypes(daoSibling.Assembly, Incubator.Default);
        }

        /// <summary>
        /// Instantiate all Dao types in the assembly that contains the specified
        /// type and place them into the specified
        /// serviceProvider
        /// </summary>
        /// <param name="daoAssembly"></param>
        /// <param name="serviceProvider"></param>
        public static void RegisterDaoTypes(Type daoSibling, Incubator serviceProvider)
        {
            RegisterDaoTypes(daoSibling.Assembly, serviceProvider);
        }

        /// <summary>
        /// Instantiate all Dao types in the specified assembly and place them into the specified
        /// serviceProvider
        /// </summary>
        /// <param name="daoAssembly"></param>
        /// <param name="serviceProvider"></param>
        public static void RegisterDaoTypes(Assembly daoAssembly, Incubator serviceProvider)
        {
            Type[] types = daoAssembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type current = types[i];
                if (current.IsSubclassOf(typeof(Dao)))
                {
                    serviceProvider.Construct(current);
                }
            }
        }

        public Type[] GetSchemaTypes()
        {
            Type thisType = this.GetType();
            return GetSchemaTypes(thisType);
        }

        public static Type[] GetSchemaTypes<T>()
        {
            return GetSchemaTypes(typeof(T));
        }

        public static Type[] GetSchemaTypes(Type daoType)
        {
            List<Type> results = new List<Type>();
            results.Add(daoType);

            results.AddRange(daoType.Assembly.GetTypes().Where(t => t.HasCustomAttributeOfType<TableAttribute>()));
            return results.ToArray();
        }

        /// <summary>
        /// An overridable method to provide constructor functionality since the constructors are generated.
        /// </summary>
        public virtual void OnInitialize()
        {
            // override if constructor functionality is required
            Initializer(this);
        }

        Action<Dao> _initializer;
        public Action<Dao> Initializer
        {
            get
            {
                if (_initializer == null)
                {
                    return GlobalInitializer;
                }
                return _initializer;
            }
            set
            {
                _initializer = value;
            }
        }

        static Action<Dao> _globalInitializer;
        public static Action<Dao> GlobalInitializer
        {
            get
            {
                if (_globalInitializer == null)
                {
                    return (dao) =>
                    {
                    };
                }
                return _globalInitializer;
            }
            set
            {
                _globalInitializer = value;
            }
        }

        public override int GetHashCode()
        {
            long id = IdValue.GetValueOrDefault();
            if (id > 0)
            {
                return id.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            Dao dao = obj as Dao;
            if (dao != null)
            {
                return dao.GetType() == this.GetType() && dao.IdValue == this.IdValue;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        PropertyInfo _uuidProp;
        bool? _hasUuid;
        protected bool HasUuidProperty(out PropertyInfo uuidProp)
        {
            if (_hasUuid == null)
            {
                _uuidProp = this.GetType().GetProperty("Uuid");
                _hasUuid = _uuidProp != null;
            }

            uuidProp = _uuidProp;
            return _hasUuid.Value;
        }

        protected Database _database;
        object _databaseSync = new object();
        public Database Database
        {
            get
            {
                return _databaseSync.DoubleCheckLock(ref _database, () => Db.For(this.GetType()));
            }
            set
            {
                _database = value;
            }
        }

        List<string> _columns;
        object _columnsLock = new object();
        public string[] Columns
        {
            get
            {
                _columns = _columnsLock.DoubleCheckLock(ref _columns, () =>
                {
                    List<string> keys = new List<string>();
                    for (int i = 0; i < DataRow?.Table?.Columns.Count; i++)
                    {
                        DataColumn current = DataRow.Table.Columns[i];
                        keys.Add(current.ColumnName);
                    }
                    return keys;
                });
                return _columns.ToArray();
            }
        }
        
        public T Value<T>(string columnName, object value = null)
        {
            DataTable table = DataRow.Table;
            if (!table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName);
            }
            if(value != null)
            {
                DataRow[columnName] = value;
            }
            return (T)GetCurrentValue(columnName);
        }

        /// <summary>
        /// If true, any references to the current
        /// record will be deleted prior to deleting
        /// the current record in a call to Delete()
        /// if those references have been hydrated on
        /// the current instance
        /// </summary>
        [Exclude]
        public bool AutoDeleteChildren { get; set; }

        /// <summary>
        /// If true, will hydrate child collections
        /// prior to deletion so that they can be deleted.
        /// Incurs performance cost if many child 
        /// collections must be loaded; can cause intense
        /// memory pressure if large amounts of data
        /// need to be loaded
        /// </summary>
        [Exclude]
        public bool AutoHydrateChildrenOnDelete { get; set; }

        public event DaoDelegate BeforeWriteCommit;
        public static event DaoDelegate BeforeWriteCommitAny;
        protected internal void OnBeforeWriteCommit(Database db)
        {
            if (BeforeWriteCommit != null)
            {
                BeforeWriteCommit(db, this);
            }

            if (BeforeWriteCommitAny != null)
            {
                BeforeWriteCommitAny(db, this);
            }
        }

        public event DaoDelegate AfterWriteCommit;
        public static event DaoDelegate AfterWriteCommitAny;
        protected internal void OnAfterWriteCommit(Database db)
        {
            if (AfterWriteCommitAny != null)
            {
                AfterWriteCommitAny(db, this);
            }

            if (AfterWriteCommitAny != null)
            {
                AfterWriteCommitAny(db, this);
            }
        }

        public event DaoDelegate BeforeCommit;
        public static event DaoDelegate BeforeCommitAny;
        protected internal void OnBeforeCommit(Database db)
        {
            if (BeforeCommit != null)
            {
                BeforeCommit(db, this);
            }

            if (BeforeCommitAny != null)
            {
                BeforeCommitAny(db, this);
            }
        }

        /// <summary>
        /// Fires after this instance has been committed.
        /// May be fired as the result of its membership in 
        /// a DaoCollection, in that case the current
        /// Dao instance may not be fully-hydrated at the
        /// time of the firing of this event
        /// </summary>
        public event ICommittableDelegate AfterCommit;
        public static event DaoDelegate AfterCommitAny;
        protected internal void OnAfterCommit(Database db)
        {
            if (AfterCommit != null)
            {
                AfterCommit(db, this);
            }

            if (AfterCommitAny != null)
            {
                AfterCommitAny(db, this);
            }
        }

        public event DaoDelegate BeforeWriteDelete;
        public static event DaoDelegate BeforeWriteDeleteAny;
        protected void OnBeforeWriteDelete(Database db)
        {
            if (BeforeWriteDelete != null)
            {
                BeforeWriteDelete(db, this);
            }

            if (BeforeWriteDeleteAny != null)
            {
                BeforeWriteDeleteAny(db, this);
            }
        }

        public event DaoDelegate AfterWriteDelete;
        public static event DaoDelegate AfterWriteDeleteAny;
        protected void OnAfterWriteDelete(Database db)
        {
            if (AfterWriteDelete != null)
            {
                AfterWriteDelete(db, this);
            }

            if (AfterWriteDeleteAny != null)
            {
                AfterWriteDeleteAny(db, this);
            }
        }

        public event DaoDelegate BeforeDelete;
        public static event DaoDelegate BeforeDeleteAny;
        protected void OnBeforeDelete(Database db)
        {
            if (BeforeDelete != null)
            {
                BeforeDelete(db, this);
            }

            if (BeforeDeleteAny != null)
            {
                BeforeDeleteAny(db, this);
            }
        }

        public event DaoDelegate AfterDelete;
        public static event DaoDelegate AfterDeleteAny;
        protected void OnAfterDelete(Database db)
        {
            if (AfterDelete != null)
            {
                AfterDelete(db, this);
            }

            if (AfterDeleteAny != null)
            {
                AfterDeleteAny(db, this);
            }

        }

        protected internal Dictionary<string, ILoadable> ChildCollections
        {
            get
            {
                return _childCollections;
            }
        }

        /// <summary>
        /// Reset the child collections for this instance forcing
        /// them to be reloaded the next time they are referenced.
        /// </summary>
        public void ResetChildren()
        {
            _childCollections.Clear();
        }

        public virtual ValidationResult Validate()
        {
            return Validator(this);
        }

        Func<Dao, ValidationResult> _validator;
        public Func<Dao, ValidationResult> Validator
        {
            get
            {
                if (_validator == null)
                {
                    return GlobalValidator;
                }

                return _validator;
            }
            set
            {
                _validator = value;
            }
        }

        static Func<Dao, ValidationResult> _globalValidator;
        public static Func<Dao, ValidationResult> GlobalValidator
        {
            get
            {
                if (_globalValidator == null)
                {
                    return (dao) =>
                    {
                        return new ValidationResult();
                    };
                }

                return _globalValidator;
            }
            set
            {
                _globalValidator = value;
            }
        }

        /// <summary>
        /// Checks that every required column has a value.  Untested as of 05/09/2013 :b
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        protected bool ValidateRequiredProperties(out string[] messages)
        {
            Type type = this.GetType();
            PropertyInfo[] props = type.GetPropertiesWithAttributeOfType<ColumnAttribute>();
            List<string> msgs = new List<string>();
            foreach (PropertyInfo prop in props)
            {
                ColumnAttribute ca = prop.GetCustomAttributeOfType<ColumnAttribute>();
                if (!(ca is KeyColumnAttribute) && !ca.AllowNull)
                {
                    string propTypeName = prop.PropertyType.Name;
                    MethodInfo getter = type.GetMethod("Get{0}Value"._Format(propTypeName));
                    object val = getter.Invoke(this, new object[] { ca.Name });
                    if (val == null || (val is string && string.IsNullOrEmpty((string)val)))
                    {
                        msgs.Add("{0} can't be null"._Format(prop.Name));
                    }
                }
            }
            messages = msgs.ToArray();
            return messages.Length == 0;
        }

        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated. Same as Commit
        /// </summary>
        public void Save()
        {
            Commit();
        }

        /// <summary>
        /// Saves the current instance asynchronously
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public Task SaveAsync(Database db = null)
        {
            return Task.Run(() =>
            {
                Save(db);
            });
        }
        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will not be triggerred.
        /// Same as Save
        /// </summary>
        public void Save(Database db)
        {
            Commit(db);
        }
        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will not be triggerred.
        /// Same as Save
        /// </summary>
        public virtual void Commit()
        {
            Database db = Database;

            Commit(db);
        }
        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will not be triggerred.
        /// Same as Save
        /// </summary>
        public virtual void Commit(DaoTransaction tx)
        {
            Commit(tx.Database);
        }
        public void Commit(Database db)
        {
            Commit(db, true);
        }
        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will be triggerred the 
        /// children should be re-hydrated to ensure they are fully
        /// hydrated.
        /// Same as Save
        /// </summary>
        public void Commit(Database db, bool commitChildren)
        {
            db = db ?? Database;

            ThrowIfInvalid();

            QuerySet querySet = GetQuerySet(db);

            WriteCommit(querySet, db);
            if (commitChildren)
            {
                WriteChildCommits(querySet, db);
            }

            if (!string.IsNullOrWhiteSpace(querySet.ToString()))
            {
                ExecuteCommit(db, querySet);
            }
        }

        public void Update(Database db = null)
        {
            db = db ?? Database;

            ThrowIfInvalid();

            QuerySet querySet = GetQuerySet(db);

            WriteUpdate(querySet);
            if (!string.IsNullOrWhiteSpace(querySet.ToString()))
            {
                ExecuteCommit(db, querySet);
            }
        }

        public void Insert(Database db = null)
        {
            db = db ?? Database;
            ThrowIfInvalid();
            QuerySet querySet = GetQuerySet(db);

            WriteInsert(querySet);
            if (!string.IsNullOrWhiteSpace(querySet.ToString()))
            {
                ExecuteCommit(db, querySet);
            }
        }

        protected internal void WriteChildCommits(SqlStringBuilder sql, Database db = null)
        {
            db = db ?? Database;
            foreach (string key in this.ChildCollections.Keys)
            {
                ICommittable c = this.ChildCollections[key];
                c.WriteCommit(sql, db);
            }

            sql.Executed += (s, d) =>
            {
                this.ChildCollections.Clear();
            };
        }

        public void WriteChildDeletes(SqlStringBuilder sql)
        {
            foreach (string key in ChildCollections.Keys)
            {
                ILoadable collection = ChildCollections[key];
                if (AutoHydrateChildrenOnDelete)
                {
                    if (collection.HasProperty(nameof(AutoHydrateChildrenOnDelete))) // it might be an XrefDaoCollection which doesn't have that property
                    {
                        collection.Property(nameof(AutoHydrateChildrenOnDelete), true);
                        collection.Load(Database);
                    }
                }
                collection.WriteDelete(sql);
            }
        }

        public virtual void Delete(Database database = null)
        {
            Database db;
            SqlStringBuilder sql = GetSqlStringBuilder(out db);
            if (database != null)
            {
                sql = GetSqlStringBuilder(database);
                db = database;
            }

            if (AutoDeleteChildren)
            {
                WriteChildDeletes(sql);
            }
            WriteDelete(sql);

            OnBeforeDelete(db);
            sql.Execute(db);
            OnAfterDelete(db);
        }

        public void PreLoadChildCollections()
        {
            foreach (ILoadable loadable in ChildCollections.Values)
            {
                loadable.Load(Database);
            }
        }

        protected virtual void Delete<C>(Func<C, IQueryFilter<C>> where) where C : IFilterToken, new()
        {
            C columns = new C();
            IQueryFilter<C> filter = where(columns);
            Delete(filter);
        }

        protected virtual void Delete(IQueryFilter filter)
        {
            Database db;
            SqlStringBuilder sql = GetSqlStringBuilder(out db);

            foreach (string key in this.ChildCollections.Keys)
            {
                this.ChildCollections[key].WriteDelete(sql);
            }

            WriteDelete(sql, filter);
            sql.Execute(db);
            sql.Reset();
        }

        protected internal SqlStringBuilder GetSqlStringBuilder()
        {
            Database ignore;
            return GetSqlStringBuilder(out ignore);
        }

        protected internal SqlStringBuilder GetSqlStringBuilder(out Database db)
        {
            db = Database;
            return GetSqlStringBuilder(db);
        }

        protected internal static SqlStringBuilder GetSqlStringBuilder(Database db)
        {
            SqlStringBuilder sql = db.ServiceProvider.Get<SqlStringBuilder>();
            return sql;
        }

        protected internal QuerySet GetQuerySet()
        {
            Database ignore;
            return GetQuerySet(out ignore);
        }

        protected internal QuerySet GetQuerySet(out Database db)
        {
            db = Database;
            return GetQuerySet(db);
        }

        protected internal static QuerySet GetQuerySet(Database db)
        {
            return db.GetQuerySet();
        }

        public virtual void WriteDelete(SqlStringBuilder sql)
        {
            Database db = Database;
            OnBeforeWriteDelete(db);
            sql.Delete(TableName()).Where(db.GetAssignment(KeyColumnName, IdValue, sql.ColumnNameFormatter));
            OnAfterWriteDelete(db);
        }

        protected internal virtual void WriteDelete(SqlStringBuilder sqlStringBuilder, IQueryFilter filter)
        {
            sqlStringBuilder.Delete(TableName()).Where(filter).Go();
        }

        /// <summary>
        /// Write the update or insert statement for the current instance
        /// to the specified SqlStringBuilder.
        /// </summary>
        /// <param name="sqlStringBuilder"></param>
        public virtual void WriteCommit(SqlStringBuilder sqlStringBuilder)
        {
            Database db = Database;
            WriteCommit(sqlStringBuilder, db);
        }

        public virtual void WriteCommit(SqlStringBuilder sqlStringBuilder, Database db)
        {
            OnBeforeWriteCommit(db);
            if (HasNewValues)
            {
                if (this.IsNew || this.ForceInsert)
                {
                    WriteInsert(sqlStringBuilder);
                }
                else
                {
                    WriteUpdate(sqlStringBuilder);
                }
            }
            OnAfterWriteCommit(db);
        }

        public void WriteUpdate(SqlStringBuilder sqlStringBuilder)
        {
            AssignValue[] valueAssignments = GetNewAssignValues();
            sqlStringBuilder
                .Update(this.TableName(), valueAssignments)
                .Where(this.GetUniqueFilter())
                .Go();
        }

        public void WriteInsert(SqlStringBuilder sqlStringBuilder)
        {
            sqlStringBuilder
                .Insert(this)
                .Go();
        }

        /// <summary>
        /// Undo any changes that have been made to the current instance
        /// since it was loaded.
        /// </summary>
        /// <param name="db"></param>
        public virtual void Undo(Database db = null)
        {
            Type thisType = this.GetType();
            if (db == null)
            {
                db = Database;
            }

            SqlStringBuilder sql = GetSqlStringBuilder(db);
            ColumnAttribute[] columns = Db.GetColumns(thisType);
            foreach (ColumnAttribute col in columns)
            {
                this.SetValue(col.Name, this.GetOriginalValue(col.Name));
            }
            AssignValue[] values = GetNewAssignValues();
            sql.Update(this.TableName(), values)
                .Where(this.GetUniqueFilter())
                .Go();

            sql.Execute(db);
        }

        /// <summary>
        /// Re-insert the current instance after it has been deleted
        /// </summary>
        /// <param name="db"></param>
        public virtual void Undelete(Database db = null)
        {
            Type thisType = this.GetType();
            if (db == null)
            {
                db = Database;// Db.For(thisType);
            }

            this.IsNew = true;
            ColumnAttribute[] columns = Db.GetColumns(thisType);
            foreach (ColumnAttribute col in columns)
            {
                this.SetValue(col.Name, this.GetCurrentValue(col.Name));
            }

            this.Commit();
        }

        static Dictionary<Type, object> _dynamicTypeLocks = new Dictionary<Type, object>();
        /// <summary>
        /// Creates an in memory dynamic type representing
        /// the current Dao's Columns only.
        /// </summary>
        /// <param name="daoObject"></param>
        /// <returns></returns>
        public object ToJsonSafe()
        {
            Type thisType = this.GetType();
            _dynamicTypeLocks.AddMissing(thisType, new object());
            lock (_dynamicTypeLocks[thisType])
            {
                Type jsonSafeType = this.BuildDynamicType<ColumnAttribute>(false);
                ConstructorInfo ctor = jsonSafeType.GetConstructor(new Type[] { });
                object jsonSafeInstance = ctor.Invoke(null);
                jsonSafeInstance.CopyProperties(this);
                return jsonSafeInstance;
            }
        }

        /// <summary>
        /// Creates an in memory dynamic type representing
        /// the current Dao's Columns only.
        /// </summary>
        /// <param name="includeExtras">Include anything added through the Value method</param>
        /// <returns></returns>
        public object ToJsonSafe(bool includeExtras = false)
        {
            object jsonSafe = ToJsonSafe();
            object result = jsonSafe;
            if (includeExtras)
            {
                object extras = ToDynamic();
                Type mergedType = new List<object>() { jsonSafe, extras }.MergeToDynamicType(Dao.TableName(this), 0);
                result = mergedType.Construct();
                result.CopyProperties(extras);
                result.CopyProperties(jsonSafe);                
            }
            return result;
        }

        /// <summary>
        /// Create an in memory dynamic type representing 
        /// all the values in DataRow including anything 
        /// added through the Value method
        /// </summary>
        /// <returns></returns>
        public object ToDynamic()
        {
            return DataRow.ToDynamic();
        }

        /// <summary>
        /// Gets an array of AssignValue instances that represent 
        /// the names and values of columns with new values
        /// </summary>
        /// <returns></returns>
        protected internal AssignValue[] GetNewAssignValues()
        {
            List<AssignValue> valueAssignments = new List<AssignValue>();
            foreach (string columnName in this.NewValues.Keys)
            {
                valueAssignments.Add(new AssignValue(columnName, this.NewValues[columnName]));
            }
            return valueAssignments.ToArray();
        }

        /// <summary>
        /// Gets an array of AssignValue instances that represent 
        /// the names and values of columns with old values (prior to any new sets)
        /// </summary>
        /// <returns></returns>
        protected internal AssignValue[] GetOldAssignValues()
        {
            List<AssignValue> valueAssignments = new List<AssignValue>();
            foreach (DataColumn column in this.DataRow.Table.Columns)
            {
                valueAssignments.Add(new AssignValue(column.ColumnName, this.DataRow[column]));
            }
            return valueAssignments.ToArray();
        }

        /// <summary>
        /// When implemented by a derived class returns filters that should uniquely
        /// identify a single record.
        /// </summary>
        /// <returns></returns>
        public abstract IQueryFilter GetUniqueFilter();


        public Func<IQueryFilter> UniqueFilterProvider
        {
            get;
            set;
        }

        public string ConnectionName()
        {
            return ConnectionName(this);
        }

        /// <summary>
        /// Returns the connection name for the specified Dao instance or the proxied
        /// name if the connection name for the specified Dao instance has been proxied
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string ConnectionName(object instance)
        {
            Type type = instance.GetType();
            return ConnectionName(type);
        }

        public static string ConnectionName<T>()
        {
            return ConnectionName(typeof(T));
        }

        /// <summary>
        /// Returns the connection name for the specified type or the proxied
        /// name if the connection name for the specified type has been proxied
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ConnectionName(Type type)
        {
            string real = RealConnectionName(type);
            string proxied = ProxiedConnectionName(type);

            if (!real.Equals(proxied))
            {
                return proxied;
            }
            else
            {
                return real;
            }
        }

        protected internal static string RealConnectionName(Type type)
        {
            TableAttribute tableAttr = type.GetCustomAttributeOfType<TableAttribute>();
            string value = string.Empty;
            if (tableAttr != null)
            {
                value = tableAttr.ConnectionName;
            }
            else
            {
                PropertyInfo prop = type.GetProperty("ConnectionName");
                if (prop != null && prop.GetGetMethod().IsStatic && prop.PropertyType == typeof(string))
                {
                    value = (string)prop.GetValue(null, null);
                }
            }

            return value;
        }

        /// <summary>
        /// Returns the proxied connection name for the specified
        /// Type if the connection hasn't been proxied/redirected
        /// then the real connection name will be returned.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected static string ProxiedConnectionName(Type type)
        {
            string realName = RealConnectionName(type);
            if (_proxiedConnectionNames.ContainsKey(realName))
            {
                return _proxiedConnectionNames[realName];
            }
            return realName;
        }

        /// <summary>
        /// Stop redirecting the connection name for the specified
        /// type
        /// </summary>
        /// <param name="type"></param>
        public static void UnproxyConnection(Type type)
        {
            string realName = RealConnectionName(type);
            UnproxyConnection(realName);
        }

        /// <summary>
        /// Stop redirecting the connection name for the specified type
        /// </summary>
        /// <param name="realName"></param>
        public static void UnproxyConnection(string realName)
        {
            if (_proxiedConnectionNames.ContainsKey(realName))
            {
                _proxiedConnectionNames.Remove(realName);
            }
        }

        /// <summary>
        /// Causes calls to ConnectionName for the specified type to 
        /// return the specified newConnectionName.  This method must be
        /// called prior to any XXXRegistrar.Register(Type, "CxName") calls
        /// for example: SqlClientRegistrar.Register(typeof(Employee), "MyConnectionName")
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newConnectionName"></param>
        public static void ProxyConnection(Type type, string newConnectionName)
        {
            string real = RealConnectionName(type);
            ProxyConnection(real, newConnectionName);
        }

        /// <summary>
        /// Causes calls to ConnectionName for the specified originalConnection 
        /// name to return the specified newConnectionName
        /// </summary>
        /// <param name="originalConnectionName"></param>
        /// <param name="newConnectionName"></param>
        public static void ProxyConnection(string originalConnectionName, string newConnectionName)
        {
            _proxiedConnectionNames.AddMissing(originalConnectionName, newConnectionName);
            _proxiedConnectionNames[originalConnectionName] = newConnectionName;
        }

        public string TableName()
        {
            return TableName(this);
        }

        public static string TableName(object instance)
        {
            Type type = instance.GetType();
            return TableName(type);
        }

        /// <summary>
        /// Returns the table name that represents the
        /// current Dao type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TableName(Type type)
        {
            TableAttribute tableAttr = type.GetCustomAttributeOfType<TableAttribute>();
            string value = type.Name;
            if (tableAttr != null)
            {
                value = tableAttr.TableName;
            }

            return value;
        }

        public static string GetKeyColumnName<T>() where T : Dao, new()
        {
            return GetKeyColumnName(typeof(T));
        }

        public static string GetKeyColumnName(Type type)
        {
            string name = "Id";
            KeyColumnAttribute attribute;
            type.GetFirstProperyWithAttributeOfType<KeyColumnAttribute>(out attribute);
            if (attribute != null)
            {
                name = attribute.Name;
            }
            return name;
        }

        long? _idValue;
        [Exclude]
        public long? IdValue
        {
            get
            {
                if (DataRow != null && DataRow.Table.Columns.Contains(KeyColumnName))
                {
                    object value = DataRow[KeyColumnName];
                    if (value != null && value != DBNull.Value)
                    {
                        try
                        {
                            _idValue = new long?(Convert.ToInt64(value));
                        }
                        catch
                        {
                            _idValue = null;
                        }
                    }
                }
                return _idValue;
            }
            set
            {
                _idValue = value;
            }
        }

        [Exclude]
        public string KeyColumnName { get; protected set; }

        protected void SetKeyColumnName()
        {
            KeyColumnName = GetKeyColumnName(this.GetType());
        }

        bool _forceInsert;

        /// <summary>
        /// Overrides default logic as to whether 
        /// to insert or update the current instance
        /// based on the state of its Id causing
        /// Save to always insert instead of checking
        /// whether it should insert or update
        /// </summary>
        [Exclude]
        public bool ForceInsert
        {
            get
            {
                return _forceInsert;
            }
            set
            {
                _forceInsert = value;
            }
        }

        /// <summary>
        /// Overrides default logic as to whether 
        /// to insert or update the current instance
        /// based on the state of its Id causing
        /// Save to always update instead of checking
        /// whether it should insert or update
        /// </summary>
        [Exclude]
        public bool ForceUpdate
        {
            get
            {
                return !_forceInsert;
            }
            set
            {
                _forceInsert = !value;
            }
        }


        bool _isNew;
        /// <summary>
        /// Returns true if the current instance hasn't been committed
        /// as determined by whether the IdValue is greater than 0
        /// </summary>
        [Exclude]
        public bool IsNew
        {
            get
            {
                if (IdValue != null && IdValue.HasValue && IdValue > 0)
                {
                    _isNew = false;
                }

                return _isNew;
            }
            set
            {
                _isNew = value;
            }
        }

        Incubator _incubator;
        [Exclude]
        public Incubator ServiceProvider
        {
            get
            {
                if (_incubator == null)
                {
                    if (Database != null)
                    {
                        _incubator = Database.ServiceProvider;
                    }
                }
                return _incubator;
            }
            protected internal set
            {
                _incubator = value;
            }
        }

        [Exclude]
        public DataRow DataRow { get; set; }

        /// <summary>
        /// Returns true if properties of the
        /// current Dao instance have been set
        /// since its instanciation
        /// </summary>
        protected internal bool HasNewValues
        {
            get
            {
                return NewValues.Count > 0;
            }
        }
        protected internal Dictionary<string, object> NewValues
        {
            get;
            set;
        }

        object _primaryKey;
        protected object PrimaryKey
        {
            get
            {
                if (DataRow != null && DataRow.Table.Columns.Contains(KeyColumnName))
                {
                    _primaryKey = DataRow[KeyColumnName];
                }

                return _primaryKey;
            }
        }

        protected internal DataRow ToDataRow()
        {
            return ToDataRow(this);
        }

        protected internal static DataRow ToDataRow(Dao instance)
        {
            if (instance.DataRow != null)
            {
                return instance.DataRow;
            }

            return ((object)instance).ToDataRow(TableName(instance));
        }


        protected internal object GetOriginalValue(string columnName)
        {
            if (DataRow != null && DataRow.Table.Columns.Contains(columnName))
            {
                return DataRow[columnName];
            }
            else if (columnName.Equals(KeyColumnName))
            {
                return IdValue;
            }

            return null;
        }

        protected internal object GetCurrentValue(string columnName)
        {
            object result = null;
            if (columnName.Equals(KeyColumnName))
            {
                result = IdValue;
                if (result == null)
                {
                    result = PrimaryKey;
                }
            }
            else
            {
                if (NewValues.ContainsKey(columnName))
                {
                    result = NewValues[columnName];
                }
                else
                {
                    result = GetOriginalValue(columnName);
                }
            }
            return result;
        }

        protected string GetStringValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return Convert.ToString(val); 
            }

            return string.Empty;
        }

        protected int? GetIntValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return new int?(Convert.ToInt32(val));
            }

            return new int?();
        }

        protected long? GetLongValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return new long?(Convert.ToInt64(val));
            }

            return new long?();
        }

        protected decimal? GetDecimalValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return new decimal?(Convert.ToDecimal(val));
            }

            return new decimal?();
        }

        protected bool? GetBooleanValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                if (val is long)
                {
                    return new bool?((long)val > 0);
                }
                else if (val is int)
                {
                    return new bool?((int)val > 0);
                }
                else if (val is string)
                {
                    string str = (string)val;
                    return new bool?(str.ToLowerInvariant().Equals("true") || str.Equals("1"));
                }
                else
                {
                    return new bool?((bool)val);
                }
            }

            return new bool?(false);
        }

        protected byte[] GetByteArrayValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val == null || val == DBNull.Value || val is string)
            {
                return new byte[] { };
            }
            else
            {
                return (byte[])val;
            }
        }

        protected DateTime GetDateTimeValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                if (val is double)
                {
                    return ((double)val).ToDateTime();
                }
                else if (val is string)
                {
                    return DateTime.Parse((string)val);
                }
                else
                {
                    return (DateTime)val;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        protected internal void SetValue(string columnName, object value)
        {
            if (columnName.Equals(KeyColumnName))
            {
                if (value != null && value != DBNull.Value)
                {
                    IdValue = new long?(Convert.ToInt64(value));
                }
            }
            else if (this.NewValues.ContainsKey(columnName))
            {
                this.NewValues[columnName] = value;
            }
            else
            {
                this.NewValues.Add(columnName, value);
            }
        }

        private void Initialize()
        {
            this._childCollections = new Dictionary<string, ILoadable>();
            this.NewValues = new Dictionary<string, object>();
            this.ServiceProvider = Incubator.Default;
            this.IsNew = true;
            this.DataRow = this.ToDataRow();
            this.AutoDeleteChildren = true;
            this.BeforeWriteCommit += (db, dao) =>
            {
                dao.EnsureUuid();
            };

            Type currentType = this.GetType();
            if (PostConstructActions.ContainsKey(currentType))
            {
                PostConstructActions[currentType](this);
            }

            this.OnInitialize();
        }
        private void EnsureUuid()
        {
            PropertyInfo uuid;
            if (HasUuidProperty(out uuid))
            {
                string currentUuid = (string)uuid.GetValue(this);
                if (string.IsNullOrEmpty(currentUuid))
                {
                    string uuidVal = Guid.NewGuid().ToString();
                    uuid.SetValue(this, uuidVal);
                }
            }
        }

        private void ExecuteCommit(Database db, QuerySet querySet)
        {
            OnBeforeCommit(db);
            querySet.Execute(db);
            this.IsNew = false;
            this.ResetChildren();
            this.Database = db;
            OnAfterCommit(db);
        }

        private void ThrowIfInvalid()
        {
            ValidationResult valid = this.Validate();
            if (!valid.Success)
            {
                throw new ValidationException(valid.Message, valid.Exception);
            }
        }
    }
}
