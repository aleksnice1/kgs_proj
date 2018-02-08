using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;

namespace Kadastr.WebApp.Controllers
{
    public class StartDateFormulaCreatorController : Controller
    {
        public ActionResult Index()
        {
			ViewBag.Formulas = GetFormulas();
			return PartialView();
        }

		public void Save(string formulaName, string formulaText)
		{
			var peniSetRepo = ObjectFactory.GetInstance<IArendaPeniSettingsRepository>();
			var formula = new StartDateFormula(formulaName, formulaText);
			peniSetRepo.AddStartDateFormula(formula);
		}

		public ActionResult GetFormulaList()
		{
			return Json(GetFormulas(), JsonRequestBehavior.AllowGet);
		}

		private List<StartDateFormula> GetFormulas()
		{
			var peniSetRepo = ObjectFactory.GetInstance<IArendaPeniSettingsRepository>();
			return peniSetRepo.GetStartDateFormulas();
		}
    }
}
