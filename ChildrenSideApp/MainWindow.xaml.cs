using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ChildrenSideApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string FILEPATH = "C:\\Users\\PC\\VNU-HCMUS\\VŨ CÔNG DUY - he dieu hanh\\thuc muc sync\\test.txt";
        public MainWindow()
        {
            InitializeComponent();
            //init
        }

        //log in
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //test CTime class
            //CTime schedule = new CTime(1, 2, 3, 4, 5, 6, 7);
            //schedule.From.Hour =2;
            //schedule.From.Minute =2;
            //schedule.To.Hour =2;
            //schedule.To.Minute =2;
            //schedule.Duration = 2;
            //schedule.Duration =2;
            //schedule.Interupt = 2;
            //schedule.Sum = 2;
            //string msg = $"{schedule.From.Hour},{schedule.From.Minute},{schedule.To.Hour},{schedule.To.Minute},{schedule.Duration},{schedule.Interupt},{schedule.Sum},";
            //MessageBox.Show(msg);

            //if logged in then hide the app and run base on actor
            //this.ShowInTaskbar = false;
            //this.Hide();


            Children();
        }

        void Children()
        {
            CTime schedule = ReadSchedule(FILEPATH);

            //while (true)
            //{
            //    String filename = DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";
            //    int screenLeft = (int)SystemParameters.VirtualScreenLeft;
            //    int screenTop = (int)SystemParameters.VirtualScreenTop;
            //    int screenWidth = (int)SystemParameters.VirtualScreenWidth;
            //    int screenHeight = (int)SystemParameters.VirtualScreenHeight;
            //    Bitmap bitmap_Screen = new Bitmap(screenWidth, screenHeight);
            //    Graphics g = Graphics.FromImage(bitmap_Screen);
            //    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap_Screen.Size);
            //    bitmap_Screen.Save("C:\\Users\\PC\\VNU-HCMUS\\VŨ CÔNG DUY - he dieu hanh\\thuc muc sync\\" + filename);
            //    Thread.Sleep(5000);
            //}

        }
        CTime ReadSchedule(string FILEPATH)
        {
            //set duration and sum to max, interup to 0 at first
            CTime schedule = new CTime(0, 0, 0, 0, 9999999, 0, 999999999);

            //Read schedule file
            //File.WriteAllText(FILEPATH, "");
            //StreamWriter sw = new StreamWriter(FILEPATH);
            //sw.WriteLine("Hello World!!1234");
            //sw.WriteLine("From the StreamWriter class");
            //sw.Close();
            StreamReader sr = new StreamReader(FILEPATH);
            string line = sr.ReadLine();
            string[] arr = line.Split(' ');
            foreach (string part in arr)
            {
                if (part[0] == 'F')
                {
                    string hourStr = part.Substring(1, 2);
                    string minuteStr = part.Substring(4, 2);
                    schedule.From.Hour = Int32.Parse(hourStr);
                    schedule.From.Minute = Int32.Parse(minuteStr);
                }
                if (part[0] == 'T')
                {
                    string hourStr = part.Substring(1, 2);
                    string minuteStr = part.Substring(4, 2);
                    schedule.To.Hour = Int32.Parse(hourStr);
                    schedule.To.Minute = Int32.Parse(minuteStr);
                }
                if (part[0] == 'T')
                {
                    string hourStr = part.Substring(1, 2);
                    string minuteStr = part.Substring(4, 2);
                    schedule.To.Hour = Int32.Parse(hourStr);
                    schedule.To.Minute = Int32.Parse(minuteStr);
                }
                if (part[0] == 'D')
                {
                    string durationStr = part.Substring(1, part.Length - 1);
                    schedule.Duration = Int32.Parse(durationStr);
                }
                if (part[0] == 'I')
                {
                    string interuptStr = part.Substring(1, part.Length - 1);
                    schedule.Interupt = Int32.Parse(interuptStr);
                }
                if (part[0] == 'S')
                {
                    string sumStr = part.Substring(1, part.Length - 1);
                    schedule.Sum = Int32.Parse(sumStr);
                }
            }
            sr.Close();
            MessageBox.Show($"From: {schedule.From.Hour},{schedule.From.Minute} To: {schedule.To.Hour},{schedule.To.Minute} Duration: {schedule.Duration} Interupt: {schedule.Interupt} Sum: {schedule.Sum}");
            return schedule;
        }
    }
}
