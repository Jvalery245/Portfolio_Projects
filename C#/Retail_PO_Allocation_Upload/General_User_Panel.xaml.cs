using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace Allocation_Upload_Program
{
    public partial class General_User_Panel : Window
    {
        //Creating Public read instances to close COM and Data Connections to database if exceptions are thrown
        public object formatUploadDateTime { get; private set; }
        public object vendor_name { get; private set; }
        public object user_name { get; private set; }
        public object worksheetRange { get; private set; }
        public object PO_worksheet { get; private set; }
        public object workbook { get; private set; }
        public object PO_File { get; private set; }
        public General_User_Panel()
        {
            Validation_Engine PO_Check = new Validation_Engine();
            PO_Check.Validating_PO();
            InitializeComponent();
            User_Delivery_Date.DisplayDateStart = DateTime.Today.AddDays(6);
        }
        /// <summary>
        /// Setting Gloabel SQL Path
        /// Getting;Setting Variable from GUI Interface
        /// </summary>
        public static string path = @"C:\Users\apruitt\Lori's Gifts\Merchandising and Supply Chain - Documents\Lee Goldstein\SKU Allocations\Store Allocation Templates\PO_Upload_Test.xlsx";
        public static string connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0;";

        private void All_PO_Preview_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Marcket_File_Read = new OpenFileDialog();
            string Desktop_Upload = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Marcket_File_Read.InitialDirectory = Desktop_Upload;
            Marcket_File_Read.RestoreDirectory = true;
            Marcket_File_Read.Filter = "Excel Fie|*.xlsx";
            Marcket_File_Read.Title = "Save an MI9 Upload Allocation";
            Marcket_File_Read.ShowDialog();


            if (Marcket_File_Read.ShowDialog() == true)
            {
                try
                {
                    string path_upload = Marcket_File_Read.FileName;
                    string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path_upload + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                    using (OleDbConnection UploadExcelCon = new OleDbConnection(connectionString))
                    {
                        OleDbCommand excelData = new OleDbCommand("SELECT id, poid, Code, VendorCode, Authorized, OrderDate, DeliveryDate, DeliverUntilDate," +
                            " CancelDate, ShipDate, LocationCode, ChannelCode, TypeCode, SettleType, BuyingMethod, PurchaseType, PayMethod," +
                            " TransportMethod, BuyerCode, SeasonCode, VendorPromoCode, TargetYrWeek, AckReference, Reference1, Reference2," +
                            " CurrencyCode, CurrRate, CalculatePOExtraCosts, OpenOrder, PriceListCode, PriceListCurrRate, OrderType, AllowBo," +
                            " ProductCode, ProductFirstCost, ProductUnitRetail, ProductSalesUnit, SKUCode, SKUQuantity, SKUUnitRetail, SKUCloseLine," +
                            " SKUExtraCostsCostCode, SKUExtraCostsCostValue, crDate, processDate, processResponse, numTries FROM [MI9_Store_Order_Sheet$]", UploadExcelCon);
                        //DbDataReader reading_excelfile = excelData.ExecuteReader();
                        OleDbDataAdapter sQLDataToUpload = new OleDbDataAdapter(excelData);
                        DataTable excelrecords = new DataTable("MI9_Store_Order_Sheet$");
                        sQLDataToUpload.Fill(excelrecords);
                        dataGrid1.ItemsSource = excelrecords.DefaultView;
                        sQLDataToUpload.Update(excelrecords);
                    }
                }
                catch (OleDbException fileloaderror)
                {
                    MessageBox.Show(fileloaderror.ToString());
                }

            }

        }

        private void All_PO_Upload_Click(object sender, RoutedEventArgs e)
        {
            if (User_Delivery_Date.SelectedDate == null)
            {
                string No_Date_Message = "Please select an order date";
                string No_Date = "Please Try Again";
                MessageBox.Show(No_Date_Message, No_Date);
            }

            else
            {
                OpenFileDialog Marcket_File_Read = new OpenFileDialog();
                string Desktop_Upload = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Marcket_File_Read.InitialDirectory = Desktop_Upload;
                Marcket_File_Read.RestoreDirectory = true;
                Marcket_File_Read.Filter = "Excel Fie|*.xlsx";
                Marcket_File_Read.Title = "Save an MI9 Upload Allocation";
                Marcket_File_Read.ShowDialog();

                if (Marcket_File_Read.ShowDialog() == true)
                {
                    try
                    {
                        string excel_path_upload = Marcket_File_Read.FileName;
                        string Lorisdb = ConfigurationManager.ConnectionStrings["HomeDevDatabase"].ConnectionString;
                        using (OleDbConnection dbUploadExcelCon = new OleDbConnection())
                        {
                            //Opening Excel file and getting Allocation records and setting it to a DataSet "excelrecords"
                            string excelconnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excel_path_upload + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                            using (OleDbConnection ExcelUploadExcelCon = new OleDbConnection(excelconnectionString))
                            {
                                ExcelUploadExcelCon.Open();
                                OleDbCommand POsinexcelData = new OleDbCommand("SELECT Code, VendorCode,ProductCode, LocationCode," +
                                    "ProductFirstCost, SKUCode, SKUQuantity,SKUExtraCostsCostCode, SKUExtraCostsCostValue FROM [MI9_Store_Order_Sheet$]", ExcelUploadExcelCon);

                                //----------------------------------------------------------------------------------------------------------------------------------------
                                //DO NOT DELETE! Full PO Parametere mockup for furthur capabilities expantion: 
                                //OleDbCommand excelData = new OleDbCommand("SELECT id, poid, Code, VendorCode, Authorized, OrderDate, DeliveryDate, DeliverUntilDate," +
                                //" CancelDate, ShipDate, LocationCode, ChannelCode, TypeCode, SettleType, BuyingMethod, PurchaseType, PayMethod," +
                                //" TransportMethod, BuyerCode, SeasonCode, VendorPromoCode, TargetYrWeek, AckReference, Reference1, Reference2," +
                                //" CurrencyCode, CurrRate, CalculatePOExtraCosts, OpenOrder, PriceListCode, PriceListCurrRate, OrderType, AllowBo," +
                                //" ProductCode, ProductFirstCost, ProductUnitRetail, ProductSalesUnit, SKUCode, SKUQuantity, SKUUnitRetail, SKUCloseLine," +
                                //" SKUExtraCostsCostCode, SKUExtraCostsCostValue, crDate, processDate, processResponse, numTries FROM [MI9_Store_Order_Sheet$]", ExcelUploadExcelCon);
                                //DbDataReader reading_excelfile = excelData.ExecuteReader();

                                OleDbDataAdapter sQLDataToUpload = new OleDbDataAdapter(POsinexcelData);
                                DataTable excelRecords = new DataTable("MI9_Store_Order_Sheet$");
                                sQLDataToUpload.Fill(excelRecords);
                                ExcelUploadExcelCon.Close();

                                //Getting GUI PO parameters
                                //string OrderDate = "0000-00-00";
                                //string CancelDate = "0000-00-00";
                                //string Shipdate = "0000-00-00";
                                string Delivery = User_Delivery_Date.SelectedDate.ToString();
                                string Type = User_PO_Type.SelectedValue.ToString();
                                string Channel = User_PO_Channel.SelectedValue.ToString();
                                string Payment = User_PO_Payment.SelectedValue.ToString();
                                string PO_Status = User_PO_Status.SelectedValue.ToString();
                                //string Settlement = User_PO_Settlement.SelectedValue.ToString();
                                string Backorder = User_PO_Backorder.SelectedValue.ToString();
                                string Authorized = User_PO_Authorized.SelectedValue.ToString();

                                //Getting unique store vendor combinations in a temporary table
                                DataTable refitemsinPO = excelRecords.DefaultView.ToTable(true, "LocationCode");
                                int numberofvalues = refitemsinPO.Rows.Count;

                                //Setting counters for loops
                                int eachstorePOsecondloop = 0;
                                int eachstoreinref = 0;

                                //Setting progress bar parameters
                                Progressbar.Minimum = 0;
                                Progressbar.Maximum = excelRecords.Rows.Count;

                                foreach (DataRow eachPoRecord in excelRecords.Rows)
                                {
                                    dbUploadExcelCon.ConnectionString = Lorisdb;
                                    using (OleDbCommand poidsp = new OleDbCommand("spPOIncrementIdentity", dbUploadExcelCon))
                                    {
                                        string store = refitemsinPO.Rows[eachstoreinref].ItemArray.GetValue(0).ToString();
                                        string eachstorePOfirstloop = excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(3).ToString();
                                        string SKUQuantitysecondloop = excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(6).ToString();
                                        //Console.WriteLine("Processing for store {0} in Allocation", store);
                                        Status_Update2.Text = "Processing for store " + store + " in allocation file";
                                        if (store == eachstorePOfirstloop && SKUQuantitysecondloop != "0")
                                        {
                                            using (OleDbCommand POupload = new OleDbCommand("{rpc spPurchaseOrderImportIns}", dbUploadExcelCon))
                                            {
                                                dbUploadExcelCon.ConnectionString = Lorisdb;
                                                //Calling the sored procedure "spPurchaseOrderImportIns" on each of the rows with unique store number and vendor
                                                dbUploadExcelCon.Open();
                                                POupload.CommandType = CommandType.StoredProcedure;
                                                POupload.Parameters.Clear();
                                                POupload.Parameters.Add("@poid", OleDbType.Integer).Direction = ParameterDirection.ReturnValue;
                                                POupload.Parameters.AddWithValue("@Code", "");
                                                POupload.Parameters.AddWithValue("@VendorCode", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(1).ToString());
                                                //POupload.Parameters.AddWithValue("@Authorized", Authorized);
                                                //POupload.Parameters.AddWithValue("@OrderDate", OrderDate);
                                                //POupload.Parameters.AddWithValue("@DeliveryDate", Delivery);
                                                //POupload.Parameters.AddWithValue("@DeliverUntilDate", Delivery);
                                                //POupload.Parameters.AddWithValue("@CancelDate", CancelDate);
                                                //POupload.Parameters.AddWithValue("@ShipDate", Shipdate);
                                                POupload.Parameters.AddWithValue("@AllowBo", Backorder);
                                                POupload.Parameters.AddWithValue("@LocationCode", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(3).ToString());
                                                POupload.Parameters.AddWithValue("@ChannelCode", Channel);
                                                POupload.Parameters.AddWithValue("@TypeCode", Type);
                                                //POupload.Parameters.AddWithValue("@SettleType", "0");
                                                POupload.Parameters.AddWithValue("@PayMethod", Payment);
                                                POupload.Parameters.AddWithValue("@OpenOrder", PO_Status);
                                                POupload.Parameters.AddWithValue("@ProductCode", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(2).ToString());
                                                POupload.Parameters.AddWithValue("@ProductFirstCost", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(4).ToString());
                                                POupload.Parameters.AddWithValue("@SKUCode", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(5).ToString());
                                                POupload.Parameters.AddWithValue("@SKUQuantity", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(6).ToString());
                                                POupload.Parameters.AddWithValue("@SKUExtraCostsCostCode", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(7).ToString());
                                                POupload.Parameters.AddWithValue("@SKUExtraCostsCostValue", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(8).ToString());
                                                POupload.ExecuteNonQuery();
                                                dbUploadExcelCon.Close();
                                                eachstorePOsecondloop++;

                                            }
                                        }
                                        else
                                        {
                                            dbUploadExcelCon.Open();
                                            poidsp.CommandType = CommandType.StoredProcedure;
                                            poidsp.ExecuteNonQuery();
                                            dbUploadExcelCon.Close();
                                            eachstoreinref++;
                                            eachstorePOsecondloop++;
                                            continue;
                                        }
                                    }
                                    Progressbar.Value++;
                                }
                                MessageBox.Show("PO Upload Complete!");
                            }
                        }
                    }
                    catch (OleDbException fileloaderror)
                    {
                        MessageBox.Show(fileloaderror.ToString());
                        //Status_Update.Text = "Upload Failed. Server busy please try again";
                    }
                    catch (Exception all_exeptions)
                    {
                        MessageBox.Show(all_exeptions.ToString());
                        //Status_Update.Text = "Upload Failed. Please review excel sheet for proper formatting";
                    }
                    finally
                    {

                    }
                }
            }
        }

        private void User_Delivery_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_Delivery_Date = sender as DatePicker;
            string out_Delivery_Date = chosen_Delivery_Date.SelectedDate.ToString();
            string.Format("{0:yyyy-mm-dd}", out_Delivery_Date);
        }

        private void User_PO_Type_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Types = new List<string>();
            PO_Types.Add("OF");
            PO_Types.Add("OP");
            PO_Types.Add("PRO");
            var PO_Types_List = sender as ComboBox;
            User_PO_Type.ItemsSource = PO_Types;
            User_PO_Type.SelectedIndex = 0;
        }

        private void User_PO_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Type = sender as ComboBox;
            string out_PO_Type = chosen_PO_Type.SelectedValue as string;
        }

        private void User_PO_Channel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Channel = sender as ComboBox;
            string out_PO_Channel = chosen_PO_Channel.SelectedItem as string;
        }

        private void User_PO_Channel_Loaded(object sender, RoutedEventArgs e)
        {
            //Ref MI( GLU code CHCD
            List<string> PO_Channel = new List<string>();
            PO_Channel.Add("DS");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Channel.ItemsSource = PO_Channel;
            User_PO_Channel.SelectedIndex = 0;
        }

        private void User_PO_Payment_Loaded(object sender, RoutedEventArgs e)
        {
            //Ref MI( GLU code CHCD
            List<string> PO_Payment = new List<string>();
            PO_Payment.Add("2");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Payment.ItemsSource = PO_Payment;
            User_PO_Payment.SelectedIndex = 0;
        }

        private void User_PO_Payment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Payment = sender as ComboBox;
            string out_PO_Payment = chosen_PO_Payment.SelectedItem as string;
        }

        private void User_PO_Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Status = sender as ComboBox;
            string out_PO_Status = chosen_PO_Status.SelectedItem as string;
        }

        private void User_PO_Status_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Status = new List<string>();
            PO_Status.Add("N");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Status.ItemsSource = PO_Status;
            User_PO_Status.SelectedIndex = 0;
        }

        private void User_PO_Backorder_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Backorder = new List<string>();
            PO_Backorder.Add("N");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Backorder.ItemsSource = PO_Backorder;
            User_PO_Backorder.SelectedIndex = 0;
        }

        private void User_PO_Backorder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Backorder = sender as ComboBox;
            string out_PO_Backorder = chosen_PO_Backorder.SelectedItem as string;
        }

        private void User_PO_Authorized_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Authorized = new List<string>();
            PO_Authorized.Add("N");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Authorized.ItemsSource = PO_Authorized;
            User_PO_Authorized.SelectedIndex = 0;
        }

        private void User_PO_Authorized_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Authorized = sender as ComboBox;
            string out_PO_Authorized = chosen_PO_Authorized.SelectedItem as string;
        }

        private void All_PO_Preview_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
