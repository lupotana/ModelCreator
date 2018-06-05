using System.Data;
using Gupta.SQLBase.Data;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ModelCreator
{
   
    public class CreateClassAutoTask
    {
        public DataTable table = new DataTable();
        private string keyCode = string.Empty;
        public CreateClassAutoTask() { }

        Dictionary<string, string> FieldEsclude = new Dictionary<string, string>();

        public bool BuildTemplate(DataTable dt, Provider provider)
        {            
            string tableName = dt.Rows[0]["Table"].ToString().Trim();
            KeyColumn key = new KeyColumn();
            key.Name = dt.Rows[0]["Name"].ToString();
            key.NetType = dt.Rows[0]["Type"].ToString();
            key.Sequence = 1;
            key.Type = dt.Rows[0]["Type"].ToString();
            key.Size = int.Parse(dt.Rows[0]["Length"].ToString());
            Dictionary<string, KeyColumn> keys = new Dictionary<string, KeyColumn>();
            keys.Add(key.Name, key);
            Session.Keys = keys;
            string keyType = "int";
            string isAutomaticKey = "false";
            string typeKey = string.Empty;

              if (Session.Keys.Count == 0) return false;

            string privateField = string.Empty;
            if (Session.Framework == Framework.net20)
                privateField = GetPrivateField(dt);
            string publicField = GetPublicField(dt);
            string mapping = GetMapping(dt);
            string parameters = string.Empty; // GetParameters(dt, tableName);
            string parametersList = string.Empty;
            string parametersListInsert = string.Empty;
            string parametersListUpdate = string.Empty;
            string parametersListDelete = string.Empty;
            string parametersListClone = string.Empty;
            string declarationParametersKeys = string.Empty;
            string parameterKeys = string.Empty;
            string listParameterKeys = string.Empty;
            string isNullKeys = string.Empty;
            string parametersKeysWithOutType = string.Empty;
            string queryInsert = string.Empty;
            string queryUpdate = string.Empty;
            string queryDelete = string.Empty;
            string fieldBoolean = string.Empty;
            string queryByUpdVer = string.Empty;
            string structureField = string.Empty;
            string isSingleKey = "true";            
            
            if (Session.Keys.Count > 1)
                isSingleKey = "false";

            string template = string.Empty;

            template = ManageTemplate.GetTemplate("CrudTemplateAutoTask.ico");

            //parametersListInsert = GetParametersList(dt, tableName, false);
            //parametersListUpdate = GetParametersList(dt, tableName, false);
            //parametersListDelete = GetParametersList(dt, tableName, true);

            GetParametersListAll(dt, ref parametersListInsert, ref parametersListDelete, ref parametersListClone, ref parameters, provider);
            parametersListUpdate = parametersListInsert;
            SetAllKeys(ref declarationParametersKeys, ref listParameterKeys, ref parametersKeysWithOutType, ref isNullKeys, ref parameterKeys);
            structureField = GetStructure(dt);

            fieldBoolean = CustomBoolean.GetList();

            //declarationParametersKeys = SetDeclarationKeys();
            //parameterKeys = GetParametersDictionaryList(Session.Keys, tableName);
            //listParameterKeys = SetParametersKeys();
            //isNullKeys = SetNullKeysCondtition();

            template = template.Replace("#Version#", Session.Version);
            template = template.Replace("#Date#", System.DateTime.Now.ToString("dd/MM/yyyy"));

            template = template.Replace("#Owner#", Session.Owner);
            template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
            template = template.Replace("***ClassName***", tableName);
            template = template.Replace("***StructureField***", structureField);
            template = template.Replace("***PrivateField***", privateField);
            template = template.Replace("***PublicField***", publicField);
            template = template.Replace("***MappingDataReader***", mapping);
            template = template.Replace("***MappingDataTable***", mapping);
            template = template.Replace("***Parameters***", parameters);
            template = template.Replace("***KetType***", keyType);
            template = template.Replace("***isAutomaticKey***", isAutomaticKey);
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                template = template.Replace("***TableKey***", kvp.Key);
                if (!kvp.Value.NetType.Equals("String"))                                 
                    template = template.Replace("***TableKeyType***", string.Concat(kvp.Value.NetType, ".Parse"));                
                else template = template.Replace("***TableKeyType***", string.Empty);                                    
                break;
            }

            template = template.Replace("***DeclarationParameterKeys***", declarationParametersKeys);
            template = template.Replace("***ParameterKeys***", parameterKeys);
            template = template.Replace("***ListParameterKeys***", listParameterKeys);

            template = template.Replace("***ParametersListInsert***", parametersListInsert);
            template = template.Replace("***ParametersListUpdate***", parametersListUpdate);
            template = template.Replace("***ParametersListDelete***", parametersListDelete);
            template = template.Replace("***ParametersListClone***", parametersListClone);

            template = template.Replace("***Collection***", Session.GetCollectionFunction);
            //template = template.Replace("#GetCollectionSql#", GetViewSql(dt, tableName, true));
            //template = template.Replace("#GetCollectionSqlWithOutOrder#", GetViewSql(dt, tableName, false));
            template = template.Replace("#GetCollectionByKeys#", "GetCollectionByKeys");
            template = template.Replace("***FieldBoolean***", fieldBoolean);
            template = template.Replace("***IsSingleKey***", isSingleKey);
            template = template.Replace("***TypeKey***", keyType);            
            
            if (Session.SuperClass)
            {
                string minorField = string.Empty;
                string tableNameRelationship = string.Empty;
                DataTable dtRelationship = GetDatatableSqlBase.GetRelationship(tableName);
                foreach (DataRow drRelationship in dtRelationship.Rows)
                {
                    if (!tableNameRelationship.Equals(drRelationship["REFDTBNAME"].ToString()))
                    {
                        tableNameRelationship = drRelationship["REFDTBNAME"].ToString();
                        DataTable dtMinor = GetDatatableSqlBase.GetSchema(drRelationship["REFDTBNAME"].ToString());
                        minorField = string.Concat(minorField, GetPublicFieldForSuperClass(dtMinor, tableNameRelationship));
                    }
                    tableNameRelationship = drRelationship["REFDTBNAME"].ToString();
                }

                template = template.Replace("***SuperClass***", minorField);
                template = template.Replace("***DeclareRelationship***", GetDeclareRelationship(tableName));
                template = template.Replace("***DeclareIndex***", GetDeclareIndex(tableName));
            }
            else
            {
                template = template.Replace("***SuperClass***", string.Empty);
                template = template.Replace("***DeclareRelationship***", string.Empty);
                template = template.Replace("***DeclareIndex***", string.Empty);
            }

            //GetSql(dt, tableName, ref queryInsert, ref queryUpdate, ref queryDelete);
            //template = template.Replace("#GetByCodeSql#", GetByCodeSql(dt, tableName));
            //template = template.Replace("#GetByCodeSqlUpdVer#", GetByCodeSqlUpdVer(dt, tableName));

            template = template.Replace("***SWITCHCUSTOMFIELDS***", GetSwitchCustomFields(dt));
            template = template.Replace("***FIELDENUM***", GetFieldEnum(dt));

            template = template.Replace("***InsertSql***", queryInsert);
            template = template.Replace("#UpdateSql#", queryUpdate);
            template = template.Replace("#DeleteSql#", queryDelete);
            template = template.Replace("***Insert***", string.Format(Session.InsertFunction, tableName));
            template = template.Replace("#InsertKey#", "InsertKey");
            template = template.Replace("***Update***", string.Format(Session.UpdateFunction, tableName));
            template = template.Replace("***Delete***", string.Format(Session.DeleteFunction, tableName));
            template = template.Replace("***Read***", string.Format(Session.ReadFunction, tableName));
            template = template.Replace("***GetCollection***", string.Format(Session.GetCollectionFunction, tableName));
            template = template.Replace("***GetByCode***", string.Format(Session.GetByCodeFunction, tableName));
            template = template.Replace("***IsUnknown***", Session.IsUnknown);
            template = template.Replace("***IsUnknownCondtition***", isNullKeys);

            template = template.Replace("***CheckExist***", string.Empty);

            bool isValid = false;
            if (!(ManageTemplate.WriteTemplate(Session.Folder, template, string.Concat(tableName, ""), null))) 
                return false;
            //{
            //    #region Class Plus

            //    //if (Session.CreateCustomClass)
            //    //{
            //    //    template = ManageTemplate.GetTemplate("CrudTemplatePlusSqlBase.ico");

            //    //    template = template.Replace("#Version#", Session.Version);
            //    //    template = template.Replace("#Date#", System.DateTime.Now.ToString("dd/MM/yyyy"));
            //    //    template = template.Replace("***NameSpace***", Session.Namespace);
            //    //    template = template.Replace("***ClassName***", tableName);
            //    //    template = template.Replace("***Collection***", Session.GetCollectionFunction);
            //    //    template = template.Replace("***Insert***", string.Format(Session.InsertFunction, tableName));
            //    //    template = template.Replace("#InsertKey#", "InsertKey");
            //    //    template = template.Replace("***Update***", string.Format(Session.UpdateFunction, tableName));
            //    //    template = template.Replace("***Delete***", string.Format(Session.DeleteFunction, tableName));
            //    //    template = template.Replace("***Read***", string.Format(Session.ReadFunction, tableName));
            //    //    template = template.Replace("***GetCollection***", string.Format(Session.GetCollectionFunction, tableName));
            //    //    template = template.Replace("#GetCollectionByKeys#", "GetCollectionByKeys");
            //    //    template = template.Replace("***GetByCode***", string.Format(Session.GetByCodeFunction, tableName));
            //    //    template = template.Replace("***IsUnknown***", Session.IsUnknown);
            //    //    template = template.Replace("***DeclarationParameterKeys***", declarationParametersKeys);
            //    //    template = template.Replace("***DeclarationParameterKeysWithOutType***", parametersKeysWithOutType);

            //    //    if (!ManageTemplate.WriteTemplate(Session.Folder, template, tableName, null)) return false;
            //    //}
                
            //    //template = ManageTemplate.GetTemplate("Interface.ico");
                
            //    //template = template.Replace("#Version#", Session.Version);
            //    //template = template.Replace("#Date#", System.DateTime.Now.ToString("dd/MM/yyyy"));
            //    //template = template.Replace("***NameSpace***", Session.Namespace);
            //    //template = template.Replace("***ClassName***", tableName);

            //    //if (ManageTemplate.WriteTemplate(Session.Folder, template, string.Concat("I", tableName), null)) return true;
            //    //else return false;

            //    #endregion

            //    return false;
            //}
            return true;
        }

        public bool BuildShared()
        {
            string template = ManageTemplate.GetTemplate("Shared.ico");
            template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
            if (ManageTemplate.WriteTemplate(Session.Folder, template, "Shared", null)) return true;
            else return false;
        }

        public bool BuildDataHelper()
        {
            string template = ManageTemplate.GetTemplate("DataHelper.ico");
            template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
            if (ManageTemplate.WriteTemplate(Session.Folder, template, "DataHelper", null)) return true;
            else return false;
        }

        public bool BuildNull()
        {
            string template = ManageTemplate.GetTemplate("Null.ico");
            template = template.Replace("***NameSpace***", Session.NamespaceDataLayer);
            if (ManageTemplate.WriteTemplate(Session.Folder, template, "Null", null)) return true;
            else return false;
        }

        private string GetSwitchCustomFields(DataTable dt)
        {
            string mainTemplate = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (!dr["Name"].ToString().StartsWith("VERSION_"))
                {
                    string template = ManageTemplate.GetTemplate("Snippet/SwithcCustomFields.ico");

                    template = template.Replace("***FIELDNAME***", GetPublic(dr["Name"].ToString()));
                    string fieldType = "FieldType.String";
                    switch (dr["type"].ToString())
                    {
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

        private string GetStructure(DataTable dt)
        {
            string text = string.Empty;
            string template = "\n\n\t\t\tcolumn = new DataColumn();\n\t\t\tcolumn.DataType = System.Type.GetType(\"{0}\");\n\t\t\tcolumn.ColumnName = \"{1}\";\n\t\t\tdt.Columns.Add(column);";
            string templateKey = "\n\t\t\tkeys[0] = column;\n\t\t\tdt.PrimaryKey = keys;";
            bool isKey = false;
            foreach (DataRow dr in dt.Rows)
            {
                isKey = false;
                foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                {
                    if (dr["Name"].Equals(kvp.Key))
                    {
                        isKey = true;
                        break;
                    }
                }

                text = string.Concat(text, string.Format(template, ConversionType(dr[1].ToString()), (string)dr["Name"]));
                if (isKey)
                    text = string.Concat(text, templateKey);
            }
            return text;
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

     

        #region Keys

        private void SetAllKeys(ref string declarationParametersKeys, ref string listParameterKeys, ref string parametersKeysWithOutType, ref string isNullKeys, ref string parameters)
        {
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                string columnName = string.Empty;
                string dbType = string.Empty;
                if (!kvp.Key.StartsWith("VERSION_"))
                {
                    //SetDeclarationKeys
                    declarationParametersKeys = string.Concat(declarationParametersKeys, GetDatatableSqlBase.GetSqlBaseType(kvp.Key, kvp.Value.Type, 0, false), " ", kvp.Key, ",");
                    //SetParametersKeys
                    listParameterKeys = string.Concat(listParameterKeys, "parameter", kvp.Key, ",");
                    //SetParametersKeysWithOutType
                    parametersKeysWithOutType = string.Concat(parametersKeysWithOutType, kvp.Key, ",");
                    //SetNullKeysCondtition
                    switch (GetDatatableSqlBase.GetSqlBaseType(kvp.Key, kvp.Value.Type, 0, false))
                    {
                        case "string":
                            isNullKeys = string.Concat(isNullKeys, "(!", kvp.Key, ".Equals(string.Empty)) && ");
                            break;
                        case "DateTime?":
                            isNullKeys = string.Concat(isNullKeys, "(DateTime.Parse(", kvp.Key, ".ToString()) != Null.MinDate) && ");
                            break;
                        default: isNullKeys = string.Concat(isNullKeys, "(", kvp.Key, " != 0", ") && "); break;
                    }
                    //GetParametersDictionaryList
                    columnName = kvp.Key;
                    dbType = GetDBType(kvp.Value.Type);
                    if (!kvp.Value.Size.ToString().Equals("0"))
                        dbType = string.Concat(dbType, ", ", kvp.Value.Size);
                    parameters = string.Concat(parameters, "\n");
                    parameters = string.Format("{0}\t\t\tSQLBaseParameter parameter{1} = new SQLBaseParameter(\"{1}\", DbType.{2}); \n", parameters, GetPublic(kvp.Key), dbType);
                    parameters = string.Concat(parameters, "\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(kvp.Key), ", ", GetPublic(kvp.Key), "); \n");
                }
            }

            if (!declarationParametersKeys.Equals(string.Empty))
                declarationParametersKeys = declarationParametersKeys.Substring(0, declarationParametersKeys.Length - 1);
            else declarationParametersKeys = string.Empty;

            if (!listParameterKeys.Equals(string.Empty))
                listParameterKeys = listParameterKeys.Substring(0, listParameterKeys.Length - 1);
            else listParameterKeys = string.Empty;

            if (!parametersKeysWithOutType.Equals(string.Empty))
                parametersKeysWithOutType = parametersKeysWithOutType.Substring(0, parametersKeysWithOutType.Length - 1);
            else parametersKeysWithOutType = string.Empty;

            if (!isNullKeys.Equals(string.Empty))
            {
                isNullKeys = isNullKeys.Substring(0, isNullKeys.Length - 3).Trim();
                isNullKeys = string.Concat("(", isNullKeys, ")");
            }
            else isNullKeys = string.Empty;
        }

        private string SetDeclarationKeys()
        {
            string returnValue = string.Empty;
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                if (!kvp.Key.StartsWith("VERSION_"))
                {
                    returnValue = string.Concat(returnValue, GetDatatableSqlBase.GetSqlBaseType(kvp.Key, kvp.Value.Type, 0, false), " ", kvp.Key, ",");
                }
            }

            if (!returnValue.Equals(string.Empty))
                return returnValue.Substring(0, returnValue.Length - 1);
            else return string.Empty;
        }

        private string SetParametersKeys()
        {
            string returnValue = string.Empty;
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                if (!kvp.Key.StartsWith("VERSION_"))
                {
                    returnValue = string.Concat(returnValue, "parameter", kvp.Key, ",");
                }
            }

            if (!returnValue.Equals(string.Empty))
                return returnValue.Substring(0, returnValue.Length - 1);
            else return string.Empty;
        }

        private string SetParametersKeysWithOutType()
        {
            string returnValue = string.Empty;
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                returnValue = string.Concat(returnValue, kvp.Key, ",");
            }

            if (!returnValue.Equals(string.Empty))
                return returnValue.Substring(0, returnValue.Length - 1);
            else return string.Empty;
        }

        private string SetNullKeysCondtition()
        {
            string returnValue = string.Empty;
            foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
            {
                switch (GetDatatableSqlBase.GetSqlBaseType(kvp.Key, kvp.Value.Type, 0, false))
                {
                    case "string":
                        returnValue = string.Concat(returnValue, "(!", kvp.Key, ".Equals(string.Empty)) && ");
                        break;
                    case "DateTime?":
                        returnValue = string.Concat(returnValue, "(DateTime.Parse(", kvp.Key, ".ToString()) != Null.MinDate) && ");
                        break;
                    default: returnValue = string.Concat(returnValue, "(", kvp.Key, " != 0", ") && "); break;
                }
            }

            if (!returnValue.Equals(string.Empty))
            {
                returnValue = returnValue.Substring(0, returnValue.Length - 3).Trim();
                returnValue = string.Concat("(", returnValue, ")");
                return returnValue;
            }
            else return string.Empty;
        }

        private string GetParametersDictionaryList(Dictionary<string, KeyColumn> columns, string tableName2)
        {
            string parameters = string.Empty;

            foreach (KeyValuePair<string, KeyColumn> column in columns)
            {
                string columnName = column.Key;
                string dbType = string.Empty;
                dbType = GetDBType(column.Value.Type);

                if (!column.Value.Size.ToString().Equals("0"))
                    dbType = string.Concat(dbType, ", ", column.Value.Size);

                parameters = string.Concat(parameters, "\n");
                parameters = string.Format("{0}\t\t\tSQLBaseParameter parameter{1} = new SQLBaseParameter(\"{1}\", DbType.{2}); \n", parameters, GetPublic(column.Key), dbType);
                parameters = string.Concat(parameters, "\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(column.Key), ", ", GetPublic(column.Key), "); \n");

            }
            return parameters;
        }

        #endregion

        private string GetDeclareRelationship(string className)
        {
            DataTable dt = GetDatatableSqlBase.GetRelationship(className);

            if (dt.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\t\tDictionary<string, Dictionary<string, string>> relationship = new Dictionary<string, Dictionary<string, string>>() {");

                foreach (DataRow dr in dt.Rows)
                    sb.Append(string.Concat("\n\t\t {\"", dr["REFDTBNAME"].ToString(), "\",new Dictionary<string,string>() { {\"", dr["REFSCOLUMN"].ToString(), "\",\"", dr["REFDCOLUMN"].ToString(), "\"}} },"));

                sb.Append("\n\t\t};");
                return sb.ToString();
            }
            else return string.Empty;
        }

        private string GetDeclareIndex(string className)
        {
            DataTable dt = GetDatatableSqlBase.GetIndex(className);

            if (dt.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\t\tDictionary<string, Dictionary<string, string>> index = new Dictionary<string, Dictionary<string, string>>() {");

                foreach (DataRow dr in dt.Rows)
                {
                    //sb.Append(string.Format("\n\t\t //{{0},new Dictionary<string,string>() { {{1},{2}}} }", dr["REFDTBNAME"].ToString(), dr["REFSCOLUMN"].ToString(), dr["REFDCOLUMN"].ToString()));                     
                    sb.Append(string.Concat("\n\t\t {\"", dr["SYSKEYS.IXNAME"].ToString(), "\",new Dictionary<string,string>() { {\"", dr["SYSINDEXES.TBNAME"].ToString(), " \",\"", dr["SYSKEYS.COLNAME"].ToString(), "\"}} },"));
                }

                sb.Append("\n\t\t};");
                return sb.ToString();
            }
            else return string.Empty;
        }

        #region ParameterList

        private void GetParametersListAll(DataTable dt, ref string parameterList, ref string parameterListDelete, ref string parameterListClone,ref string parameters, Provider provider)
        {
            string dbType = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (!dr["Name"].ToString().StartsWith("VERSION_"))
                {
                    //GetParametersList
                    if (Session.Keys.ContainsKey(dr["Name"].ToString()))
                        parameterListDelete = string.Concat(parameterListDelete, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameter", GetPublic(dr["Name"].ToString()), ", \n");
                    
                    parameterListClone = string.Concat(parameterListClone, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameter", GetPublic(dr["Name"].ToString()), ", \n");
                    parameterList = string.Concat(parameterList, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameter", GetPublic(dr["Name"].ToString()), ", \n");

                    //GetParameters
                    dbType = GetDBType(dr["DbType"].ToString());
                    if (!dr["Length"].ToString().Equals("0"))
                        dbType = string.Concat(dbType, ", ", dr["Length"].ToString());

                    //if ((Session.IsCustomDecimal) && dbType.Equals("Decimal, 1"))
                    //    dbType = "Boolean";

                    parameters = string.Concat(parameters, "\n");
                    parameters = string.Format("{0}\t\t\t\tSQLBaseParameter parameter{1} = new SQLBaseParameter(\"{1}\", DbType.{2}); \n", parameters, GetPublic(dr["Name"].ToString()), dbType);

                    if (Session.TableKey == dr["Name"].ToString())
                    {
                        if (provider == Provider.SqlBase)
                        {
                            parameters = string.Concat(parameters, "\t\t\t\tif (operation == EventContext.Insert) \n");
                            parameters = string.Concat(parameters, "\t\t\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", GetLastId(\"", GetPublic(dr["Name"].ToString()), "\")); \n");
                            parameters = string.Concat(parameters, "\t\t\t\telse DataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", GetPublic(dr["Name"].ToString()), "); \n");
                        }
                        else
                            parameters = string.Concat(parameters, "\t\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", GetPublic(dr["Name"].ToString()), "); \n");
                    }
                    else
                    {
                        if (dr["Name"].ToString().Equals("N_UPD_VER"))
                        {
                            parameters = string.Concat(parameters, "\t\t\t\tif (operation == EventContext.Update) \n");
                            parameters = string.Concat(parameters, "\t\t\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", GetLastUpdVer()); \n");
                            parameters = string.Concat(parameters, "\t\t\t\telse DataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", 1); \n");
                        }
                        else
                            parameters = string.Concat(parameters, "\t\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", GetPublic(dr["Name"].ToString()), "); \n");
                    }

                }
            }
            parameterList = parameterList.Substring(0, parameterList.Length - 3);
            parameterListDelete = parameterListDelete.Substring(0, parameterListDelete.Length - 3);
        }

        private string GetParameters(DataTable dt, string tableName)
        {
            string parameters = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                string columnName = dr["Name"].ToString();

                if (!columnName.StartsWith("VERSION_"))
                {
                    string dbType = string.Empty;
                    dbType = GetDBType(dr["DbType"].ToString());

                    if (!dr["Length"].ToString().Equals("0"))
                        dbType = string.Concat(dbType, ", ", dr["Length"].ToString());

                    if ((Session.IsCustomDecimal) && dbType.Equals("Decimal, 1"))
                        dbType = "Boolean";

                    parameters = string.Concat(parameters, "\n");
                    parameters = string.Format("{0}\t\t\t\tSQLBaseParameter parameter{1} = new SQLBaseParameter(\"{1}\", DbType.{2}); \n", parameters, GetPublic(dr["Name"].ToString()), dbType);
                    parameters = string.Concat(parameters, "\t\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", GetPublic(dr["Name"].ToString()), "); \n");
                }
            }
            return parameters;
        }

        private string GetParametersList(DataTable dt, string tableName, bool IsDelete)
        {
            string parametersList = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                if ((!IsDelete) || (Session.Keys.ContainsKey(dr["Name"].ToString())))
                {
                    if (!dr["Name"].ToString().StartsWith("VERSION_"))
                    {
                        parametersList = string.Concat(parametersList, "\t\t\t\t\t\t\t\t\t\t\t\t\tparameter", GetPublic(dr["Name"].ToString()), ", \n");
                    }
                }
            }
            return parametersList.Substring(0, parametersList.Length - 3);
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
                //string t = "";
                //if (dr["FieldName"].ToString().Equals("G02_UPD_VER"))
                //{
                //    t = "a";
                //    publicField = string.Empty;
                //}

                //if (dr["FieldName"].ToString().Equals("G02_E02_INAD"))
                //{
                //    t = "b";
                //    publicField = string.Empty;
                //}

                if (!dr["FieldName"].ToString().StartsWith("VERSION_"))
                {
                    publicField = string.Concat(publicField, "\t\t", "///<summary>\n");
                    //publicField = string.Concat(publicField, "\t\t", "///", string.Format("{0} ({1}) - {2} {3} {4}", dr["FieldName"].ToString(), dr["BusinessName"], dr["DbType"].ToString(), dr["Length"].ToString(), dr["Nullable"].ToString()));
                    publicField = string.Concat(publicField, "\t\t", "///", string.Format("{0} ({1}) - {2} {3} {4}", dr["FieldName"].ToString(), "", dr["DbType"].ToString(), dr["Length"].ToString(), dr["Nullable"].ToString()));
                    publicField = string.Concat(publicField, "\n\t\t///</summary>");
                    publicField = string.Concat(publicField, "\n\t\tpublic ");
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
            }
            return publicField;
        }

        private string GetPublicFieldForSuperClass(DataTable dt, string tableName)
        {
            string publicField = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                if (!dr["FieldName"].ToString().StartsWith("VERSION_"))
                {
                    publicField = string.Concat(publicField, "\t\t", "///<summary>\n");
                    publicField = string.Concat(publicField, "\t\t", "///", string.Format("{0} ({1}) - {2} {3} {4}", dr["FieldName"].ToString(), dr["BusinessName"], dr["DbType"].ToString(), dr["Length"].ToString(), dr["Nullable"].ToString()));
                    publicField = string.Concat(publicField, "\n\t\t///</summary>");
                    publicField = string.Concat(publicField, "\n\t\tpublic ");
                    publicField = string.Concat(publicField, GetNetType(dr["Type"].ToString()), " ");

                    if (Session.Framework == Framework.net20)
                        publicField = string.Concat(publicField, tableName, "_", GetPublic(dr["Name"].ToString()), " \n");
                    if (Session.Framework == Framework.net35)
                        publicField = string.Concat(publicField, tableName, "_", GetPublic(dr["Name"].ToString()), " { get; set; } \n");

                    if (Session.Framework == Framework.net20)
                    {
                        publicField = string.Concat(publicField, "\t\t{ \n ");
                        publicField = string.Concat(publicField, "\t\t\tget { return ", GetPrivate(dr["Name"].ToString()), "; } \n ");
                        publicField = string.Concat(publicField, "\t\t\tset { ", GetPrivate(dr["Name"].ToString()), " = value; } \n ");
                        publicField = string.Concat(publicField, "\t\t} \n ");
                    }
                }
            }
            return publicField;
        }

        private string GetMapping(DataTable dt)
        {
            string mapping = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                if (!dr["FieldName"].ToString().StartsWith("VERSION_"))
                {
                    string fieldName = GetPublic(dr["Name"].ToString());
                    if (keyCode == string.Empty) keyCode = fieldName;

                    if (!(bool)dr["Nullable"])
                    {
                        if ((dr["Type"].ToString().Equals("bool")) && Session.IsCustomDecimal)
                        {
                            mapping = string.Concat(mapping, "\t\t\tdecimal ", dr["Name"].ToString(), " = (decimal)dataReader[\"", dr["Name"].ToString(), "\"];\n");
                            mapping = string.Concat(mapping, "\t\t\tif (", dr["Name"].ToString(), " != Null.Int) item.", dr["Name"].ToString(), " = true;\n");
                            mapping = string.Concat(mapping, "\t\t\telse item.", dr["Name"].ToString(), " = false;\n");
                        }
                        else
                        {
                            if (dr["DbType"].ToString().Equals("TIME"))
                            {
                                mapping = string.Concat(mapping, "\t\t\titem.", fieldName, " = ");
                                mapping = string.Concat(mapping, "new DateTime(2000, 1, 1).Add((TimeSpan)dataReader[\"");
                                mapping = string.Concat(mapping, dr["Name"].ToString(), "\"]); \n");
                            }
                            else
                            {
                                //mapping = string.Concat(mapping, "\t\t\t", GetNetTypeNullable(GetNetType(dr["Type"].ToString())), " ", dr["Name"].ToString(), " = (decimal)dataReader[\"", dr["Name"].ToString(), "\"];\n");
                                //mapping = string.Concat(mapping, "\t\t\tif (", dr["Name"].ToString(), " != Null.Int) item.", dr["Name"].ToString(), " = true;\n");
                                //mapping = string.Concat(mapping, "\t\t\telse item.", dr["Name"].ToString(), " = false;\n");

                                mapping = string.Concat(mapping, "\t\t\titem.", fieldName, " = ");
                                mapping = string.Concat(mapping, "(", GetNetType(dr["Type"].ToString()), ")", "dataReader[\"");
                                mapping = string.Concat(mapping, dr["Name"].ToString(), "\"]; \n");
                            }
                        }
                    }
                    else
                    {
                        if ((dr["Type"].ToString().Equals("bool")) && Session.IsCustomDecimal)
                        {
                            mapping = string.Concat(mapping, "\t\t\tdecimal ", dr["Name"].ToString(), " = dataReader[\"", dr["Name"].ToString(), "\"] == DBNull.Value ? Null.Int : (decimal)dataReader[\"", dr["Name"].ToString(), "\"];\n");
                            mapping = string.Concat(mapping, "\t\t\tif (", dr["Name"].ToString(), " != Null.Int) item.", dr["Name"].ToString(), " = true;\n");
                            mapping = string.Concat(mapping, "\t\t\telse item.", dr["Name"].ToString(), " = false;\n");
                        }
                        else
                        {
                            if (dr["DbType"].ToString().Equals("TIME"))
                            {
                                mapping = string.Concat(mapping, "\t\t\tDateTime? ", dr["Name"].ToString(), " = null;\n");
                                mapping = string.Concat(mapping, "\t\t\tif (dataReader[\"", dr["Name"].ToString(), "\"] != DBNull.Value) ", dr["Name"].ToString(), " = new DateTime(2000, 1, 1).Add((TimeSpan)dataReader[\"", dr["Name"].ToString(), "\"]); \n");
                                mapping = string.Concat(mapping, "\t\t\titem.", dr["Name"].ToString(), " = ", dr["Name"].ToString(), ";\n");
                            }
                            else
                            {
                                string nullOperator = "null";
                                if (Session.NullLogic)
                                {
                                    nullOperator = GetNullOperator(dr["Type"].ToString());
                                    mapping = string.Format("{0}\t\t\titem.{1} = dataReader[\"{2}\"] == DBNull.Value ? {3} : ({4})dataReader[\"{2}\"]; \n", mapping, fieldName, dr["Name"].ToString(), nullOperator, dr["Type"].ToString());
                                }
                                else 
                                {
                                    mapping = string.Format("{0}\t\t\tif(dataReader[\"{2}\"] == DBNull.Value)\n\t\t\t\titem.{1} = null;\n\t\t\telse item.{1} = ({3})dataReader[\"{2}\"]; \n", mapping, fieldName, dr["Name"].ToString(), dr["Type"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            return mapping;
        }

        private string GetNetTypeNullable(string typeNormal)
        {
            if (typeNormal.Contains("?"))
                return typeNormal;

            if (typeNormal.Contains("tring"))
                return typeNormal;

            return string.Concat(typeNormal, "?");
        }

        private Dictionary<string, KeyColumn> GetKeys(string tableName)
        {
            Dictionary<string, KeyColumn> keys = new Dictionary<string, KeyColumn>();

            string query = string.Format("SELECT A.COLNAME,B.COLTYPE,B.LENGTH FROM SYSPKCONSTRAINTS A,SYSCOLUMNS B WHERE TBNAME = '{0}' AND B.NAME= A.COLNAME AND B.TBNAME= A.NAME", tableName);
            DataTable dt = SqlHelperSqlBase.ExecuteDataTable(Session.ConnectionString, CommandType.Text, query);

            KeyColumn column = new KeyColumn();
            string tableNamePrevious = string.Empty;

            //column = new KeyColumn();
            //column.Name = "B07_SN_PK";
            //column.Type = "INTEGER";
            //column.Size = 4;          
            //keys.Add(column.Name, column);

            foreach (DataRow dr in dt.Rows)
            {
                column = new KeyColumn();
                column.Name = dr["A.COLNAME"].ToString();
                column.Type = dr["B.COLTYPE"].ToString();
                column.Size = int.Parse(dr["B.LENGTH"].ToString());
                column.NetType = GetDBType(column.Type);
                if (!keys.ContainsKey(column.Name))
                    keys.Add(column.Name, column);
            }

            return keys;
        }

        private string GetDeclareParametersIndex(Dictionary<string, string> dictionary)
        {
            string parameters = string.Empty;
            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                if (!parameters.Equals(string.Empty))
                    parameters = string.Concat(parameters, ",");
                parameters = string.Concat(parameters, kvp.Value, " ", kvp.Key);
            }

            return parameters;
        }

        private string GetDeclareParametersIndexKeys(Dictionary<string, string> dictionary, DataTable dt)
        {
            string parameters = string.Empty;
            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                if (!parameters.Equals(string.Empty))
                    parameters = string.Concat(parameters, "\n\n");

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["Name"].ToString().Equals(kvp.Key))
                    {
                        parameters = string.Format("{0}\t\t\tSQLBaseParameter parameter{1} = new SQLBaseParameter(\"{1}\", DbType.{2}); \n", parameters, GetPublic(dr["Name"].ToString()), GetDBType(dr["DbType"].ToString()));
                        parameters = string.Concat(parameters, "\t\t\tDataHelper.ManageParameter(ref parameter", GetPublic(dr["Name"].ToString()), ", ", GetPublic(dr["Name"].ToString()), "); \n");
                    }
                }
            }
            return parameters;
        }

        private string GetDeclareParametersIndexList(Dictionary<string, string> dictionary)
        {
            string parameterList = string.Empty;

            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                if (!parameterList.Equals(string.Empty))
                    parameterList = string.Concat(parameterList, ",");

                parameterList = string.Concat(parameterList, "parameter", kvp.Key);
            }

            return parameterList;
        }

        private string GetDeclareParametersIndexKeyWhere(Dictionary<string, string> dictionary)
        {
            string parameterList = string.Empty;

            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                if (!parameterList.Equals(string.Empty))
                    parameterList = string.Concat(parameterList, " AND ");

                parameterList = string.Concat(parameterList, kvp.Key, " = :", kvp.Key);
            }

            return parameterList;
        }

        #region Function String

        private string GetPrivate(string text)
        {
            return string.Concat("_",text.Substring(0, 1).ToLower(), text.Substring(1));
        }

        private string GetDBType(string type)
        {
            switch (type)
            {
                case "NVARCHAR": return "String";
                case "NCHAR": return "String";
                case "CHAR": return "String";
                case "VARCHAR": return "String";
                case "BOOL": return "Boolean";
                case "DATE": return "DateTime";
                case "DATETIME": return "DateTime";
                case "TIME": return "DateTime";
                case "INTEGER": return "Int32";
                case "SMALLINT": return "Int16";
                case "DECIMAL": return "Decimal";
                case "LONGVAR": return "String"; //"Binary";
                case "TEXT": return "Text";
                case "FLOAT": return "Double";
                case "TIMESTMP": return "DateTime";
                case "ntext": return "NText";
                case "smallint": return "Int16";
                case "tinyint": return "TinyInt";
                case "uniqueidentifier": return "UniqueIdentifier";
                case "smalldatetime": return "SmallDateTime";
                case "varbinary": return "VarBinary";
                case "TIMESTAMP": return "String"; //"Binary";
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
            return text;
        }

        private string GetNullOperator(string type)
        {
            switch (type)
            {
                case "bool": return "false";
                case "int": return "Null.Int";
                case "Single": return "Null.Int";
                case "Double": return "Null.Int";
                case "double": return "Null.Int";
                case "byte": return "Null.ByteNull";
                case "decimal": return "Null.DecimalNull";
                case "short": return "Null.ShortNull";
                case "Int16": return "Null.ShortNull";
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
                default: return string.Empty;
            }
        }

        private string ConversionType(string type)
        {
            switch (type)
            {
                case "bool": return "System.Boolean";
                case "string": return "System.String";
                case "DateTime?": return "System.DateTime";
                case "int": return "System.Int32";
                case "Int16": return "System.Int16";
                case "double": return "System.Double";
                case "decimal": return "System.Decimal";
                case "real": return "Null.Decimal";
                case "float": return "Null.Double";
                default: return "System.String";
            }
        }

        #endregion
    }

   
}