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
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel;

namespace ChildrenSideApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        string ROOTPATH = "C:\\Users\\PC\\VNU-HCMUS\\VŨ CÔNG DUY - he dieu hanh\\thuc muc sync\\";
        string SCHEDULEFILEPATH = "schedule.txt";
        public static string LOGFOLDERPATH, LOGFILEPATH;
        public static Thread th_ShutDownTiming15s;
        public static Thread th_ScreenShot;
        public static Thread th_ShowMessageBox;
        public static Thread th_UpdateSchedule;
        public static Thread th_CheckTimeUp;
        public int AttemptToLogin { get; set; }

        //Biến để lưu thời gian mở và tắt máy để ghi vào file nhật ký
        CTime log = new CTime(0, 0, 0, 0, 0, 0, 0);
        //Biến lưu thời gian biểu
        CTime schedule = new CTime(0, 0, 0, 0, 9999999, 0, 999999999);
        //Biến báo hiệu thời gian biểu bị thay đổi
        bool isScheduleModified = false;
        //Biến lưu danh sách thời gian biểu (1 dòng trong file là 1 thời gian biểu)
        List<CTime> ScheduleList = new List<CTime>();
        //Biến lưu toàn bộ file thời gian biểu , dùng để kiểm tra nhanh có thay đổi hay không
        string ScheduleFileContent = "";
        //Biến đếm ngược 
        private System.Windows.Forms.Timer timer1;
        private int counter = wait15sec;
        //Biến xem xét có tắt máy hay không
        bool shutdown = true;
        //Biến lưu thời điểm được sử dụng máy tiếp theo
        CTime NextTime = new CTime(0, 0, 0, 0, 9999999, 0, 999999999);



        //================Các biến mô phỏng thời gian và các biến test================
        int Wait60Minutes = 15000; //3600000;
        int Wait10Minutes = 3000; //600000;
        int Wait15Seconds = 3000; //15000;
        int Wait1Minute = 3000; //15000;
        int WaitMsgBox = 3000; //15000;
        static int wait15sec = 15;
        public static Thread th_two = new Thread(ThreadTwo);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            //Khởi tạo các giá trị ban đầu cần thiết
            string dateStr = DateTime.Now.ToString("dd-MM-yyyy");
            LOGFOLDERPATH = dateStr + "\\";
            SCHEDULEFILEPATH = ROOTPATH + SCHEDULEFILEPATH;
            LOGFOLDERPATH = ROOTPATH + LOGFOLDERPATH;
            LOGFILEPATH = LOGFOLDERPATH + "log.txt";
            AttemptToLogin = 0;
            //Tạo thread chụp ảnh màn hình với đầu vào là đường dẫn folder nhật ký
            th_ScreenShot = new Thread(() => ScreenShot(LOGFOLDERPATH));
            //Tạo thread cập nhật thời gian biểu liên tục
            th_UpdateSchedule = new Thread(() => ReadSchedule_Thread(SCHEDULEFILEPATH));
            //Thread 
            th_CheckTimeUp = new Thread(() => CheckTimeUp());

            th_ShutDownTiming15s = new Thread(() => ShutDownTiming());
        }

        //log in
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            //Lấy thời gian hiện tại
            var date = DateTime.Now;
            int currentHour = date.Hour;
            int currentMinute = date.Minute;

            //Lưu thời gian bắt đầu dùng máy
            log.From.Hour = currentHour;
            log.From.Minute = currentMinute;

            //actor 0:stranger, 1: parents, 2: children
            int actor = Loggin(usernameBox.Text, passwordBox.Password);
            if (actor == 1)
            {
                //Nếu đăng nhập đúng là phụ huynh thì reset số lần đăng nhập thất bại về 0
                AttemptToLogin = 0;
                MessageBox.Show("Ban la phu huynh, duoc dung may 60p!!!");

                //Ngừng thread đếm ngược tắt máy
                shutdown = false;
                //Ẩn cho app chạy ngầm
                this.Hide();
                //Thời gian dùng 60 của phụ huynh
                Thread.Sleep(Wait60Minutes);
                //Reset lại biến đếm cho đồng hồ đếm ngược 15s
                counter = wait15sec;
                //Xóa ô mật khẩu
                passwordBox.Clear();
            }
            else
            {
                //Đọc file thông tin khung giờ được dùng
                ReadSchedule(SCHEDULEFILEPATH);

                //MessageBox.Show($"121: readchedule From: {schedule.From.Hour},{schedule.From.Minute} To: {schedule.To.Hour},{schedule.To.Minute} Duration: {schedule.Duration} Interupt: {schedule.Interupt} Sum: {schedule.Sum} \nCalcMLeft: {CalcMinuteLeft(schedule)}");


                //Nếu đang không trong thời gian được dùng 
                if (CalcMinuteLeft(schedule)==0)
                {
                    MessageBox.Show("Chưa tới thời gian dùng máy, thời gian dùng máy tiếp theo là : ");
                    //Thực hiện song song 2 việc đếm ngược 15s tắt máy và quay lại đăng nhập, nếu đăng nhập kịp thì cho dùng tiếp
                    //th_ShutDownTiming15s.Start();
                    ShutDownTiming();
                }
                //Nếu đang trong thời gian dùng máy và đúng mật khẩu của trẻ
                else if (actor == 2)
                {
                    Children();
                }
            }
            //Tăng số lần đăng nhập thất bại lên, nếu đã đủ 3 lần thì không cho dùng máy trong 10 phút rồi shutdown
            ++AttemptToLogin;
            if (AttemptToLogin == 3)
            {
                //TODO, chặn không cho người đung dùng máy
                MessageBox.Show("Đăng nhập quá 3 lần, tắt máy trong 10p");
                Thread.Sleep(Wait10Minutes);
                MessageBox.Show("Shutdown");
                //Process.Start("shutdown", "/s");
            }

            //Hiển thị lại màn hình chuẩn bị quay lại lấy mật khẩu
            this.Show();

        }


        int Loggin(string username, string password)
        {
            if (username == "child" && password == "child")
                return 2;
            else if (username == "parent" && password == "parent")
                return 1;
            else return 0;
        }

        void Children()
        {
            //Tạo thư mục chứa toàn bộ lịch sử dùng máy và ảnh chụp màn hình trong ngày
            System.IO.Directory.CreateDirectory(LOGFOLDERPATH);

            //Đọc thời gian biểu dùng máy
            ReadSchedule(SCHEDULEFILEPATH);
            //Thông báo thời gian dùng còn lại của máy


            //Đọc file lịch sử dùng máy
            //List<CTime> logList = ReadLogFile(LOGFILEPATH);

            // Chụp màn hình và lưu lại

            //Bắt đầu vào qúa trình giám sat chạy song song 3 công việc

            th_ScreenShot.Start();
            th_UpdateSchedule.Start();
            th_CheckTimeUp.Start();
        }

        //Hàm đọc thời gian biểu, nếu thời gian biểu có thay đổi thì hàm cũng sẽ gắn cờ báo hiệu
        void ReadSchedule(string FILEPATH)
        {
            //Kiểm tra xem file thời gian biểu có thay đổi gì không
            string temp = File.ReadAllText(SCHEDULEFILEPATH);
            if (temp == ScheduleFileContent)
                return;

            //Nếu file thời gian biểu có thay đổi thì xóa toàn bộ phần tử có sẵn của danh sách thời gian biểu đọc lại từ đầu
            ScheduleFileContent = temp;
            ScheduleList.Clear();
            //ScheduleFileContent = temp;
            string[] lines = temp.Split('\n');
            foreach (string line in lines)
            {
                //Khi khởi tạo cho duration và tổng thời gian dùng là max, thời gian interupt là 0
                CTime tempSchedule = new CTime(0, 0, 0, 0, 9999999, 0, 999999999);
                string[] arr = line.Split(' ');
                foreach (string part in arr)
                {
                    if (part[0] == 'F')
                    {
                        string hourStr = part.Substring(1, 2);
                        string minuteStr = part.Substring(4, 2);
                        tempSchedule.From.Hour = Int32.Parse(hourStr);
                        tempSchedule.From.Minute = Int32.Parse(minuteStr);
                    }
                    else if (part[0] == 'T')
                    {
                        string hourStr = part.Substring(1, 2);
                        string minuteStr = part.Substring(4, 2);
                        tempSchedule.To.Hour = Int32.Parse(hourStr);
                        tempSchedule.To.Minute = Int32.Parse(minuteStr);
                    }
                    else if (part[0] == 'D')
                    {
                        string durationStr = part.Substring(1, part.Length - 1);
                        tempSchedule.Duration = Int32.Parse(durationStr);
                    }
                    else if (part[0] == 'I')
                    {
                        string interuptStr = part.Substring(1, part.Length - 1);
                        tempSchedule.Interupt = Int32.Parse(interuptStr);
                    }
                    else if (part[0] == 'S')
                    {
                        string sumStr = part.Substring(1, part.Length - 1);
                        tempSchedule.Sum = Int32.Parse(sumStr);
                    }

                    //Xác định thời gian biểu mà khung giờ mở máy thuộc về

                    if ((log.From.Hour > tempSchedule.From.Hour || log.From.Hour == tempSchedule.From.Hour && log.From.Minute >= tempSchedule.From.Minute) &&
                        (log.From.Hour < tempSchedule.To.Hour || log.From.Hour == tempSchedule.To.Hour && log.From.Minute < tempSchedule.To.Minute))
                        schedule = tempSchedule;
                    //Lưu thời gian biểu này vào danh sách thời gian biểu
                    ScheduleList.Add(tempSchedule);
                }
            }

            //Kiểm tra nếu thời gian biểu bị thay đổi thì báo hiệu lên
            //if (tempSchedule.From.Hour != schedule.From.Hour || tempSchedule.From.Minute != schedule.From.Minute || tempSchedule.To.Hour != schedule.To.Hour|| tempSchedule.To.Minute != schedule.To.Minute || tempSchedule.Duration != schedule.Duration || tempSchedule.Interupt != schedule.Interupt || tempSchedule.Sum != schedule.Sum)
            isScheduleModified = true;

        }

        //Hàm đọc file lịch sử dùng máy, trả về danh sách CTime
        List<CTime> ReadLogFile(string FILEPATH)
        {
            List<CTime> logList = new List<CTime>();
            StreamReader sr = new StreamReader(FILEPATH);
            string line, hourStr, minuteStr;
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

        //Tính thời gian dùng còn lại
        int CalcMinuteLeft(CTime schedule)
        {

            var date = DateTime.Now;
            int currentHour = date.Hour;
            int currentMinute = date.Minute;
            //Check có nằm trong lịch không nếu không thì return 0 luôn
            bool isInSchedule = ((currentHour > schedule.From.Hour || currentHour == schedule.From.Hour && currentMinute >= schedule.From.Minute) &&
                    (currentHour < schedule.To.Hour || currentHour == schedule.To.Hour && currentMinute < schedule.To.Minute));
            if (!isInSchedule)
            {
                //Lưu lại thời gian được dùng tiếp theo
                //foreach (CTime s in ScheduleList)
                //{
                //    if(((currentHour > schedule.From.Hour || currentHour == schedule.From.Hour && currentMinute >= schedule.From.Minute) &&
                //    (currentHour < schedule.To.Hour || currentHour == schedule.To.Hour && currentMinute < schedule.To.Minute)))
                //}

                return 0;
            }
            //Quy đổi giờ ra phút để cộng trừ nhân chia cho dễ
            int now = (int)(((float)currentHour + (float)currentMinute / 60) * 60);

            //Tính thời gian từ thời điểm hiện tại tới mốc "To" trong thời gian biểu
            int to = (int)(((float)schedule.To.Hour + (float)schedule.To.Minute / 60) * 60);
            int MinutesLeftTillEnd = to - now;

            //Tính thời gian từ thời điểm hiện tại tới khi hết Duration
            int begin = (int)(((float)log.From.Hour + (float)log.From.Minute / 60) * 60);
            int MinutesLeftTillDuration = schedule.Duration - (now - begin);

            int TotalTimeLeft = schedule.Sum;
            //Nếu tồn tại file log
            if (File.Exists(LOGFILEPATH))
            {
                //Tính thời gian còn lại so với sum
                List<CTime> LogList = ReadLogFile(LOGFILEPATH);
                foreach (CTime log in LogList)
                {
                    int F = log.From.Hour * 60 + log.From.Minute;
                    int T = log.To.Hour * 60 + log.To.Minute;
                    TotalTimeLeft -= T - F;
                }

                //Tính xem có phải đang trong khoảng thời gian interupt không, có thì return 0 luôn
                CTime LatestUseLog = LogList[LogList.Count-1];
                if (LatestUseLog.To.Hour * 60 + LatestUseLog.To.Minute + schedule.Interupt > currentHour * 60 + currentMinute)
                    return 0;
            }

            //MessageBox.Show($"MinutesLeftTillDuration: {MinutesLeftTillDuration}, MinutesLeftTillEnd: {MinutesLeftTillEnd}, TotalTimeLeft: {TotalTimeLeft}");


            //So sánh thời gian từ thời điểm hiện tại tới mốc "To" trong thời gian biểu và thời gian từ thời điểm hiện tại tới khi hết Duration, cái nào nhỏ hơn thì lấy
            //if(MinutesLeftTillDuration< MinutesLeftTillEnd)
            //{
            //    if(MinutesLeftTillDuration< TotalTimeLeft)
            //    {
            //        return MinutesLeftTillDuration;
            //    }
            //    else return 
            //}

            int[] arr ={MinutesLeftTillDuration, MinutesLeftTillEnd, TotalTimeLeft};
            return arr.Min();
        }

        //Hàm ghi lịch sử dùng máy, giá trị truyền vào là thời gian mở và tất(Gộp vào 1 biến dạng CTime)
        void WriteLogFile(CTime log)
        {
            string FromHourStr = log.From.Hour.ToString("00");
            string FromMinuteStr = log.From.Minute.ToString("00");
            string ToHourStr = log.To.Hour.ToString("00");
            string ToMinuteStr = log.To.Minute.ToString("00");

            string logString = $"F{FromHourStr}:{FromMinuteStr} T{ToHourStr}:{ToMinuteStr}";
            if (!File.Exists(LOGFILEPATH))
            {
                //TODO
                File.Create(LOGFILEPATH).Dispose();

                using (TextWriter tw = new StreamWriter(LOGFILEPATH))
                {
                    tw.WriteLine(logString);
                }

            }
            else if (File.Exists(LOGFILEPATH))
            {
                using (StreamWriter sw = File.AppendText(LOGFILEPATH))
                {
                    sw.WriteLine(logString);
                }
            }
        }
        //=================================Các hàm dùng tạo thread====================================
        private void ShutDownTiming()
        {
            shutdown = true;
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second
            timer1.Start();
            CountDownBox.Text = counter.ToString();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter == 0)
            {
                //TODO
                if (shutdown) //Process.Start("shutdown", "/s");
                    MessageBox.Show($"Shut down");

                timer1.Stop();
            }
            CountDownBox.Text = counter.ToString();
        }
        //Screen shot and save to filepath passed in (filepath should include capture time)
        void ScreenShot(string LOGFOLDERPATH)
        {
            while (true)
            {
                String filename = DateTime.Now.ToString("HH-mm") + ".png";
                int screenLeft = (int)SystemParameters.VirtualScreenLeft;
                int screenTop = (int)SystemParameters.VirtualScreenTop;
                int screenWidth = (int)SystemParameters.VirtualScreenWidth;
                int screenHeight = (int)SystemParameters.VirtualScreenHeight;
                Bitmap bitmap_Screen = new Bitmap(screenWidth, screenHeight);
                Graphics g = Graphics.FromImage(bitmap_Screen);
                g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap_Screen.Size);
                bitmap_Screen.Save($"{LOGFOLDERPATH}{filename}");
                Thread.Sleep(Wait1Minute);
            }
        }
        void ReadSchedule_Thread(string FILEPATH)
        {
            while (true)
            {
                ReadSchedule(FILEPATH);
                //Nếu thời gian biểu bị thay đổi thì thông báo thời gian còn lại
                if (isScheduleModified == true)
                {
                    MessageBox.Show($"Ban con {CalcMinuteLeft(schedule)} phut de su dung may tinh");
                    isScheduleModified = false;
                }
                Thread.Sleep(Wait1Minute);
            }
        }

        //Hàm kiểm tra còn 1 phút thì thông báo thời gian mở máy tiếp theo và shutdown(đồng thời viết log xuống)
        void CheckTimeUp()
        {
            while (CalcMinuteLeft(schedule) > 1)
                Thread.Sleep(Wait1Minute);

            //Ghi file log xuống, chưa có thì tạo, có rồi thì ghi tiếp vào file
            var date = DateTime.Now;
            int currentHour = date.Hour;
            int currentMinute = date.Minute;
            log.To.Hour = currentHour;
            log.To.Minute = currentMinute;
            //Thông báo và tắt máy
            MessageBox.Show("Da het thoi gian dung may, tat may sau 1 phut");
            Thread.Sleep(Wait1Minute);
            WriteLogFile(log);

            MessageBox.Show("shutdown");

            //Tắt 2 thread đang chạy song song với thread này
            th_ScreenShot.Abort();
            th_UpdateSchedule.Abort();
            //TODO
            //Process.Start("shutdown", "/s");
            MessageBox.Show($"Shut down");
        }

        //=================================Dev Test=============================
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second
            timer1.Start();
            CountDownBox.Text = counter.ToString();
            //Test đọc log file
            //List<CTime> LogList = ReadLogFile(LOGFILEPATH);
            //foreach (var log in LogList)
            //{
            //    MessageBox.Show($"F{log.From.Hour}:{log.From.Minute} T{log.To.Hour}:{log.To.Minute}");
            //}


            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            //// sử dụng Thread để lập trình bất đồng bộ
            //Thread th_one = new Thread(ThreadOne);
            ////Thread th_two = new Thread(ThreadTwo);
            //th_two.Start();
            //th_one.Start();
            //th_ShutDownTiming15s.Start();
            //MessageBox.Show($"Is thread one alive: {th_one.IsAlive}");

            //// Chặn luồng tiếp tục cho tới khi các tiến trình th_one và th_two hoàn thành
            ////th_one.Join();
            ////th_two.Join();

            //watch.Stop();

            //MessageBox.Show($"Execution Time: {watch.ElapsedMilliseconds} ms");
            //Window window = new Window()
            //{
            //    Visibility = Visibility.Hidden,
            //    // Just hiding the window is not sufficient, as it still temporarily pops up the first time. Therefore, make it transparent.
            //    AllowsTransparency = true,
            //    Background = System.Windows.Media.Brushes.Transparent,
            //    WindowStyle = WindowStyle.None,
            //    ShowInTaskbar = false
            //};
            //window.Show();
            //MessageBox.Show(window, "Titie", "Text");
            ////Thread.Sleep(3000);
            //window.Close();

        }


        //Tạo thread show message box để thông báo mà không bị chặn thread chính của chương trình
        void ShowMessageBox(string message)
        {
            MessageBox.Show(message);
        }

        private static void ThreadOne()
        {
            th_two.Abort();
            Thread.Sleep(2000);
            MessageBox.Show("Thread 1");
        }

        private static void ThreadTwo()
        {
            Thread.Sleep(2000);
            MessageBox.Show("Thread 2");
        }


        public event PropertyChangedEventHandler PropertyChanged;
    } 
}


//MessageBox.Show($"From: {schedule.From.Hour},{schedule.From.Minute} To: {schedule.To.Hour},{schedule.To.Minute} Duration: {schedule.Duration} Interupt: {schedule.Interupt} Sum: {schedule.Sum}");

