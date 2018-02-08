namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Панель действий с кнопками добавить/изменить/удалить
	/// </summary>
	public class PanelActions
	{
		/// <summary>
		/// Текст кнопки добавить
		/// </summary>
		public string AddLabel { get; set; }

		/// <summary>
		/// Параметры маршрута кнопки добавления
		/// </summary>
		public object AddRouteValues { get; set; }

		/// <summary>
		/// Текст кнопки изменить
		/// </summary>
		public string EditLabel { get; set; }

		/// <summary>
		/// Текст кнопки удалить
		/// </summary>
		public string DeleteLabel { get; set; }

		/// <summary>
		/// Необходимость дизейблить
		/// </summary>
		public bool Disabled { get; set; }

		public PanelActions()
		{
			AddLabel = "Добавить";
			EditLabel = "Изменить";
			DeleteLabel = "Удалить";
		}
	}
}