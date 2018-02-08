<%@ 
	Page Language="C#" AutoEventWireup="true" Inherits="ContragentEditor" Title="Контрагент"  ValidateRequest="false" 
	Codebehind="ContragentEditor.aspx.cs" MasterPageFile="~/EntityEditorMaster.master"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="EditorHead" runat="Server">
    <%@ Register Src="Controls/ctlSaveCancel.ascx" TagName="ctlSaveCancel" TagPrefix="ctrl" %>

	<title>Контрагент</title>
	<base target="_self" />
    <meta http-equiv="Pragma" content="no-cache" />

	<asp:PlaceHolder runat="server">
	<link type="text/css" href="<%: System.Web.Optimization.BundleTable.Bundles.ResolveBundleUrl("~/bundles/ContragentEditorCss") %>" rel="Stylesheet">
	<script type="text/javascript" src="<%: System.Web.Optimization.BundleTable.Bundles.ResolveBundleUrl("~/bundles/ContragentEditorJs") %>"></script>
	
	</asp:PlaceHolder>
	<style type="text/css">
		div.content_row .item_description.UO-header-description {
			width: 115px;
		}
		div.content_row .UO-header-value span {
			padding-left: 4px !important;
		}
		div.content_row .UO-header-value.property-type span {
			padding-left: 0px !important;
		}
		div.content_row .UO-header-value {
			margin-left: 10px !important;
		}
	</style>
    <script type="text/javascript"> 
        function SetTabHelper() {
            var tabHelper = getTabHelper();
            tabHelper.css("display", "none");
        }

        /*
            функция, предназначенная для обновления текущей (bookmark) вкладки: 
            происходит переключение на пустую  (tabHelper) вкладку, а затем обратно на ту, которую нужно обновить
        */
        function handleEndRequest() {
            SetTabHelper();
            var tabHelper = getTabHelper();
            if (!tabHelper)
                c.err("Не определен объект tabHelper!");
            var currentTabIndex = $find("<%=tabTabContainer.ClientID%>").get_activeTabIndex();
            $find("<%=tabTabContainer.ClientID%>").set_activeTabIndex(tabHelper.index());
        	$find("<%=tabTabContainer.ClientID%>").set_activeTabIndex(currentTabIndex);
        	ResizePanel();
        	initTabsClasses();
        }

        function getTabHelper()
        {
            var tabHelper = jQuery('#<%=tpTabHelper.ClientID%>_tab');
            if (!tabHelper)
                c.err("Не определен объект tabHelper!");
            return tabHelper;
        }

        jQuery(function () {
            SetTabHelper();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(handleEndRequest);
        });
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="EditorMain" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" AllowCustomErrorsRedirect="false"
        EnableScriptGlobalization="true" EnableScriptLocalization="true"
        OnAsyncPostBackError="ScriptManager1_AsyncPostBackError" AsyncPostBackTimeout="7200">
    <Scripts>
        <asp:ScriptReference Path="Scripts/ProgressBar.js" />
    </Scripts>
    </asp:ScriptManager>
    <asp:Button CssClass="hiddenButton" ID="btnAjaxError" runat="server" OnClick="btnAjaxError_Click" />
    <asp:HiddenField ID="hfError" runat="server" Value="" />
    <asp:UpdatePanel ID="upUP" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate> 
        <table width="100%" class="FloatButtonPanel">
        <tr>
            <td>
                <asp:Panel ID="pnlActions" runat="server" Width="200px" Style="margin-top: 2px">
                    <div style="float: left">
                        <asp:Label CssClass="lable_action" Height="19px" ID="lblActions" runat="server" Text="Действия"></asp:Label>
                    </div>
                    <div class="pnlActions_arrow"></div>
                </asp:Panel>
                <asp:Panel ID="pnlMenu" Style="display: none" runat="server" Width="125px">
                    <asp:Menu ID="menuActions" runat="server" OnMenuItemClick="MenuActions_MenuItemClick">
                    </asp:Menu>
                </asp:Panel>
                <ajax:PopupControlExtender ID="PopupControlExtender1" runat="server" TargetControlID="pnlActions"
                    PopupControlID="pnlMenu" OffsetY="31" OffsetX="5">
                </ajax:PopupControlExtender>
                <!-- правил OffsetY и OffsetX под KD-735 -->
            </td>
            <td style="width: 100%" align="right">
                <table cellpadding="0" cellspacing="0" align="right">
                    <tr>
	                    <td>
		                    <asp:Label ID="lblCopyCount" runat="server" Text="Количество создаваемых копий по шаблону: " Visible="False"></asp:Label>
						</td>
						<td>
							<asp:TextBox ID="txtCopyCount" runat="server" Text="1" CssClass="tb-button-panel-count" Visible="False" Columns="2"/>
                        </td>
                        <td>
                            <asp:Button ID="btnSave" runat="server" Text="Сохранить" SkinID="FilterButton" OnClick="SaveClick" OnClientClick="CloseEditorWindow();" />
                        </td>
                        <td>
                            <asp:Button ID="btnSaveAndClose" runat="server" Text="Сохранить и закрыть" SkinID="FilterButton" OnClientClick="CloseEditorWindow();"
                                OnClick="SaveAndCloseClick" />
                        </td>
                        <td>
                            <asp:Button ID="btnCancel" runat="server" Text="Закрыть" SkinID="FilterButton" CausesValidation="false" OnClientClick="CloseEditorWindow();"
                                OnClick="CancelClick" />
                        </td>
                    </tr>
					<tr>
						<td colspan="4">
							<asp:Label runat="server" ID="LockMessage" CssClass="LockMessage" Visible="false"></asp:Label>
						</td>
						<td>									
						</td>
					</tr>
                </table>
            </td>
        </tr>
    </table>
        
        <ajax:TabContainer ID="tabTabContainer" runat="server" EnableTheming="true" OnClientActiveTabChanged="ActiveTabChanged">
            <ajax:TabPanel ID="tpMainData" runat="server" HeaderText="Свойства объекта">
                <ContentTemplate>
                    <table class="propertyObjects" width="100%"> <!--style="background-color: #D8E8F8; height: 100%"-->
                        <tr>
                            <td valign="top">
                                <table width="100%" cellpadding="0" cellspacing="0"><!-- style="background-color: #D8E8F8;" -->
                                    <tr>
                                        <td class="attributeEditor_td1"> <!-- style="width: 30px; background-color: White;"-->
                                        </td>
                                        <td colspan="3" class="attributeEditor_td2"> <!-- style="background-color: White; font-family: Verdana; font-size: 13pt;
                                            color: #525250;"-->
                                            <asp:Label runat="server" ID="lblCaption" Text="Просмотр контрагента"></asp:Label></td>
                                        <td style="width: 30px;" class="attributeEditor_td1">
                                        </td>
                                    </tr>
                                    <tr>
                                    </tr>
                                    <tr class="entity_top" style="vertical-align: bottom; text-align: right">
                                        <td>
                                            &nbsp;</td>
                                        <td colspan="3">
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                        </td>
                                        <td colspan="3" valign="top">
                                            <asp:Label ID="lblIsArchive" runat="server" Text="Архивный" Visible="false"/>
                                            <asp:Label ID="lblIsDeleted" runat="server" Text="Удален" Visible="false"/>

                                            <%--    <asp:UpdatePanel ID="GridPanel" UpdateMode="Conditional" ChildrenAsTriggers="true"
    runat="server">
        <ContentTemplate>
            
--%>
                                            <table width="100%" cellspacing="2" cellpadding="0"><!--style="background-color: #FFFFFF"-->
                                                <tr>
                                                    <td colspan="2">
														<div class="content_row">
															<div class="item_description UO-header-description">Тип контрагента</div>
															<div class="item_value UO-header-value">
																<asp:Label runat="server" ID="lblType"></asp:Label>
															</div>
														</div>
													</td>
                                                </tr>
                                                <tr style="height: 20px" valign="bottom">
                                                    <td colspan="2">
														<div class="content_row">
															<div class="item_description UO-header-description">Наименование</div>
															<div class="item_value UO-header-value">
																<asp:Label runat="server" ID="lblName"></asp:Label>
															</div>
														</div>
													</td>
                                                </tr>
                                                <tr runat="server" id="operationDateRow" visible="false">
                                                    <td colspan="2">
														<div class="content_row">
															<div class="item_description UO-header-description">Дата проведения операции</div>
															<div class="item_value UO-header-value">
																<kadastrctrl:ctlCustomCalendar runat="server" ID="operationDate" />
															</div>
														</div>
													</td>
                                                </tr>
                                                <tr runat="server" id="trLoad" visible="false">
                                                    <td class="text_field_contragent" style="padding-left: 7px">
                                                        Файл
                                                        <div style="padding-top: 25px">Лист файла</div>
                                                    </td>
                                                    <td>
                                                        <table>
                                                            <tr>
                                                                <td style="position: relative;">
                                                                    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                                    <ContentTemplate> 
                                                                    <asp:FileUpload ID="upFileLoad" runat="server" SkinID="BlueStyleUpload" onchange="Upload(this)" />
                                                                    <div style="display: inline-table;">
                                                                        <asp:TextBox runat="server" ID="txtFake" Style="height: 17px" SkinID="BlueStyleUploadTxt"
                                                                            ReadOnly="true" />
                                                                        <br/>
                                                                        <asp:RequiredFieldValidator ID="rfvFile" runat="server" ValidationGroup="vgLoad" ControlToValidate="txtFake" 
                                                                            ErrorMessage="<%$ FormattedResource:Resources, SelectFile %>" Display="Dynamic"></asp:RequiredFieldValidator>
                                                                    </div>
                                                                    <asp:Button runat="server" ID="btnFake" Text="Обзор..." SkinID="smallButton" />
                                                                        
                                                                        <div>
                                                                            <div style="display: inline-table;">
                                                                                <asp:DropDownList runat="server" ID="ddlSpreadsheets" CssClass="fake_txt" />
                                                                                <br />
                                                                                <asp:RequiredFieldValidator ID="rfvSpreadsheets" runat="server" ControlToValidate="ddlSpreadsheets" ValidationGroup="vgLoad"
                                                                                    ErrorMessage="<%$ FormattedResource:Resources, SelectSpreadsheet %>"></asp:RequiredFieldValidator>
                                                                            </div>
                                                                            <asp:Button runat="server" ID="btnLoad" ValidationGroup="vgLoad" Text="Загрузить" SkinID="smallButton"
                                                                                OnClick="lbtnLoad_Click" />
                                                                            <asp:Button runat="server" ID="btnUnLoad" Text="Выгрузить" Enabled="False" SkinID="smallButton"
                                                                                OnClick="btnUnLoad_Click" />
                                                                        </div>
                                                                    </ContentTemplate>
                                                                        <Triggers>
                                                                        <asp:PostBackTrigger ControlID="btnLoad" />
                                                                        <asp:PostBackTrigger ControlID="btnUnLoad" />
                                                                        <asp:PostBackTrigger ControlID="gvEntity"/>
                                                                    </Triggers>
                                                                    </asp:UpdatePanel>  
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:Label runat="server" ID="lReqAttrEmpty" Visible="false" CssClass="required_Fields" Text="*не заполнены обязательные атрибуты."></asp:Label><!-- ForeColor="Red" -->
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:Label runat="server" ID="lblErrMess" CssClass="required_Fields"></asp:Label><!-- Style="color: Red"-->
                                                    </td>
                                                </tr>
                                                <tr style="height: 18px" runat="server" id="trObjects" visible="false">
                                                    <td colspan="2" class="propertyObjects_trObjects" height="10px" valign="middle"
                                                        nowrap="nowrap">
                                                        <img src="Images/collapse.png" alt="Свернуть" id="imgCollapse1" style="cursor: pointer;"
                                                            onclick="javascript:tl_ShowHideRow('imgCollapse1', 'objectsList')" />&nbsp;&nbsp;
                                                        Объекты операции</td>
                                                </tr>
                                                <tr id="objectsList">
                                                    <td colspan="2">
                                                        <div id="dv_container">
                                                            <asp:UpdatePanel ID="pnlEntityes" runat="server" UpdateMode="Conditional">
                                                                <ContentTemplate>
                                                                    <asp:GridView EnableTheming="true" OnRowDeleting="gvEntity_RowDeleting" ID="gvEntity"
                                                                        runat="server" AutoGenerateColumns="False" DataKeyNames="Id" HeaderStyle-HorizontalAlign="Left"
                                                                        IsMakeFullPostBackAllways="true">
                                                                        <Columns>
                                                                            <asp:ButtonField ButtonType="Button" ItemStyle-Width="22px" ControlStyle-CssClass="DeleteButton"
                                                                                CommandName="Delete" />
                                                                            <asp:BoundField DataField="sName" HeaderText="Название" HtmlEncode="false" />
                                                                        </Columns>
                                                                    </asp:GridView>
                                                                </ContentTemplate>
                                                            </asp:UpdatePanel>
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        &nbsp;<asp:PlaceHolder ID="phAttrValues" runat="server"></asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                        </td>
                                        <td colspan="3">
                                            <asp:PlaceHolder ID="phOperations" runat="server"></asp:PlaceHolder>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            &nbsp;</td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                        <td>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <%--            </ContentTemplate>
    </asp:UpdatePanel>
--%>
                </ContentTemplate>
            </ajax:TabPanel>
            <ajax:TabPanel ID="TabPanelAddit" runat="server" HeaderText="Дополнительно">
                    <ContentTemplate>
                        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                 <table cellpadding="0" cellspacing="0" class="PropertyEditor_MainTable">
                                    <tr>
                                        <td class="attributeEditor_td1"> </td>
                                        <td colspan="3" class="attributeEditor_td2"> 
                                            <asp:Label runat="server" ID="Label3" Text="Дополнительные поля"></asp:Label></td>
                                        <td style="width: 30px;" class="attributeEditor_td1"> </td>
                                    </tr>
                                    <tr style="vertical-align: bottom;">
                                        <td class="entity_top" colspan="4"></td>
                                        <td class="entity_top"></td>
                                        <td class="entity_top"></td>
                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td colspan="4">
                                            <table cellpadding="2" cellspacing="0" border="0" class="PropertyEditor_TableData">
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="Label2" runat="server" CssClass="text_field_table main_div" Text="Атрибут"></asp:Label>
                                                        <asp:DropDownList ID="ddlAddsAttribs" runat="server" 
                                                            DataTextField="sName" DataValueField="Id" ></asp:DropDownList>
                                                        <br />
                                                        <asp:Button ID="Button1" runat="server" Text="Добавить" SkinID="FilterButton" 
                                                            OnClick="AddAdditionalAttribute_Click" />
                                                        <br />
                                                        <br />
                                                        <asp:PlaceHolder ID="phAddsAttribs" runat="server"></asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td></td>
                                    </tr>
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td colspan="4"></td>
                                        <td></td>
                                    </tr>
                                 </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajax:TabPanel>
            <ajax:TabPanel ID="tpOperationHistory" runat="server" HeaderText="История">
                <ContentTemplate>
                    <kadastrctrl:ctrlIndividualHistory ID="gvHistory" runat="server" />
                </ContentTemplate>
            </ajax:TabPanel>
            <ajax:TabPanel ID="TabPanel2" runat="server" HeaderText="Связи">
                    <ContentTemplate>
                        <asp:UpdatePanel ID="UpdatePanel3" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                 <table cellpadding="0" cellspacing="0" class="PropertyEditor_MainTable">
                                    <tr>
                                        <td class="attributeEditor_td1"> </td>
                                        <td colspan="3" class="attributeEditor_td2"> 
                                            <asp:Label runat="server" ID="Label1" Text="Связи"></asp:Label></td>
                                        <td style="width: 30px;" class="attributeEditor_td1"> </td>
                                    </tr>
                                    <tr style="vertical-align: bottom;">
                                        <td class="entity_top" colspan="4"></td>
                                        <td class="entity_top"></td>
                                        <td class="entity_top"></td>
                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td colspan="4">
                                            <table cellpadding="2" cellspacing="0" border="0" class="PropertyEditor_TableData">
                                                <tr>
                                                    <td>
                                                        <asp:PlaceHolder ID="phLinks" runat="server"></asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td></td>
                                    </tr>
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td colspan="4"></td>
                                        <td></td>
                                    </tr>
                                 </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajax:TabPanel>
            <ajax:TabPanel ID="TabPanel3" runat="server" HeaderText="Документы">
                    <ContentTemplate>
                        <asp:UpdatePanel ID="UpdatePanel4" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                 <table cellpadding="0" cellspacing="0" class="PropertyEditor_MainTable">
                                    <tr>
                                        <td class="attributeEditor_td1"> </td>
                                        <td colspan="3" class="attributeEditor_td2"> 
                                            <asp:Label runat="server" ID="Label4" Text="Документы"></asp:Label></td>
                                        <td style="width: 30px;" class="attributeEditor_td1"> </td>
                                    </tr>
                                    <tr style="vertical-align: bottom;">
                                        <td class="entity_top" colspan="4"></td>
                                        <td class="entity_top"></td>
                                        <td class="entity_top"></td>
                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td colspan="4">
                                            <table cellpadding="2" cellspacing="0" border="0" class="PropertyEditor_TableData">
                                                <tr>
                                                    <td>
                                                        <asp:PlaceHolder ID="phDocs1" runat="server"></asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="Label5" runat="server" CssClass="text_field_table main_div" Text="Вложение"></asp:Label>
                                                        <asp:DropDownList ID="ddlAttachments" runat="server" 
                                                            DataTextField="sFileName" DataValueField="Id" ></asp:DropDownList>
                                                        <br />
                                                        <asp:Button ID="Button2" runat="server" Text="Добавить" SkinID="FilterButton" 
                                                            OnClick="AddAttachment_Click" />
                                                        <br />
                                                        <br />
                                                        <asp:PlaceHolder ID="phDocs2" runat="server"></asp:PlaceHolder>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td></td>
                                    </tr>
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td colspan="4"></td>
                                        <td></td>
                                    </tr>
                                 </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajax:TabPanel>
			<ajax:TabPanel runat="server" HeaderText="Вкладка-помощник" ID="tpTabHelper" CssClass="nonDisplayedElement">
                <ContentTemplate></ContentTemplate>
            </ajax:TabPanel>
        </ajax:TabContainer>
        <asp:HiddenField ID="hfTabActiveIndex" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
        
    <div id="pnlPopup" class="PrProgress" style="display: none;">
        <div id="innerPopup" class="PrContainer">
            <div class="PrHeader">Пожалуйста, ждите...</div>
            <div class="PrBody">
                <img width="220px" height="19px"  src="Images/activity.gif" alt="loading..." />
            </div>
		</div>
	</div>
        
    <div id="tabs_blocker">
	    <img src="../Images/loader.gif" />
    </div>

    <div id="ProgressBarResolver" runat="server">
            <script type="text/javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ResizePanel);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(RefreshDeleteFlags);
        </script>
    </div>
    <script language="javascript" type="text/javascript">
        function ShowErrorPage(message) {
            if (message) {
                var hfError = $get("<%= hfError.ClientID %>");
                hfError.value = message;
            }

            var button = $get("<%= btnAjaxError.ClientID %>");
            button.click();
        }
        </script>
</asp:Content>
