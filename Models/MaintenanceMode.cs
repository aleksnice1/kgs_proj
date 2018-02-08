using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kadastr.WebApp.Properties;

namespace Kadastr.WebApp.Models
{
	/// <summary>
	/// Режим техобслуживания.
	/// </summary>
	public class MaintenanceMode
	{
		private static bool _active;

		/// <summary>
		/// Приложение находится в режиме техобслуживания.
		/// </summary>
		public static bool Active 
		{
			get
			{
				return _active;
			}
			set
			{
				if (!value && IsDbNeedUpdate())
					throw new InvalidOperationException(Properties.Resources.DatabaseInvalidState);
				_active = value;
			}
		}

		/// <summary>
		/// Проверяем нужно ли перевести приложение в режим техобслуживания.
		/// </summary>
		public static void Initialize()
		{
			_active = false;
			if (Settings.Default.MaintenanceModeActive)
			{
				_active = true;
			}
			else if (Settings.Default.ValidateDbAtStart)
			{
				_active = IsDbNeedUpdate();
				if (!Active && Settings.Default.AutomaticUpdateDb)
				{
					UpdateDb();
				}
			}
		}

		/// <summary>
		/// Проверяет совпадении базы данных.
		/// </summary>
		/// <returns></returns>
		public static bool IsDbNeedUpdate()
		{
			bool migrationPossible;
			return IsDbNeedUpdate(out migrationPossible);
		}

		/// <summary>
		/// Проверяет совпадении базы данных.
		/// </summary>
		/// <returns></returns>
		public static bool IsDbNeedUpdate(out bool migrationPossible)
		{
			return new Kadastr.Database.Migration.Migrator().IsDbNeedUpdate(out migrationPossible) ||
				new Kadastr.Database.Migration.Attachments.Migrator().IsDbNeedUpdate(out migrationPossible);
		}

		/// <summary>
		/// Обновляет базу данных.
		/// </summary>
		public static void UpdateDb()
		{
			new Kadastr.Database.Migration.Migrator().UpdateDb();
			new Kadastr.Database.Migration.Attachments.Migrator().UpdateDb();
			_active = false;
		}
	}
}