using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Linq;
using Bam.Net;
using Bam.Net.Data;
using Bam.Net.Data.Repositories;
using Newtonsoft.Json;
using Bam.Net.CoreServices.ApplicationRegistration;
using Bam.Net.CoreServices.ApplicationRegistration.Dao;

namespace Bam.Net.CoreServices.ApplicationRegistration.Wrappers
{
	// generated
	[Serializable]
	public class OrganizationWrapper: Bam.Net.CoreServices.ApplicationRegistration.Organization, IHasUpdatedXrefCollectionProperties
	{
		public OrganizationWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public OrganizationWrapper(DaoRepository repository) : this()
		{
			this.Repository = repository;
		}

		[JsonIgnore]
		public DaoRepository Repository { get; set; }

		[JsonIgnore]
		public Dictionary<string, PropertyInfo> UpdatedXrefCollectionProperties { get; set; }

		protected void SetUpdatedXrefCollectionProperty(string propertyName, PropertyInfo correspondingProperty)
		{
			if(UpdatedXrefCollectionProperties != null && !UpdatedXrefCollectionProperties.ContainsKey(propertyName))
			{
				UpdatedXrefCollectionProperties.Add(propertyName, correspondingProperty);				
			}
			else if(UpdatedXrefCollectionProperties != null)
			{
				UpdatedXrefCollectionProperties[propertyName] = correspondingProperty;				
			}
		}

Bam.Net.CoreServices.ApplicationRegistration.Application[] _applications;
		public override Bam.Net.CoreServices.ApplicationRegistration.Application[] Applications
		{
			get
			{
				if (_applications == null)
				{
					_applications = Repository.ForeignKeyCollectionLoader<Bam.Net.CoreServices.ApplicationRegistration.Organization, Bam.Net.CoreServices.ApplicationRegistration.Application>(this).ToArray();
				}
				return _applications;
			}
			set
			{
				_applications = value;
			}
		}

// Xref property: Left -> Organization ; Right -> User

		Bam.Net.CoreServices.ApplicationRegistration.User[] _users;
		public override Bam.Net.CoreServices.ApplicationRegistration.User[] Users
		{
			get
			{
				if(_users == null)
				{
					 var xref = new XrefDaoCollection<Bam.Net.CoreServices.ApplicationRegistration.Dao.OrganizationUser,  Bam.Net.CoreServices.ApplicationRegistration.Dao.User>(Repository.GetDaoInstance(this), false);
					 xref.Load(Repository.Database);
					 _users = ((IEnumerable)xref).CopyAs<Bam.Net.CoreServices.ApplicationRegistration.User>().ToArray();
					 SetUpdatedXrefCollectionProperty("Users", this.GetType().GetProperty("Users"));
				}

				return _users;
			}
			set
			{
				_users = value;
				SetUpdatedXrefCollectionProperty("Users", this.GetType().GetProperty("Users"));
			}
		}
	}
	// -- generated
}																								
