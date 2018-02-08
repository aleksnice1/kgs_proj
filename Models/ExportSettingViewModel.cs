using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kadastr.Domain;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Модель настроек экспорта
	/// </summary>
	public class ExportSettingViewModel
	{
		public const int ListBoxSize  = 20;

		/// <summary>
		/// Форма настроек экспорта
		/// </summary>
		public ExportSettingForm Form { get; private set; }

		public enEntityType EnumEntityType { get; set; }

		public int? IdType { get; set; }

		/// <summary>
		/// Все доступные категории или атрибуты
		/// </summary>
		public IEnumerable<SelectListItem> AllExportItems { get; set; }

		public ExportSettingViewModel()
		{
			Form = new ExportSettingForm();
		}

		public ExportSettingViewModel(ExportSettingForm form)
		{
			Form = form;
		}

		public IEnumerable<SelectListItem>  GetExportItems()
		{
			return	from selectedItem in Form.SelectedExportItems
					join item in AllExportItems on selectedItem.ToString() equals item.Value
					select item;
		}
	}
}