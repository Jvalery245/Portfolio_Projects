using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;

namespace Allocation_Upload_Program
{
    public class Allocation_Excel_Sheet
    {
        public string path_upload = string.Empty;
        public void Excel_FileUpload()
        {
            OpenFileDialog PO_Gen_Upload_File = new OpenFileDialog();
            string Desktop_Upload = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            PO_Gen_Upload_File.InitialDirectory = Desktop_Upload;
            PO_Gen_Upload_File.RestoreDirectory = true;
            PO_Gen_Upload_File.Filter = "Excel Fie|*.xlsx";
            PO_Gen_Upload_File.Title = "Save an MI9 Upload Allocation";
            PO_Gen_Upload_File.ShowDialog();
            if (PO_Gen_Upload_File.ShowDialog() == true)

            {
                path_upload = PO_Gen_Upload_File.FileName;
            }
        }
    }
}
