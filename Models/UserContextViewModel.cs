using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using Kadastr.Domain;
using Kadastr.DomainModel.Helpers;
using Kadastr.WebApp.Code.Helpers.UIHelpers;
using Kadastr.DomainModel;
using StructureMap;
using Kadastr.DomainModel.Infrastructure;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Пользователь контекста
	/// </summary>
	public class UserContextViewModel : ViewModel
	{
		private const string BlankText = "Выберите пользователя";
		private static readonly SelectListItem BlankItem = new SelectListItem { Value = "", Text = BlankText };

		/// <summary>
		/// Идентификатор объекта
		/// </summary>
		[HiddenInput(DisplayValue = false)]
		public int? Id { get; set; }

		/// <summary>
		/// Id контекста
		/// </summary>
		[HiddenInput(DisplayValue = false)]
		public int ContextId { get; set; }

		/// <summary>
		/// Id пользователя
		/// </summary>
		[Display(Name = "Пользователь")]
		[Required]
		[Remote("NameAvailable", "FilterUserContext")]
		public int UserId { get; set; }

		/// <summary>
		/// Признак доступности всех сущностей
		/// </summary>
		[Display(Name = "Доступно все")]
		public bool All { get; set; }

		/// <summary>
		/// Тип сущности
		/// </summary>
		[Display(Name = "Тип сущности")]
		public enEntityType EnumEntityType
		{
			get
			{
				if (_enumEntityType == enEntityType.Undefined)
				{
					var contextRepo = ObjectFactory.GetInstance<IFilterContextRepository>();
					var context = contextRepo.GetById(this.ContextId);
					_enumEntityType = context.EnumEntityType;
				}
				return _enumEntityType;
			}
		}

		private enEntityType _enumEntityType;

		/// <summary>
		/// Подтип
		/// </summary>
		[Display(Name = "Подтип")]
		public SelectList Subtypes { get; set; }

		/// <summary>
		/// Сущности, доступные пользователю
		/// </summary>
		[Display(Name = "Пользователю доступны сущности")]
		public Guid[] EntityGuids
		{
			get
			{
				if (_entityGuids == null)
				{
					_entityGuids = All || Id == null
						? EntityGuids = new Guid[] { }
						: EntityGuids = ObjectFactory.GetInstance<IContextEntitiesRepository>()
							.GetContextEntities(Id.Value)
							.Select(e => e.GuidEntity).ToArray();
				}
				return _entityGuids;
			}
			set
			{
				_entityGuids = value;
			}
		}

		private Guid[] _entityGuids;

		/// <summary>
		/// Модель для рендеринга Id пользователя
		/// </summary>
		[Display(Name = "Пользователь")]
		public DropDownModel UserModel { get; private set; }

		/// <summary>
		/// Режим создания/редактирвоания
		/// </summary>
		public bool IsInEditMode
		{
			get { return Id != null; }
		}

		public void SetModelForUser(IEnumerable<User> users)
		{
			UserModel = new DropDownModel("UserId", UserId.ToString(), users.ToSelectList(UserId, BlankItem));
		}

		public DropDownModel GetSubtypes()
		{
			var repo = ObjectFactory.GetInstance<IEntityTypeRepository>();
			var items = repo.GetAllEntityTypes(EnumEntityType)
				.Select(t => new SelectListItem { Text = t.sName, Value = t.EntityGuid.ToString() })
				.OrderBy(i => i.Text);
			return new DropDownModel("EntitySubtype", "Выберите значение", items, new { @class = "entity-subtype" }, "Выберите значение");
		}

		public DropDownModel GetEnumEntityTypeModel()
		{
			var attrs = new
			{
				@class = "entity-type",
				disabled = "disabled"
			};
			return new DropDownModel("EnumEntityType", EnumEntityType.ToString(), EnumEntityType.ToSelectList(), attrs);
		}

		public IEnumerable<clsBaseDomainEntity> GetEntities()
		{
			var repo = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
			return EntityGuids.Select(g => repo.GetByGuid(g));
		}
	}
}