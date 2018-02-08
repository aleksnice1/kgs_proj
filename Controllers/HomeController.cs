using Kadastr.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public RedirectResult Index()
        {
			return Redirect(GlobalSettings.MainPageUrl);
        }
    }
}
