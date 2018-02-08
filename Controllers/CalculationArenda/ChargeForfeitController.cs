using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Kadastr.WebApp.Controllers.CalculationArenda
{
    public class ChargeForfeitController : Controller
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
		/// Начисление штрафа
		/// </summary>
		/// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
		#region Начисление штрафа

		public string ButtonAddChargeClick()
		{
			string path = string.Concat(
				"TransactionEditor.aspx?IdDoc=",
				oEditedDocument.Id.ToString(),
				"&CredAcc=",
				oEditedDocument.Arenda.IdAccountFineCharges.ToString()
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
			List<clsTransaction> dataSource = oTransactionRep.GetByParam(oEditedDocument, AccountTypes.FineCharges, false);
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

		ChargeForfeitGrid chargeForfeitGrid = new ChargeForfeitGrid();
		public JsonResult GetChargeForfeitGridColumnNames()
		{
			return Json(chargeForfeitGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}
		public ActionResult GetChargeForfeitGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(chargeForfeitGrid.GetGridData(sidx, sord, page, rows, npage, oEditedDocument), "json");
		}
		public string ChargeForfeitTransactTypes()
		{
			return chargeForfeitGrid.GetTransactionsTypes(oEditedDocument);
		}
		public ActionResult GetChargeForfeitGridData_Edit(string oper, string id, string additionalInfo)
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
		#endregion
		#region Перечисление штрафа

		public string ButtonAddTransferClick()
		{
			string path = string.Concat(
				"TransactionEditor.aspx?IdDoc=",
				oEditedDocument.Id.ToString(),
				"&DebAcc=",
				oEditedDocument.Arenda.IdAccountFineCharges.ToString()
			);

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(path);
		}
		public ActionResult ButtonExportTransferClick()
		{
			DataTable data = new DataTable();
			data.Columns.Add("Дата", typeof(string));
			data.Columns.Add("Начислено", typeof(string));
			data.Columns.Add("Комментарий", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParam(oEditedDocument, AccountTypes.FineCharges, true);
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

		TransferForfeitGrid transferForfeitGrid = new TransferForfeitGrid();
		public JsonResult GetTransferForfeitGridColumnNames()
		{
			return Json(transferForfeitGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}
		public ActionResult GetTransferForfeitGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(transferForfeitGrid.GetGridData(sidx, sord, page, rows, npage, oEditedDocument), "json");
		}
		public string TransferForfeitTransactTypes()
		{
			return transferForfeitGrid.GetTransactionsTypes(oEditedDocument);
		}
		public ActionResult GetTransferPenaltyGridData_Edit(string oper, string id, string additionalInfo)
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
		#endregion

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
    }

	public class ChargeForfeitGrid : MVCGridModel
	{
		public override List<object> ColumnNames()
		{
			List<object> result = new List<object>();

			result.Add(new
			{
				index = 0,
				name = "Id",
				hidden = true
			});

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

			result.Add(new
			{
				index = 2,
				name = "Начислено"
			});

			result.Add(new
			{
				index = 3,
				name = "Комментарий"
			});

			return result;
		}

		public string GetGridData(string sidx, string sord, int page, int rows, string npage, clsDocument oEditedDocument)
		{
			DataTable data = new DataTable();
			data.Columns.Add("Id", typeof(string));
			data.Columns.Add("Date", typeof(DateTime));
			data.Columns.Add("Summ", typeof(decimal));
			data.Columns.Add("Comment", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParam(oEditedDocument, AccountTypes.FineCharges, false);
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
				oEditedDocument, AccountTypes.FineCharges, false);

			return TransactionTypesJson(dataSource);
		}
	}

	public class TransferForfeitGrid : MVCGridModel
	{
		public override List<object> ColumnNames()
		{
			List<object> result = new List<object>();

			result.Add(new
			{
				index = 0,
				name = "Id",
				hidden = true
			});

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

			result.Add(new
			{
				index = 2,
				name = "Перечислено"
			});

			result.Add(new
			{
				index = 3,
				name = "Комментарий"
			});

			return result;
		}

		public string GetGridData(string sidx, string sord, int page, int rows, string npage, clsDocument oEditedDocument)
		{
			DataTable data = new DataTable();
			data.Columns.Add("Id", typeof(string));
			data.Columns.Add("Date", typeof(DateTime));
			data.Columns.Add("Summ", typeof(decimal));
			data.Columns.Add("Comment", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParam(oEditedDocument, AccountTypes.FineCharges, true);
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
				oEditedDocument, AccountTypes.FineCharges, true);

			return TransactionTypesJson(dataSource);
		}
	}
}
