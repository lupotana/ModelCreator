using System;
using System.IO;
using System.Collections;
using System.Text;

namespace ModelCreator
{
    /// <summary>
    /// Classe per l'implementazione dell'oggetto ManageTemplate.
    /// </summary>
    public static class ManageTemplate
    {
        /// <summary>
        /// Legge il Template da un file di testo e lo tiene in memoria 
        /// </summary>
        /// <param name="path">Percorso del Template</param>
        /// <returns>Ritorna il Template</returns>
        public static string GetTemplate(string path)
        {            
            // Variabili temporanea per la copia in memoria del template
            string stringTemplate = string.Empty;
            StringBuilder stringBuilderTemplate = new StringBuilder();
            StreamReader streamReaderTemplate = File.OpenText(string.Concat(Session.ApplicationFolder,"\\",Session.Provider.ToString(),"\\",path));

            // Scrittura su stringa del file letto dal template
            while (streamReaderTemplate.Peek() >= 0)
            {
                stringTemplate = streamReaderTemplate.ReadLine();
                if (stringTemplate == null) continue;
                if (stringBuilderTemplate.Length > 0) stringBuilderTemplate.Append("\n");
                stringBuilderTemplate.Append(stringTemplate);
            }
            return stringBuilderTemplate.ToString();
        }

        public static bool WriteTemplate(string pathTemplate, string builderTemplate, string templateName, string extension)
        {
            return WriteTemplate(pathTemplate, builderTemplate, templateName, extension, true);
        }

        /// <summary>
        /// Scrive il Template modificato nel file di destinazione
        /// rinominandolo con il nome del programma 
        /// </summary>
        /// <param name="pathTemplate">Percorso di destinazione del Template</param>
        /// <param name="builderTemplate">Template</param>
        /// <param name="templateName">Nome del template</param>
        /// <returns>Ritorna l'esito della scrittura sul file Template (true OK,false ERROR)</returns>
        public static bool WriteTemplate(string pathTemplate, string builderTemplate, string templateName, string extension, bool useCreateFirstRelase)
        {
            if (extension == null)
            {
                extension = ".cs";
                builderTemplate = builderTemplate + "\n";
            }

            try
            {
                if (useCreateFirstRelase)
                {
                    if (Session.CreateFirstRelease)
                    {
                        templateName = string.Concat(Session.Version, " ", templateName);
                    }
                }

                // Inizializzazione delle variabili temporanee
                StreamWriter pathDestinationTemplate;
                
                // Controlli sul path e sul nome del file di destinazione
                if (!pathTemplate.EndsWith("\\")) pathTemplate = string.Concat(pathTemplate,"\\");
                if (!Directory.Exists(pathTemplate.ToString())) Directory.CreateDirectory(pathTemplate.ToString());
                pathTemplate = string.Concat(pathTemplate, templateName, extension);
                if (File.Exists(pathTemplate.ToString())) File.Delete(pathTemplate.ToString());
                
                // Scrittura del Template
                pathDestinationTemplate = File.AppendText(pathTemplate.ToString());
                pathDestinationTemplate.WriteLine(builderTemplate);
                pathDestinationTemplate.Close();
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return false;
            }
        }

    }
}
