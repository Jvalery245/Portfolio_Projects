using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;

namespace Allocation_Upload_Program
{

    public partial class Admin_Window : Window
    {
        public object worksheetRange { get; private set; }
        public object PO_worksheet { get; private set; }
        public object workbook { get; private set; }
        public object PO_File { get; private set; }
        public OleDbConnection dbUploadExcelCon { get; set; }
        public OleDbConnection ExcelUploadExcelCon { get; set; }

        public Admin_Window()
        {
            Validation_Engine PO_Check = new Validation_Engine();
            PO_Check.Validating_PO();
            InitializeComponent();
            Status_Update.Text = "Welcome! Please select file for allocation generation.";
            //var currentDate = new DateTime();
            User_Order_Date.DisplayDateStart = DateTime.Today;
            User_Ship_Date.DisplayDateStart = DateTime.Today.AddDays(1);
            User_Delivery_Date.DisplayDateStart = DateTime.Today.AddDays(6);
            User_Cancel_Date.DisplayDateStart = DateTime.Today.AddDays(97);
        }

        private void Admin_All_PO_Upload_Click(object sender, RoutedEventArgs e)
        {
            //Datepicker logic controls for PO restrictions based on MI9 documentation
            if (User_Order_Date.SelectedDate == null|| User_Ship_Date.SelectedDate == null|| User_Cancel_Date.SelectedDate == null|| User_Delivery_Date.SelectedDate == null)
            {
                string No_Date_Message = "Please complete all date requirements";
                string No_Date = "Please Try Again";
                MessageBox.Show(No_Date_Message, No_Date);
            }
            else if (User_Order_Date.SelectedDate > User_Ship_Date.SelectedDate)
            {
                string Wrong_Date_Message = "Order date must be before ship date";
                string Wrong_Date = "Please Try Again";
                MessageBox.Show(Wrong_Date_Message, Wrong_Date);
            }
            else if (User_Order_Date.SelectedDate > User_Delivery_Date.SelectedDate)
            {
                string Wrong_Date_Message = "Order date must be before delivery date";
                string Wrong_Date = "Please Try Again";
                MessageBox.Show(Wrong_Date_Message, Wrong_Date);
            }
            else if (User_Order_Date.SelectedDate < DateTime.Today)
            {
                string Wrong_Date_Message = "Order date cannot be before today";
                string Wrong_Date = "Please Try Again";
                MessageBox.Show(Wrong_Date_Message, Wrong_Date);
            }
            else if (User_Cancel_Date.SelectedDate > DateTime.Today.AddDays(97))
            {
                string Wrong_Date_Message = "Cancel date cannot be greater than 90 days";
                string Wrong_Date = "Please Try Again";
                MessageBox.Show(Wrong_Date_Message, Wrong_Date);
            }
            else
            {
                OpenFileDialog marketFile = new OpenFileDialog();
                string Desktop_Upload = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                marketFile.InitialDirectory = Desktop_Upload;
                marketFile.RestoreDirectory = true;
                marketFile.Filter = "Excel Fie|*.xlsx";
                marketFile.Title = "Save an MI9 Upload Allocation";
                marketFile.ShowDialog();
                if (marketFile.ShowDialog() == true)
                {
                    try
                    {
                        string excelDataPathUpload = marketFile.FileName;
                        string Lorisdb = ConfigurationManager.ConnectionStrings["HomeDevDatabase"].ConnectionString;
                        using (OleDbConnection dbUploadExcelCon = new OleDbConnection())
                        {
                            //Opening Excel file and getting Allocation records and setting it to a DataSet "excel records"
                            string excelconnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excelDataPathUpload + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                            using (OleDbConnection excelUploadExcelCon = new OleDbConnection(excelconnectionString))
                            {
                                excelUploadExcelCon.Open();
                                OleDbCommand POsinexcelData = new OleDbCommand("SELECT Code, VendorCode,ProductCode, LocationCode," +
                                    "ProductFirstCost, SKUCode, SKUQuantity,SKUExtraCostsCostCode, SKUExtraCostsCostValue FROM [MI9_Store_Order_Sheet$]", excelUploadExcelCon);

                                //----------------------------------------------------------------------------------------------------------------------------------------
                                //DO NOT DELETE! Full PO Parameter mock up for further capabilities expansion: 
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
                                excelUploadExcelCon.Close();

                                //Getting GUI PO parameters
                                string OrderDate = User_Order_Date.SelectedDate.ToString();
                                string CancelDate = User_Cancel_Date.SelectedDate.ToString();
                                string Shipdate = User_Ship_Date.SelectedDate.ToString();
                                string Delivery = User_Delivery_Date.SelectedDate.ToString();
                                string Type = User_PO_Type.SelectedValue.ToString();
                                string Channel = User_PO_Channel.SelectedValue.ToString();
                                string Payment = User_PO_Payment.SelectedValue.ToString();
                                string PO_Status = User_PO_Status.SelectedValue.ToString();
                                string Settlement = User_PO_Settlement.SelectedValue.ToString();
                                string Backorder = User_PO_Backorder.SelectedValue.ToString();
                                string Authorized = User_PO_Authorized.SelectedValue.ToString();
                                
                                //Getting unique store vendor combinations in a temporary table
                                DataTable refitemsinPO = excelRecords.DefaultView.ToTable(true, "LocationCode");
                                int numberofvalues = refitemsinPO.Rows.Count;
                                
                                //Setting counters for loops
                                int eachstorePOsecondloop = 0;
                                int eachstoreinref = 0;
                                
                                //Setting progress bar parameters
                                ProgressBar.Minimum = 0;
                                ProgressBar.Maximum = excelRecords.Rows.Count;

                                foreach (DataRow eachPoRecord in excelRecords.Rows)
                                {
                                    dbUploadExcelCon.ConnectionString = Lorisdb;
                                    using (OleDbCommand poidsp = new OleDbCommand("spPOIncrementIdentity", dbUploadExcelCon))
                                    {
                                        string store = refitemsinPO.Rows[eachstoreinref].ItemArray.GetValue(0).ToString();
                                        string eachstorePOfirstloop = excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(3).ToString();
                                        string SKUQuantitysecondloop = excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(6).ToString();
                                        //Console.WriteLine("Processing for store {0} in Allocation", store);
                                        Status_Update.Text = "Processing for store "+store+" in allocation file";
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
                                                POupload.Parameters.AddWithValue("@Authorized", Authorized);
                                                POupload.Parameters.AddWithValue("@OrderDate", OrderDate);
                                                POupload.Parameters.AddWithValue("@DeliveryDate", Delivery);
                                                POupload.Parameters.AddWithValue("@DeliverUntilDate", Delivery);
                                                POupload.Parameters.AddWithValue("@CancelDate", CancelDate);
                                                POupload.Parameters.AddWithValue("@ShipDate", Shipdate);
                                                POupload.Parameters.AddWithValue("@AllowBo", Backorder);
                                                POupload.Parameters.AddWithValue("@LocationCode", excelRecords.Rows[eachstorePOsecondloop].ItemArray.GetValue(3).ToString());
                                                POupload.Parameters.AddWithValue("@ChannelCode", Channel);
                                                POupload.Parameters.AddWithValue("@TypeCode", Type);
                                                POupload.Parameters.AddWithValue("@SettleType", Settlement);
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
                                    ProgressBar.Value++;
                                }
                                MessageBox.Show("PO Upload Complete!");
                            }
                        }
                    }
                    catch (OleDbException fileloaderror)
                    {
                        MessageBox.Show(fileloaderror.ToString());
                        Status_Update.Text = "Upload Failed. Server busy please try again";
                        Environment.Exit(1);
                    }
                    catch (Exception all_exeptions)
                    {
                        MessageBox.Show(all_exeptions.ToString());
                        Status_Update.Text = "Upload Failed. Please review excel sheet for proper formatting.";
                        Environment.Exit(1);
                    }
                    finally
                    {

                    }
                }
            }
        }

        private void Admin_All_PO_Preview_Click(object sender, RoutedEventArgs e)
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

        private void User_Order_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_Order_Date = sender as DatePicker;
            string out_Order_Date = chosen_Order_Date.SelectedDate.ToString();
            string.Format("{0:yyyy-mm-dd}",out_Order_Date);
        }

        private void User_Ship_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_Ship_Date = sender as DatePicker;
            string out_Ship_Date = chosen_Ship_Date.SelectedDate.ToString();
            string.Format("{0:yyyy-mm-dd}", out_Ship_Date);
        }

        private void User_Delivery_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_Delivery_Date = sender as DatePicker;
            string out_Delivery_Date = chosen_Delivery_Date.SelectedDate.ToString();
            string.Format("{0:yyyy-mm-dd}", out_Delivery_Date);
        }

        private void User_Cancel_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_Cancel_Date = sender as DatePicker;
            string out_Cancel_Date = chosen_Cancel_Date.SelectedDate.ToString();
            string.Format("{0:yyyy-mm-dd}", out_Cancel_Date);
        }

        private void User_PO_Type_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Types = new List<string>();
            PO_Types.Add("AR");
            PO_Types.Add("LS");
            PO_Types.Add("OF");
            PO_Types.Add("OP");
            PO_Types.Add("PRO");
            PO_Types.Add("REG");
            PO_Types.Add("ST");
            var PO_Types_List = sender as ComboBox;
            User_PO_Type.ItemsSource = PO_Types;
            User_PO_Type.SelectedIndex = 5;
        }

        private void User_PO_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Type = sender as ComboBox;
            string out_PO_Type = chosen_PO_Type.SelectedValue as string;
        }

        private void User_PO_Channel_Loaded(object sender, RoutedEventArgs e)
        {
            //Ref MI( GLU code CHCD
            List<string> PO_Channel = new List<string>();
            PO_Channel.Add("DS");
            PO_Channel.Add("FT");
            PO_Channel.Add("ST");
            PO_Channel.Add("XD");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Channel.ItemsSource = PO_Channel;
            User_PO_Channel.SelectedIndex = 0;
        }

        private void User_PO_Channel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Channel = sender as ComboBox;
            string out_PO_Channel = chosen_PO_Channel.SelectedItem as string;
        }

        private void User_PO_Payment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Payment = sender as ComboBox;
            string out_PO_Payment = chosen_PO_Payment.SelectedItem as string;
        }

        private void User_PO_Payment_Loaded(object sender, RoutedEventArgs e)
        {
            //Ref MI( GLU code CHCD
            List<string> PO_Payment = new List<string>();
            PO_Payment.Add("1");
            PO_Payment.Add("2");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Payment.ItemsSource = PO_Payment;
            User_PO_Payment.SelectedIndex = 1;
        }

        private void User_PO_Status_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Status = new List<string>();
            PO_Status.Add("N");
            PO_Status.Add("Y");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Status.ItemsSource = PO_Status;
            User_PO_Status.SelectedIndex = 0;
        }

        private void User_PO_Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Status = sender as ComboBox;
            string out_PO_Status = chosen_PO_Status.SelectedItem as string;
        }

        private void User_PO_Settlement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Settlement = sender as ComboBox;
            string out_PO_Settlement = chosen_PO_Settlement.SelectedItem as string;
        }

        private void User_PO_Settlement_Loaded(object sender, RoutedEventArgs e)
        {
            //Refer to GLU code SSET
            List<string> PO_Settlement = new List<string>();
            PO_Settlement.Add("NET30");
            PO_Settlement.Add("01");
            PO_Settlement.Add("05");
            PO_Settlement.Add("07");
            PO_Settlement.Add("1");
            PO_Settlement.Add("10");
            PO_Settlement.Add("14");
            PO_Settlement.Add("15");
            PO_Settlement.Add("2");
            PO_Settlement.Add("2%");
            PO_Settlement.Add("20");
            PO_Settlement.Add("30");
            PO_Settlement.Add("31");
            PO_Settlement.Add("45");
            PO_Settlement.Add("60");
            PO_Settlement.Add("7");
            PO_Settlement.Add("80");
            PO_Settlement.Add("90");
            PO_Settlement.Add("A1");
            PO_Settlement.Add("AA");
            PO_Settlement.Add("ACH");
            PO_Settlement.Add("AD");
            PO_Settlement.Add("AE");
            PO_Settlement.Add("AF");
            PO_Settlement.Add("AI");
            PO_Settlement.Add("BA");
            PO_Settlement.Add("BB");
            PO_Settlement.Add("BE");
            PO_Settlement.Add("BF");
            PO_Settlement.Add("BG");
            PO_Settlement.Add("BI");
            PO_Settlement.Add("BK");
            PO_Settlement.Add("CB");
            PO_Settlement.Add("CC");
            PO_Settlement.Add("CRE");
            PO_Settlement.Add("LL");
            PO_Settlement.Add("N45");
            PO_Settlement.Add("PP");
            PO_Settlement.Add("PRE");
            var PO_Settlement_List = sender as ComboBox;
            User_PO_Settlement.ItemsSource = PO_Settlement;
            User_PO_Settlement.SelectedIndex = 0;
        }

        private void User_PO_Backorder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Backorder = sender as ComboBox;
            string out_PO_Backorder = chosen_PO_Backorder.SelectedItem as string;
        }

        private void User_PO_Backorder_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Backorder = new List<string>();
            PO_Backorder.Add("N");
            PO_Backorder.Add("Y");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Backorder.ItemsSource = PO_Backorder;
            User_PO_Backorder.SelectedIndex = 0;
        }

        private void User_PO_Authorized_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosen_PO_Authorized = sender as ComboBox;
            string out_PO_Authorized = chosen_PO_Authorized.SelectedItem as string;
        }

        private void User_PO_Authorized_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> PO_Authorized = new List<string>();
            PO_Authorized.Add("N");
            PO_Authorized.Add("Y");
            var PO_Channel_List = sender as ComboBox;
            User_PO_Authorized.ItemsSource = PO_Authorized;
            User_PO_Authorized.SelectedIndex = 0;
        }
    }
}
