using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace ModelCreator
{
    public class GetWebControls
    {
        public bool BuildControls(string nameSpace, ArrayList tables, string pathfile, SqlConnection connection)
        {
            bool result = false;            

            foreach (DataTable dt in tables)
            {        
                string getData = string.Empty;
                string setData = string.Empty;
                string templatePopulateList = string.Empty;

                string tableName = dt.Rows[0]["Table"].ToString().Trim();

                if (getData.Equals(string.Empty))
                {
                    if (Session.Modality == Modality.Professional)
                    {
                        if (getData.Equals(string.Empty)) getData = string.Concat("\t\titem = Manager", tableName, ".GetByKeyuni(Key);\n");
                    }
                    if (Session.Modality == Modality.Easy)
                    {
                        if (getData.Equals(string.Empty)) getData = "\t\titem = item.GetByKeyuni(Key);\n";
                    }
                }                               
               
                string template = "<table>";

                foreach (DataRow dr in dt.Rows)
                {
                    if ((!dr["Name"].Equals(Session.TableKey) && (!dr["Name"].Equals("Keyuni")) && (!dr["Name"].Equals("Timespan"))))
                    {

                        string templateControl = string.Empty;
                        string tableLink = string.Empty;
                        string controlName = string.Empty;

                        #region GetValue

                        string name = dr["Name"].ToString();
                        string type = dr["Type"].ToString();
                        string dbType = dr["DbType"].ToString();
                        int length = (int)dr["Length"];
                        bool nullable = (bool)dr["Nullable"];                        

                        #endregion

                        switch (type)
                        {
                            case "decimal":
                            case "int":
                                if (type.Equals("int")) tableLink = GetInfoFieldSqlServer.IsForeignKeySimple(tableName, name);
                                if (!tableLink.Equals(string.Empty))
                                {
                                    templateControl = GetLabel(templateControl, name);
                                    controlName = string.Concat("ddl", name);
                                    templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//dropdownlist.ico"));
                                    templateControl = templateControl.Replace("***Name***", name);
                                    templateControl = templateControl.Replace("#SkinId#", GetSkinByLength(length));
                                    getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.DropDownList));
                                    setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.DropDownList, nullable));
                                    string populateList =  ManageTemplate.GetTemplate("WebTemplate//populateList.ico");
                                    populateList = populateList.Replace("***Name***", name);
                                    templatePopulateList = string.Concat(templatePopulateList,populateList);                                    
                                    break;
                                }
                                else
                                {
                                    controlName = string.Concat("txt", name);
                                    templateControl = GetLabel(templateControl, name);
                                    templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//textbox.ico"));
                                    templateControl = templateControl.Replace("***Name***", name);
                                    templateControl = templateControl.Replace("#Length#", length.ToString());
                                    templateControl = templateControl.Replace("#SkinId#", GetSkinByLength(length));
                                    getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.TextBox));
                                    setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.TextBox, nullable));

                                }
                                break;
                            case "string":
                                templateControl = GetLabel(templateControl, name);
                                controlName = string.Concat("txt", name);
                                templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//textbox.ico"));
                                templateControl = templateControl.Replace("***Name***", name);
                                templateControl = templateControl.Replace("#Length#", length.ToString());
                                templateControl = templateControl.Replace("#SkinId#", GetSkinByLength(length));
                                getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.TextBox));
                                setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.TextBox, nullable));
                                break;
                            case "System.DateTime":
                                templateControl = GetLabel(templateControl, name);
                                controlName = string.Concat("cal", name);
                                templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//calendar.ico"));
                                templateControl = templateControl.Replace("***Name***", name);
                                //templateControl = templateControl.Replace("#SkinId#", GetSkinByLength(length));
                                getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.Calendar));
                                setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.Calendar, nullable));
                                break;                            
                        }

                        if ((!nullable))
                        {
                            string templateRequiredFieldValidator = string.Empty;
                            templateRequiredFieldValidator = string.Concat(ManageTemplate.GetTemplate("WebTemplate//requiredFieldValidator.ico"));
                            templateRequiredFieldValidator = templateRequiredFieldValidator.Replace("***Name***", name);
                            templateRequiredFieldValidator = templateRequiredFieldValidator.Replace("#ControlName#", controlName);
                            templateControl = string.Concat(templateControl, "&nbsp;", templateRequiredFieldValidator);
                        }

                        if (!templateControl.Equals(string.Empty)) templateControl = CloseRow(templateControl);

                        template = string.Concat(template, templateControl);
                    }
                }
                template = string.Concat(template, "</table>");

                if (ManageTemplate.WriteTemplate(string.Concat(pathfile, "WebControls//"), template, tableName, ".aspx")) result = true;
                else result = false;

                #region GetData
               

                string templateGetData = string.Concat(ManageTemplate.GetTemplate("WebTemplate//getData.ico"));
                templateGetData = templateGetData.Replace("***Body***", getData);

                string templateSetData = string.Concat(ManageTemplate.GetTemplate("WebTemplate//setData.ico"));
                templateSetData = templateSetData.Replace("***Body***", setData);

                string finallyTemplate = string.Concat(templateGetData, "\n\n", templateSetData);

                finallyTemplate = string.Concat(finallyTemplate,"\n\n", templatePopulateList);

                if (ManageTemplate.WriteTemplate(string.Concat(pathfile, "WebControls//Get&SetData//"), finallyTemplate, tableName, ".ico")) result = true;


                #endregion


            }
            return true;           
        }

        public bool BuildControlsPage(string nameSpace, ArrayList tables, string pathfile, SqlConnection connection)
        {
            bool result = false;

            foreach (DataTable dt in tables)
            {
                string getData = string.Empty;
                string setData = string.Empty;
                string templatePopulateList = string.Empty;

                string tableName = dt.Rows[0]["Table"].ToString().Trim();

                if (getData.Equals(string.Empty))
                {
                    if (Session.Modality == Modality.Professional)
                    {
                        if (getData.Equals(string.Empty)) getData = string.Concat("\t\titem = Manager", tableName, ".GetByKeyuni(Key);\n");
                    }
                    if (Session.Modality == Modality.Easy)
                    {
                        //if (getData.Equals(string.Empty)) getData = "\t\titem = item.GetByKeyuni(Key);\n";
                    }
                }

                string template = "<table>";

                foreach (DataRow dr in dt.Rows)
                {
                    if ((!dr["Name"].Equals(Session.TableKey) && (!dr["Name"].Equals("Keyuni")) && (!dr["Name"].Equals("Timespan"))))
                    {                                           
                        string templateControl = string.Empty;
                        string tableLink = string.Empty;

                        #region GerValue

                        string name = dr["Name"].ToString();
                        string type = dr["Type"].ToString();
                        string dbType = dr["DbType"].ToString();
                        int length = (int)dr["Length"];
                        bool nullable = (bool)dr["Nullable"];

                        bool isSearch = false;
                        int searchPosition = 0;
                        bool isDetail = false;
                        int detailPosition = 0;
                        string descriptionName = string.Empty;

                        GetDatatableSqlServer.GetPlusInformation(tableName, name, ref descriptionName, ref isSearch, ref searchPosition, ref isDetail, ref detailPosition);

                        if (descriptionName.Equals(string.Empty)) descriptionName = name;

                        #endregion

                        switch (type)
                        {
                            case "decimal":
                            case "int":
                                if (type.Equals("int")) tableLink = GetInfoFieldSqlServer.IsForeignKeySimple(tableName, name);
                                if (!tableLink.Equals(string.Empty))
                                {
                                    templateControl = GetLabel(templateControl,name, descriptionName);
                                    templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//dropdownlist.ico"));
                                    templateControl = templateControl.Replace("***Name***", name);                                    
                                    getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.DropDownList));
                                    setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.DropDownList, nullable));
                                    string nameTable = name.Replace("Code", string.Empty);
                                    string populateList = ManageTemplate.GetTemplate("WebTemplate//populateList.ico");
                                    populateList = populateList.Replace("***Name***", nameTable);
                                    templatePopulateList = string.Concat(templatePopulateList, populateList);
                                    break;
                                }
                                else
                                {

                                    templateControl = GetLabel(templateControl, name, descriptionName);
                                    templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//textbox.ico"));
                                    templateControl = templateControl.Replace("***Name***", name);
                                    templateControl = templateControl.Replace("#Length#", length.ToString());
                                    templateControl = templateControl.Replace("#SkinId#", GetSkinByLength(length));
                                    getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.TextBox));
                                    setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.TextBox, nullable));

                                }
                                break;
                            case "string":
                                templateControl = GetLabel(templateControl, name, descriptionName);
                                templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//textbox.ico"));
                                templateControl = templateControl.Replace("***Name***", name);
                                templateControl = templateControl.Replace("#Length#", length.ToString());
                                templateControl = templateControl.Replace("#SkinId#", GetSkinByLength(length));
                                getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.TextBox));
                                setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.TextBox, nullable));
                                break;
                            case "System.DateTime":
                                templateControl = GetLabel(templateControl, name, descriptionName);
                                templateControl = string.Concat(templateControl, ManageTemplate.GetTemplate("WebTemplate//calendar.ico"));
                                templateControl = templateControl.Replace("***Name***", name);
                                templateControl = templateControl.Replace("#SkinId#", GetSkinByLength(length));
                                getData = string.Concat(getData, WriteGetDataControl(name, type, WebControl.Calendar));
                                setData = string.Concat(setData, WriteSetDataControl(name, type, WebControl.Calendar, nullable));
                                break;
                        }

                        if ((!nullable))
                        {
                            string templateRequiredFieldValidator = string.Empty;
                            templateRequiredFieldValidator = string.Concat(ManageTemplate.GetTemplate("WebTemplate//requiredFieldValidator.ico"));
                            templateRequiredFieldValidator = templateRequiredFieldValidator.Replace("***Name***", name);
                            templateControl = string.Concat(templateControl, "&nbsp;", templateRequiredFieldValidator);
                        }

                        if (!templateControl.Equals(string.Empty)) templateControl = CloseRow(templateControl);

                        template = string.Concat(template, templateControl);
                    }
                }
                template = string.Concat(template, "</table>");

                if (ManageTemplate.WriteTemplate(string.Concat(pathfile, "WebControls//"), template, tableName, ".aspx")) result = true;
                else result = false;

                #region GerData


                string templateGetData = string.Concat(ManageTemplate.GetTemplate("WebTemplate//getData.ico"));
                templateGetData = templateGetData.Replace("***Body***", getData);

                string templateSetData = string.Concat(ManageTemplate.GetTemplate("WebTemplate//setData.ico"));
                templateSetData = templateSetData.Replace("***Body***", setData);

                string finallyTemplate = string.Concat(templateGetData, "\n\n", templateSetData);

                

                bool pageEasy = true;
                if (pageEasy)
                {
                    string templateHtml = string.Concat(ManageTemplate.GetTemplate("WebForm//pageListDetail.aspx"));
                    templateHtml = templateHtml.Replace("***Name***", tableName);
                    templateHtml = templateHtml.Replace("#WebControls#", template);

                    if (ManageTemplate.WriteTemplate(string.Concat(pathfile, "WebControls//Page//"), templateHtml, tableName, "ListDetail.aspx")) result = true;

                    string templateCode = string.Concat(ManageTemplate.GetTemplate("WebForm//pageListDetail.aspx.ico"));
                    templateCode = templateCode.Replace("***Name***", tableName);
                    templateCode = templateCode.Replace("#GetData#", getData);
                    templateCode = templateCode.Replace("#SetData#", setData);
                    templateCode = templateCode.Replace("#PopulateList#", templatePopulateList);

                    if (ManageTemplate.WriteTemplate(string.Concat(pathfile, "WebControls//Page//"), templateCode, tableName, "ListDetail.aspx.ico")) result = true;
                }
                else
                {
                    string templateHtml = string.Concat(ManageTemplate.GetTemplate("WebForm//pageDetail.aspx"));
                    templateHtml = templateHtml.Replace("***Name***", tableName);
                    templateHtml = templateHtml.Replace("#WebControls#", template);

                    if (ManageTemplate.WriteTemplate(string.Concat(pathfile, "WebControls//Page//"), templateHtml, tableName, "Detail.aspx")) result = true;

                    string templateCode = string.Concat(ManageTemplate.GetTemplate("WebForm//pageDetail.aspx.ico"));
                    templateCode = templateCode.Replace("***Name***", tableName);
                    templateCode = templateCode.Replace("#GetData#", getData);
                    templateCode = templateCode.Replace("#SetData#", setData);
                    templateCode = templateCode.Replace("#PopulateList#", templatePopulateList);

                    if (ManageTemplate.WriteTemplate(string.Concat(pathfile, "WebControls//Page//"), templateCode, tableName, "Detail.aspx.ico")) result = true;
                }

                #endregion


            }
            return true;
        }

        private string GetLabel(string templateRow, string fieldName, string descriptionName)
        {
            string labelTemplate = OpenRow(templateRow);

            labelTemplate = string.Concat(labelTemplate, ManageTemplate.GetTemplate("WebTemplate\\label.ico"));
            labelTemplate = labelTemplate.Replace("***Name***", fieldName);
            labelTemplate = labelTemplate.Replace("#Description#", descriptionName);
            
            labelTemplate = string.Concat(labelTemplate,"</td><td>");

            return  string.Concat(templateRow,labelTemplate);
        }

        private string GetLabel(string templateRow, string fieldName)
        {
            string labelTemplate = OpenRow(templateRow);

            labelTemplate = string.Concat(labelTemplate, ManageTemplate.GetTemplate("WebTemplate\\label.ico"));
            labelTemplate = labelTemplate.Replace("***Name***", fieldName);
   
            labelTemplate = string.Concat(labelTemplate, "</td><td>");

            return string.Concat(templateRow, labelTemplate);
        }
        
        private string OpenRow(string template)
        {
            return "<tr><td>";
        }

        private string CloseRow(string template)
        {
            return string.Concat(template,"</td></tr>");
        }
       
        private string GetSkinByLength(int length)
        {
            if (length < 10) return "txtDataMin";
            {
                if (length <= 20) return "txtData";
                else
                {
                    if (length <= 50) return "txtDataMedium";
                    else return "txtDataMax";
                }
            }
        }

        private string WriteGetDataControl(string name, string type, WebControl webControl)
        {
            string control = string.Empty;
            string prefix = "\n\t\t";

            switch (webControl)
            {
                case WebControl.DropDownList :
                    switch (type)
                    {
                        case "decimal":
                        case "float":
                        case "int": control = string.Concat(prefix,"ddl", name, ".SelectedValue = item.", name, ".ToString();"); break;
                        case "string": control = string.Concat(prefix, "ddl", name, ".SelectedValue = item.", name, ";"); break;
                    }                    
                    break;
                case WebControl.TextBox:
                    switch (type)
                    {
                        case "decimal":
                        case "float":
                        case "int": control = string.Concat(prefix, "txt", name, ".Text = item.", name, ".ToString();"); break;
                        case "string": control = string.Concat(prefix, "txt", name, ".Text = item.", name, ";"); break;
                    }
                    break;
                case WebControl.Calendar:
                    control = string.Concat(prefix, "cal", name, ".SelectedDate = item.", name, ";"); break;                                        
                case WebControl.CheckBox:
                    control = string.Concat(prefix, "chk", name, ".Cheked = item.", name); break;                    
                                                
            }

            return control;
         }

        private string WriteSetDataControl(string name, string type, WebControl webControl, bool nullable)
        {
            string control = string.Empty;
            string prefix = "\n\t\t";

            if (nullable)
            {
                switch (webControl)
                {
                    case WebControl.DropDownList:
                        switch (type)
                        {
                            case "decimal": control = string.Concat(prefix, "item.", name, " = ", "Decimal.Parse(", "ddl", name, ".SelectedValue", ");"); break;
                            case "float": control = string.Concat(prefix, "item.", name, " = ", "float.Parse(", "ddl", name, ".SelectedValue", ");"); break;
                            case "int": control = string.Concat(prefix, "item.", name, " = ", "Int32.Parse(", "ddl", name, ".SelectedValue", ");"); break;
                            case "string": control = string.Concat(prefix, "item.", name, " = ddl", name, ".SelectedValue", ";"); break;
                        }
                        break;
                    case WebControl.TextBox:
                        switch (type)
                        {
                            case "decimal": control = string.Concat(prefix, "item.", name, " = ", "Decimal.Parse(", "txt", name, ".Text", ");"); break;
                            case "float": control = string.Concat(prefix, "item.", name, " = ", "float.Parse(", "txt", name, ".Text", ");"); break;
                            case "int": control = string.Concat(prefix, "item.", name, " = ", "Int32.Parse(", "txt", name, ".Text", ");"); break;
                            case "string": control = string.Concat(prefix, "item.", name, " = txt", name, ".Text;"); break;
                        }
                        break;
                    case WebControl.Calendar:
                        control = string.Concat(prefix, "item.", name, " = cal", name, ".SelectedDate;"); break;
                    case WebControl.CheckBox:
                        control = string.Concat(prefix, "item.", name, " = chk", name, ".Checked;"); break;
                }
            }
            else
            {
                string structureDropDownList = "\n\t\titem.{0} = (!WebTools.ComboValue(ddl{0})) ? Null.{1}(ddl{0}.SelectedValue);";
                string structureTextBox = "\n\t\titem.{0} = txt{0}.Text.Equals(string.Empty) ? Null.{1}(txt{0}.Text);";                
                switch (webControl)
                {
                    case WebControl.DropDownList:
                        switch (type)
                        {
                            case "decimal": control = string.Format(structureDropDownList, name, "DecimalNull : Decimal.Parse"); break;
                            case "float": control = string.Format(structureDropDownList, name, "FloatNull : float.Parse"); break;
                            case "int": control = string.Format(structureDropDownList, name, "IntNull : Int32.Parse"); break;
                            case "string": control = string.Concat(prefix, "item.", name, " = ddl", name, ".SelectedValue;"); break;
                        }
                        break;
                    case WebControl.TextBox:
                        switch (type)
                        {
                            case "decimal": control = string.Format(structureTextBox, name, "DecimalNull : Decimal.Parse"); break;
                            case "float": control = string.Format(structureTextBox, name, "FloatNull : float.Parse"); break;
                            case "int": control = string.Format(structureTextBox, name, "IntNull : Int32.Parse"); break;
                            case "string": control = string.Concat(prefix, "item.", name, " = txt", name, ".Text;"); break;
                        }
                        break;
                    case WebControl.Calendar:
                        control = string.Format(structureTextBox, name, "DateTime.MinValue : DateTime.Parse"); break;
                    case WebControl.CheckBox:
                        control = string.Concat(prefix, "item", name, " = chk", name, ".Checked;"); break;
                }
            }

            return control;
        }
    }
}