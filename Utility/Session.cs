using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCreator
{
    public static class Session
    {
        public static string Version = string.Empty;
        public static string ApplicationFolder = string.Empty;
        public static string ClassSuffix = string.Empty;

        public static Provider Provider = Provider.BaseProvider;
        public static Dictionary<string, KeyColumn> Keys = new Dictionary<string, KeyColumn>();
        public static string Color = string.Empty;
        public static string ManageKey = string.Empty;

        public static string ConnectionString = string.Empty;

        public static string NamespaceDataLayer = string.Empty;
        public static string NamespaceBusinessLayer = string.Empty;
        public static string Folder = string.Empty;
        public static string SqlScriptFile = string.Empty;
        public static string Owner = string.Empty;

        public static bool CopyProject = false;
        public static string PathCopyBusiness = string.Empty;
        public static string PathCopyDataLayer = string.Empty;

        public static Modality Modality = Modality.Easy;
        public static string GuidCreator = string.Empty;

        public static string Code = string.Empty;
        public static bool StandardCode = false;
        public static string TableKey = string.Empty;

        public static bool Timespan = false;
        public static bool Keyuni = false;

        public static Framework Framework = Framework.net20;

        public static bool Comment = false;
        public static string AuthorName = string.Empty;

        public static string InsertSql = string.Empty;
        public static string CloneSql = string.Empty;
        public static string UpdateSql = string.Empty;
        public static string DeleteSql = string.Empty;

        public static string GetCollectionSql = string.Empty;
        public static string GetByCodeSql = string.Empty;
        public static string GetByKeyuniSql = string.Empty;
        public static string GetCustomItemSql = string.Empty;
        public static string GetCustomCollectionSql = string.Empty;

        public static string InsertFunction = string.Empty;
        public static string CloneFunction = string.Empty;
        public static string UpdateFunction = string.Empty;
        public static string DeleteFunction = string.Empty;
        public static string ReadFunction = string.Empty;

        public static string GetCollectionFunction = string.Empty;
        public static string GetByCodeFunction = string.Empty;
        public static string GetByKeyuniFunction = string.Empty;
        public static string GetCustomItemFunction = string.Empty;
        public static string GetCustomCollectionFunction = string.Empty;

        public static string ReadingFunctionCode = string.Empty;

        public static string View = string.Empty;
        public static string IsUnknown = string.Empty;

        public static bool IsCustomDecimal = false;
        public static bool SuperClass = false;
        public static bool CreateCustomClass = false;
        public static bool CreateFirstRelease = false;
        public static bool NullLogic = false;

        public static string Language = string.Empty;
    }
}