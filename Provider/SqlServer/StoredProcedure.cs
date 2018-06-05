using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace ModelCreator
{
    public class StoredProcedureSqlServer
    {
        public enum StoredProcedureTypes
        {
            Update,
            Insert,
            Clone,
            Delete,
            GetByKeyuni,
            GetCollection,
            GetByCode,
            View,
            ViewPlus,
        }

        public string Generate(StoredProcedureTypes sptypeGenerate, FieldCollection fieldCollection, string tableName)
        {
            Session.Keys = GetInfoFieldSqlServer.GetKeysDictionary(tableName);

            StringBuilder sGeneratedCode = new StringBuilder();
            StringBuilder sParamDeclaration = new StringBuilder();
            StringBuilder sBody = new StringBuilder();
            StringBuilder sINSERTValues = new StringBuilder();

            bool isFirst = true;
            string functionName = string.Empty;

            switch (sptypeGenerate)
            {
                case StoredProcedureTypes.Insert: functionName = string.Format(Session.InsertSql, tableName); break;
                case StoredProcedureTypes.Clone: functionName = string.Format(Session.CloneSql, tableName); break;
                case StoredProcedureTypes.Update: functionName = string.Format(Session.UpdateSql, tableName); break;
                case StoredProcedureTypes.Delete: functionName = string.Format(Session.DeleteSql, tableName); break;
                case StoredProcedureTypes.GetCollection: functionName = string.Format(Session.GetCollectionSql, tableName); break;
                case StoredProcedureTypes.GetByCode: functionName = string.Format(Session.GetByCodeSql, tableName); break;
                case StoredProcedureTypes.GetByKeyuni: functionName = string.Format(Session.GetByKeyuniSql, tableName); break;
            }

            if (sptypeGenerate != StoredProcedureTypes.View)
            {
                if (sptypeGenerate != StoredProcedureTypes.ViewPlus)
                {
                    sGeneratedCode.AppendFormat(string.Format("if exists(select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsProcedure') = 1) drop procedure [dbo].[{0}]", functionName));
                    sGeneratedCode.Append(Environment.NewLine);
                    sGeneratedCode.Append("GO");
                    sGeneratedCode.Append(Environment.NewLine);
                }
            }
            else
            {
                sGeneratedCode.AppendFormat(string.Format("if exists(select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsView') = 1) drop view [dbo].[{0}]", string.Concat(Session.View, tableName)));
                sGeneratedCode.Append(Environment.NewLine);
                sGeneratedCode.Append("GO");
                sGeneratedCode.Append(Environment.NewLine);
            }

            // Setup SP code, begining is the same no matter the type
            //if (sGeneratedCode.Length != 0) sGeneratedCode.Append("\\n");
            if (Session.Comment)
            {
                sGeneratedCode.AppendFormat(string.Concat("-- TABLE     : ", tableName));
                sGeneratedCode.Append(Environment.NewLine);
                sGeneratedCode.AppendFormat(string.Format("-- AUTHOR : {0}", Session.AuthorName));
                sGeneratedCode.Append(Environment.NewLine);
                sGeneratedCode.AppendFormat(string.Concat("-- DATE      : ", DateTime.Now.ToString("dd MMM yyyy  HH:mm:ss")));
                sGeneratedCode.Append(Environment.NewLine);
                sGeneratedCode.Append(Environment.NewLine);
            }

            if ((sptypeGenerate != StoredProcedureTypes.View) && (sptypeGenerate != StoredProcedureTypes.ViewPlus))
            {
                sGeneratedCode.AppendFormat("CREATE PROCEDURE {0}", functionName);
            }
            else
            {
                if (sptypeGenerate == StoredProcedureTypes.View)
                    sGeneratedCode.AppendFormat("CREATE VIEW {0}{1}", Session.View, tableName);
                else sGeneratedCode.AppendFormat("CREATE VIEW {0}{1}Plus", Session.View, tableName);
            }
            sGeneratedCode.Append(Environment.NewLine);            

            switch (sptypeGenerate)
            {
                case StoredProcedureTypes.Insert:
                    sBody.AppendFormat("INSERT INTO [{0}] (", tableName);
                    sBody.Append(Environment.NewLine);

                    sINSERTValues.Append("VALUES (");
                    sINSERTValues.Append(Environment.NewLine);
                    break;

                case StoredProcedureTypes.Clone:
                    sBody.AppendFormat("INSERT INTO [{0}] (", tableName);
                    sBody.Append(Environment.NewLine);

                    sINSERTValues.Append("VALUES (");
                    sINSERTValues.Append(Environment.NewLine);
                    break;

                case StoredProcedureTypes.Update:
                    sBody.AppendFormat("UPDATE [{0}]", tableName);
                    sBody.Append(Environment.NewLine);
                    sBody.Append("SET");
                    sBody.Append(Environment.NewLine);
                    break;
            }

            string sqlMax = string.Empty;
            bool isFirstInsert = true;
            bool isFirstClone = true;
            bool isFirstUpdate = true;

            switch (sptypeGenerate)
            {
                case StoredProcedureTypes.Insert:
                case StoredProcedureTypes.Clone:
                case StoredProcedureTypes.Update:                    
                    foreach (Field colCurrent in fieldCollection)
                    {
                        bool isFieldValid = true;

                        if (Session.GuidCreator == "Database")
                        {

                            if ((Session.Keyuni) && (sptypeGenerate == StoredProcedureTypes.Insert) && (colCurrent.Name.Equals(Session.TableKey)))
                                isFieldValid = false;

                            if ((!Session.Keyuni) && (sptypeGenerate == StoredProcedureTypes.Insert) && (colCurrent.Name.Equals(Session.TableKey)))
                                isFieldValid = false;

                            if ((Session.Keyuni) && (sptypeGenerate == StoredProcedureTypes.Update) && (colCurrent.Name.Equals(Session.TableKey)))
                                isFieldValid = false;
                        }

                        if (Session.GuidCreator == "Framework")
                        {

                            if ((sptypeGenerate == StoredProcedureTypes.Insert) && (Session.Keys.ContainsKey(colCurrent.Name)))
                                isFieldValid = true;

                            if ((sptypeGenerate == StoredProcedureTypes.Update) && (Session.Keys.ContainsKey(colCurrent.Name)))
                                isFieldValid = false;

                            isFieldValid = true;
                        }

                        if (colCurrent.Name.Equals("Timespan"))
                            isFieldValid = false;

                        if (isFieldValid)
                        {
                            // Declaration
                            sParamDeclaration.AppendFormat("    @{0} {1}", new string[] { colCurrent.Name, colCurrent.Datatype });

                            if (
                                colCurrent.Datatype == "binary" ||
                                colCurrent.Datatype == "char" ||
                                colCurrent.Datatype == "nchar" ||
                                colCurrent.Datatype == "nvarchar" ||
                                colCurrent.Datatype == "varbinary" ||
                                colCurrent.Datatype == "varchar")
                                if (colCurrent.Length == -1)
                                    sParamDeclaration.AppendFormat("(max)");
                                else
                                    sParamDeclaration.AppendFormat("({0})", colCurrent.Length);

                            sParamDeclaration.Append(",");
                            sParamDeclaration.Append(Environment.NewLine);

                            //Insert Value
                            if (!colCurrent.Name.Equals("Keyuni"))
                            {
                                switch (sptypeGenerate)
                                {
                                    case StoredProcedureTypes.Insert:
                                        if (Session.GuidCreator == "Database")
                                        {
                                            if (!colCurrent.Name.Equals(Session.TableKey))
                                            {
                                                sINSERTValues.AppendFormat("    @{0},", colCurrent.Name);
                                                sINSERTValues.Append(Environment.NewLine);
                                            }
                                        }
                                        if (Session.GuidCreator == "Framework")
                                        {
                                            sINSERTValues.AppendFormat("    @{0},", colCurrent.Name);
                                            sINSERTValues.Append(Environment.NewLine);
                                        }
                                        if (isFirstInsert)
                                            isFirstInsert = false;
                                        else
                                            sBody.Append(Environment.NewLine);
                                        sBody.AppendFormat("    {0},", colCurrent.Name);
                                        break;

                                    case StoredProcedureTypes.Clone:
                                        sINSERTValues.AppendFormat("    @{0},", colCurrent.Name);
                                        sINSERTValues.Append(Environment.NewLine);
                                        if (isFirstClone)
                                            isFirstClone = false;
                                        else
                                            sBody.Append(Environment.NewLine);
                                        sBody.AppendFormat("    {0},", colCurrent.Name);
                                        break;

                                    case StoredProcedureTypes.Update:
                                        if (colCurrent.Name.Equals(Session.TableKey))
                                        {
 
                                        }
                                        else
                                        {
                                            if (isFirstUpdate)
                                                isFirstUpdate = false;
                                            else
                                                sBody.Append(Environment.NewLine);
                                            sBody.AppendFormat("    {0} = @{0},", new string[] { colCurrent.Name, });                                            
                                        }
                                        break;
                                }
                            }
                        }

                    }
                    break;

                case StoredProcedureTypes.View:
                    sBody.Append("AS");
                    sBody.Append(Environment.NewLine);                    
                    string fields = string.Empty;
                    //foreach (SQLDMO.Column colCurrent in colsFields)
                    foreach (Field colCurrent in fieldCollection)
                    {
                        if (!fields.Equals(string.Empty)) fields = string.Concat(fields, ",");
                        fields = string.Concat(fields, colCurrent.Name);
                    }
                    sBody.AppendFormat(string.Concat("SELECT  ", fields));
                    sBody.Append(Environment.NewLine);
                    sBody.AppendFormat(string.Format("FROM [dbo].[{0}]", tableName));
                    sBody.Append(Environment.NewLine);
                    sBody.Append("GO");
                    sBody.Append(Environment.NewLine);
                    sGeneratedCode.Append(sBody);
                    break;

                case StoredProcedureTypes.ViewPlus:
                    sBody.Append("AS");
                    sBody.Append(Environment.NewLine);
                    fields = string.Empty;
                    //foreach (SQLDMO.Column colCurrent in colsFields)
                    foreach (Field colCurrent in fieldCollection)
                    {
                        if (!fields.Equals(string.Empty)) fields = string.Concat(fields, ",");
                        fields = string.Concat(fields, colCurrent.Name);
                    }
                    sBody.AppendFormat(string.Concat("SELECT  ", fields));
                    sBody.Append(Environment.NewLine);
                    sBody.AppendFormat(string.Format("FROM [dbo].[{0}]", tableName));
                    sBody.Append(Environment.NewLine);
                    sBody.Append("GO");
                    sBody.Append(Environment.NewLine);
                    sGeneratedCode.Append(sBody);
                    break;

                case StoredProcedureTypes.Delete:
                    if (Session.Keyuni)
                    {
                        sBody.AppendFormat("	(@Keyuni 	[uniqueidentifier])");
                        sBody.Append(Environment.NewLine);
                        sBody.Append("AS");
                        sBody.Append(Environment.NewLine);
                        sBody.Append("SET NOCOUNT ON");
                        sBody.Append(Environment.NewLine);
                        sBody.AppendFormat(string.Format("DELETE [dbo].[{0}]", tableName));
                        sBody.Append(Environment.NewLine);
                        sBody.AppendFormat("WHERE 	( [Keyuni]  = @Keyuni)");
                        sBody.Append(Environment.NewLine);
                        sBody.Append("GO");
                        sBody.Append(Environment.NewLine);
                        sGeneratedCode.Append(sBody);
                    }
                    else
                    {
                        bool isFirstParameter = true;
                        foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                        {
                            if (!isFirstParameter)
                            {
                                sBody.Append(",");
                                sBody.Append(Environment.NewLine);
                            }
                            else isFirstParameter = false;
                            
                            sBody.AppendFormat(string.Format("	@{0} {1}", kvp.Key, kvp.Value.Type));                            
                        }
                        sBody.Append(Environment.NewLine);
                        sBody.Append("AS");
                        sBody.Append(Environment.NewLine);
                        sBody.Append("SET NOCOUNT ON");
                        sBody.Append(Environment.NewLine);
                        sBody.AppendFormat(string.Format("DELETE [dbo].[{0}]", tableName));
                        sBody.Append(Environment.NewLine);
                        sBody.AppendFormat("WHERE 	( ");
                        isFirst = true;
                        foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                        {
                            if (!isFirst)
                                sBody.AppendFormat(" AND ");
                            sBody.AppendFormat(string.Format(" [{0}]  = @{0} ", kvp.Key));
                            isFirst = false;
                        }
                        sBody.AppendFormat(" )");
                        sBody.Append(Environment.NewLine);
                        sBody.Append("GO");
                        sBody.Append(Environment.NewLine);
                        sGeneratedCode.Append(sBody);
                    }
                    break;
                case StoredProcedureTypes.GetByCode:
                    //sBody.AppendFormat("	(@Code 	[int])");
                    bool isFirstParameterGetByCode = true;
                    foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                    {
                        if (!isFirstParameterGetByCode)
                        {
                            sBody.Append(",");
                            sBody.Append(Environment.NewLine);
                        }
                        else isFirstParameterGetByCode = false;

                        sBody.AppendFormat(string.Format("	@{0} {1}", kvp.Key, kvp.Value.Type));                        
                    }                    
                    sBody.Append(Environment.NewLine);
                    sBody.Append("AS");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("SET NOCOUNT ON");
                    sBody.Append(Environment.NewLine);
                    sBody.AppendFormat(string.Format("SELECT * FROM [dbo].[{0}{1}]", Session.View, tableName));
                    sBody.Append(Environment.NewLine);
                    //sBody.AppendFormat(string.Format("WHERE 	( [{0}]  = @Code)", Session.TableKey));
                    sBody.AppendFormat("WHERE 	( ");
                    isFirst = true;
                    foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                    {
                        if (!isFirst)
                            sBody.AppendFormat(" AND ");
                        sBody.AppendFormat(string.Format(" [{0}]  = @{0} ", kvp.Key));
                        isFirst = false;
                    }
                    sBody.AppendFormat(" )");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("RETURN");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("GO");
                    sBody.Append(Environment.NewLine);
                    sGeneratedCode.Append(sBody);
                    break;
                case StoredProcedureTypes.GetByKeyuni:
                    sBody.AppendFormat("	(@Keyuni 	[uniqueidentifier])");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("AS");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("SET NOCOUNT ON");
                    sBody.Append(Environment.NewLine);
                    sBody.AppendFormat(string.Format("SELECT * FROM [dbo].[{0}{1}]", Session.View, tableName));
                    sBody.Append(Environment.NewLine);
                    sBody.AppendFormat("WHERE 	( [Keyuni]  = @Keyuni)");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("RETURN");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("GO");
                    sBody.Append(Environment.NewLine);
                    sGeneratedCode.Append(sBody);
                    break;
                case StoredProcedureTypes.GetCollection:
                    sBody.Append("AS");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("SET NOCOUNT ON");
                    sBody.Append(Environment.NewLine);
                    sBody.AppendFormat(string.Format("SELECT * FROM [dbo].[{0}{1}]", Session.View, tableName));
                    sBody.Append(Environment.NewLine);
                    sBody.Append("RETURN");
                    sBody.Append(Environment.NewLine);
                    sBody.Append("GO");
                    sBody.Append(Environment.NewLine);
                    sGeneratedCode.Append(sBody);
                    break;
            }

            switch (sptypeGenerate)
            {
                case StoredProcedureTypes.Insert:
                case StoredProcedureTypes.Clone:
                case StoredProcedureTypes.Update:
                    if (Session.GuidCreator == "Database")
                    {
                        sParamDeclaration.Append("@Identity int OUTPUT,");
                        sParamDeclaration.Append(Environment.NewLine);
                    }
                    sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));
                    sGeneratedCode.Append(Environment.NewLine);
                    sGeneratedCode.Append("AS");
                    sGeneratedCode.Append(Environment.NewLine);
                    sGeneratedCode.Append("SET NOCOUNT ON");
                    sGeneratedCode.Append(Environment.NewLine);
                    //sGeneratedCode.Append(sBody);
                    sGeneratedCode.Append(sBody.Remove(sBody.Length - 1, 1));
                    //sGeneratedCode.Append(sBody.Remove(sBody.Length - 3, 3));

                    if ((sptypeGenerate == StoredProcedureTypes.Insert) || (sptypeGenerate == StoredProcedureTypes.Clone))
                    {
                        sGeneratedCode.Append(")");
                        sGeneratedCode.Append(Environment.NewLine);
                        sGeneratedCode.Append(sINSERTValues.Remove(sINSERTValues.Length - 3, 3));
                        //sGeneratedCode.Append(sINSERTValues);
                        sGeneratedCode.Append(")");
                        if (Session.GuidCreator == "Database")
                        {
                            sGeneratedCode.Append(Environment.NewLine);
                            sGeneratedCode.Append("SELECT @Identity = @@IDENTITY");
                        }
                    }
                    else
                    {
                        sGeneratedCode.Append(Environment.NewLine);

                        sGeneratedCode.AppendFormat("WHERE 	( ");
                        isFirst = true;
                        foreach (KeyValuePair<string, KeyColumn> kvp in Session.Keys)
                        {
                            if (!isFirst)
                                sGeneratedCode.AppendFormat(" AND ");
                            sGeneratedCode.AppendFormat(string.Format(" [{0}]  = @{0} ", kvp.Key));
                            isFirst = false;
                        }
                        sGeneratedCode.AppendFormat(" )");
                        sGeneratedCode.Append(Environment.NewLine);
                        sGeneratedCode.Append("GO");
                        sGeneratedCode.Append(Environment.NewLine);

                        //if (Session.Keyuni)
                        //    sGeneratedCode.AppendFormat("WHERE 	( [Keyuni]  = @Keyuni)");
                        //else sGeneratedCode.AppendFormat(string.Format("WHERE 	( [{0}]  = @{0})", Session.TableKey));
                    }

                    sGeneratedCode.Append(Environment.NewLine);
                    sGeneratedCode.Append("GO");
                    sGeneratedCode.Append(Environment.NewLine);
                    break;
            }
            return sGeneratedCode.ToString();
        }



    }

    public class Field
    {
        public string Name { get; set; }
        public string Datatype { get; set; }
        public int Length { get; set; }
    }

    public class FieldCollection : List<Field> { }


}