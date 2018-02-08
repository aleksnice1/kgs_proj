using Kadastr.Domain;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.WebApp.Models;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace Kadastr.WebApp.Controllers
{
    public class EntityEditorController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idEntity"></param>
        /// <param name="idOpeartion"></param>
        /// <param name="idEntityType"></param>
        /// <returns></returns>
		public ActionResult Edit(long idEntity, int idOpeartion, string sEntityType)
		{
			#region Получение операции
			var operationRepository = ObjectFactory.GetInstance<IOperationTypeRepository>();
			clsOperationType operationType = operationRepository.GetById(idOpeartion);
			ViewBag.OparerationType = operationType;
			#endregion

			#region Получение редактируемого объекта
			DataType entityType = DataType.ProperyEntity;
			switch (sEntityType) {
				case "Document": entityType = DataType.DocumentEntity; ViewBag.Title = "Редактирование документа";  break;
				case "Contragent": entityType = DataType.ContragentEntity; ViewBag.Title = "Редактирование контрагента"; break;
				case "Property": entityType = DataType.ProperyEntity; ViewBag.Title = "Редактирование имущества"; break;
			}
			var entityRepository = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
			clsBaseDomainEntity entity = entityRepository.GetEntity(entityType, idEntity);
			BaseDomainEntityDTO editedEntity = new BaseDomainEntityDTO(entity);
			editedEntity.ExtendAttributes(operationType);
			#endregion	

			#region Получение группировки атрибутов
			List<long> calcArendaAttributeIds = (from attribute in entity.Type.colAttributes
												 where attribute.AttributeDataType.enDataType == DataType.Arenda || attribute.AttributeDataType.enDataType == DataType.DeferredPayment
												 select attribute.Id).ToList(); // entity.Type.colAttributes.Select(attr => attr.AttributeDataType.enDataType == DataType.Arenda || attr.AttributeDataType.enDataType == DataType.DeferredPayment);
			
			XmlDocument xmlSchema = new XmlDocument();
			xmlSchema.LoadXml(entity.Type.AttributeManager);
			AttributesOrder attributesOrder = new AttributesOrder(xmlSchema, calcArendaAttributeIds);
			ViewBag.AttributesOrder = attributesOrder;
			#endregion

			return View("Edit", editedEntity);
        }

		[ValidateInput(false)]
		public ActionResult SaveEntity(BaseDomainEntityDTO domainEntityDTO, [Bind(Prefix = "OperationDate")] AttributeValueDTO operationDate)
		{
			return View("Edit", domainEntityDTO);
		}
    }

	public class AttributesOrder
	{
		public bool HasCalculationArendaAttribute { get; set; }
		public List<AttributesGroup> Groups { get; set; }

		public AttributesOrder() {
			this.HasCalculationArendaAttribute = false;
			this.Groups = new List<AttributesGroup>();
		}

		public AttributesOrder(XmlDocument xml, List<long> calcArendaIds)
		{
			this.HasCalculationArendaAttribute = false;
			this.Groups = new List<AttributesGroup>();
			
			bool hasCalc = false;
			foreach (XmlNode GroupNode in xml.DocumentElement.SelectNodes("AttributeGroup"))
			{
				AttributesGroup group = new AttributesGroup(GroupNode, calcArendaIds, out hasCalc);
				this.HasCalculationArendaAttribute = this.HasCalculationArendaAttribute || hasCalc;
				this.Groups.Add(group);
			}			
		}
	}

	public class AttributesGroup 
	{
		public string Name { get; set; }
		public List<long> Attributes { get; set; }

		public AttributesGroup() {
			this.Name = string.Empty;
			this.Attributes = new List<long>();
		}

		public AttributesGroup(XmlNode node, List<long> calcArendaIds, out bool hasCalculationArendaAttribute)
		{
			hasCalculationArendaAttribute = false;
			
			this.Name = node.Attributes["Caption"].Value;
			this.Attributes = new List<long>();

			XmlNodeList Columns = node.SelectNodes("GroupColumn");
			foreach (XmlNode ColumnNode in Columns)
			{
				foreach (XmlNode AttributeNode in ColumnNode.SelectNodes("Attribute"))
				{ 
					long attrId;
					if(Int64.TryParse(AttributeNode.Attributes["name"].Value, out attrId))
					{
						if (calcArendaIds.Exists(id => id == attrId)) 
						{
							hasCalculationArendaAttribute = true;
							continue;
						}
						
						this.Attributes.Add(attrId);
					}
				}
			};
		}
	}
}
