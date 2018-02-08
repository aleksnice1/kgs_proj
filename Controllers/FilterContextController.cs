using AutoMapper;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Models;
using StructureMap;
using System.Web.Mvc;

namespace Kadastr.WebApp.Controllers
{
	[Authorize]
    public class FilterContextController : Controller
    {
		private IFilterContextRepository Repository { get; set; }

		public FilterContextController()
		{
			Repository = ObjectFactory.GetInstance<IFilterContextRepository>();
		}

        public ActionResult List()
        {
			var model = Repository.GetAll();
			return View(model);
        }

	    public ViewResult Edit(int? id)
	    {
		    var model = id.HasValue
			                ? Mapper.Map<FilterContextViewModel>(Repository.GetById(id))
			                : new FilterContextViewModel();
			return View(model);
	    }

		[HttpPost]
		public ActionResult Edit(FilterContextViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			
			var entity = Mapper.Map<FilterContext>(model);
			try
			{
				Repository.Save(entity);
			}
			catch (UniqueFieldException<string> exc)
			{
				ModelState.AddModelError(exc.FieldName, string.Format(Properties.Resources.DuplicateItem, exc.Value));
				return View(model);
			}

			return RedirectToAction("List");
		}

		[HttpPost]
		public RedirectToRouteResult Delete(int id)
		{
			var entity = Repository.GetById(id);
			entity.State = ObjectStates.Delete;
			Repository.Delete(entity);

			return RedirectToAction("List");
		}
    }
}
