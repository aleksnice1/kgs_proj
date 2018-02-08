using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
    public class GridViewController : Controller
    {
		private static clsEntityType oEntityType;
		private static clsEntityViewSetting oView;
		
		public ActionResult Index()
        {
			// fast example
			SetupSettings(3, Guid.Parse("5c7d6da4-054f-4dee-b94c-11ac33b2f3f3"), 292);
			// slow example
			//SetupSettings(1, Guid.Parse("3C1DDAC9-B71C-4CBB-AB19-B79AD1CE7FF1"), 485);

			ViewBag.EntityType = GetEntityTypeString(3);


			return View();
        }

		private void SetupSettings(int EntityType, Guid EntitySubtype, int idView)
		{
			var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();

			Type entityType = clsEntityType.GetTypeByEnum((enEntityType)EntityType);
			oEntityType = oEntityTypeRepository.GetByGuid(entityType, EntitySubtype);			

			var oViewSettingRepository = ObjectFactory.GetInstance<IEntityViewSettingRepository>();
			oView = oViewSettingRepository.GetEntityView(idView);
		}

		private string GetEntityTypeString(int EntityType)
		{
			var result = string.Empty;
			switch ((enEntityType)EntityType)
			{
				case enEntityType.Documents: result = "Document"; break;
				case enEntityType.Contragents: result = "Contragent"; break;
				case enEntityType.Propertys: result = "Property"; break;
				default:
					throw new InvalidCastException("Неопределенный тип сущности");
			}
			return result;
		}

		/// <summary>
		/// Возвращает страницу с данными для грида
		/// </summary>
		/// <param name="sidx">id атрибута для сортировки</param>
		/// <param name="sord">направление сортировки</param>
		/// <param name="page">номер страницы</param>
		/// <param name="rows">количество строк на странице</param>
		/// <returns>json c данными: количество страниц, текущая страница, общее количество строк, перечисление данных строк </returns>
		[HttpPost]
		public ActionResult GetGridData(string sidx, string sord, int page, int rows, string npage)
		{
			FilterAttributeValue[] colFilter = null;
			SortAttributeValue[] colSortings = null;
			bool isDeleted = false;
			bool showSumInfo = false;

			int pageCount;

			if (!Int32.TryParse(npage, out pageCount))
				pageCount = 1;

			colSortings = getSortingExpressions(sidx, sord);

			var oBaseDomainEntityRepository = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
			clsResultAllEntity oResultAllEntity = oBaseDomainEntityRepository.GetAllEntities(oEntityType, oView, colFilter, colSortings,
				isDeleted, rows * (page - 1), rows * (page - 1) + rows, showSumInfo);

			return Content(JsonForJqgrid(oResultAllEntity.oEntitys, rows, oResultAllEntity.TotalCountRow, page), "json");
		}

		[HttpPost]
		public JsonResult AttributesNames()
		{
			return Json(GetAttributesNames(oView), JsonRequestBehavior.AllowGet);
	
		}

		private SortAttributeValue[] getSortingExpressions(string sIdAttribute, string sord)
		{

			long idAttribute = 0;
			if (!long.TryParse(sIdAttribute, out idAttribute))
				throw new ArgumentException();

			var oAttrRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			clsAttribute sortAttr = oAttrRepository.GetById(idAttribute);

			SortDirections sortDir;
			if (sord.ToLower() == "asc")
				sortDir = SortDirections.ASC;
			else
				sortDir = SortDirections.DESC;

			SortAttributeValue sa = new SortAttributeValue(sortAttr, sortDir, true);

			return new SortAttributeValue[1] { sa };
		}

		private Dictionary<string, long> GetAttributesNames(clsEntityViewSetting oView)
		{
			Dictionary<string, long> result = new Dictionary<string, long>();

			List<long> IdsAttributes = oView.GetIdAttributesForGrid();

			var oAttrRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			result.Add("Id", 0);
			foreach (long attrId in IdsAttributes)
			{
				clsAttribute Attr = oAttrRepository.GetById(attrId);

				result.Add(Attr.sName, Attr.Id);
			}

			return result;
		}

		private string JsonForJqgrid(DataTable dt, int pageSize, int totalRecords, int page)
		{
			int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
			StringBuilder jsonBuilder = new StringBuilder();
			jsonBuilder.Append("{");
			jsonBuilder.Append("\"total\":" + totalPages + ",\"page\":" + page + ",\"records\":" + (totalRecords) + ",\"rows\"");
			jsonBuilder.Append(":[");
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				jsonBuilder.Append("{\"i\":" + (i) + ",\"cell\":[");
				for (int j = 0; j < dt.Columns.Count; j++)
				{
					jsonBuilder.Append("\"");

					string buf = dt.Rows[i][j].ToString();
					buf = buf.Replace("\"", "'");
					jsonBuilder.Append(buf);


					jsonBuilder.Append("\",");
				}
				jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
				jsonBuilder.Append("]},");
			}
			jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
			jsonBuilder.Append("]");
			jsonBuilder.Append("}");

			string watch = jsonBuilder.ToString();
			return jsonBuilder.ToString();
		}
    }
}
