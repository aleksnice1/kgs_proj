<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddresEditor.aspx.cs" Inherits="Kadastr.WebApp.AddresEditor"  %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Редактирование адреса</title>
    <link rel="stylesheet" type="text/css" href="/css/Settings.css"/>
    <link rel="stylesheet" type="text/css" href="/css/kadastr.css"/>
    <link rel="stylesheet" type="text/css" href="/css/markupHelper.css"/>
    <style type="text/css">
        .auto-style1 {
            height: 22px;
        }
    </style>
</head>

<script type="text/javascript" language="javascript"> 

    function SearchList() {
        var l = document.getElementById('<%= lst_street.ClientID %>');
        var tb = document.getElementById('<%= txtSearchVilStr.ClientID %>');
        if (tb.value == "") {
            ClearSelection(l);
        }
        else {
            for (var i = 0; i < l.options.length; i++) {
                if (l.options[i].value.toLowerCase().match(tb.value.toLowerCase())) {
                    l.options[i].selected = true;
                    return false;
                }
                else {
                    ClearSelection(l);
                }
            }
        }
    }

    function SearchListStreet() {
        var l = document.getElementById('<%= lst_street_mini.ClientID %>');
            var tb = document.getElementById('<%= txtSearchStrHouses.ClientID %>');
        if (tb.value == "") {
            ClearSelection(l);
        }
        else {
            for (var i = 0; i < l.options.length; i++) {
                if (l.options[i].value.toLowerCase().match(tb.value.toLowerCase())) {
                    l.options[i].selected = true;
                    return false;
                }
                else {
                    ClearSelection(l);
                }
            }
        }
    }

    function SearchListHouse() {
        var l = document.getElementById('<%= lst_houses.ClientID %>');
            var tb = document.getElementById('<%= txtSearchHouse.ClientID %>');
        if (tb.value == "") {
            ClearSelection(l);
        }
        else {
            for (var i = 0; i < l.options.length; i++) {
                if (l.options[i].value.toLowerCase().match(tb.value.toLowerCase())) {
                    l.options[i].selected = true;
                    return false;
                }
                else {
                    ClearSelection(l);
                }
            }
        }
        }

    function ClearSelection(lb) {
        lb.selectedIndex = -1;
    }


    function openBackWindowOk() {
        alert(document.history);
    }

    function transfer() {
        var a = document.getElementById('txtFullAdr').value;
        location.href = document.getElementById('txtUnVisible').value + "?" + a;
    }

    function visibleDiv() {
        document.getElementById('divName').style.display = "block";
        document.getElementById('divName2').style.display = "block";
        document.getElementById('divName3').style.display = "block";
        document.getElementById('tableAddr').style.display = "table";
        document.getElementById('tableAddr').style.width = "100%";
    }

</script>

    <script  type="text/javascript">

</script>



<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnableScriptGlobalization="true"
		EnableScriptLocalization="true" />

       <div class="single-page-wrapper"> 

             <div class="content_row">
                <h1>Редактирование адреса</h1>
             </div>

             <div class="content_row">
                <input id="btnOk" type="button" value="Применить" runat="server" 
                  style="border-radius: 5px;font-size:12px; height:30px;width:100px; margin-right:20px" class="menu_actions"/>
                <input id="Button1" type="button" value="Отмена" runat="server" onclick="location.href = document.getElementById('txtUnVisible').value;"
                  style="border-radius: 5px;font-size:12px; height:30px;width:100px" class="menu_actions"/>
             </div>

             <div class="content_row">
                <div class="item_description">
				    <asp:Label ID="Label7" runat="server" Text="Регион" style="margin-right:10px;font-size:12px"></asp:Label>
                </div>
                <div class="item_value ">
                  <asp:DropDownList ID="cmb_region" runat="server"  AutoPostBack="true" style="Width:500px" CssClass="ddl" 
                        CaseSensitive="False" ItemInsertLocation="OrdinalText" OnSelectedIndexChanged="cmb_region_SelectedIndexChanged"> </asp:DropDownList>        
                </div>
             </div>

             <div class="content_row">
            <div class="item_description">
                <asp:Label ID="Label3" runat="server" Text="Город или район" style="margin-right:10px;font-size:12px"></asp:Label>
            </div>
             <div class="item_value ">          
               <asp:DropDownList ID="cmb_town" runat="server" AutoPostBack="True" style="Width:500px"
                                 CaseSensitive="False"  ItemInsertLocation="OrdinalText"  CssClass="ddl" 
                    OnTextChanged="cmb_town_TextChanged"> </asp:DropDownList>      
            </div>
        </div>

             <div class="content_row">

                 <table>
                   <tr>
                     <th> <asp:Label ID="LabelVill_Street" runat="server" Text="Населенный пункт (улица)" CssClass="item_description" style="font-size:12px;float:none;text-align:center;"></asp:Label> </th>
                     <th>  <asp:Label ID="LabelSreet_House" runat="server" Text="Улица"  Visible="false" CssClass="item_description" style="font-size:12px;float:none;text-align:center;"></asp:Label> </th>
                     <th>  <asp:Label ID="LabelHouse" runat="server" Text="Дом"  Visible="false" CssClass="item_description" style="font-size:12px;float:none;text-align:center;"></asp:Label> </th>
                   </tr>
                    <tr>
                     <td><asp:TextBox  ID="txtSearchVilStr" style="width:225px;margin:2px auto; display:block;float:none" runat="server" AutoPostBack="true" CssClass="tb_text" onkeyup="return SearchList();"></asp:TextBox></td>
                     <td><asp:TextBox  ID="txtSearchStrHouses" style="width:225px;margin:2px auto; display:block;float:none" runat="server" AutoPostBack="true" Visible ="false" CssClass="tb_text" onkeyup="return SearchListStreet();"></asp:TextBox>  </td>
                     <td><asp:TextBox  ID="txtSearchHouse" style="width:175px;margin:2px auto; display:block;float:none" runat="server" AutoPostBack="true" Visible ="false" CssClass="tb_text" onkeyup="return SearchListHouse();"></asp:TextBox>  </td>
                   </tr>
                   <tr>
                     <td><asp:ListBox ID="lst_street" runat="server" AutoPostBack="true" Width="250px" Height="300px" OnTextChanged="lst_street_TextChanged" CssClass="item_description"></asp:ListBox></td>
                     <td><asp:ListBox ID="lst_street_mini" runat="server" Visible ="false" Width="250px" Height="300px" AutoPostBack="true" OnTextChanged="lst_street_mini_TextChanged" CssClass="item_description"></asp:ListBox></td>
                     <td><asp:ListBox ID="lst_houses" runat="server" Width="200px" Height="300px" Visible ="false" AutoPostBack="true" CssClass="item_description" OnTextChanged="lst_houses_TextChanged"></asp:ListBox></td>
                   </tr>
                 </table>
           
             </div>

             <div class="content_row">
                <div class="item_description">
                  <asp:Label ID="Label1" runat="server" Text="Номер квартиры (офиса)" style="margin-right:10px;font-size:12px"></asp:Label>
                </div>
                <div class="item_value">          
                  <asp:TextBox ID="txtNumOffice" runat="server" OnTextChanged="txtNumOffice_TextChanged" AutoPostBack="true"></asp:TextBox>  
                </div>
             </div>

             <div class="content_row">
                <div class="item_description">
                   <asp:Label ID="Label5" runat="server" Text="Индекс" style="margin-right:10px;font-size:12px"></asp:Label>
                </div>
                <div class="item_value">          
                  <asp:TextBox ID="txtPostCode" runat="server"></asp:TextBox>  
                </div>
             </div>

             <div class="content_row">
                <div class="item_description">
                   <asp:Label ID="Label6" runat="server" Text="Полный адрес" style="margin-right:10px; font-size:12px"></asp:Label>
                </div>
                <div class="item_value">          
                  <asp:TextBox ID="txtFullAdr" runat="server"></asp:TextBox>  
                </div>
             </div>

             <div class="content_row">
                <input type="button" id="btnTransfer" value="Применить" runat="server" 
                  style="border-radius: 5px;font-size:12px; height:30px;width:100px;" class="menu_actions"/>
                <input type="button" value="Отмена" runat="server" onclick="location.href = document.getElementById('txtUnVisible').value;"
                  style="border-radius: 5px;font-size:12px; height:30px;width:100px; margin-right:20px" class="menu_actions"/>
                  <input type="button" value="Добавить новый адрес" runat="server" onclick="visibleDiv();" 
                  style="border-radius: 5px;font-size:12px; height:30px;width:250px" class="menu_actions"/>
             </div>
             <asp:HiddenField id="txtUnVisible" runat="server" />
      <%--        <asp:TextBox ID="txtUnVisible"  runat="server" AutoPostBack="true"></asp:TextBox>  --%>

             <div class="content_row" id="divName" style="display:none;">
                <h1>Добавление неового адреса в базу</h1>
             </div>
              
            <div class="content_row" id="divName3" style="display:none;">
				  <asp:Label ID="Label16" runat="server"
                      Text="Перед добавлением нового адреса в базу данных, пожалуйста, ознакомьтесь с инструкцией!!!"
                      Font-Names="Trebuchet MS" style="margin-right:10px;font-size:14px;text-align:justify;color:red"></asp:Label>
             </div>
           <div class="content_row" id="divName2" style="display:none;">
                <input type="submit" SkinId="InfOpen" runat="server" onclick="window.open('InformationAboutAddAddr.aspx', '_blank', 'location=yes,height=500,width=450,scrollbars=yes,status=yes');" 
                style="border-radius: 5px;font-size:12px;height:30px;" class="menu_actions" value="Вызов инструкции"/>
            </div>

            <table style="width:100%; display:none;" id="tableAddr">
            <tr style="vertical-align:top;">
                <td>
                    <table style="width:100%;">
                        <tr>
                            <td style="width:150px"> 
                                  <asp:Label ID="Label2" runat="server" Text="Регион" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;" class="item_value">
                              <asp:TextBox ID="txtAddRegion" runat="server" ReadOnly="true"/>
                            </td>
                        </tr>
                        <tr>
                            <td style="width:150px"> 
                                  <asp:Label ID="Label14" runat="server" Text="Тип" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;" class="item_value"  >
                                <asp:DropDownList ID="DropDownListTypeTown" runat="server" class="EntityEditor_DropDownList" CssClass="ddl">
                                      <asp:ListItem Selected="True" Value="city"> город </asp:ListItem>
                                            <asp:ListItem Value="rayon"> район </asp:ListItem>
                                            <asp:ListItem Value="sp"> с/п </asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                         <tr>
                            <td class="item_description">
                                 <asp:Label ID="Label4" runat="server" Text="Город (район)" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;" class="item_value">
                                <asp:TextBox ID="txtAddTown" runat="server" />
                            </td>
                             <td style="padding-top:6px;">
                                 <asp:ImageButton ID="ImageButtonAddTownOrVil" runat="server" AlternateText="Добавить"  AutoPostBack="true" OnClick="ImageButtonAddTownOrVil_Click"  ImageUrl="~/Images/add.jpg" />
                            </td>
                        </tr>
                          <tr>
                            <td style="width:150px"> 
                                  <asp:Label ID="Label15" runat="server" Text="Тип населенного пункта "  style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;" class="item_value"  >
                             <asp:TextBox ID="TextBoxAddTypeVillage" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td  class="item_description" style="height: 22px">
                                 <asp:Label ID="Label13" runat="server" Text="Населенный пункт" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;" class="auto-style1">
                                <asp:TextBox ID="txtAddVillage" runat="server" />
                            </td>
                            <td style="padding-top:6px;" class="auto-style1">
                                 <asp:ImageButton ID="ImageButtonAddVil" runat="server" AlternateText="Добавить" AutoPostBack="true" OnClick="ImageButtonAddVil_Click" ImageUrl="~/Images/add.jpg" />
                            </td>
                        </tr>
                         <tr>
                            <td  class="item_description">
                                 <asp:Label ID="Label8" runat="server" Text="Улица" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;">
                                <asp:TextBox ID="txtAddStreet" runat="server" />
                            </td>
                            <td style="padding-top:6px;">
                                 <asp:ImageButton ID="ImageButtonAddStreet" AutoPostBack="true" OnClick="ImageButtonAddStreet_Click" runat="server" AlternateText="Добавить" ImageUrl="~/Images/add.jpg" />
                            </td>
                        </tr>
                        <tr>
                            <td style="width:150px"> 
                                  <asp:Label ID="Label9" runat="server" Text="Тип здания" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;" class="item_value"  >
                                <asp:DropDownList ID="ddlTypeBuild" runat="server" class="EntityEditor_DropDownList" CssClass="ddl">
                                      <asp:ListItem Selected="True" Value="house"> дом </asp:ListItem>
                                            <asp:ListItem Value="str"> строение </asp:ListItem>
                                            <asp:ListItem Value="st1r"> сооружение </asp:ListItem>
                                            <asp:ListItem Value="litera"> литера </asp:ListItem>
                                            <asp:ListItem Value="uch"> участок </asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                         <tr>
                            <td  class="item_description">
                                 <asp:Label ID="Label12" runat="server" Text="Индекс" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;">
                                <asp:TextBox ID="txtAddPostalCode" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td  class="item_description">
                                 <asp:Label ID="Label10" runat="server" Text="Номер" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;">
                                <asp:TextBox ID="txtAddBuildNum" runat="server" />
                            </td>
                            <td style="padding-top:6px;">
                                 <asp:ImageButton ID="ImageButtonAddNumBuild" runat="server" AlternateText="Добавить" OnClick="ImageButtonAddNumBuild_Click" AutoPostBack="true" ImageUrl="~/Images/add.jpg" />
                            </td>
                        </tr>
                        <tr>
                            <td  class="item_description">
                                 <asp:Label ID="Label11" runat="server" Text="Корпус" style="margin-right:10px; font-size:14px"></asp:Label>
                            </td>
                            <td style="text-align:left;">
                                <asp:TextBox ID="txtAddKorpusNum" runat="server" />
                            </td>
                        </tr>
                        </table>
                    </td>
                </tr>
                </table>
       </div>

    </form>

        <script type="text/javascript">
            //if (document.getElementById('txtUnVisible').value == "") {
            //    let a = location.search.substring(1);
            //    document.getElementById('txtUnVisible').value = a;
            //}
</script>
</body>
</html>
