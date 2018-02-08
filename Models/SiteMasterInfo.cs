using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using Kadastr.DataAccessLayer;
using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.Utils;
using StructureMap;
using Kadastr.WebApp.Code.Extensions;

namespace Kadastr.WebApp.Models
{
    /// <summary>
    /// Данные для мастер-страниц сайта
    /// </summary>
    public static class SiteMasterInfo
    {
        private static clsSiteInfoConfiguration _siteInfo;
		private static string _server;

        static SiteMasterInfo()
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~/");
            _siteInfo = (clsSiteInfoConfiguration)config.GetSection("clsSiteInfoConfiguration");

			var request = HttpContext.Current.Request;
			_server = string.Format("{0}{1}/", request.Url.GetLeftPart(UriPartial.Authority), request.ApplicationPath);
        }

		/// <summary>
		/// Полный путь каталога приложения с сервером
		/// </summary>
        public static string FullApplicationPath
        {
            get
            {
				return _server;
            }
        }

        /// <summary>
        /// Url логотипа
        /// </summary>
        public static string LogoUrl
        {
            get
            {
                return _siteInfo.Logo;
            }
        }
        

        /// <summary>
        /// Url главной страницы
        /// </summary>
        public static string MainPageUrl
        {
            get
            {
                if (GlobalSettings.MainPageUrl != null && GlobalSettings.MainPageUrl != string.Empty)
                    if (GlobalSettings.MainPageUrl.StartsWith("~/")) return GlobalSettings.MainPageUrl.Substring(2, GlobalSettings.MainPageUrl.Length - 2);
                    else return GlobalSettings.MainPageUrl;
                else throw new Exception("Не указан адрес начальной страницы");
            }
        }

        /// <summary>
        /// Имя пользователя 
        /// </summary>
        public static string UserName
        {
            get
            {
	            User user;
                return HttpContext.Current.Session.TryGetCurrentUser(out user) 
					? user.sName : string.Empty;
            }
        }

        /// <summary>
        /// Тема сайта, заданная в конфиге
        /// </summary>
        public static string Theme
        {
            get
            {
                return (string)HttpContext.Current.Application["CurrentTheme"] ?? string.Empty;
            }
        }

        /// <summary>
        /// Заголовок сайта
        /// </summary>
        public static string Title
        {
            get
            {
                return _siteInfo.Title;
            }
        }

        /// <summary>
        /// Текст подвала сайта
        /// </summary>
        public static string Footer
        {
            get
            {
                return _siteInfo.IsShowAssemblyVersion ? _siteInfo.AssemblyVersion + _siteInfo.Footer : _siteInfo.Footer;
            }
        }

        /// <summary>
        /// Показывать элементы, если сайт не в режиме техобслуживания
        /// </summary>
        public static string DisplayIfNotMaintenanceMode
        {
            get
            {
                return MaintenanceMode.Active ? "display:none" : string.Empty;
            }
        }

        /// <summary>
        /// Кол-во новых уведомлений с нормальным приоритетом
        /// </summary>
        public static int NormalNotificationCount
        {
            get
            {
                return GetNotificationMessageCount(enNotificationMessagePriorityLevel.Normal);
            }
        }

        /// <summary>
        /// Кол-во новых уведомлений с высоким приоритетом
        /// </summary>
        public static int HighNotificationCount
        {
            get
            {
                return GetNotificationMessageCount(enNotificationMessagePriorityLevel.High);
            }
        }

        private static int GetNotificationMessageCount(enNotificationMessagePriorityLevel level)
        {
			User user;
			if (!HttpContext.Current.Session.TryGetCurrentUser(out user)) return 0;
            var repository = ObjectFactory.GetInstance<INotificationsRepository>();
            return repository.GetCountMessageByStatus(user.Id, enNotificationStatusMessage.New, level);
        }
    }
}