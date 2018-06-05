using System;
using System.Collections;
using System.Configuration;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ModelCreator
{
    public static class GetDatatableSqlServer
    {
        private static SqlDataReader GetReader(string filter)
        {
            string sqlDatabase = "SELECT table_name, column_name, data_type,CHARACTER_MAXIMUM_LENGTh, IS_NULLABLE FROM information_schema.columns WHERE table_name in (select table_name	FROM Information_Schema.Tables WHERE Table_Type='Base Table')AND table_name NOT LIKE '%xxx_%' AND table_name NOT LIKE '%aspnet_%' AND table_name NOT LIKE '%dashCommerce_%' AND table_name NOT LIKE '%sysdiagrams%' {0} ORDER BY table_name";            

            if (filter.Equals(string.Empty))
                sqlDatabase = string.Format(sqlDatabase, string.Empty);
            else sqlDatabase = string.Format(sqlDatabase, string.Format(" AND table_name LIKE '%{0}'",filter));

            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(Session.ConnectionString);
                if (connection == null)
                    return null;
                SqlDataReader dr = SqlHelperSqlServer.ExecuteReader(connection, CommandType.Text, sqlDatabase);
                if (dr.HasRows)
                    return dr;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static DataTable GetView()
        {
            string sqlDatabase = "SELECT distinct(table_name) FROM information_schema.columns WHERE table_name in (select table_name	FROM Information_Schema.Tables WHERE Table_Type !='Base Table')AND table_name NOT LIKE '%xxx_%' AND table_name NOT LIKE '%aspnet_%' AND table_name NOT LIKE '%dashCommerce_%'  ORDER BY table_name";
                        
            try
            {                
                DataTable dt = SqlHelperSqlServer.ExecuteDataTable(Session.ConnectionString, CommandType.Text, sqlDatabase);
                return dt;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public static DataTable GetTable()
        {
            string sqlDatabase = "SELECT distinct(table_name) FROM information_schema.columns WHERE table_name in (select table_name	FROM Information_Schema.Tables WHERE Table_Type='Base Table')AND table_name NOT LIKE '%xxx_%' AND table_name NOT LIKE '%aspnet_%' AND table_name NOT LIKE '%sysdiagrams%'  AND table_name NOT LIKE '%dtproperties%' AND table_name NOT LIKE '%dashCommerce_%' ORDER BY table_name";

            try
            {
                DataTable dt = SqlHelperSqlServer.ExecuteDataTable(Session.ConnectionString, CommandType.Text, sqlDatabase);
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static DataTable GetStoredProcedure(string filter)
        {
            string sqlDatabase = "SELECT * FROM SYS.procedures WHERE name NOT LIKE 'dt_%' and name  NOT LIKE 'sp_%' {0} ORDER BY name";

            if (filter.Equals(string.Empty))
                sqlDatabase = string.Format(sqlDatabase, string.Empty);
            else sqlDatabase = string.Format(sqlDatabase, string.Format(" AND name LIKE '{0}%'", filter));

            try
            {
                DataTable dt = SqlHelperSqlServer.ExecuteDataTable(Session.ConnectionString, CommandType.Text, sqlDatabase);
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public static string TestView(string viewName)
        {
            string sqlDatabase = string.Format("SELECT * FROM dbo.{0}", viewName); ;

            if (viewName.StartsWith("viewHomePageTiles"))
            {

            }

            try
            {
                DataTable dt = SqlHelperSqlServer.ExecuteDataTable(Session.ConnectionString, CommandType.Text, sqlDatabase);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }  

        public static ArrayList GetSchema(string filter)
        {
            SqlDataReader dr = GetReader(filter);

            ArrayList lobjTables = new ArrayList();
            ArrayList tables = new ArrayList();

            if (dr != null)
            {
                string oldTableName = string.Empty;
                DataTable table = GetDatatableSchema();                

                int j = 0;
                while (dr.Read())
                {
                    if (!dr.GetString(0).Equals("dtproperties"))
                    {
                        string newTableName = dr.GetString(0);
                        string columnsName = dr.GetString(1);
                        
                        string columnsDBType = dr.GetString(2);                                        
                        int columnsMaxLength = dr[3].ToString().Equals(string.Empty) ? 0 : Int32.Parse(dr[3].ToString());
                        bool columnsNullable = dr.GetString(4).ToUpper().Equals("YES") ? true : false;
                        string columnsType = GetSystemType(dr.GetString(2), columnsNullable);

                        if (oldTableName != newTableName)
                        {
                            if (oldTableName != string.Empty)
                            {
                                tables.Add(table);                                
                                j++;
                                table = table = GetDatatableSchema();
                            }
                        }

                        DataRow row = table.NewRow();
                        row["Name"] = columnsName;
                        row["Type"] = columnsType;
                        row["DbType"] = columnsDBType;
                        row["Length"] = columnsMaxLength;
                        row["Nullable"] = columnsNullable;
                        row["Table"] = newTableName;
                        table.Rows.Add(row);

                        oldTableName = newTableName;
                    }
                }
                tables.Add(table);
                return tables;
            }
            return null;
         }

        public static ArrayList GetSchemaPlus(SqlDataReader dr)
        {
            ArrayList lobjTables = new ArrayList();
            ArrayList tables = new ArrayList();

            if (dr != null)
            {
                string oldTableName = string.Empty;
                DataTable table = GetDatatableSchemaPlus();

                int j = 0;
                while (dr.Read())
                {
                    if (!dr.GetString(0).Equals("dtproperties"))
                    {
                        string newTableName = dr.GetString(0);
                        string columnsName = dr.GetString(1);
                        
                        
                        string columnsDBType = dr.GetString(2);
                        int columnsMaxLength = dr[3].ToString().Equals(string.Empty) ? 0 : Int32.Parse(dr[3].ToString());
                        bool columnsNullable = dr.GetString(4).ToUpper().Equals("YES") ? true : false;
                        string columnsType = GetSystemType(dr.GetString(2), columnsNullable);

                        if (oldTableName != newTableName)
                        {
                            if (oldTableName != string.Empty)
                            {
                                tables.Add(table);
                                j++;
                                table = table = GetDatatableSchemaPlus();
                            }
                        }

                        DataRow row = table.NewRow();
                        row["Name"] = columnsName;
                        row["Type"] = columnsType;
                        row["DbType"] = columnsDBType;
                        row["Length"] = columnsMaxLength;
                        row["Nullable"] = columnsNullable;
                        row["PrimaryKey"] = false;
                        row["ForeignKey"] = GetInfoFieldSqlServer.IsForeignKey(newTableName,columnsName);                                                
                        row["Key"] = false;                        
                        row["Table"] = newTableName;
                        table.Rows.Add(row);

                        oldTableName = newTableName;
                    }
                }
                tables.Add(table);
                return tables;
            }
            return null;
        }

        public static ArrayList GetSchemaWebControls(SqlDataReader dr)
        {
            ArrayList lobjTables = new ArrayList();
            ArrayList tables = new ArrayList();

            if (dr != null)
            {
                string oldTableName = string.Empty;
                DataTable table = GetDatatableSchemaPlus();

                int j = 0;
                while (dr.Read())
                {
                    if (!dr.GetString(0).Equals("dtproperties"))
                    {
                        string newTableName = dr.GetString(0);
                        string columnsName = dr.GetString(1);
                        
                        string columnsDBType = dr.GetString(2);
                        int columnsMaxLength = dr[3].ToString().Equals(string.Empty) ? 0 : Int32.Parse(dr[3].ToString());
                        bool columnsNullable = dr.GetString(4).ToUpper().Equals("YES") ? true : false;
                        string columnsType = GetSystemType(dr.GetString(2), columnsNullable);

                        string labelName = string.Empty;
                        bool isSearch = false;
                        int searchPosition = 0;
                        bool isDetail = false;
                        int detailPosition = 0;

                        if (oldTableName != newTableName)
                        {
                            if (oldTableName != string.Empty)
                            {
                                tables.Add(table);
                                j++;
                                table = GetDatatableSchemaWebControls();
                            }
                        }

                        GetPlusInformation(newTableName, columnsName, ref labelName, ref isSearch, ref searchPosition, ref isDetail, ref detailPosition);

                        DataRow row = table.NewRow();
                        row["Name"] = columnsName;
                        row["Type"] = columnsType;
                        row["DbType"] = columnsDBType;
                        row["Length"] = columnsMaxLength;
                        row["Nullable"] = columnsNullable;
                        row["LabelName"] = labelName;
                        row["isSearch"] = isSearch;
                        row["SearchPosition"] = searchPosition;
                        row["isDetail"] = isSearch;
                        row["DetailPosition"] = searchPosition;
                        row["Table"] = newTableName;
                        table.Rows.Add(row);

                        oldTableName = newTableName;
                    }
                }
                tables.Add(table);
                return tables;
            }
            return null;
        }     

        public static string GetSystemType(string tstrSqlType, bool isNullable)
        {
            string _Type = string.Empty;
            bool isNumeric = false;

            switch (tstrSqlType)
            {
                case "biginit":
                    {
                        _Type = "long";
                        isNumeric = true;
                    } break;
                case "smallint":
                    {
                        _Type = "Int16";
                        isNumeric = true;
                    } break;
                case "tinyint":
                    {
                        _Type = "byte";                        
                    } break;
                case "int":
                    {
                        _Type = "int";
                        isNumeric = true;
                    } break;
                case "bit":
                    {
                        _Type = "bool";                        
                    } break;
                case "decimal":
                case "numeric":
                    {
                        _Type = "decimal";
                        isNumeric = true;
                    } break;
                case "money":
                case "smallmoney":
                    {
                        _Type = "decimal";
                        isNumeric = true;
                    } break;
                case "float":
                case "real":
                    {
                        _Type = "float";
                        isNumeric = true;
                    } break;
                case "time":
                    {
                        _Type = "TimeSpan?";
                    }break;
                case "date":
                case "datetime":
                case "smalldatetime":
                    {
                        _Type = "DateTime?";                        
                    } break;
                case "char":
                    {
                        _Type = "string";                        
                    } break;
                case "sql_variant":
                    {
                        _Type = "object";                        
                    } break;
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    {
                        _Type = "string";                        
                    } break;
                case "binary":
                case "varbinary":
                    {
                        _Type = "byte[]";                        
                    } break;
                case "image":
                    {
                        _Type = "System.Drawing.Image";                        
                    } break;
                case "timestamp":
                    {
                       _Type = "byte[]";
                    } break;
                case "uniqueidentifier":
                    {
                        _Type = "Guid";                        
                    } break;
                default:
                    {
                        _Type = "unknown";                        
                    } break;
            }

            if ((!Session.NullLogic) && (isNullable))
            {
                if (isNumeric)
                    _Type = string.Concat(_Type, "?");
            }

            return _Type;
        }
        
        public static DataTable GetDatatableSchema()
        {
            DataTable dt = new DataTable();

            DataColumn[] keys = new DataColumn[1];
            DataColumn column;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Name";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Type";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "DbType";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Length";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Boolean");
            column.ColumnName = "Nullable";
            dt.Columns.Add(column);
                        
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Table";
            dt.Columns.Add(column);

            return dt;
        }

        public static DataTable GetDatatableSchemaPlus()
        {
            DataTable dt = new DataTable();

            DataColumn[] keys = new DataColumn[1];
            DataColumn column;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Name";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Type";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "DbType";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Length";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Boolean");
            column.ColumnName = "Nullable";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Boolean");
            column.ColumnName = "PrimaryKey";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "ForeignKey";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Boolean");
            column.ColumnName = "Key";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Table";
            dt.Columns.Add(column);

            return dt;
        }

        public static DataTable GetDatatabaseItem()
        {
            DataTable dt = new DataTable();

            DataColumn[] keys = new DataColumn[1];
            DataColumn column;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Name";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Table";
            dt.Columns.Add(column);

            return dt;
        }

        public static DataTable ConvertDataViewInDataTable(DataView obDataView)
        {
            if (null == obDataView)
            {
                throw new ArgumentNullException
                ("DataView", "Invalid DataView object specified");
            }

            DataTable obNewDt = obDataView.Table.Clone();
            int idx = 0;
            string[] strColNames = new string[obNewDt.Columns.Count];
            foreach (DataColumn col in obNewDt.Columns)
            {
                strColNames[idx++] = col.ColumnName;
            }

            IEnumerator viewEnumerator = obDataView.GetEnumerator();
            while (viewEnumerator.MoveNext())
            {
                DataRowView drv = (DataRowView)viewEnumerator.Current;
                DataRow dr = obNewDt.NewRow();
                try
                {
                    foreach (string strName in strColNames)
                    {
                        dr[strName] = drv[strName];
                    }
                }
                catch (Exception ex)
                {
                }
                obNewDt.Rows.Add(dr);
            }

            return obNewDt;
        }

        public static DataTable GetDatatableSchemaWebControls()
        {
            DataTable dt = new DataTable();

            DataColumn[] keys = new DataColumn[1];
            DataColumn column;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Name";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Type";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "DbType";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Length";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Boolean");
            column.ColumnName = "Nullable";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "LabelName";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Boolean");
            column.ColumnName = "isSearch";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "SearchPosition";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Boolean");
            column.ColumnName = "isDetail";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "DetailPosition";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Table";
            dt.Columns.Add(column);

            return dt;
        }

        public static void GetPlusInformation(string tablename, string columnname,ref string labelName, ref bool isSearch, ref int searchPosition, ref bool isDetail, ref int detailPosition)
        {
            SqlDataReader reader = GetReaderInformationPlus(tablename, columnname);

            string information = string.Empty;

            if (reader != null)
            {
                while (reader.Read())
                {
                    information = reader["DescriptionPlus"].ToString().Trim();

                    if (information.Equals(string.Empty))
                    {
                        if (information.IndexOf("*") != -1)
                        {

                        }
                        else columnname = reader["DescriptionPlus"].ToString();
                    }
                    else
                    {                        
                        labelName = information;
                        labelName = columnname;
                    }

                }
                reader.Close();
            }
        }

        private static SqlDataReader GetReaderInformationPlus(string tableName, string columnName)
        {
            string sqlDatabase = string.Format("SELECT value as DescriptionPlus FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '{0}', 'column', '{1}')", tableName, columnName);
            
            SqlConnection connection = new SqlConnection(Session.ConnectionString);

            try
            {                
                if (connection == null)
                    return null;
                SqlDataReader dr = SqlHelperSqlServer.ExecuteReader(connection, CommandType.Text, sqlDatabase);
                if (dr.HasRows)
                    return dr;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

    }
}