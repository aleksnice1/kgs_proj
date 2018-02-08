using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Kadastr.Domain;
using Kadastr.WebApp.Code.Helpers.UIHelpers;
using DataType = Kadastr.Domain.DataType;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Нумератор. Генератор значений с использованием формата
	/// </summary>
	public class Numerator : ViewModel
	{
		/// <summary>
		/// Id нумератора
		/// </summary>
		[ScaffoldColumn(false)]
		public int? Id { get; set; }

		/// <summary>
		/// Наименование нумератора
		/// </summary>
		[Display(Name = "Наименование")]
		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		/// <summary>
		/// Формат нумератора
		/// </summary>
		[Display(Name = "Формат")]
		[StringLength(50)]
		public string Format { get; set; }

		/// <summary>
		/// Регулярное выражение
		/// </summary>
		[Display(Name = "Регулярное выражение")]
		[StringLength(250)]
		public string RegularExpression { get; set; }

		/// <summary>
		/// Тип сущности (только для определения типа категории, находящихся в разных таблицах)
		/// </summary>
		[Display(Name = "Тип сущности")]
		public enEntityType EnumEntityType { get; set; }

		/// <summary>
		/// Тип данных, задаваемый для выбора категорий атрибутов нужного типа
		/// </summary>
		[Display(Name = "Тип категорий атрибутов")]
		public DataType AttributeType { get; set; }

		/// <summary>
		/// Флаг - не генерировать, если нумератор используется только для проверки уникальности
		/// </summary>
		[Display(Name = "Отключить генерацию")]
		public bool DoNotGenerate { get; set; }

		/// <summary>
		/// Флаг - не генерировать, если нумератор используется только для проверки уникальности
		/// </summary>
		[Display(Name = "Проверять на уникальность среди удаленных")]
		public bool CheckInDeleted { get; set; }

		/// <summary>
		/// Флаг - не генерировать, если нумератор используется только для проверки уникальности
		/// </summary>
		[Display(Name = "Проверять на уникальность среди архивных")]
		public bool CheckInArchive { get; set; }

		/// <summary>
		/// Регулярное выражение
		/// </summary>
		[Display(Name = "Сообщение об ошибке")]
		[Required]
		[StringLength(250)]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Нумерируемая категория
		/// </summary>
		[Display(Name = "Категория - счетчик")]
		[Required]
		public Guid NumeratedCategoryGuid { get; set; }

		/// <summary>
		/// Категории для проверки на уникальность
		/// </summary>
		[DisplayName("Категории, участвующие в проверке на уникальность")]
		[UIHint("CategoriesList")]
		public NumeratorCategoryCollection CategoriesForUniqueness { get; private set; }

		public Numerator()
		{
			EnumEntityType = enEntityType.Propertys;
			AttributeType = DataType.Symbol;
			CategoriesForUniqueness = new NumeratorCategoryCollection();
		}

		/// <summary>
		/// Записывает категории в нумератор
		/// </summary>
		/// <param name="model">Нумератор с клиента</param>
		public void MergeCategories(Numerator model)
		{
			model.CategoriesForUniqueness.Clear();
			model.CategoriesForUniqueness.AddRange(CategoriesForUniqueness);
		}

		/// <summary>
		/// Выполняет дополнительное слияние нумератора модели в сущность
		/// </summary>
		/// <param name="numerator">нумератор - сущность</param>
		public void Merge(DomainModel.Numerator numerator)
		{
			numerator.CategoriesForUniqueness.Clear();
			numerator.CategoriesForUniqueness.AddRange(CategoriesForUniqueness);
			MapState(numerator);
		}

		/// <summary>
		/// Обрезает пробелы свойствам, которым они не нужны
		/// </summary>
		public void Trim()
		{
			Name = Name.Trim();
			var format = Format;
			Format = string.IsNullOrWhiteSpace(format) ? format : format.Trim();
		}

		public DropDownModel GetEnumEntityTypeModel()
		{
			return new DropDownModel("EnumEntityType", EnumEntityType.ToString(), EnumEntityType.ToSelectList())
				{
					Disabled = Id != null
				};
		}

		public DropDownModel GetAttributeTypeModel()
		{
			return new DropDownModel("AttributeType", AttributeType.ToString(), AttributeType.ToSelectList())
				{
					Disabled = Id != null
				};
		}
	}
}