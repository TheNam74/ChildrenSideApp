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
        public static string LOGFOLDERPATH, LOGFILEPATH;
        public static Thread th_ShutDownTiming15s= new Thread(()=> ShutDownTiming(5000));
        public static Thread th_ScreenShot;
        public static Thread th_ShowMessageBox;
        public int AttemptToLogin { get; set; }
        
        //Biến để lưu thời gian mở và tắt máy để ghi vào file nhật ký
        CTime log=new CTime(0,0,0,0,0,0,0);
        //Biến lưu thời gian biểu
        CTime schedule = new CTime(0, 0, 0, 0, 9999999, 0, 999999999);
        //Biến báo hiệu thời gian biểu bị thay đổi
        bool isScheduleModified=false;

        //================Các biến mô phỏng thời gian và các biến test================
        int Wait60Minutes = 3000; //3600000;
        int Wait10Minutes = 3000; //600000;
        int Wait15Seconds = 3000; //15000;
        int Wait1Minute = 3000; //15000;
        public static Thread th_two= new Thread(ThreadTwo);
        public MainWindow()
        {
            InitializeComponent();
            //Khởi tạo các giá trị ban đầu cần thiết
            string dateStr = DateTime.Now.ToString("dd-MM-yyyy");
            LOGFOLDERPATH = dateStr + "\\";
            SCHEDULEFILEPATH = ROOTPATH + SCHEDULEFILEPATH;
            LOGFOLDERPATH = ROOTPATH + LOGFOLDERPATH;
            LOGFILEPATH = LOGFOLDERPATH + "log.txt";
            AttemptToLogin = 0;
            //Tạo thread chụp ảnh màn hình với đầu vào là đường dẫn folder nhật ký
            th_ScreenShot = new Thread(() => ScreenShot(LOGFOLDERPATH));
    }

    //log in
    private void Login_Click(object sender, RoutedEventArgs e)
        {
            //Vòng lặp này nghĩa là bắt đầu từ bước lấy mật khẩu, điều kiện là chưa đăng nhập thất bại đủ 3 lần
            //actor 0:stranger, 1: parents, 2: children
            int actor = Loggin(usernameBox.Text,passwordBox.Password);
            if (actor==1)
            {
                MessageBox.Show("Ban la phu huynh, duoc dung may 60p!!!");
                //Nếu thread hẹn giờ tắt máy đang chạy thì tắt đi
                if (th_ShutDownTiming15s.IsAlive)
                    th_ShutDownTiming15s.Abort();
                //Ẩn cho app chạy ngầm
                this.Hide();
                //Thời gian dùng 60 của phụ huynh
                Thread.Sleep(5000);
            }
            else
            {
                //Đọc file thông tin khung giờ được dùng
                ReadSchedule(SCHEDULEFILEPATH);
                MessageBox.Show($"From: {schedule.From.Hour},{schedule.From.Minute} To: {schedule.To.Hour},{schedule.To.Minute} Duration: {schedule.Duration} Interupt: {schedule.Interupt} Sum: {schedule.Sum}");


                //Lấy thời gian hiện tại
                var date = DateTime.Now;
                int currentHour = date.Hour;
                int currentMinute = date.Minute;

                //Nếu đang không trong thời gian được dùng máy
                bool isInSchedule = ((currentHour > schedule.From.Hour || currentHour == schedule.From.Hour && currentMinute >= schedule.From.Minute) &&
                    (currentHour < schedule.To.Hour || currentHour == schedule.To.Hour && currentMinute < schedule.To.Minute));
                bool isInInteruptTime = false;
                if (!isInSchedule || isInInteruptTime)
                {
                    MessageBox.Show("Chua toi thoi gian dung may, thoi gian dung may tiep theo la : ");
                    //Thực hiện song song 2 việc đếm ngược 15s tắt máy và quay lại đăng nhập, nếu đăng nhập kịp thì cho dùng tiếp
                    th_ShutDownTiming15s.Start();
                }
                //Nếu đang trong thời gian dùng máy
                else
                {
                    Children();
                }
            }
            //Tăng số lần đăng nhập thất bại lên, nếu đã đủ 3 lần thì không cho dùng máy trong 10 phút rồi shutdown
            AttemptToLogin++;
            if (AttemptToLogin == 3)
            {
                //TODO, chặn không cho người đung dùng máy
                Thread.Sleep(5000);
                MessageBox.Show("attempt login 3 times, lock 10p then shutdown");
            }

            //Hiển thị lại màn hình chuẩn bị quay lại lấy mật khẩu
            this.Show();
        }


        int Loggin(string username,string password)
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

            //Tạo file text để ghi lịch sử dùng máy(TODO: không phải tạo chỗ này ok?)

            //Đọc thời gian biểu dùng máy
            ReadSchedule(SCHEDULEFILEPATH);
            //MessageBox.Show($"From: {schedule.From.Hour},{schedule.From.Minute} To: {schedule.To.Hour},{schedule.To.Minute} Duration: {schedule.Duration} Interupt: {schedule.Interupt} Sum: {schedule.Sum}");
            //Thông báo thời gian dùng còn lại của máy
            //Lấy thời gian hiện tại
            var date = DateTime.Now;
            int currentHour = date.Hour;
            int currentMinute = date.Minute;

            int to = (int)(((float)schedule.To.Hour + (float)schedule.To.Minute / 60)*60);
            int now= (int)(((float)currentHour + (float)currentMinute / 60) * 60);

            //So sánh duration và khoảng thời gian từ hiện tại tới mốc To trong thời gian biểu, cái nào nhỏ hơn thì là thời gian còn lại
            int MinutesLeft = (to - now) < schedule.Duration ? (to - now) : schedule.Duration;
            MessageBox.Show($"Ban con {MinutesLeft} phut de su dung may tinh");
            //Lưu thời gian bắt đầu dùng máy
            log.From.Hour = currentHour;
            log.From.Minute = currentMinute;

            //Đọc file lịch sử dùng máy
            //List<CTime> logList = ReadLogFile(LOGFILEPATH);

            // Chụp màn hình và lưu lại

            //Bắt đầu vào qúa trình giám sat chạy song song 3 công việc

            th_ScreenShot.Start();
        }

        //Return a CTime by reading from FILEPATH passed in
        void ReadSchedule(string FILEPATH)
        {
            //Khi khởi tạo cho duration và tổng thời gian dùng là max, thời gian interupt là 0
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

        //=================================Các hàm dùng tạo thread====================================
        private static void ShutDownTiming(int miliseconds)
        {
            Thread.Sleep(miliseconds);
            MessageBox.Show($"Shut down({miliseconds})");
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
                Thread.Sleep(Wait1Minute);
            }
            //TODO: Nếu có thay đổi gì thì báo lên biến global
        }


        //=================================Dev Test=============================
        private void Button_Click2(object sender, RoutedEventArgs e)
        {

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // sử dụng Thread để lập trình bất đồng bộ
            Thread th_one = new Thread(ThreadOne);
            //Thread th_two = new Thread(ThreadTwo);
            th_two.Start();
            th_one.Start();
            th_ShutDownTiming15s.Start();
            MessageBox.Show($"Is thread one alive: {th_one.IsAlive}");

            // Chặn luồng tiếp tục cho tới khi các tiến trình th_one và th_two hoàn thành
            //th_one.Join();
            //th_two.Join();

            watch.Stop();

            MessageBox.Show($"Execution Time: {watch.ElapsedMilliseconds} ms");
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

    }
}
