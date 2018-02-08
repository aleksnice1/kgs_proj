using System.Web.Mvc;
using Kadastr.Domain;
using Kadastr.WebApp.Models;
using Kadastr.WebApp.Code.Extensions;
using Kadastr.ServiceLayer.Reporting;
using System.Web.Hosting;
using System.Configuration;
using System.Web.Configuration;
using Kadastr.WebApp.Code;
using Kadastr.WebApp.Old_App_Code.Domain;

namespace Kadastr.WebApp.Controllers
{
	/// <summary>
	/// Отвечает за сервисное обслуживание приложения.
	/// </summary>
    [Authorize]
    public class MaintenanceController : Controller
    {
        // GET: /Maintenance/
        public ActionResult Index()
        {
			if (IsAdminUser())
				return RedirectToRoute(new { controller = "Maintenance", action = "Administrate" }); 
            return View();
        }

		// GET: /Maintenance/Administrate
		public ActionResult Administrate()
		{
			if (!IsAdminUser())
				return RedirectToRoute(new { controller = "Maintenance", action = "Index" });
			if (MaintenanceMode.Active)
			{
				bool migrationPossible;
				if (MaintenanceMode.IsDbNeedUpdate(out migrationPossible))
				{
					if (migrationPossible)
						return View("UpdateDb");
					else 
						return View("Result", (object)Properties.Resources.NeedManualDbUpdate);
				}
				return View("StopMaintenance");
			}
			return View("StartMaintenance");
		}

		// Post: /Maintenance/UpdateDb
		[HttpPost]
		public ActionResult UpdateDb()
		{
			if (!IsAdminUser())
				return RedirectToRoute(new { controller = "Maintenance", action = "Index" });
			MaintenanceMode.UpdateDb();
			return View("Result", (object)Properties.Resources.DbUpdatedSuccessfully);
		}

		[HttpPost]
		public ActionResult UpdateUkk(long idDocument)
		{
			if (!IsAdminUser())
				return RedirectToRoute(new { controller = "Maintenance", action = "Index" });
			string message;
			return new UkkRecalculator().DoRecalc(new long?[] { idDocument }, out message)
				? View("Result", (object)("Все ОК " + message))
				: View("Result", (object)("Ошибка " + message));
		}

		// Post: /Maintenance/UpdateReportService
		[HttpPost]
		public ActionResult UpdateReportService()
		{
			if (!IsAdminUser())
				return RedirectToRoute(new { controller = "Maintenance", action = "Index" });
			ReportUpdater.UpdateReports(HostingEnvironment.MapPath("~/" + GlobalSettings.ReportsPath),
				GetReportingServiceUrl(), GetServerReportsPath(), GlobalSettings.KadastrConnectionString,
				new ReportServerCredentials().NetworkCredentials);
			return View("Result", (object)Properties.Resources.ReportServiceUpdatedSuccessfully);
		}

		// Post: /Maintenance/ChangeMode
		[HttpPost]
		public ActionResult ChangeMode(bool active)
		{
			if (!IsAdminUser())
				return RedirectToRoute(new { controller = "Maintenance", action = "Index" });
			object message;
			MaintenanceMode.Active = active;
			if (active)
			{
				message = Properties.Resources.MaintenanceModeOn;
			}
			else
			{
				message = Properties.Resources.MaintenanceModeOff;
			}
			return View("Result", message);
		}

		private bool IsAdminUser()
		{
			User user = Session.GetCurrentUser();
			return user != null && user.IsAdmin();
		}

		private string GetReportingServiceUrl()
		{
			Configuration config = WebConfigurationManager.OpenWebConfiguration("~/");
			clsMSReportingConfiguration reportingConfig =
				(clsMSReportingConfiguration)config.GetSection("clsMSReportingConfiguration");
			return reportingConfig.ConnectionString;
		}

		private string GetServerReportsPath()
		{
			Configuration config = WebConfigurationManager.OpenWebConfiguration("~/");
			clsMSReportingConfiguration reportingConfig =
				(clsMSReportingConfiguration)config.GetSection("clsMSReportingConfiguration");
			return reportingConfig.ReportPath;
		}
    }
}
