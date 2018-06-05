using System;
using System.Collections;
using System.Data;
using Gupta.SQLBase.Data;
using System.Collections.Generic;

namespace ModelCreator
{
    public static class GetDatatableSqlBase
    {
        public static Dictionary<string, int> IndexKeys { get; set; }

        public static ArrayList GetListTables(string filter)
        {
            ArrayList list = new ArrayList();

            try
            {
                DataTable dt = SqlHelperSqlBase.ExecuteDataTable(Session.ConnectionString, CommandType.Text, "select * from systables order by 1,2");

                foreach (DataRow dr in dt.Rows)
                {
                    if ((dr["CREATOR"].ToString() != "SYSADM") && (dr["CREATOR"].ToString() != "SYSSQL"))
                    {
                        if (!filter.Equals(string.Empty))
                        {
                            if (dr["NAME"].ToString().StartsWith(filter))
                                list.Add(dr["NAME"].ToString());
                        }
                        else list.Add(dr["NAME"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return list;
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

        public static DataTable GetSchema(string tableName)
        {
            CustomBoolean.Clear();

            DataTable table = GetDatatableSchema();
            foreach (DataRow dr in GetColumns(tableName).Rows)
            {
                string newTableName = dr[1].ToString().Trim();
                string columnsName = dr[0].ToString().Trim();
                string schema = dr[2].ToString().Trim();
                int columnsMaxLength = dr[5].ToString().Trim().Equals(string.Empty) ? 0 : Int32.Parse(dr[5].ToString().Trim());
                bool columnsNullable = dr[7].ToString().Trim().ToUpper().StartsWith("Y") ? true : false;
                string columnsType = GetSqlBaseType(columnsName, dr[4].ToString(), columnsMaxLength, columnsNullable).Trim();
                string columnsDBType = dr[4].ToString().Trim();


                columnsNullable = dr[7].ToString().Trim().ToUpper().Equals("Y") ? true : false;
                string businessName = dr[9].ToString().Trim();
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
                //row["Name"] = businessName.Equals(string.Empty) ? columnsName : string.Empty;
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

                        string columnsType = GetSqlBaseType(columnsName, dr.GetString(2), columnsMaxLength, columnsNullable);
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

        public static string GetSqlBaseType(string name, string tstrSqlType, int lenght, bool IsNullable)
        {
            string _Type = string.Empty;
            bool isNumeric = false;

            switch (tstrSqlType)
            {
                case "BIGINT":
                    {
                        _Type = "Int64";
                        isNumeric = true;
                    } break;
                case "SMALLINT":
                    {
                        _Type = "Int16";
                        isNumeric = true;
                    } break;
                case "tinyint":
                    {
                        _Type = "byte";
                    } break;
                case "INTEGER":
                    {
                        _Type = "int";
                        isNumeric = true;
                    } break;
                case "bit":
                    {
                        _Type = "bool";
                    } break;
                case "DECIMAL":
                    {
                        if (lenght == 1)
                        {
                            _Type = "bool";
                            CustomBoolean.Add(name);
                        }
                        else
                        {
                            _Type = "decimal";
                            isNumeric = true;
                        }
                    } break;
                case "NUMBER":
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
                case "DOUBLE":
                    {
                        _Type = "double";
                        isNumeric = true;
                    } break;
                case "FLOAT":
                    {
                        if (lenght == 8)
                        {
                            _Type = "double";
                        }
                        else _Type = "float";
                        isNumeric = true;
                    } break;
                case "REAL":
                    {
                        _Type = "float";
                        isNumeric = true;
                    } break;
                case "DATE":
                case "DATETIME":
                case "SMALLDATETIME":
                case "TIME":
                    {
                        _Type = "DateTime?";
                    } break;
                case "CHAR":
                    {
                        _Type = "string";
                    } break;
                case "sql_variant":
                    {
                        _Type = "object";
                    } break;
                case "VARCHAR":
                case "TEXT":
                case "NCHAR":
                case "NVARCHAR":
                case "NTEXT":
                    {
                        _Type = "string";
                    } break;
                case "GRAPHIC":
                case "VARGRAPHIC":
                case "BINARY":
                case "VARBINARY":
                case "LONGVAR":
                case "LONGGRAPHICS":
                case "LONGBINARY":
                case "CHAR>254":
                case "VARCHAR>254":
                    {
                        //_Type = "byte[]";
                        _Type = "string";
                    } break;
                case "TIMESTMP":
                case "TIMESTAMP":
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