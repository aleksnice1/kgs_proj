using System.Web.Mvc;
using Kadastr.DataAccessLayer.Helpers;
using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Code.Extensions;
using Kadastr.WebApp.Code.Helpers.UIHelpers;
using StructureMap;
using System.Web.Script.Serialization;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Kadastr.WebApp.Controllers
{
    public class AttributeTypeChangeController : Controller
    {
		public ActionResult ChangeType()
		{
			//типы сущности
			ViewBag.DDLEntityTypes = EntitySelectsHelper.EntityTypes();
			return View("ChangeType");
		}		

		public string ChangeAttributeType(string sIdAttribute, string sDataType) 
		{
			var serializer = new JavaScriptSerializer();
			var result = new ChangeTypeResult();

			long idAttribute;
			int dataType;

			if (!IsAdminUser())
			{
				result.success = false;
				result.message = "У Вас не достаточно прав для смены типа атрибута.";
				return serializer.Serialize(result);
			}

			if (!Int64.TryParse(sIdAttribute, out idAttribute)) {
				result.success = false;
				result.message = string.Format("Не удалось преобразовать sIdAttribute = {0} в тип long.", sIdAttribute);
				return serializer.Serialize(result);
			}

			if (!Int32.TryParse(sDataType, out dataType))
			{
				result.success = false;
				result.message = string.Format("Не удалось преобразовать sDataType = {0} в тип int.", sDataType);
				return serializer.Serialize(result);
			}
			
			try
			{
				var attributesRepository = ObjectFactory.GetInstance<IAttributeRepository>();
				clsAttribute attribute = attributesRepository.GetById(idAttribute);
				AttributeTypeChangeHelper.ChangeType(attribute, DataType.Integer);
			}
			catch (Exception ex)
			{
				result.success = false;
				result.message = ex.Message;
				return serializer.Serialize(result);
			}
			
			return serializer.Serialize(result);
		}

		private bool IsAdminUser()
		{
			User user = Session.GetCurrentUser();
			return user != null && user.IsAdmin();
		}

		#region	Методы для контролов интерфейса	
		/// <summary>
		/// Доступные подтипы сущности
		/// </summary>
		/// <param name="entityType">enum типа сущности</param>
		/// <returns>Сериализованный List из SelectListItem'ов</returns>
		public string EntitySubtypes(int entityType)
		{
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(EntitySelectsHelper.EntitySubtypes((enEntityType)entityType));
		}
		/// <summary>
		/// Доступные атрибуты
		/// </summary>
		/// <param name="sGuid">Guid подтипа сущности</param>
		/// <returns>Сериализованный List из SelectListItem'ов</returns>
		public string EntityAttributes(string sGuid)
		{
			Guid enSubtypeGuid;
			if (!Guid.TryParse(sGuid, out enSubtypeGuid))
			{
				throw new InvalidCastException();
			}

			// Ограничение по типу атрибута
			Dictionary<DataType, DataType> map = AttributeTypeChangeHelper.ConvertionMap;
			List<DataType> attrRestrict = new List<DataType>();
			foreach (KeyValuePair<DataType, DataType> kvp in map)
				attrRestrict.Add(kvp.Key);

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(EntitySelectsHelper.EntitySubtypeAttributes(enSubtypeGuid, attrRestrict));
		}
		/// <summary>
		/// Строковое представление типа выбранного атрибута
		/// </summary>
		/// <param name="idAttribute">Id атрибута</param>
		/// <returns>название типа атрибута</returns>
		public string AttributeType(string idAttribute)
		{
			long idAttr;
			if (!Int64.TryParse(idAttribute, out idAttr))
			{
				throw new InvalidCastException();
			}

			var attributesRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			clsAttribute attribute = attributesRepository.GetById(idAttr);

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(attribute.AttributeDataType.sDataTypeName);
		}
		/// <summary>
		/// Доступные типы, в которые можно конвертировать выбранный атрибут
		/// </summary>
		/// <param name="idAttribute">Id атрибута</param>
		/// <returns>Сериализованный List из SelectListItem'ов</returns>
		public string EnabledTypes(string idAttribute)
		{
			long idAttr;
			if (!Int64.TryParse(idAttribute, out idAttr))
			{
				throw new InvalidCastException();
			}

			var oDataTypeRepository = ObjectFactory.GetInstance<IDataTypeRepository>();
			List<clsDataType> colDataType = new List<clsDataType>(oDataTypeRepository.GetAll());

			var attributeRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			clsAttribute attribute = attributeRepository.GetById(idAttr);

			Dictionary<DataType, DataType> map = AttributeTypeChangeHelper.ConvertionMap;

			List<SelectListItem> types = new List<SelectListItem>();
			types.Add(new SelectListItem()
			{
				Text = "--- Выберите тип ---",
				Value = "0"
			});
			clsDataType datatype;
			foreach (KeyValuePair<DataType, DataType> kvp in map)
				if (kvp.Key == attribute.AttributeDataType.enDataType)
				{
					datatype = colDataType.Find(p => p.enDataType == kvp.Value);
					types.Add(new SelectListItem()
					{
						Text = datatype.sDataTypeName,
						Value = datatype.Id.ToString()
					});
				};

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(types);
		}
		#endregion
	}

	/// <summary>
	/// Класс для ответа с результатом конверитирования типа атрибута
	/// </summary>
	public class ChangeTypeResult {
		public bool success { get; set; }
		public string message { get; set; }

		public ChangeTypeResult() { 
			success = true;
			message = string.Empty;
		}
	}
}
