using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace ModelCreator
{
    public class CreateClassSqlServer
    {
        public DataTable table = new DataTable();
        private string keyCode = string.Empty;

        public CreateClassSqlServer()
        {
        }

        public bool BuildTemplate(ArrayList tables, SqlConnection connection, Modality modality)
        {
            if (modality == Modality.Easy)
                return GetTemplateEasy(tables, connection);
            if (modality == Modality.Professional)
                return GetTemplateProfessional(tables, connection);
            if (modality == Modality.ThreeLayer)
                return GetTemplateThreeLayer(tables, connection);

            return false;
        }

        #region Easy Mode

        private bool GetTemplateEasy(ArrayList tables, SqlConnection connection)
        {
            bool result = false;

            if (tables != null)
            {
                foreach (DataTable dt in tables)
                {
                    string tableName = dt.Rows[0]["Table"].ToString().Trim();
                    SetRealTableKey(tableName);
                    Session.Keys = GetInfoFieldSqlServer.GetKeysDictionary(tableName);

                    if (!tableName.Equals("sysdiagrams"))
                    {
                        string provider = "ProviderHelper.ConnectionString";
                        if (connection.ConnectionString.Contains("Counter"))
                            provider = "ProviderHelper.ConnectionStringCounter";
                        string privateField = string.Empty;
                        if (Session.Framework == Framework.net20)
                            privateField = GetPrivateField(dt);
                        string publicField = GetPublicField(dt);
                        string mapping = GetMapping(dt);
                        string parameters = GetParametersEasy(dt, tableName);
                        string parametersKey = GetParametersKey(dt, tableName, true);
                        string parametersKeyForCode = GetParametersKey(dt, tableName, false);
                        string parametersList = string.Empty;
                        string parametersListInsert = string.Empty;
                        string parametersListUpdate = string.Empty;
                        string parametersListDelete = string.Empty;
                        string parametersListKey = string.Empty;
                        string fieldLog = string.Empty;

                        if (Session.Keyuni)
                            parametersList = GetParametersListEasy(dt, tableName);
                        else
                        {
                            parametersListInsert = GetParametersListEasy(dt, tableName, "Insert");
                            parametersListUpdate = GetParametersListEasy(dt, tableName, "Update");
                            parametersListDelete = GetParametersListEasy(dt, tableName, "Delete");
                        }
                        parametersListKey = GetParametersListKey(dt, tableName);

                        fieldLog = GetLogSelect(dt, tableName);

                        string storedProcedures = GetStoredProceduresSqlServer.GetStored(connection, tableName, string.Empty); ;

                        string template = string.Empty;

                        if (Session.Keyuni && Session.Timespan)
                            template = ManageTemplate.GetTemplate("CrudTemplate.ico");
                        else
                        {
                            if (!Session.Keyuni && !Session.Timespan)
                                if (Session.ManageKey == "DATABASE")
                                    template = ManageTemplate.GetTemplate("CrudTemplateNoTimespanNoKeyuni.ico");
                                else
                                {
                                    template = ManageTemplate.GetTemplate("CrudTemplateDynamic.ico");
                                    if (Session.Keys.Count == 1)
                                    {
                                        foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                                            Session.TableKey = kvp.Key;
                                    }
                                }

                            if (Session.Keyuni && !Session.Timespan)
                                template = ManageTemplate.GetTemplate("CrudTemplateNoTimespan.ico");

                            if (!Session.Keyuni && Session.Timespan)
                                template = ManageTemplate.GetTemplate("CrudTemplateNoKeyuni.ico");
                        }

                        if (Session.Modality == Modality.ThreeLayer)
                        {
                            template = ManageTemplate.GetTemplate("/2015ThreeLayer/TemplateDataLayerThreeLayer.ico");
                        }

                        string mainCode = string.Empty;
                        if (Session.Keyuni) mainCode = "Keyuni";
                        else mainCode = Session.TableKey;

                        template = template.Replace("#Version#", Session.Version);
                        template = template.Replace("#Date#", System.DateTime.Now.ToString("dd/MM/yyyy"));
                        template = template.Replace("***Provider***", provider);

                        template = template.Replace("***SWITCHCUSTOMFIELDS***", GetSwitchCustomFields(dt));
                        template = template.Replace("***FIELDENUM***", GetFieldEnum(dt));

                        template = template.Replace("***MainCode***", mainCode);
                        template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                        template = template.Replace("***ClassName***", tableName);
                        template = template.Replace("***ClassFile***", tableName);

                        template = template.Replace("***PrivateField***", privateField);
                        template = template.Replace("***PublicField***", publicField);

                        template = template.Replace("***KeysParameter***", GetReturnTypeKeyParameters(tableName, true));
                        template = template.Replace("***SetKeyValues***", SetKeyValues(tableName));
                        template = template.Replace("***ReturnTypeKey***", GetReturnTypeKey(tableName));
                        template = template.Replace("***ReturnTypeKeyParameters***", GetReturnTypeKeyParameters(tableName, true));

                        template = template.Replace("***TestEmptyKey***", TestEmptyKey());
                        template = template.Replace("***ReturnKeyInsert***", ReturnKeyInsert());
                        template = template.Replace("***SetNullValueFromType***", Session.Keys.Count > 1 ? "string.Empty" : GetNullOperator(Session.Keys[Session.TableKey].NetType));
                        template = template.Replace("***SetConvertReturnIdentity***", SetConvertReturnIdentity(tableName));

                        template = template.Replace("***FirstKey***", GetFirstKeys());
                        template = template.Replace("***FirstKeyNull***", GetFirstKeysNull());


                        template = template.Replace("***Mapping***", mapping);
                        template = template.Replace("***Parameters***", parameters);
                        if (Session.Keyuni)
                            template = template.Replace("***ParametersList***", parametersList);
                        else
                        {
                            template = template.Replace("***ParametersListInsert***", parametersListInsert);
                            template = template.Replace("***ParametersListUpdate***", parametersListUpdate);
                            template = template.Replace("***ParametersListDelete***", parametersListDelete);
                        }

                        template = template.Replace("***ParametersListKey***", parametersListKey);
                        template = template.Replace("***GetParametersKeyCode***", parametersKeyForCode);

                        template = template.Replace("***GetParametersKey***", parametersKey);

                        template = template.Replace("***Collection***", storedProcedures);

                        template = template.Replace("***Insert***", string.Format(Session.InsertFunction, tableName));
                        template = template.Replace("***Update***", string.Format(Session.UpdateFunction, tableName));
                        template = template.Replace("***Delete***", string.Format(Session.DeleteFunction, tableName));
                        template = template.Replace("***Read***", string.Format(Session.ReadFunction, tableName));
                        template = template.Replace("***GetCollection***", string.Format(Session.GetCollectionFunction, tableName));
                        template = template.Replace("***GetByKeyuni***", string.Format(Session.GetByKeyuniFunction, tableName));
                        template = template.Replace("***GetByCode***", string.Format(Session.GetByCodeFunction, tableName));
                        template = template.Replace("***IsUnknown***", Session.IsUnknown);

                        template = template.Replace("***LogFieldSelect***", fieldLog);

                        if (ManageTemplate.WriteTemplate(Session.Folder, template, tableName, null))
                            result = true;

                        if (Session.CopyProject)
                            ManageTemplate.WriteTemplate(Session.PathCopyDataLayer, template, tableName, null);
                    }
                }
            }
            return result;
        }

        private string GetFirstKeys()
        {
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                return kvp.Key;
            }
            return string.Empty;
        }

        private string GetFirstKeysNull()
        {
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                return GetNullOperator(kvp.Value.NetType);
            }
            return string.Empty;
        }

        private bool GetTemplateThreeLayer(ArrayList tables, SqlConnection connection)
        {
            GetTemplateEasy(tables, connection);

            bool result = false;

            if (tables != null)
            {
                foreach (DataTable dt in tables)
                {
                    string template = string.Empty;

                    string tableName = dt.Rows[0]["Table"].ToString().Trim();
                    string className = tableName;

                    if (className.Equals("Validity"))
                    {
                        string t = "";
                    }

                    Session.Keys = GetInfoFieldSqlServer.GetKeysDictionary(tableName);
                    SetRealTableKey(tableName);

                    if (!tableName.Equals("sysdiagrams"))
                    {
                        string provider = "ProviderHelper.ConnectionString";
                        if (connection.ConnectionString.Contains("Counter"))
                            provider = "ProviderHelper.ConnectionStringCounter";
                        string privateField = string.Empty;
                        if (Session.Framework == Framework.net20)
                            privateField = GetPrivateField(dt);
                        string publicField = GetPublicField(dt);
                        string mapping = GetMapping(dt);
                        string parameters = GetParametersEasy(dt, tableName);
                        string parametersKey = GetParametersKey(dt, tableName, true);
                        string parametersKeyForCode = GetParametersKey(dt, tableName, false);
                        string parametersList = string.Empty;
                        string parametersListInsert = string.Empty;
                        string parametersListUpdate = string.Empty;
                        string parametersListKey = string.Empty;
                        string fieldLog = string.Empty;

                        parametersListInsert = GetParametersListEasy(dt, tableName, "Insert");
                        parametersListUpdate = GetParametersListEasy(dt, tableName, "Update");

                        parametersListKey = GetParametersListKey(dt, tableName);

                        fieldLog = GetLogSelect(dt, tableName);

                        string storedProcedures = GetStoredProceduresSqlServer.GetStoredBusiness(connection, tableName, string.Empty);

                        // Forzatura  Modality.ThreeLayer                        
                        template = ManageTemplate.GetTemplate("2015ThreeLayer/TemplateBusinessThreeLayer.ico");

                        string mainCode = string.Empty;
                        if (Session.Keyuni) mainCode = "Keyuni";
                        else mainCode = Session.TableKey;

                        template = template.Replace("***Collection***", storedProcedures);

                        template = template.Replace("***ClassFile***", string.Concat(className, Session.ClassSuffix));
                        template = template.Replace("***ClassName_LowerCase***", className.ToLower());
                        template = template.Replace("***MappingDataLayer***", CreateMapping(dt, false, className.ToLower(), string.Concat("map_", className)));
                        template = template.Replace("***ReverseMappingDataLayer***", CreateMapping(dt, true, GetPrivate(className), string.Concat("map_", className)));

                        template = template.Replace("#Version#", Session.Version);
                        template = template.Replace("#Date#", System.DateTime.Now.ToString("dd/MM/yyyy"));
                        template = template.Replace("***Provider***", provider);

                        template = template.Replace("***SWITCHCUSTOMFIELDS***", GetSwitchCustomFields(dt));
                        template = template.Replace("***FIELDENUM***", GetFieldEnum(dt));

                        template = template.Replace("***MainCode***", mainCode);
                        template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                        template = template.Replace("***NameSpaceBusiness***", Session.NamespaceBusinessLayer);
                        template = template.Replace("***ClassName***", tableName);
                        template = template.Replace("***PrivateField***", privateField);
                        template = template.Replace("***PublicField***", publicField);

                        template = template.Replace("***KeysParameter***", GetReturnTypeKeyParameters(tableName, true));                        
                        template = template.Replace("***ReturnTypeKey***", GetReturnTypeKey(tableName));
                        template = template.Replace("***ReturnTypeKeyParameters***", GetReturnTypeKeyParameters(tableName, true));
                        template = template.Replace("***ReturnTypeKeyParametersOnlyValue***", GetReturnTypeKeyParametersOnlyValue(tableName));

                        template = template.Replace("***TestEmptyKey***", TestEmptyKey());
                        template = template.Replace("***ReturnKeyInsert***", ReturnKeyInsert());
                        template = template.Replace("***SetNullValueFromType***", Session.Keys.Count > 1 ? "string.Empty" : GetNullOperator(Session.Keys[Session.TableKey].NetType));
                        template = template.Replace("***SetConvertReturnIdentity***", SetConvertReturnIdentity(tableName));

                        template = template.Replace("***Mapping***", mapping);
                        template = template.Replace("***Parameters***", parameters);
                        if (Session.Keyuni)
                            template = template.Replace("***ParametersList***", parametersList);
                        else
                        {
                            template = template.Replace("***ParametersListInsert***", parametersListInsert);
                            template = template.Replace("***ParametersListUpdate***", parametersListUpdate);
                        }

                        template = template.Replace("***ParametersListKey***", parametersListKey);
                        template = template.Replace("***GetParametersKeyCode***", parametersKeyForCode);

                        template = template.Replace("***GetParametersKey***", parametersKey);

                        

                        template = template.Replace("***Insert***", string.Format(Session.InsertFunction, tableName));
                        template = template.Replace("***Update***", string.Format(Session.UpdateFunction, tableName));
                        template = template.Replace("***Delete***", string.Format(Session.DeleteFunction, tableName));
                        template = template.Replace("***Read***", string.Format(Session.ReadFunction, tableName));
                        template = template.Replace("***GetCollection***", string.Format(Session.GetCollectionFunction, tableName));
                        template = template.Replace("***GetByKeyuni***", string.Format(Session.GetByKeyuniFunction, tableName));
                        template = template.Replace("***GetByCode***", string.Format(Session.GetByCodeFunction, tableName));
                        template = template.Replace("***IsUnknown***", Session.IsUnknown);

                        template = template.Replace("***LogFieldSelect***", fieldLog);

                        string folderBusiness = string.Concat(Session.Folder, "", "Business", "\\");
                        if (!Directory.Exists(folderBusiness))
                            Directory.CreateDirectory(folderBusiness);                        

                        if (ManageTemplate.WriteTemplate(folderBusiness, template, tableName, null))
                            result = true;

                        if (Session.CopyProject)
                            ManageTemplate.WriteTemplate(Session.PathCopyBusiness, template, tableName, null);
                    }
                }
            }
            return result;
        }

        private string CreateMapping(DataTable dt, bool reverse, string privateClass, string businessLayerClass)
        {
            StringBuilder sb = new StringBuilder();
            string fieldName = string.Empty;
            string fromNamespace = string.Empty;
            string toNamespace = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                fieldName = GetPublic(dr["Name"].ToString());
                if (reverse)
                {
                    toNamespace = businessLayerClass;
                    fromNamespace = string.Concat("new", GetPublic(privateClass));
                }
                else
                {
                    toNamespace = "this";
                    fromNamespace = privateClass;
                }

                sb.Append(string.Format("\n\t\t\t {0}.{1} = {2}.{1};", fromNamespace, fieldName, toNamespace));
            }

            //if (reverse)
            //    sb.Append(string.Concat("\n\t\t\t return ", businessLayerClass));

            return sb.ToString();
        }

        private string ReturnKeyInsert()
        {
            StringBuilder sb = new StringBuilder();

            if (Session.Keys.Count == 1)
            {
                if (Session.GuidCreator == "Framework")
                    sb.Append(string.Format("return {0};", Session.Keys[Session.TableKey].Name));

                if (Session.GuidCreator == "Database")
                {
                    sb.Append("\nforeach (SqlParameter parameter in parameters)");
                    sb.Append("\n{");
                    sb.Append(@"\n\tif (parameter.ParameterName.Equals(""@Identity""))");
                    sb.Append("\n\t\treturn ***SetConvertReturnIdentity***");
                    sb.Append("\n}");
                }
            }
            else
            {
                sb.Append("\n\t\t\tArrayList returnKey = new ArrayList();");

                foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                    sb.Append(string.Format("\n\t\t\treturnKey.Add({0});", kvp.Key));

                sb.Append("\n\t\t\treturn returnKey;");
            }

            return sb.ToString();
        }

        private string TestEmptyKey()
        {
            StringBuilder sb = new StringBuilder();

            if (Session.Keys.Count == 1)
            {
                if (Session.GuidCreator == "Framework")
                {
                    foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                        sb.Append(string.Format("if ({0} == {1}) {0} = {2}", kvp.Key, GetNullOperator(kvp.Value.NetType), GetDefaultValue(kvp.Value.NetType)));

                    sb.Append("\n\n\t\t\t");
                }
            }

            return sb.ToString();
        }

        private string SetConvertReturnIdentity(string tableName)
        {
            StringBuilder sb = new StringBuilder();

            if (Session.Keys.Count > 1)
            {
                return "parameter.Value.ToString();";
            }
            else
            {
                foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                    return string.Format("{0}.Parse(parameter.Value.ToString());", kvp.Value.NetType);
            }
            return string.Empty;
        }

        private string GetReturnTypeKey(string tableName)
        {
            StringBuilder sb = new StringBuilder();

            if (Session.Keys.Count > 1)
            {
                //foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                //    sb.Append(string.Concat(kvp.Value.NetType, " ", kvp.Key, "***"));

                return "ArrayList";
            }
            else
            {
                foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                    return kvp.Value.NetType;
            }

            return sb.ToString().Substring(0, sb.Length - 3);
        }

        private string GetReturnTypeKeyParametersOnlyValue(string tableName)
        {
            StringBuilder sb = new StringBuilder();

            //if (Session.Keys.Count > 1)
            //{

            //else
            //{
            //    foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            //        return kvp.Value.NetType;
            //}

            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                sb.Append(string.Concat(GetPrivate(kvp.Key), ", "));

            return sb.ToString().Substring(0, sb.Length - 2);
        }

        private string GetReturnTypeKeyParameters(string tableName, bool isPrivate)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                sb.Append(string.Concat(kvp.Value.NetType, " ", isPrivate ? GetPrivate(kvp.Key) : kvp.Key, ", "));

            return sb.ToString().Substring(0, sb.Length - 2);
        }

        private string SetKeyValues(string tableName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                sb.Append(string.Concat(GetPublic(kvp.Key), " = ", GetPrivate(kvp.Key), ";\n\t\t\t"));            

            return sb.ToString().Substring(0, sb.Length - 2);
        }

        //private string GetKeysParameter(string tableName)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
        //    {
        //        sb.Append(string.Concat(kvp.Value.NetType, " ", kvp.Key, ", "));
        //    }

        //    return sb.ToString().Substring(0, sb.Length - 2);
        //}

        private string GetParametersKey(DataTable dt, string tableName, bool isPublic)
        {
            string parameters = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string columnName = dr["Name"].ToString();
                bool isFieldValid = false;

                if (Session.Keys.ContainsKey(columnName))
                    isFieldValid = true;

                if (isFieldValid)
                {
                    string dbType = string.Empty;
                    dbType = GetDBType(dr["DbType"].ToString());

                    if (!dr["Length"].ToString().Equals("0"))
                        dbType = string.Concat(dbType, ", ", dr["Length"].ToString());

                    parameters = string.Concat(parameters, "\n");
                    parameters = string.Format("{0}\t\t\tSqlParameter parameter{1} = new SqlParameter(\"@{1}\", SqlDbType.{2}); \n", parameters, GetPublic(dr["Name"].ToString()), dbType);
                    if (Session.NullLogic)
                        parameters = string.Concat(parameters, "\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", isPublic ? GetPublic(dr["Name"].ToString()) : GetPrivate(dr["Name"].ToString()), "); \n");
                    else
                    {
                        //TODO : ECCCEZIONE CAMPI SHORT NULLABILI
                        if (((dr["Name"].Equals("HomePoints")) || (dr["Name"].Equals("HomePoints"))) || ((dr["Name"].Equals("PartialHome1")) || (dr["Name"].Equals("PartialVisitors1")))
                            || ((dr["Name"].Equals("PartialHome2")) || (dr["Name"].Equals("PartialVisitors2"))) || ((dr["Name"].Equals("PartialHome3")) || (dr["Name"].Equals("PartialVisitors3")))
                            || ((dr["Name"].Equals("PartialHome4")) || (dr["Name"].Equals("PartialVisitors4"))) || ((dr["Name"].Equals("PartialHome5")) || (dr["Name"].Equals("PartialVisitors5")))
                            || ((dr["Name"].Equals("PartialHome6")) || (dr["Name"].Equals("PartialVisitors6"))) || ((dr["Name"].Equals("PartialHome7")) || (dr["Name"].Equals("PartialVisitors7")))
                            || ((dr["Name"].Equals("PartialHome8")) || (dr["Name"].Equals("PartialVisitors8"))) || ((dr["Name"].Equals("PartialHome9")) || (dr["Name"].Equals("PartialVisitors9")))
                            || ((dr["Name"].Equals("PartialHome10")) || (dr["Name"].Equals("PartialVisitors10"))) || ((dr["Name"].Equals("PartialHome11")) || (dr["Name"].Equals("PartialVisitors11")))
                            || (dr["Name"].Equals("TaskRunningNow"))                            
                            )
                            parameters = string.Concat(parameters, "\t\t\tparameter", GetPublic(dr["Name"].ToString()), ".Value = ", GetPublic(dr["Name"].ToString()), ";\n");
                        else
                        {
                            if ((dr["Type"].ToString().Equals("int?")) || (dr["Type"].ToString().Equals("Int16?")) || (dr["Type"].ToString().Equals("decimal?")))
                                parameters = string.Concat(parameters, "\t\t\tparameter", GetPublic(dr["Name"].ToString()), ".Value = ", isPublic ? GetPublic(dr["Name"].ToString()) : GetPrivate(dr["Name"].ToString()), ";\n");
                            else
                                parameters = string.Concat(parameters, "\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", isPublic ? GetPublic(dr["Name"].ToString()) : GetPrivate(dr["Name"].ToString()), "); \n");
                        }
                    }

                }
            }
            return parameters;
        }

        private string GetParametersEasy(DataTable dt, string tableName)
        {
            string parameters = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string columnName = dr["Name"].ToString();
                bool isFieldValid = true;

                if (columnName.Equals("Keyuni"))
                    isFieldValid = false;

                if (columnName.Equals("Timespan"))
                    isFieldValid = false;

                if ((Session.Keyuni) && (columnName.Equals(Session.TableKey)))
                    isFieldValid = false;

                if (isFieldValid)
                {
                    string dbType = string.Empty;
                    dbType = GetDBType(dr["DbType"].ToString());

                    if (!dr["Length"].ToString().Equals("0"))
                        dbType = string.Concat(dbType, ", ", dr["Length"].ToString());

                    parameters = string.Concat(parameters, "\n");
                    parameters = string.Format("{0}\t\t\t\tSqlParameter parameter{1} = new SqlParameter(\"@{1}\", SqlDbType.{2}); \n", parameters, GetPublic(dr["Name"].ToString()), dbType);
                    if (Session.NullLogic)
                        parameters = string.Concat(parameters, "\t\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", GetPublic(dr["Name"].ToString()), "); \n");
                    else
                    {
                        //TODO : ECCCEZIONE CAMPI SHORT NULLABILI
                        if (((dr["Name"].Equals("HomePoints")) || (dr["Name"].Equals("HomePoints"))) || ((dr["Name"].Equals("PartialHome1")) || (dr["Name"].Equals("PartialVisitors1")))
                            || ((dr["Name"].Equals("PartialHome2")) || (dr["Name"].Equals("PartialVisitors2"))) || ((dr["Name"].Equals("PartialHome3")) || (dr["Name"].Equals("PartialVisitors3")))
                            || ((dr["Name"].Equals("PartialHome4")) || (dr["Name"].Equals("PartialVisitors4"))) || ((dr["Name"].Equals("PartialHome5")) || (dr["Name"].Equals("PartialVisitors5")))
                            || ((dr["Name"].Equals("PartialHome6")) || (dr["Name"].Equals("PartialVisitors6"))) || ((dr["Name"].Equals("PartialHome7")) || (dr["Name"].Equals("PartialVisitors7")))
                            || ((dr["Name"].Equals("PartialHome8")) || (dr["Name"].Equals("PartialVisitors8"))) || ((dr["Name"].Equals("PartialHome9")) || (dr["Name"].Equals("PartialVisitors9")))
                            || ((dr["Name"].Equals("PartialHome10")) || (dr["Name"].Equals("PartialVisitors10"))) || ((dr["Name"].Equals("PartialHome11")) || (dr["Name"].Equals("PartialVisitors11"))))
                            parameters = string.Concat(parameters, "\t\t\t\tparameter", GetPublic(dr["Name"].ToString()), ".Value = ", GetPublic(dr["Name"].ToString()), ";\n");
                        else
                        {
                            if ((dr["Type"].ToString().Equals("int?")) || (dr["Type"].ToString().Equals("Int16?")) || (dr["Type"].ToString().Equals("decimal?")))
                                parameters = string.Concat(parameters, "\t\t\t\tparameter", GetPublic(dr["Name"].ToString()), ".Value = ", GetPublic(dr["Name"].ToString()), ";\n");
                            else
                                parameters = string.Concat(parameters, "\t\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", GetPublic(dr["Name"].ToString()), "); \n");
                        }
                    }

                }
            }
            return parameters;
        }

        private string GetParametersListEasy(DataTable dt, string tableName)
        {
            return GetParametersListEasy(dt, tableName, string.Empty);
        }

        private string GetParametersListEasy(DataTable dt, string tableName, string operation)
        {
            string parametersList = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string fieldName = dr["Name"].ToString();
                bool isFieldValid = true;

                if (fieldName.Equals("Timespan"))




                    isFieldValid = false;

                if (!Session.Keyuni)
                {
                    if (fieldName.Equals("Keyuni"))
                        isFieldValid = false;
                }

                if ((Session.Keyuni) && (fieldName.Equals(Session.TableKey)))
                    isFieldValid = false;

                if (Session.GuidCreator == "Database")
                {
                    if ((!Session.Keyuni) && (fieldName.Equals(Session.TableKey) && operation.Equals("Insert")))
                        isFieldValid = false;
                }
                else isFieldValid = true;

                if (operation.Equals("Delete"))
                    if (Session.Keys.ContainsKey(fieldName))
                        isFieldValid = true;
                    else
                        isFieldValid = false;

                if (isFieldValid)
                {
                    parametersList = string.Concat(parametersList, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameter", GetPublic(fieldName), ", \n");
                }
            }

            if (Session.GuidCreator == "Database")
                parametersList = string.Concat(parametersList, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameterIdentity, \n");

            parametersList = string.Concat(parametersList.Substring(0, parametersList.Length - 3), " };");

            return parametersList;
        }

        private string GetParametersListKey(DataTable dt, string tableName)
        {
            string parametersList = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string fieldName = dr["Name"].ToString();
                bool isFieldValid = false;

                if (Session.Keys.ContainsKey(fieldName))
                    isFieldValid = true;

                if (isFieldValid)
                    parametersList = string.Concat(parametersList, "\t\t\t\t", "parameter", GetPublic(fieldName), ", \n");
            }

            //parametersList = string.Concat(parametersList, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameterIdentity, \n");

            parametersList = string.Concat(parametersList.Substring(0, parametersList.Length - 3), "");

            return parametersList;
        }

        private string GetLogSelect(DataTable dt, string tableName)
        {
            string fieldLog = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                fieldLog = string.Concat(fieldLog, string.Format("'{0}=',{0},", dr["Name"].ToString()));

            }

            fieldLog = string.Format("SELECT {0} FROM {1} WHERE {1}Code = ", fieldLog.Substring(0, fieldLog.Length - 1), tableName);

            return fieldLog;
        }

        private string GetSwitchCustomFields(DataTable dt)
        {
            string mainTemplate = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (!dr["Name"].ToString().StartsWith("VERSION_"))
                {
                    string template = ManageTemplate.GetTemplate("Snippet/SwithcCustomFields.ico");

                    template = template.Replace("***FIELDNAME***", dr["Name"].ToString());
                    string fieldType = "FieldType.String";
                    switch (dr["type"].ToString())
                    {
                        case "Time":
                        case "Date":
                        case "DateTime":
                        case "DateTime?": fieldType = "FieldType.Date"; break;
                        case "int":
                        case "decimal":
                        case "double":
                        case "Int16":
                        case "Int64":
                        case "float": fieldType = "FieldType.Numeric"; break;
                        case "bool": fieldType = "FieldType.Bool"; break;
                    }
                    template = template.Replace("***FIELDTYPE***", fieldType);
                    mainTemplate = string.Concat(mainTemplate, "\n\n", template);
                }
            }

            return mainTemplate;
        }

        private string GetFieldEnum(DataTable dt)
        {
            string enumList = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (enumList.Equals(string.Empty))
                    enumList = string.Concat("\t\t\t", dr["Name"].ToString());
                else enumList = string.Concat(enumList, ",", "\n\t\t\t", dr["Name"].ToString());
            }
            return enumList;
        }


        #endregion

        #region Professional Mode

        private bool GetTemplateProfessional(ArrayList tables, SqlConnection connection)
        {
            bool result = false;

            // FACTORY
            string templateFactory = ManageTemplate.GetTemplate("TemplateDataFactoryHeader.ico");

            foreach (DataTable dt in tables)
            {
                string tableName = dt.Rows[0]["Table"].ToString().Trim();

                string privateField = string.Empty;
                if (Session.Framework == Framework.net20)
                    GetPrivateField(dt);
                string publicField = GetPublicField(dt);
                string template = string.Empty;

                // CLASS
                template = ManageTemplate.GetTemplate("TemplateClassLayer.ico");
                template = template.Replace("#Version#", Session.Version);
                template = template.Replace("#Date#", System.DateTime.Now.ToString("dd/MM/yyyy"));
                template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                template = template.Replace("***ClassName***", tableName);
                template = template.Replace("***PrivateField***", privateField);
                template = template.Replace("***PublicField***", publicField);
                if (!ManageTemplate.WriteTemplate(string.Concat(Session.Folder, "//ClassLayer//", tableName), template, tableName, null)) result = true;

                // COLLECTION
                template = ManageTemplate.GetTemplate("TemplateCollection.ico");
                template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                template = template.Replace("***ClassName***", tableName);
                if (!ManageTemplate.WriteTemplate(string.Concat(Session.Folder, "//ClassLayer//", tableName), template, string.Concat(tableName, "Collection"), null)) result = true;

                // UNKNOW
                template = ManageTemplate.GetTemplate("TemplateUnknown.ico");
                template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                template = template.Replace("***ClassName***", tableName);
                if (!ManageTemplate.WriteTemplate(string.Concat(Session.Folder, "//ClassLayer//", tableName), template, string.Concat(tableName, "Unknown"), null)) result = true;

                // DATALAYER

                string mapping = GetMapping(dt);
                string parameters = GetParametersProfessional(dt, tableName);
                string parametersList = GetParametersListProfessional(dt, tableName);
                string storedProcedures = GetStoredProceduresSqlServer.GetStored(connection, tableName, string.Empty);

                template = ManageTemplate.GetTemplate("TemplateDataLayer.ico");
                template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                template = template.Replace("***ClassName***", tableName);
                template = template.Replace("***Mapping***", mapping);
                template = template.Replace("***Parameters***", parameters);
                template = template.Replace("***ParametersList***", parametersList);
                template = template.Replace("***Collection***", storedProcedures);
                if (!ManageTemplate.WriteTemplate(string.Concat(Session.Folder, "//DataLayer//"), template, string.Concat("DataProvider", tableName), null)) result = true;

                // MANAGER
                storedProcedures = GetStoredProceduresSqlServer.GetStored(connection, tableName, "Manager");
                template = ManageTemplate.GetTemplate("TemplateManager.ico");
                template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                template = template.Replace("***ClassName***", tableName);
                template = template.Replace("***Collection***", storedProcedures);
                if (!ManageTemplate.WriteTemplate(string.Concat(Session.Folder, "//ManagerLayer//"), template, string.Concat("Manager", tableName), null)) result = true;

                // INTERFACE
                template = ManageTemplate.GetTemplate("TemplateInterface.tmc");
                storedProcedures = GetStoredProceduresSqlServer.GetStored(connection, tableName, "Interface");
                template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                template = template.Replace("***ClassName***", tableName);
                template = template.Replace("***Collection***", storedProcedures);
                if (!ManageTemplate.WriteTemplate(string.Concat(Session.Folder, "//DataLayer//Interface//"), template, string.Concat("IDataProvider", tableName), null)) result = true;

                // FACTORY
                template = ManageTemplate.GetTemplate("TemplateDataFactoryBody.ico");
                template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
                template = template.Replace("***ClassName***", tableName);
                template = string.Concat(template, "\n\t\t", "***GenerateFactory***");
                templateFactory = templateFactory.Replace("***GenerateFactory***", template);

            }
            templateFactory = templateFactory.Replace("***NameSpace***", Session.NamespaceDataLayer);
            templateFactory = templateFactory.Replace("***GenerateFactory***", string.Empty);
            if (ManageTemplate.WriteTemplate(string.Concat(Session.Folder, "//"), templateFactory, "DataProviderFactory", null)) result = true;

            return result;
        }

        private string GetParametersProfessional(DataTable dt, string tableName)
        {
            string parameters = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string columnName = dr["Name"].ToString();

                if (!GetInfoFieldSqlServer.IsRowGuid(tableName, columnName))
                {
                    if (!GetInfoFieldSqlServer.IsIdentity(tableName, columnName))
                    {
                        bool isUniqueIdentifier = GetInfoFieldSqlServer.IsUniqueIdentifier(tableName, columnName);
                        bool databaseCreator = Session.GuidCreator.Equals("Database");

                        string fieldName = GetPublic(dr["Name"].ToString());

                        string dbType = string.Empty;
                        dbType = GetDBType(dr["DbType"].ToString());

                        if (!dr["Length"].ToString().Equals("0"))
                            dbType = string.Concat(dbType, ", ", dr["Length"].ToString());

                        parameters = string.Concat(parameters, "\n");
                        parameters = string.Format("{0}\t\t\t\tSqlParameter parameter{1} = new SqlParameter(\"@{1}\", SqlDbType.{2}); \n", parameters, GetPublic(dr["Name"].ToString()), dbType);

                        if (isUniqueIdentifier)
                        {
                            string prefix = "if (!operation.Equals(EventContext.Insert))";
                            if (databaseCreator)
                            {
                                parameters = string.Concat(parameters, "\t\t\t\t", prefix, "parameter", fieldName, ".Value = item.", fieldName, "; \n");
                                parameters = string.Concat(parameters, "\t\t\t\t", "parameter", fieldName, ".Direction = ParameterDirection.Output;");
                            }
                            else
                            {
                                parameters = string.Concat(parameters, "\t\t\t\t", prefix, "parameter", fieldName, ".Value = item.", fieldName, "; \n");
                                parameters = string.Concat(parameters, "\t\t\t\t", "else parameter", fieldName, ".Value = Guid.NewGuid();");
                            }
                        }
                        else parameters = string.Concat(parameters, "\t\t\t\tparameter", fieldName, ".Value = item.", fieldName, "; \n");
                    }
                }
            }
            return parameters;
        }

        private string GetParametersListProfessional(DataTable dt, string tableName)
        {
            string parametersList = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string fieldName = dr["Name"].ToString();

                bool goUniqueIdentifier = true;
                if (GetInfoFieldSqlServer.IsUniqueIdentifier(tableName, fieldName))
                {
                    if (GetInfoFieldSqlServer.IsRowGuid(tableName, fieldName) || Session.GuidCreator.Equals("Database"))
                        goUniqueIdentifier = false;
                }

                if (!GetInfoFieldSqlServer.IsIdentity(tableName, fieldName) && goUniqueIdentifier)
                {
                    parametersList = string.Concat(parametersList, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameter", GetPublic(fieldName), ", \n");
                }
            }
            parametersList = string.Concat(parametersList.Substring(0, parametersList.Length - 3), " };");
            return parametersList;
        }

        #endregion


        private string GetPrivateField(DataTable dt)
        {
            string privateField = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                privateField = string.Concat(privateField, "\t\tprivate ");
                privateField = string.Concat(privateField, GetNetType(dr["Type"].ToString()), " ");
                privateField = string.Concat(privateField, GetPrivate(dr["Name"].ToString()), "; \n");
            }

            return privateField;
        }

        private string GetPublicField(DataTable dt)
        {
            string publicField = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["Name"].ToString().Equals("Timespan") && !Session.Timespan)
                    break;

                if (dr["Name"].ToString().Equals("Keyuni") && !Session.Keyuni)
                    break;

                publicField = string.Concat(publicField, "\t\tpublic ");
                publicField = string.Concat(publicField, GetNetType(dr["Type"].ToString()), " ");

                if (Session.Framework == Framework.net20)
                    publicField = string.Concat(publicField, GetPublic(dr["Name"].ToString()), " \n");
                if (Session.Framework == Framework.net35)
                    publicField = string.Concat(publicField, GetPublic(dr["Name"].ToString()), " { get; set; } \n");

                if (Session.Framework == Framework.net20)
                {
                    publicField = string.Concat(publicField, "\t\t{ \n ");
                    publicField = string.Concat(publicField, "\t\t\tget { return ", GetPrivate(dr["Name"].ToString()), "; } \n ");
                    publicField = string.Concat(publicField, "\t\t\tset { ", GetPrivate(dr["Name"].ToString()), " = value; } \n ");
                    publicField = string.Concat(publicField, "\t\t} \n ");
                }
            }

            return publicField;
        }

        private string GetMapping(DataTable dt)
        {
            string mapping = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string fieldName = GetPublic(dr["Name"].ToString());
                if (keyCode == string.Empty) keyCode = fieldName;

                if ((!fieldName.Equals("Keyuni")) && (!Session.Keyuni))
                {
                    if ((!fieldName.Equals("Timespan")) && (!Session.Timespan))
                    {
                        if (!(bool)dr["Nullable"])
                        {
                            mapping = string.Concat(mapping, "\t\t\titem.", fieldName, " = ");
                            mapping = string.Concat(mapping, "(", GetNetType(dr["Type"].ToString()), ")", "dataReader[\"");
                            mapping = string.Concat(mapping, dr["Name"].ToString(), "\"]; \n");
                        }
                        else
                        {
                            if (Session.NullLogic)
                            {
                                string nullOperator = GetNullOperator(dr["Type"].ToString());
                                mapping = string.Format("{0}\t\t\titem.{1} = dataReader[\"{2}\"] == DBNull.Value ? {3} : ({4})dataReader[\"{2}\"]; \n", mapping, fieldName, dr["Name"].ToString(), nullOperator, dr["Type"].ToString());
                            }
                            else
                            {
                                string type = GetNetType(dr["Type"].ToString());


                                switch (type)
                                {
                                    case "Int16?":
                                    case "int?":
                                    case "decimal?":
                                        mapping = string.Concat(mapping, "\t\t\titem.", dr["Name"].ToString(), " = null;\n");
                                        mapping = string.Concat(mapping, "\t\t\tif (dataReader[\"", dr["Name"].ToString(), "\"] != DBNull.Value)\n");
                                        mapping = string.Concat(mapping, "\t\t\t\titem.", dr["Name"].ToString(), " = (", dr["Type"].ToString().Substring(0, dr["Type"].ToString().Length - 1), ")dataReader[\"", fieldName, "\"]; \n");
                                        break;
                                    default:
                                        mapping = string.Format("{0}\t\t\titem.{1} = dataReader[\"{2}\"] == DBNull.Value ? {3} : ({4})dataReader[\"{2}\"]; \n", mapping, fieldName, dr["Name"].ToString(), GetNullOperator(dr["Type"].ToString()), dr["Type"].ToString());
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return mapping;
        }

        public static void SetRealTableKey(string tableName)
        {
            if ((Session.Code == "TableCode") || (Session.Code.ToUpper() == "ID"))
            {
                if (Session.Code == "TableCode")
                    Session.TableKey = string.Concat(tableName, "Code");
                if (Session.Code.ToUpper() == "ID")
                    Session.TableKey = Session.Code;
            }
            else Session.TableKey = Session.Code;
        }

        #region Function String

        private string GetPrivate(string text)
        {
            return string.Concat(text.Substring(0, 1).ToLower(), text.Substring(1));
        }

        private string GetDBType(string type)
        {
            switch (type)
            {
                case "nvarchar": return "NVarChar";
                case "nchar": return "Char";
                case "varchar": return "VarChar";
                case "bool": return "Boolean";
                case "datetime": return "DateTime";
                case "text": return "Text";
                case "ntext": return "NText";
                case "smallint": return "SmallInt";
                case "tinyint": return "TinyInt";
                case "uniqueidentifier": return "UniqueIdentifier";
                case "smalldatetime": return "SmallDateTime";
                case "varbinary": return "VarBinary";
                case "timespan": return "byte[]";
                case "image": return "VarBinary";
                case "smallmoney": return "Money";

                default: return GetPublic(type);
            }
        }



        private string GetNetType(string type)
        {
            switch (type)
            {
                case "System.Drawing.Image": return "byte[]";
                default: return type;
            }
        }

        private string GetPublic(string text)
        {
            return string.Concat(text.Substring(0, 1).ToUpper(), text.Substring(1));
        }

        private string GetNullOperator(string type)
        {
            switch (type)
            {
                case "bool": return "false";
                case "int": return "Null.Int";
                case "byte": return "Null.ByteNull";
                case "decimal": return "Null.DecimalNull";
                case "Int16": return "Null.Int";
                case "real": return "Null.RealNull";
                case "float": return "Null.FloatNull";
                case "string": return "string.Empty";
                case "System.DateTime": return "null";
                case "DateTime?": return "null";
                case "smalldatetime": return "Null.MinDate";
                case "varbinary": return "Null.Int";
                case "timespan": return "null";
                case "Guid": return "Guid.Empty";
                case "byte[]": return "null";
                case "TimeSpan?": return "null";
                default: return string.Empty;
            }
        }

        private string GetDefaultValue(string type)
        {
            switch (type)
            {
                case "Guid": return "Guid.NewGuid();";
                case "int": return "Null.Int;";
                case "string": return "string.Empty;";
                default: return " ";
            }
        }

        #endregion
    }
}