using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.WebApp.Code.Helpers.UIHelpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Модель именованного контекста фильтра
	/// </summary>
	public class FilterContextViewModel : ViewModel
	{
		/// <summary>
		/// Идентификатор контекста
		/// </summary>
		[HiddenInput(DisplayValue = false)]
		public int? Id { get; set; }

		/// <summary>
		/// Наименование контекста
		/// </summary>
		[Display(Name = "Наименование контекста")]
		[StringLength(50)]
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// Тип сущности
		/// </summary>
		[Display(Name = "Тип сущности")]
		public enEntityType EnumEntityType { get; set; }

		/// <summary>
		/// Режим создания/редактирвоания
		/// </summary>
		public bool IsInEditMode 
		{
			get { return Id != null; }
		}

		/// <summary>
		/// Список пользователей в контексте
		/// </summary>
		public List<UserFilterContext> Users { get; private set; }

		public FilterContextViewModel()
		{
			Users = new List<UserFilterContext>();
			if (Id == null)
			{
				EnumEntityType = enEntityType.Propertys;
			}
		}

		public FilterContextViewModel(List<UserFilterContext> list)
		{
			Users = list;
			if (Id == null)
			{
				EnumEntityType = enEntityType.Propertys;
			}
		}

		/// <summary>
		/// Получает модель перечисления для типа сущности
		/// </summary>
		public DropDownModel GetEnumEntityTypeModel()
		{
			return new DropDownModel("EnumEntityType", EnumEntityType.ToString(), EnumEntityType.ToSelectList())
				{
					Disabled = IsInEditMode
				};
		}
	}
}