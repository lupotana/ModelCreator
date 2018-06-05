using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelCreator
{
    public class Language_en : MessageGeneral
    {
        // MESSAGE                
        public string Message_GeneralOK() { return "OPERAZIONE AVVENUTA CON SUCCESSO"; }
        public string Message_GeneralERROR() { return "SI SONO VERIFICATI DEGLI ERRORI"; }
        public string Message_CreateDataLayer() { return "BUILD AND WRITE MODEL SUCCESSFULL"; }

        // TAB : DATALAYER
        public string DataLayer_Info() { return "DATA LAYER"; }
        public string DataLayer_Check() { return " CHECK"; }
        public string DataLayer_UnCheck() { return " UNCHECK"; }
        public string DataLayer_Tag_FilterList() { return "FILTER LIST OF TABLES"; }
        public string DataLayer_Filter() { return " FILTER"; }
        public string DataLayer_GoFileFolder() { return "GO TO FILE FOLDER"; }
        public string DataLayer_Calculate_TableSelected() { return "{0} SELECTED TABLES, {1} TABLES PRESENT"; }
        public string DataLayer_CreateDataLayer() { return " CREATE DATA LAYER"; }

        // TAB : SETTING
        public string Settings_SaveInformation() { return " SAVE INFORMATIONS"; }

        public string Settings_DataProvider() { return "DATA PROVIDER"; }
        public string Settings_ClassSuffix() { return "CLASS SUFFIX"; }

        public string Settings_Modality_Control() { return "MODALITY"; }
        public string Settings_Modality() { return "...(en)..."; }
        public string Settings_Modality_Simple() { return "Simple"; }
        public string Settings_Modality_Professional() { return "Professional"; }
        public string Settings_Modality_ThreeLayer() { return "Three Layer"; }

        public string Settings_Framework() { return "Select the .NET Framework Compatibility"; }

        public string Settings_CodeInReading_Control() { return "DATA PROVIDER"; }
        public string Settings_CodeInReading() { return "...(en)..."; }
        public string Settings_CodeInReading_MainCode() { return "Main Code"; }
        public string Settings_CodeInReading_Keyuni() { return "Keyuni Column"; }

        public string Settings_ManageKey_Control() { return "MANAGE KEYS"; }
        public string Settings_ManageKey() { return "...(en)..."; }
        public string Settings_ManageKey_Database() { return "Database"; }
        public string Settings_ManageKey_Custom() { return "Custom"; }

        public string Settings_MainTableKey_Control() { return "MAIN TABLE KEY"; }
        public string Settings_MainTableKey() { return "...(en)..."; }
        public string Settings_MainTableKey_Code() { return "{0}Code"; }
        public string Settings_MainTableKey_id() { return "id"; }

        public string Settings_SpecialFields_Control() { return "SPECIAL FIELDS (Only Sql Server)"; }
        public string Settings_SpecialFields() { return "...(en)..."; }

        public string Settings_GuidCreator_Control() { return "GUID CREATOR (Only Sql Server)"; }
        public string Settings_GuidCreator() { return "...(en)..."; }

        public string Settings_CustomBool_Control() { return "CUSTOM BOOL"; }
        public string Settings_SuperClass_Control() { return "SUPER CLASS"; }
        public string Settings_CustomClass_Control() { return "CUSTOM CLASS"; }
        public string Settings_FirstRelease_Control() { return "FIRST RELEASE"; }
        public string Settings_NullLogic_Control() { return "NULL LOGIC"; }

        public string Settings_CustomBool() { return "Transform type Decimal(1,0) as bool (Only SqlBase)"; }
        public string Settings_SuperClass() { return "...(en)..."; }
        public string Settings_CustomClass() { return "...(en)..."; }
        public string Settings_FirstRelease() { return "Create file with prefix the version of Model Creator"; }
        public string Settings_NullLogic() { return "...(en)..."; }

        public string Settings_Namespace() { return "...(en)..."; }

        public string Settings_FolderDestination() { return "...(en)..."; }
        public string Settings_FolderDestination_Control() { return "FOLDER DESTINATION"; }

        public string Settings_SQLScriptFile() { return "...(en)..."; }
        public string Settings_SQLScriptFile_Control() { return "SQL SCRIPT FILE"; }

        public string Settings_Color() { return "COLOR OF BACKGROUND"; }
        public string Settings_Color_Grey() { return "Grey"; }
        public string Settings_Color_Orange() { return "Orange"; }
        public string Settings_Color_LightBlue() { return "Light Blue"; }
        public string Settings_Color_Green() { return "Green"; }
        
        public string Settings_LanguageGroup() { return "LANGUAGE"; }
        public string Settings_LanguageIT() { return "Italian"; }
        public string Settings_LanguageEN() { return "English"; }
    }
}