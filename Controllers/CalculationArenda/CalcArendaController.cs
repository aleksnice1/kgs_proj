using Kadastr.CommonUtils;
using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.GUI;
using Kadastr.WebApp.Code;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Kadastr.WebApp.Controllers.CalculationArenda
{
	public class CalcArendaController : Controller
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
		
		public ActionResult Index()
		{
			return View();
		}

		#region Кнопки
		/// <summary>
		/// Добавить расчет
		/// </summary>
		/// <returns>строка-url для открытия в модальном окне</returns>
		public string ButtonAddCalculationClick()
		{
			var serializer = new JavaScriptSerializer();
			string sPath = String.Format(
				"WindowCalculationArenda.aspx?{0}={1}&TempGuid={2}&IdDocument={3}",
				SessionViewstateConstants.CalculationType,
				CalcType.New,
				Guid.NewGuid(),
				oEditedDocument.Id.ToString()
			);

			return serializer.Serialize(sPath);
		}
		/// <summary>
		/// Пролонгация договора
		/// </summary>
		/// <param name="ids">список id редактируемых расчетов</param>
		/// <returns>строка-url для открытия в модальном окне</returns>
		public string ButtonProlongationClick(string ids)
		{
			List<long> colSelectedID = (from id in ids.Split(',') select Int64.Parse(id)).ToList<long>();
			Session[SessionViewstateConstants.spIdsForProlongation] = colSelectedID;

			string sPath = String.Format("Prolongation.aspx");

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(sPath);
		}
		/// <summary>
		/// Досрочное расторжение
		/// </summary>
		/// <param name="ids">список id редактируемых расчетов</param>
		/// <returns>строка-url для открытия в модальном окне</returns>
		public string ButtonEarlyDissolutionClick(string ids)
		{
			List<long> colSelectedID = (from id in ids.Split(',') select Int64.Parse(id)).ToList<long>();
			Session[SessionViewstateConstants.spIdsForEarlyDissolution] = colSelectedID;

			string sPath = String.Format("EarlyDissolution.aspx");

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(sPath);
		}
		#endregion
		#region Расчет арендной платы
		CalcArendaGrid calcArendaGrid = new CalcArendaGrid();
		/// <summary>
		/// Имена столбцов для инициализации грида на клиенте
		/// </summary>
		public JsonResult GetCalcGridColumnNames()
		{
			return Json(calcArendaGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}
		/// <summary>
		/// Обработка пэйджинга
		/// </summary>
		public ActionResult GetCalcGridGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(calcArendaGrid.GetSchedulesProperty(sidx, sord, page, rows, npage, oEditedDocument), "json");
		}
		/// <summary>
		/// Обработка события редактирования грида
		/// </summary>
		public ActionResult GetCalcGridGridData_Edit(string oper, string id, string additionalInfo)
		{
			switch (oper)
			{
				// операция удаления
				case "del":
					{
						List<long> rowsIds = new List<long>();		// номера выделенных строк
						List<Guid> calcsGuids = new List<Guid>();	// Guid выделенных строк

						List<string> srowsIds = id.Split(',').ToList();
						foreach (string sid in srowsIds)
							rowsIds.Add(Int64.Parse(sid));
						List<string> scalcsGuids = additionalInfo.Split(',').ToList();	// Guid'ы всех строк
						int indx = 1;
						foreach (string sid in scalcsGuids)
						{
							if (rowsIds.Contains(indx))
								calcsGuids.Add(Guid.Parse(sid));
							indx++;
						}

						// Удаляем расчеты
						if (oEditedDocument.Arenda.SchedulesProperties != null)
							foreach (clsSchedulesProperty oSchedProperty in oEditedDocument.Arenda.SchedulesProperties)
								if (calcsGuids.Contains(oSchedProperty.TempGuid))
								{
									oSchedProperty.StateTemp = ObjectStates.Delete;
									if (oSchedProperty.colSchedDetail != null)
										foreach (clsSchedulesDetail oSchedDetail in oSchedProperty.colSchedDetail)
											oSchedDetail.StateTemp = ObjectStates.Delete;
								}
					};
					break;
			};
			var cdr = ObjectFactory.GetInstance<IDocumentRepository>();
			cdr.SaveDocument(oEditedDocument);

			return Content("true", "json");
		}
		/// <summary>
		/// Содержимое всплывающего сообщения для столбца "Формула"
		/// </summary>
		public string GetFormulaPopup(string idShedule, string tempGuid)
		{
			if (!String.IsNullOrEmpty(idShedule) && !String.IsNullOrEmpty(tempGuid))
			{
				long IdScheduleProperty = Convert.ToInt64(idShedule);
				string TempGuidDoc = Convert.ToString(tempGuid);

				List<clsSchedulesProperty> list = oEditedDocument.Arenda.SchedulesProperties;
				if (list != null)
					foreach (clsSchedulesProperty oSchedProperty in list)
						if (oSchedProperty.Id != 0 && oSchedProperty.Id == IdScheduleProperty)
						{
							return "<table><tr><td>" + oSchedProperty.DescrKoef + "</td></tr></table>";
						}
			}
			return string.Empty;
		}
		/// <summary>
		/// Содержимое всплывающего сообщения для столбца "Сумма"
		/// </summary>
		public string GetSummPopup(string idShedule, string tempGuid)
		{
			string s = string.Empty;
			string style = "style = 'text-align:center;color:#0A3C7B;white-space:nowrap;background:url(Images/pageTitleBKGDnew.gif) repeat-x;font-weight:bold;font-family: verdana;'";

			s += String.Format("<tr><th {0}>Нач.</th><th {0}>Оконч.</th><th {0}>Суммa</th><tr>", style);
			string table = "<table>{0}</table>";

			if (!String.IsNullOrEmpty(idShedule) && !String.IsNullOrEmpty(tempGuid))
			{
				long IdScheduleProperty = Convert.ToInt64(idShedule);
				string TempGuidDoc = Convert.ToString(tempGuid);

				List<clsSchedulesProperty> list = oEditedDocument.Arenda.SchedulesProperties;
				if (list != null)
					foreach (clsSchedulesProperty oSchedProperty in list)
						if (oSchedProperty.Id != 0 && oSchedProperty.Id == IdScheduleProperty)
						{
							foreach (clsSchedulesDetail oSchedDetail in oSchedProperty.GetPredicateSchedDetail)
								s += String.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><tr>", oSchedDetail.DateBegin.ToShortDateString(),
												   oSchedDetail.DateEnd.ToShortDateString(),
												   String.Format("{0:f2}", oSchedDetail.Amount));
							break;
						}
			}
			return String.Format(table, s);
		}
		#endregion
		#region План-факт
		PlanFactArendaGrid planfactArendaGrid = new PlanFactArendaGrid();

		public JsonResult GetPlanFactGridColumnNames()
		{
			return Json(planfactArendaGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetPlanFactGridGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(planfactArendaGrid.GetGridData(sidx, sord, page, rows, npage, oEditedDocument), "json");
		}
		#endregion
	}

	/// <summary>
	/// Грид План/Факт 
	/// </summary>
	public class PlanFactArendaGrid : MVCGridModel
	{
		/// <summary>
		/// Имена колонок
		/// </summary>
		/// <returns></returns>
		public override List<object> ColumnNames()
		{
			List<object> result = new List<object>();

			result.Add(new
			{
				index = 0,
				name = "Год"
			});
			result.Add(new
			{
				index = 1,
				name = "Месяц"
			});
			result.Add(new
			{
				index = 2,
				name = "План"
			});
			result.Add(new
			{
				index = 3,
				name = "Факт"
			});

			return result;
		}

		/// <summary>
		/// Получение данных
		/// </summary>
		/// <param name="sidx">Поле, по которому проводится сортировка</param>
		/// <param name="sord">Направление сортировки</param>
		/// <param name="page">Номер страницы</param>
		/// <param name="rows">Количество строк на странице</param>
		/// <param name="npage"></param>
		/// <param name="oEditedDocument">Редактируемы договор</param>
		/// <returns></returns>
		public string GetGridData(string sidx, string sord, int page, int rows, string npage, clsDocument oEditedDocument)
		{
			List<clsSchedulesProperty> colSchedulesProperty = oEditedDocument.Arenda.ActiveSchedulesProperties;
			var colViewCalcArenda = GetViewCalcArenda(colSchedulesProperty);
			DataTable data = new DataTable();
			data.Columns.Add("Year", typeof(string));
			data.Columns.Add("Month", typeof(string));
			data.Columns.Add("Plan", typeof(decimal));
			data.Columns.Add("Fact", typeof(decimal));

			DataRow dr;
			foreach (var year in colViewCalcArenda) {
				foreach (var month in year.ViewDetailCalcArenda) {
					dr = data.NewRow();
					dr["Year"] = year.Year;
					dr["Month"] = month.MonthName;
					dr["Plan"] = month.SummPlan;
					dr["Fact"] = month.SummFact;
					data.Rows.Add(dr);
				};
			};		

			return JsonForJqgrid(data, sidx, sord, page, rows, npage);
		}
		/// <summary>
		/// Вспомогательный метод для получения данных
		/// </summary>
		private List<clsViewCalcArenda> GetViewCalcArenda(List<clsSchedulesProperty> schedulesProperties)
		{
			List<clsViewCalcArenda> result = new List<clsViewCalcArenda>();
			if (schedulesProperties == null)
				return result;
			result = schedulesProperties
				.SelectMany(sp => sp.colSchedDetail)
				.Where(sd => sd.State != ObjectStates.Delete
					&& sd.StateTemp != ObjectStates.Delete)
				.GroupBy(sd => sd.DateBegin.Year)
				.Select(g => new clsViewCalcArenda(g.Key, g))
				.OrderBy(v => v.YearInt)
				.ToList();
			return result;
		}
	}
	/// <summary>
	/// Грид Расчет арендной платы
	/// </summary>
	public class CalcArendaGrid : MVCGridModel
	{
		public override List<object> ColumnNames()
		{
			/// <summary>
			/// Имена колонок
			/// </summary>
			/// <returns></returns>
			List<object> result = new List<object>();

			result.Add(new
			{
				index = 0,
				name = "Id",
				type = "integer",
				format = new{
					thousandsSeparator = ""
				},
				hidden = true
			});
			result.Add(new
			{
				index = 1,
				name = "Guid",
				type = "string",
				hidden = true
			});
			result.Add(new
			{
				index = 2,
				name = "Начало",
				type = "date",
				format = new
				{
					srcformat = "d-m-Y H:i:s",
					newformat = "d.m.Y"
				},
				width = 50
			});
			result.Add(new
			{
				index = 3,
				name = "Окончание",
				type = "date",
				format = new
				{
					srcformat = "d-m-Y H:i:s",
					newformat = "d.m.Y"
				},
				width = 50
			});
			result.Add(new
			{
				index = 4,
				name = "Площадь",
				width = 50
			});
			result.Add(new
			{
				index = 5,
				name = "Имущество"
			});
			result.Add(new
			{
				index = 6,
				name = "Описание",
				width = 70
			});
			result.Add(new
			{
				index = 7,
				name = "Формула",
				width = 110
			});
			result.Add(new
			{
				index = 8,
				name = "Сумма",
				width = 70
			});

			return result;
		}

		/// <summary>
		/// Получение данных
		/// </summary>
		/// <param name="sidx">Поле, по которому проводится сортировка</param>
		/// <param name="sord">Направление сортировки</param>
		/// <param name="page">Номер страницы</param>
		/// <param name="rows">Количество строк на странице</param>
		/// <param name="npage"></param>
		/// <param name="oEditedDocument">Редактируемы договор</param>
		/// <returns></returns>
		public string GetSchedulesProperty(string sidx, string sord, int page, int rows, string npage, clsDocument oEditedDocument)
		{
			DataTable data = GetAllSchedProperty(oEditedDocument);
			return JsonForJqgrid(data, sidx, sord, page, rows, npage);
		}
		/// <summary>
		/// Вспомогательный метод для получения данных
		/// </summary>
		private DataTable GetAllSchedProperty(clsDocument document)
		{
			try
			{
				var result = new DataTable();
				result.Columns.Add("Id", typeof(Int64));
				result.Columns.Add("TempGuid", typeof(Guid));
				result.Columns.Add("DateBegin", typeof(DateTime));
				result.Columns.Add("DateEnd", typeof(DateTime));
				result.Columns.Add("ArendaArea", typeof(Decimal));
				result.Columns.Add("PropertyName", typeof(String));
				result.Columns.Add("Descr", typeof(String));
				//dt.Columns.Add("DescrKoef", typeof(String));
				result.Columns.Add("sCalculationName", typeof(String));
				result.Columns.Add("Amount", typeof(Decimal));
				result.Columns.Add("BaseDocument", typeof(String));

				//Заполняем _colSchedProperty одним запросом, чтобы не дергать каждый объект 
				//по одиночке из БД
				if (document.Arenda.SchedulesProperties.Count == 0)
				{
					ObjectFactory.GetInstance<ISchedulesDetailRepository>()
						.AddShedulesDetailToDocument(document);
				}

				foreach (clsSchedulesProperty oSchedProperty in document
					.Arenda.ActiveSchedulesProperties)
				{
					if (oSchedProperty.StateTemp != ObjectStates.Delete)
					{
						//получаем отображаемое имя
						//т.к. каждое имущество может быть разным по типу, то находим тип и формируем название 
						//clsProperty oProperty = oPropertyRep.GetProperty(oSchedProperty.IdProperty);

						//сумма по графикам детализации
						decimal dAmount = 0;
						if (oSchedProperty.colSchedDetail != null)
						{
							foreach (var oSchedDetail in oSchedProperty.GetPredicateSchedDetail)
							{
								if ((oSchedDetail.StateTemp != ObjectStates.Delete) || (oSchedDetail.State != ObjectStates.Delete))
								{
									dAmount += oSchedDetail.Amount;
								}
							}
						}
						DataRow dr = result.NewRow();
						if (!string.IsNullOrEmpty(oSchedProperty.DescrKoef))
							oSchedProperty.DescrKoef = oSchedProperty.DescrKoef.Replace("\r\n", "<br>");

						dr["Id"] = oSchedProperty.Id;
						dr["TempGuid"] = oSchedProperty.TempGuid;
						dr["DateBegin"] = oSchedProperty.DateBegin;
						dr["DateEnd"] = oSchedProperty.DateEnd;
						dr["ArendaArea"] = oSchedProperty.ArendaArea;

						//Не понятно для чего бралось значение именно sNameFromAttribute
						//поставил вместо него sName,т.к. sNameFromAttribute при большом количестве 
						//объектов отнимает очень много времени
						//dr["PropertyName"] = oSchedProperty.oProperty.sNameFromAttribute;
						dr["PropertyName"] = oSchedProperty.oProperty.sName;

						dr["Descr"] = oSchedProperty.Descr;
						//dr["DescrKoef"] = oSchedProperty.DescrKoef;

						dr["sCalculationName"] = oSchedProperty.sCalculationName;
						dr["Amount"] = dAmount;
						dr["BaseDocument"] = oSchedProperty.BaseDocument;

						result.Rows.Add(dr);
					}
				}

				return result;
			}
			catch (Exception exc)
			{
				Logger.Error(string.Format("Ошибка возвращения графика, IdDocument={0}",
					document.Id), exc);
				throw;
			}
		}
	}
}
