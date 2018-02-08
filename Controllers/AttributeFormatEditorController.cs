using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Kadastr.WebApp.Controllers
{
    public class AttributeFormatEditorController : Controller
    {
        public ActionResult Index(long? id)
        {
			string settings = string.Empty;
			if (id != null) {
				var attributeRepository = ObjectFactory.GetInstance<IAttributeRepository>();
				clsAttribute attribute = attributeRepository.GetById((long)id);
				settings = attribute.DisplayFormat;
			}

			List<SelectListItem> sufList = new List<SelectListItem>() { new SelectListItem(){
				Text="нет", 
				Value="", 
				Selected = true}
			};
			foreach (string value in DisplayFormatParametrs.Suffixes)
			{
				SelectListItem item = new SelectListItem()
				{
					Text = value,
					Value = value,
					Selected = false
				};

				sufList.Add(item);
			};
			var serializer = new JavaScriptSerializer();
			ViewBag.sufList = serializer.Serialize(sufList);

			List<SelectListItem> decimalSeparatorList = new List<SelectListItem>();
			foreach (string value in DisplayFormatParametrs.Delimiters)
			{
				SelectListItem item = new SelectListItem()
				{
					Text = value,
					Value = value,
					Selected = false
				};
				decimalSeparatorList.Add(item);
			};
			ViewBag.decimalSeparatorList = serializer.Serialize(decimalSeparatorList);


			return View((object)settings);
        }
    }
}
