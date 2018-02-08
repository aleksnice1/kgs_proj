using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Code.Helpers.UIHelpers;
using StructureMap;
using Kadastr.CommonUtils;

namespace Kadastr.WebApp.Controllers
{
	public class BookmarkEditorController : Controller
    {
		private clsEntityType oParentEntityType
		{
			get { return (clsEntityType)Session["EntityType"]; }
		}
		private clsBookmarks oBookmark
		{
			get { return (clsBookmarks)Session["Bookmark"]; }
		}

		private string GetBookmarkType(clsBookmarks bmark)
		{
			if (bmark.IdReport != null)
			{
				return "ReportBookmark";
			}
			else if (bmark.IsFilterClass)
			{
				return "ClassBookmark";
			}
			else if (!string.IsNullOrEmpty(bmark.StoreProcedure))
			{
				return "StoredProcedureBookmark";
			}
			else
			{
				return "EntityBookmark";
			}
		}
		
		public ActionResult Index()
        {
			string bookmarkType = GetBookmarkType(oBookmark);
			if (oBookmark.State == ObjectStates.New)
				ViewBag.Title = "Создание закладки";
			else
				ViewBag.Title = "Редактирование закладки";
			return View((object)bookmarkType);
        }

		#region контроллеры вьюх частичных представлений вкладок

		public ActionResult EntityViewBookmark()
		{
			BookmarkEntityView newBookmark = new BookmarkEntityView();
			if ((oBookmark.State != ObjectStates.New) && (GetBookmarkType(oBookmark) == "EntityBookmark"))
			{
				newBookmark.Name = oBookmark.sName;
				newBookmark.Type = BookmarkType.EntityBookmark;
				newBookmark.IdEntityType = (int)oBookmark.IdEntityType;
				newBookmark.GuidEntitySubtype = oBookmark.GuidEntityAttr;
				newBookmark.IdAttribute = (int)oBookmark.IdAttribute;
				newBookmark.IdView = (int)oBookmark.IdEntityView;
				newBookmark.WithHierarchy = oBookmark.WithHierarchy;
				newBookmark.IdHierarchyAttribute = oBookmark.IdHierarchyAttributeCategory;
				newBookmark.IsNew = false;
			}
			else
			{
				newBookmark.IsNew = true;
			};
			ViewBag.HierarchyAttributes = HierarchyAttributes(oParentEntityType, newBookmark.IdHierarchyAttribute);
			return View("EntityViewBookmark", newBookmark);
		}

		public ActionResult ClassViewBookmark()
		{
			BookmarkClassView newBookmark = new BookmarkClassView();
			if ((oBookmark.State != ObjectStates.New) && (GetBookmarkType(oBookmark) == "ClassBookmark"))
			{
				newBookmark.Name = oBookmark.sName;
				newBookmark.Type = BookmarkType.ClassBookmark;
				newBookmark.IdEntityType = (int)oBookmark.IdEntityType;

				newBookmark.GuidEntitySubtypes = new List<Guid>(oBookmark.ColEntityType);
				newBookmark.IdClassCategory = (int)oBookmark.IdCategory;
				newBookmark.IdView = (int)oBookmark.IdClassView;
				newBookmark.WithHierarchy = oBookmark.WithHierarchy;
				newBookmark.IdHierarchyAttribute = oBookmark.IdHierarchyAttributeCategory;
				newBookmark.IsNew = false;
			}
			else{
				newBookmark.IsNew = true;
			};

			ViewBag.HierarchyAttributes = HierarchyAttributes(oParentEntityType, newBookmark.IdHierarchyAttribute);
			return View("ClassViewBookmark", newBookmark);
		}

		public ActionResult StoredProcedureViewBookmark()
		{
			BookmarkStoredProcedure newBookmark = new BookmarkStoredProcedure();
			if ((oBookmark.State != ObjectStates.New) && (GetBookmarkType(oBookmark) == "StoredProcedureBookmark"))
			{
				newBookmark.Name = oBookmark.sName;
				newBookmark.Type = BookmarkType.ReportBookmark;
				newBookmark.ProcedureName = oBookmark.StoreProcedure;
				newBookmark.IsNew = false;
			}
			else
			{
				newBookmark.IsNew = true;
			};
			return View("StoredProcedureViewBookmark", newBookmark);
		}

		public ActionResult ReportViewBookmark()
		{
			BookmarkReport newBookmark = new BookmarkReport();
			if ((oBookmark.State != ObjectStates.New) && (GetBookmarkType(oBookmark) == "ReportBookmark"))
			{
				newBookmark.Name = oBookmark.sName;
				newBookmark.Type = BookmarkType.ReportBookmark;
				newBookmark.IdReport = (int)oBookmark.IdReport;
				newBookmark.IsNew = false;
			}
			else
			{
				newBookmark.IsNew = true;
			};
			return View("ReportViewBookmark", newBookmark);
		}

		#endregion

		#region сохранение закладок
		[HttpPost]
		public ActionResult SaveEntityBookmarkAct(BookmarkEntityView newBookmark)
		{
			newBookmark.Type = BookmarkType.EntityBookmark;
			if(SaveBookmark(newBookmark))
				return View("SaveBookmarkSuccess", (object)newBookmark.Name);
			else 
				return View("SaveBookmarkDenied", (object)newBookmark.Name);
		}		

		[HttpPost]
		public ActionResult SaveClassBookmarkAct(BookmarkClassView newBookmark)
		{
			newBookmark.Type = BookmarkType.ClassBookmark;
			if (SaveBookmark(newBookmark))
				return View("SaveBookmarkSuccess", (object)newBookmark.Name);
			else
				return View("SaveBookmarkDenied", (object)newBookmark.Name);
		}		

		[HttpPost]
		public ActionResult SaveStoredProcedureBookmarkAct(BookmarkStoredProcedure newBookmark)
		{
			newBookmark.Type = BookmarkType.StoredProcedureBookmark;
			if (SaveBookmark(newBookmark))
				return View("SaveBookmarkSuccess", (object)newBookmark.Name);
			else
				return View("SaveBookmarkDenied", (object)newBookmark.Name);
		}

		[HttpPost]
		public ActionResult SaveReportBookmarkAct(BookmarkReport newBookmark)
		{
			newBookmark.Type = BookmarkType.ReportBookmark;
			if (SaveBookmark(newBookmark))
				return View("SaveBookmarkSuccess", (object)newBookmark.Name);
			else
				return View("SaveBookmarkDenied", (object)newBookmark.Name);
		}

		private bool SaveBookmark(BookmarkEntityView bookmark)
		{
			oBookmark.State = ObjectStates.Dirty;
			oBookmark.sName = bookmark.Name;
			oBookmark.StoreProcedure = string.Empty;
			oBookmark.ColEntityType.Clear();
			oBookmark.IdAttribute = bookmark.IdAttribute;
			oBookmark.IdEntityView = bookmark.IdView;
			oBookmark.IdCategory = null;
			oBookmark.IdClassView = null;
			oBookmark.IdEntityType = bookmark.IdEntityType;
			oBookmark.IdReport = null;
			oBookmark.GuidEntityAttr = bookmark.GuidEntitySubtype;
			oBookmark.IdHierarchyAttributeCategory = bookmark.WithHierarchy ? bookmark.IdHierarchyAttribute : null;

			if (oParentEntityType.FindBookmark(oBookmark.TempGuid) == null)
				oParentEntityType.AddBookmark(oBookmark);

			Session["Bookmark"] = oBookmark;
			Session["EntityType"] = oParentEntityType;

			return true;
		}

		private bool SaveBookmark(BookmarkClassView bookmark)
		{
			oBookmark.State = ObjectStates.Dirty;
			oBookmark.sName = bookmark.Name;
			oBookmark.StoreProcedure = null;
			oBookmark.ColEntityType.Clear();
			oBookmark.IdAttribute = null;
			oBookmark.IdEntityView = null;
			oBookmark.IdCategory = bookmark.IdClassCategory;
			oBookmark.IdClassView = bookmark.IdView;
			oBookmark.IdEntityType = bookmark.IdEntityType;
			oBookmark.IdReport = null;
			oBookmark.GuidEntityAttr = bookmark.GuidEntitySubtypes[0];
			oBookmark.IsFilterClass = true;
			oBookmark.ColEntityType = new List<Guid>(bookmark.GuidEntitySubtypes);
			oBookmark.IdHierarchyAttributeCategory = bookmark.WithHierarchy ? bookmark.IdHierarchyAttribute : null;

			if (oParentEntityType.FindBookmark(oBookmark.TempGuid) == null)
				oParentEntityType.AddBookmark(oBookmark);

			Session["Bookmark"] = oBookmark;
			Session["EntityType"] = oParentEntityType;

			return true;
		}

		private bool SaveBookmark(BookmarkStoredProcedure bookmark)
		{
			oBookmark.State = ObjectStates.Dirty;
			oBookmark.sName = bookmark.Name;
			oBookmark.StoreProcedure = bookmark.ProcedureName;
			oBookmark.ColEntityType.Clear();
			oBookmark.IdAttribute = null;
			oBookmark.IdEntityView = null;
			oBookmark.IdCategory = null;
			oBookmark.IdClassView = null;
			oBookmark.IdEntityType = null;
			oBookmark.IdReport = null;
			oBookmark.GuidEntityAttr = Guid.Empty;
			oBookmark.IsFilterClass = false;
			oBookmark.ColEntityType = null;

			if (oParentEntityType.FindBookmark(oBookmark.TempGuid) == null)
				oParentEntityType.AddBookmark(oBookmark);

			Session["Bookmark"] = oBookmark;
			Session["EntityType"] = oParentEntityType;

			return true;
		}

		private bool SaveBookmark(BookmarkReport bookmark)
		{
			oBookmark.State = ObjectStates.Dirty;
			oBookmark.sName = bookmark.Name;
			oBookmark.StoreProcedure = string.Empty;
			oBookmark.ColEntityType.Clear();
			oBookmark.IdAttribute = null;
			oBookmark.IdEntityView = null;
			oBookmark.IdCategory = null;
			oBookmark.IdClassView = null;
			oBookmark.IdEntityType = null;
			oBookmark.IdReport = bookmark.IdReport;
			oBookmark.GuidEntityAttr = Guid.Empty;
			oBookmark.IsFilterClass = false;
			oBookmark.ColEntityType = null;

			if (oParentEntityType.FindBookmark(oBookmark.TempGuid) == null)
				oParentEntityType.AddBookmark(oBookmark);

			Session["Bookmark"] = oBookmark;
			Session["EntityType"] = oParentEntityType;

			return true;
		}

		#endregion

		#region контроллеры получения списка параметров атрибутов 

		public string EntityTypes()
		{
			List<SelectListItem> entityTypesList = EntitySelectsHelper.EntityTypes();

			if (oBookmark.State != ObjectStates.New)
			{
				entityTypesList.Find(p => p.Value == Convert.ToString((int)oBookmark.EntityType.GetEnumEntityType())).Selected = true;
			}

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(entityTypesList);
		}

		public string EntitySubtypes(int entityTypeEnum)
		{
			if (entityTypeEnum == 0)
				throw new ArgumentException("Неверный тип сущности");

			List<SelectListItem> entitySubtypesList = EntitySelectsHelper.EntitySubtypes((enEntityType)entityTypeEnum);
			Guid entityTypeGuid;
			foreach (SelectListItem item in entitySubtypesList)
				if	((oBookmark.State != ObjectStates.New) && Guid.TryParse(item.Value, out entityTypeGuid) &&
						(
							(oBookmark.ColEntityType.Exists(t => t == entityTypeGuid)) ||
							(oBookmark.GuidEntityAttr == entityTypeGuid)
						)
					)
					item.Selected = true;
			
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(entitySubtypesList);
		}

		public string EntitySubtypeAttributes(string entitySubtypeGUID)
		{
			if (string.Equals(entitySubtypeGUID, Guid.Empty.ToString()))
				throw new ArgumentException("Неверный GUID подтипа сущности");

			Guid sybtypeGuid = new Guid();
			if (entitySubtypeGUID != "null")
				sybtypeGuid = new Guid(entitySubtypeGUID);
			List<SelectListItem> entityAttributes = EntitySelectsHelper.EntitySubtypeAttributes(sybtypeGuid);

			string sDataTypeCode = getDataTypeCode(oParentEntityType);
			foreach (SelectListItem item in entityAttributes)
				if	(	
						(oBookmark.State != ObjectStates.New) &&
						(item.Value == oBookmark.IdAttribute.ToString())
					)
					item.Selected = true;

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(entityAttributes);
		}

		public string EntitySubtypeViews(string entitySubtypeGUID)
		{
			if (string.Equals(entitySubtypeGUID, Guid.Empty.ToString()))
				throw new ArgumentException("Неверный GUID подтипа сущности");

			List<SelectListItem> entityViewsList = EntitySelectsHelper.EntitySubtypeViews(new Guid(entitySubtypeGUID));

			foreach (SelectListItem item in entityViewsList)
				if	(
						(oBookmark.State != ObjectStates.New) && 
						(item.Value == oBookmark.IdEntityView.ToString())
					)
					item.Selected = true;

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(entityViewsList);
		}

		public string ClassCategories(int entityTypeEnum)
		{
			List<SelectListItem> classCategories = new List<SelectListItem>();
			clsAttributeCategory[] attributeCategories = null;
			var oAttributeCategoryRepository = ObjectFactory.GetInstance<IAttributeCategoryRepository>();

			switch (entityTypeEnum)
			{
				case (int)enEntityType.Propertys:
					attributeCategories = oAttributeCategoryRepository.GetAllOrderedAttributeCategory(typeof(clsCategoryPropertys)); break;
				case (int)enEntityType.Contragents:
					attributeCategories = oAttributeCategoryRepository.GetAllOrderedAttributeCategory(typeof(clsCategoryContragents)); break;
				case (int)enEntityType.Documents:
					attributeCategories = oAttributeCategoryRepository.GetAllOrderedAttributeCategory(typeof(clsCategoryDocuments)); break;
			}

			string sDataTypeCode = getDataTypeCode(oParentEntityType);
			classCategories.Add(new SelectListItem() { Text = "--- Выберите класс ---", Value = "" });
			foreach (clsAttributeCategory oAttributeCategory in attributeCategories)
			{
				if (string.Equals(oAttributeCategory.oDataType.sDataTypeCode, sDataTypeCode))
				{
					SelectListItem s = new SelectListItem() { Text = oAttributeCategory.sName, Value = oAttributeCategory.Id.ToString(), Selected = false };
					if ((oBookmark.State != ObjectStates.New) && (oAttributeCategory.Id == oBookmark.IdCategory))
						s.Selected = true;
					classCategories.Add(s);
				}
			}
			classCategories.Sort((m1, m2) => m1.Text.CompareTo(m2.Text));

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(classCategories);
		}

		public string ClassViewes(int entityTypeEnum)
		{
			List<SelectListItem> classViewes = new List<SelectListItem>();

			var oClassViewSettingRep = ObjectFactory.GetInstance<IClassViewSettingsRepository>();
			FilterExpression[] viewFilter = new FilterExpression[1];
			viewFilter[0] = new FilterExpression(FilterPrefix.AND, "IdClassEntity", LogicalExpression.Equal, entityTypeEnum);

			classViewes.Add(new SelectListItem() { Text = "--- Выберите представление ---", Value = "" });
			foreach (clsClassViewSettings oCVS in oClassViewSettingRep.GetAllByFilter(viewFilter))
			{
				SelectListItem s = new SelectListItem() { Text = oCVS.sName, Value = oCVS.Id.ToString(), Selected = false };
				if ((oBookmark.State != ObjectStates.New) && (oCVS.Id == oBookmark.IdClassView))
					s.Selected = true;
				classViewes.Add(s);
			}
			classViewes.Sort((m1, m2) => m1.Text.CompareTo(m2.Text));

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(classViewes);
		}

		public string Reports()
		{
			List<SelectListItem> reportsList = new List<SelectListItem>();
			reportsList.Add(new SelectListItem() { Text = "--- Выберите отчет ---", Value = "" });

			var repository = ObjectFactory.GetInstance<IReportRepository>();
			foreach (clsReport report in repository.GetAllReports())
			{
				SelectListItem s = new SelectListItem() { Text = report.sName, Value = report.Id.ToString(), Selected = false };
				if ((oBookmark.State != ObjectStates.New) && (report.Id == oBookmark.IdReport))
					s.Selected = true;
				reportsList.Add(s);
			};
			reportsList.Sort((m1, m2) => m1.Text.CompareTo(m2.Text));

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(reportsList);
		}

		public string StoredProcedures()
		{
			List<SelectListItem> proceduresList = new List<SelectListItem>();
			proceduresList.Add(new SelectListItem() { Text = "--- Выберите хранимую процедуру ---", Value = "" });

			var repository = ObjectFactory.GetInstance<IBookmarksRepository>();
			foreach (string procedure in repository.GetStoreProcedure())
			{
				SelectListItem s = new SelectListItem() { Text = procedure, Value = procedure, Selected = false };
				if ((oBookmark.State != ObjectStates.New) && (procedure == oBookmark.StoreProcedure))
					s.Selected = true;
				proceduresList.Add(s);
			};
			proceduresList.Sort((m1, m2) => m1.Text.CompareTo(m2.Text));

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(proceduresList);
		}

		public IEnumerable<SelectListItem> HierarchyAttributes(clsEntityType entityType, long? selectedAttributeId)
		{
			return entityType.colActiveAttributes.ToSelectList(clsEntityType.GetDataTypeByEnumEntityType(entityType.GetEnumEntityType()), selectedAttributeId, true); 
		}

		private string getDataTypeCode(clsEntityType EntityType)
		{
			if (EntityType.GetType() == typeof(clsPropertyType))
				return Kadastr.Domain.DataType.ProperyEntity.ToString();
			if (EntityType.GetType() == typeof(clsDocumentType))
				return Kadastr.Domain.DataType.DocumentEntity.ToString();
			if (EntityType.GetType() == typeof(clsContragentType))
				return Kadastr.Domain.DataType.ContragentEntity.ToString();
			return string.Empty;
		}

		#endregion
    }
}
