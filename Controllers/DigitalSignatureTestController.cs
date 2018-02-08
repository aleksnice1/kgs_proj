using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
	public class DigitalSignatureTestController : Controller
	{
		public ActionResult Test()
		{
			return View("Index");
		}

		public ActionResult SignClientFile()
		{

			return View("SignClientFile");
		}

		public ActionResult CertificateChoice()
		{

			return View("CertificateChoice");
		}

		public ActionResult SignServerFile()
		{

			return View("SignServerFile");
		}

		[HttpPost]
		public string GetBase64()
		{
			var stringBase64 = string.Empty;
			var file = Request.Files[0];
			if (file != null && file.ContentLength != 0)
			{
				byte[] bytes = new byte[file.ContentLength];
				file.InputStream.Read(bytes, 0, file.ContentLength);
				stringBase64 = Convert.ToBase64String(bytes);
			}
			return stringBase64;
		}

		public string GetBase64String(clsAttachment attachment)
		{
			return GetBase64String(attachment.GetAttachmentData());
		}

		public string GetBase64String(byte[] data) {
			string result = Convert.ToBase64String(data);
			return result;
		}
	}
}