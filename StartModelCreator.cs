using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.SqlClient;
using Gupta.SQLBase.Data;
using System.Data.OleDb;
using Devart.Data.PostgreSql;

namespace ModelCreator
{
    public partial class StartModelCreator : Form
    {
        MessageGeneral MSG;

        SqlConnection connectionSqlServer = new SqlConnection();
        SQLBaseConnection connectionSqlBase = new SQLBaseConnection();
        SQLBaseConnection connectionMySql = new SQLBaseConnection();
        OleDbConnection connectionAccess = new OleDbConnection();
        PgSqlConnection connectionPostGres = new PgSqlConnection();

        Color colorBase = new Color();

        [STAThread]
        static void Main()
        {
            Application.Run(new StartModelCreator());
        }

        public StartModelCreator()
        {               
            InitializeComponent();
            InitializeSettings();

            RefreshColor(true);

            Session.ApplicationFolder = string.Concat(Application.StartupPath.Replace("bin\\Debug", string.Empty), "Template");

            #region CONTROLLO SULLA SICUREZZA
            //Security security = new Security();
            //if (!security.CheckExpiration())
            //{
            //    MessageBox.Show("Software scaduto. Contattare l'amministratore."); Close();
            //}
            //else
            //{
            //    if (!security.CheckExecute())
            //    {
            //        MessageBox.Show("Software usato troppe volte. Contattare l'amministratore.");
            //        Close();
            //    }
            //    else
            //    {
            //        //MessageBox.Show(string.Concat("ModelCreator  - Il software non sarà più accessibile dopo ", security.DateExpired.ToLongDateString(), "."));
            //        //StartModelCreator.ActiveForm.Text = string.Concat(StartModelCreator.ActiveForm.Text, " - Utilizzabile ancora per ", security.ExecuteProgram.ToString() ," volte.");
            //        Load();
            //    }
            //}
            #endregion

            SetLanguage(Session.Language);

            Load();
        }

        private void SetLanguage(string language)
        {
            switch (language)
            {
                case "IT": MSG = new Language_it(); break;
                case "EN": MSG = new Language_en(); break;
            }

            // TAB : DATALAYER
            tpDataLayer.Text = MSG.DataLayer_Info();
            btnTableListCheckedAll.Text = MSG.DataLayer_Check();
            btnTableListUnCheckedAll.Text = MSG.DataLayer_UnCheck();
            lbl_DataLayer_FilterList.Text = MSG.DataLayer_Tag_FilterList();
            btnFilter.Text = MSG.DataLayer_Filter();
            btnOpenFolder.Text = MSG.DataLayer_GoFileFolder();
            btnCreate.Text = MSG.DataLayer_CreateDataLayer();

            // TAB : SETTINGS
            gbProvider.Text = MSG.Settings_DataProvider();
            btnSaveSetting.Text = MSG.Settings_SaveInformation();
            lblTagSuffix.Text = MSG.Settings_ClassSuffix();

            gbModality.Text = MSG.Settings_Modality_Control();
            lblSettings_Modality.Text = MSG.Settings_Modality();            
            rbSimple.Text = MSG.Settings_Modality_Simple();
            rbProfessional.Text = MSG.Settings_Modality_Professional();
            rbThreeLayer.Text = MSG.Settings_Modality_ThreeLayer();

            lblSettings_Framework.Text = MSG.Settings_Framework();

            gbReadingFunction.Text = MSG.Settings_CodeInReading_Control();
            rbReadTableCode.Text = MSG.Settings_CodeInReading_MainCode();
            rbReadKeyuni.Text = MSG.Settings_CodeInReading_Keyuni();
            lblSettings_CodeUsingReading.Text = MSG.Settings_CodeInReading();

            gbManageKey.Text = MSG.Settings_ManageKey_Control();
            lblSettings_ManageKey.Text = MSG.Settings_ManageKey();
            rbManageKeyDatabase.Text = MSG.Settings_ManageKey_Database();
            rbManageKeysCustom.Text = MSG.Settings_ManageKey_Custom();

            gbMainTableKey.Text = MSG.Settings_MainTableKey_Control();
            lblSettings_MainTableKey.Text = MSG.Settings_MainTableKey();
            rbTableCode.Text = MSG.Settings_MainTableKey_Code();
            rbIdCode.Text = MSG.Settings_MainTableKey_id();

            gbSpecialFields.Text = MSG.Settings_SpecialFields_Control();
            lblSettings_SpecialField.Text = MSG.Settings_SpecialFields();

            gbGuid.Text = MSG.Settings_GuidCreator_Control();
            lblSettings_GuidCreator.Text = MSG.Settings_GuidCreator();
          
            chkIsCutomDecimal.Text = MSG.Settings_CustomBool_Control();
            chkRelationshipClass.Text = MSG.Settings_SuperClass_Control();
            chkCreateCustomClass.Text = MSG.Settings_CustomClass_Control();
            chkFirstRelase.Text = MSG.Settings_FirstRelease_Control();
            chkNullLogic.Text = MSG.Settings_NullLogic_Control();

            lblSettings_CustomBool.Text = MSG.Settings_CustomBool();
            lblSettings_SuperClass.Text = MSG.Settings_SuperClass();
            lblSettings_CustomClass.Text = MSG.Settings_CustomClass();
            lblSettings_FirstRelease.Text = MSG.Settings_FirstRelease();
            lblSettings_NullLogic.Text = MSG.Settings_NullLogic();

            lblSettings_Namespace.Text = MSG.Settings_Namespace();

            lblSettings_FolderDestination.Text = MSG.Settings_FolderDestination();
            gbFolder.Text = MSG.Settings_FolderDestination_Control(); 

            lblSettings_SqlScriptFile.Text = MSG.Settings_SQLScriptFile();
            gbSqlScriptFile.Text = MSG.Settings_SQLScriptFile_Control();

            gbColor.Text = MSG.Settings_Color();
            rbColorGrey.Text = MSG.Settings_Color_Grey();
            rbColorOrange.Text = MSG.Settings_Color_Orange();
            rbColorBlue.Text = MSG.Settings_Color_LightBlue();
            rbGreen.Text = MSG.Settings_Color_Green();

            gbLanguage.Text = MSG.Settings_LanguageGroup();
            rbLanguageIT.Text = MSG.Settings_LanguageIT();
            rbLanguageEN.Text = MSG.Settings_LanguageEN();
        }

        private void Load()
        {
            //TODO             
            if (TestConnection())
            {
                chlTable.Items.Clear();

                this.Cursor = Cursors.WaitCursor;

                ArrayList tables = new ArrayList();

                switch (Session.Provider)
                {
                    case Provider.SqlServer: tables = GetDatatableSqlServer.GetSchema(txtFilterSearch.Text.Trim()); break;
                    case Provider.SqlBase2:
                    case Provider.SqlBase: tables = GetDatatableSqlBase.GetListTables(txtFilterSearch.Text.Trim()); break;
                    case Provider.MySql: break;
                    case Provider.Access: 
                    case Provider.Access2: tables = GetDatatableAccess.GetListTables(txtFilterSearch.Text.Trim()); break;
                    case Provider.PostGres: tables = GetDatatablePostGres.GetListTables(txtFilterSearch.Text.Trim()); break;
                    case Provider.AutoTask: tables = GetDatatableAutoTask.GetListTables(txtFilterSearch.Text.Trim()); break;
                }

                PutListTables(tables);
                TestTable();

                LoadListClass();

                this.Text = string.Concat("  MODEL CREATOR ", Session.Version);

                lblNumberTable.Text = string.Format(MSG.DataLayer_Calculate_TableSelected(), chlTable.CheckedIndices.Count, chlTable.Items.Count);

                this.Cursor = Cursors.Default;
            }
            else MessageBox.Show("Errore nella connessione", "Errore di connessione", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void LoadListClass()
        {
            string listClass = ManageTemplate.GetTemplate("ListClass/ListLoad.ico");
            bool firstRecord = true;
            string[] list = listClass.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in list)
            {
                if (firstRecord)
                {
                    if (s == "[false]")
                        break;
                    firstRecord = false;
                }
                else
                {
                    for (int i = 0; i < chlTable.Items.Count; i++)
                    {
                        if (chlTable.Items[i].Equals(s))
                        {
                            chlTable.SetItemChecked(i, true);
                        }
                    }
                }
            }
        }

        private void InitializeSettings()
        {
            Session.Version = ConfigurationManager.AppSettings["Version"].ToString();

            Session.AuthorName = ConfigurationManager.AppSettings["AuthorName"].ToString();
            txtAuthorName.Text = Session.AuthorName;
            btnClearStored.Visible = false;

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.SqlServer.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.SqlServer;
                rbProviderSqlServer.Checked = true;
                rbProviderSqlBase.Checked = false;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = false;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = false;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_SQLSERVER;
                lblDatabaseName.Text = Session.ConnectionString.Substring(0, Session.ConnectionString.IndexOf(';'));
                lblServerName.Text = Session.ConnectionString.Substring(0, Session.ConnectionString.IndexOf(';'));
                btnClearStored.Visible = true;
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.SqlBase.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.SqlBase;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = true;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = false;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = false;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_SQLBASE;
                lblDatabaseName.Text = Session.ConnectionString.Substring(0, Session.ConnectionString.IndexOf(';'));
                lblServerName.Text = Session.ConnectionString.Substring(0, Session.ConnectionString.IndexOf(';'));
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.SqlBase2.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.SqlBase2;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = true;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = false;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = false;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_SQLBASE;
                lblDatabaseName.Text = Session.ConnectionString.Substring(0, Session.ConnectionString.IndexOf(';'));
                lblServerName.Text = Session.ConnectionString.Substring(0, Session.ConnectionString.IndexOf(';'));
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.MySql.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.MySql;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = false;
                rbProviderMySql.Checked = true;
                rbProviderAccess.Checked = false;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = false;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_MYSQL;
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.Access.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.Access;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = false;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = true;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = false;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_ACCESS;
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.Access2.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.Access2;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = false;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = true;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = false;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_ACCESS;
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.PostGres.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.PostGres;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = false;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = false;
                rbProviderPostGres.Checked = true;
                rbProviderOracle.Checked = false;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_POSTGRES;
                string[] partConnection = Session.ConnectionString.Split(';');
                lblServerName.Text = partConnection[2].Replace("Host=", string.Empty);
                lblDatabaseName.Text = partConnection[4].Replace("Database=", string.Empty);
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.Oracle.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.Oracle;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = false;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = false;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = true;
                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_ORACLE;
            }

            if (ConfigurationManager.AppSettings["Provider"].ToString().Equals(Provider.AutoTask.ToString()))
            {
                Session.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                Session.Provider = Provider.AutoTask;
                rbProviderSqlServer.Checked = false;
                rbProviderSqlBase.Checked = false;
                rbProviderMySql.Checked = false;
                rbProviderAccess.Checked = false;
                rbProviderPostGres.Checked = false;
                rbProviderOracle.Checked = true;
                
                lblServerName.Visible = false;
                lblDatabaseName.Visible = false;
                lblTag_Database.Visible = false;
                pbDatabase.Visible = false;                

                lbl_DataLayer_Tag_Server.Text = "WEBSERVICES";
                
                txt_DataLayer_GetWebService.Visible = true;

                pbProvider.Image = global::ModelCreator.Properties.Resources.Provider_AUTOTASK;
            }

            Session.ClassSuffix = txtSuffix.Text = ConfigurationManager.AppSettings["SuffixClassName"].ToString();
            
            Session.Owner = ConfigurationManager.AppSettings["Owner"].ToString();
            Session.Color = ConfigurationManager.AppSettings["Color"].ToString();
            Session.ManageKey = ConfigurationManager.AppSettings["ManageKey"].ToString();

            txtConnectionString.Text = Session.ConnectionString;
            txt_DataLayer_GetWebService.Text = Session.ConnectionString;

            Session.NamespaceDataLayer = ConfigurationManager.AppSettings["NamespaceDataLayer"].ToString();
            Session.NamespaceBusinessLayer = ConfigurationManager.AppSettings["NamespaceBusinessLayer"].ToString();
            Session.Folder = ConfigurationManager.AppSettings["ClassDirectory"].ToString();
            Session.SqlScriptFile = ConfigurationManager.AppSettings["SqlScriptFile"].ToString();
            Session.IsCustomDecimal = bool.Parse(ConfigurationManager.AppSettings["IsCustomDecimal"].ToString());
            Session.SuperClass = bool.Parse(ConfigurationManager.AppSettings["SuperClass"].ToString());

            Session.PathCopyBusiness = ConfigurationManager.AppSettings["PathCopyBusiness"].ToString();
            Session.PathCopyDataLayer = ConfigurationManager.AppSettings["PathCopyDataLayer"].ToString();


            txtNameSpaceDataLayer.Text = Session.NamespaceDataLayer;
            txtFolder.Text = Session.Folder;
            txtSqlScriptFile.Text = Session.SqlScriptFile;

            chkIsCutomDecimal.Checked = Session.IsCustomDecimal;
            chkRelationshipClass.Checked = Session.SuperClass;

            if (ConfigurationManager.AppSettings["ManageKey"].ToString().Equals("Database"))
            {
                rbManageKeyDatabase.Checked = true;
                rbManageKeysCustom.Checked = false;
            }
            else
            {
                rbManageKeyDatabase.Checked = false;
                rbManageKeysCustom.Checked = true;
            }

            if (ConfigurationManager.AppSettings["Modality"].ToString().Equals(Modality.Easy.ToString()))
            {
                Session.Modality = Modality.Easy;
                rbSimple.Checked = true;
                rbProfessional.Checked = false;
                rbThreeLayer.Checked = false;
            }

            if (ConfigurationManager.AppSettings["Modality"].ToString().Equals(Modality.Professional.ToString()))
            {
                Session.Modality = Modality.Professional;
                rbSimple.Checked = false;
                rbProfessional.Checked = true;
                rbThreeLayer.Checked = false;
            }

            if (ConfigurationManager.AppSettings["Modality"].ToString().Equals(Modality.ThreeLayer.ToString()))
            {
                Session.Modality = Modality.ThreeLayer;
                rbSimple.Checked = false;
                rbProfessional.Checked = true;
                rbThreeLayer.Checked = true;
            }

            if (ConfigurationManager.AppSettings["GuidCreator"].ToString().Equals("Database"))
            {
                Session.GuidCreator = "Database";
                rbDatabase.Checked = true;
                rbFramework.Checked = false;
            }
            else
            {
                Session.GuidCreator = "Framework";
                rbDatabase.Checked = false;
                rbFramework.Checked = true;
            }

            if (ConfigurationManager.AppSettings["Framework"].ToString().Equals("20"))
            {
                Session.Framework = Framework.net20;
                rbFramework20.Checked = true;
            }
            else
            {
                Session.Framework = Framework.net35;
                rbFramework35.Checked = true;
            }

            Session.Code = ConfigurationManager.AppSettings["Code"].ToString();

            switch (Session.Code.ToUpper())
            {
                case "TABLECODE": rbTableCode.Checked = true; rbIdCode.Checked = false; rbOtherCode.Checked = false; txtOtherCode.Enabled = false; txtOtherCode.Text = string.Empty; break;
                case "ID": rbTableCode.Checked = false; rbIdCode.Checked = true; rbOtherCode.Checked = false; txtOtherCode.Enabled = false; txtOtherCode.Text = string.Empty; break;
                default: rbTableCode.Checked = false; rbIdCode.Checked = false; rbOtherCode.Checked = true; txtOtherCode.Enabled = true; txtOtherCode.Text = Session.Code; break;
            }

            Session.Timespan = bool.Parse(ConfigurationManager.AppSettings["Timespan"].ToString());
            Session.Keyuni = bool.Parse(ConfigurationManager.AppSettings["Keyuni"].ToString());

            chkTimespan.Checked = Session.Timespan;
            gbGuid.Enabled = chkKeyuni.Checked;

            Session.Comment = bool.Parse(ConfigurationManager.AppSettings["Comment"].ToString());

            if (Session.Comment)
            {
                txtAuthorName.Text = Session.AuthorName;
                txtAuthorName.Enabled = true;
            }
            else
            {
                txtAuthorName.Text = string.Empty;
                txtAuthorName.Enabled = false;
            }

            #region Database Settings

            //Session.Insert = ConfigurationManager.AppSettings["Insert"].ToString();
            //txtInsert.Text = Session.Insert;

            //Session.Update = ConfigurationManager.AppSettings["Update"].ToString();
            //txtUpdate.Text = Session.Update;

            //Session.Delete = ConfigurationManager.AppSettings["Delete"].ToString();
            //txtDelete.Text = Session.Delete;

            //Session.Read = ConfigurationManager.AppSettings["Read"].ToString();
            ////txtDelete.Text = Session.Delete;

            //Session.GetCollection = ConfigurationManager.AppSettings["GetCollection"].ToString();
            //txtGetCollection.Text = Session.GetCollection;

            //Session.GetByCode = ConfigurationManager.AppSettings["GetByCode"].ToString();
            //txtGetByCode.Text = Session.GetByCode;

            //Session.GetByKeyuni = ConfigurationManager.AppSettings["GetByKeyuni"].ToString();
            //txtGetByKeyuni.Text = Session.GetByKeyuni;

            Session.InsertSql = ConfigurationManager.AppSettings["InsertSql"].ToString();
            Session.InsertFunction = ConfigurationManager.AppSettings["InsertFunction"].ToString();
            Session.ReadFunction = ConfigurationManager.AppSettings["ReadFunction"].ToString();
            Session.CloneSql = ConfigurationManager.AppSettings["CloneSql"].ToString();
            Session.CloneFunction = ConfigurationManager.AppSettings["CloneFunction"].ToString();
            Session.UpdateSql = ConfigurationManager.AppSettings["UpdateSql"].ToString();
            Session.UpdateFunction = ConfigurationManager.AppSettings["UpdateFunction"].ToString();
            Session.DeleteSql = ConfigurationManager.AppSettings["DeleteSql"].ToString();
            Session.DeleteFunction = ConfigurationManager.AppSettings["DeleteFunction"].ToString();
            Session.GetCollectionSql = ConfigurationManager.AppSettings["GetCollectionSql"].ToString();
            Session.GetCollectionFunction = ConfigurationManager.AppSettings["GetCollectionFunction"].ToString();
            Session.GetByCodeSql = ConfigurationManager.AppSettings["GetByCodeSql"].ToString();
            Session.GetByCodeFunction = ConfigurationManager.AppSettings["GetByCodeFunction"].ToString();
            Session.GetByKeyuniSql = ConfigurationManager.AppSettings["GetByKeyuniSql"].ToString();
            Session.GetByKeyuniFunction = ConfigurationManager.AppSettings["GetByKeyuniFunction"].ToString();
            Session.GetCustomItemSql = ConfigurationManager.AppSettings["GetCustomItemSql"].ToString();
            Session.GetCustomItemFunction = ConfigurationManager.AppSettings["GetCustomItemFunction"].ToString();
            Session.GetCustomCollectionSql = ConfigurationManager.AppSettings["GetCustomCollectionSql"].ToString();
            Session.GetCustomCollectionFunction = ConfigurationManager.AppSettings["GetCustomCollectionFunction"].ToString();

            Session.View = ConfigurationManager.AppSettings["View"].ToString();
            txtView.Text = Session.View;

            dgvNames.Rows.Add(9);
            dgvNames.Rows[0].Cells[0].Value = true;
            dgvNames.Rows[0].Cells[1].Value = "Insert";
            dgvNames.Rows[0].Cells[2].Value = Session.InsertSql;
            dgvNames.Rows[0].Cells[3].Value = Session.InsertFunction;

            dgvNames.Rows[1].Cells[0].Value = true;
            dgvNames.Rows[1].Cells[1].Value = "Read";
            dgvNames.Rows[1].Cells[2].Value = string.Empty;
            dgvNames.Rows[1].Cells[2].ReadOnly = true;
            dgvNames.Rows[1].Cells[2].Style.BackColor = System.Drawing.Color.LightGray;
            dgvNames.Rows[1].Cells[3].Value = Session.ReadFunction;

            dgvNames.Rows[2].Cells[0].Value = true;
            dgvNames.Rows[2].Cells[1].Value = "Update";
            dgvNames.Rows[2].Cells[2].Value = Session.UpdateSql;
            dgvNames.Rows[2].Cells[3].Value = Session.UpdateFunction;

            dgvNames.Rows[3].Cells[0].Value = true;
            dgvNames.Rows[3].Cells[1].Value = "Delete";
            dgvNames.Rows[3].Cells[2].Value = Session.DeleteSql;
            dgvNames.Rows[3].Cells[3].Value = Session.DeleteFunction;

            dgvNames.Rows[4].Cells[0].Value = true;
            dgvNames.Rows[4].Cells[1].Value = "GetCollection";
            dgvNames.Rows[4].Cells[2].Value = Session.GetCollectionSql;
            dgvNames.Rows[4].Cells[3].Value = Session.GetCollectionFunction;

            dgvNames.Rows[5].Cells[0].Value = true;
            dgvNames.Rows[5].Cells[1].Value = "GetByCode";
            dgvNames.Rows[5].Cells[2].Value = Session.GetByCodeSql;
            dgvNames.Rows[5].Cells[3].Value = Session.GetByKeyuniFunction;

            if (Session.Keyuni)
            {
                dgvNames.Rows[6].Cells[0].Value = true;
                dgvNames.Rows[6].Cells[1].Value = "GetByKeyuni";
                dgvNames.Rows[6].Cells[2].Value = Session.GetByKeyuniSql;
                dgvNames.Rows[6].Cells[3].Value = Session.GetCollectionFunction;
            }

            dgvNames.Rows[7].Cells[0].Value = true;
            dgvNames.Rows[7].Cells[1].Value = "Custom Item";
            dgvNames.Rows[7].Cells[2].Value = Session.GetCustomItemSql;
            dgvNames.Rows[7].Cells[3].Value = Session.GetCustomItemFunction;

            dgvNames.Rows[8].Cells[0].Value = true;
            dgvNames.Rows[8].Cells[1].Value = "Custom Coll.";
            dgvNames.Rows[8].Cells[2].Value = Session.GetCustomCollectionSql;
            dgvNames.Rows[8].Cells[3].Value = Session.GetCustomCollectionFunction;

            #endregion

            chkKeyuni.Checked = Session.Keyuni;

            Session.ReadingFunctionCode = ConfigurationManager.AppSettings["ReadingFunctionCode"].ToString();

            if (Session.ReadingFunctionCode == "MainCode")
                rbReadKeyuni.Checked = true;

            if (Session.ReadingFunctionCode == "Keyuni")
                rbReadTableCode.Checked = true;

            Session.IsUnknown = ConfigurationManager.AppSettings["IsUnknown"].ToString();
            txtIsUnknown.Text = Session.IsUnknown;

            Session.CreateCustomClass = bool.Parse(ConfigurationManager.AppSettings["CreateCustomClass"].ToString());
            chkCreateCustomClass.Checked = Session.CreateCustomClass;
            chkFirstRelase.Checked = Session.CreateFirstRelease;

            Session.NullLogic = bool.Parse(ConfigurationManager.AppSettings["NullLogic"].ToString());
            Session.CreateFirstRelease = bool.Parse(ConfigurationManager.AppSettings["CreateFirstRelease"].ToString());

            Session.Language = ConfigurationManager.AppSettings["Language"].ToString().ToUpper().Trim();
            if (Session.Language.Equals("IT"))
            {
                rbLanguageIT.Checked = true;
                rbLanguageEN.Checked = false;
            }
            else
            {
                rbLanguageIT.Checked = false;
                rbLanguageEN.Checked = true;
            }
        }

        #region Tab: Create Layers

        private bool CreateLayer()
        {
            switch (Session.Provider)
            {
                case Provider.SqlServer: return new CreateClassSqlServer().BuildTemplate(GetDatatableSqlServer.GetSchema(txtFilterSearch.Text.Trim()), connectionSqlServer, Session.Modality);
                case Provider.SqlBase:
                case Provider.SqlBase2:
                    bool isValidSqlBase = true;
                    GetDatatableSqlBase.IndexKeys = new Dictionary<string, int>();
                    foreach (string tableName in chlTable.CheckedItems)
                    {                       
                        if (!new CreateClassSqlBase().BuildTemplate(GetDatatableSqlBase.GetSchema(tableName), Session.Provider))
                            isValidSqlBase = false;
                    }

                    if (Session.Provider == Provider.SqlBase2)
                    {
                        CreateClassSqlBase ccsb = new CreateClassSqlBase();
                        ccsb.BuildDbShared();
                        ccsb.BuildSqlHelperGupta();
                        ccsb.BuildNull();
                        ccsb.BuildDataHelper();
                    }
                    return isValidSqlBase;
                case Provider.MySql: return false;
                case Provider.Access:
                    bool isValidAccess = true;
                    GetDatatableAccess.IndexKeys = new Dictionary<string, int>();
                    foreach (string tableName in chlTable.CheckedItems)
                    {
                        if (!new CreateClassAccess().BuildTemplate(GetDatatableAccess.GetSchema(tableName, string.Empty), Session.Provider))
                            isValidAccess = false;
                    }
                    return isValidAccess;
                case Provider.Access2:
                    bool isValidAccess2 = true;
                    GetDatatableAccess.IndexKeys = new Dictionary<string, int>();
                    foreach (string tableName in chlTable.CheckedItems)
                    {
                        if (!new CreateClassAccess().BuildTemplate(GetDatatableAccess.GetSchema(tableName, string.Empty), Session.Provider))
                            isValidAccess2 = false;
                    }
                    return isValidAccess2;
                case Provider.PostGres:
                    bool isValidPostGres = true;
                    foreach (string tableName in chlTable.CheckedItems)
                    {
                        if (!new CreateClassPostGres().BuildTemplate(GetDatatablePostGres.GetSchema(tableName, string.Empty), Session.Provider))
                            isValidPostGres = false;
                    }
                    return isValidPostGres;
                    break;
                case Provider.AutoTask:
                    bool isValidAutoTask = true;
                    foreach (string tableName in chlTable.CheckedItems)
                    {
                        if (!new CreateClassAutoTask().BuildTemplate(GetDatatableAutoTask.GetSchema(tableName, string.Empty), Session.Provider))
                            isValidAutoTask = false;
                    }
                    return isValidAutoTask;
                    break;
                default: return true;
            }
        }
        
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (TestTable())
            {
                this.Cursor = Cursors.WaitCursor;

                Session.CopyProject = chkCopyClass.Checked;

                if (CreateLayer())
                {
                    MessageBox.Show(MSG.Message_CreateDataLayer(), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //btnResult.Image = global::ModelCreator.Properties.Resources.check_24;
                    //btnResult.BackColor = System.Drawing.Color.RoyalBlue;
                    //btnResult.Text = "Build and Write Model successfull";

                    btnOpenFolder.Enabled = true;
                }
                else
                {
                    MessageBox.Show(MSG.Message_GeneralERROR(), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //btnResult.Image = global::ModelCreator.Properties.Resources.error_24;
                    //btnResult.BackColor = System.Drawing.Color.Red;
                    //btnResult.Text = "Build and Write Model error";
                }

                this.Cursor = Cursors.Default;
            }
            else MessageBox.Show("No tables selected for creation", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void PutListTables(ArrayList tables)
        {
            switch (Session.Provider)
            {
                case Provider.SqlServer: foreach (DataTable dt in tables) chlTable.Items.Add(dt.Rows[0]["Table"].ToString()); break;
                case Provider.MySql:
                case Provider.SqlBase:
                case Provider.SqlBase2: foreach (string s in tables) chlTable.Items.Add(s); break;
                case Provider.Access: foreach (string s in tables) chlTable.Items.Add(s); break;
                case Provider.Access2: foreach (string s in tables) chlTable.Items.Add(s); break;
                case Provider.PostGres: foreach (string s in tables) chlTable.Items.Add(s); break;
                case Provider.AutoTask: foreach (string s in tables) chlTable.Items.Add(s); break;
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = string.Format(@"/n,/e,{0}", Session.Folder);
            process.Start();
        }

        private bool TestTable()
        {
            if (chlTable.CheckedItems.Count > 0)
            {
                //TODO MESSAGE
                //btnResult.Text = string.Empty;
                //btnResult.BackColor = colorBase;
                //btnResult.Image = null;
                //btnResultScript.Text = string.Empty;
                //btnResultScript.BackColor = colorBase;
                //btnResultScript.Image = null;
                return true;
            }
            else
            {
                //TODO MESSAGE
                //btnResult.Text = "  No tables selected for creation";
                //btnResult.BackColor = System.Drawing.Color.Orange;
                //btnResult.Image = global::ModelCreator.Properties.Resources.warning_24;
                //btnResultScript.Text = "  No tables selected for creation";
                //btnResultScript.BackColor = System.Drawing.Color.Orange;
                //btnResultScript.Image = global::ModelCreator.Properties.Resources.warning_24;
                return false;
            }
        }

        private void btnTableListCheckedAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chlTable.Items.Count; i++)
                chlTable.SetItemChecked(i, true);
            lblNumberTable.Text = string.Format(MSG.DataLayer_Calculate_TableSelected(), chlTable.CheckedIndices.Count, chlTable.Items.Count);
            btnCreate.Enabled = true;
        }

        private void btnTableListUnCheckedAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chlTable.Items.Count; i++)
                chlTable.SetItemChecked(i, false);
            lblNumberTable.Text = string.Format(MSG.DataLayer_Calculate_TableSelected(), chlTable.CheckedIndices.Count, chlTable.Items.Count);
            btnCreate.Enabled = false;
        }

        private void chlTable_SelectedValueChanged(object sender, EventArgs e)
        {
            TestTable();
            lblNumberTable.Text = string.Format(MSG.DataLayer_Calculate_TableSelected(), chlTable.CheckedIndices.Count, chlTable.Items.Count);
            btnCreate.Enabled = true;
        }

        #region Presentation

        // CREATE CONTROLS
        private void btnCreateControlPage_Click(object sender, EventArgs e)
        {
            ArrayList tables = GetDatatableSqlServer.GetSchema(txtFilterSearch.Text.Trim());
            GetWebControls getWebControls = new GetWebControls();
            connectionSqlServer = new SqlConnection(Session.ConnectionString);

            if (getWebControls.BuildControls(Session.NamespaceDataLayer, tables, Session.Folder, connectionSqlServer))
            {
                //TODO MESSAGE
                //btnResult.BackColor = System.Drawing.Color.RoyalBlue;
                //btnResult.Text = "Build and Write Web Control successfull";
            }
            else
            {
                //TODO MESSAGE
                //btnResult.BackColor = System.Drawing.Color.Red;
                //btnResult.Text = "Build and Write Web Control error";
            }
        }

        private SQLBaseDataReader GetReaderSqlBase(string tableName)
        {
            string sqlDatabase = string.Format("SELECT TBNAME,NAME,COLTYPE,LENGTH,NULLS FROM SYSCOLUMNS WHERE TBNAME = '{0}' ORDER BY TBNAME", tableName);

            SQLBaseConnection connection = null;
            try
            {
                connection = new SQLBaseConnection(Session.ConnectionString);
                if (connection == null)
                    return null;
                SQLBaseDataReader dr = SqlHelperSqlBase.ExecuteReader(connection, CommandType.Text, sqlDatabase);
                return dr;

            }
            catch
            {
                return null;
            }
        }

        #endregion

        #endregion

        #region Tab: Database

        private void btnGenerateSqlScript_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            Session.AuthorName = txtAuthorName.Text;
            SaveSession();

            bool result = false;
            string script = string.Empty;

            try
            {
                if (TestTable())
                {
                    foreach (object item in chlTable.CheckedItems)
                    {
                        string table = item.ToString();
                        CreateClassSqlServer.SetRealTableKey(table);

                        if (!table.Equals("sysdiagrams"))
                        {
                            FieldCollection collection = GetInfoFieldSqlServer.GetFields(table);

                            if (cbView.Checked) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.View, collection, table));
                            if (cbView.Checked) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.ViewPlus, collection, table));
                            if (bool.Parse(dgvNames.Rows[0].Cells[0].Value.ToString())) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.Insert, collection, table));
                            if (bool.Parse(dgvNames.Rows[2].Cells[0].Value.ToString())) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.Clone, collection, table));
                            if (bool.Parse(dgvNames.Rows[2].Cells[0].Value.ToString())) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.Update, collection, table));
                            if (bool.Parse(dgvNames.Rows[3].Cells[0].Value.ToString())) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.Delete, collection, table));
                            if (bool.Parse(dgvNames.Rows[4].Cells[0].Value.ToString())) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.GetCollection, collection, table));
                            if (Session.Keyuni)
                                if (bool.Parse(dgvNames.Rows[6].Cells[0].Value.ToString())) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.GetByKeyuni, collection, table));
                            if (bool.Parse(dgvNames.Rows[5].Cells[0].Value.ToString())) script = string.Concat(script, new StoredProcedureSqlServer().Generate(StoredProcedureSqlServer.StoredProcedureTypes.GetByCode, collection, table));
                        }
                    }
                }
                else MessageBox.Show("No tables selected for creation", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                #region WriteFile

                StreamWriter writer;

                string pathFile = Session.SqlScriptFile;

                if (File.Exists(pathFile)) File.Delete(pathFile);

                writer = File.AppendText(pathFile);
                writer.WriteLine(script);
                writer.Close();

                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally { }

            this.Cursor = Cursors.Default;

            if (result)
            {
                btnResultScript.Image = global::ModelCreator.Properties.Resources.check_24;
                btnResultScript.BackColor = System.Drawing.Color.RoyalBlue;
                btnResultScript.Text = "  SQL Script write successfull";
            }
            else
            {
                btnResultScript.Image = global::ModelCreator.Properties.Resources.error_24;
                btnResultScript.BackColor = System.Drawing.Color.Red;
                btnResultScript.Text = "  SQL Script write error";
            }
        }

        private void btnUnChecked_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvNames.Rows.Count; i++)
                dgvNames.Rows[i].Cells[0].Value = false;
        }

        private void btnChecked_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvNames.Rows.Count; i++)
                dgvNames.Rows[i].Cells[0].Value = true;
        }

        private void chkComment_CheckedChanged(object sender, EventArgs e)
        {
            txtAuthorName.Enabled = chkComment.Checked;
            Session.Comment = chkComment.Checked;

            if (chkComment.Checked)
                txtAuthorName.Text = Session.AuthorName;
            else txtAuthorName.Text = string.Empty;
        }

        private void chkIsUnknown_CheckedChanged(object sender, EventArgs e)
        {
            txtIsUnknown.Enabled = chkIsUnknown.Checked;

            if (chkIsUnknown.Checked)
                txtIsUnknown.Text = Session.IsUnknown;
            else txtIsUnknown.Text = string.Empty;
        }

        private void cbView_CheckedChanged(object sender, EventArgs e)
        {
            txtView.Enabled = cbView.Checked;

            if (cbView.Checked)
                txtView.Text = Session.View;
            else txtView.Text = string.Empty;
        }

        private void btnSaveInformationDatabase_Click(object sender, EventArgs e)
        {
            Session.InsertSql = dgvNames.Rows[0].Cells[2].Value.ToString();
            Session.InsertFunction = dgvNames.Rows[0].Cells[3].Value.ToString();
            Session.ReadFunction = dgvNames.Rows[1].Cells[3].Value.ToString();
            Session.UpdateSql = dgvNames.Rows[2].Cells[2].Value.ToString();
            Session.UpdateFunction = dgvNames.Rows[2].Cells[3].Value.ToString();
            Session.DeleteSql = dgvNames.Rows[3].Cells[2].Value.ToString();
            Session.DeleteFunction = dgvNames.Rows[3].Cells[3].Value.ToString();

            Session.GetCollectionSql = dgvNames.Rows[4].Cells[2].Value.ToString();
            Session.GetCollectionFunction = dgvNames.Rows[4].Cells[3].Value.ToString();
            Session.GetByCodeSql = dgvNames.Rows[5].Cells[2].Value.ToString();
            Session.GetByCodeFunction = dgvNames.Rows[5].Cells[3].Value.ToString();
            Session.GetByKeyuniSql = dgvNames.Rows[6].Cells[2].Value.ToString();
            Session.GetByKeyuniFunction = dgvNames.Rows[6].Cells[3].Value.ToString();

            Session.GetCustomItemSql = dgvNames.Rows[7].Cells[2].Value.ToString();
            Session.GetCustomItemFunction = dgvNames.Rows[7].Cells[3].Value.ToString();
            Session.GetCustomCollectionSql = dgvNames.Rows[8].Cells[2].Value.ToString();
            Session.GetCustomCollectionFunction = dgvNames.Rows[8].Cells[3].Value.ToString();
        }

        #endregion

        #region Tab: Settings

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            fbdModelCreator.ShowDialog();
            txtFolder.Text = fbdModelCreator.SelectedPath;
        }

        private void btnChooseSqlScriptFile_Click(object sender, EventArgs e)
        {
            fbdModelCreator.ShowDialog();
            txtSqlScriptFile.Text = fbdModelCreator.SelectedPath;
        }

        private void rbProfessional_CheckedChanged(object sender, EventArgs e)
        {
            if (rbProfessional.Checked) Session.Modality = Modality.Professional;
        }

        private void rbSimple_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSimple.Checked) Session.Modality = Modality.Easy;
        }

        private void rbThreeLayer_CheckedChanged(object sender, EventArgs e)
        {
            if (rbThreeLayer.Checked) Session.Modality = Modality.ThreeLayer;
        }

        private void rbFramework_CheckedChanged(object sender, EventArgs e)
        {
            if (rbFramework.Checked) Session.GuidCreator = "Framework";
        }

        private void rbDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (rbFramework.Checked) Session.GuidCreator = "Database";
        }

        private void btnSaveNameSpace_Click(object sender, EventArgs e)
        {
            Session.NamespaceDataLayer = txtNameSpaceDataLayer.Text.Trim();
            Session.NamespaceBusinessLayer = txtNameSpaceBusinessLayer.Text.Trim();
        }

        private void btnSaveSetting_Click(object sender, EventArgs e)
        {
            if (rbTableCode.Checked) Session.Code = txtOtherCode.Text.Trim();
            Session.NamespaceDataLayer = txtNameSpaceDataLayer.Text.Trim();
            Session.NamespaceBusinessLayer = txtNameSpaceBusinessLayer.Text.Trim();
            Session.Folder = txtFolder.Text.Trim();
            Session.SqlScriptFile = txtSqlScriptFile.Text.Trim();

            SaveSession();
            MessageBox.Show("Saved sucessfully", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void chkTimespan_CheckedChanged(object sender, EventArgs e)
        {
            Session.Timespan = chkTimespan.Checked;
        }

        private void chkKeyuni_CheckedChanged(object sender, EventArgs e)
        {
            Session.Keyuni = chkKeyuni.Checked;
            gbGuid.Enabled = Session.Keyuni;

            if (Session.Keyuni)
            {
                dgvNames.Rows[6].Cells[0].Value = true;
                dgvNames.Rows[6].Cells[0].ReadOnly = false;
                dgvNames.Rows[6].Cells[2].Style.Font = new Font("Verdana", 8, FontStyle.Regular);
                dgvNames.Rows[6].Cells[3].Style.Font = new Font("Verdana", 8, FontStyle.Regular);
                dgvNames.Rows[6].Cells[2].ReadOnly = false;
                dgvNames.Rows[6].Cells[3].ReadOnly = false;
            }
            else
            {
                rbReadKeyuni.Checked = false;
                rbReadTableCode.Checked = true;

                dgvNames.Rows[6].Cells[0].Value = false;
                dgvNames.Rows[6].Cells[0].ReadOnly = true;
                dgvNames.Rows[6].Cells[2].Style.Font = new Font("Verdana", 8, FontStyle.Strikeout);
                dgvNames.Rows[6].Cells[3].Style.Font = new Font("Verdana", 8, FontStyle.Strikeout);
                dgvNames.Rows[6].Cells[2].ReadOnly = true;
                dgvNames.Rows[6].Cells[3].ReadOnly = true;
            }
        }

        private void rbTableCode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTableCode.Checked)
            {
                Session.Code = "TableCode";
                txtOtherCode.Text = string.Empty;
                txtOtherCode.Enabled = false;
            }
        }

        private void rbIdCode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbIdCode.Checked)
            {
                Session.Code = "ID";
                txtOtherCode.Text = string.Empty;
                txtOtherCode.Enabled = false;
            }
        }

        private void rbOtherCode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbOtherCode.Checked)
            {
                txtOtherCode.Enabled = true;
                Session.Code = txtOtherCode.Text.Trim();
            }
        }

        private void rbFramework20_CheckedChanged(object sender, EventArgs e)
        {
            Session.Framework = Framework.net20;
        }

        private void rbFramework35_CheckedChanged(object sender, EventArgs e)
        {
            Session.Framework = Framework.net35;
        }

        private void rbColorGrey_CheckedChanged(object sender, EventArgs e)
        {
            Session.Color = "Grey";
            RefreshColor(false);
        }

        private void rbColorOrange_CheckedChanged(object sender, EventArgs e)
        {
            Session.Color = "Orange";
            RefreshColor(false);
        }

        private void rbColorBlue_CheckedChanged(object sender, EventArgs e)
        {
            Session.Color = "Blue";
            RefreshColor(false);
        }

        private void rbGreen_CheckedChanged(object sender, EventArgs e)
        {
            Session.Color = "Green";
            RefreshColor(false);
        }

        private void SaveSession()
        { }

        private void RefreshColor(bool refresh)
        {
            string colorCode = string.Empty;

            switch (Session.Color)
            {
                case "Grey": colorCode = "212-208-200"; if (refresh) rbColorGrey.Checked = true; break;
                case "Orange": colorCode = "255-224-192"; if (refresh) rbColorOrange.Checked = true; break;
                case "Blue": colorCode = "176-196-222"; if (refresh) rbColorBlue.Checked = true; break;
                case "Green": colorCode = "143-188-139"; if (refresh) rbColorBlue.Checked = true; break;
            }

            string[] Color = colorCode.Split('-');
            colorBase = System.Drawing.Color.FromArgb(Int32.Parse(Color[0]), Int32.Parse(Color[1]), Int32.Parse(Color[2]));
            this.BackColor = colorBase;
            tpDatabase.BackColor = colorBase;
            tpDataLayer.BackColor = colorBase;
            tpSettings.BackColor = colorBase;
            tpConnection.BackColor = colorBase;
        }

        private void rbProviderSqlBase_CheckedChanged(object sender, EventArgs e)
        {
            DisableDatabaseFunction(false);
        }

        private void rbProviderSqlServer_CheckedChanged(object sender, EventArgs e)
        {
            DisableDatabaseFunction(true);
        }

        private void rbMySql_CheckedChanged(object sender, EventArgs e)
        {
            DisableDatabaseFunction(false);
        }

        private void rbManageKeyDatabase_CheckedChanged(object sender, EventArgs e)
        {
            DisableDatabaseFunctionKey(false);
        }

        private void rbManageKeysCustom_CheckedChanged(object sender, EventArgs e)
        {
            DisableDatabaseFunctionKey(true);
        }

        private void DisableDatabaseFunction(bool isValid)
        {
            gbModality.Enabled = isValid;
            gbSqlScriptFile.Enabled = isValid;
            gbBuildConnectionString.Enabled = isValid;
            cbView.Enabled = isValid;
            txtView.Enabled = isValid;
            chkIsUnknown.Enabled = isValid;
            txtIsUnknown.Enabled = isValid;
            gbStoredProcedure.Enabled = isValid;
            btnGenerateSqlScript.Enabled = isValid;
            DisableDatabaseFunctionKey(isValid);
        }

        private void DisableDatabaseFunctionKey(bool isValid)
        {
            gbMainTableKey.Enabled = isValid;
            gbSpecialFields.Enabled = isValid;
            gbReadingFunction.Enabled = isValid;
            gbGuid.Enabled = isValid;
        }

        #endregion

        #region Tab: Connection

        private bool TestConnection()
        {
            btnConnectionTest.Text = string.Empty;
           
            try
            {
                switch (Session.Provider)
                {
                    case Provider.SqlServer: connectionSqlServer = new SqlConnection(txtConnectionString.Text); connectionSqlServer.Open(); break;
                    case Provider.SqlBase2:
                    case Provider.SqlBase: connectionSqlBase = new SQLBaseConnection(txtConnectionString.Text); connectionSqlBase.Open(); break;
                    case Provider.MySql: break;
                    case Provider.Access: connectionAccess = new OleDbConnection(txtConnectionString.Text); connectionAccess.Open(); break;
                    case Provider.Access2: connectionAccess = new OleDbConnection(txtConnectionString.Text); connectionAccess.Open(); break;
                    case Provider.PostGres: connectionPostGres = new PgSqlConnection(txtConnectionString.Text); connectionPostGres.Open(); break;
                    case Provider.Oracle: break;

                }

                btnConnectionTest.Image = global::ModelCreator.Properties.Resources.text_ok_24;
                btnConnectionTest.BackColor = System.Drawing.Color.RoyalBlue;
                btnConnectionTest.Text = "  ConnectionString Test Passed";
                return true;
            }
            catch (Exception ex)
            {
                btnConnectionTest.Image = global::ModelCreator.Properties.Resources.error_24;
                btnConnectionTest.BackColor = System.Drawing.Color.Red;
                btnConnectionTest.Text = "  ConnectionString Test Not Passed";
                return false;
            }
            finally
            {
                switch (Session.Provider)
                {
                    case Provider.SqlServer: if (connectionSqlServer.State != ConnectionState.Closed) connectionSqlServer.Close(); break;
                    case Provider.SqlBase2:
                    case Provider.SqlBase: if (connectionSqlBase.State != ConnectionState.Closed) connectionSqlBase.Close(); break;
                    case Provider.MySql: break;
                    case Provider.Access: if (connectionSqlBase.State != ConnectionState.Closed) connectionAccess.Close(); break;
                    case Provider.Access2: if (connectionSqlBase.State != ConnectionState.Closed) connectionAccess.Close(); break;
                    case Provider.PostGres: if (connectionPostGres.State != ConnectionState.Closed) connectionPostGres.Close(); break;
                    case Provider.Oracle: break;
                }
            }
        }

        private void btnBuildConnectionString_Click(object sender, EventArgs e)
        {
            switch (Session.Provider)
            {
                case Provider.SqlServer: txtConnectionString.Text = string.Format("data source={0};initial catalog={1};integrated security=false;persist security info=True;User ID={2};Password={3}", txtServer.Text, txtDataBase.Text, txtUser.Text, txtPassword.Text); break;
                case Provider.MySql:
                case Provider.SqlBase2:
                case Provider.SqlBase: txtConnectionString.Text = Session.ConnectionString; break;
                case Provider.Access: break;
                case Provider.Access2: break;
                case Provider.PostGres: break;
                case Provider.Oracle: break;
            }

            if (TestConnection())
            {
                ArrayList tables = GetDatatableSqlServer.GetSchema(txtFilterSearch.Text.Trim());
                PutListTables(tables);
            }
        }

        private void btnSaveConnectionString_Click(object sender, EventArgs e)
        {
            Session.ConnectionString = txtConnectionString.Text;

            SaveSession();
            MessageBox.Show("Saved sucessfully", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private void tbMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbMain.SelectedIndex == 0) Load();
            TestTable();

        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            Load();
        }

        private void btnXml_Click(object sender, EventArgs e)
        {
            StreamReader reader = File.OpenText(string.Concat(Application.StartupPath.Replace("bin\\Debug", string.Empty), "\\ListClass.xml"));
            string input = null;
            Dictionary<string, string> list = new Dictionary<string, string>();
            while ((input = reader.ReadLine()) != null)
            {
                if (input.Trim().StartsWith("<Table>"))
                {
                    string table = input.Replace("<Table>", string.Empty);
                    table = table.Replace("</Table>", string.Empty);
                    table = table.Trim();
                    list.Add(table, table);
                }
            }

            for (int i = 0; i < chlTable.Items.Count; i++)
            {
                if (list.ContainsKey(chlTable.Items[i].ToString()))
                    chlTable.SetItemChecked(i, true);
            }
        }

        private void btnCheckStructure_Click(object sender, EventArgs e)
        {
            ArrayList errors = new ArrayList();
            Dictionary<string, string> tables = new Dictionary<string, string>();
            Dictionary<string, string> views = new Dictionary<string, string>();
            DataTable dtViewBase = GetDatatableSqlServer.GetView();
            DataTable dtDataTableBase = GetDatatableSqlServer.GetTable();

            DataTable dtView = GetDatatableSqlServer.GetDatatabaseItem();

            foreach (DataRow drTableBase in dtDataTableBase.Rows)
                tables.Add(drTableBase[0].ToString(), drTableBase[0].ToString());

            //foreach (DataRow drViewBase in dtViewBase.Rows)            
            //    views.Add(drViewBase[0].ToString(), dr[0].ToString());

            foreach (DataRow drViewBase in dtViewBase.Rows)
            {
                string name = drViewBase[0].ToString().Trim();
                if (name.StartsWith("view"))
                {
                    string table = name.Substring(4).Trim();
                    if (table.EndsWith("Plus"))
                        table = table.Substring(0, table.Length - 4);

                    if (tables.ContainsKey(table))
                    {
                        DataRow row = dtView.NewRow();
                        row[0] = name;
                        row[1] = table;
                        dtView.Rows.Add(row);
                        //views.Add(name, table);
                    }
                    else errors.Add(string.Format("La vista {0} non è mappata su nessuna tabella", name));
                }
            }

            DataView dv = new DataView(dtView);
            dv.Sort = "Table,Name";
            dtView = GetDatatableSqlServer.ConvertDataViewInDataTable(dv);

            bool isPlus = false;
            string viewNamePre = string.Empty;
            foreach (DataRow drView in dtView.Rows)
            {
                string viewName = drView[0].ToString().Trim();

                if (viewName.StartsWith("view"))
                {
                    viewName = viewName.Substring(4).Trim();

                    if (viewName.EndsWith("Plus"))
                    {
                        if (!isPlus)
                            errors.Add(string.Concat("Manca la versione normale della vista : ", drView[0].ToString().Trim()));
                        isPlus = false;
                        viewNamePre = drView[0].ToString().Trim();
                    }
                    else
                    {
                        if (isPlus)
                            errors.Add(string.Concat("Manca la versione Plus della vista : ", viewNamePre));
                        isPlus = true;
                        viewNamePre = drView[0].ToString().Trim();
                    }
                }
            }

            foreach (KeyValuePair<string, string> kpv in tables)
            {
                isPlus = false;
                bool isFindNormal = false;
                bool isFindPlus = false;
                foreach (DataRow drView in dtView.Rows)
                {
                    string viewName = drView[0].ToString().Trim();

                    if (viewName.StartsWith("view"))
                    {
                        viewName = viewName.Substring(4).Trim();

                        if (viewName.EndsWith("Plus"))
                        {
                            viewName = viewName.Substring(0, viewName.Length - 4);

                            if (kpv.Value.Equals(viewName))
                                isFindPlus = true;

                        }
                        else
                        {
                            if (kpv.Value.Equals(viewName))
                                isFindNormal = true;
                        }
                    }
                }
                if (!isFindNormal) errors.Add(string.Format("Manca la vista della tabella {0}", kpv.Value));
                if (!isFindPlus) errors.Add(string.Format("Manca la vista Plus della tabella {0}", kpv.Value));

                foreach (DataRow drView in dtView.Rows)
                {
                    DataTable dtTest = new DataTable();
                    if (!GetDatatableSqlServer.TestView(drView[0].ToString().Trim()).Equals(string.Empty))
                        errors.Add(string.Format("La vista {0} è mal formattata", drView[0].ToString()));
                }

                //STORED PROCEDURE

                DataTable dtStoredInsert = GetDatatableSqlServer.GetStoredProcedure("Insert");
                DataTable dtStoredUpdate = GetDatatableSqlServer.GetStoredProcedure("Update");
                DataTable dtStoredDelete = GetDatatableSqlServer.GetStoredProcedure("Delete");
                DataTable dtStoredGetByCode = GetDatatableSqlServer.GetStoredProcedure("GetByCode");
                DataTable dtStoredGetCollection = GetDatatableSqlServer.GetStoredProcedure("GetCollection");
                DataTable dtStoredGetCollection_ = GetDatatableSqlServer.GetStoredProcedure("GetCollection_");
                DataTable dtStoredGetItem_ = GetDatatableSqlServer.GetStoredProcedure("GetItem_");

                // FROM STORED TO TABLE
                foreach (DataRow drInsert in dtStoredInsert.Rows)
                {
                    string name = drInsert[0].ToString().Trim().Replace("Insert", string.Empty);
                    if (!tables.ContainsKey(name))
                        errors.Add(string.Format("La stored prodedure {0} non è collegata a nessuna tabella", drInsert[0].ToString()));
                }

                foreach (DataRow drUpdate in dtStoredUpdate.Rows)
                {
                    string name = drUpdate[0].ToString().Trim().Replace("Update", string.Empty);
                    if (!tables.ContainsKey(name))
                        errors.Add(string.Format("La stored prodedure {0} non è collegata a nessuna tabella", drUpdate[0].ToString()));
                }

                foreach (DataRow drDelete in dtStoredDelete.Rows)
                {
                    string name = drDelete[0].ToString().Trim().Replace("Delete", string.Empty);
                    if (!tables.ContainsKey(name))
                        errors.Add(string.Format("La stored prodedure {0} non è collegata a nessuna tabella", drDelete[0].ToString()));
                }

                foreach (DataRow drGetByCode in dtStoredGetByCode.Rows)
                {
                    string name = drGetByCode[0].ToString().Trim().Replace("GetByCode", string.Empty);
                    if (!tables.ContainsKey(name))
                        errors.Add(string.Format("La stored prodedure {0} non è collegata a nessuna tabella", drGetByCode[0].ToString()));
                }

                int collectionBase = 0;
                foreach (DataRow drCollection in dtStoredGetCollection.Rows)
                {
                    string name = drCollection[0].ToString().Trim();

                    if (name.StartsWith("GetCollection_"))
                    {
                        name = name.Replace("GetCollection_", string.Empty);
                        name = name.Substring(0, name.IndexOf("_"));

                        if (!tables.ContainsKey(name))
                            errors.Add(string.Format("La stored prodedure {0} non è collegata a nessuna tabella", drCollection[0].ToString()));
                    }
                    else
                    {
                        collectionBase++;
                        name = name.Replace("GetCollection", string.Empty);
                        if (!tables.ContainsKey(name))
                            errors.Add(string.Format("La stored prodedure {0} non è collegata a nessuna tabella", drCollection[0].ToString()));
                    }
                }

                foreach (DataRow drItem in dtStoredGetItem_.Rows)
                {
                    string name = drItem[0].ToString().Trim();
                    name = name.Replace("GetItem_", string.Empty);
                    name = name.Substring(0, name.IndexOf("_"));

                    if (!tables.ContainsKey(name))
                        errors.Add(string.Format("La stored prodedure {0} non è collegata a nessuna tabella", drItem[0].ToString()));
                }

                // FROM TABLE TO STORED
                if (dtStoredInsert.Rows.Count != tables.Count)
                    errors.Add(string.Format("Le stored prodedure di Insert sono di un numero differente ({0}) rispetto a quanto ipotizzato ({1})", dtStoredInsert.Rows.Count, tables.Count));

                if (dtStoredUpdate.Rows.Count != tables.Count)
                    errors.Add(string.Format("Le stored prodedure di Update sono di un numero differente ({0}) rispetto a quanto ipotizzato ({1})", dtStoredUpdate.Rows.Count, tables.Count));

                if (dtStoredDelete.Rows.Count != tables.Count)
                    errors.Add(string.Format("Le stored prodedure di Delete sono di un numero differente ({0}) rispetto a quanto ipotizzato ({1})", dtStoredDelete.Rows.Count, tables.Count));

                if (collectionBase != tables.Count)
                    errors.Add(string.Format("Le stored prodedure di GetCollection sono di un numero differente ({0}) rispetto a quanto ipotizzato ({1})", collectionBase, tables.Count));

                if (dtStoredGetByCode.Rows.Count != tables.Count)
                    errors.Add(string.Format("Le stored prodedure di GetByCode sono di un numero differente ({0}) rispetto a quanto ipotizzato ({1})", dtStoredGetByCode.Rows.Count, tables.Count));

            }
        }

        private void btnClearStored_Click(object sender, EventArgs e)
        {
            ArrayList listWaste = new ArrayList();
            Dictionary<string, string> objects = new Dictionary<string, string>();

            for (int i = 0; i < chlTable.Items.Count; i++)
                objects.Add(chlTable.Items[i].ToString(), chlTable.Items[i].ToString());

            ArrayList listStored = GetStoredProceduresSqlServer.GetReaderAllObjects(new SqlConnection(Session.ConnectionString));
            ArrayList list = new ArrayList();

            foreach (string s in listStored)
            {
                if ((s.StartsWith("Clone")) || (s.StartsWith("Delete")) || (s.StartsWith("GetByCode")) || (s.StartsWith("GetCollection")) || (s.StartsWith("Insert")) || (s.StartsWith("Update")) || (s.StartsWith("GetItem_")) || (s.StartsWith("GetCollection_") || (s.StartsWith("view"))))
                {
                    bool custom = false;

                    if (s.StartsWith("GetItem_"))
                    {
                        string t = s.Replace("GetItem_", string.Empty);
                        t = t.Substring(0, t.IndexOf('_')).Trim();
                        if (!objects.ContainsKey(t))
                            list.Add(s);
                        custom = true;
                    }

                    if (s.StartsWith("GetCollection_"))
                    {
                        string t = s.Replace("GetCollection_", string.Empty);
                        t = t.Substring(0, t.IndexOf('_')).Trim();
                        if (!objects.ContainsKey(t))
                            list.Add(s);
                        custom = true;
                    }

                    if (!custom)
                    {
                        if (s.StartsWith("Clone"))
                        {
                            string t = s.Replace("Clone", string.Empty);
                            if (!objects.ContainsKey(t))
                                list.Add(s);
                        }

                        if (s.StartsWith("Delete"))
                        {
                            string t = s.Replace("Delete", string.Empty);
                            if (!objects.ContainsKey(t))
                                list.Add(s);
                        }

                        if (s.StartsWith("GetByCode"))
                        {
                            string t = s.Replace("GetByCode", string.Empty);
                            if (!objects.ContainsKey(t))
                                list.Add(s);
                        }

                        if (s.StartsWith("GetCollection"))
                        {
                            string t = s.Replace("GetCollection", string.Empty);
                            if (!objects.ContainsKey(t))
                                list.Add(s);
                        }

                        if (s.StartsWith("Insert"))
                        {
                            string t = s.Replace("Insert", string.Empty);
                            if (!objects.ContainsKey(t))
                                list.Add(s);
                        }

                        if (s.StartsWith("Update"))
                        {
                            string t = s.Replace("Update", string.Empty);
                            if (!objects.ContainsKey(t))
                                list.Add(s);
                        }

                        if (s.StartsWith("view"))
                        {
                            string t = s.Replace("view", string.Empty);

                            if (s.Contains("Plus"))
                            {
                                t = t.Replace("Plus", string.Empty);
                                if (!objects.ContainsKey(t.Trim()))
                                    list.Add(s);
                            }
                            else
                            {
                                if (!objects.ContainsKey(t.Trim()))
                                    list.Add(s);
                            }
                        }
                    }
                }
                else list.Add(s);
            }

            if (list.Count > 0)
            {
                string error = string.Empty;
                for (int i = 0; i < list.Count; i++)
                {
                    error = string.Concat(error, list[i].ToString(), " - ");
                }
                error = error.Substring(0, error.Length - 2).Trim();

                MessageBox.Show(error, "OGGETTI NON COERENTI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else MessageBox.Show("TUTTI GLI OGGETTI SONO COERENTI", "OGGETTI COERENTI", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void rbLanguageEN_CheckedChanged(object sender, EventArgs e)
        {
            SetLanguage("EN");
        }

        private void rbLanguageIT_CheckedChanged(object sender, EventArgs e)
        {
            SetLanguage("IT");
        }

        private void DATALAYER_Click(object sender, EventArgs e)
        {

        }

       
           
    }
}