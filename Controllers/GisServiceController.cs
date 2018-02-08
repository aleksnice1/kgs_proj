using Kadastr.CommonUtils;
using Kadastr.ServiceLayer.Service;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
    public class GisServiceController : Controller
    {
		public string GetProfile(string param)
		{
			GisService service = new GisService();
			return service.XMLGetProfile(param);
		}

		public string GetObjects(string ids)
		{
			GisService service = new GisService();
			return service.GetObjects(ids);
		}

		public string GetPlot(string param)
		{
			GisService service = new GisService();
			string result = service.XMLGetPlot(param);
			result = result.Replace("<", "&lt").Replace(">", "&gt");
			return result;
		}

		public string SavePlot(string param)
		{
			GisService service = new GisService();
			param = param.Replace("&lt", "<").Replace("&gt", ">");
			return service.XMLSavePlot(param);
		}
    }
}
