using Kadastr.CommonUtils;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace Kadastr.WebApp.Models
{
	public class AttributeValueDTO
	{
		[HiddenInput(DisplayValue = false)]
		public long Id { get; set; }					
		[HiddenInput(DisplayValue = false)]
		public string Type { get; set; }
		[HiddenInput(DisplayValue = false)]
		public bool Required { get; set; }
		[HiddenInput(DisplayValue = false)]
		public bool MultiLine { get; set; }
		[HiddenInput(DisplayValue = false)]	
		public long IdAttribute { get; set; }
		[HiddenInput(DisplayValue = false)]
		public string AttributeDataType { get; set; }

		[HiddenInput(DisplayValue = false)]
		public bool IsMultiple { get; set; }
		[HiddenInput(DisplayValue = false)]
		public bool IsCalculate { get; set; }

		[HiddenInput(DisplayValue = false)]
		public long IdDictionary { get; set; }

		[HiddenInput(DisplayValue = false)]
		public string AvailableTypes { get; set; }

		public string AttributeName { get; set; }
		public string Value { get; set; }
		
		public AttributeValueDTO()
		{
		}

		public AttributeValueDTO(clsAttributeValue av, long idParentEntity = 0)
		{
			this.Id = av.Id;
			this.IdAttribute = av.IdAttribute;
			this.Type = av.Type.ToString();
			this.Value = av.Value;

			if (av.Attribute.AttributeDataType.enDataType == DataType.Dictionary)
			{
				string value = av.MainValue;
				string name = GetDictionaryValueName(value);

				this.Value = string.Format("<AttributeValue><MainValue>{1}</MainValue><SubValues><SubValue Id=\"{0}\" Name=\"{1}\"></SubValue></SubValues></AttributeValue>", value, name);
			}

			if (av.Attribute.AttributeDataType.enDataType == DataType.DocumentEntity ||
				av.Attribute.AttributeDataType.enDataType == DataType.ContragentEntity ||
				av.Attribute.AttributeDataType.enDataType == DataType.ProperyEntity)
			{
				StringBuilder stringValue = new StringBuilder();
				stringValue.Append("<AttributeValue><MainValue></MainValue><SubValues>");
				foreach (SubValue subvalue in av.SubValues) {
					string value = subvalue.Id.ToString();
					string name = string.IsNullOrEmpty(subvalue.Name) ? GetEntityName(av.Attribute.AttributeDataType.enDataType, subvalue.Id) : subvalue.Name;
					stringValue.AppendFormat("<SubValue Id=\"{0}\" Name=\"{1}\"></SubValue>", value, name);
				}
				stringValue.Append("</SubValues></AttributeValue>");
				this.Value = stringValue.ToString();

				#region Доступные типа для выбора в модальном окне
				this.AvailableTypes = string.Empty;
				if (av.Attribute.Restriction == null || av.Attribute.Restriction.AvailableTypes.Count == 0)
				{
					this.AvailableTypes = "All";
				}
				else
				{
					try
					{
						foreach (var availableType in av.Attribute.Restriction.AvailableTypes)
						{
							if (this.AvailableTypes.Length > 0)
								this.AvailableTypes += "|";
							this.AvailableTypes += availableType.Id.ToString();
							this.AvailableTypes += ":";
							this.AvailableTypes += availableType.IdView.ToString();
						}
					}
					catch
					{
						this.AvailableTypes = "All";
					}
				}
				#endregion
			}
			this.IsCalculate = av.Attribute.IsCalculation;
			this.MultiLine = av.Attribute.IsMultiLine;
			this.Required = av.Attribute.IsRequired;
			this.AttributeDataType = av.Attribute.AttributeDataType.enDataType.ToString();
			this.AttributeName = av.Attribute.sName;
			this.IsMultiple = av.Attribute.IsMultiSelect;
			this.IdDictionary = (av.Attribute.AttributeDataType.enDataType == DataType.MultiDictionary || av.Attribute.AttributeDataType.enDataType == DataType.Dictionary) ? av.Attribute.IdDictionary : 0;
		}

		public AttributeValueDTO(clsAttribute attribute, long idParentEntity = 0)
		{
			this.Id = 0;
			this.IdAttribute = attribute.Id;
			this.Type = attribute.AttributeDataType.enDataType.ToString();
			this.Value = "<AttributeValue><MainValue></MainValue></AttributeValue>";

			this.MultiLine = attribute.IsMultiLine;
			this.Required = attribute.IsRequired;
			this.AttributeDataType = attribute.AttributeDataType.enDataType.ToString();
			this.AttributeName = attribute.sName;
			this.IsMultiple = attribute.IsMultiSelect;
			this.IdDictionary = (attribute.AttributeDataType.enDataType == DataType.MultiDictionary || attribute.AttributeDataType.enDataType == DataType.Dictionary) ? attribute.IdDictionary : 0;

			if (attribute.IsCalculation && idParentEntity != 0)
			{
				this.Value = string.Format("<AttributeValue><MainValue>{0}</MainValue></AttributeValue>", GetCalculationValue(attribute, idParentEntity));
			}
			this.IsCalculate = attribute.IsCalculation;

			#region Доступные типа для выбора в модальном окне
			this.AvailableTypes = string.Empty;
			if (attribute.Restriction == null || attribute.Restriction.AvailableTypes.Count == 0)
			{
				this.AvailableTypes = "All";
			}
			else
			{
				try
				{
					foreach (var availableType in attribute.Restriction.AvailableTypes)
					{
						if (this.AvailableTypes.Length > 0)
							this.AvailableTypes += "|";
						this.AvailableTypes += availableType.Id.ToString();
						this.AvailableTypes += ":";
						this.AvailableTypes += availableType.IdView.ToString();
					}
				}
				catch
				{
					this.AvailableTypes = "All";
				}
			}
			#endregion
		}

		public DataType GetDataType()
		{
			DataType result;
			if (Enum.TryParse(Type, true, out result))
			{
				return result;
			}
			return DataType.Undefined;
		}

		public clsAttributeValue GetAttributeValue() 
		{
			clsAttributeValue result = new clsAttributeValue();
			result.Id = this.Id;
			result.IdAttribute = IdAttribute;
			result.Value = this.Value;
			return result;
		}

		private string GetEntityName(DataType entityType, long entityId) {
			switch (entityType)
			{
				case DataType.DocumentEntity:
					{
						var repository = ObjectFactory.GetInstance<IDocumentRepository>();
						clsDocument document;
						if (repository.TryGetById(entityId, out document) && document != null)	
						{
							return document.sName;
						};
					}; break;
				case DataType.ContragentEntity:
					{
						var repository = ObjectFactory.GetInstance<IContragentRepository>();
						clsContragent contragent;
						if (repository.TryGetById(entityId, out contragent) && contragent != null)
						{
							return contragent.sName;
						};
					}; break;
				case DataType.ProperyEntity:
					{
						var repository = ObjectFactory.GetInstance<IPropertyRepository>();
						clsProperty property;
						if (repository.TryGetById(entityId, out property) && property != null)
						{
							return property.sName;
						};
					}; break;
			}

			return WebApp.Properties.Resources.EntityNotFound;
		}
		private string GetDictionaryValueName(string sValue)
		{
			long idDV;
			if (Int64.TryParse(sValue, out idDV))
			{
				var oDictValuesRepository = ObjectFactory.GetInstance<IDictionaryValuesRepository>();
				clsDictionaryValues oDictionaryValue;
				if (oDictValuesRepository.TryGetDictionaryValueById(idDV, out oDictionaryValue))
				{
					return oDictionaryValue.sValue;
				};
			};

			return WebApp.Properties.Resources.DictionaryNotFound;
		}
		private string GetCalculationValue(clsAttribute oAttribute, long idParentEntity)
		{
			if (oAttribute != null && oAttribute.IsCalculation && !String.IsNullOrEmpty(oAttribute.sViewTableName) && idParentEntity != 0)
			{
				string result;
				var oStoredProcRep = ObjectFactory.GetInstance<IStoredProcedureRepository>();
				result = oStoredProcRep.GetValueFromView(idParentEntity, oAttribute.sViewTableName);
				return result;
			}
			return null;
		}
	}	
}