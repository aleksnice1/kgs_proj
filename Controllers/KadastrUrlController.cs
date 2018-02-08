using System.Web.Mvc;
using Kadastr.WebApp.Code;

namespace Kadastr.WebApp.Controllers
{
    public class KadastrUrlController : Controller
    {
		public JsonResult GetRootUrl()
	    {
		    var url = Session[SessionViewstateConstants.KadastrRootUrl] as string;
			return Json(new { success = !string.IsNullOrEmpty(url), rootUrl = url ?? string.Empty}, JsonRequestBehavior.AllowGet);
	    }
    }
}
