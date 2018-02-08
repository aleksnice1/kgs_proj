using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Kadastr.CommonUtils;
using Kadastr.DataAccessLayer.Helpers;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.GUI;
using Kadastr.PrintOnTemplate;
using Kadastr.Utils;
using Kadastr.WebApp.Code;
using Kadastr.WebApp.Code.Extensions;
using Kadastr.WebApp.Code.Helpers;
using Kadastr.WebApp.Code.Import;
using Kadastr.WebApp.Code.Import.Importers;
using Kadastr.WebApp.Controls;
using Resources;
using StructureMap;
using System.Text.RegularExpressions;
using System.Text;
using Kadastr.WebApp.Statics;

public partial class ContragentEditor : BaseEditorPage<clsContragent>, IValidationPage
{
	/// <summary>
	/// Редактируемый контрагент
	/// </summary>
	protected override clsContragent EditedEntity
	{
		get
		{
			return ViewState["Contragent"] as clsContragent;
		}
		set
		{
			ViewState["Contragent"] = value;
		}
	}

	/// <summary>
	/// Сделать только для чтения
	/// </summary>
	private void MakeReadOnly()
	{
		btnSave.Visible = false;
		btnSaveAndClose.Visible = false;
		if (EditedEntity != null)
		{
			EditedEntity.ReadOnly = true;
		}
		gvHistory.ReadOnly = true;
	}

	/// <summary>
	/// Заблокировать возможность отредактировать
	/// </summary>
	private void MakeEditingLock(string message)
	{
		btnSave.Enabled = false;
		btnSaveAndClose.Enabled = false;

		LockMessage.Visible = true;
		LockMessage.Text = message;

		gvHistory.ReadOnly = true;
	}

	private void MakeArchive()
	{
		lblIsArchive.Visible = true;
		MakeReadOnly();

		if (OperationType.KindOperationType.KindOperation == KindOperation.Delete ||
			OperationType.KindOperationType.KindOperation == KindOperation.Restore ||
			OperationType.KindOperationType.KindOperation == KindOperation.AddToArchive ||
			OperationType.KindOperationType.KindOperation == KindOperation.ExtractFromArchive ||
            OperationType.KindOperationType.KindOperation == KindOperation.AddDelitedObjectToArchive)
		{
			btnSaveAndClose.Visible = true;
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		try
		{
            ViewState["AddsAttribsSelectedValue"] = ddlAddsAttribs.SelectedValue;
            ViewState["AttachmentsSelectedValue"] = ddlAttachments.SelectedValue;

            if (!IsPostBack)
			{
				InitOperationTypeFromId();
				InitEntities();
				InitEditedEntity();
				InitParentEntity();

				if (OperationType != null)
				{
					if (OperationType.KindOperationType.KindOperation == KindOperation.TemplateCreating)
					//Создание по шаблону
					{
						Session[SessionViewstateConstants.EntityParentObject] = EditedEntity;
					}

					RenameFirstTab(tabTabContainer, OperationType);
					EditedEntity.TypeGuid = OperationType.EntityTypeGuid;
					lblCaption.Text = OperationType.sName;
					operationDateRow.Visible = true;
					operationDate.sDateValue = DateTime.Now.ToShortDateString();
				}
				else
				{
					//закрываем возможность сохранения для не операций
					MakeReadOnly();
				}

				DataBind();

				if (EditedEntity.IsArchive)
				{
					MakeArchive();
					lblActions.Visible = false;
				}
				if (EditedEntity.IsDelete)
					lblIsDeleted.Visible = true;

				if (OperationType != null)
				{
					switch (OperationType.KindOperationType.KindOperation)
					{
						case KindOperation.ExtractFromArchive:
						case KindOperation.AddToArchive:
						case KindOperation.Restore:
						case KindOperation.Delete:
							btnSaveAndClose.Text = "Выполнить операцию";
							btnSave.Visible = pnlActions.Visible = false;
							btnSaveAndClose.CausesValidation = false;
							switch (OperationType.KindOperationType.KindOperation)
							{
								case KindOperation.Delete:
									EditedEntity.IsDelete = true;
									break;
								case KindOperation.Restore:
									EditedEntity.IsDelete = false;
									break;
							}
							break;
						case KindOperation.Edit:
							lblCaption.Text = LocalizationRes.EditionOf + "контрагента";
							break;
						case KindOperation.TemplateCreating:
							txtCopyCount.Visible = lblCopyCount.Visible = true;
							btnSaveAndClose.Text = "Сохранить и Закрыть";
                            pnlActions.Visible = false;
                            btnSave.Visible = true;
                            break;
						case KindOperation.View:
							btnCancel.Text = LocalizationRes.Close;
							MakeReadOnly();
							break;
						case KindOperation.LoadExcel:
							trLoad.Visible = true;
							ImportManager = ExcelImportManager.CreateImportManager(OperationType.ExcelLoadType);
							break;
                        case KindOperation.AddDelitedObjectToArchive:
                            btnSaveAndClose.Text = "Добавить в архив";
                            EditedEntity.IsArchive = true;
                            break;
                    }
				}

				var lockOperationValidator = ObjectFactory.TryGetInstance<ILockOperationValidator>();
				if (lockOperationValidator != null)
				{
					string message;
					User curUser;
					Session.TryGetCurrentUser(out curUser);
					if (!lockOperationValidator.ValidateContragent(curUser, EditedEntity, out message) &&
						OperationType.KindOperationType.KindOperation == KindOperation.Edit)
					{
						MakeEditingLock(message);
					}
				}
			}

			if (IsPostBack && IsExcelImport)
			{
				ExcelImportManager.SetupControls(ImportManager, txtFake, ddlSpreadsheets, upFileLoad);
			}

			FillTabContainer(tabTabContainer, EditedEntity, OperationType);
			BindObject();
		}
		catch (KadastrUserFriendlyException kufEx)
		{
			this.ShowMessage(kufEx.Message);
		}
	}

	private void InitEntities()
	{
		var uid = Request.GetValue<string>("uid");
		if (string.IsNullOrEmpty(uid)) return;

		var selectedId = (string)Session["SelectedID" + uid];
		Session.Remove("SelectedID" + uid);
		if (string.IsNullOrEmpty(selectedId)) return;

		var filter = new[]
			{
				new FilterExpression(FilterPrefix.AND, "IdContragent", LogicalExpression.In, selectedId)
			};
		Entities = ObjectFactory.GetInstance<IBaseDomainEntityRepository>()
			.GetBaseDomainEntitys(typeof(clsContragent), filter);
		gvEntity.DataSource = Entities;
		gvEntity.DataBind();
		trObjects.Visible = true;
	}

	private void InitEditedEntity()
	{
		var idContragent = Request.GetValue<string>("IdContragent");

        if (idContragent != null)
            idContragent = Transform(idContragent);

        if (idContragent != null && !idContragent.Contains("|"))
        {
			EditedEntity = ObjectFactory.GetInstance<IContragentRepository>()
										.GetById(Convert.ToInt64(idContragent));
			gvHistory.sGUID = EditedEntity.GUID.ToString();
			lblType.Text = EditedEntity.Type.sName;

			InitOperationTypeFromEntity();
		}
		else
		{
			EditedEntity = new clsContragent
			{
				IdType = Request.GetValue<int?>("SelectedTypeId"),
				State = ObjectStates.New,
				GUID = Guid.NewGuid()
			};

			if (OperationType != null)
				EditedEntity.TypeGuid = OperationType.EntityTypeGuid;

			if (EditedEntity.Type == null)
			{
				if (OperationType != null)
				{
					EditedEntity.Type = ObjectFactory.GetInstance<IEntityTypeRepository>()
						.GetByGuid(typeof(clsContragentType), OperationType.EntityTypeGuid);
					lblType.Text = EditedEntity.Type.sName;
				}
				else
				{
					var colEntityTypes = ObjectFactory.GetInstance<IEntityTypeRepository>()
						.GetAllEntityTypes(typeof(clsContragentType));
					EditedEntity.Type = colEntityTypes[0];
					lblType.Text = EditedEntity.Type.sName;
				}
			}
			else /*Если тип задан, его менять нельзя*/
			{
				lblType.Text = EditedEntity.Type.sName;
			}
		}
	}

	protected override void CheckOperationTypes()
	{
		if (OperationType != null) return;
		MakeReadOnly();

		tabTabContainer
			.Tabs
			.Cast<AjaxControlToolkit.TabPanel>()
			.Where(tab => tab.ID != MainTabName)
			.DoForEach(t => t.Visible = false);
		throw new KadastrUserFriendlyException("У пользователя нет прав на редактирование и просмотр контрагента");
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		if (!IsPostBack && OperationType != null && Entities == null)
		{
			for (int i = 0; i < OperationType.colSubOperations.Count; i++)
			{
				MenuCreator mc = new MenuCreator();
				MenuItem mi = mc.GetMenuItem(OperationType.colSubOperations[i], this, false);
				if (mi != null)
					menuActions.Items.Add(mi);
			}
		}

		if (!IsPostBack && menuActions.Items.Count == 0)
		{
			pnlActions.Visible = false;
		}
	}

	protected void Page_PreRender(object sender, EventArgs e)
	{
		if (IsExcelImport)
		{
			var enabled = Entities != null && Entities.Count > 0;
			btnSave.Enabled = enabled;
			btnSaveAndClose.Enabled = enabled;
			var importManager = ImportManager;
			btnUnLoad.Enabled = importManager != null && importManager.ImportData.Rows.Count > 0;
		}

		if (phAttrValues.HasControls())
		{
			phAttrValues.Controls[0].SetRestrictionsByOtherAttributes(EditedEntity);
		}
	}

	/// <summary>
	/// Привязывает контрагента к контролам
	/// </summary>
	private void BindObject()
	{
		if (EditedEntity.Type != null)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(EditedEntity.Type.AttributeManager);

			// Определяем, одинаковое ли значение атрибута у всех редактируемых сущностей
			Dictionary<long, string> sameValues = clsCommonHelper.GetSameValuesDictionary(Entities, OperationType);

			phAttrValues.Controls.Clear();
			phAttrValues.Controls.Add(
				ControlFactory.CreateTable(EditedEntity, doc, OperationType,
				IsPostBack ? null : ParentEntity, "Contragent", tpMainData.ClientID,
				tabTabContainer, sameValues));
		}
		if (EditedEntity.Id == null)
			lblName.Text = Entities == null ? "Новый контрагент" : string.Format("Объектов операции: {0}", Entities.Count);
		else
			lblName.Text = EditedEntity.sNameFromAttribute;

        BindAdditionalTab();
        BindLinksTab();
        BindDocTab();
    }

    private void BindAdditionalTab()
    {
        var addsAttribs = EditedEntity.Type.colAttributes
            .Except(EditedEntity.Type.colAttributes.Where(attr => Regex.IsMatch(attr.sName, "delete", RegexOptions.IgnoreCase)))
            .Except(EditedEntity.Type.colTypeAttributes, new clsAttributeEqualityComparerById())
            .Except(EditedEntity.Type.colAditionalAttributes, new clsAttributeEqualityComparerById());

        ddlAddsAttribs.DataSource = addsAttribs;
        ddlAddsAttribs.DataBind();

        TabPanelAddit.Visible = addsAttribs.Count() > 0;

        if (ViewState["AddsAttribsSelectedValue"] != null)
        {
            string selectedCombo = ViewState["AddsAttribsSelectedValue"].ToString();
            if (addsAttribs.Count(attr => attr.Id.ToString() == selectedCombo) > 0)
            {
                ddlAddsAttribs.SelectedValue = selectedCombo;
            }
        }

        if (ViewState["AdditOperation"] != null && ViewState["AdditOperation"].ToString() == "add")
        {
            long attrId;

            if (long.TryParse(ddlAddsAttribs.SelectedValue, out attrId))
            {
                var repository = ObjectFactory.GetInstance<IAttributeRepository>();
                var addsAttrib = repository.GetByIds(new long[] { attrId }).FirstOrDefault();

                EditedEntity.Type.AddAdditionalAttribute(addsAttrib);
            }

            ViewState["AdditOperation"] = "";
        }

        XmlDocument doc2 = null;
        if (!string.IsNullOrEmpty(EditedEntity.Type.XMLaddsAttribs))
        {
            doc2 = new XmlDocument();
            doc2.LoadXml(EditedEntity.Type.XMLaddsAttribs);
        }

        Control ctrlAdds = ControlFactory.CreateAddsTabTable(
            EditedEntity,
            IsPostBack ? null : ParentEntity,
            tpMainData.ClientID,
            OperationType,
            doc2,
            "Contragent",
            OnDeleteAdditionalAttribute);

        phAddsAttribs.Controls.Clear();
        phAddsAttribs.Controls.Add(ctrlAdds);
    }

    public void OnDeleteAdditionalAttribute(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        long idAttr = long.Parse(btn.Attributes["idAttribute"].ToString());
        long idProp = long.Parse(btn.Attributes["propertyId"].ToString());

        clsAdditionalAttributeValue deleteAttr = EditedEntity.colAdditionalAttributeValues
            .FirstOrDefault(attr => attr.IdAttribute == idAttr);

        if (deleteAttr != null)
        {

            EditedEntity.Type.RemoveAdditionalAttribute(deleteAttr.Attribute);
            EditedEntity.colAdditionalAttributeValues.Remove(deleteAttr);
        }
        else
        {
            EditedEntity.Type.RemoveAdditionalAttribute(idAttr);
        }

        BindAdditionalTab();
    }

    public void OnDeleteAttachment(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        long idAtt = long.Parse(btn.Attributes["idAttribute"].ToString());
        long idProp = long.Parse(btn.Attributes["propertyId"].ToString());

        clsAttachment deleteAtt = EditedEntity.colAttachments
            .FirstOrDefault(att => att.Id == idAtt);

        EditedEntity.RemoveAttachment(deleteAtt.Id);

        BindDocTab();
    }

    private void BindLinksTab()
    {
        var linksAttribs = EditedEntity.Type.colAttributes
            .Except(EditedEntity.Type.colAttributes.Where(attr => Regex.IsMatch(attr.sName, "delete", RegexOptions.IgnoreCase)))
            .Where(attr =>
                   attr.AttributeDataType.enDataType == DataType.DocumentEntity
                || attr.AttributeDataType.enDataType == DataType.ContragentEntity
                || attr.AttributeDataType.enDataType == DataType.ProperyEntity);

        Control ctrlLinksTable = ControlFactory.CreateLinksTabTable(EditedEntity, OperationType, "Contragent", linksAttribs);

        phLinks.Controls.Clear();
        phLinks.Controls.Add(ctrlLinksTable);
    }

    private void BindDocTab()
    {
        var docs1Attribs = EditedEntity.Type.colAttributes
            .Except(EditedEntity.Type.colAttributes.Where(attr => Regex.IsMatch(attr.sName, "delete", RegexOptions.IgnoreCase)))
            .Where(attr => attr.AttributeDataType.enDataType == DataType.Attachment);

        var attachRepo = ObjectFactory.GetInstance<IAttachmentRepository>();
        var attachments = attachRepo.GetAll();

        ddlAttachments.DataSource = attachments;
        ddlAttachments.DataBind();

        if (ViewState["AttachmentsSelectedValue"] != null)
        {
            string selectedCombo = ViewState["AttachmentsSelectedValue"].ToString();
            if (attachments.Count(attr => attr.Id.ToString() == selectedCombo) > 0)
            {
                ddlAttachments.SelectedValue = selectedCombo;
            }
        }

        if (ViewState["AttachOperation"] != null && ViewState["AttachOperation"].ToString() == "add")
        {
            long attId;

            if (long.TryParse(ddlAttachments.SelectedValue, out attId))
            {
                var repository = ObjectFactory.GetInstance<IAttachmentRepository>();
                var attachment = repository.GetByIds(new long[] { attId }).FirstOrDefault();

                EditedEntity.AddAttachment(attachment);
            }

            ViewState["AttachOperation"] = "";
        }

        XmlDocument doc = null;
        if (!string.IsNullOrEmpty(EditedEntity.XMLattachments))
        {
            doc = new XmlDocument();
            doc.LoadXml(EditedEntity.XMLattachments);
        }

        Control ctrlDocs1Table = ControlFactory.CreateDocs1TabTable(
            EditedEntity, IsPostBack ? null : ParentEntity, OperationType, "Contragent", docs1Attribs);
        Control ctrlDocs2Table = ControlFactory.CreateDocs2TabTable(
            EditedEntity, IsPostBack ? null : ParentEntity, OperationType, doc, "Contragent", OnDeleteAttachment);

        phDocs1.Controls.Clear();
        phDocs1.Controls.Add(ctrlDocs1Table);

        phDocs2.Controls.Clear();
        phDocs2.Controls.Add(ctrlDocs2Table);
    }

    protected void gvEntity_RowDeleting(object sender, GridViewDeleteEventArgs e)
	{
		if (IsExcelImport)
		{
			var importData = ImportManager.ImportData;
			ExcelImportManager.DeleteItem(OperationType.ExcelLoadType, importData, e.RowIndex, Entities);

			gvEntity.DataSource = importData;
			gvEntity.Columns[0].ControlStyle.CssClass = "DeleteButton";
		}
		else if (gvEntity.DataKeyNames.Length > 0)
		{
			long Id = (long)gvEntity.DataKeys[e.RowIndex].Value;
			for (int i = 0; i < Entities.Count; i++)
				if (Entities[i].Id == Id)
				{
					Entities.RemoveAt(i);
					break;
				}
			gvEntity.DataSource = Entities;
		}
		gvEntity.DataBind();
		lblName.Text = string.Format("Объектов операции: {0}", gvEntity.Rows.Count);
	}

	protected bool Save()
	{
		Dictionary<long, bool> attrToDelete = new Dictionary<long, bool>();
		phAttrValues.FillDeleteDictionary(attrToDelete);

		if (!SetValues(phAttrValues, true, attrToDelete))
			return false;


		string sCheckMessage;
		List<clsContragent> colEditedEntities = null;
		int countCopies;
		int.TryParse(txtCopyCount.Text, out countCopies);

		if (OperationType.KindOperationType.KindOperation == KindOperation.TemplateCreating)
		{
			colEditedEntities = new List<clsContragent>();
			if (countCopies > 0)
			{
				for (var i = 0; i < countCopies; i++)
					colEditedEntities.Add(EditedEntity.CloneWithAttributes());
			}

			sCheckMessage = colEditedEntities.ValidateAttributeValues(EditedEntity.Type.GetNumeratedAttributes());
			if (!String.IsNullOrEmpty(sCheckMessage))
			{
				this.ShowMessage(sCheckMessage);
				return false;
			}
		}
		else
		{

			sCheckMessage = EditedEntity.ValidateAttributeValues(EditedEntity.Type.GetNumeratedAttributes());
			if (!String.IsNullOrEmpty(sCheckMessage))
			{
				this.ShowMessage(sCheckMessage);
				return false;
			}
		}
		var oContragentRepository = ObjectFactory.GetInstance<IContragentRepository>();
		var oOperationRepository = ObjectFactory.GetInstance<IOperationRepository>();
		var curUser = Session.GetCurrentUser();

		if (Entities != null)
		{
			ExcelImportSimpleLoadHelper oExcelExportHelper = null;
			if (IsExcelImport)
				oExcelExportHelper = new ExcelImportSimpleLoadHelper();

			Dictionary<long, string> editedContragentAV = new Dictionary<long, string>();
			foreach (clsAttributeValue av in EditedEntity.colAttributeValues)
				editedContragentAV.Add(av.IdAttribute, av.Value);

			var rule = OperationType.GetRule();
			foreach (clsBaseDomainEntity oCurContragent in Entities)
			{
				string errorMessage;
				if (!oCurContragent.CheckRule(rule, out errorMessage))
				{
					this.ShowMessage(errorMessage + string.Format(" ({0})", oCurContragent.sName));
					return false;
				}

				clsOperation oOperation = new clsOperation(OperationType, curUser, operationDate.DateValue);
				if (IsExcelImport)
				{
					sCheckMessage = oExcelExportHelper.SetImportAttributes(oCurContragent, EditedEntity, ImportManager.ImportData, gvEntity, OperationType.ExcelLoadType, oOperation);
					if (!string.IsNullOrEmpty(sCheckMessage))
					{
						this.ShowMessage(sCheckMessage);
						return false;
					}
				}

				if (OperationType.KindOperationType.KindOperation == KindOperation.Delete
					|| 
                    OperationType.KindOperationType.KindOperation == KindOperation.Restore
                    ||
                    OperationType.KindOperationType.KindOperation == KindOperation.AddDelitedObjectToArchive)
				{
					var entity = oCurContragent as clsContragent;
					if (entity != null)
					{
                        entity.IsDelete = OperationType.KindOperationType.KindOperation == KindOperation.Delete;
                        entity.IsArchive = OperationType.KindOperationType.KindOperation == KindOperation.AddDelitedObjectToArchive;
						entity.State = ObjectStates.Dirty;
					}
				}

				if (OperationType.KindOperationType.KindOperation == KindOperation.AddToArchive)
				{
					clsContragent oContrTmp = oCurContragent as clsContragent;
					if (oContrTmp != null)
					{
						oContrTmp.IsArchive = true;
						oContrTmp.State = ObjectStates.Dirty;
					}
				}
                if (OperationType.KindOperationType.KindOperation == KindOperation.AddToArchive
                    || 
                    OperationType.KindOperationType.KindOperation == KindOperation.Delete
                    ||
                    OperationType.KindOperationType.KindOperation == KindOperation.AddDelitedObjectToArchive)
                {
                    var oPropertyRepository = ObjectFactory.GetInstance<IPropertyRepository>();
                    oPropertyRepository.PropertyByContragentToArhive(oCurContragent.Id);

                }
                if (OperationType.KindOperationType.KindOperation == KindOperation.ExtractFromArchive)
				{
					clsContragent oContrTmp = oCurContragent as clsContragent;
					if (oContrTmp != null)
					{
						oContrTmp.IsArchive = false;
						oContrTmp.State = ObjectStates.Dirty;
					}
				}

				foreach (clsAttributeValue av in EditedEntity.colAttributeValues)
					if (editedContragentAV.ContainsKey(av.IdAttribute))
						av.Value = editedContragentAV[av.IdAttribute];

				clsBaseDomainEntity extendedEditedContragent = clsCommonHelper.GetExtendedValues(EditedEntity, oCurContragent, attrToDelete);
				oOperationRepository.ExecuteOperation(oOperation, oCurContragent, extendedEditedContragent, oContragentRepository);
			}
			SyncAttachments();
			return true;
		}
		else
		{
            phAddsAttribs.FillFromAdditionalTable(EditedEntity, new Dictionary<long, bool>(), true);

            if (OperationType != null)
			{
				string errorMessage;
				if (!EditedEntity.CheckRule(OperationType, out errorMessage))
				{
					this.ShowMessage(errorMessage);
					return false;
				}

				if (OperationType.KindOperationType.KindOperation == KindOperation.AddToArchive)
				{
					EditedEntity.IsArchive = true;
					EditedEntity.State = ObjectStates.Dirty;
				}
				if (OperationType.KindOperationType.KindOperation == KindOperation.ExtractFromArchive)
				{
					EditedEntity.IsArchive = false;
					EditedEntity.State = ObjectStates.Dirty;
				}
             
                if (OperationType.KindOperationType.KindOperation == KindOperation.TemplateCreating && colEditedEntities != null)
				{
					foreach (var editedEntity in colEditedEntities)
					{
						var oOperation = new clsOperation(OperationType, curUser, operationDate.DateValue);
                        if (!oOperationRepository.ExecuteOperation(oOperation, editedEntity, oContragentRepository))
                        {
                            lReqAttrEmpty.Visible = true;
                            return false;
                        }
                        
                       
					}
					return true;

				}
				else //Для одиночной операции над одиночным объектом 
				{

					clsOperation oOperation = new clsOperation(OperationType, curUser, operationDate.DateValue);
					if (oOperationRepository.ExecuteOperation(oOperation, EditedEntity, oContragentRepository))
                    {
                        
                        if (OperationType.KindOperationType.KindOperation == KindOperation.AddToArchive
                            || 
                            OperationType.KindOperationType.KindOperation == KindOperation.Delete
                            ||
                            OperationType.KindOperationType.KindOperation == KindOperation.AddDelitedObjectToArchive)
                        {
                            var oPropertyRepository = ObjectFactory.GetInstance<IPropertyRepository>();
                            oPropertyRepository.PropertyByContragentToArhive(EditedEntity.Id);
                        }
                        else if(OperationType.KindOperationType.KindOperation == KindOperation.Restore
                            || OperationType.KindOperationType.KindOperation == KindOperation.ExtractFromArchive)
                        {
                            var oPropertyRepository = ObjectFactory.GetInstance<IPropertyRepository>();
                            oPropertyRepository.PropertyByContragentFromArhive(EditedEntity.Id);
                        }
                        
                        return true;
					}
					else
					{
						lReqAttrEmpty.Visible = true;
						return false;
					}
				}
			}
			else
				if (oContragentRepository.SaveContragent(EditedEntity))
				{
					//нужно для вычислимых полей
					BindObject();
					return true;
				}
				else
				{
					lReqAttrEmpty.Visible = true;
					return false;
				}
		}
	}

	protected void SaveClick(object sender, EventArgs e)
	{
		if (Save())
		{
			if (OperationType.KindOperationType.KindOperation == KindOperation.New)
			{
				var operationTypeRepositoty = ObjectFactory.GetInstance<IOperationTypeRepository>();
				clsOperationType oContragentEditOperation = operationTypeRepositoty.GetFirstEditOperationForUser(Session.GetCurrentUser(), EditedEntity.Type);
				if (oContragentEditOperation == null)
				{
					btnSave.Visible = false;
					btnSaveAndClose.Visible = false;
				}
				else
				{
					string link = string.Format("ContragentEditor.aspx?OperationID={0}&Operation={1}&IdContragent={2}",
										oContragentEditOperation.Id, oContragentEditOperation.KindOperationType.Name, EditedEntity.Id);
					ScriptManager.RegisterClientScriptBlock(this, GetType(), "_Script_", "location.href='" + link + "';", true);
				}
			}
		}
	}

	protected void SaveAndCloseClick(object sender, EventArgs e)
	{
		if (Save())
		{
            // очистка папки с темповыми файлами
            FilesFactory.ClearingDirectoryFromFolder();
            Session["LastAction"] = "Save";
			ClearResources();
			CloseAndRefreshParent(EditedEntity.Id);
		}
	}

	protected void CancelClick(object sender, EventArgs e)
	{
        // очистка папки с темповыми файлами
        FilesFactory.ClearingDirectoryFromFolder();
        ClearResources();
		CloseAndRefreshParent(EditedEntity.Id);
	}

	private bool SetValues(Control Ctrl, bool withCheck, Dictionary<long, bool> attrToDelete = null)
	{
		if (attrToDelete == null)
			attrToDelete = new Dictionary<long, bool>();

		EditedEntity.GetAVCollection();

		if (!Ctrl.FillFromTable(EditedEntity, attrToDelete, withCheck))
			return false;
		return true;
	}

	protected void lbtnLoad_Click(object sender, EventArgs e)
	{
		// заполнение oEditedProperty значениями с формы
		SetValues(phAttrValues, false);

		lblErrMess.Clear();

		var importManager = ImportManager;
		importManager.Error += importer_Error;
		importManager.LoadData(ddlSpreadsheets.SelectedValue, gvEntity, EditedEntity, this);
		Entities = importManager.ImportEntities;

		lblName.Text = string.Format("Объектов операции: {0}", importManager.ImportEntities.Count);
		trObjects.Visible = true;
	}

	private void importer_Error(object sender, ImportErrorEventArgs e)
	{
		lblErrMess.Text = e.ErrorMessage;
	}

	protected void btnUnLoad_Click(object sender, EventArgs e)
	{
		if (ImportManager == null) return;
		ExcelExportHelper.DoExport(ImportManager.ImportData, gvEntity, ImportManager.ExcelFileName);
	}

	protected void ScriptManager1_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
	{
		var exception = e.Exception;
		ErrorHandlingHelper.HandleError(exception);
		ScriptManager1.AsyncPostBackErrorMessage = HttpUtility.HtmlEncode(exception.Message);
	}

	protected void btnAjaxError_Click(object sender, EventArgs e)
	{
		if (hfError.Value.Length != 0)
		{
			ErrorHandlingHelper.ExceptionMessage = hfError.Value;
		}
		Server.Transfer("~/ErrorPage.aspx");
	}

    private string Transform(string input)
    {
        var builder = new StringBuilder();
        foreach (var symbol in input)
            if (symbol != '?')
                builder.Append(symbol);
            else
                break;
        return builder.ToString();
    }


    protected void AddAdditionalAttribute_Click(object sender, EventArgs e)
    {
        ViewState["AdditOperation"] = "add";

        BindAdditionalTab();
    }

    protected void AddAttachment_Click(object sender, EventArgs e)
    {
        ViewState["AttachOperation"] = "add";

        BindDocTab();
    }
}
