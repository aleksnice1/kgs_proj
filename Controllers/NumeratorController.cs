using AutoMapper;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Code.Helpers.UIHelpers;
using Kadastr.WebApp.Models;
using StructureMap;
using System;
using System.Web.Mvc;
using Numerator = Kadastr.WebApp.Models.Numerator;
using NumeratorHelper = Kadastr.DataAccessLayer.Helpers.NumeratorHelper;

namespace Kadastr.WebApp.Controllers
{
	[Authorize]
    public class NumeratorController : Controller
    {
	    private const string NumeratorKey = "NumeratorOriginal";
	    private INumeratorRepository Repository { get; set; }

	    private Numerator NumeratorOriginal
	    {
			get { return Session[NumeratorKey] as Numerator; }
			set { Session[NumeratorKey] = value; }
	    }

	    public NumeratorController ()
		{
			Repository = ObjectFactory.GetInstance<INumeratorRepository>();
		}

	    public ViewResult List()
        {
			var numerators = Repository.GetAll();
			return View(numerators);
        }

        public ViewResult Edit(int? id)
        {
			var model = id.HasValue ? Mapper.Map<Numerator>(Repository.GetById(id)) : new Numerator();
			NumeratorHelper.SetCategoriesFactory(model.EnumEntityType);
	        NumeratorOriginal = model;
			return EditForm(model);
        }

		public ViewResult Add(int? id) {
			return Edit(id);
		}

		[HttpPost]
		public ActionResult Edit(Numerator model)
		{
			NumeratorOriginal.MergeCategories(model);
			if (!ModelState.IsValid)
			{
				return EditForm(model);
			}

			model.Trim();

			var entity = Mapper.Map<DomainModel.Numerator>(model);
			try
			{
				Repository.Save(entity);
			}
			catch (UniqueFieldException<string> exc)
			{
				ModelState.AddModelError(exc.FieldName, string.Format(Properties.Resources.DuplicateItem, exc.Value));
				return EditForm(model);
			}

			return RedirectToAction("List");
		}

		[HttpPost]
		public PartialViewResult DeleteCategory(Guid categoryGuid)
		{
			var collection = NumeratorOriginal.CategoriesForUniqueness;
			collection.DeleteCategory(categoryGuid);
			return EditNumeratorCategory(collection);
		}

		[HttpPost]
		public PartialViewResult AddCategory(Guid categoryGuid)
		{
			var collection = NumeratorOriginal.CategoriesForUniqueness;
			if (collection.IsExist(categoryGuid))
			{
				ModelState.AddModelError("", string.Format(Properties.Resources.DuplicateItem, "Добавляемая категория в списке"));
			}
			else
			{
				collection.AddCategory(categoryGuid);
			}
			
			return EditNumeratorCategory(collection);
		}

        [HttpPost]
        public RedirectToRouteResult Delete(int id)
        {
			var entity = Repository.GetById(id);
			entity.State = ObjectStates.Delete;
			Repository.Delete(entity);
			
	        return RedirectToAction("List");
        }

		[HttpPost]
		public PartialViewResult ChangeCategoriesList(enEntityType enumEntityType)
		{
			NumeratorOriginal.CategoriesForUniqueness.Clear();
			NumeratorHelper.SetCategoriesFactory(enumEntityType);
			ViewBag.Categories = NumeratorHelper.Categories;
			return PartialView("EditorTemplates/CategoriesList", NumeratorOriginal.CategoriesForUniqueness);
		}

		public JsonResult RefreshNumeratedCategories(DataType dataType)
		{
			var selectList = NumeratorHelper.Categories.ToCategorySelectList(dataType, null, true);
			return Json(selectList, JsonRequestBehavior.AllowGet);
		}

	    private ViewResult EditForm(Numerator model)
	    {
			ViewBag.Categories = NumeratorHelper.Categories;
			return View("Edit", model);
		}

		private PartialViewResult EditNumeratorCategory(NumeratorCategoryCollection collection)
		{
			return PartialView("Partials/EditNumeratorCategory", collection.GetActiveCategories());
		}
    }
}
