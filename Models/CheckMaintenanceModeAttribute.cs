using System.Web.Mvc;
using System.Web.Routing;
using Kadastr.WebApp.Code;
using Kadastr.WebApp.Controllers;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Проверка на валидность базы данных.
	/// </summary>
	public class CheckMaintenanceModeAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!(filterContext.Controller is MaintenanceController))
			{
				if (MaintenanceMode.Active)
				{
					var routeValues = new RouteValueDictionary(new
					{
						action = "Index",
						controller = "Maintenance"
					});
					filterContext.Result = new RedirectToRouteResult(routeValues);
				}
			}
		}
	}
}