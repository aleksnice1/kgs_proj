using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kadastr.WebApp.Models
{
	public class ArendaPeriodViewModel : ViewModel
	{
		/// <summary>
		/// Id нумератора
		/// </summary>
		[ScaffoldColumn(false)]
		public long? Id { get; set; }

		/// <summary>
		/// Наименование нумератора
		/// </summary>
		[Display(Name = "Дата")]
		[Required]
		//		[StringLength(50)]
		[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}",
		   ApplyFormatInEditMode = true)]
		public DateTime EndDate { get; set; }
	}
}