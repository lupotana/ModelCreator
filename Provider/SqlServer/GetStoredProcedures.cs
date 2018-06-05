using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace ModelCreator
{
    public static class GetStoredProceduresSqlServer
    {
        public static string GetStored(SqlConnection connection, string tableName, string type)
        {
            string storedFunctions = string.Empty;

            SqlConnection conn = new SqlConnection(Session.ConnectionString);

            ArrayList storedProcedures = GetReader(conn, tableName);
            DataTable tableParameters = GetDatatableSqlServer.GetDatatableSchema();

            foreach (string storedProcedure in storedProcedures)
            {
                SqlCommand cmd = new SqlCommand(storedProcedure, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlCommandBuilder.DeriveParameters(cmd);

                foreach (SqlParameter parameter in cmd.Parameters)
                {
                    if (!parameter.ParameterName.Equals("@RETURN_VALUE"))
                    {
                        DataRow row = tableParameters.NewRow();
                        row["Name"] = parameter.ParameterName.Replace("@", string.Empty);
                        row["Type"] = GetDatatableSqlServer.GetSystemType(parameter.SqlDbType.ToString().ToLower(), parameter.IsNullable);
                        row["DbType"] = parameter.SqlDbType;
                        row["Length"] = parameter.Size;
                        row["Nullable"] = parameter.IsNullable;
                        row["Table"] = storedProcedure;
                        tableParameters.Rows.Add(row);
                    }
                }

                storedFunctions = string.Concat(storedFunctions, BuildStoredProcedureList(tableParameters, tableName, type), "\n\n\t\t");
                conn.Close();
                tableParameters.Clear();
            }

            return storedFunctions;
        }

        public static string GetStoredBusiness(SqlConnection connection, string tableName, string type)
        {
            string storedFunctions = string.Empty;

            SqlConnection conn = new SqlConnection(Session.ConnectionString);

            ArrayList storedProcedures = GetReader(conn, tableName);
            DataTable tableParameters = GetDatatableSqlServer.GetDatatableSchema();

            foreach (string storedProcedure in storedProcedures)
            {
                SqlCommand cmd = new SqlCommand(storedProcedure, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlCommandBuilder.DeriveParameters(cmd);

                foreach (SqlParameter parameter in cmd.Parameters)
                {
                    if (!parameter.ParameterName.Equals("@RETURN_VALUE"))
                    {
                        DataRow row = tableParameters.NewRow();
                        row["Name"] = parameter.ParameterName.Replace("@", string.Empty);
                        row["Type"] = GetDatatableSqlServer.GetSystemType(parameter.SqlDbType.ToString().ToLower(), parameter.IsNullable);
                        row["DbType"] = parameter.SqlDbType;
                        row["Length"] = parameter.Size;
                        row["Nullable"] = parameter.IsNullable;
                        row["Table"] = storedProcedure;
                        tableParameters.Rows.Add(row);
                    }
                }

                storedFunctions = string.Concat(storedFunctions, BuildStoredProcedureListBusiness(tableParameters, tableName, type), "\n\t\t");                
                conn.Close();
                tableParameters.Clear();
            }

            return storedFunctions;
        }

        public static ArrayList GetReaderAll(SqlConnection connection)
        {
            string storedName = string.Empty;
            ArrayList storedProcedures = new ArrayList();

            #region StartConnection
            connection.Open();
            SqlCommand cmd = new SqlCommand("Select name from sysobjects where type='P' and xtype='P' and category=0 order by name", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            #endregion

            while (reader.Read())
            {
                storedName = reader[0].ToString();
                storedProcedures.Add(storedName);
            }

            #region CloseConnection
            connection.Close();
            cmd.Dispose();
            #endregion

            return storedProcedures;
        }

        public static ArrayList GetReaderAllObjects(SqlConnection connection)
        {
            string storedName = string.Empty;
            ArrayList storedProcedures = new ArrayList();

            #region StartConnection
            connection.Open();
            SqlCommand cmd = new SqlCommand("Select name from sysobjects where (type='P' and xtype='P') OR (type='V' and xtype='V') and category=0 order by name", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            #endregion

            while (reader.Read())
            {
                storedName = reader[0].ToString();
                storedProcedures.Add(storedName);
            }

            #region CloseConnection
            connection.Close();
            cmd.Dispose();
            #endregion

            return storedProcedures;
        }


        private static ArrayList GetReader(SqlConnection connection, string tableName)
        {
            string storedName = string.Empty;
            ArrayList storedProcedures = new ArrayList();

            #region StartConnection
            connection.Open();
            SqlCommand cmd = new SqlCommand("Select name from sysobjects where type='P' and xtype='P' and category=0 order by name", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            #endregion

            while (reader.Read())
            {
                storedName = reader[0].ToString();
                
                if (storedName.Equals("GetCollection_Validity_Load"))
                {
                    string tt = "ddd";
                }

                if (storedName.Equals("GetCollection_Validity_PrevertDelete"))
                {
                    string tt = "ddd";
                }

                if ((!storedName.StartsWith("Insert")) && (!storedName.StartsWith("Update")) && (!storedName.StartsWith("Delete"))
                     && (!storedName.StartsWith("GetByKeyuni")) && (!storedName.StartsWith("GetByCode"))
                    && (storedName.Contains(string.Concat("_", tableName, "_")))
                    )
                {
                    if (!storedName.Equals(string.Concat("GetCollection", tableName)))
                    {
                        storedProcedures.Add(storedName);
                    }
                }
            }

            #region CloseConnection
            connection.Close();
            cmd.Dispose();
            #endregion

            return storedProcedures;
        }

        private static string BuildStoredProcedureList(DataTable completeStoredProcedures, string tableName, string type)
        {
            string storedFunctions = string.Empty;
            string storedName = string.Empty;
            string functionName = string.Empty;
            string template = string.Empty;

            string parameterListHeader = string.Empty;
            string parameterListBody = string.Empty;
            string parameterListCall = string.Empty;
            string parameterListSubHeader = string.Empty;

            string headerInterface = string.Empty;
            string allTemplate = string.Empty;           

            storedName = completeStoredProcedures.Rows[0]["Table"].ToString();
            functionName = storedName.Replace(string.Concat("_", tableName, "_"), string.Empty);

            foreach (DataRow parameter in completeStoredProcedures.Rows)
            {
                if (parameterListHeader != string.Empty) parameterListHeader = string.Concat(parameterListHeader, ", ");
                parameterListHeader = string.Concat(parameterListHeader, parameter["Type"], " ", GetPrivate(parameter["Name"].ToString()));

                if (parameterListCall != string.Empty) parameterListCall = string.Concat(parameterListCall, ", ");
                parameterListCall = string.Concat(parameterListCall, "parameter", parameter["Name"]);

                if (parameterListSubHeader != string.Empty) parameterListSubHeader = string.Concat(parameterListSubHeader, ", ");
                parameterListSubHeader = string.Concat(parameterListSubHeader, GetPrivate(parameter["Name"].ToString()));

                string suffixTable = string.Empty;
                if (storedName.StartsWith("GetCollection")) suffixTable = "Collection";
                headerInterface = string.Concat(tableName, suffixTable, " ", functionName, "(", parameterListHeader, ");");
            }

            template = string.Empty;

            parameterListBody = GetParameters(completeStoredProcedures, storedName);

            if (Session.Modality.Equals(Modality.Professional))
            {
                if (storedName.StartsWith("GetCollection")) template = ManageTemplate.GetTemplate("Professional//GetCollectionTemplate.ico");
                if (storedName.StartsWith("GetItem")) template = ManageTemplate.GetTemplate("Professional//GetItemTemplate.ico");
            }
            else
            {
                if (storedName.StartsWith("GetCollection")) template = ManageTemplate.GetTemplate("Simple//GetCollectionTemplate.ico");
                if (storedName.StartsWith("GetItem")) template = ManageTemplate.GetTemplate("Simple//GetItemTemplate.ico");

                if (storedName.Equals(string.Format(Session.GetCustomItemSql, tableName))) template = ManageTemplate.GetTemplate("Simple//GetItemTemplate.ico");
                if (storedName.Equals(string.Format(Session.GetCustomCollectionSql, tableName))) template = ManageTemplate.GetTemplate("Simple//GetCollectionTemplate.ico");
            }

            //if (allTemplate.Equals(string.Empty)) template.Substring(2, template.Length - 8);

            template = template.Replace("***ClassName***", tableName);
            template = template.Replace("#StoredName#", storedName);
            template = template.Replace("***FunctionName***", functionName);
            template = template.Replace("***parameterListHeader***", parameterListHeader);
            template = template.Replace("***ParameterListBody***", parameterListBody);
            template = template.Replace("***ParameterListCall***", parameterListCall);            

            allTemplate = string.Concat(allTemplate, template);

            if (type.Equals("Interface")) return headerInterface;

            if (type.Equals("Manager"))
            {
                string templateManager = string.Empty;
                if (storedName.StartsWith("GetItem")) templateManager = ManageTemplate.GetTemplate("TemplateManagerBodyGetItem.ico");

                if (storedName.StartsWith("GetCollection")) templateManager = ManageTemplate.GetTemplate("TemplateManagerBodyGetCollection.ico");

                //string templateManager = ManageTemplate.GetTemplate("TemplateManagerBody.ico");
                templateManager = templateManager.Replace("***ClassName***", tableName);
                templateManager = templateManager.Replace("***FunctionName***", functionName);
                templateManager = templateManager.Replace("***parameterListHeader***", parameterListHeader);
                templateManager = templateManager.Replace("***parameterListSubHeader***", parameterListSubHeader);
                return templateManager;
            }

            return allTemplate;
        }

        private static string BuildStoredProcedureListBusiness(DataTable completeStoredProcedures, string tableName, string type)
        {
            string storedFunctions = string.Empty;
            string storedName = string.Empty;
            string functionName = string.Empty;
            string template = string.Empty;

            string parameterListHeader = string.Empty;
            string parameterListCall = string.Empty;

            string headerInterface = string.Empty;
            string allTemplate = string.Empty;

            storedName = completeStoredProcedures.Rows[0]["Table"].ToString();
            functionName = storedName.Replace(string.Concat("_", tableName, "_"), string.Empty);

            foreach (DataRow parameter in completeStoredProcedures.Rows)
            {
                if (parameterListHeader != string.Empty) parameterListHeader = string.Concat(parameterListHeader, ", ");
                parameterListHeader = string.Concat(parameterListHeader, parameter["Type"], " ", GetPrivate(parameter["Name"].ToString()));

                if (parameterListCall != string.Empty) parameterListCall = string.Concat(parameterListCall, ", ");
                parameterListCall = string.Concat(parameterListCall, GetPrivate(parameter["Name"].ToString()));           
            }

            template = string.Empty;


            if (Session.Modality == Modality.ThreeLayer)
            {
                if (storedName.StartsWith("GetCollection")) template = ManageTemplate.GetTemplate("2015ThreeLayer//GetCollectionTemplate.ico");
                if (storedName.StartsWith("GetItem")) template = ManageTemplate.GetTemplate("2015ThreeLayer//GetItemTemplate.ico");
            }

            template = template.Replace("***ClassName***", tableName);
            template = template.Replace("#StoredName#", storedName);
            template = template.Replace("***FunctionName***", functionName);
            template = template.Replace("***parameterListHeader***", parameterListHeader);            
            template = template.Replace("***ParameterListCall***", parameterListCall);       

            allTemplate = string.Concat(allTemplate, template);
           
            return allTemplate;
        }
       
        private static string GetType(string type)
        {

            return string.Empty;
        }

        private static string BuildGetItem(DataTable table)
        {
            string getFunction = string.Empty;


            return getFunction;
        }

        private static string GetParameters(DataTable dt, string storeProcedure)
        {
            string parameters = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["Table"].ToString().Equals(storeProcedure))
                {
                    string dbType = string.Empty;
                    dbType = GetDBType(dr["DbType"].ToString());

                    if (!dr["Length"].ToString().Equals("0"))
                        dbType = string.Concat(dbType, ", ", dr["Length"].ToString());

                    parameters = string.Concat(parameters, "\n");
                    parameters = string.Format("{0}\t\t\t\tSqlParameter parameter{1} = new SqlParameter(\"@{1}\", SqlDbType.{2}); \n", parameters, dr["Name"].ToString(), dbType);
                    parameters = string.Concat(parameters, "\t\t\t\tparameter", dr["Name"].ToString(), ".Value = ", GetPrivate(dr["Name"].ToString()), "; \n");
                }
            }

            return parameters;
        }

        private static string GetDBType(string type)
        {
            switch (type)
            {
                case "nvarchar": return "NVarChar";
                case "varchar": return "VarChar";
                case "bool": return "Boolean";
                case "datetime": return "DateTime";
                case "ntext": return "NText";
                case "smallint": return "SmallInt";
                case "varbinary": return "VarBinary";
                case "uniqueidentifier": return "UniqueIdentifier";
                default: return GetPublic(type);
            }
        }

        private static string GetPublic(string text)
        {
            return string.Concat(text.Substring(0, 1).ToUpper(), text.Substring(1));
        }

        private static string GetPrivate(string text)
        {
            return string.Concat(text.Substring(0, 1).ToLower(), text.Substring(1));
        }

    }
}
