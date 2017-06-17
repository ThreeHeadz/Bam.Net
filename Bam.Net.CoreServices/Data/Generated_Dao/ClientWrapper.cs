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
using Bam.Net.CoreServices.Data;
using Bam.Net.CoreServices.Data.Dao;

namespace Bam.Net.CoreServices.Data.Wrappers
{
	// generated
	[Serializable]
	public class ClientWrapper: Bam.Net.CoreServices.Data.Client, IHasUpdatedXrefCollectionProperties
	{
		public ClientWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public ClientWrapper(DaoRepository repository) : this()
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


Bam.Net.CoreServices.Data.Application _application;
		public override Bam.Net.CoreServices.Data.Application Application
		{
			get
			{
				if (_application == null)
				{
					_application = (Bam.Net.CoreServices.Data.Application)Repository.GetParentPropertyOfChild(this, typeof(Bam.Net.CoreServices.Data.Application));
				}
				return _application;
			}
			set
			{
				_application = value;
			}
		}Bam.Net.CoreServices.Data.Machine _machine;
		public override Bam.Net.CoreServices.Data.Machine Machine
		{
			get
			{
				if (_machine == null)
				{
					_machine = (Bam.Net.CoreServices.Data.Machine)Repository.GetParentPropertyOfChild(this, typeof(Bam.Net.CoreServices.Data.Machine));
				}
				return _machine;
			}
			set
			{
				_machine = value;
			}
		}

	}
	// -- generated
}																								