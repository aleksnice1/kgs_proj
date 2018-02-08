using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Code;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Kadastr.WebApp.Controllers
{
    public class CalculationArendaController : Controller
    {
		private clsDocument oEditedDocument
		{
			get
			{
				return Session["ArendaCalculationDocument"] as clsDocument;
			}
			set
			{
				Session["ArendaCalculationDocument"] = value;
			}
		}
		/// <summary>
		/// Начальная страница
		/// </summary>
		/// <returns></returns>
		public ActionResult Index(long? id)
        {
			ViewBag.Id = id;
			return View();
        }

		public ActionResult GetPartial(long? id) {
			var documentRepository = ObjectFactory.GetInstance<IDocumentRepository>();
			oEditedDocument = documentRepository.GetDocument(id);
			Session[SessionViewstateConstants.ArendaDocument] = oEditedDocument;

			return View("Partial");
		}
    }

	abstract public class MVCGridModel
	{
		/// <summary>
		/// Список имен и id колонок таблицы
		/// </summary>
		/// <returns></returns>
		abstract public List<object> ColumnNames();
		protected string JsonForJqgrid(DataTable data, string sidx, string sord, int page, int rows, string npage)
		{
			int sortRowNum;
			if (!Int32.TryParse(sidx, out sortRowNum))
				sortRowNum = 0;

			EnumerableRowCollection<DataRow> query;

			if (sord == "asc")
				query = from dat in data.AsEnumerable()
						orderby dat.Field<object>(sortRowNum)
						select dat;
			else
				query = from dat in data.AsEnumerable()
						orderby dat.Field<object>(sortRowNum) descending
						select dat;

			return JsonForJqgrid(query.AsDataView().ToTable(), rows, data.Rows.Count, page);
		}
		protected string JsonForJqgrid(DataTable dt, int pageSize, long totalRecords, int page)
		{
			int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
			StringBuilder jsonBuilder = new StringBuilder();
			jsonBuilder.Append("{");
			jsonBuilder.Append("\"total\":" + totalPages + ",\"page\":" + page + ",\"records\":" + (totalRecords) + ",\"rows\"");
			jsonBuilder.Append(":[");
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				if ((i >= (page - 1) * pageSize) && (i < page * pageSize))
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
			}
			if (dt.Rows.Count > 0)
				jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
			jsonBuilder.Append("]");
			jsonBuilder.Append("}");

			string watch = jsonBuilder.ToString();
			return jsonBuilder.ToString();
		}
		protected string TransactionTypesJson(List<clsTransaction> transactions) {
			bool hasData = false;
			List<long> annihilated = new List<long>(), changed = new List<long>(), userEdited = new List<long>();
			foreach (clsTransaction transact in transactions)
			{
				if (transact.IsAnnihilated)
					annihilated.Add(transact.Id);
				if (transact.IsChange)
					changed.Add(transact.Id);
				if (transact.IsUserEdited)
					userEdited.Add(transact.Id);
			}

			if (annihilated.Count > 0 || changed.Count > 0 || userEdited.Count > 0)
				hasData = true;

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(new
			{
				hasData = hasData,
				Annihilated = annihilated,
				Changed = changed,
				UserEdited = userEdited
			});
		}
	}
}
