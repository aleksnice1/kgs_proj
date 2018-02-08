using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Kadastr.Domain;
using Kadastr.DomainModel.BusinessLogic.ArendaPeriod;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Models;
using StructureMap;

namespace Kadastr.WebApp.Controllers
{
	public class ArendaPeriodController : Controller
	{
		//
		// GET: /ArendaPeriod/

		public ActionResult Index()
		{
			var periods = ObjectFactory.GetInstance<IArendaPeriodRepository>()
									   .GetAll()
									   .OrderByDescending(p => p.EndDate);
			var models = Mapper.Map<ArendaPeriodViewModel[]>(periods);
			return View(models);
		}

		public ViewResult Edit(long? id)
		{
			var period = id.HasValue
				? ObjectFactory.GetInstance<IArendaPeriodRepository>().GetById(id.Value)
				: new ArendaPeriod();
			var model = Mapper.Map<ArendaPeriodViewModel>(period);
			return View("Edit", model);
		}

		public ViewResult Add(long? id)
		{
			return Edit(id);
		}

		[HttpPost]
		public RedirectToRouteResult Delete(long id)
		{
			var entity = ObjectFactory.GetInstance<IArendaPeriodRepository>().GetById(id);
			entity.State = ObjectStates.Delete;
			ObjectFactory.GetInstance<IArendaPeriodRepository>().Delete(entity);

			return RedirectToAction("Index");
		}


		[HttpPost]
		public ActionResult Edit(ArendaPeriodViewModel model)
		{
			try
			{
				var period = Mapper.Map<ArendaPeriod>(model);
				ObjectFactory.GetInstance<IArendaPeriodRepository>()
							 .Save(period);
			}
			catch (Exception exc)
			{
				//ModelState.AddModelError(exc.FieldName, string.Format(Properties.Resources.DuplicateItem, exc.Value));
				return View("Edit", model);
			}

			return RedirectToAction("Index");
		}
	}
}
