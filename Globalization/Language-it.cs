using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelCreator
{
    public class Language_it : MessageGeneral
    {
        // MESSAGE                
        public string Message_GeneralOK() { return "OPERAZIONE AVVENUTA CON SUCCESSO"; }        
        public string Message_GeneralERROR() { return "SI SONO VERIFICATI DEGLI ERRORI"; }
        public string Message_CreateDataLayer() { return "CREAZIONE STRATO DATI EFFETTUATO"; }

        // TAB : DATALAYER
        public string DataLayer_Info() { return "STRATO DATI"; }
        public string DataLayer_Check() { return " SELEZIONA"; }
        public string DataLayer_UnCheck() { return " DESELEZIONA"; }
        public string DataLayer_Tag_FilterList() { return "FILTRO SULLE TABELLE"; }
        public string DataLayer_Filter() { return " RICERCA"; }
        public string DataLayer_GoFileFolder() { return "CARTELLA FILE GENERATI"; }
        public string DataLayer_Calculate_TableSelected() { return "{0} TABELLE SELEZIONATE, {1} TABELLE PRESENTI"; }
        public string DataLayer_CreateDataLayer() { return " CREA STRATO DATI"; }

        // TAB : SETTING
        public string Settings_SaveInformation() { return " SALVATAGGIO DELLE INFORMAZIONI"; }

        public string Settings_DataProvider() { return "PROVIDER DATI"; }
        public string Settings_ClassSuffix() { return "SUFFISO CLASSE"; }

        public string Settings_Modality_Control() { return "MODALITA'"; }
        public string Settings_Modality() { return "La modalità semplice crea un super strato dati mentre la modalità professional crea un'architettura più complessa' "; }
        public string Settings_Modality_Simple() { return "Semplice"; }
        public string Settings_Modality_Professional() { return "Professionale"; }
        public string Settings_Modality_ThreeLayer() { return "Three Layer"; }

        public string Settings_Framework() { return "Selezionare la compatibilità del .NET Framework"; }

        public string Settings_CodeInReading_Control() { return "CODE USING IN READING FUNCTION"; }
        public string Settings_CodeInReading() { return "...(it)..."; }
        public string Settings_CodeInReading_MainCode() { return "Codice principale"; }
        public string Settings_CodeInReading_Keyuni() { return "Colonna Keyuni"; }        

        public string Settings_ManageKey_Control() { return "GESTIONE CHIAVI"; }
        public string Settings_ManageKey() { return "...(it)..."; }
        public string Settings_ManageKey_Database() { return "Database"; }
        public string Settings_ManageKey_Custom() { return "Personalizzata"; }

        public string Settings_MainTableKey_Control() { return "CHIAVE PRINCIPALE"; }
        public string Settings_MainTableKey() { return "..(it).."; }
        public string Settings_MainTableKey_Code() { return "{0}Code"; }
        public string Settings_MainTableKey_id() { return "id"; }

        public string Settings_SpecialFields_Control() { return "CAMPI SPECIALI (Solo Sql Server)"; }
        public string Settings_SpecialFields() { return "...(it)..."; }

        public string Settings_GuidCreator_Control() { return "CREAZIONE FILE GUID (Solo Sql Server)"; }
        public string Settings_GuidCreator() { return "...(it)..."; }

        public string Settings_CustomBool_Control() { return "BOOL PERSONALIZZATO"; }
        public string Settings_SuperClass_Control() { return "CLASSE EVOLUTA"; }
        public string Settings_CustomClass_Control() { return "CLASSE PERSONALIZZATA"; }
        public string Settings_FirstRelease_Control() { return "RELEASE PRIMA DEL NOME"; }
        public string Settings_NullLogic_Control() { return "NULL LOGICO"; }

        public string Settings_CustomBool() { return "Trasforma il tipo Decimal(1,0) in campo booleano (Solo SqlBase)"; }
        public string Settings_SuperClass() { return "...(it)..."; }
        public string Settings_CustomClass() { return "...(it)..."; }
        public string Settings_FirstRelease() { return "Prefisso della versione del Model Creator nel nome del fiel creato"; }
        public string Settings_NullLogic() { return "...(it)..."; }

        public string Settings_Namespace() { return "...(it)..."; }        

        public string Settings_FolderDestination() { return "...(it)..."; }
        public string Settings_FolderDestination_Control() { return "CARTELLA DI SALVATAGGIO FILE GENERATI"; }

        public string Settings_SQLScriptFile() { return "...(it)..."; }
        public string Settings_SQLScriptFile_Control() { return "CARTELLA DI SALVATAGGIO DEL FILE SCRIPT"; }

        public string Settings_Color() { return "COLORE DI SFONDO"; }
        public string Settings_Color_Grey() { return "Grigio"; }
        public string Settings_Color_Orange() { return "Arancione"; }
        public string Settings_Color_LightBlue() { return "Azzurro"; }
        public string Settings_Color_Green() { return "Verde"; }

        public string Settings_LanguageGroup() { return "LINGUA"; }
        public string Settings_LanguageIT() { return "Italiano"; }
        public string Settings_LanguageEN() { return "Inglese"; }
    }
}