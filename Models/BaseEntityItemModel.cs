using System;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Модель базовой сущности как элемент списка 
	/// </summary>
	public class BaseEntityItemModel
	{
		public long Id { get; private set; }
		public Guid GUID { get; private set; }
		public string Name { get; private set; }

		public BaseEntityItemModel()
		{
			GUID = Guid.Empty;
			Id = -1;
			Name = string.Empty;
		}
	}
}