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
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using Allocation_Upload_Program;

namespace Allocation_Upload_Program
{
    /// <summary>
    /// Interaction logic for Login_Window.xaml
    /// </summary>
    public partial class Login_Window : Window
    {
        public Login_Window()
        {
            ///Adding opening splash screen and opening login screen
            SplashScreen splash = new SplashScreen("Lori's-Logo-Print.png");
            splash.Show(false, true);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            timer.Stop();
            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
            InitializeComponent();
        }
        private const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds 
        private const int SPLASH_FADE_TIME = 500;     // Miliseconds 
        public string user { get; set; }

        private void Upload_Login_Button_Click(object sender, RoutedEventArgs e)
        {
            /// Logic for login screen
            string user = User_Name.Text;
            string password = User_Password.Password;

            if (User_Name.Text.Length == 0)
            {
                Error_Message.Text = "Please enter user name";
            }
            else if (!Regex.IsMatch(User_Name.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                Error_Message.Text = "Please enter valid user name";
            }
            else if (user != "admin@lorisgifts.com" && user != "purchasing@lorisgifts.com")
            {
                Error_Message.Text = "Please enter a valid user ID";
            }
            else
            {
                if (user == "admin@lorisgifts.com" && password == "Purchasing2019")
                {
                    Admin_Window win2 = new Admin_Window();
                    win2.Show();
                    this.Close();
                }
                if (user == "purchasing@lorisgifts.com" && password == "Purchasing1")
                {
                    General_User_Panel win3 = new General_User_Panel();
                    win3.Show();
                    this.Close();
                }
            }
        }
    }
}
