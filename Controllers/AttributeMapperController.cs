using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Code;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Kadastr.DataAccessLayer.Helpers;
using Kadastr.WebApp.Code.Extensions;


namespace Kadastr.WebApp.Controllers
{
    public class AttributeMapperController : Controller
    {
		/// <summary>
		/// Страница для мапинга из одного типа сущности в другой
		/// </summary>
		/// <param name="firstparam">Guid типа сущности, которую будем мапить</param>
		/// <param name="idEntities">список id сущностей, который будем мапить</param>
		/// <param name="guidTypeTo">Guid типа сущности, в который будем мапить</param>
		/// <param name="idEntityType">id класса сушности (документ, контрагент, имущество)</param>
		public ActionResult TypeConvert(string firstparam, string idEntities, string guidTypeTo, string idEntityType, string operationTypeId)
        {		
			string guidTypeFrom = firstparam;

			Guid guidEntityFrom;
			Guid guidEntityTo;
			int idEntityClass;

			#region prepare arguments
			if (!Guid.TryParse(guidTypeFrom, out guidEntityFrom))
				throw new ArgumentException("Неверный guid сущности");

			if (!Guid.TryParse(guidTypeTo, out guidEntityTo))
				guidEntityTo = Guid.Empty;

			if (!Int32.TryParse(idEntityType, out idEntityClass))
				throw new ArgumentException("Неверный id категории");
			
			#endregion

			var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
			
			Type entityType = clsEntityType.GetTypeByEnum((enEntityType)idEntityClass);
			clsEntityType entityTypeFrom = oEntityTypeRepository.GetByGuid(entityType, guidEntityFrom);

			List<SelectListItem> enabledEntitySubtypes = GetEnabledEntitySubtypes(entityTypeFrom, guidEntityTo);

			ViewBag.SourceConvert = "Type";
			ViewBag.Caption = entityTypeFrom.sName;
			ViewBag.SourceIdentity = guidTypeFrom;
			ViewBag.IdEntities = idEntities;
			ViewBag.IdClass = idEntityType;
			ViewBag.OperationTypeId = operationTypeId;

			return View("Index", enabledEntitySubtypes);
        }

		/// <summary>
		/// Страница для мапинга из класса типа сущности в тип
		/// </summary>
		/// <param name="firstparam">id представления класса, и которого будем мапить</param>
		/// <param name="idEntities">список id сущностей, который будем мапить</param>
		/// <param name="guidTypeTo">Guid типа сущности, в который будем мапить. Если незвестен, то "-"</param>
		/// <param name="idEntityType">id класса сушности (документ, контрагент, имущество)</param>
		public ActionResult ClassConvert(string firstparam, string idEntities, string guidTypeTo, string idEntityType)
		{
			string idClassView = firstparam;

			int idClassFrom;
			Guid guidEntityTo;
			int idEntityClass;

			#region prepare arguments
			if (!Int32.TryParse(idClassView, out idClassFrom))
				throw new ArgumentException("Неверный id класса");

			if (!Guid.TryParse(guidTypeTo, out guidEntityTo))
				guidEntityTo = Guid.Empty;

			if (!Int32.TryParse(idEntityType, out idEntityClass))
				throw new ArgumentException("Неверный id категории");

			#endregion

			// доступные типы сущности для мапинга
			Type entityType = clsEntityType.GetTypeByEnum((enEntityType)idEntityClass);
			List<SelectListItem> enabledEntitySubtypes = GetEnabledEntitySubtypes(entityType, Guid.Empty);

			// заголовок страницы (класс сущности + название представления)
			string caption = string.Empty;
			switch ((enEntityType)idEntityClass) {
				case enEntityType.Documents: caption = "Докуметы"; break;
				case enEntityType.Propertys: caption = "Имущество"; break;
				case enEntityType.Contragents: caption = "Контрагенты"; break;
			};
			var oClassViewRepository = ObjectFactory.GetInstance<IClassViewSettingsRepository>();
			clsClassViewSettings viewSetting = oClassViewRepository.GetById(idClassFrom);
			caption += " (" + viewSetting.sName + ")";

			ViewBag.SourceConvert = "Class";
			ViewBag.Caption = caption;
			ViewBag.SourceIdentity = idClassView;
			ViewBag.IdEntities = idEntities;
			ViewBag.IdClass = idEntityType;

			return View("Index", enabledEntitySubtypes);
		}

		private List<SelectListItem> GetEnabledEntitySubtypes(clsEntityType entityType, Guid entityGuidTypeTo)
		{
			
			var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
			clsEntityType[] enabledTypes = oEntityTypeRepository.GetAllEntityTypes(entityType.GetType());

			List<SelectListItem> result = new List<SelectListItem>();
			result.Add(new SelectListItem() { Text = "Выберите тип", Value = Guid.Empty.ToString(), Selected = Guid.Empty == entityGuidTypeTo });
			foreach(clsEntityType enType in enabledTypes)
				if (enType.Id != entityType.Id)
					result.Add(new SelectListItem() { Text = enType.sName, Value = enType.EntityGuid.ToString(), Selected = enType.EntityGuid == entityGuidTypeTo });

			return result;
		}

		private List<SelectListItem> GetEnabledEntitySubtypes(Type entityType, Guid entityGuidTypeTo)
		{

			var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
			clsEntityType[] enabledTypes = oEntityTypeRepository.GetAllEntityTypes(entityType);

			List<SelectListItem> result = new List<SelectListItem>();
			result.Add(new SelectListItem() { Text = "Выберите тип", Value = Guid.Empty.ToString(), Selected = Guid.Empty == entityGuidTypeTo });
			foreach(clsEntityType enType in enabledTypes)
					result.Add(new SelectListItem() { Text = enType.sName, Value = enType.EntityGuid.ToString(), Selected = enType.EntityGuid == entityGuidTypeTo });

			return result;
		}

		public JsonResult GetAttributeMap(string json, string guidEntityTypeFrom, string guidEntityTypeTo, string entitiesId, string idEntityType, int operationTypeId)
		{                 
            JArray dict = (JArray)JsonConvert.DeserializeObject(json);

			Dictionary<long, long> attrDict = new Dictionary<long, long>();
			long key, value; //key - старый атрибут, value - новый атрибут,
			for (int i = 0; i < dict.Count; i += 2)
                if (long.TryParse(dict[i + 1].ToString(), out value) && long.TryParse(dict[i].ToString(), out key))
					attrDict.Add(key, value);

            Guid guidEntityFrom = Guid.Empty;
            Guid guidEntityTo = Guid.Empty;
            int idEntityClass = 0;

            #region prepare arguments
            if (!Guid.TryParse(guidEntityTypeFrom, out guidEntityFrom))
                throw new ArgumentException("Неверный guid сущности");


            if (!Guid.TryParse(guidEntityTypeTo, out guidEntityTo))
                throw new ArgumentException("Неверный guid сущности");


            if (!Int32.TryParse(idEntityType, out idEntityClass))
                throw new ArgumentException("Неверный id категории");

            #endregion

			string nextOperation = string.Empty;

			User curUser = Session.GetCurrentUser();

			try
			{
				TypeChanger changer = new TypeChanger(curUser, operationTypeId);
				changer.ChangeEntity(entitiesId, idEntityClass, guidEntityFrom, guidEntityTo, attrDict);

				nextOperation = GetNextOperationLink(guidEntityTypeTo, entitiesId, idEntityType);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, errorMessage = ex.Message, nextOperation = nextOperation }, JsonRequestBehavior.AllowGet);
			}

			return Json(new { success = true, errorMessage = string.Empty, nextOperation = nextOperation }, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// возвращает атрибуты для мапинга из одного типа сущности в другой
		/// </summary>
		/// <param name="guidFrom">Guid типа сущности, которой меняем тип</param>
		/// <param name="GuidTo">Guid типа сущности, в который идет преобразование</param>
		public JsonResult AttributesForMappingType(string guidFrom, string GuidTo, string IdEntities) 
		{
			Guid guidEntityFrom;
			Guid guidEntityTo;

			if (!(Guid.TryParse(guidFrom, out guidEntityFrom) && 
					Guid.TryParse(GuidTo, out guidEntityTo)))
				throw new ArgumentException("Указаны неверные Guid");

			List<long> entitiesIdList = new List<long>();
			IdEntities.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList()
			 .ForEach(p =>
			 {
				 long entityId;
				 if (long.TryParse(p, out entityId)) entitiesIdList.Add(entityId);
			 });

			List<clsBaseDomainEntity> entities = new List<clsBaseDomainEntity>();

			var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();				
			var oEntityRepository = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
			clsEntityType entityType = oEntityTypeRepository.GetByGuid(guidEntityFrom);
			// получаем редактируемые объекты
			foreach (long entityId in entitiesIdList)
				entities.Add(oEntityRepository.GetByTypeAndId(entityType, entityId));
			

			var oAttrRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			// атрибуты левой части 
			List<clsAttribute> attrListEntityFrom = oAttrRepository.GetListAllAttributes(guidEntityFrom);
			// атрибуты правой части
			List<clsAttribute> attrListEntityTo = oAttrRepository.GetListAllAttributes(guidEntityTo);

			// подготовка ответа
			List<ClientAttribute> attrResponseListEntityFrom = new List<ClientAttribute>();
			bool isNeededAttribute = true;
			clsAttributeValue av = null;
			foreach (clsAttribute attr in attrListEntityFrom)
			{
				// маппим только атрибуты, имеющие значения
				isNeededAttribute = false;
				foreach(clsBaseDomainEntity entity in entities){
					av = entities[0].colAttributeValues.Find(cav => cav.Attribute.Id == attr.Id);
					if(av != null && av.HasValue() ){
						isNeededAttribute = true;
						break;
					}
				};
				
				string attrType;
				if (isNeededAttribute)
				{
					attrType = attr.AttributeDataType.sDataTypeName;
					if (attr.AttributeDataType.enDataType == DataType.Dictionary)
					{
						attrType = string.Concat(attrType, "_", attr.IdDictionary);
					}
					attrResponseListEntityFrom.Add(new ClientAttribute() { Id = attr.Id, Name = attr.sName, Type = attrType });
				}
			}
			List<ClientAttribute> attrResponseListEntityToRequired = new List<ClientAttribute>();
			List<ClientAttribute> attrResponseListEntityToNotrequired = new List<ClientAttribute>();
			foreach(clsAttribute attr in attrListEntityTo){
				string attrType = attr.AttributeDataType.sDataTypeName;
				if(attr.AttributeDataType.enDataType == DataType.Dictionary)
					attrType = string.Concat(attrType, "_", attr.IdDictionary);
				if (attr.IsRequired)
					attrResponseListEntityToRequired.Add(new ClientAttribute() { Id = attr.Id, Name = attr.sName, Type = attrType });
				else
					attrResponseListEntityToNotrequired.Add(new ClientAttribute() { Id = attr.Id, Name = attr.sName, Type = attrType });
			}

			// формирование ответа
			var jsonData = new
			{
				EntityFrom = attrResponseListEntityFrom.ToArray(),
				EntityTo = new {
					total = attrListEntityTo.Count,
					RequiredAttributes = attrResponseListEntityToRequired.ToArray(),
					NotrequiredAttributes = attrResponseListEntityToNotrequired.ToArray()
				}
			};
			
			return Json(jsonData, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// возвращает атрибуты для мапинга из класса в тип сущности
		/// </summary>
		/// <param name="idClassView">id представления класса</param>
		/// <param name="GuidTo">Guid типа сущности</param>
		public JsonResult AttributesForMappingClass(string idClassView, string GuidTo)
		{
			int idClassViewSetting;
			Guid guidEntityTo;

			if (!Guid.TryParse(GuidTo, out guidEntityTo))
				throw new ArgumentException("Указан неверный Guid");

			if (!Int32.TryParse(idClassView, out idClassViewSetting))
				throw new ArgumentException("Указан неверный id категории");			

			// атрибуты левой части 
			var oClassViewRepository = ObjectFactory.GetInstance<IClassViewSettingsRepository>();
			List<clsAttributeCategory> categoriesListClassFrom = oClassViewRepository.GetById(idClassViewSetting).colCategoryList;

			// атрибуты правой части
			var oAttrRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			List<clsAttribute> attrListEntityTo = oAttrRepository.GetListAllAttributes(guidEntityTo);

			// подготовка ответа
			List<ClientAttribute> attrResponseListEntityFrom = new List<ClientAttribute>();
			string attrType;
			foreach (clsAttributeCategory attr in categoriesListClassFrom)
			{
				attrResponseListEntityFrom.Add(new ClientAttribute() { Id = attr.Id, Name = attr.sName, Type = attr.oDataType.sDataTypeName});
			}
			List<ClientAttribute> attrResponseListEntityToRequired = new List<ClientAttribute>();
			List<ClientAttribute> attrResponseListEntityToNotrequired = new List<ClientAttribute>();
			foreach (clsAttribute attr in attrListEntityTo)
			{
				attrType = attr.AttributeDataType.sDataTypeName;
				if (attr.AttributeDataType.enDataType == DataType.Dictionary)
				{
					attrType = string.Concat(attrType, "_", attr.IdDictionary);
				}
				if (attr.IsRequired)
					attrResponseListEntityToRequired.Add(new ClientAttribute() { Id = attr.Id, Name = attr.sName, Type = attrType });
				else
					attrResponseListEntityToNotrequired.Add(new ClientAttribute() { Id = attr.Id, Name = attr.sName, Type = attrType });
			}

			// формирование ответа
			var jsonData = new
			{
				EntityFrom = attrResponseListEntityFrom.ToArray(),
				EntityTo = new
				{
					total = attrListEntityTo.Count,
					RequiredAttributes = attrResponseListEntityToRequired.ToArray(),
					NotrequiredAttributes = attrResponseListEntityToNotrequired.ToArray()
				}
			};

			return Json(jsonData, JsonRequestBehavior.AllowGet);
		}

		private string GetNextOperationLink(string guidEntityTypeTo, string entitiesId, string idEntityType)
		{
			string nextOperation = string.Empty;
			// получаем список Id конкретных сущностей
			List<long> entitiesIdList = new List<long>();
			entitiesId.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList()
				.ForEach(p =>
				{
					long entityId;
					if (long.TryParse(p, out entityId)) entitiesIdList.Add(entityId);
				});

			if (entitiesIdList.Count != 0)
			{
				// Получем тип сущности
				var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
				var entityType = oEntityTypeRepository.GetByGuid(new Guid(guidEntityTypeTo));

				// Получаем операцию редактирования
				var operationTypeRepositoty = ObjectFactory.GetInstance<IOperationTypeRepository>();
				clsOperationType editOperation = operationTypeRepositoty.GetFirstEditOperationForUser(Session.GetCurrentUser(), entityType);

				// Формируем строку ссылки
				string entityName = "";
				int idEt;
				int.TryParse(idEntityType, out idEt);
				switch (idEt)
				{
					case 1:
						entityName = "Property";
						break;
					case 2:
						entityName = "Contragent";
						break;
					case 3:
						entityName = "Document";
						break;
					default:
						throw new InvalidOperationException("Некорректный тип сущности");
				}

				if (editOperation != null)
				{
					var url = Session[SessionViewstateConstants.KadastrRootUrl] as string;
					if (entitiesIdList.Count == 1)
						nextOperation =
							//"http://localhost:40953/PropertyEditor.aspx?OperationID=29&Operation=Edit&IdProperty=1582971";//
							string.Format("{0}{1}Editor.aspx?OperationID={2}&Operation={3}&Id{4}={5}", url, entityName,
										  editOperation.Id, editOperation.KindOperationType.Name, entityName,
										  entitiesIdList[0]);

					if (entitiesIdList.Count > 1)
					{
						var uid = new Random().Next(1000);

						// в сессию для групповой операции
						Session.Add(SessionViewstateConstants.SelectedId + uid.ToString(), "(" + entitiesId.Replace(';', ',') + ")");

						nextOperation = //"http://localhost:40953/PropertyEditor.aspx?OperationID=7&Operation=Edit&uid=759"; //
							string.Format("{0}{1}Editor.aspx?OperationID={2}&Operation={3}&uid={4}", url, entityName,
										  editOperation.Id, editOperation.KindOperationType.Name, uid);
					}
				}
			}
			return nextOperation;
		}
    }

	public class ClientAttribute {
		public string Name;
		public string Type;
		public long Id;
	}
}
