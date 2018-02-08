using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kadastr.GUI;
using System.IO;

namespace Kadastr.WebApp.Controllers.CalculationArenda
{
	public class ChargeArendaController : Controller
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
		private DateTime transactionDate 
		{
			get 
			{
				DateTime date;
				if (Session["TransactionDate"] != null)
					date = (DateTime)Session["TransactionDate"];
				else
					date = new DateTime();
				return date;
			}
			set 
			{
				Session["TransactionDate"] = value;
			}
		}
		/// <summary>
		/// Начисление арендной платы
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			return View();
		}

		#region Начисление арендной платы
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string ButtonAddChargeClick()
		{
			string path = string.Concat(
				"TransactionEditor.aspx?IdDoc=", 
				oEditedDocument.Id.ToString(), 
				"&CredAcc=", 
				oEditedDocument.Arenda.IdAccountCharges.Value.ToString()
			);

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(path);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ActionResult  ButtonExportChargeClick()
		{
			DataTable data = new DataTable();
			data.Columns.Add("Дата", typeof(string));
			data.Columns.Add("Начислено", typeof(string));
			data.Columns.Add("Комментарий", typeof(string));

			IEnumerable<clsTransaction> dataSource = chargeArendaGrid.GroupArendaChargeSourceByDate(ArendaChargeSource);
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

		ChargeArendaGrid chargeArendaGrid = new ChargeArendaGrid();

		public JsonResult GetChargeGridColumnNames()
		{
			return Json(chargeArendaGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetChargeGridData(string sidx, string sord, int page, int rows, string npage)
		{
			ArendaChargeSource = null;
			return Content(chargeArendaGrid.GetGridData(sidx, sord, page, rows, npage, ArendaChargeSource), "json");
		}

		public string ChargeGridClick(string transactDate)
		{
			var serializer = new JavaScriptSerializer();
			DateTime date;
			if (DateTime.TryParse(transactDate, out date))
			{
				transactionDate = date;
				var result = new { success = true };
				return serializer.Serialize(result);
			}
			else {
				var result = new { 
					success = false,
					message = string.Format("Не удалось преобразовать {0} в дату", transactDate)
				};
				return serializer.Serialize(result);
			}
		}

		#endregion
		#region Детализация начислений арендной платы
		ChargeDetailArendaGrid chargeDetailArendaGrid = new ChargeDetailArendaGrid();

		public JsonResult GetChargeDetailGridColumnNames()
		{
			return Json(chargeDetailArendaGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetChargeDetailGridData(string sidx, string sord, int page, int rows, string npage)
		{
			ArendaChargeSource = null;
			return Content(chargeDetailArendaGrid.GetGridData(sidx, sord, page, rows, npage, transactionDate, ArendaChargeSource), "json");
		}

		public ActionResult GetChargeDetailGridData_Edit(string oper, string id, string additionalInfo)
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
		#region Перечисление арендной платы
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string ButtonAddTransferClick()
		{
			string path = string.Concat(
				"TransactionEditor.aspx?IdDoc=",
				oEditedDocument.Id.ToString(),
				"&DebAcc=",
				oEditedDocument.Arenda.IdAccountCharges.Value.ToString()
			);

			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(path);
		}

		TransferArendaGrid transferArendaGrid = new TransferArendaGrid();

		public JsonResult GetTransferGridColumnNames()
		{
			return Json(transferArendaGrid.ColumnNames(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetTransferGridGridData(string sidx, string sord, int page, int rows, string npage)
		{
			return Content(transferArendaGrid.GetGridData(sidx, sord, page, rows, npage, oEditedDocument), "json");
		}

		public ActionResult GetTransferGridGridData_Edit(string oper, string id, string additionalInfo)
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

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ActionResult ButtonExportTransferClick()
		{
			DataTable data = new DataTable();
			data.Columns.Add("Дата", typeof(string));
			data.Columns.Add("Перечислено", typeof(string));
			data.Columns.Add("Комментарий", typeof(string));

			var oTransactionRep = ObjectFactory.GetInstance<ITransactionRepository>();
			List<clsTransaction> dataSource = oTransactionRep.GetByParamWithExtendedComment(
				oEditedDocument, AccountTypes.Charges, true);
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
		#endregion
	}

	public class SpreadsheetModel
	{
		public String fileName { get; set; }
		public String[,] contents { get; set; }

		public SpreadsheetModel(string fileName, DataTable data)
		{
			this.fileName = fileName;

			int tw = data.Columns.Count,
				th = data.Rows.Count + 1;

			this.contents = new String[th,tw];

			int row = 1, col = 0;
			foreach (DataColumn column  in data.Columns) {
				this.contents[0, col] = column.Caption;
				col++;
			};

			foreach (DataRow dataRow in data.Rows) {
				col = 0;
				foreach (var cell in dataRow.ItemArray) {
					this.contents[row, col] = cell.ToString();
					col++;
				}
				row++;
			};             
		}
	}

	public class ChargeArendaGrid : MVCGridModel
	{
		public override List<object> ColumnNames()
		{
			List<object> result = new List<object>();

			result.Add(new
			{
				index = 0,
				name = "Id",
				type = "integer",
				format = new
				{
					thousandsSeparator = ""
				},
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
		public string GetGridData(string sidx, string sord, int page, int rows, string npage, List<clsTransaction> ArendaChargeSource)
		{
			DataTable data = new DataTable();
			data.Columns.Add("Id", typeof(string));
			data.Columns.Add("Date", typeof(DateTime));
			data.Columns.Add("Summ", typeof(decimal));
			data.Columns.Add("Comment", typeof(string));

			IEnumerable<clsTransaction> dataSource = GroupArendaChargeSourceByDate(ArendaChargeSource);
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

		public IEnumerable<clsTransaction> GroupArendaChargeSourceByDate(List<clsTransaction> ArendaChargeSource)
		{
			return (from transaction in ArendaChargeSource
					group transaction by transaction.Date
						into @group
						select new clsTransaction
						{
							Id = @group.First().Id,
							Date = @group.Key,
							Summ = @group.Sum(transaction => transaction.Summ),
							Comment = @group.First().Comment,
							IsUserEdited = @group.FirstOrDefault(transaction => transaction.IsUserEdited) != null,
						}).ToList();
		}

		public GridView CreateGridForExport(string sCaptionColumn)
		{
			GridView oGridExport = new GridView();
			oGridExport.AutoGenerateColumns = false;
			oGridExport.ShowFooter = true;

			BoundField bf1 = new BoundField();
			bf1.DataField = "Date";
			bf1.HeaderText = "Дата";
			bf1.ReadOnly = true;
			bf1.HtmlEncode = false;
			bf1.DataFormatString = "{0:dd.MM.yyyy}";
			bf1.ItemStyle.Width = Unit.Pixel(150);
			oGridExport.Columns.Add(bf1);

			BoundField bf2 = new BoundField();
			bf2.DataField = "Summ";
			bf2.HeaderText = sCaptionColumn;
			bf2.ReadOnly = true;
			bf2.HtmlEncode = false;
			bf2.DataFormatString = "{0:f2}";
			bf2.ItemStyle.Width = Unit.Pixel(150);
			oGridExport.Columns.Add(bf2);

			BoundField bf3 = new BoundField();
			bf3.DataField = "Comment";
			bf3.HeaderText = "Комментарий";
			bf3.ReadOnly = true;
			oGridExport.Columns.Add(bf3);

			return oGridExport;
		}

	}
	public class ChargeDetailArendaGrid : MVCGridModel
	{
		public override List<object> ColumnNames()
		{
			List<object> result = new List<object>();

			result.Add(new
			{
				index = 0,
				name = "Id",
				type = "integer",
				format = new
				{
					thousandsSeparator = ""
				},
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

		public string GetGridData(string sidx, string sord, int page, int rows, string npage, DateTime transactionDate, List<clsTransaction> ArendaChargeSource)
		{
			DataTable data = new DataTable();
			data.Columns.Add("Id", typeof(string));
			data.Columns.Add("Date", typeof(DateTime));
			data.Columns.Add("Summ", typeof(decimal));
			data.Columns.Add("Comment", typeof(string));

			IEnumerable<clsTransaction> dataSource = ArendaChargeSourceByDate(transactionDate, ArendaChargeSource);
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

		private IEnumerable<clsTransaction> ArendaChargeSourceByDate(DateTime transactionDate, List<clsTransaction> ArendaChargeSource)
		{
			return ArendaChargeSource.Where(transaction => transaction.Date == transactionDate);
		}
	}
	public class TransferArendaGrid : MVCGridModel
	{
		public override List<object> ColumnNames()
		{
			List<object> result = new List<object>();

			result.Add(new
			{
				index = 0,
				name = "Id",
				type = "integer",
				format = new
				{
					thousandsSeparator = ""
				},
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
			List<clsTransaction> dataSource = oTransactionRep.GetByParamWithExtendedComment(
				oEditedDocument, AccountTypes.Charges, true);
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
				oEditedDocument, AccountTypes.Charges, true);

			return TransactionTypesJson(dataSource);
		}
	}
}
