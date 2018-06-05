using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelCreator
{
    public interface MessageGeneral
    {
        //// MESSAGE                
        string Message_GeneralOK();
        string Message_GeneralERROR();
        string Message_CreateDataLayer();

        //// TAB : DATALAYER
        string DataLayer_Info();
        string DataLayer_Check();
        string DataLayer_UnCheck();
        string DataLayer_Tag_FilterList();
        string DataLayer_Filter();
        string DataLayer_GoFileFolder();
        string DataLayer_Calculate_TableSelected();
        string DataLayer_CreateDataLayer();

        // TAB : SETTING
        string Settings_SaveInformation();

        string Settings_DataProvider();
        string Settings_ClassSuffix();

        string Settings_Modality_Control();
        string Settings_Modality();
        string Settings_Modality_Simple();
        string Settings_Modality_Professional();
        string Settings_Modality_ThreeLayer();

        string Settings_Framework();
        
        string Settings_CodeInReading_Control();
        string Settings_CodeInReading();
        string Settings_CodeInReading_MainCode();
        string Settings_CodeInReading_Keyuni();
                
        string Settings_ManageKey_Control();
        string Settings_ManageKey();
        string Settings_ManageKey_Database();
        string Settings_ManageKey_Custom();

        string Settings_MainTableKey_Control();
        string Settings_MainTableKey();
        string Settings_MainTableKey_Code();
        string Settings_MainTableKey_id();        

        string Settings_SpecialFields_Control();
        string Settings_SpecialFields();

        string Settings_GuidCreator_Control();
        string Settings_GuidCreator();

        string Settings_CustomBool_Control();
        string Settings_SuperClass_Control();
        string Settings_CustomClass_Control();
        string Settings_FirstRelease_Control();
        string Settings_NullLogic_Control();

        string Settings_CustomBool();
        string Settings_SuperClass();
        string Settings_CustomClass();
        string Settings_FirstRelease();
        string Settings_NullLogic();

        string Settings_Namespace();

        string Settings_FolderDestination();
        string Settings_FolderDestination_Control();

        string Settings_SQLScriptFile();
        string Settings_SQLScriptFile_Control();
      
        string Settings_Color();
        string Settings_Color_Grey();
        string Settings_Color_Orange();
        string Settings_Color_LightBlue();
        string Settings_Color_Green();
        
        string Settings_LanguageGroup();
        string Settings_LanguageIT();
        string Settings_LanguageEN();

    }
}

