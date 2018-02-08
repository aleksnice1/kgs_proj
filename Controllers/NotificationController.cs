using Kadastr.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kadastr.WebApp.Code.Extensions;
using StructureMap;
using Kadastr.DomainModel.Infrastructure;

namespace Kadastr.WebApp.Controllers
{
    public class NotificationController : Controller
    {
		public int GetNewNotificationsCount(string priorityLevel) 
		{
			enNotificationMessagePriorityLevel priority;
			if (!string.IsNullOrEmpty(priorityLevel))
			{
				switch (priorityLevel)
				{
					case "Normal": priority = enNotificationMessagePriorityLevel.Normal; break;
					case "High": priority = enNotificationMessagePriorityLevel.High; break;
					default: priority = enNotificationMessagePriorityLevel.Normal; break;
				}
			}
			else {
				priority = enNotificationMessagePriorityLevel.Normal;
			}

			if (Session == null) {
				return 0;
			}

			var user = Session.GetCurrentUser();
			if (user == null)
				return 0;

			try
			{
				var repository = ObjectFactory.GetInstance<INotificationsRepository>();
				return repository.GetCountMessageByStatus(user.Id, enNotificationStatusMessage.New, priority);
			}
			catch{
				return -1;
			}
		}
    }
}