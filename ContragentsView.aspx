<%@ Page Language="C#" MasterPageFile="~/UMS.master" AutoEventWireup="true"
	Inherits="ContragentsView" Title="Контрагенты" EnableTheming="true"
	ValidateRequest="false" EnableEventValidation="false" CodeBehind="ContragentsView.aspx.cs" %>

<%@ Register Src="Controls/ctlDictionaryPanel.ascx" TagName="ctlDictionaryPanel"
	TagPrefix="ctrl" %>
<%@ Register assembly="Kadastr.WebApp" namespace="Kadastr.WebApp.Code.GUI" tagprefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="phHead" runat="Server">
	<script type="text/javascript" src="<%: System.Web.Optimization.BundleTable.Bundles.ResolveBundleUrl("~/bundles/ContragentsViewJs") %>"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phMain" runat="Server">
	<asp:HiddenField ID="idEntityView" Value="" runat="server"/>
	<%--<asp:Button ID="btnRefresh" Style="display: none" runat="server" OnClick="RefreshClick" />--%>
	<asp:MultiView ID="mvMultiView" runat="server" EnableTheming="true">
		<asp:View ID="vGrouping" runat="server" EnableTheming="true">
			<div class="dvFilter">
				<asp:UpdatePanel ID="upGrouping" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
					<ContentTemplate>
						<kadastrctrl:ctrlFilterAndSortingControl ID="ctrlFilterAndSortingControl1" runat="server"
							Visible="false" OnOnFilteringAndSorting="SetFilterAndSortingGrouping" />
					</ContentTemplate>
				</asp:UpdatePanel>
			</div>
			<asp:UpdatePanel ID="upGroupingEntities" runat="server" UpdateMode="Conditional"
				ChildrenAsTriggers="true">
				<ContentTemplate>
					<table width="100%" cellpadding="0" cellspacing="0" style="margin-top: 5px">
						<tr>
							<td>
								<kadastrctrl:clsEntityGridView EnableTheming="true" Width="100%" ID="gvGrouping"
									EnableViewState="true" OnPageIndexChange="gvGrouping_PageIndexChanging" OnPageIndexChanging="gvGrouping_PageIndexChanging"
									runat="server" MouseHoverRowHighlightEnabled="true" DataKeyNames="Id" OnRowDoubleClick="GroupingGridDblClick"
									IsMakeFullPostBackAllways="true">
								</kadastrctrl:clsEntityGridView>
							</td>
						</tr>
					</table>
				</ContentTemplate>
				<Triggers>
					<asp:AsyncPostBackTrigger ControlID="ctrlFilterAndSortingControl1" EventName="OnFilteringAndSorting" />
				</Triggers>
			</asp:UpdatePanel>
		</asp:View>
		<asp:View ID="vEntityVew" runat="server">
			<div class="dvFilter">
				<asp:UpdatePanel ID="upEntityPanel" UpdateMode="Conditional" runat="server" ChildrenAsTriggers="true">
					<ContentTemplate>
						<kadastrctrl:ctrlFilterAndSortingControl ID="ctrlFilterAndSortingControl2" runat="server" OnOnFilteringAndSorting="SetFilterAndSortingEntity" />
					</ContentTemplate>
				</asp:UpdatePanel>
			</div>
			<asp:UpdatePanel ID="upEntities" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
				<ContentTemplate>
					<div class="Grid_wrapper">
                        <asp:GridView runat="server" ID="tableFilter">
                            
                        </asp:GridView>
						<kadastrctrl:clsEntityGridView OnExportClick="ExportClick" ID="gvEntity" runat="server"
							AdditionalFields="CheckBox" MouseHoverRowHighlightEnabled="true" Width="100%"
							OnRowDoubleClick="EditClick" DataKeyNames="Id" OnPageIndexChange="gvEntity_PageIndexChanging"
							OnPageIndexChanging="gvEntity_PageIndexChanging" IsFixedHeader="true" FilterId="ctrlFilterAndSortingControl2" DoubleClickScriptHandlerMaker="true">
						</kadastrctrl:clsEntityGridView>
					</div>
					<div style="float:left;display:inline;margin-right: 10px;">
						<asp:LinkButton ID="btnShowSummaryRow" runat="server" Text="Показать строку с итогами"
							Visible="false" OnClick="btnShowSummaryRow_Click" CssClass="ShowSummaryInfoButton" />
                    </div>
                    <div style="float:left;display:inline;">
                        <asp:LinkButton ID="lbConfigureUserView" runat="server" Text="Настроить вид" 
                            class="ShowSummaryInfoButton"  OnClick="lbConfigureUserView_Click"/>
					</div>
				</ContentTemplate>
				<Triggers>
					<asp:AsyncPostBackTrigger ControlID="ctrlFilterAndSortingControl2" EventName="OnFilteringAndSorting" />
				</Triggers>
			</asp:UpdatePanel>
		</asp:View>
	</asp:MultiView>

	<script type="text/javascript">
		if (typeof Sys != "undefined") {
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(RemakeFilterSelects);
		}
		else if (c)
			c.err("Необпределен объект Sys");
	</script> 
</asp:Content>
