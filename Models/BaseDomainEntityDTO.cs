using Kadastr.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Models
{
	public class BaseDomainEntityDTO
	{
		[HiddenInput(DisplayValue=false)]
		public long Id { get; set; }
		[HiddenInput]
		public string Name { get; set; }
		[HiddenInput]
		public string Type { get; set; }
		public List<AttributeValueDTO> AttributeValues { get; set; }

		public BaseDomainEntityDTO() 
		{
			this.AttributeValues = new List<AttributeValueDTO>();
		}

		public BaseDomainEntityDTO(clsBaseDomainEntity entity) : this()
		{
			this.Id = (long)entity.Id;
			this.Name = entity.sName;
			this.Type = entity.Type.sName;
			
			foreach (clsAttributeValue av in entity.colAttributeValues) {
				this.AttributeValues.Add(new AttributeValueDTO(av, Id));
			}
		}

		public void ExtendAttributes(clsOperationType operationType)
		{
			foreach (clsAttribute attribute in operationType.colAttributes) {
				if (!this.AttributeValues.Exists(av => av.IdAttribute == attribute.Id))
				{
					this.AttributeValues.Add(new AttributeValueDTO(attribute, Id));
				}
			}
		}
	}
}