using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.MovementMoney;
using Kadastr.WebApp.Controllers.CalculationArenda;
using Kadastr.CommonUtils;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Text;

namespace Kadastr.WebApp.Controllers
{
    public class ChargePenaltyController : Controller
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
		private List<clsDocumentArendaPeniSettings> ActivePeniSettings
		{
			get
			{
				return oEditedDocument.Arenda.ArendaPeniSettings
					.Where(s => s.State != ObjectStates.Delete && s.State != ObjectStates.Unknown)
					.ToList();
			}
		}
		private List<clsTransaction> ArendaChargeSource
		{
			get
			{
				var source = Session["ArendaChargeSource"] as IEnumerable<clsTransaction>;
				if (source == null)
				{
					var repository = ObjectFactory.GetInstance<ITransactionRepository>();
					source = repository.GetByParam(oEditedDocument, AccountTypes.Charges, false).Where(transaction => !transaction.IsAnnihilated);
					Session["ArendaChargeSource"] = source;
				}
				return source.ToList();
			}
			set { Session["ArendaChargeSource"] = value; }
		}
		/// <summary>
		/// Начисление пени
		/// </summary>
		/// <returns></returns>
        public ActionResult Index()
        {
            return View();
		}

		#region Параметры начисления пени
		/// <summary>
		/// Доступные типы пени для настроек
		/// </summary>
		/// <returns></returns>
		ParametrsPenaltyGrid parametrsPenaltyGrid = new ParametrsPenaltyGrid();
		public string PenyTypes() 
		{
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(parametrsPenaltyGrid.PenyTypes(oEditedDocument));
		}
		/// <summary>
		/// Добавление настроек пени
		/// </summary>
		/// <param name="penyTypeName">Название типа пени</param>
		/// <param name="penyDataString">Дата</param>
		/// <param name="selectedFormula">Выбранная формула</param>
		/// <returns></returns>
		public string AddPeniSetting(string penyTypeName, string penyDataString, string selectedFormula) 
		{
			var serializer = new JavaScriptSerializer();

			DateTime data;
			if (!DateTime.TryParse(penyDataString, out data)) {
				return serializer.Serialize(
					new
					{
						success = false,
						message = "Дата имела неверный формат."
					}
				);
			};

			clsDocumentArendaPeniSettings newPeniSetting = new clsDocumentArendaPeniSettings();
			var arendaPeniSettings = ObjectFactory.GetInstance<IArendaPeniSettingsRepository>()
				.GetPeniSettingsByName(penyTypeName, oEditedDocument.Arenda.DocumentType == DocumentsTypes.Arenda);
			arendaPeniSettings.PeniChargeStartDateFormula = selectedFormula;
			newPeniSetting.dStartDate = data.AddMonths(arendaPeniSettings.CountMonthsDelay);
			newPeniSetting.dFinishDate = null;
			newPeniSetting.State = ObjectStates.New;
			newPeniSetting.IdDocument = oEditedDocument.Id.Value;
			newPeniSetting.oArendaPeniSettings = arendaPeniSettings;

			var finishDate = newPeniSetting.dStartDate.AddDays(-1);
			ActivePeniSettings
				.Where(s => s.dFinishDate == null && s.dStartDate < finishDate)
				.DoForEach(s =>
				{
					s.dFinishDate = finishDate;
					s.State = ObjectStates.Dirty;
				});

			oEditedDocument.Arenda.ArendaPeniSettings.Add(newPeniSetting);
			
			return serializer.Serialize(
				new
				{
					success = true,
					message = string.Empty
				}
			);
		}
		
		// Грид		
		/// <summary>
		/// Настройки колонок
		/// </summary>
		/// <returns></returns>
		public JsonResult GetParametrPenaltyGridColumnNames()
		{
			return Json(parametrsPenaltyGrid.ColumnNames(oEditedDocument), JsonRequestBehavior.AllowGet);
		}
		/// <summary>
		/// Метод получения данных 
		/// </summary>
		public ActionResult GetParametrPenaltyGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(parametrsPenaltyGrid.GetGridData(sidx, sord, page, rows, npage, ActivePeniSettings), "json");
		}
		public ActionResult GetParametrPenaltyGridData_Edit(string oper, object id, string additionalInfo)
		{
			switch (oper)
			{
				// операция удаления
				case "del":
					{
						List<long> rowsIds = new List<long>();		// номера выделенных строк
						List<long> parametrIds = new List<long>();

						List<string> srowsIds = ((string)id).Split(',').ToList();
						foreach (string sid in srowsIds)
							rowsIds.Add(Int64.Parse(sid));
						List<string> sparametrIds = additionalInfo.Split(',').ToList();	// Guid'ы всех строк
						int indx = 1;
						foreach (string sid in sparametrIds)
						{
							if (rowsIds.Contains(indx))
								parametrIds.Add(Int64.Parse(sid));
							indx++;
						}

						foreach(long paramId in parametrIds){
							clsDocumentArendaPeniSettings oDocumentArendaPeniSettings = ActivePeniSettings.Find(p => p.Id == paramId);
							if (oDocumentArendaPeniSettings != null)
								oDocumentArendaPeniSettings.State = ObjectStates.Delete;
						}
					};
					break;
			};
			return Content("true", "json");
		}
		public ActionResult GetParametrPenaltyGridData_EditRow(string Id, string PenyType, string DayStart, string Percent, string Refinance, string Delay, string Formula, string DateStart, string DateEnd)
		{
			long settingId = 0;
			Int64.TryParse(Id, out settingId);

			clsDocumentArendaPeniSettings oDocumentArendaPeniSettings = ActivePeniSettings.Find(p => p.Id == settingId);
			oDocumentArendaPeniSettings.State = ObjectStates.Dirty;
			clsArendaPeniSettings oArendaPeniSettings = new clsArendaPeniSettings();
			oArendaPeniSettings.State = ObjectStates.New;

			clsArendaPeniSettings peniSettings = ObjectFactory.GetInstance<IArendaPeniSettingsRepository>()
				.GetPeniSettingsByName(PenyType, oEditedDocument.Arenda.DocumentType == DocumentsTypes.Arenda);

			oArendaPeniSettings.PeniChargeStartDateFormula = peniSettings.PeniChargeStartDateFormula;
			oArendaPeniSettings.FormulaType = peniSettings.FormulaType;
			oArendaPeniSettings.Name = PenyType;
			oArendaPeniSettings.OldName = oDocumentArendaPeniSettings.sName;

			oArendaPeniSettings.CalcPeniStartDay = Convert.ToInt32(DayStart);
			oArendaPeniSettings.PeniPercent = Convert.ToDecimal(Percent);
			oArendaPeniSettings.PartRefinancingRate = Convert.ToDecimal(Refinance);
			oArendaPeniSettings.CountMonthsDelay = Convert.ToInt32(Delay);
			oArendaPeniSettings.PeniChargeStartDateFormula = Formula;

			DateTime dateStart, dateEnd;
			oDocumentArendaPeniSettings.oArendaPeniSettings = oArendaPeniSettings;
			DateTime.TryParse(DateStart, out dateStart);
			oDocumentArendaPeniSettings.dStartDate = dateStart;
			if (DateTime.TryParse(DateEnd, out dateEnd))
			{
				oDocumentArendaPeniSettings.dFinishDate = dateEnd;
			}
			else
			{
				oDocumentArendaPeniSettings.dFinishDate = null;
			}

			return Content("true", "json");
		}
		#endregion
		#region Начисление пени
		public string ButtonAddChargeClick()
		{
			string path = string.Concat(
				"TransactionEditor.aspx?IdDoc=",
				oEditedDocument.Id.ToString(),
				"&CredAcc=",
				oEditedDocument.Arenda.IdAccountPeniCharges.ToString()
			);

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(path);
		}
		public ActionResult ButtonExportChargeClick()
		{
			DataTable data = new DataTable();
			data.Columns.Add("Дата", typeof(string));
			data.Columns.Add("Начислено", typeof(string));
			data.Columns.Add("Комментарий", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			IEnumerable<clsTransaction> dataSource = oTransactionRep.GetByParam(
				oEditedDocument, AccountTypes.PeniCharges, false);
			DataRow dr;
			decimal summTransact = 0;
			foreach (clsTransaction transact in dataSource)
			{
				dr = data.NewRow();
				dr["Дата"] = transact.Date.ToShortDateString();
				dr["Начислено"] = transact.Summ.ToString();
				dr["Комментарий"] = transact.Comment;
				data.Rows.Add(dr);

				summTransact += transact.Summ;
			};

			dr = data.NewRow();
			dr["Начислено"] = summTransact.ToString();
			data.Rows.Add(dr);

			SpreadsheetModel mySpreadsheet = new SpreadsheetModel("Транзакции.xls", data);
			return View("ExcelExport", mySpreadsheet);
		}
		public string RecalcPenalty(bool isForToday, bool isForPeriod, string DateStart, string DateEnd) 
		{
			var serializer = new JavaScriptSerializer();
			if (oEditedDocument == null || oEditedDocument.Id == 0) {
				return serializer.Serialize(
					new 
					{
						success = false,
			 			message = "Редактируемый документ не определен"
					}
				);
			}

			ICharges charges = ObjectFactory.GetInstance<ICharges>();
			if (isForToday)
			{
				charges.ChargePeni(oEditedDocument, null, DateTime.Now);
			}
			else if (isForPeriod)
			{
				DateTime dateStart, dateEnd;
				if (!DateTime.TryParse(DateStart, out dateStart) ||
					!DateTime.TryParse(DateEnd, out dateEnd)) 
				{
					return serializer.Serialize(
						new
						{
							success = false,
							message = "Дата имеет неверный формат"
						}
					);
				}
				charges.ChargePeni(oEditedDocument, dateStart, dateEnd);
			}
			else
			{
				return serializer.Serialize(
					new
					{
						success = false,
						message = "Нужно указать период пересчета"
					}
				);
			}

			return serializer.Serialize(
				new
				{
					success = true,
					message = string.Empty
				}
			);
		}

		ChargePenaltyGrid chargePenaltyGrid = new ChargePenaltyGrid();
		public JsonResult GetChargePenaltyGridColumnNames()
		{
			return Json(chargePenaltyGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}
		public ActionResult GetChargePenaltyGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(chargePenaltyGrid.GetGridData(sidx, sord, page, rows, npage, oEditedDocument), "json");
		}
		public string ChargePenaltyTransactTypes() {
			return chargePenaltyGrid.GetTransactionsTypes(oEditedDocument);
		}
		public ActionResult GetChargePenaltyGridData_Edit(string oper, string id, string additionalInfo)
		{
			switch (oper)
			{
				// операция удаления
				case "del":
					{
						List<long> rowsIds = new List<long>();		// номера выделенных строк
						List<long> transactIds = new List<long>();	// Id выделенных строк

						List<string> srowsIds = id.Split(',').ToList();
						foreach (string sid in srowsIds)
							rowsIds.Add(Int64.Parse(sid));
						List<string> stransactIds = additionalInfo.Split(',').ToList();	// Guid'ы всех строк
						int indx = 1;
						foreach (string sid in stransactIds)
						{
							if (rowsIds.Contains(indx))
								transactIds.Add(Int64.Parse(sid));
							indx++;
						}

						var repository = ObjectFactory.GetInstance<ITransactionRepository>();
						foreach (long tId in transactIds)
						{
							DeleteTransactionById(repository, tId);
						};

						ArendaChargeSource = null;
					};
					break;
			};

			return Content("true", "json");
		}

		private void DeleteTransactionById(ITransactionRepository oTransactionRepository, long id)
		{
			clsTransaction oTransaction = oTransactionRepository.GetById(id);
			if (oTransaction != null)
			{
				oTransaction.State = ObjectStates.Delete;
				oTransactionRepository.Delete(oTransaction);
				oEditedDocument.Arenda.HistoryTransaction.Add(oTransaction);
			}
		}

		#endregion
		#region Перечисление пени
		public string ButtonAddTransferClick()
		{
			string path = string.Concat(
				"TransactionEditor.aspx?IdDoc=" + 
				oEditedDocument.Id.Value.ToString() +
				"&DebAcc=" + 
				oEditedDocument.Arenda.IdAccountPeniCharges.ToString()
			);

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(path);
		}
		public ActionResult ButtonExportTransferClick()
		{
			DataTable data = new DataTable();
			data.Columns.Add("Дата", typeof(string));
			data.Columns.Add("Перечислено", typeof(string));
			data.Columns.Add("Комментарий", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			IEnumerable<clsTransaction> dataSource = oTransactionRep.GetByParam(
				oEditedDocument, AccountTypes.PeniCharges, true);
			DataRow dr;
			decimal summTransact = 0;
			foreach (clsTransaction transact in dataSource)
			{
				dr = data.NewRow();
				dr["Дата"] = transact.Date.ToShortDateString();
				dr["Перечислено"] = transact.Summ.ToString();
				dr["Комментарий"] = transact.Comment;
				data.Rows.Add(dr);

				summTransact += transact.Summ;
			};

			dr = data.NewRow();
			dr["Перечислено"] = summTransact.ToString();
			data.Rows.Add(dr);

			SpreadsheetModel mySpreadsheet = new SpreadsheetModel("Транзакции.xls", data);
			return View("ExcelExport", mySpreadsheet);
		}

		TransferPenaltyGrid transferPenaltyGrid = new TransferPenaltyGrid();
		public JsonResult GetTransferPenaltyGridColumnNames()
		{
			return Json(transferPenaltyGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}
		public ActionResult GetTransferPenaltyGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(transferPenaltyGrid.GetGridData(sidx, sord, page, rows, npage, oEditedDocument), "json");
		}
		public string TransferPenaltyTransactTypes()
		{
			return transferPenaltyGrid.GetTransactionsTypes(oEditedDocument);
		}
		public ActionResult GetTransferPenaltyGridData_Edit(string oper, string id, string additionalInfo)
		{
			switch (oper)
			{
				// операция удаления
				case "del":
					{
						List<long> rowsIds = new List<long>();		// номера выделенных строк
						List<long> transactIds = new List<long>();	// Guid выделенных строк

						List<string> srowsIds = id.Split(',').ToList();
						foreach (string sid in srowsIds)
							rowsIds.Add(Int64.Parse(sid));
						List<string> stransactIds = additionalInfo.Split(',').ToList();	// Guid'ы всех строк
						int indx = 1;
						foreach (string sid in stransactIds)
						{
							if (rowsIds.Contains(indx))
								transactIds.Add(Int64.Parse(sid));
							indx++;
						}

						var repository = ObjectFactory.GetInstance<ITransactionRepository>();
						foreach (long tId in transactIds)
						{
							DeleteTransactionById(repository, tId);
						};

						ArendaChargeSource = null;
					};
					break;
			};

			return Content("true", "json");
		}

		#endregion
	}
	/// <summary>
	/// Параметры начисления пени
	/// </summary>
	public class ParametrsPenaltyGrid : MVCGridModel
	{
		/// <summary>
		/// Представляемые столбцы в гриде
		/// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:colmodel_options
		/// </summary>
		/// <returns></returns>
		public override List<object> ColumnNames()
		{
			return null;
		}
		public List<object> ColumnNames(clsDocument oEditedDocument)
		{
			List<object> result = new List<object>();
			result.Add(new { index = 0, name = "Id",		hidden = true });
			result.Add(new 
			{ 
				index = 1, 
				name = "PenyType", 
				label = "Тип расчета пени",
				edittype = "select",
				editoptions = new { value = GetPebiTypeString(oEditedDocument) }
			});
			result.Add(new { index = 2, name = "DayStart",	label = "День начала начисления" });
			result.Add(new { index = 3, name = "Percent",	label = "Процент" });
			result.Add(new { index = 4, name = "Refinance",	label = "Доля ставки рефинансирования" });
			result.Add(new { index = 5, name = "Delay",		label = "Отсрочка" });
			result.Add(new { index = 6, name = "Formula",	label = "Формула расчета даты начисления" });
			result.Add(new
			{
				index = 7,
				name = "DateStart",
				label = "Дата начала",
				type = "date",
				format = new
				{
					srcformat = "d-m-Y H:i:s",
					newformat = "d.m.Y"
				},
				editrules = new {
					date = true
				}
			});
			result.Add(new
			{
				index = 8,
				name = "DateEnd",
				label = "Дата окончания",
				type = "date",
				format = new
				{
					srcformat = "d-m-Y H:i:s",
					newformat = "d.m.Y"
				},
				editrules = new
				{
					date = true
				}
			});			
			return result;
		}

		private string GetPebiTypeString(clsDocument oEditedDocument) 
		{
			List<SelectListItem> peniTypes = PenyTypes(oEditedDocument);
			peniTypes.RemoveAt(0);
			StringBuilder result = new StringBuilder();
			foreach (SelectListItem type in peniTypes) {
				result.Append(type.Text);
				result.Append(":");
				result.Append(type.Value);
				result.Append(";");
			}
			result.Remove(result.Length - 1, 1);
			return result.ToString();
		}
		/// <summary>
		/// Возвращает страницу с данными для грида
		/// </summary>
		/// <param name="sidx">id атрибута для сортировки</param>
		/// <param name="sord">направление сортировки</param>
		/// <param name="page">номер страницы</param>
		/// <param name="rows">количество строк на странице</param>
		/// <returns>json c данными: количество страниц, текущая страница, общее количество строк, перечисление данных строк </returns>
		public string GetGridData(string sidx, string sord, int page, int rows, string npage, List<clsDocumentArendaPeniSettings> ActivePeniSettings)
		{
			DataTable data = new DataTable();
			data.Columns.Add("Id", typeof(string));
			data.Columns.Add("Type", typeof(string));
			data.Columns.Add("DateCalc", typeof(int));
			data.Columns.Add("Percent", typeof(decimal));
			data.Columns.Add("Refinance", typeof(decimal));
			data.Columns.Add("Delay", typeof(int));
			data.Columns.Add("Formula", typeof(string));
			data.Columns.Add("DateStart", typeof(DateTime));
			data.Columns.Add("DateEnd");

			DataRow dr;
			foreach (clsDocumentArendaPeniSettings setting in ActivePeniSettings)
			{
				dr = data.NewRow();
				dr["Id"] = setting.Id;
				dr["Type"] = setting.sName;
				dr["DateCalc"] = setting.CalcPeniStartDay;
				dr["Percent"] = setting.PeniPercent;
				dr["Refinance"] = setting.PartRefinancingRate;
				dr["Delay"] = setting.CountMonthsDelay;
				dr["Formula"] = setting.PeniChargeStartDateFormula;
				dr["DateStart"] = setting.dStartDate;
				dr["DateEnd"] = setting.dFinishDate;
				data.Rows.Add(dr);
			};
			return JsonForJqgrid(data, sidx, sord, page, rows, npage);
		}

		public List<SelectListItem> PenyTypes(clsDocument oEditedDocument)
		{
			List<SelectListItem> peniTypes = new List<SelectListItem>();

			peniTypes.Add(new SelectListItem()
			{
				Text = "Выберите тип начисления пени",
				Value = "0"
			});

			if (oEditedDocument != null)
			{
				List<clsArendaPeniSettings> colArendaPeniSettings = ObjectFactory.GetInstance<IArendaPeniSettingsRepository>()
					.GetAllPeniSettings(oEditedDocument.Arenda.DocumentType == DocumentsTypes.Arenda);
				foreach (clsArendaPeniSettings oArendaPeniSetting in colArendaPeniSettings)
				{
					peniTypes.Add(new SelectListItem()
					{
						Text = oArendaPeniSetting.Name,
						Value = oArendaPeniSetting.Name
					});
				}
			}

			return peniTypes;
		}
	}
	/// <summary>
	/// Начисление пени
	/// </summary>
	public class ChargePenaltyGrid : MVCGridModel
	{
		/// <summary>
		/// Представляемые столбцы в гриде
		/// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:colmodel_options
		/// </summary>
		/// <returns></returns>
		public override List<object> ColumnNames()
		{			
			List<object> result = new List<object>();
			result.Add(new { index = 0, name = "Id",					hidden = true });
			result.Add(new
			{
				index = 1,
				name = "Дата начисления",
				type = "date",
				format = new
				{
					srcformat = "d-m-Y H:i:s",
					newformat = "d.m.Y"
				}
			});
			result.Add(new { index = 2, name = "Начислено" });
			result.Add(new { index = 3, name = "Комментарий" });
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
		public string GetGridData(string sidx, string sord, int page, int rows, string npage, clsDocument oEditedDocument)
		{
			DataTable data = new DataTable();
			data.Columns.Add("Id", typeof(string));
			data.Columns.Add("Date", typeof(DateTime));
			data.Columns.Add("Summ", typeof(decimal));
			data.Columns.Add("Comment", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParam(
				oEditedDocument, AccountTypes.PeniCharges, false);
			DataRow dr;
			foreach (clsTransaction transact in dataSource)
			{
				dr = data.NewRow();
				dr["Id"] = transact.Id;
				dr["Date"] = transact.Date;
				dr["Summ"] = transact.Summ;
				dr["Comment"] = transact.Comment;
				data.Rows.Add(dr);
			};

			return JsonForJqgrid(data, sidx, sord, page, rows, npage);
		}

		public string GetTransactionsTypes(clsDocument oEditedDocument) {
			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParam(
				oEditedDocument, AccountTypes.PeniCharges, false);

			return TransactionTypesJson(dataSource);
		}
	}
	/// <summary>
	/// Перечисление пени
	/// </summary>
	public class TransferPenaltyGrid : MVCGridModel
	{
		/// <summary>
		/// Представляемые столбцы в гриде
		/// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:colmodel_options
		/// </summary>
		/// <returns></returns>
		public override List<object> ColumnNames()
		{
			List<object> result = new List<object>();
			result.Add(new { index = 0, name = "Id",					hidden = true });
			result.Add(new
			{
				index = 1,
				name = "Дата начисления",
				type = "date",
				format = new
				{
					srcformat = "d-m-Y H:i:s",
					newformat = "d.m.Y"
				}
			});
			result.Add(new { index = 2, name = "Перечислено" });
			result.Add(new { index = 3, name = "Комментарий" });
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
		public string GetGridData(string sidx, string sord, int page, int rows, string npage, clsDocument oEditedDocument)
		{
			DataTable data = new DataTable();
			data.Columns.Add("Id", typeof(string));
			data.Columns.Add("Date", typeof(DateTime));
			data.Columns.Add("Summ", typeof(decimal));
			data.Columns.Add("Comment", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParamWithExtendedComment(
				oEditedDocument, AccountTypes.PeniCharges, true);
			DataRow dr;
			foreach (clsTransaction transact in dataSource)
			{
				dr = data.NewRow();
				dr["Id"] = transact.Id;
				dr["Date"] = transact.Date;
				dr["Summ"] = transact.Summ;
				dr["Comment"] = transact.Comment;
				data.Rows.Add(dr);
			};

			return JsonForJqgrid(data, sidx, sord, page, rows, npage);
		}

		public string GetTransactionsTypes(clsDocument oEditedDocument)
		{
			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParam(
				oEditedDocument, AccountTypes.PeniCharges, true);

			return TransactionTypesJson(dataSource);
		}
	}
}
