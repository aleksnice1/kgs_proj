using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Базовый класс модели представления
	/// </summary>
	public abstract class ViewModel
	{
		/// <summary>
		/// Маппит состояние сущности
		/// </summary>
		/// <typeparam name="T">Тип сущности</typeparam>
		/// <param name="entity">Сущность предметной области</param>
		public void MapState<T>(T entity) where T : clsDomainElement, ICommonDomainElement<int?>
		{
			MapState<T, int?>(entity);
		}


		/// <summary>
		/// Маппит состояние сущности
		/// </summary>
		/// <typeparam name="T">Тип сущности</typeparam>
		/// <typeparam name="I">Тип поля Id</typeparam>
		/// <param name="entity">Сущность предметной области</param>
		public void MapState<T, I>(T entity) where T : clsDomainElement,
			ICommonDomainElement<I>
		{
			if (entity.Id.Equals(default(I)))
			{
				entity.State = ObjectStates.Unknown;
				entity.State = ObjectStates.New;
			}
			else
			{
				entity.State = ObjectStates.FromDB;
				entity.State = ObjectStates.Dirty;
			}
		}
	}
}