using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Helpers;
using Kadastr.DomainModel.Infrastructure; //
using Kadastr.WebApp.Models;
using StructureMap; //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
    public class MVCControlFactoryController : Controller
    {
		public PartialViewResult AttributeValueEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			if (attributeValue == null)
			{
				return UndefinedControl(controlName);
			}

			switch (attributeValue.GetDataType())
			{
				case Kadastr.Domain.DataType.Symbol: return TextControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.Date: return DateControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.Numeric: return NumericControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.Integer: return IntegerControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.Logical: return LogicalControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.Dictionary: return DictionaryControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.MultiDictionary: return DictionaryControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.ContragentEntity: return EntityControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.DocumentEntity: return EntityControlEditor(attributeValue, controlName);
				case Kadastr.Domain.DataType.ProperyEntity: return EntityControlEditor(attributeValue, controlName);

				default: return NotImplemetedControlEditor(attributeValue, controlName);
			}
		}
		public PartialViewResult AttributeValueLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			if (attributeValue == null)
			{
				return UndefinedControl(controlName);
			}

			switch (attributeValue.GetDataType())
			{
				case Kadastr.Domain.DataType.Symbol: return TextControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.Date: return DateControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.Numeric: return NumericControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.Integer: return IntegerControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.Logical: return LogicalControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.Dictionary: return DictionaryControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.MultiDictionary: return DictionaryControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.ContragentEntity: return EntityControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.DocumentEntity: return EntityControlLabel(attributeValue, controlName);
				case Kadastr.Domain.DataType.ProperyEntity: return EntityControlLabel(attributeValue, controlName);

				default: return NotImplemetedControlLabel(attributeValue, controlName);
			}
		}
		// Текст
		public PartialViewResult TextControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("TextControlEditor", attributeValue);
		}
		public PartialViewResult TextControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("TextControlLabel", attributeValue);
		}
		// Дата
		public PartialViewResult DateControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("DateControlEditor", attributeValue);
		}
		public PartialViewResult DateControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("DateControlLabel", attributeValue);
		}
		// Числа
		public PartialViewResult NumericControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("NumericControlEditor", attributeValue);
		}
		public PartialViewResult NumericControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("NumericControlLabel", attributeValue);
		}
		// Целые
		public PartialViewResult IntegerControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("IntegerControlEditor", attributeValue);
		}
		public PartialViewResult IntegerControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("IntegerControlLabel", attributeValue);
		}
		// Логическое 
		public PartialViewResult LogicalControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("LogicalControlEditor", attributeValue);
		}
		public PartialViewResult LogicalControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("LogicalControlLabel", attributeValue);
		}
		// Словарь
		public PartialViewResult DictionaryControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("DictionaryControlEditor", attributeValue);
		}
		public PartialViewResult DictionaryControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("DictionaryControlLabel", attributeValue);
		}
		// Сущность (документ/контрагент/имущество)
		public PartialViewResult EntityControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("EntityControlEditor", attributeValue);
		}
		public PartialViewResult EntityControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("EntityControlLabel", attributeValue);
		}
		// По умолчанию
		public PartialViewResult NotImplemetedControlEditor(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("NotImplemetedControlEditor", attributeValue);
		}
		public PartialViewResult NotImplemetedControlLabel(AttributeValueDTO attributeValue, string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("NotImplemetedControlLabel", attributeValue);
		}
		// Тип не определен
		public PartialViewResult UndefinedControl(string controlName = "undefinedName")
		{
			ViewBag.ControlName = controlName;
			return PartialView("UndefinedControl");
		}

		// Список логический выражений, доступных для атрибута
		public JsonResult LogicalExpressionsForAttribute(string sIdAttribute) 
		{
			long idAttribute;
			if (!Int64.TryParse(sIdAttribute, out idAttribute)) 
			{
				return Json(new { errorMessage = string.Format(Properties.Resources.CanNotConvert, sIdAttribute, "Int64") }, JsonRequestBehavior.AllowGet);
			}

			var attributeRepo = ObjectFactory.GetInstance<IAttributeRepository>();
			
			clsAttribute attribute;
			if (!attributeRepo.TryGetById(idAttribute, out attribute))
			{
				return Json(new { errorMessage = string.Format(Properties.Resources.HeNotFound, "атрибут", "Id = " + sIdAttribute) }, JsonRequestBehavior.AllowGet);
			}

			List<SelectListItem> expressions = new List<SelectListItem>();
			expressions.Add(new SelectListItem(){ 
				Text = EnumHelper.GetDisplayName(LogicalExpression.Equal),
				Value = ((int)LogicalExpression.Equal).ToString()
			});
			expressions.Add(new SelectListItem(){
				Text = EnumHelper.GetDisplayName(LogicalExpression.Nonequal),
				Value = ((int)LogicalExpression.Nonequal).ToString()
			});

			if (attribute.AttributeDataType.enDataType == DataType.Date ||
				attribute.AttributeDataType.enDataType == DataType.Numeric ||
				attribute.AttributeDataType.enDataType == DataType.Integer)
			{
				expressions.Add(new SelectListItem(){
					Text = EnumHelper.GetDisplayName(LogicalExpression.Grater),
					Value = ((int)LogicalExpression.Grater).ToString()
				});
				expressions.Add(new SelectListItem(){
					Text = EnumHelper.GetDisplayName(LogicalExpression.GraterOrEqual),
					Value = ((int)LogicalExpression.GraterOrEqual).ToString()
				});
				expressions.Add(new SelectListItem(){
					Text = EnumHelper.GetDisplayName(LogicalExpression.Less),
					Value = ((int)LogicalExpression.Less).ToString()
				});
				expressions.Add(new SelectListItem(){
					Text = EnumHelper.GetDisplayName(LogicalExpression.LessOrEqual),
					Value = ((int)LogicalExpression.LessOrEqual).ToString()
				});
				expressions.Add(new SelectListItem(){
					Text = EnumHelper.GetDisplayName(LogicalExpression.In),
					Value = ((int)LogicalExpression.In).ToString()
				});
			}

			if (attribute.AttributeDataType.enDataType == DataType.Symbol)
			{
				expressions.Add(new SelectListItem(){
					Text = EnumHelper.GetDisplayName(LogicalExpression.Like),
					Value = ((int)LogicalExpression.Like).ToString()
				});
				expressions.Add(new SelectListItem(){
					Text = EnumHelper.GetDisplayName(LogicalExpression.In),
					Value = ((int)LogicalExpression.In).ToString()
				});
                expressions.Add(new SelectListItem()
                {
                    Text = EnumHelper.GetDisplayName(LogicalExpression.StartWith),
                    Value = ((int)LogicalExpression.StartWith).ToString()
                });
            }

			expressions.Add(new SelectListItem(){
				Text = EnumHelper.GetDisplayName(LogicalExpression.IsNotNull),
				Value = ((int)LogicalExpression.IsNotNull).ToString()
			});
			expressions.Add(new SelectListItem(){
				Text = EnumHelper.GetDisplayName(LogicalExpression.IsNull),
				Value = ((int)LogicalExpression.IsNull).ToString()
			});

			return Json(expressions, JsonRequestBehavior.AllowGet);
		}

		[ValidateInput(false)]
		public PartialViewResult AttributeValueRepresent(string sIdAttribute, string sLogicalExpression, string value, string controlName)
		{
			long idAttribute;
			Int64.TryParse(sIdAttribute, out idAttribute);

			var attributeRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			clsAttribute attribute = attributeRepository.GetById(idAttribute);
			AttributeValueDTO attributeValueDTO = new AttributeValueDTO(attribute);
			if (!string.IsNullOrEmpty(value)) {
				attributeValueDTO.Value = value;
			}

			return AttributeValueEditor(attributeValueDTO, controlName);
		}

	}
}
