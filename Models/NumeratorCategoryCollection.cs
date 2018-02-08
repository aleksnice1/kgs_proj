using System;
using System.Collections.Generic;
using System.Linq;
using Kadastr.CommonUtils;
using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Уникальная коллекция категорий нумератора
	/// </summary>
	public class NumeratorCategoryCollection : List<clsAttributeCategory>
	{
		/// <summary>
		/// Получает неудаленные категории
		/// </summary>
		public IEnumerable<clsAttributeCategory> GetActiveCategories()
		{
			return from item in this
			       where item.State != ObjectStates.Delete
			       orderby item.sName
			       select item;
		}

		/// <summary>
		/// Проверяет наличие категории в коллекции нумератора с учетом состояния
		/// </summary>
		public bool IsExist(Guid categoryGuid)
		{
			var category = GetCategory(categoryGuid);
			return category != null && category.State != ObjectStates.Delete;
		}

		/// <summary>
		/// Добавляет категории в коллекцию с учетом состояния
		/// </summary>
		public void AddCategory(Guid categoryGuid)
		{
			var category = GetCategory(categoryGuid);
			if (category != null)
			{
				if (category.State != ObjectStates.Delete)
				{
					// если добавление добавленного
					throw new InvalidOperationException("Повторное добавление категории запрещено. Коллекция должна быть уникальной");
				}
				RestoreCategory(category);
			}
			else
			{
				var repository = ObjectFactory.GetInstance<IAttributeCategoryRepository>();
				category = repository.GetByGuid(categoryGuid);
				category.CheckIsNotNull("category");
				AddNewCategory(category);
			}
		}

		/// <summary>
		/// Удаляет категорию из коллекции с учетом состояния
		/// </summary>
		public void DeleteCategory(Guid categoryGuid)
		{
			var category = GetCategory(categoryGuid);
			category.CheckIsNotNull("category");
			if (category.State == ObjectStates.New)
			{
				base.Remove(category);
			}
			else
			{
				if (category.State == ObjectStates.Delete )
				{
					// если удаление удаленного
					throw new InvalidOperationException();
				}
				DeleteExistCategory(category);
			}
		}

		public new void Add(clsAttributeCategory category)
		{
			throw new NotImplementedException("использовать метод AddCategory");
		}

		public new void Remove(clsAttributeCategory category)
		{
			throw new NotImplementedException("использовать метод DeleteCategory");
		}

		/// <summary>
		/// Получает категорию из коллекции нумератора 
		/// </summary>
		private clsAttributeCategory GetCategory(Guid categoryGuid)
		{
			return Find(item => item.GUIDCategory == categoryGuid);
		}

		private void AddNewCategory(clsAttributeCategory category)
		{
			category.State = ObjectStates.Unknown;
			category.State = ObjectStates.New;
			base.Add(category);
		}

		private void RestoreCategory(clsAttributeCategory category)
		{
			category.State = ObjectStates.Unknown;
		}

		private void DeleteExistCategory(clsAttributeCategory category)
		{
			category.State = ObjectStates.FromDB;
			category.State = ObjectStates.Delete;
		}
	}
}