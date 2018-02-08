using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kadastr.CommonUtils;

namespace Kadastr.WebApp.Controllers
{
    public class FormulaHelperController : Controller
    {
        //
        // GET: /FormulaHelper/

        public ActionResult Index()
        {
			return PartialView();
        }

		public string GetResult(string dateFormula, string currentDate)
		{
			DateTime date;
			if (DateTime.TryParse(currentDate, out date))
				return DateTimeFormulaParcer.Parce(dateFormula, date).ToShortDateString();
			else
				return "Неверный формат даты!";
		}

    }
}
