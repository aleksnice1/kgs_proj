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
	public class DigitalSignatureController : Controller
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

		[HttpGet]
		public string GetBase64(long id)
		{
			var attachment = ObjectFactory.GetInstance<IAttachmentRepository>().GetById(id);
			byte[] buf = attachment.AttachmentData;
			return Convert.ToBase64String(buf);
		}

		[HttpGet]
		public void SaveDigitalSign(long id, string sign)
		{
			if (id <= 0)
			{
				throw new InvalidOperationException("Невозможно подписать неопределенное вложение!");
			}
			if (string.IsNullOrEmpty(sign))
			{
				throw new InvalidOperationException("Отсутсвует цифровая подпись для вложения с Id = " + id);
			}
			else
			{
				var repo = ObjectFactory.GetInstance<IAttachmentRepository>();
				var attachment = repo.GetById(id);
				attachment.SignData = Convert.FromBase64String(sign);
				attachment.State = ObjectStates.Dirty;
				repo.Save(attachment);
			}
		}
	}
}