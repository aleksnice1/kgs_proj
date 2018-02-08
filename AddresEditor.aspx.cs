using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kadastr.CommonUtils;
using Kadastr.DataAccessLayer;
using Kadastr.DataAccessLayer.Helpers;
using Kadastr.DataAccessLayer.Properties;
using Kadastr.Domain;
using Kadastr.DomainModel;
using Kadastr.DomainModel.Infrastructure;
using System.Data;
using Kadastr.WebApp.Models;
using Kadastr.WebApp.Statics;

namespace Kadastr.WebApp
{
    public partial class AddresEditor : System.Web.UI.Page
    {
        #region  Команды sql
        string sCommandTextSelectTowns72 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME FROM TownsAndStreets72 WHERE (AOLEVEL = 4 OR AOLEVEL = 3) AND ACTSTATUS = 1";
        string sCommandTextSelectVillagesAndStreets72 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME FROM TownsAndStreets72 WHERE (AOLEVEL = 65 OR AOLEVEL = 6 OR AOLEVEL = 65 OR AOLEVEL = 35) AND ACTSTATUS = 1";
        string sCommandTextSelectStreets72 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME, POSTALCODE FROM TownsAndStreets72 WHERE (AOLEVEL = 7 OR AOLEVEL = 90 OR AOLEVEL = 91 OR AOLEVEL = 5 OR AOLEVEL = 75) AND ACTSTATUS = 1";
        string sCommandTextSelectHouses72 = @"SELECT HOUSENUM, BUILDNUM, STRSTATUS, POSTALCODE FROM HOUSE72 WHERE ";
        string sCommandTextSelectSteads72 = @"SELECT NUMBER, POSTALCODE FROM STEAD72 WHERE LIVESTATUS = 1 AND PARENTGUID ='";

        string sCommandTextSelectTowns89 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME FROM TownsAndStreets89 WHERE (AOLEVEL = 4 OR AOLEVEL = 3) AND ACTSTATUS = 1";
        string sCommandTextSelectVillagesAndStreets89 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME FROM TownsAndStreets89 WHERE (AOLEVEL = 65 OR AOLEVEL = 6 OR AOLEVEL = 65 OR AOLEVEL = 35) AND ACTSTATUS = 1";
        string sCommandTextSelectStreets89 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME, POSTALCODE FROM TownsAndStreets89 WHERE  (AOLEVEL = 7 OR AOLEVEL = 90 OR AOLEVEL = 91 OR AOLEVEL = 5 OR AOLEVEL = 75) AND ACTSTATUS = 1";
        string sCommandTextSelectHouses89 = @"SELECT HOUSENUM, BUILDNUM, STRSTATUS, POSTALCODE FROM HOUSE89 WHERE ";
        string sCommandTextSelectSteads89 = @"SELECT NUMBER, POSTALCODE FROM STEAD89 WHERE LIVESTATUS = 1 AND PARENTGUID ='";

        string sCommandTextSelectTowns86 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME FROM TownsAndStreets86  WHERE (AOLEVEL = 4 OR AOLEVEL = 3) AND ACTSTATUS = 1";
        string sCommandTextSelectVillagesAndStreets86 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME FROM TownsAndStreets86  WHERE (AOLEVEL = 65 OR AOLEVEL = 6 OR AOLEVEL = 65 OR AOLEVEL = 35) AND ACTSTATUS = 1";
        string sCommandTextSelectStreets86 = @"SELECT AOLEVEL, AOGUID, SHORTNAME, PARENTGUID, OFFNAME, POSTALCODE FROM TownsAndStreets86  WHERE (AOLEVEL = 7 OR AOLEVEL = 90 OR AOLEVEL = 91 OR AOLEVEL = 5 OR AOLEVEL = 75) AND ACTSTATUS = 1";
        string sCommandTextSelectHouses86 = @"SELECT HOUSENUM, BUILDNUM, STRSTATUS, POSTALCODE FROM HOUSE86  WHERE ";
        string sCommandTextSelectSteads86 = @"SELECT NUMBER, POSTALCODE FROM STEAD86  WHERE LIVESTATUS = 1 AND PARENTGUID ='";
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                cmb_region.Items.Add("Тюменская область");
                cmb_region.Items.Add("ХМАО");
                cmb_region.Items.Add("ЯНАО");
                cmb_region.SelectedIndex = 0;

                UpdateListTown(sCommandTextSelectTowns72);
                txtFullAdr.Text = "Российская Федерация, " + cmb_region.SelectedValue;
            }
            if(Session["PageIndex"] != null) txtUnVisible.Value = Session["PageIndex"].ToString();         
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            // 
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            // 
            InitializeComponent();

        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnTransfer.ServerClick += new System.EventHandler(this.btnTransfer_ServerClick);
            this.btnOk.ServerClick += new System.EventHandler(this.btnTransfer_ServerClick);
            this.Load += new System.EventHandler(this.Page_Load);

        }
        #endregion

        private void btnTransfer_ServerClick(object sender, System.EventArgs e)
        {
            Session["PageIndex"] = null;
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "transfer()", true);
        }


        protected void cmb_town_TextChanged(object sender, EventArgs e)
        {
            // скрытие объектов
            if(sclsTypeDocumentsOfExport.List_houses != null) sclsTypeDocumentsOfExport.List_houses.Clear();
            if (sclsTypeDocumentsOfExport.List_streets != null) sclsTypeDocumentsOfExport.List_streets.Clear();
            if (sclsTypeDocumentsOfExport.List_streets_or_village != null) sclsTypeDocumentsOfExport.List_streets_or_village.Clear();
            txtAddRegion.Text = cmb_region.SelectedValue;
            lst_street_mini.Items.Clear();
            lst_houses.Items.Clear();
            lst_street.Items.Clear();
            lst_houses.Visible = false;
            // добавляем в текстбокс значение если индекс больше 0
            if (cmb_town.SelectedIndex != 0) txtAddTown.Text = cmb_town.SelectedValue.Remove(0, cmb_town.SelectedValue.IndexOf(' ') + 1);

            string guid = "";
            // поиск в списке городов выбранный город и получаем его уникальный номер
            foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_towns)
            {
                if ((adr.SHORTNAME + " " + adr.OFFNAME) == cmb_town.SelectedValue)
                {
                    if(adr.SHORTNAME == "г")
                    {
                        LabelVill_Street.Text = "Улица";
                    }
                    else
                    {
                        LabelVill_Street.Text = "Населенный пункт";
                    }
                    guid = adr.AOGUID;
                    break;
                }
            }
            // выполнение запросов в зависимости от выбора региона
            switch (cmb_region.SelectedIndex)
            {
                case 0:
                    // заполение списка населенных пунктов (улицы)
                    if (LabelVill_Street.Text != "Улица")
                        sclsTypeDocumentsOfExport.List_streets_or_village = CoolectObjects(sCommandTextSelectVillagesAndStreets72 + @" AND PARENTGUID = '" + guid + "'");
                    else
                        sclsTypeDocumentsOfExport.List_streets_or_village = CoolectObjects(sCommandTextSelectStreets72 + @" AND PARENTGUID = '" + guid + "'");
                    break;
                case 1:
                    if (LabelVill_Street.Text != "Улица")
                        sclsTypeDocumentsOfExport.List_streets_or_village = CoolectObjects(sCommandTextSelectVillagesAndStreets86 + @" AND PARENTGUID = '" + guid + "'");
                    else
                        sclsTypeDocumentsOfExport.List_streets_or_village = CoolectObjects(sCommandTextSelectStreets86 + @" AND PARENTGUID = '" + guid + "'");
                    break;
                case 2:
                    if (LabelVill_Street.Text != "Улица")
                        sclsTypeDocumentsOfExport.List_streets_or_village = CoolectObjects(sCommandTextSelectVillagesAndStreets89 + @" AND PARENTGUID = '" + guid + "'");
                    else
                        sclsTypeDocumentsOfExport.List_streets_or_village = CoolectObjects(sCommandTextSelectStreets89 + @" AND PARENTGUID = '" + guid + "'");
                    break;
            }

            // заполняем листбокс
            foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_streets_or_village)
            {
                lst_street.Items.Add(adr.SHORTNAME + " " + adr.OFFNAME);
            }

            txtFullAdr.Text = txtFullAdr.Text + ", " + cmb_town.SelectedValue;
        }

        protected void lst_street_TextChanged(object sender, EventArgs e)
        {
            // получем подстроку для определения, то выбранный населеный пункт ГОРОД
            string testSubLine = cmb_town.SelectedValue.Substring(0, 1);
            if (sclsTypeDocumentsOfExport.List_streets != null) sclsTypeDocumentsOfExport.List_streets.Clear();
            if (sclsTypeDocumentsOfExport.List_houses != null) sclsTypeDocumentsOfExport.List_houses.Clear();
            lst_street_mini.Items.Clear();
            lst_houses.Items.Clear();
            lst_houses.Visible = false;

            string guid = "";
            foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_streets_or_village)
            {
                if ((adr.SHORTNAME + " " + adr.OFFNAME) == lst_street.SelectedValue)
                {
                    guid = adr.AOGUID;
                    break;
                }
            }
            // есои выбранный объект не город
            if (testSubLine != "г")
            {
                txtAddVillage.Text = lst_street.SelectedValue.Remove(0, lst_street.SelectedValue.IndexOf(' ') + 1);

                switch (cmb_region.SelectedIndex)
                {
                    case 0:
                        sCommandTextSelectStreets72 = sCommandTextSelectStreets72 + @" AND PARENTGUID = '" + guid + "'";
                        // заполняем список List_streets улицами
                        sclsTypeDocumentsOfExport.List_streets = CoolectObjects(sCommandTextSelectStreets72);
                        break;
                    case 1:
                        sCommandTextSelectStreets86 = sCommandTextSelectStreets86 + @" AND PARENTGUID = '" + guid + "'";
                        sclsTypeDocumentsOfExport.List_streets = CoolectObjects(sCommandTextSelectStreets86);
                        break;
                    case 2:
                        sCommandTextSelectStreets89 = sCommandTextSelectStreets89 + @" AND PARENTGUID = '" + guid + "'";
                        sclsTypeDocumentsOfExport.List_streets = CoolectObjects(sCommandTextSelectStreets89);
                        break;
                }

                if (sclsTypeDocumentsOfExport.List_streets.Count > 0)
                {
                    // показываем скрытые элементы
                    LabelSreet_House.Visible = true;
                    txtSearchStrHouses.Visible = true;
                    lst_street_mini.Visible = true;
                    // заполнение листбокса улицами
                    foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_streets)
                    {
                        lst_street_mini.Items.Add(adr.SHORTNAME + " " + adr.OFFNAME);
                    }
                }
                // условаие  для првоерки нахождения домов если улицы не были найдены
                if(lst_street_mini.Items.Count < 1)
                {
                    MakeQueryHouse(cmb_region.SelectedIndex, guid, lst_street_mini);

                    LabelSreet_House.Visible = true;
                    txtSearchStrHouses.Visible = true;
                    lst_street_mini.Visible = true;
                }
            }
            else
            {
                MakeQueryHouse(cmb_region.SelectedIndex, guid, lst_street_mini);
                txtAddStreet.Text = lst_street.SelectedValue.Remove(0, lst_street.SelectedValue.IndexOf(' ') + 1);
                LabelSreet_House.Visible = true;
                txtSearchStrHouses.Visible = true;
                lst_street_mini.Visible = true;
            }

            txtFullAdr.Text = txtFullAdr.Text + ", " + lst_street.SelectedValue;
        }

        protected void lst_street_mini_TextChanged(object sender, EventArgs e)
        {
            //if (sclsTypeDocumentsOfExport.List_houses != null) sclsTypeDocumentsOfExport.List_houses.Clear();
            lst_houses.Items.Clear();
            string guid = "";
            // берем подстроку для того чтобы узнать, что это не дом
            string testSubLine = lst_street_mini.SelectedValue.Substring(0, 3);
            // проверяем не выбран ли дом
            if (testSubLine != "дом" && testSubLine != "стр" && testSubLine != "лит")
            {
                foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_streets)
                {
                    if ((adr.SHORTNAME + " " + adr.OFFNAME) == lst_street_mini.SelectedValue)
                    {
                        guid = adr.AOGUID;
                        break;
                    }
                }
                txtAddStreet.Text = lst_street_mini.SelectedValue.Remove(0, lst_street_mini.SelectedValue.IndexOf(' ') + 1);
                // вызов метода для получения домов
                MakeQueryHouse(cmb_region.SelectedIndex, guid, lst_houses, true);

                txtFullAdr.Text = txtFullAdr.Text + ", " + lst_street.SelectedValue + ", " + lst_street_mini.SelectedValue;
            }
            else
            {
                // получаем индекс
                txtPostCode.Text = sclsTypeDocumentsOfExport.List_houses[lst_street_mini.SelectedIndex].POSTALCODE;
                txtFullAdr.Text = txtPostCode.Text + ", " +  txtFullAdr.Text + ", " + lst_street.SelectedValue + ", " + lst_street_mini.SelectedValue;
            }

            
        }

        protected void lst_houses_TextChanged(object sender, EventArgs e)
        {
            // получаем индекс
            txtPostCode.Text = sclsTypeDocumentsOfExport.List_houses[lst_houses.SelectedIndex].POSTALCODE;
            // заполняем полный адрес
            txtFullAdr.Text = txtPostCode.Text + ", " + txtFullAdr.Text + ", " + lst_street.SelectedValue + ", " + lst_street_mini.SelectedValue + ", " + lst_houses.SelectedValue;
        }

        protected void txtNumOffice_TextChanged(object sender, EventArgs e)
        {
            txtFullAdr.Text = txtPostCode.Text + ", " + txtFullAdr.Text + ", " + lst_street.SelectedValue + ", " + lst_street_mini.SelectedValue + ", " + lst_houses.SelectedValue + ", офис " + txtNumOffice.Text;
        }

        protected void cmb_region_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtAddRegion.Text = cmb_region.SelectedValue;
            cmb_town.Items.Add("Выберите из списка");
            switch (cmb_region.SelectedIndex)
            {
                case 0:
                    CollapseElements();
                    // заполняем комбобокс городв, для этого выполняем соответсвующтй запрос
                    sclsTypeDocumentsOfExport.List_towns = CoolectObjects(sCommandTextSelectTowns72);

                    foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_towns)
                    {
                        cmb_town.Items.Add(adr.SHORTNAME + " " + adr.OFFNAME);
                    }
                    txtFullAdr.Text = "Российская Федерация, " + cmb_region.SelectedValue;
                    break;
                case 1:
                    CollapseElements();
                    sclsTypeDocumentsOfExport.List_towns = CoolectObjects(sCommandTextSelectTowns86);

                    foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_towns)
                    {
                        cmb_town.Items.Add(adr.SHORTNAME + " " + adr.OFFNAME);
                    }
                    break;
                case 2:
                    CollapseElements();
                    sclsTypeDocumentsOfExport.List_towns = CoolectObjects(sCommandTextSelectTowns89);

                    foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_towns)
                    {
                        cmb_town.Items.Add(adr.SHORTNAME + " " + adr.OFFNAME);
                    }
                    break;

            }
        }

        #region Методы
        // метод для скрытия и очистки элементов
        private void CollapseElements()
        {
            cmb_town.Items.Clear();
            cmb_town.Items.Add("Выберите из списка");
            lst_street.Items.Clear();
            lst_street_mini.Items.Clear();
            lst_houses.Items.Clear();
            lst_houses.Visible = false;
            lst_street_mini.Visible = false;
            txtSearchStrHouses.Visible = false;
            txtSearchHouse.Visible = false;
            LabelHouse.Visible = false;
            LabelSreet_House.Visible = false;
        }

        /// <summary>
        /// Метод для выполнения запросов получения населенных пунктов и улиц
        /// </summary>
        /// <param name="command">запрос</param>
        /// <returns></returns>
        private List<AddresModel> CoolectObjects(string command)
        {
            List<AddresModel> listObj = new List<AddresModel>();
            // запрос к бд для получения населенных пунктов и улиц
            SqlCommand SelectCommand = new SqlCommand(command, ConnectionFactory.CreateConnection());
            using (SelectCommand.Connection)
            {
                SelectCommand.Connection.Open();
                SqlDataReader reader = SelectCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("AOLEVEL", typeof(string));
                    dt.Columns.Add("AOGUID", typeof(string));
                    dt.Columns.Add("SHORTNAME", typeof(string));
                    dt.Columns.Add("PARENTGUID", typeof(string));
                    dt.Columns.Add("OFFNAME", typeof(string));
                    dt.Load(reader);
                    listObj = dt.DataTableToList<AddresModel>();
                }
            }

            return listObj;
        }

        /// <summary>
        /// Метод для выполнение запросов для получение домов
        /// </summary>
        /// <param name="guid">Родителскьий номер</param>
        /// <param name="commandHouse">запрос для получение домов</param>
        /// <param name="commandStead">запрос для получение участков</param>
        /// <param name="addLst">листбокс где будут отобрааться данные</param>
        /// <param name="visibleLst">отображение или скрытие элементов экрана</param>
        private void CoolectObjectsHouses(string guid, string commandHouse, string commandStead, ListBox addLst, bool visibleLst = false)
        {
            List<AddresModel> listObj = new List<AddresModel>();
            lst_houses.Items.Clear();
            // формирование запроса
            commandHouse = commandHouse + @"AOGUID = '" + guid + "'";
            // выполнение запроса
            SqlCommand SelectCommand = new SqlCommand(commandHouse, ConnectionFactory.CreateConnection());
            using (SelectCommand.Connection)
            {
                SelectCommand.Connection.Open();
                SqlDataReader reader = SelectCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    // заполняем таблицу с данными
                    DataTable dt = new DataTable();
                    dt.Columns.Add("HOUSENUM", typeof(string));
                    dt.Columns.Add("BUILDNUM", typeof(string));
                    dt.Columns.Add("STRSTATUS", typeof(string));
                    dt.Columns.Add("POSTALCODE", typeof(string));
                    dt.Load(reader);
                    // заполняем список домами
                    sclsTypeDocumentsOfExport.List_houses = dt.DataTableToList<HouseModel>();
                }
            }
            // запрос для получение уастков
            commandStead = commandStead + guid + "'";
            // выполенние запроса для получение участков
            SelectCommand = new SqlCommand(commandStead, ConnectionFactory.CreateConnection());
            using (SelectCommand.Connection)
            {
                SelectCommand.Connection.Open();
                SqlDataReader reader = SelectCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("NUMBER", typeof(string));
                    dt.Columns.Add("POSTALCODE", typeof(string));
                    dt.Load(reader);
                    sclsTypeDocumentsOfExport.List_houses.AddRange(dt.DataTableToList<HouseModel>());
                }
            }

            if (sclsTypeDocumentsOfExport.List_houses != null)
            {
                // настройка отображение объектов
                LabelSreet_House.Visible = visibleLst;
                lst_houses.Visible = visibleLst;
                LabelHouse.Visible = visibleLst;
                txtSearchHouse.Visible = visibleLst;
                txtSearchStrHouses.Visible = visibleLst;

                if (visibleLst == false)
                {
                    LabelSreet_House.Text = "Дом";
                }
                // заполнение листбокса домами
                foreach (HouseModel adr in sclsTypeDocumentsOfExport.List_houses)
                {
                    // если нмоер дома пустой, то прерываем иттерацию
                    if ((adr.HOUSENUM == "" || adr.HOUSENUM == null) && (adr.NUMBER == null || adr.NUMBER == ""))
                        continue;

                    string korpus = "";
                    // если есть корпус то записываем его
                    if (adr.BUILDNUM != "" && adr.BUILDNUM != null)
                    {
                        korpus = " корпус " + adr.BUILDNUM;
                    }

                    string num_build = "";
                    if((adr.HOUSENUM == null || adr.HOUSENUM == "") && (adr.NUMBER != null || adr.NUMBER != ""))
                    {
                        num_build = adr.NUMBER;
                    }
                    else
                    {
                        num_build = adr.HOUSENUM;
                    }
                    // взависимости от типа сооруения заносим его в листбокс
                    switch (adr.STRSTATUS)
                    {
                        case 0:
                            addLst.Items.Add("дом " + num_build + korpus);
                            break;
                        case 1:
                            addLst.Items.Add("строение " + num_build + korpus);
                            break;
                        case 2:
                            addLst.Items.Add("сооружение " + num_build + korpus);
                            break;
                        case 3:
                            addLst.Items.Add("литер " + num_build + korpus);
                            break;
                        default:
                            addLst.Items.Add("участок" + num_build + korpus);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Метод для выполнения метода получение домов
        /// </summary>
        /// <param name="index">номер выбранного элемента в комбобоксе региона</param>
        /// <param name="guid">//родительский номер</param>
        /// <param name="addLst">листбок куда заполняем данные</param>
        /// <param name="visibleLst">скрытие или отобраение элементов</param>
        private void MakeQueryHouse(int index, string guid, ListBox addLst, bool visibleLst = false)
        {
            switch (index)
            {
                case 0:
                    // вызов метода для запроса домов
                    CoolectObjectsHouses(guid, sCommandTextSelectHouses72, sCommandTextSelectSteads72, addLst, visibleLst);
                    break;
                case 1:
                    CoolectObjectsHouses(guid, sCommandTextSelectHouses86, sCommandTextSelectSteads86, addLst, visibleLst);
                    break;
                case 2:
                    CoolectObjectsHouses(guid, sCommandTextSelectHouses89, sCommandTextSelectSteads89, addLst, visibleLst);
                    break;
            }
        }

        /// <summary>
        /// Формирование уникального номера
        /// </summary>
        /// <param name="command">запрос в бд</param>
        /// <returns></returns>
        private string GetGuid(string command)
        {
            String guid = Guid.NewGuid().ToString();
            command = command + guid + "\';";
            SqlCommand SelectCommand = new SqlCommand(command, ConnectionFactory.CreateConnection());
            // поиск созданого номера в бд
            // создаем новый объект пока не получим уникальный номер
            using (SelectCommand.Connection)
            {
                SelectCommand.Connection.Open();
                SqlDataReader reader = SelectCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    return GetGuid(command);
                }
            }
            return guid;
        }

        /// <summary>
        /// метод вставки в бд
        /// </summary>
        /// <param name="com">запрос вставки объектов</param>
        /// <param name="table">табилца бд</param>
        private void InsertToBd(string com, string table)
        {
            using (SqlConnection conn = ConnectionFactory.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand("", conn))
                {
                    try
                    {
                        conn.Open();
                        //создаем темповую таблицуку
                        command.CommandText = com;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Подсчет дубликатов
        /// </summary>
        /// <param name="type">тип объекта</param>
        /// <returns></returns>
        private bool CheckingDublicate(string type)
        {
            bool check = false;
            if (type == "town")
            {
                int num = (from l in sclsTypeDocumentsOfExport.List_towns where l.OFFNAME.ToString() == txtAddTown.Text select l).Count();
                check = num > 0 ? true : false;
            }
            else if (type == "selo")
            {
                int num = (from l in sclsTypeDocumentsOfExport.List_streets_or_village where l.OFFNAME.ToString() == txtAddVillage.Text select l).Count();
                check = num > 0 ? true : false;
            }
            else if (type == "street")
            {
                int num = 0;
                if (sclsTypeDocumentsOfExport.List_streets != null && sclsTypeDocumentsOfExport.List_streets.Count > 0)
                    num = (from l in sclsTypeDocumentsOfExport.List_streets where l.OFFNAME.ToString() == txtAddStreet.Text select l).Count();
                else
                    num = (from l in sclsTypeDocumentsOfExport.List_streets_or_village where l.OFFNAME.ToString() == txtAddStreet.Text select l).Count();
                check = num > 0 ? false : true;
            }
            return check;
        }

        /// <summary>
        /// Обнволение списка городов (оайлнов)
        /// </summary>
        /// <param name="command">запрос получения городов (райнов)</param>
        private void UpdateListTown(string command)
        {
            sclsTypeDocumentsOfExport.List_towns = CoolectObjects(command);
            cmb_town.Items.Clear();
            cmb_town.Items.Add("Выберите из списка");
            foreach (AddresModel adr in sclsTypeDocumentsOfExport.List_towns)
            {
                cmb_town.Items.Add(adr.SHORTNAME + " " + adr.OFFNAME);
            }
        }

        #endregion


        protected void ImageButtonAddTownOrVil_Click(object sender, ImageClickEventArgs e)
        {
            // Добавление населенного пункта
            // если введеный город уникальный
            if (CheckingDublicate("town") == true)
            {
                string command = @"SELECT AOID FROM ";
                string nametable = "";
                // в зависимости от региона выбирается нужная таблица
                if (txtAddRegion.Text == "Тюменская область")
                {
                    command = command + "TownsAndStreets72 WHERE AOID = '";
                    nametable = "TownsAndStreets72";
                }
                else if (txtAddRegion.Text == "ЯНАО")
                {
                    command = command + "TownsAndStreets89 WHERE AOID = '";
                    nametable = "TownsAndStreets89";
                }
                else if (txtAddRegion.Text == "ХМАО")
                {
                    command = command + "TownsAndStreets86 WHERE AOID = '";
                    nametable = "TownsAndStreets86";
                }
                else command = "";

                if (command != "")
                {
                    string guid = GetGuid(command);
                    string aoguid = GetGuid(command);
                    string aolevel = "";
                    string shortname = "";

                    switch (DropDownListTypeTown.SelectedIndex)
                    {
                        case 0:
                            aolevel = "4";
                            shortname = "г";
                            break;
                        case 1:
                            aolevel = "3";
                            shortname = "р-н";
                            break;
                        case 2:
                            aolevel = "4";
                            shortname = "с/п";
                            break;
                    }

                    command = "INSERT INTO " + nametable + " (ACTSTATUS, AOLEVEL, AOGUID, AOID, SHORTNAME, OFFNAME) values (1," + aolevel + ",\'" + aoguid + "\',\'" + guid + "\',\'" + shortname + "\',\'" + txtAddTown.Text + "\');";
                    InsertToBd(command, nametable);

                    switch (nametable)
                    {
                        case "TownsAndStreets72":
                            UpdateListTown(sCommandTextSelectTowns72);
                            break;
                        case "TownsAndStreets86":
                            UpdateListTown(sCommandTextSelectTowns86);
                            break;
                        case "TownsAndStreets89":
                            UpdateListTown(sCommandTextSelectTowns89);
                            break;
                    }
                
                    txtAddTown.Clear();

                    this.ShowMessage("Запись добавлена в базу данных!");
                }              
            }
            else
            {
                this.ShowMessage("Данный объект уже есть в базе данных!");
            }
        }

        protected void ImageButtonAddVil_Click(object sender, ImageClickEventArgs e)
        {
            if (CheckingDublicate("selo") == true)
            {
                string parentid = (from l in sclsTypeDocumentsOfExport.List_towns where l.OFFNAME.ToString() == txtAddTown.Text select l.AOGUID).FirstOrDefault().ToString();

                if (CheckingDublicate("town") == true)
                {
                    string command = @"SELECT O FROM ";
                    string nametable = "";
                    if (txtAddRegion.Text == "Тюменская область")
                    {
                        command = command + "TownsAndStreets72 WHERE AOID = '";
                        nametable = "TownsAndStreets72";
                    }
                    else if (txtAddRegion.Text == "ЯНАО")
                    {
                        command = command + "TownsAndStreets89 WHERE AOID = '";
                        nametable = "TownsAndStreets89";
                    }
                    else if (txtAddRegion.Text == "ХМАО")
                    {
                        command = command + "TownsAndStreets86 WHERE AOID = '";
                        nametable = "TownsAndStreets86";
                    }
                    else command = "";

                    if (command != "")
                    {
                        string guid = GetGuid(command);
                        string aoguid = GetGuid(command);
                        string aolevel = "6";
                        string shortname = TextBoxAddTypeVillage.Text;
                        command = "INSERT INTO " + nametable + " (ACTSTATUS, AOLEVEL, AOGUID, AOID, SHORTNAME, OFFNAME, PARENTGUID) values (1," + aolevel + ",\'" + aoguid + "\',\'" + guid + "\',\'" + shortname + "\',\'" + txtAddVillage.Text + "\',\'" + parentid + "\');";
                        InsertToBd(command, nametable);
                        txtAddVillage.Clear();

                        this.ShowMessage("Запись добавлена в базу данных!");
                    }
                }
            }
            else
            {
                this.ShowMessage("Данный объект уже есть в базе данных!");
            }
        }

        protected void ImageButtonAddStreet_Click(object sender, ImageClickEventArgs e)
        {
            if (CheckingDublicate("town") == true)
            {
                if (CheckingDublicate("street") == true)
                {
                    string parentid = "";
                    if (sclsTypeDocumentsOfExport.List_streets_or_village != null && txtAddVillage.Text != "")
                        parentid = (from l in sclsTypeDocumentsOfExport.List_streets_or_village where l.OFFNAME.ToString() == txtAddVillage.Text select l.AOGUID).First().ToString();
                    else
                        parentid = (from l in sclsTypeDocumentsOfExport.List_towns where l.OFFNAME.ToString() == txtAddTown.Text select l.AOGUID).First().ToString();

                    string command = @"SELECT AOID FROM ";
                    string nametable = "";
                    if (txtAddRegion.Text == "Тюменская область")
                    {
                        command = command + "TownsAndStreets72 WHERE AOID = '";
                        nametable = "TownsAndStreets72";
                    }
                    else if (txtAddRegion.Text == "ЯНАО")
                    {
                        command = command + "TownsAndStreets89 WHERE AOID = '";
                        nametable = "TownsAndStreets89";
                    }
                    else if (txtAddRegion.Text == "ХМАО")
                    {
                        command = command + "TownsAndStreets86 WHERE AOID = '";
                        nametable = "TownsAndStreets86";
                    }
                    else command = "";

                    if (command != "")
                    {
                        string guid = GetGuid(command);
                        string aoguid = GetGuid(command);
                        string aolevel = "7";
                        string shortname = "ул";
                        command = "INSERT INTO " + nametable + " (ACTSTATUS, AOLEVEL, AOGUID, AOID, SHORTNAME, OFFNAME, PARENTGUID) values (1," + aolevel + ",\'" + aoguid + "\',\'" + guid + "\',\'" + shortname + "\',\'" + txtAddStreet.Text + "\',\'" + parentid + "\');";
                        InsertToBd(command, nametable);
                        txtAddStreet.Clear();

                        this.ShowMessage("Запись добавлена в базу данных!");
                    }
                }
                else
                {
                    this.ShowMessage("Данная улица уже есть в базе данных!");
                }
            }
            else
            {
                this.ShowMessage("Данного населенного пункта нет в базе данных!");
            }
        }


        protected void ImageButtonAddNumBuild_Click(object sender, ImageClickEventArgs e)
        {
            if (txtAddStreet.Text != "" && CheckingDublicate("street") == false)
            {
                string parentid = "";
                if (sclsTypeDocumentsOfExport.List_streets_or_village != null && txtAddStreet.Text != "" && sclsTypeDocumentsOfExport.List_streets == null)
                    parentid = (from l in sclsTypeDocumentsOfExport.List_streets_or_village where l.OFFNAME.ToString() == txtAddStreet.Text select l.AOGUID).First().ToString();
                else if (sclsTypeDocumentsOfExport.List_streets_or_village != null && txtAddStreet.Text != "" && sclsTypeDocumentsOfExport.List_streets != null && sclsTypeDocumentsOfExport.List_streets.Count > 0)
                    parentid = (from l in sclsTypeDocumentsOfExport.List_streets where l.OFFNAME.ToString() == txtAddStreet.Text select l.AOGUID).First().ToString();
                else
                    parentid = (from l in sclsTypeDocumentsOfExport.List_streets_or_village where l.OFFNAME.ToString() == txtAddStreet.Text select l.AOGUID).First().ToString();

                string command = @"SELECT HOUSEGUID FROM ";
                string nametable = "";
                if (txtAddRegion.Text == "Тюменская область")
                {
                    command = command + "HOUSE72 WHERE HOUSEGUID = '";
                    nametable = "HOUSE72";
                }
                else if (txtAddRegion.Text == "ЯНАО")
                {
                    command = command + "HOUSE89 WHERE HOUSEGUID = '";
                    nametable = "ADDROB89";
                }
                else if (txtAddRegion.Text == "ХМАО")
                {
                    command = command + "HOUSE86 WHERE HOUSEGUID = '";
                    nametable = "HOUSE86";
                }
                else command = "";

                if (command != "")
                {
                    string guid = GetGuid(command);
                    string housenum = txtAddBuildNum.Text;
                    string housekorpus = txtAddKorpusNum.Text;
                    string postalcode = txtAddPostalCode.Text;
                    string strstatus = "";
                    switch (ddlTypeBuild.SelectedIndex)
                    {
                        case 0:
                            strstatus = "0";
                            break;
                        case 1:
                            strstatus = "1";
                            break;
                        case 2:
                            strstatus = "2";
                            break;
                        case 3:
                            strstatus = "3";
                            break;
                        case 4:
                            strstatus = "4";
                            break;
                    }

                    command = "INSERT INTO " + nametable + " (HOUSEGUID, HOUSENUM, BUILDNUM, STRSTATUS, POSTALCODE, AOGUID) values (\'" + guid + "\',\'" + housenum + "\',\'" + housekorpus + "\'," + strstatus + ",\'" + postalcode + "\',\'" + parentid + "\');";
                    InsertToBd(command, nametable);
                    txtAddStreet.Clear();
                    txtAddBuildNum.Clear();
                    txtAddKorpusNum.Clear();
                    txtAddPostalCode.Clear();

                    this.ShowMessage("Запись добавлена в базу данных, необходимо обновить страницу.");
                }

            }
            else
            {
                this.ShowMessage("Для добавления дома нужно выбрать улицу");
            }
        }


    }
}