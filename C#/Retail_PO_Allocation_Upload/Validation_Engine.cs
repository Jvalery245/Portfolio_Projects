using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Allocation_Upload_Program
{
    /// <summary>
    /// Due to the one drive integration with Loris Gifts, an outside verification method has
    /// been implemented to allow for spreadsheet uploads to MI9. The following method calls the validation engine and 
    /// awaits a return code of 0 for a successful validation of all PO components. Anything else results in an error and the 
    /// application terminating.
    /// </summary>
    class Validation_Engine
    {
        public void Validating_PO()
        {
            string path = Directory.GetCurrentDirectory();
            string program = @"\Allocation_Parsing_and_Validation.exe";
            string fullpath = path + program;
            var processcode = Process.Start(fullpath);
            processcode.WaitForExit();
            var exit = processcode.ExitCode;
            if (exit == 0)
            {
                string processingtime = processcode.ExitTime.ToString();
                MessageBox.Show("Validation engine complete. Launching main upload program. Processing ended at: "+ processingtime);
            }
            else
            {
                Environment.Exit(1);
            }
        }
    }
}
