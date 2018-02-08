using Kadastr.CommonUtils;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Models;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
    public class OperationRestrictionController : Controller
    {
		protected OperationRules operationRules
		{
			get { return Session["OperationRules"] as OperationRules; }
			set { Session["OperationRules"] = value; }
		}

		public ActionResult Edit(string sOperationId) {
			int operationId = 0;
			if (!Int32.TryParse(sOperationId, out operationId))
				throw new InvalidOperationException();

			if (operationRules == null)
			{
				operationRules = new OperationRules { State = ObjectStates.New, IdOperation = operationId };
				
			}
			var operationRepository = ObjectFactory.GetInstance<IOperationTypeRepository>();
			clsOperationType operationType = operationRepository.GetById(operationId);
			OperationRulesStruct operationRulesStruct = operationRules.GetRulesStruct(operationType.EntityTypeGuid);
			OperationRestrictionDTO operationRestriction = new OperationRestrictionDTO(operationRulesStruct);

			#region Атрибуты для фильтра
			var entityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
			clsEntityType entityType = entityTypeRepository.GetByGuid(operationType.EntityTypeGuid);
			Dictionary<long, string> filteringAttributes = new Dictionary<long, string>();
			filteringAttributes.Add(0, "Выберите атрибут");
			foreach (clsAttribute attribute in entityType.colActiveAttributes)
			{
				if (IsTrueType(attribute.AttributeDataType.enDataType))
				{
					filteringAttributes.Add(attribute.Id, attribute.sName);
				}
			}
			ViewData["FilteringAttributes"] = filteringAttributes;
			#endregion	

			if (operationRules.State == ObjectStates.New)
			{
				ViewBag.EditingStat = "создание";
			}
			else {
				ViewBag.EditingStat = "редактирование";
			}

			return View("Edit", operationRestriction);
		}

		[ValidateInput(false)]
		public ActionResult Save(OperationRestrictionDTO result)
		{
			OperationRulesStruct recievedStruct = result.GetStruct();
			operationRules.SetByStruсt(recievedStruct);

			return View("Save");
		}

		private bool IsTrueType(DataType type) 
		{
			if (type == DataType.Date ||
				type == DataType.Integer ||
				type == DataType.Logical ||
				type == DataType.Numeric ||
				type == DataType.Symbol)
				return true;

			return false;
		}
    }
}
