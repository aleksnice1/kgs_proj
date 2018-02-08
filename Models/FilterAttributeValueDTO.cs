using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kadastr.WebApp.Models
{
	public class FilterAttributeValueDTO
	{
		public long AttributeId { get; set; }
		public int LogicalExpression { get; set; }
		public AttributeValueDTO AttributeValue { get; set; }

		public FilterAttributeValueDTO()
		{
			this.LogicalExpression = 0;
			this.AttributeValue = null;
		}

		public FilterAttributeValueDTO(FilterAttributeValue fav) : this()
		{
			this.AttributeId = fav.oAttribute.Id;			
			this.AttributeValue = new AttributeValueDTO(fav.oAttribute);
		}

		public FilterAttributeValueDTO(int idAttribute, int logicalExpression, string value)
		{
			this.AttributeId = (long)idAttribute;
			this.LogicalExpression = logicalExpression;
			
			clsAttribute attr;
			AttributeValueDTO avDTO;
			var attrRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			if (attrRepository.TryGetById(idAttribute, out attr))
			{
				avDTO = new AttributeValueDTO(attr);
			}
			else {
				avDTO = new AttributeValueDTO();
			};
			avDTO.Value = clsAttributeValue.WriteXML(value);

			this.AttributeValue = avDTO;
		}

	}	
}