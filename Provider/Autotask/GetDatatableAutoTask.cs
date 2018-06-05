using System;
using System.Collections;
using System.Data;
using Gupta.SQLBase.Data;
using System.Collections.Generic;
using ModelCreator.autotask;
using System.Net;

namespace ModelCreator
{
    public static class GetDatatableAutoTask
    {
        public static Dictionary<string, int> IndexKeys { get; set; }

        public static ArrayList GetListTables(string filter)
        {
            ArrayList list = new ArrayList();

            try
            {
                ATWS atws = new ATWS();
                atws.Credentials = new NetworkCredential("prusso@TecnoemaDemo.com", "12345");

                EntityInfo[] entityList = atws.getEntityInfo();

                foreach (EntityInfo entity in entityList)
                {
                    if (!filter.Equals(string.Empty))
                    {
                        if (entity.Name.ToUpper().Contains(filter.ToUpper()))
                            list.Add(entity.Name);
                    }
                    else list.Add(entity.Name);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }

        public static DataTable GetSchema(string tableName, string columnNameParameter)
        {
            CustomBoolean.Clear();

            DataTable table = GetDatatableSchema();

            ATWS atws = new ATWS();
            atws.Credentials = new NetworkCredential("prusso@TecnoemaDemo.com", "12345");
            ModelCreator.autotask.Field[] fields = atws.GetFieldInfo(tableName);

            foreach (ModelCreator.autotask.Field field in fields)
            {
                DataRow row = table.NewRow();
                row["Name"] = field.Name;
                row["FieldName"] = field.Name;
                row["Type"] = GetType(field.Type);
                row["DbType"] = GetType(field.Type);
                row["Length"] = field.Length;
                row["Nullable"] = field.IsRequired;
                row["Table"] = tableName;
                row["Schema"] = "";
                row["BusinessName"] = field.Name;
                row["Description"] = field.Description;

                table.Rows.Add(row);
            }

            return table;
        }

        private static string GetType(string type)
        {
            switch (type)
            {
                case "integer": return "int"; break;
                case "datetime": return "DateTime"; break;
                case "long": return "Int64"; break;
                case "string": return "string"; break;
                case "short": return "Int16"; break;
                case "double": return "decimal"; break;
                case "boolean": return "bool"; break;
                case "float": return "decimal"; break;

            }
            return "unkonow";
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

    }
}