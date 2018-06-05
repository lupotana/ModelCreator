using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace ModelCreator
{
    public static class GetInfoFieldSqlServer
    {
        public static bool IsIdentity(string tableName, string columnName)
        {
            ArrayList array = new ArrayList();

            string connectionString = Session.ConnectionString;

            string sql = string.Format("SELECT NAME,IDENT_SEED(OBJECT_NAME(id)) AS Seed, IDENT_INCR(OBJECT_NAME(id)) AS Increment, OBJECT_NAME(id) FROM syscolumns WHERE (status & 128) = 128 AND OBJECT_NAME(id) = '{0}' AND NAME = '{1}'", tableName, columnName);

            SqlDataReader dataReader = SqlHelperSqlServer.ExecuteReader(connectionString, CommandType.Text, sql);

            try
            {
                while (dataReader.Read())
                {
                    return true;
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally
            {
                if (!dataReader.IsClosed) dataReader.Close();
                dataReader.Dispose();
            }

            return false;
        }

        public static bool IsUniqueIdentifierOld(string tableName, string columnName)
        {
            ArrayList array = new ArrayList();

            string connectionString = Session.ConnectionString;

            string sql = string.Format("SELECT NAME FROM syscolumns WHERE (xtype = 36) AND OBJECT_NAME(id) = '{0}' AND NAME = '{1}'", tableName, columnName);

            SqlDataReader dataReader = SqlHelperSqlServer.ExecuteReader(connectionString, CommandType.Text, sql);

            try
            {
                while (dataReader.Read())
                {
                    return true;
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally
            {
                if (!dataReader.IsClosed) dataReader.Close();
                dataReader.Dispose();
            }

            return false;
        }

        public static bool IsUniqueIdentifier(string tableName, string columnName)
        {
            if (columnName.Equals(ConfigurationManager.AppSettings["NameUniqueIdentifier"])) return true;
            else return false;
        }

        //public static bool IsRowGuidOld(string tableName, string columnName)
        //{
        //    SQLDMOHelper dmoMain = new SQLDMOHelper();

        //    dmoMain.ServerName = Session.Server;
        //    dmoMain.Database = Session.Database;
        //    dmoMain.UserName = Session.Username;
        //    dmoMain.Password = Session.Password;

        //    dmoMain.Table = tableName;

        //    try
        //    {
        //        dmoMain.Connect();

        //        foreach (SQLDMO.Column column in dmoMain.Fields)
        //        {
        //            if (column.Name.Equals(columnName))
        //            {
        //                if (column.IsRowGuidCol) return true;
        //            }
        //        }
        //    }
        //    catch (Exception ex) { throw new Exception(ex.Message); }
        //    finally { dmoMain.DisConnect(); }

        //    return false;
        //}

        public static bool IsRowGuid(string tableName, string columnName)
        {
            if (columnName.Equals(ConfigurationManager.AppSettings["NameUniqueIdentifier"]))
            {
                if (ConfigurationManager.AppSettings["IsRowGuid"].Equals("true")) return true;
                else return false;
            }
            else return false;
        }

        public static string IsForeignKey(string tableName, string columnName)
        {
            string connectionString = Session.ConnectionString;

            StringBuilder sqlBuilder = new StringBuilder("SELECT KCU1.CONSTRAINT_NAME AS 'FK_CONSTRAINT_NAME', KCU1.TABLE_NAME AS 'FK_TABLE_NAME', KCU1.COLUMN_NAME AS 'FK_COLUMN_NAME', KCU1.ORDINAL_POSITION AS 'FK_ORDINAL_POSITION', KCU2.CONSTRAINT_NAME AS 'UQ_CONSTRAINT_NAME', KCU2.TABLE_NAME AS 'UQ_TABLE_NAME', KCU2.COLUMN_NAME AS 'UQ_COLUMN_NAME', KCU2.ORDINAL_POSITION AS 'UQ_ORDINAL_POSITION' ");
            sqlBuilder.Append("FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG ");
            sqlBuilder.Append("AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION WHERE KCU1.TABLE_NAME = '{0}' AND KCU1.COLUMN_NAME = '{1}'");
            string sql = string.Format(sqlBuilder.ToString(), tableName, columnName);

            //string sql = String.Format("SELECT KCU1.CONSTRAINT_NAME AS 'FK_CONSTRAINT_NAME', KCU1.TABLE_NAME AS 'FK_TABLE_NAME', KCU1.COLUMN_NAME AS 'FK_COLUMN_NAME', KCU1.ORDINAL_POSITION AS 'FK_ORDINAL_POSITION', KCU2.CONSTRAINT_NAME AS 'UQ_CONSTRAINT_NAME', KCU2.TABLE_NAME AS 'UQ_TABLE_NAME', KCU2.COLUMN_NAME AS 'UQ_COLUMN_NAME', KCU2.ORDINAL_POSITION AS 'UQ_ORDINAL_POSITION' " +
            //"FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG " +
            //"AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION WHERE KCU1.TABLE_NAME = '{0}' AND KCU1.COLUMN_NAME = '{1}'",tableName, columnName);

            SqlDataReader dataReader = SqlHelperSqlServer.ExecuteReader(connectionString, CommandType.Text, sql);

            try
            {
                while (dataReader.Read())
                {
                    return dataReader["UQ_TABLE_NAME"].ToString();
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally
            {
                if (!dataReader.IsClosed) dataReader.Close();
                dataReader.Dispose();
            }

            return string.Empty;
        }

        public static string IsForeignKeySimple(string tableName, string columnName)
        {
            if (!columnName.Contains(tableName))
            {
                if (columnName.EndsWith("Code"))
                {
                    return columnName.Replace("Code", string.Empty);
                }
            }

            return string.Empty;
        }

        public static ArrayList GetKeys(string tableName)
        {
            ArrayList array = new ArrayList();

            string connectionString = Session.ConnectionString;

            string sql = string.Format("select * from information_schema.CONSTRAINT_COLUMN_USAGE WHERE CONSTRAINT_NAME LIKE '%PK_%' AND TABLE_NAME = '{0}'", tableName);

            SqlDataReader dataReader = SqlHelperSqlServer.ExecuteReader(connectionString, CommandType.Text, sql);

            try
            {
                while (dataReader.Read())
                {
                    array.Add(dataReader["COLUMN_NAME"].ToString());
                    break;
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally
            {
                if (!dataReader.IsClosed) dataReader.Close();
                dataReader.Dispose();
            }

            return array;
        }

        public static Dictionary<string, KeyColumn> GetKeysDictionary(string tableName)
        {
            Dictionary<string, KeyColumn> collection = new Dictionary<string, KeyColumn>();

            string connectionString = Session.ConnectionString;

            string sql = string.Format("select * from information_schema.CONSTRAINT_COLUMN_USAGE WHERE CONSTRAINT_NAME LIKE '%PK_%' AND TABLE_NAME = '{0}'", tableName);

            SqlDataReader dataReader = SqlHelperSqlServer.ExecuteReader(connectionString, CommandType.Text, sql);

            Dictionary<string, string> fields = GetFieldsEasy(tableName);

            try
            {
                int sequence = 1;
                while (dataReader.Read())
                {
                    //TODO: Per ora le chiavi le ipotizzo solo GUID
                    KeyColumn column = new KeyColumn();
                    column.Name = dataReader["COLUMN_NAME"].ToString();
                    column.Unique = true;
                    column.Type = fields[column.Name];
                    column.NetType = GetDatatableSqlServer.GetSystemType(column.Type, false);                    
                    column.Sequence = sequence;
                    collection.Add(column.Name, column);
                    sequence++;                    
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally
            {
                if (!dataReader.IsClosed) dataReader.Close();
                dataReader.Dispose();
            }

            return collection;
        }

        public static FieldCollection GetFields(string tableName)
        {
            FieldCollection collection = new FieldCollection();

            string query = string.Format("SELECT clmns.name AS [Name],usrt.name AS [DataType],ISNULL(baset.name, N'') AS [SystemType],CAST(CASE WHEN baset.name IN (N'nchar', N'nvarchar') AND clmns.max_length <> -1 THEN clmns.max_length/2 ELSE clmns.max_length END AS int) AS [Length],CAST(clmns.precision AS int) AS [NumericPrecision] FROM sys.tables AS tbl INNER JOIN sys.all_columns AS clmns ON clmns.object_id=tbl.object_id LEFT OUTER JOIN sys.types AS usrt ON usrt.user_type_id = clmns.user_type_id LEFT OUTER JOIN sys.types AS baset ON baset.user_type_id = clmns.system_type_id and baset.user_type_id = baset.system_type_id WHERE (tbl.name='{0}' and SCHEMA_NAME(tbl.schema_id)=N'dbo') ORDER BY clmns.column_id ASC", tableName);

            DataTable dt = SqlHelperSqlServer.ExecuteDataTable(Session.ConnectionString, CommandType.Text, query);

            foreach (DataRow dr in dt.Rows)
            {
                Field field = new Field();
                field.Name = dr["Name"].ToString();
                field.Datatype = dr["DataType"].ToString();
                field.Length = (Int32.Parse(dr["Length"].ToString()));
                collection.Add(field);
            }

            return collection;
        }

        public static Dictionary<string, string> GetFieldsEasy(string tableName)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();
            
            string query = string.Format("SELECT clmns.name AS [Name],usrt.name AS [DataType],ISNULL(baset.name, N'') AS [SystemType],CAST(CASE WHEN baset.name IN (N'nchar', N'nvarchar') AND clmns.max_length <> -1 THEN clmns.max_length/2 ELSE clmns.max_length END AS int) AS [Length],CAST(clmns.precision AS int) AS [NumericPrecision] FROM sys.tables AS tbl INNER JOIN sys.all_columns AS clmns ON clmns.object_id=tbl.object_id LEFT OUTER JOIN sys.types AS usrt ON usrt.user_type_id = clmns.user_type_id LEFT OUTER JOIN sys.types AS baset ON baset.user_type_id = clmns.system_type_id and baset.user_type_id = baset.system_type_id WHERE (tbl.name='{0}' and SCHEMA_NAME(tbl.schema_id)=N'dbo') ORDER BY clmns.column_id ASC", tableName);

            DataTable dt = SqlHelperSqlServer.ExecuteDataTable(Session.ConnectionString, CommandType.Text, query);

            foreach (DataRow dr in dt.Rows)            
                collection.Add(dr["Name"].ToString(), dr["DataType"].ToString());

            return collection;
        }
    }
}
