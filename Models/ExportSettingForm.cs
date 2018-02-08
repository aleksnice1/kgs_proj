using Kadastr.Domain;
using Kadastr.WebApp.Code.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Kadastr.WebApp.Models
{
	public class ExportSettingForm : ViewModel
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[HiddenInput(DisplayValue = false)]
		public int? Id { get; set; }

		/// <summary>
		/// Наименование настройки
		/// </summary>
		[Display(Name = "Наименование")]
		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		/// <summary>
		/// Флаг скрытия настройки от других пользователей
		/// </summary>
		[Display(Name = "Скрывать настройку от других пользователей")]
		public bool IsHidden { get; set; }

		/// <summary>
		/// Выбранные атрибуты или категории для экпорта из view 
		/// </summary>
		[Required]
		public long[] SelectedExportItems { get; set; }

		public ExportSettingForm()
		{
			SelectedExportItems = new long[0];
		}

		public int? MapUser()
		{
			User user;
			return !HttpContext.Current.Session.TryGetCurrentUser(out user)
				|| !IsHidden ? new int?() : user.Id;
		}
	}
}