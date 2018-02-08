using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Text;
using System.Data;
using AutoMapper;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Models;
using Kadastr.CommonUtils;
using StructureMap;
using System.Web.UI;

namespace Kadastr.WebApp.Controllers
{
	[Authorize]
	public class FilterUserContextController : Controller
	{
		private const string UsersKey = "UsersForFilter";
		private IUserFilterContextRepository Repository { get; set; }

		public FilterUserContextController()
		{
			Repository = ObjectFactory.GetInstance<IUserFilterContextRepository>();
		}

		/// <summary>
		/// Справочник пользователей
		/// </summary>
		private IEnumerable<User> Users
		{
			get
			{
				var users = Session[UsersKey] as IEnumerable<User>;
				if (users == null)
				{
					users = ObjectFactory.GetInstance<IUserRepository>().GetAllUsers().OrderBy(user => user.sName);
					Session[UsersKey] = users;
				}
				return users;
			}
		}

		public ActionResult List(int? parentId)
		{
			if (parentId == null)
				return RedirectToAction("List", "FilterContext");
			var model = Mapper.Map<FilterContextViewModel>(ObjectFactory.GetInstance<IFilterContextRepository>().GetById(parentId));
			return View(model);
		}

		public ViewResult Edit(int parentId, int? id)
		{
			var model = id.HasValue
							? Mapper.Map<UserContextViewModel>(Repository.GetById(id))
							: new UserContextViewModel { ContextId = parentId };
			model.SetModelForUser(Users);
			return View(model);
		}

		[HttpPost]
		public ActionResult Edit(UserContextViewModel model)
		{
			if (ModelState.IsValid)
			{
				SaveUserFilterContext(model);
				return RedirectToAction("List", "FilterUserContext", new { parentId = model.ContextId });
			}
			else
			{
				model.SetModelForUser(Users);
				return View(model);
			}
		}

		[HttpPost]
		public JsonResult AttributesNames()
		{
			return Json(GetAttributesNames(), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult GetGridData(string sidx, string sord, int page, int rows, string npage)
		{
			var entityType = GetType();
			var view = GetView(entityType);
			bool isDeleted = false;
			bool showSumInfo = false;
			int pageCount;
			if (!int.TryParse(npage, out pageCount))
				pageCount = 1;
			var repo = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
			var allEntities = repo.GetAllEntities(entityType, view, null, null, isDeleted, rows * (page - 1), rows * (page - 1) + rows, showSumInfo);
			var entities = GetEntities(allEntities.oEntitys, entityType.GetEnumEntityType());
			return Content(JsonForJqgrid(entities, rows, allEntities.TotalCountRow, page), "json");
		}

		private IEnumerable<clsBaseDomainEntity> GetEntities(DataTable entitiesDatatable, enEntityType type)
		{
			var ids = entitiesDatatable.Rows
				.OfType<DataRow>()
				.Select(row => (long?)row["Id"]);
			switch (type)
			{
				case enEntityType.Documents:
					var docsRepo = ObjectFactory.GetInstance<IDocumentRepository>();
					return docsRepo.GetByIds(ids);
				case enEntityType.Contragents:
					var caRepo = ObjectFactory.GetInstance<IContragentRepository>();
					return caRepo.GetByIds(ids);
				case enEntityType.Propertys:
					var propsRepo = ObjectFactory.GetInstance<IPropertyRepository>();
					return propsRepo.GetByIds(ids);
				default:
					throw new NotSupportedException("Не поддерживаемый тип сущности!");
			}
		}

		private clsEntityType GetType()
		{
			enEntityType enType;
			if (!Enum.TryParse(Request["enType"], out enType))
				throw new ArgumentException("Недопустимое значение параметра запроса", "enType");
			Guid subtypeGuid;
			if (!Guid.TryParse(Request["subtypeGuid"], out subtypeGuid))
				throw new ArgumentException("Недопустимое значение параметра запроса", "entitySubtypeGuid");
			var entityTypeRepo = ObjectFactory.GetInstance<IEntityTypeRepository>();
			var entityType = clsEntityType.GetTypeByEnum(enType);
			return entityTypeRepo.GetByGuid(entityType, subtypeGuid);
		}

		private clsEntityViewSetting GetView(clsEntityType type)
		{
			int idView;
			var repo = ObjectFactory.GetInstance<IEntityViewSettingRepository>();
			if (int.TryParse(Request["entityViewId"], out idView) && idView > 0)
				return repo.GetEntityView(idView);
			return repo.GetDefaultEntityView(type);
		}

		private string JsonForJqgrid(IEnumerable<clsBaseDomainEntity> entities, int pageSize, int totalRecords, int page)
		{
			var totalPages = GetTotalPages(pageSize, totalRecords);
			var jsonBuilder = new StringBuilder();
			jsonBuilder.Append("{");
			jsonBuilder.Append("\"total\":" + totalPages + ",\"page\":" + page + ",\"records\":" + (totalRecords) + ",\"rows\"");
			jsonBuilder.Append(":[");
			var i = 0;
			if (entities.Any())
				entities.DoForEach(e => jsonBuilder.Append(CreateRow(i++, e.GUID, e.Id, e.sName, e.Type.sName)));
			else
				jsonBuilder.Append(CreateRow(0, null, null, string.Empty, string.Empty));
			jsonBuilder.Length--;
			jsonBuilder.Append("]");
			jsonBuilder.Append("}");
			return jsonBuilder.ToString();
		}

		private int GetTotalPages(int pageSize, int totalRecords)
		{
			return (int)Math.Ceiling((float)totalRecords / (float)pageSize);
		}

		private string CreateRow(int rowIndex, Guid? guid, long? id, string name, string typeName)
		{
			var builder = new StringBuilder();
			builder.Append("{\"i\":" + (rowIndex++) + ",\"cell\":[");
			//GUID:
			builder.Append("\"");
			builder.Append(guid);
			builder.Append("\",");
			//Id:
			builder.Append("\"");
			builder.Append(id);
			builder.Append("\",");
			//sName:
			builder.Append("\"");
			builder.Append(name.Replace("\"", "'"));
			builder.Append("\",");
			//type name:
			builder.Append("\"");
			builder.Append(typeName.Replace("\"", "'"));
			builder.Append("\",");

			builder.Length--;
			builder.Append("]},");
			return builder.ToString();
		}

		private Dictionary<string, long> GetAttributesNames()
		{
			var result = new Dictionary<string, long>();
			result.Add("Id", 0);
			result.Add("Ид", 1);
			result.Add("Имя", 2);
			result.Add("Подтип", 3);
			return result;
		}

		private void SaveUserFilterContext(UserContextViewModel model)
		{
			var userFilterContext = Mapper.Map<UserFilterContext>(model);
			userFilterContext.Entities.Clear();
			userFilterContext.Entities.AddRange(GetEntities(model.EntityGuids));
			if (!userFilterContext.Id.HasValue || userFilterContext.Id == 0)
				userFilterContext.State = ObjectStates.New;
			else
				userFilterContext.State = ObjectStates.Dirty;
			Repository.Save(userFilterContext);
		}

		public IEnumerable<clsBaseDomainEntity> GetEntities(IEnumerable<Guid> guidEntities)
		{
			var result = new List<clsBaseDomainEntity>();
			if (guidEntities != null && guidEntities.Any())
			{
				var repo = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
				result.AddRange(guidEntities.Select(g => repo.GetByGuid(g)));
			}
			return result;
		}


		[HttpPost]
		public RedirectToRouteResult Delete(int parentId, int id)
		{
			var entity = Repository.GetById(id);
			entity.State = ObjectStates.Delete;
			Repository.Save(entity);
			return RedirectToAction("List", new { parentId });
		}

		[OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
		public JsonResult NameAvailable(int UserId)
		{
			var user = Repository.GetById(UserId);
			if (user != null)
				return Json(true, JsonRequestBehavior.AllowGet);
			return Json("Контекст для данного пользователя уже существует", JsonRequestBehavior.AllowGet);
		}
	}
}