using Kadastr.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Models
{
	public class OperationRestrictionDTO
	{
		[HiddenInput(DisplayValue = false)]
		public long Id { get; set; }
		[HiddenInput(DisplayValue = false)]
		public int? IdOperation { get; set; }
		public string Formula { get; set; }
		public string ErrorMessage { get; set; }
		public List<OperationRestrictionRuleDTO> Rules { get; set; }
		public OperationRestrictionRuleDTO AddRow{ get; set; }

		public OperationRestrictionDTO() {
			this.Id = 0;
			this.IdOperation = 0;
			this.Rules = new List<OperationRestrictionRuleDTO>();
			this.AddRow = new OperationRestrictionRuleDTO();
		}

		public OperationRestrictionDTO(OperationRulesStruct ruleStruct) : this()
		{
			this.Id = ruleStruct.Id;
			this.Formula = ruleStruct.Formula;
			this.ErrorMessage = ruleStruct.ErrorMessage;
			this.IdOperation = ruleStruct.IdOperation;
			if (ruleStruct.Rules != null) {
				OperationRestrictionRuleDTO ruleDTO;
				foreach (RuleStruct rule in ruleStruct.Rules) {
					ruleDTO = new OperationRestrictionRuleDTO(rule);
					this.Rules.Add(ruleDTO);
				}
			};
		}

		public OperationRulesStruct GetStruct() {
			OperationRulesStruct result = new OperationRulesStruct();

			result.Formula = this.Formula;
			result.ErrorMessage = this.ErrorMessage;
			result.Id = this.Id;
			result.IdOperation = this.IdOperation;
			result.Rules = null;
			if (this.Rules != null) {
				result.Rules = new List<RuleStruct>();
				foreach (OperationRestrictionRuleDTO ruleDTO in this.Rules) {
					result.Rules.Add(ruleDTO.GetStruct());
				}				
			}

			return result;
		}
	}
}