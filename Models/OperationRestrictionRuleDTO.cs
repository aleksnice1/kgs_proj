using Kadastr.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kadastr.WebApp.Models
{
	public class OperationRestrictionRuleDTO
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public FilterAttributeValueDTO Filter { get; set; }

		public OperationRestrictionRuleDTO()
		{
			this.Id = 0;
			this.Filter = new FilterAttributeValueDTO();
		}

		public OperationRestrictionRuleDTO(RuleStruct ruleStruct) : this()
		{
			this.Id = ruleStruct.IdRule;
			this.Name = ruleStruct.Name;
			FilterAttributeValue fav = new FilterAttributeValue();
			this.Filter = new FilterAttributeValueDTO(ruleStruct.IdAttribute, ruleStruct.LogicalExpression, ruleStruct.Value);
		}

		public RuleStruct GetStruct() {
			RuleStruct result = new RuleStruct();
			result.Name = this.Name;
			result.IdRule = this.Id;
			result.IdAttribute = (int)this.Filter.AttributeId;
			result.LogicalExpression = this.Filter.LogicalExpression;
			if (this.Filter != null && this.Filter.AttributeValue != null)
			{
				result.Value = clsAttributeValue.ReadXML(this.Filter.AttributeValue.Value);
			}
			else 
			{
				result.Value = null;
			}			
			return result;
		}
	}
}