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
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Dotter
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer refreshTimer = new DispatcherTimer();
        DispatcherTimer clockTimer = new DispatcherTimer();

        GlobalHook GlobalHooker = new GlobalHook();

        Thread ProcessDataThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalHook.GlobalAction += GlobalHook_GlobalAction;
            
            MainFunction.serverLog += "Dotter Initialization\n";
            MainFunction.serverLogTime += DateTime.Now.ToLongTimeString() + '\n';

            MainFunction.LoadSettings();

            try
            {
                MapImg.Source = new BitmapImage(new Uri(MapImgTb.Text));
            }
            catch (Exception)
            {
                DebugTb.Text += "MapImg.Source : Exception occured\n";
            }

            ProcessDataThread = new Thread(new ParameterizedThreadStart(MainFunction.ThreadlizedProcessData));
            ProcessDataThread.Start(ServerAddrTb.Text);

            refreshTimer.Interval = new TimeSpan(0, 0, 15);
            refreshTimer.Tick += refreshTimer_Tick;
            refreshTimer.Start();

            clockTimer.Interval = new TimeSpan(0, 0, 1);
            clockTimer.Tick += clockTimer_Tick;
            clockTimer.Start();

            this.Topmost = true;
        }

        private void GlobalHook_GlobalAction(object sender, EventArgs e)
        {
            if (ToggleDot.IsChecked == true)
            {
                if (sender.ToString() == "WM_RBUTTONDOWN")
                {
                    DotGrid.Visibility = Visibility.Collapsed;
                }
                else if (sender.ToString() == "WM_RBUTTONUP")
                {
                    DotGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (ProcessDataThread.ThreadState != System.Threading.ThreadState.Running)
            {
                ProcessDataThread = new Thread(new ParameterizedThreadStart(MainFunction.ThreadlizedProcessData));
                ProcessDataThread.Start(ServerAddrTb.Text);
            }
        }

        private void clockTimer_Tick(object sender, EventArgs e)
        {
            Clock.Text = DateTime.Now.ToLongTimeString();
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock targetObj = sender as TextBlock;
            targetObj.Opacity = 0.35;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock targetObj = sender as TextBlock;
            targetObj.Opacity = 1;
        }

        private void ExitButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbi.Visibility = Visibility.Collapsed;
            this.Close();
            MainFunction.SaveSettings();
        }

        private void MinButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingPanel.Visibility = Visibility.Collapsed;
            MainFunction.SaveSettings();
        }

        private void tbi_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            SettingPanel.Visibility = Visibility.Visible;
        }

        private void DotChanger(object sender, RoutedEventArgs e)
        {
            CheckBox targetObj = sender as CheckBox;

            switch (targetObj.Name)
            {
                case "EllipseCb":
                    DotEllipse.Visibility = targetObj.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case "CrossCb":
                    DotCross.Visibility = targetObj.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case "TacticCb":
                    DotTactic.Visibility = targetObj.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                    break;
            }
        }

        private void ColorPreset(object sender, MouseButtonEventArgs e)
        {
            Rectangle targetObj = sender as Rectangle;

            ColorTb.Text = targetObj.Fill.ToString().Remove(0, 1);
        }

        public void Changer(object sender, TextChangedEventArgs e)
        {
            TextBox targetObj = sender as TextBox;

            switch (targetObj.Name)
            {
                case "ColorTb":
                    try
                    {
                        DotEllipse.Stroke = new SolidColorBrush(MainFunction.ConvertColor(ColorTb.Text));
                    }
                    catch (Exception)
                    {
                        DebugTb.Text += "DotEllipse.Stroke : Exception occured\n";
                    }
                    break;

                case "SizeTb":
                    try
                    {
                        DotEllipseScale.ScaleX = Convert.ToDouble(SizeTb.Text);
                        DotEllipseScale.ScaleY = Convert.ToDouble(SizeTb.Text);
                        DotCrossScale.ScaleX = Convert.ToDouble(SizeTb.Text);
                        DotCrossScale.ScaleY = Convert.ToDouble(SizeTb.Text);
                        DotTacticScale.ScaleX = Convert.ToDouble(SizeTb.Text);
                        DotTacticScale.ScaleY = Convert.ToDouble(SizeTb.Text);
                    }
                    catch (Exception)
                    {
                        DebugTb.Text += "Dot*.Scale? : Exception occured\n";
                    }
                    break;

                case "StrokeTb":
                    try
                    {
                        DotEllipse.StrokeThickness = Convert.ToDouble(StrokeTb.Text);
                    }
                    catch (Exception)
                    {
                        DebugTb.Text += "DotEllipse.StrokeThickness : Exception occured\n";
                    }
                    break;
            }
        }

        private void MapImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image targetObj = sender as Image;

            targetObj.Visibility = Visibility.Collapsed;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid targetObj = sender as Grid;

            targetObj.Opacity = 0.9F;

            switch (targetObj.Name)
            {
                case "DocViewer":
                case "SidePlayer":
                    DocViewer.Visibility = Visibility.Visible;
                    break;

                case "LogViewer":
                case "SideLog":
                    LogViewer.Visibility = Visibility.Visible;
                    break;

                case "SideMap":
                    MapImg.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void PlayerFilterTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainFunction.FilteredParsor(PlayerFilterTb.Text);
            PlayerList.Text = MainFunction.playerList;
        }

        private void PlayerFilterTb_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (DocViewer.IsMouseOver == false)
            {
                DocViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void CrossGap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock targetObj = sender as TextBlock;

            switch (targetObj.Name)
            {
                case "CrossGapM":
                    DotCross1.X2--;
                    DotCross2.X2++;
                    DotCross3.Y2--;
                    DotCross4.Y2++;

                    DotCross1.X1--;
                    DotCross2.X1++;
                    DotCross3.Y1--;
                    DotCross4.Y1++;
                    break;

                case "CrossGapP":
                    DotCross1.X2++;
                    DotCross2.X2--;
                    DotCross3.Y2++;
                    DotCross4.Y2--;

                    DotCross1.X1++;
                    DotCross2.X1--;
                    DotCross3.Y1++;
                    DotCross4.Y1--;
                    break;

                case "CrossGapR":
                    DotCross1.X2 = 20;
                    DotCross2.X2 = 30;
                    DotCross3.Y2 = 20;
                    DotCross4.Y2 = 30;

                    DotCross1.X1 = 10;
                    DotCross2.X1 = 40;
                    DotCross3.Y1 = 10;
                    DotCross4.Y1 = 40;
                    break;
            }
        }

        private void Tb_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox targetObj = sender as TextBox;

            if (e.Key == Key.Enter)
            {
                switch (targetObj.Name)
                {
                    case "MapImgTb":
                        try
                        {
                            MapImg.Source = new BitmapImage(new Uri(MapImgTb.Text));
                        }
                        catch (Exception)
                        {
                            DebugTb.Text += "MapImg.Source : Exception occured\n";
                        }
                        break;

                    case "ServerAddrTb":
                        if (ProcessDataThread.ThreadState != System.Threading.ThreadState.Running)
                        {
                            MainFunction.prePlayerList = string.Empty;
                            MainFunction.playerList = string.Empty;
                            ProcessDataThread = new Thread(new ParameterizedThreadStart(MainFunction.ThreadlizedProcessData));
                            ProcessDataThread.Start(ServerAddrTb.Text);
                        }
                        break;
                }
            }
        }

        private void SidePlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ProcessDataThread.ThreadState != System.Threading.ThreadState.Running)
            {
                ProcessDataThread = new Thread(new ParameterizedThreadStart(MainFunction.ThreadlizedProcessData));
                ProcessDataThread.Start(ServerAddrTb.Text);
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid targetObj = sender as Grid;

            targetObj.Opacity = 0.65F;

            switch (targetObj.Name)
            {
                case "DocViewer":
                case "SidePlayer":
                    if (PlayerFilterTb.IsKeyboardFocused == false)
                    {
                        DocViewer.Visibility = Visibility.Collapsed;
                    }
                    break;

                case "LogViewer":
                case "SideLog":
                    LogViewer.Visibility = Visibility.Collapsed;
                    break;

                case "SideMap":
                    MapImg.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void DebugCb_Click(object sender, RoutedEventArgs e)
        {
            if (DebugCb.IsChecked == true)
            {
                DebugTb.Visibility = Visibility.Visible;
            }
            else
            {
                DebugTb.Visibility = Visibility.Collapsed;
                DebugTb.Text = string.Empty;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalHook.EndHook();
        }
    }

    public static class MainFunction
    {
        const string FAILED_TO_PARSING = "!-Failed to parsing data\n!-Invaild Data\n";

        static MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        static RequestData requestData = new RequestData();
        static Parsor parsor = new Parsor();

        static DispatcherTimer refreshTimer = new DispatcherTimer();

        public static RequestData.QueryData targetData;

        public static string playerCount = "0/0", serverLog, serverLogTime, playerList = string.Empty, prePlayerList = string.Empty;

        public static void ThreadlizedProcessData(object target)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate { mainWindow.Cursor = Cursors.Wait; }));
            string temp = target as string;
            ProcessData((string)target);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate { mainWindow.Cursor = Cursors.Arrow; }));
        }

        public static void ProcessData(string target)
        {
            string targetIP;
            int targetPort;

            try
            {
                targetIP = target.Split(':')[0];
                targetPort = Convert.ToInt32(target.Split(':')[1]);
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    mainWindow.DebugTb.Text += "Parsing_targetIPPORT : Exception occured\n";
                }));

                return;
            }

            try
            {
                targetData = requestData.Querying(targetIP, targetPort);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    mainWindow.DebugTb.Text += targetData.QUERYING_LOG;
                }));
            }
            catch (Exception)
            {
                playerCount = "Off-L";
                prePlayerList = string.Empty;
                playerList = "Server failed to respond to query";
                serverLog += "Server failed to respond to query\n";
                serverLogTime += DateTime.Now.ToLongTimeString() + '\n';

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    mainWindow.SidePlayerTb.Text = playerCount;
                    mainWindow.PlayerList.Text = playerList;
                    mainWindow.LogList.Text = serverLog;
                    mainWindow.LogTimeList.Text = serverLogTime;
                    mainWindow.DebugTb.Text += "requestData.Quering -> targetData : Exception occured\n";
                }));

                return;
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                try
                {
                    playerCount = parsor.ParsePlayerCount(targetData.A2S_INFO);
                    mainWindow.SidePlayerTb.Text = playerCount;
                }
                catch (Exception)
                {
                    mainWindow.SidePlayerTb.Text = "Off-L";
                    mainWindow.DebugTb.Text += "parsor.ParsePlayerCount -> playerCount -> SidePlayerTb.Text : Exception occured\n";
                }

                try
                {
                    playerList = parsor.ParsePlayerList(targetData.A2S_PLAYER, mainWindow.PlayerFilterTb.Text);
                    if (playerList == "\0\n")
                    {
                        playerList = "!-CHANCE TO LOOT AIRDROP!";
                        mainWindow.PlayerList.Text = playerList;
                        playerList = string.Empty;
                    }
                    else
                    {
                        mainWindow.PlayerList.Text = playerList;
                    }
                }
                catch (Exception)
                {
                    mainWindow.PlayerList.Text = FAILED_TO_PARSING;
                    mainWindow.DebugTb.Text += "parsor.ParsePlayerList -> playerList -> PlayerList.Text : Exception occured\n";
                }

                try
                {
                    PushNotify(playerList, prePlayerList);

                    mainWindow.LogList.Text = serverLog;
                    mainWindow.LogTimeList.Text = serverLogTime;
                }
                catch (Exception)
                {
                    mainWindow.DebugTb.Text += "PushNotify : Exception occured\n";
                }
                
                mainWindow.DocViewerScroll.ScrollToBottom();
                mainWindow.LogViewerScroll.ScrollToBottom();
            }));
        }

        public static void PushNotify(string now, string pre)
        {
            if ((pre != string.Empty && pre != now) && now != FAILED_TO_PARSING)
            {
                string[] nowTemp = now.Split('\n'),
                preTemp = pre.Split('\n');
                string temp = string.Empty;
                string tempTime = string.Empty;

                foreach (string nowData in nowTemp)
                {
                    int count = 0;

                    foreach (string preData in preTemp)
                    {
                        if (nowData == preData)
                        {
                            count++;
                        }
                    }

                    if (count == 0)
                    {
                        temp += nowData + " joined the game" + '\n';
                        tempTime += DateTime.Now.ToLongTimeString() + '\n';
                    }
                }

                foreach (string preData in preTemp)
                {
                    int count = 0;

                    foreach (string nowData in nowTemp)
                    {
                        if (preData == nowData)
                        {
                            count++;
                        }
                    }

                    if (count == 0)
                    {
                        temp += preData + " left the game" + '\n';
                        tempTime += DateTime.Now.ToLongTimeString() + '\n';
                    }
                }

                if (temp != string.Empty)
                {
                    serverLog += temp;
                    serverLogTime += tempTime;
                    temp = temp.Remove(temp.Length - 1);
                    tempTime = tempTime.Remove(tempTime.Length - 1);
                    mainWindow.NotifyName.Text = temp;
                    mainWindow.NotifyGrid.BeginStoryboard(mainWindow.NotifyName.FindResource("PopAnimation") as Storyboard);
                }
            }

            prePlayerList = now;
        }

        public static void FilteredParsor(string targetText)
        {
            playerList = parsor.ParsePlayerList(targetData.A2S_PLAYER, targetText);
        }

        public static Color ConvertColor(string hexColor)
        {
            byte a = (byte)(Convert.ToUInt32(hexColor.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hexColor.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hexColor.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hexColor.Substring(6, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }

        public static void LoadSettings()
        {
            mainWindow.ToggleDot.IsChecked = Properties.Settings.Default._DOTTOGGLE;
            mainWindow.ColorTb.Text = Properties.Settings.Default._DOTCOLOR;
            mainWindow.SizeTb.Text = Properties.Settings.Default._DOTSIZE;
            mainWindow.StrokeTb.Text = Properties.Settings.Default._DOTSTROKE;
            mainWindow.MapImgTb.Text = Properties.Settings.Default._MAPIMGURL;
            mainWindow.ServerAddrTb.Text = Properties.Settings.Default._SERVERID;
            mainWindow.EllipseCb.IsChecked = Properties.Settings.Default._DOTELLIPSE;
            mainWindow.CrossCb.IsChecked = Properties.Settings.Default._DOTCROSS;
            mainWindow.TacticCb.IsChecked = Properties.Settings.Default._DOTTACTIC;

            mainWindow.DotCross1.X2 = Properties.Settings.Default._DOTCROSSX1;
            mainWindow.DotCross2.X2 = Properties.Settings.Default._DOTCROSSX2;
            mainWindow.DotCross3.Y2 = Properties.Settings.Default._DOTCROSSX1;
            mainWindow.DotCross4.Y2 = Properties.Settings.Default._DOTCROSSX2;
            mainWindow.DotCross1.X1 = Properties.Settings.Default._DOTCROSSY1;
            mainWindow.DotCross2.X1 = Properties.Settings.Default._DOTCROSSY2;
            mainWindow.DotCross3.Y1 = Properties.Settings.Default._DOTCROSSY1;
            mainWindow.DotCross4.Y1 = Properties.Settings.Default._DOTCROSSY2;
        }

        public static void SaveSettings()
        {
            Properties.Settings.Default._DOTTOGGLE = Convert.ToBoolean(mainWindow.ToggleDot.IsChecked);
            Properties.Settings.Default._DOTCOLOR = mainWindow.ColorTb.Text;
            Properties.Settings.Default._DOTSIZE = mainWindow.SizeTb.Text;
            Properties.Settings.Default._DOTSTROKE = mainWindow.StrokeTb.Text;
            Properties.Settings.Default._MAPIMGURL = mainWindow.MapImgTb.Text;
            Properties.Settings.Default._SERVERID = mainWindow.ServerAddrTb.Text;
            Properties.Settings.Default._DOTELLIPSE = Convert.ToBoolean(mainWindow.EllipseCb.IsChecked);
            Properties.Settings.Default._DOTCROSS = Convert.ToBoolean(mainWindow.CrossCb.IsChecked);
            Properties.Settings.Default._DOTTACTIC = Convert.ToBoolean(mainWindow.TacticCb.IsChecked);

            Properties.Settings.Default._DOTCROSSX1 = Convert.ToInt16(mainWindow.DotCross1.X2);
            Properties.Settings.Default._DOTCROSSX2 = Convert.ToInt16(mainWindow.DotCross2.X2);
            Properties.Settings.Default._DOTCROSSY1 = Convert.ToInt16(mainWindow.DotCross1.X1);
            Properties.Settings.Default._DOTCROSSY2 = Convert.ToInt16(mainWindow.DotCross2.X1);

            Properties.Settings.Default.Save();
        }
    }
}
