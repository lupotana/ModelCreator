using System;
using System.Collections;
using System.Data;
using Gupta.SQLBase.Data;
using System.Collections.Generic;
using Devart.Data.PostgreSql;

namespace ModelCreator
{
    public static class GetDatatablePostGres
    {
        public static Dictionary<string, int> IndexKeys { get; set; } 

        //POST GRES
        public static ArrayList GetListTables(string filter)
        {
            ArrayList list = new ArrayList();
            //PgSqlConnection connection = new PgSqlConnection(Session.ConnectionString);

            try
            {                
                //connection.Open();

                DataTable dt = SqlHelperPostGres.ExecuteDataTable(Session.ConnectionString, CommandType.Text, "select * from information_schema.tables WHERE table_type = 'BASE TABLE' AND TABLE_SCHEMA = 'public'", null);
                   
                foreach (DataRow dr in dt.Rows)
                    list.Add(dr[2].ToString());
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //if (connection.State == ConnectionState.Open)
                //    connection.Close();
            }

            return list;
        }

        public static void RetrieveColumnInformation()
        {
            PgSqlConnection connection = new PgSqlConnection(Session.ConnectionString);

            try
            {
                connection.Open();
                DataTable tables = new DataTable(); //connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, null);
                // Print out the columns
                Console.WriteLine("\nListing Column Metadata Information ...");
                foreach (DataColumn column in tables.Columns)
                {
                    //Console.WriteLine(column);
                }
                Console.WriteLine("\nListing Columns (TableName : ColumnName format)...");
                foreach (DataRow row in tables.Rows)
                {
                    // Console.WriteLine(row["TABLE_NAME"] + " : " + row["COLUMN_NAME"]);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

       
        public static DataTable GetColumns(string tableName)
        {
            return SqlHelperSqlBase.ExecuteDataTable(Session.ConnectionString, CommandType.Text, string.Format("SELECT * FROM SYSCOLUMNS WHERE TBNAME = '{0}'", tableName));
        }        

        public static DataTable GetRelationship(string tableName)
        {
            return SqlHelperSqlBase.ExecuteDataTable(Session.ConnectionString, CommandType.Text, string.Format("SELECT * FROM SYSFKCONSTRAINTS WHERE NAME = '{0}'", tableName));
        }

        public static DataTable GetIndex(string tableName)
        {
            return SqlHelperSqlBase.ExecuteDataTable(Session.ConnectionString, CommandType.Text, string.Format("SELECT SYSKEYS.IXNAME,SYSKEYS.COLNAME,SYSKEYS.COLSEQ,SYSINDEXES.TBNAME,SYSINDEXES.UNIQUERULE FROM SYSKEYS INNER JOIN SYSINDEXES ON SYSKEYS.IXNAME = SYSINDEXES.NAME WHERE SYSINDEXES.TBNAME = '{0}' AND SYSINDEXES.UNIQUERULE = 'U' ORDER BY SYSKEYS.IXNAME,SYSKEYS.COLSEQ", tableName));
        }

        public static DataTable GetSchema(string tableName, string columnNameParameter)
        {
            CustomBoolean.Clear();

            DataTable table = GetDatatableSchema();

            string query = string.Format("select * from information_schema.columns where table_name = '{0}' order by ordinal_position", tableName);
            DataTable columns = SqlHelperPostGres.ExecuteDataTable(Session.ConnectionString,CommandType.Text,query,null); 
            
            foreach (DataRow dr in columns.Rows)
            {
                if (dr["table_name"].ToString().Equals(tableName))
                {
                    string newTableName = dr["table_name"].ToString().Trim();
                    string columnsName = dr["column_name"].ToString().Trim();

                    bool isOk = true;
                    if (!columnNameParameter.Equals(string.Empty))
                    {
                        isOk = false;
                        if (columnNameParameter.Equals(columnsName))
                            isOk = true;
                    }

                    if (isOk)
                    {
                        string schema = dr["table_schema"].ToString().Trim();
                        int columnsMaxLength = dr["character_octet_length"].ToString().Trim().Equals(string.Empty) ? 0 : Int32.Parse(dr["character_octet_length"].ToString().Trim());
                        bool columnsNullable = dr["is_nullable"].ToString().Trim().ToUpper().StartsWith("True") ? true : false;
                        string columnsType = GetType(columnsName, dr["udt_name"].ToString().Trim(), columnsMaxLength, columnsNullable).Trim();
                        string columnsDBType = GetType(columnsName, dr["udt_name"].ToString().Trim(), columnsMaxLength, columnsNullable).Trim();

                        string businessName = dr["column_name"].ToString().Trim();
                        string description = string.Empty;
                        if (!businessName.Equals(string.Empty))
                        {
                            if (businessName.Contains("-"))
                            {
                                string[] array = businessName.Split('-');
                                businessName = array[0].Trim();
                                description = array[1].Trim();
                            }
                        }

                        DataRow row = table.NewRow();
                        row["Name"] = columnsName;
                        row["FieldName"] = columnsName;
                        row["Type"] = columnsType;
                        row["DbType"] = columnsDBType;
                        row["Length"] = columnsMaxLength;
                        row["Nullable"] = columnsNullable;
                        row["Table"] = newTableName;
                        row["Schema"] = schema;
                        row["BusinessName"] = businessName;
                        row["Description"] = description;

                        table.Rows.Add(row);
                    }
                }
            }
            return table;
        }
    
        public static ArrayList GetSchemaWebControls(SQLBaseDataReader dr)
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
                        int columnsMaxLength = dr[3].ToString().Equals(string.Empty) ? 0 : Int32.Parse(dr[3].ToString());
                        bool columnsNullable = dr.GetString(4).ToUpper().Equals("YES") ? true : false;

                        string columnsType = GetType(columnsName, dr.GetString(2), columnsMaxLength, columnsNullable);
                        string columnsDBType = dr.GetString(2);
                        
                       
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

        public static string GetType(string name, string tstrSqlType, int lenght, bool IsNullable)
        {
            string _Type = string.Empty;
            bool isNumeric = false;

            switch (tstrSqlType)
            {
                case "int4":
                    {
                        _Type = "int";
                        isNumeric = true;
                    } break;
                case "6":
                    {
                        _Type = "decimal";
                        isNumeric = true;
                    } break;

                case "11":
                    {
                        _Type = "bool";
                    } break;
                case "_char":
                    {
                        _Type = "string";
                    } break;
                case "7":
                    {
                        _Type = "DateTime?";
                    } break;
                default:
                    {
                        _Type = "unknown";
                    } break;
            }

            if ((!Session.NullLogic) && (IsNullable))
            {
                //if (isNumeric)
                //    _Type = string.Concat(_Type, "?");
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

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Schema";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "FieldName";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "BusinessName";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Description";
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

        public static void GetPlusInformation(string tablename, string columnname, ref string labelName, ref bool isSearch, ref int searchPosition, ref bool isDetail, ref int detailPosition)
        {
            SQLBaseDataReader reader = GetReaderInformationPlus(tablename, columnname);

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

        private static SQLBaseDataReader GetReaderInformationPlus(string tableName, string columnName)
        {
            string sqlDatabase = string.Format("SELECT value as DescriptionPlus FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '{0}', 'column', '{1}')", tableName, columnName);

            SQLBaseConnection connection = new SQLBaseConnection(Session.ConnectionString);

            try
            {
                if (connection == null)
                    return null;
                SQLBaseDataReader dr = SqlHelperSqlBase.ExecuteReader(connection, CommandType.Text, sqlDatabase);
                //    if (dr.HasRows)
                //        return dr;
                //    else
                return null;
            }
            catch
            {
                return null;
            }
        }

    }
}