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
using Bam.Net.Services.AssemblyManagement.Data;
using Bam.Net.Services.AssemblyManagement.Data.Dao;

namespace Bam.Net.Services.AssemblyManagement.Data.Wrappers
{
	// generated
	[Serializable]
	public class AssemblyDescriptorWrapper: Bam.Net.Services.AssemblyManagement.Data.AssemblyDescriptor, IHasUpdatedXrefCollectionProperties
	{
		public AssemblyDescriptorWrapper()
		{
			this.UpdatedXrefCollectionProperties = new Dictionary<string, PropertyInfo>();
		}

		public AssemblyDescriptorWrapper(DaoRepository repository) : this()
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



// Xref property: Left -> AssemblyDescriptor ; Right -> ProcessRuntimeDescriptor

		List<Bam.Net.Services.AssemblyManagement.Data.ProcessRuntimeDescriptor> _processRuntimeDescriptors;
		public override List<Bam.Net.Services.AssemblyManagement.Data.ProcessRuntimeDescriptor> ProcessRuntimeDescriptor
		{
			get
			{
				if(_processRuntimeDescriptors == null)
				{
					 var xref = new XrefDaoCollection<Bam.Net.Services.AssemblyManagement.Data.Dao.AssemblyDescriptorProcessRuntimeDescriptor,  Bam.Net.Services.AssemblyManagement.Data.Dao.ProcessRuntimeDescriptor>(Repository.GetDaoInstance(this), false);
					 xref.Load(Repository.Database);
					 _processRuntimeDescriptors = ((IEnumerable)xref).CopyAs<Bam.Net.Services.AssemblyManagement.Data.ProcessRuntimeDescriptor>().ToList();
					 SetUpdatedXrefCollectionProperty("ProcessRuntimeDescriptors", this.GetType().GetProperty("ProcessRuntimeDescriptor"));
				}

				return _processRuntimeDescriptors;
			}
			set
			{
				_processRuntimeDescriptors = value;
				SetUpdatedXrefCollectionProperty("ProcessRuntimeDescriptors", this.GetType().GetProperty("ProcessRuntimeDescriptor"));
			}
		}
// Xref property: Left -> AssemblyReferenceDescriptor ; Right -> AssemblyDescriptor

		Bam.Net.Services.AssemblyManagement.Data.AssemblyReferenceDescriptor[] _assemblyReferenceDescriptors;
		public override Bam.Net.Services.AssemblyManagement.Data.AssemblyReferenceDescriptor[] AssemblyReferenceDescriptors
		{
			get
			{
				if(_assemblyReferenceDescriptors == null)
				{
					 var xref = new XrefDaoCollection<Bam.Net.Services.AssemblyManagement.Data.Dao.AssemblyReferenceDescriptorAssemblyDescriptor, Bam.Net.Services.AssemblyManagement.Data.Dao.AssemblyReferenceDescriptor>(Repository.GetDaoInstance(this), false);
					 xref.Load(Repository.Database);
					 _assemblyReferenceDescriptors = ((IEnumerable)xref).CopyAs<Bam.Net.Services.AssemblyManagement.Data.AssemblyReferenceDescriptor>().ToArray();
					 SetUpdatedXrefCollectionProperty("AssemblyReferenceDescriptors", this.GetType().GetProperty("AssemblyReferenceDescriptors"));
				}

				return _assemblyReferenceDescriptors;
			}
			set
			{
				_assemblyReferenceDescriptors = value;
				SetUpdatedXrefCollectionProperty("AssemblyReferenceDescriptors", this.GetType().GetProperty("AssemblyReferenceDescriptors"));
			}
		}	}
	// -- generated
}																								
