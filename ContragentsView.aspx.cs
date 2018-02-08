using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using Kadastr.GUI;
using Kadastr.Utils;
using StructureMap;
using System.Linq;
using Kadastr.WebApp.Code;
using Kadastr.WebApp.Code.Extensions;
using System.Reflection;
using Kadastr.WebApp.Controls;
using System.Web.UI.HtmlControls;

public partial class ContragentsView : EntityViewWithGroupingBase
{
    /// <summary>
    /// Содержит тип категории, переопределяется для каждой сущности
    /// </summary>
    protected override Type typEntityCategory
    {
        get { return typeof(clsCategoryContragents); }
    }

    /// <summary>
    /// Содержит тип сущности, отображение которой настраиваем
    /// </summary>
    protected override Type typEntity
    {
        get { return typeof(clsContragentType); }
    }

    protected List<clsOperationType> OperationTypes
    {
        get { return ViewState["OperationType"] as List<clsOperationType>; }
    }
    
    protected List<long> SelectedID
    {
        get { return gvEntity.colSelectedID; }
        set { gvEntity.colSelectedID = value; }
    }

	protected override clsEntityGridView GroupingGrid
	{
		get { return gvGrouping; }
	}

	protected override UpdatePanel GroupingUpdatePanel
	{
		get { return upGrouping; }
	}

	protected override clsEntityGridView EntityGrid
	{
		get { return gvEntity; }
	}

    protected override GridView FilterGrid
    {
        get { return tableFilter; }
        set { tableFilter = value; }
    }

    protected override LinkButton BtnShowSummaryRow
	{
		get { return btnShowSummaryRow; }
	}

	protected override UpdatePanel EntityUpdatePanel
	{
		get { return upEntities; }
	}

    protected void Page_Load(object sender, EventArgs e)
    {
		Session[SessionViewstateConstants.FromViewPage] = Request.UrlReferrer.LocalPath.ToString() != "/EntityBookmarkPage.aspx";
        ClientScript.RegisterClientScriptBlock(this.GetType(), "RefreshFunction", "<script>javascript:function RefreshClick(){document.getElementById(" + upEntityPanel.ClientID + ").Update()}</script>");

        //gvEntity.LocalFilterChanged += GvEntity_LocalFilterChanged;
    }

    private void GvEntity_LocalFilterChanged(object sender, EventArgs e)
    {
        uctlLocalFilter filterControl = sender as uctlLocalFilter;
        Dictionary<string, string> filter = filterControl.Filters;

        DataTable table = filterControl.DataSource;
        EnumerableRowCollection<DataRow> query = null;

        if (table != null)
        {
            foreach (var dict in filter)
            {
                if (query == null)
                {
                    query = from t in table.AsEnumerable()
                            where t.Field<string>(dict.Key) != null ? t.Field<string>(dict.Key).Contains(dict.Value) : false
                            select t;
                }
                else
                {
                    query = from t in query
                            where t.Field<string>(dict.Key) != null ? t.Field<string>(dict.Key).Contains(dict.Value) : false
                            select t;
                }
            }
        }

        if (query != null && query.Count() > 0)
        {
            upEntities.Update();
            DataView view = query.AsDataView();
            gvEntity.SetBaseDatasource(view);

            if (view.Count == 0)
            {

            }
            else
            {
                this.DataBind();

                uctlLocalFilter localFilter = gvEntity.TopPagerRow.Page.LoadControl("~/Controls/uctlLocalFilter.ascx") as uctlLocalFilter;
                if (localFilter != null)
                {
                    HtmlTable ht = null;
                    if (gvEntity.TopPagerRow.HasControls() && gvEntity.TopPagerRow.Controls[0].HasControls())
                    {
                        ht = gvEntity.TopPagerRow.Controls[0].Controls[0] as HtmlTable;
                    }

                    if (ht != null)
                    {
                        Panel localFilterPanel = (Panel)ht.Rows[1].Cells[0].FindControl("localFilterPanel");
                        localFilterPanel.Controls.Clear();
                        localFilterPanel.Controls.Add(localFilter);
                    }

                    localFilter.InitControls(view.ToTable());
                }
            }
        }
    }

    protected void Page_LoadComplete(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Control InitialContr = null;
            GetControl(ref InitialContr, this.Master, Request.QueryString["ClientID"]);

            WebControl InitialControl = InitialContr as WebControl;

            ///Если не нашли инициирующую ссылку - то на выход
            if (InitialControl == null)
            {
				throw new ArgumentNullException(@"InitialControl");
			}

			RenameCurrentSiteMapNode(((LinkButton)InitialControl).Text);
            InitialControl.Font.Bold = true;
            ///Если в инициурующей ссылке нет атрибута с настройками отображения, то на выход
            if (InitialControl.Attributes["Settings"] == null)
            {
				throw new ArgumentNullException(@"InitialControl.Attributes[""Settings""]");
			}

            Settings = InitialControl.Attributes["Settings"];
            AnalizeXML(Settings);

            if (IsGroping)
            {
                CreateGroupingView();
                mvMultiView.ActiveViewIndex = 0;
            }
            else
            {
                CreateEntityView();
                mvMultiView.ActiveViewIndex = 1;
            }


        }
    }


    #region CreateGroupingView() - Настраивает вид группировки
    /// <summary>
    /// Настраивает вид группировки
    /// </summary>
    private void CreateGroupingView()
    {
        ///Группировка по справочнику или мультисправочнику
        if ((GroupingAttribute.AttributeDataType.sDataTypeCode == DataType.Dictionary.ToString()) || (GroupingAttribute.AttributeDataType.sDataTypeCode == DataType.MultiDictionary.ToString()))
        {
            var oDictValuesRepository = ObjectFactory.GetInstance<IDictionaryValuesRepository>();
            clsDictionaryValues[] colDictValues = oDictValuesRepository.GetDictionaryValueByDicId(GroupingAttribute.IdDictionary);
            gvGrouping.Columns.Clear();
			gvGrouping.AutoGenerateColumns = false;
            gvGrouping.DataKeyNames = new string[] { "Id" };
            BoundField bf = new BoundField();
            bf.DataField = "sValue";
            gvGrouping.Columns.Add(bf);
            gvGrouping.DataSource = colDictValues;
            gvGrouping.DataBind();
        }
        /// Группировка по какой-либо сущности
        else
        {
            ctrlFilterAndSortingControl1.Visible = true;
            Type type = GetGroupingType();
            if (type != null)
            {
                FillGroupingDDL(type);
            }
        }
    }
    
    #endregion


    /// <summary>
    /// Настраивает вид отображаемой сущности
    /// </summary>
    private void CreateEntityView()
    {
        if (IsFiltering)
        {
            FillEntityTypesDDL(typEntity);
        }
    }


    #region FillGroupingDDL - Заполняет ддл выбора типов группирующего атрибута
    /// <summary>
    /// Заполняет ддл выбора типов группирующего атрибута
    /// </summary>
    /// <param name="GroupingType"></param>
    private void FillGroupingDDL(Type GroupingType)
    {
        dicGroupingEntityViewDictionary.Clear();
        ///Нет ограничения по типам, все типы используются
        if (IsAllGroupingTypes)
        {
            var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
            clsEntityType[] colEntityTypes = oEntityTypeRepository.GetAllEntityTypes(GroupingType);
            var oEntityViewSettingRepository = ObjectFactory.GetInstance<IEntityViewSettingRepository>();
            dicGroupingEntityViewDictionary = oEntityViewSettingRepository.GetDictionaryForFilterWithDefaultSettings(colEntityTypes);
        }
        ///Есть ограничение по типам
        else
        {
            var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
            clsEntityType[] colEntityTypes = oEntityTypeRepository.GetAllEntityTypes(GroupingType);
            var oEntityViewSettingRepository = ObjectFactory.GetInstance<IEntityViewSettingRepository>();

            foreach (clsEntityType oEntityType in colEntityTypes)
            {
                if (colGroupingTypesIdViewId.ContainsKey(oEntityType.Id))
                {
                    int IdView = colGroupingTypesIdViewId[oEntityType.Id];
                    if (IdView != 0)
                        dicGroupingEntityViewDictionary.Add(oEntityType, oEntityViewSettingRepository.GetEntityView(IdView));
                }
            }
        }
        ctrlFilterAndSortingControl1.dicEntityTypesWithView = dicGroupingEntityViewDictionary;
    }
    
    #endregion

    #region FillEntityTypesDDL - Заполняет ддл выбора типов сущности
    /// <summary>
    /// Заполняет ддл выбора типов сущности
    /// </summary>
    private void FillEntityTypesDDL(Type FilteringType)
    {
        dicEntityViewDictionary.Clear();

        ///Нет ограничения по типам, все типы используются
        if (IsAllFilteringTypes)
        {
            var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
            clsEntityType[] colEntityTypes = oEntityTypeRepository.GetAllEntityTypes(FilteringType);
            var oEntityViewSettingRepository = ObjectFactory.GetInstance<IEntityViewSettingRepository>();
            dicEntityViewDictionary = oEntityViewSettingRepository.GetDictionaryForFilterWithDefaultSettings(colEntityTypes);
        }
        ///Есть ограничение по типам
        else
        {
            var oEntityTypeRepository = ObjectFactory.GetInstance<IEntityTypeRepository>();
            clsEntityType[] colEntityTypes = oEntityTypeRepository.GetAllEntityTypes(FilteringType);
            var oEntityViewSettingRepository = ObjectFactory.GetInstance<IEntityViewSettingRepository>();
            
            foreach (clsEntityType oEntityType in colEntityTypes)
            {
                if (colEntityTypesIdViewId.ContainsKey(oEntityType.Id))
                {
                    int IdView = colEntityTypesIdViewId[oEntityType.Id];
                    ViewState["IdView"] = IdView;
                    ViewState["EntityTypeId"] = oEntityType.Id;

                    clsEntityViewSetting entityViewSetting =
                        oEntityViewSettingRepository.GetEntityViewByIdOrDefaultView(IdView, oEntityType);

                    ApplyUserEntityView(entityViewSetting, IdView);

                    dicEntityViewDictionary.Add(oEntityType, entityViewSetting);
                }
            }
        }

        ctrlFilterAndSortingControl2.dicEntityTypesWithView = dicEntityViewDictionary;
    }

    #endregion


    protected void RefreshClick(object sender, EventArgs e)
    {
        this.ctrlFilterAndSortingControl2.dicEntityTypesWithView = dicEntityViewDictionary;
    }

    protected void AddClick(object sender, EventArgs e)
    {
        if (typEntity != null)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "_Script_", clsCommonHelper.sGetWindowOpen("ContragentEditor.aspx?SelectedTypeId=" + ctrlFilterAndSortingControl2.ddlEntityTypeSelector.SelectedValue), true);

        }
    }

    protected void EditClick(object sender, EventArgs e)
    {
        if (gvEntity.SelectedDataKey != null)
        {
            gvEntity.OpenDefaultOperation(sender, e);
        }

    }

    protected void GroupingGridDblClick(object sender, EventArgs e)
    {
        string sName = string.Empty;
        if (GroupingAttribute.AttributeDataType.enDataType == DataType.Dictionary || GroupingAttribute.AttributeDataType.enDataType == DataType.MultiDictionary)
        {
            sName = gvGrouping.SelectedRow.Cells[0].Text;
        }
        else
        {
            var entityRepository = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
            switch (GroupingAttribute.AttributeDataType.enDataType)
            {
                case DataType.ContragentEntity:
                    sName = entityRepository.GetNameEntity(new clsContragentType(), (long)gvGrouping.SelectedDataKey.Value);
                    break;
                case DataType.DocumentEntity:
                    sName = entityRepository.GetNameEntity(new clsDocumentType(), (long)gvGrouping.SelectedDataKey.Value);
                    break;
                case DataType.ProperyEntity:
                    sName = entityRepository.GetNameEntity(new clsPropertyType(), (long)gvGrouping.SelectedDataKey.Value);
                    break;
            }
        }

        SelectedGroupingId = (long)gvGrouping.SelectedDataKey.Value;

        CreateEntityView();
        mvMultiView.ActiveViewIndex = 1;

        ViewState["ActiveViewIndex"] = mvMultiView.ActiveViewIndex;

        AddContragentToSiteMap(sName);
    }

	protected override IEnumerable<FilterAttributeValue> GetFilterByParentEntity()
	{
		return Enumerable.Empty<FilterAttributeValue>();
	}

    protected void gvEntity_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
		GridView oGrid = (GridView)sender;
		var oBaseDomainEntityRepository = ObjectFactory.GetInstance<IBaseDomainEntityRepository>();
		clsResultAllEntity oResultAllEntity = oBaseDomainEntityRepository.GetAllEntities(oEntityType, oView, colFV, colSortings, oView.IsShowDeleted,
			oGrid.PageSize * e.NewPageIndex + 1, oGrid.PageSize * e.NewPageIndex + oGrid.PageSize, oView.IsDisplaySummaryAttributes);
		gvEntity.DataSource = oResultAllEntity;
		gvEntity.PageIndex = e.NewPageIndex;
		gvEntity.ShowFooter = oView.IsDisplaySummaryAttributes;
		gvEntity.DataBind();
		if (oView.IsDisplaySummaryAttributes)
			MakeGridSummary(gvEntity, oView, oResultAllEntity.oResult);
    }

    protected void ExportClick(object sender, EventArgs e)
    {
		SetExportData();
		this.ShowExportPage();
    }

    protected void lbConfigureUserView_Click(object sender, EventArgs e)
    {
        User currentUser;
        Session.TryGetCurrentUser(out currentUser);

        string idVew = ViewState["IdView"].ToString();
        string entityTypeId = ViewState["EntityTypeId"].ToString();

        ScriptManager.RegisterStartupScript(Page, typeof(Page), "Open user view disigner",
            string.Format(
                "javascript:window.open('UserEntityTypeEditor.aspx?UserId={0}&ViewId={1}&EntityTypeId={2}&EntityCategory={3}', '', 'width=500,height=650');",
                currentUser.Id.ToString(), idVew, entityTypeId,
                DataType.ContragentEntity.ToString()), true);
    }
}
