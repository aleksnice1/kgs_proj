using System.Collections.Generic;
using System.Web.Mvc;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Модель для DropDownList
	/// </summary>
	public class DropDownModel
	{
		public DropDownModel(string propertyName, string selected, IEnumerable<SelectListItem> items, object htmlAttributes, string optionLabel)
		{
			PropertyName = propertyName;
			SelectedValue = selected;
			Items = items;
			HtmlAttributes = htmlAttributes;
			OptionLabel = optionLabel;
		}

		public DropDownModel(string propertyName, string selected, IEnumerable<SelectListItem> items, object htmlAttributes)
			: this(propertyName, selected, items, htmlAttributes, null)
		{

		}

		public DropDownModel(string propertyName, string selected, IEnumerable<SelectListItem> items)
			: this(propertyName, selected, items, null, null)
		{

		}

		/// <summary>
		/// Свойство объекта
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// Выбранное значение
		/// </summary>
		public string SelectedValue { get; private set; }

		/// <summary>
		/// Список значений
		/// </summary>
		public IEnumerable<SelectListItem> Items { get; private set; }

		/// <summary>
		/// Необходимость дизейблить
		/// </summary>
		public bool Disabled { get; set; }

		/// <summary>
		/// Атрибуты
		/// </summary>
		public object HtmlAttributes { get; set; }

		/// <summary>
		/// Значение по умолчанию
		/// </summary>
		public string OptionLabel { get; set; }
	}
}