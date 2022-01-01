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
    public partial class MainWindow : Window
    {
        string ROOTPATH = "C:\\Users\\PC\\VNU-HCMUS\\VŨ CÔNG DUY - he dieu hanh\\thuc muc sync\\";
        string SCHEDULEFILEPATH = "schedule.txt";
        string LOGFOLDERPATH, LOGFILEPATH;
        public MainWindow()
        {
            InitializeComponent();
            //init file path
            string dateStr = DateTime.Now.ToString("dd-MM-yyyy");
            LOGFOLDERPATH = dateStr + "\\";
            SCHEDULEFILEPATH = ROOTPATH + SCHEDULEFILEPATH;
            LOGFOLDERPATH = ROOTPATH + LOGFOLDERPATH;
            LOGFILEPATH = LOGFOLDERPATH + "log.txt";

        }

        //log in
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //actor 0:stranger, 1: parents, 2: children
            int actor = 0;
            CTime schedule = ReadSchedule(SCHEDULEFILEPATH);
            //get current time
            var date = DateTime.Now;
            int currentHour=date.Hour;
            int currentMinute=date.Minute;
            //if logged in then hide the app and run in background
            //this.ShowInTaskbar = false;
            //this.Hide();


            if (actor == 1)
            {

            }
            else
            {
                //Computer is not available at current time (either current time not fall in From-To period or in interupt time)
                bool isInSchedule = ((currentHour > schedule.From.Hour || currentHour == schedule.From.Hour && currentMinute >= schedule.From.Minute) &&
                    (currentHour < schedule.To.Hour || currentHour == schedule.To.Hour && currentMinute < schedule.To.Minute));
                bool isInInteruptTime = true ; 
                if (!isInSchedule|| isInInteruptTime)
                {
                    //Notify user about next available time
                    //
                }
            }


            Children();
        }

        void Children()
        {
         
            //create folder to save log and screenshot
            System.IO.Directory.CreateDirectory(LOGFOLDERPATH);

            //create log file or open then write 

            //read schedule
            CTime schedule = ReadSchedule(SCHEDULEFILEPATH);
            MessageBox.Show($"From: {schedule.From.Hour},{schedule.From.Minute} To: {schedule.To.Hour},{schedule.To.Minute} Duration: {schedule.Duration} Interupt: {schedule.Interupt} Sum: {schedule.Sum}");

            //read log file
            List<CTime> logList = ReadLogFile(LOGFILEPATH);
            //foreach (CTime log in logList)
            //    MessageBox.Show($"From: {log.From.Hour},{log.From.Minute} To: {log.To.Hour},{log.To.Minute}");

            // Screenshot
            String FILEPATH = DateTime.Now.ToString("HH-mm") + ".png";
            ScreenShot($"{LOGFOLDERPATH}{FILEPATH}");
        }


        //Return a CTime by reading from FILEPATH passed in
        CTime ReadSchedule(string FILEPATH)
        {
            //set duration and sum to max, interup to 0 at first
            CTime schedule = new CTime(0, 0, 0, 0, 9999999, 0, 999999999);

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
            return schedule;
        }
        //Return a list of CTime by reading from FILEPATH passed in
        List<CTime> ReadLogFile(string FILEPATH)
        {
            List<CTime> logList = new List<CTime>();
            StreamReader sr = new StreamReader(FILEPATH);
            string line,hourStr,minuteStr;
            while ((line = sr.ReadLine()) != null)
            {
                //set duration and sum to max, interup to 0 at first
                CTime schedule = new CTime(0, 0, 0, 0, 9999999, 0, 999999999);
                string[] part = line.Split(' ');
                //read start time
                hourStr = part[0].Substring(1, 2);
                minuteStr = part[0].Substring(4, 2);
                schedule.From.Hour = Int32.Parse(hourStr);
                schedule.From.Minute = Int32.Parse(minuteStr);

                //read close time
                hourStr = part[1].Substring(1, 2);
                minuteStr = part[1].Substring(4, 2);
                schedule.To.Hour = Int32.Parse(hourStr);
                schedule.To.Minute = Int32.Parse(minuteStr);

                logList.Add(schedule);
            }
            sr.Close();
            return logList;
        }
        //Screen shot and save to filepath passed in (filepath should include capture time)
        void ScreenShot(string FILEPATH)
        {
            int screenLeft = (int)SystemParameters.VirtualScreenLeft;
            int screenTop = (int)SystemParameters.VirtualScreenTop;
            int screenWidth = (int)SystemParameters.VirtualScreenWidth;
            int screenHeight = (int)SystemParameters.VirtualScreenHeight;
            Bitmap bitmap_Screen = new Bitmap(screenWidth, screenHeight);
            Graphics g = Graphics.FromImage(bitmap_Screen);
            g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap_Screen.Size);
            bitmap_Screen.Save(FILEPATH);
        }
    }
}
