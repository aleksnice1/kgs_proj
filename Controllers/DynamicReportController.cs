using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using Newtonsoft.Json;
using Kadastr.DomainModel;
using Kadastr.DataAccessLayer.Helpers;
using Microsoft.Reporting.WebForms;
using Kadastr.WebApp.Code;
using System.Configuration;
using System.Text;
using System.Data.Common;
using System.Web.Configuration;
using Kadastr.DomainModel.ReportSettings;

namespace Kadastr.WebApp.Controllers
{
	public class DynamicReportController : Controller
	{
		public byte[] Buffer
		{
			get { return (byte[])Session["Buffer"]; }
			set { Session["Buffer"] = value; }
		}

		public string MimeType
		{
			get { return (string)Session["MimeType"]; }
			set { Session["MimeType"] = value; }
		}

		public clsReport DynamicReport
		{
			get { return (clsReport)Session["DynamicReport"]; }
			set { Session["DynamicReport"] = value; }
		}

		public string FileDownloadName
		{
			get { return (string)Session["FileDownloadName"]; }
			set { Session["FileDownloadName"] = value; }
		}

		public ActionResult Report(int id)
		{
			var reportRepository = ObjectFactory.GetInstance<IReportRepository>();
			DynamicReport = reportRepository.GetReport(id);

			IEnumerable<ParameterField> dynamicParams = DynamicReport.ReportSettings.Report.ParameterFields
				.FindAll(p => p.IsDynamic);

			IEnumerable<StaticParamsItem> staticParams = DynamicReport.ReportSettings.Report.ParameterFields
				.FindAll(p => !p.IsDynamic)
				.Select(item => new StaticParamsItem() {
					Name = item.Name,
					DataType = item.DataType,
					PromptText = item.PromptText,
					IsMultiSelect = item.IsMultiSelect
				});

			var viewRepository = ObjectFactory.GetInstance<IEntityViewSettingRepository>();
			var entityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();

			List<DynamicParamsItem> modelParams = new List<DynamicParamsItem>();

			foreach (ParameterField parameter in dynamicParams)
			{
				var items = parameter.Class.Items
					.Select(p => p.IdView.HasValue ? (int)p.IdView.Value : 0)
					.ToList();

				List<SelectListItem> selectList = new List<SelectListItem>();				

				foreach (int idView in items)
				{
					if (idView == 0)
					{
						// TODO: здесь можно подставлять представление по умолчанию для данного типа
						continue;
					}
					clsEntityViewSetting viewSettings = viewRepository.GetById(idView);
					clsEntityType entityType = entityTypeRepository.GetByGuid(viewSettings.GuidEntityType);
					selectList.Add(new SelectListItem()
					{
						Selected = false,
						Text = entityType.sName,
						Value = idView.ToString()
					});
				}

				if (selectList.Count > 0)
				{
					selectList[0].Selected = true; 
				}				

				modelParams.Add(new DynamicParamsItem()	{ 
					Name = parameter.Name, 
					Values = selectList.AsEnumerable(),
					ClassIdView = parameter.Class.IdView,
					DataType = parameter.DataType,
					PromptText = parameter.PromptText
				});
			}

			EntityFilterModel model = new EntityFilterModel()
			{
				DynamicParams = modelParams,
				StaticParams = staticParams,
				ExportFormats = GetReportFormatsList(),
				ReportName = DynamicReport.sName
			};

			return View("Entity", model);
		}

		public string EntityReportFormat(string sfav, string format, string reportName)
		{

			int viewId = 5;

			bool success = true;
			string message = string.Empty;
			var viewRepository = ObjectFactory.GetInstance<IEntityViewSettingRepository>();

			FilterAttributeValue[] FAV = GetFAV(sfav);
			clsEntityViewSetting EVS = viewRepository.GetById(viewId);

			if (!string.IsNullOrEmpty(reportName))
			{
				FileDownloadName = reportName;
			}
			else
			{
				throw new ArgumentNullException("Не задано имя экспортируемого файла");
			}

			try
			{
				DynamicReportsHelper reportHelper = new DynamicReportsHelper();
				string query = reportHelper.GetEntityFilteringQuery(EVS, FAV);

				Buffer = GenerateReport(query, format);
			}
			catch (Exception ex)
			{
				success = false;
				message = ex.Message;
			}

			var response = new
			{
				success = success,
				message = message
			};
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(response);
		}

		private byte[] GenerateReport(string query, string format)
		{
			ServerReport serverReport = new ServerReport();
			ReportHelper reportHelper = new ReportHelper();
			reportHelper.ExecuteServerReport(serverReport, DynamicReport);

			Warning[] warnings;
			string[] streamids;
			string mimeType;
			string encoding;
			string extension;

			byte[] bytes = serverReport.Render(
			   format, null, out mimeType, out encoding, // Excel PDF WORD IMAGE RPL
				out extension,
			   out streamids, out warnings);
			MimeType = mimeType;
			FileDownloadName = string.Format("{0}.{1}", FileDownloadName, extension);
			return bytes;
		}

		private FilterAttributeValue[] GetFAV(string sfav)
		{
			FilterAttributeValue[] result = new FilterAttributeValue[0];

			Newtonsoft.Json.Linq.JArray array = Newtonsoft.Json.Linq.JArray.Parse(sfav);
			var attributeRepository = ObjectFactory.GetInstance<IAttributeRepository>();
			foreach (Newtonsoft.Json.Linq.JObject obj in array)
			{
				string buf = obj.ToString();
				ClientFav cfav = JsonConvert.DeserializeObject<ClientFav>(buf);

				clsAttribute attribute = attributeRepository.GetById(cfav.attribute);
				LogicalExpression filterLogicExpression = (LogicalExpression)cfav.expression;
				string filterValue = cfav.value;
				bool isNeedAttrInFilter = true;


				if (!string.IsNullOrEmpty(filterValue) && !string.IsNullOrWhiteSpace(filterValue)
					|| attribute.AttributeDataType.enDataType == DataType.Logical)
				{
					FilterAttributeValue fav = new FilterAttributeValue(attribute, filterLogicExpression, filterValue, isNeedAttrInFilter);
					Array.Resize(ref result, result.Length + 1);
					result[result.Length - 1] = fav;
				}
			};

			return result;
		}

		public ActionResult ReportSave()
		{
			return File(Buffer, MimeType, FileDownloadName);
		}

		private List<SelectListItem> GetReportFormatsList()
		{
			List<SelectListItem> result = new List<SelectListItem>();
			foreach (string key in ReportMimeType.Keys)
			{
				result.Add(new SelectListItem()
				{
					Selected = false,
					Text = ReportMimeType[key],
					Value = key
				});
			}
			return result;
		}

		private Dictionary<string, string> ReportMimeType = new Dictionary<string, string>()
		{
			{ "Excel", ".xls" },
			{ "PDF" , ".pdf" },
			{ "WORD", ".doc" }
		};

		public ActionResult MyAction(string idView, string name, string viewModel)
		{
			EntityFilterModel model = JsonConvert.DeserializeObject<EntityFilterModel>(viewModel);

			int intValue;
			if (int.TryParse(idView, out intValue))
			{
				var modelItem = model.DynamicParams.FirstOrDefault(p => p.Name == name);
				if (modelItem != null)
				{
					foreach (var item in modelItem.Values)
					{
						item.Selected = item.Value == idView;
					}
				}			
				return View("Entity", model);
			}

			return new EmptyResult();
		}
	}

	/// <summary>
	/// Класс-модель для типизированного представления
	/// </summary>
	public class EntityFilterModel
	{
		/// <summary>
		/// Наименование отчета
		/// </summary>
		public string ReportName { get; set; }

		/// <summary>
		/// Статические параметры отчета
		/// </summary>
		public IEnumerable<StaticParamsItem> StaticParams { get; set; }

		/// <summary>
		/// Динамические параметры очтета
		/// </summary>
		public IEnumerable<DynamicParamsItem> DynamicParams { get; set; }		

		/// <summary>
		/// Список расширений файлов, в которые можно экспортировать отчет
		/// </summary>
		public IEnumerable<SelectListItem> ExportFormats { get; set; }
	}

	/// <summary>
	/// Динамический параметр
	/// </summary>
	public class DynamicParamsItem : StaticParamsItem
	{
		/// <summary>
		/// Список типов для фильтра типа
		/// </summary>
		public IEnumerable<SelectListItem> Values { get; set; }

		/// <summary>
		/// Идентификатор представления для фильтра класса
		/// </summary>
		public long ClassIdView { get; set; }
	}

	/// <summary>
	/// Статический параметр
	/// </summary>
	public class StaticParamsItem
	{
		/// <summary>
		/// Уникальное в пределах отчета наименование параметра, 
		/// совпадает с именем параметра в хранимой процедуре
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Тип данных параметра
		/// </summary>
		public DataType DataType { get; set; }

		/// <summary>
		/// Отображаемое в разметке имя
		/// </summary>
		public string PromptText { get; set; }

		public bool IsMultiSelect { get; set; }
	}

	struct ClientFav
	{
		public long attribute { get; set; }
		public int expression { get; set; }
		public string value { get; set; }
		//public string?	addfield	{ get; set; }
		public long rowId { get; set; }
	}
}
