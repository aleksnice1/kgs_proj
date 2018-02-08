using AutoMapper;
using Kadastr.CommonUtils;
using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Code;
using Kadastr.WebApp.Code.Helpers.UIHelpers;
using Kadastr.WebApp.Models;
using StructureMap;
using System.Web.Mvc;
using SessionHelper = Kadastr.WebApp.Code.Extensions.SessionExtension;

namespace Kadastr.WebApp.Controllers
{
	/// <summary>
	/// Создание настроек экпорта и выполнение операции экспорта сущностей(имущество, контрагенты, документы) в Excel
	/// </summary>
	public class ExtendedExportController : Controller
	{
		private const string ExporterKey = "Exporter";
		private IExportSettingsRepository Repository { get; set; }

		private ExtendedExporter Exporter
		{
			get { return (ExtendedExporter)Session[ExporterKey]; }
			set { Session[ExporterKey] = value; }
		}

		public ExtendedExportController()
		{
			Repository = ObjectFactory.GetInstance<IExportSettingsRepository>();
		}

		public ViewResult List(int enumEntityType, int? idType)
		{
			var et = EnumHelper.ParseEnum<enEntityType>(enumEntityType);
			var user = SessionHelper.GetCurrentUser(Session);
			var exportSettings = Repository.GetExportSettingsByUser(et, idType, user);
			return View(exportSettings);
		}

		public ViewResult ExportType(int operationTypeId, int enumEntityType, int idType, int? id)
		{
			var et = EnumHelper.ParseEnum<enEntityType>(enumEntityType);
			Exporter = new AttributeExporter(et, idType, operationTypeId);
			return EditForm(CreateModel(id));
		}

		public ViewResult ExportClass(int operationTypeId, int enumEntityType, int? id)
		{
			var et = EnumHelper.ParseEnum<enEntityType>(enumEntityType);
			Exporter = new CategoryExporter(et, operationTypeId);

			return EditForm(CreateModel(id));
		}

		public ViewResult Edit(int? id)
		{
			return EditForm(CreateModel(id));
		}

		[HttpPost]
		public ActionResult Edit([Bind(Include = "Form")]ExportSettingViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return EditForm(model);
			}
			var entity = Exporter.MapToEntity(model);
			Repository.Save(entity);

			return Content(string.Format(Properties.Resources.SuccessfulSave,
				                      string.Format("Настройки экспорта \"{0}\"", model.Form.Name)));
		}

		[HttpPost]
		public string Export([Bind(Include = "Form")]ExportSettingViewModel model)
		{
			var exporter = Exporter;
			exporter.SetViewSetting(model);
			return exporter.BuildReturnUrl();
		}

		[HttpPost]
		public RedirectToRouteResult Delete(int enumEntityType, int? idType, int id)
		{
			var entity = Repository.GetById(id);
			entity.State = ObjectStates.Delete;
			Repository.Delete(entity);

			return RedirectToAction("List", new { enumEntityType, idType });
		}

		public bool CheckOperationData()
		{
			var user = SessionHelper.GetCurrentUser(Session);
			var exporter = Exporter;
			return user != null && exporter != null && exporter.CheckAvailableExportData();
		}

		private ViewResult EditForm(ExportSettingViewModel model)
		{
			var exporter = Exporter;
			if (exporter != null)
			{
				model.EnumEntityType = exporter.EnumEntityType;
				model.IdType = exporter.TypeId;
				model.AllExportItems = exporter.GetAll().ToSelectList();
			}
			return View("Edit", model);
		}

		private ExportSettingViewModel CreateModel(int? id)
		{
			if (id.HasValue)
			{
				var form = Mapper.Map<ExportSettingForm>(Repository.GetById(id));
				return new ExportSettingViewModel(form);
			} 

			return new ExportSettingViewModel();
		}
	}
}