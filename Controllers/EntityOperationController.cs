using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Kadastr.WebApp.Controllers
{
	public class EntityOperationController : Controller
	{
		[HttpPost]
		public JsonResult GetOperationLink(long idEntity, int entityType, int? IdClassView, int? IdEntityView)
		{
			if (idEntity <= 0)
				throw new ArgumentException("idEntity");

			if (entityType <= 0)
				throw new ArgumentException("entityType");

			if (HttpContext.Session["CurrentUser"] == null)
				return null;
			Kadastr.Domain.User user = HttpContext.Session["CurrentUser"] as Kadastr.Domain.User;
			string link = string.Empty;
			DataType enEntityType = DataType.ProperyEntity;
			string entityName = string.Empty;
			clsBaseDomainEntity entity = null;
			try
			{
				FillEntityTypeAndName(out enEntityType, out entityName, entityType);
				var baseEntityRepository = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
				entity = baseEntityRepository.GetEntity(enEntityType, idEntity);
				if (IdClassView.HasValue && IdClassView.Value != 0)
					link = GetLinkFromClassViewSettingsForUser((int)IdClassView, user, entityName, entity);
				else if (IdEntityView.HasValue)
					link = GetLinkForEntityViewSettingsForUser((int)IdEntityView, user, entity, entityName);
				else
					link = GetLinkFromOperationTypesForUser(user, entity, entityName);
			}
			catch (Exception iop)
			{
				return Json(new { errorMessage = iop.Message });
			}
			return Json(new { link = link });
		}

		private string GetLinkForEntityViewSettingsForUser(int idEntityView, User user, clsBaseDomainEntity entity, string entityName)
		{
			var classViewRepo = ObjectFactory.GetInstance<IEntityViewSettingRepository>();
			var entityView = classViewRepo.GetEntityViewByIdOrDefaultView(idEntityView, entity.Type);
			if(entityView == null)
				throw new InvalidOperationException("Не найдено представление с Id " + idEntityView);
			var defaultOperation = GetEditOrViewOperationFromOperationTypes(entityView, user, entity);
			if(defaultOperation == null)
				throw new InvalidOperationException("Не удалось получить операцию для пользователя " + user.sName + ". Проверьте настройки представления");
			return string.Format("{0}Editor.aspx?OperationID={1}&Operation={2}&Id{3}={4}"
								, entityName
								, defaultOperation.Id
								, defaultOperation.KindOperationType.Name
								, entityName
								, entity.Id);
		}

		private clsOperationType GetEditOrViewOperationFromOperationTypes(clsEntityViewSetting entityView, User user, clsBaseDomainEntity entity)
		{
			Dictionary<KindOperation, clsOperationType> defaultDoubleClickOperations = new Dictionary<KindOperation, clsOperationType>();
			var operationTypesRepo = ObjectFactory.GetInstance<IOperationTypeRepository>();
			var userOperations = operationTypesRepo.GetUsersOperations(user, entity.Type).ToList();

			foreach (var operationType in entityView.GetOpeartionTypesFromViewSettings())
			{
				if ((operationType.KindOperationType.KindOperation == KindOperation.Edit
					|| operationType.KindOperationType.KindOperation == KindOperation.View) &&
					(operationType.sName.StartsWith("Редакт") || operationType.sName.StartsWith("Просмот")))
				{
					bool isOperationAllowed = userOperations.Select(t => t.Id).Contains(operationType.Id);
					if (isOperationAllowed)
					{
						if (!defaultDoubleClickOperations.ContainsKey(operationType.KindOperationType.KindOperation))
							defaultDoubleClickOperations.Add(operationType.KindOperationType.KindOperation, operationType);
					}
				}
			}
			return FindDefaultOperationInOperationTypes(defaultDoubleClickOperations);
		}

		private clsOperationType FindDefaultOperationInOperationTypes(Dictionary<KindOperation, clsOperationType> defaultDoubleClickOperations)
		{
			if (defaultDoubleClickOperations.ContainsKey(KindOperation.Edit))
				return defaultDoubleClickOperations[KindOperation.Edit];
			else if (defaultDoubleClickOperations.ContainsKey(KindOperation.View))
				return defaultDoubleClickOperations[KindOperation.View];
			else
				return null;
		}

		private string GetLinkFromOperationTypesForUser(User user, clsBaseDomainEntity entity, string entityName)
		{
			var operationTypeRepository = ObjectFactory.GetInstance<IOperationTypeRepository>();
			clsOperationType entityOperation = operationTypeRepository.GetDefaultOperation(user, entity.Type);
			return string.Format("{0}Editor.aspx?OperationID={1}&Operation={2}&Id{3}={4}"
											, entityName
											, entityOperation.Id
											, entityOperation.KindOperationType.Name
											, entityName
											, entity.Id);
		}

		private string GetLinkFromClassViewSettingsForUser(int idClassView, User user, string entityName, clsBaseDomainEntity entity)
		{
			var classViewRepo = ObjectFactory.GetInstance<IClassViewSettingsRepository>();
			var classViewSettings = classViewRepo.GetById((int)idClassView);
			var defaultUserOperation = GetEditOrViewOperationFromUnitedOperationType(classViewSettings, user);
			if (defaultUserOperation == null)
				throw new InvalidOperationException("Не удалось получить операцию для пользователя " + user.sName + ". Проверьте настройки представления");
			return string.Format("{0}Editor.aspx?OperationID={1}&Operation={2}&Id{3}={4}"
											, entityName
											, defaultUserOperation.dicEntityTypeOperationType[entity.Type.EntityGuid]
											, defaultUserOperation.KindOperationType.Name
											, entityName
											, entity.Id);
		}

		private void FillEntityTypeAndName(out DataType enEntityType, out string entityName, int entityType)
		{
			switch (entityType)
			{
				case 1:
					enEntityType = DataType.ProperyEntity;
					entityName = "Property";
					break;
				case 2:
					enEntityType = DataType.ContragentEntity;
					entityName = "Contragent";
					break;
				case 3:
					enEntityType = DataType.DocumentEntity;
					entityName = "Document";
					break;
				default:
					throw new InvalidOperationException("Некорректный тип сущности");
			}
		}

		private clsUnitedOperationType GetEditOrViewOperationFromUnitedOperationType(clsClassViewSettings classViewSettings, User user)
		{
			Dictionary<KindOperation, clsUnitedOperationType> defaultDoubleClickOperations = new Dictionary<KindOperation, clsUnitedOperationType>();
			foreach (clsUnitedOperationType oOperationType in classViewSettings.GetOpeartionTypesFromViewSettings())
			{
				if ((oOperationType.KindOperationType.KindOperation == KindOperation.Edit
					|| oOperationType.KindOperationType.KindOperation == KindOperation.View) &&
					(oOperationType.sName.StartsWith("Редакт") || oOperationType.sName.StartsWith("Просмот")))
				{
					
					bool isOperationAllowed = clsClassViewSettings.CheckUnitedOperationAllowed(user, oOperationType);
					//если есть доступ
					if (isOperationAllowed)
					{
						if (!defaultDoubleClickOperations.ContainsKey(oOperationType.KindOperationType.KindOperation))
							defaultDoubleClickOperations.Add(oOperationType.KindOperationType.KindOperation, oOperationType);
					}
				}
			}
			return FindDefaultOperationInUnitedOperationTypes(defaultDoubleClickOperations);
		}

		private clsUnitedOperationType FindDefaultOperationInUnitedOperationTypes(Dictionary<KindOperation, clsUnitedOperationType> defaultDoubleClickOperations)
		{
			if (defaultDoubleClickOperations.ContainsKey(KindOperation.Edit))
				return defaultDoubleClickOperations[KindOperation.Edit];
			else if (defaultDoubleClickOperations.ContainsKey(KindOperation.View))
				return defaultDoubleClickOperations[KindOperation.View];
			else
				return null;
		}
	}
}
