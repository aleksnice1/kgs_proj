using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
    public class MapController : Controller
    {
        public ActionResult GetMap(string id, string readOnly)
        {
			bool read = false;
			bool.TryParse(readOnly, out read);
			
			ViewBag.id = id;
			ViewBag.readOnly = read.ToString(); ;

			return View("GetMap");
        }
    }
}
