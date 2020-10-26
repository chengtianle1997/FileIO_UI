using System;
using System.Collections.Generic;
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
using libMetroTunnelDB;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using FileIO;


namespace FileIO_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MetroTunnelDB Database = new MetroTunnelDB();

        public static List<String> DataDiskDir = new List<string>();

        Dictionary<DateTime, DataRecord> record_dict = new Dictionary<DateTime, DataRecord>();

        public static String BackupDiskDir;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize
            DebugWriteLine("程序初始化完成");

            // Update UI
            Thread ui_refresh = new Thread(Refresh_UI);
            ui_refresh.Start();
            //Dispatcher.Invoke(Refresh_UI);
            //DateTime dateTime = DateTime.Now;
            //LineInfoList.Items.Add(new LineInfo("1", "111", Convert.ToSingle(234.5), dateTime));

        }

        // UI Refresh
        public void Refresh_UI()
        {
            // Update LineInfo 
            Refresh_LineInfo();
            Refresh_DeviceInfo();
        }

        public void Refresh_LineInfo()
        {
            Thread refresh_lineinfo = new Thread(Refresh_LineInfo_t);
            refresh_lineinfo.Start();
            //refresh_lineinfo.Join();
            Thread.Sleep(300);
        }

        private void Refresh_LineInfo_t()
        {
            List<libMetroTunnelDB.Line> line = new List<libMetroTunnelDB.Line>();
            Database.QueryLine(ref line);
            Dispatcher.Invoke(new Action(() => { LineInfoList.Items.Clear(); }));
            for(int i = 0; i < line.Count; i++)
            {
                Dispatcher.Invoke(new Action(() => { LineInfoList.Items.Add(
                    new LineInfo(line[i].LineNumber, line[i].LineName, line[i].TotalMileage, line[i].CreateTime)); }));
            }
            DebugWriteLine("线路信息更新完成");
            return;
        }

        public void Refresh_DeviceInfo()
        {
            Thread refresh_deviceinfo = new Thread(Refresh_DeviceInfo_t);
            refresh_deviceinfo.Start();
            //refresh_deviceinfo.Join();
            Thread.Sleep(300);
        }

        private void Refresh_DeviceInfo_t()
        {
            List<DetectDevice> detectDevices = new List<DetectDevice>();
            Database.QueryDetectDevice(ref detectDevices);
            Dispatcher.Invoke(new Action(() => { DetectDeviceInfoList.Items.Clear(); }));
            for(int i = 0; i < detectDevices.Count(); i++)
            {
                Dispatcher.Invoke(new Action(() => { DetectDeviceInfoList.Items.Add(
                    new DetectDeviceInfo(detectDevices[i].DetectDeviceNumber, 
                    detectDevices[i].DetectDeviceName, detectDevices[i].CreateTime)); }));
            }
            DebugWriteLine("设备信息更新完成");
            return;
        }

        // Debug list Writer
        public void DebugWriteLine(String debug_string)
        {
            Dispatcher.Invoke(new Action(() => { DebugList.Items.Add(DateTime.Now.ToString() + ": " + debug_string); }));
            Dispatcher.Invoke(new Action(() => { DebugList.ScrollIntoView(DebugList.Items[DebugList.Items.Count - 1]); }));

            // Show no more than 200 lines
            if (DebugList.Items.Count > 200)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    DebugList.Items.Clear();
                }));
            }
        }

        // Process Bar
        public void MainProcessBarSet(int percentage)
        {
            Dispatcher.Invoke(new Action(() => { MainPbar.Value = percentage; }));
            Dispatcher.Invoke(new Action(() => { MainPercentage.Text = percentage.ToString(); }));
        }

        public void MainProcessBarReset()
        {
            Dispatcher.Invoke(new Action(() => { MainPbar.Value = 0; }));
            Dispatcher.Invoke(new Action(() => { MainPercentage.Text = "0"; }));
        }

        public void SubProcessBarSet(int percentage)
        {
            Dispatcher.Invoke(new Action(() => { SubPbar.Value = percentage; }));
            Dispatcher.Invoke(new Action(() => { SubPbarText.Text = percentage.ToString(); }));
        }

        public void SubProcessBarReset()
        {
            Dispatcher.Invoke(new Action(() => { SubPbar.Value = 0; }));
            Dispatcher.Invoke(new Action(() => { SubPbarText.Text = "0"; }));
        }

        // Button click ------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------
        
        // Line List
        private void Refresh_Line_Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh_LineInfo();
        }

        private void Add_New_Line_Button_Click(object sender, RoutedEventArgs e)
        {
            NewLineDialog new_line_dlg = new NewLineDialog(Database, null);
            new_line_dlg.dlg_closed_event += new NewLineDialog.Dlg_Closed_Event(Refresh_LineInfo);
            new_line_dlg.ShowDialog();
        }

        private void Edit_Line_Option_Click(object sender, RoutedEventArgs e)
        {
            if (LineInfoList.SelectedItems.Count > 1)
            {
                DebugWriteLine("编辑选项仅可作用于单行信息");
                return;
            } 
            if(LineInfoList.SelectedItems.Count < 1)
            {
                DebugWriteLine("未选中任何行");
                return;
            }
            LineInfo selected_line = LineInfoList.SelectedItem as LineInfo;
            NewLineDialog new_line_dlg = new NewLineDialog(Database, selected_line.ToLine());
            new_line_dlg.dlg_closed_event += new NewLineDialog.Dlg_Closed_Event(Refresh_LineInfo);
            new_line_dlg.ShowDialog();
        }

        private void Edit_Device_Option_Click(object sender, RoutedEventArgs e)
        {
            if(DetectDeviceInfoList.SelectedItems.Count > 1)
            {
                DebugWriteLine("编辑选项仅可作用于单行信息");
                return;
            }
            if(DetectDeviceInfoList.SelectedItems.Count < 1)
            {
                DebugWriteLine("未选中任何行");
                return;
            }
            DetectDeviceInfo selected_device = DetectDeviceInfoList.SelectedItem as DetectDeviceInfo;
            NewDeviceDialog new_device_dlg = new NewDeviceDialog(Database, selected_device.ToDevice());
            new_device_dlg.dlg_closed_event += new NewDeviceDialog.Dlg_Closed_Event(Refresh_DeviceInfo);
            new_device_dlg.ShowDialog();
        }

        //Device List

        private void Refresh_Device_Button_Click_1(object sender, RoutedEventArgs e)
        {
            Refresh_DeviceInfo();
        }

        private void Add_New_Device_Button_Click(object sender, RoutedEventArgs e)
        {
            NewDeviceDialog new_device_dlg = new NewDeviceDialog(Database, null);
            new_device_dlg.dlg_closed_event += new NewDeviceDialog.Dlg_Closed_Event(Refresh_DeviceInfo);
            new_device_dlg.ShowDialog();
        }

        private void Clear_DebugList_Button_Click(object sender, RoutedEventArgs e)
        {
            DebugList.Items.Clear();
        }

        //File choice
        private void Data_Disk_Choose_Button_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Multiselect = true;
            //openFileDialog.Title = "请选择数据盘/文件夹";
            //openFileDialog.Filter = "所有文件(*.*)|*.*";

            //if ((bool)openFileDialog.ShowDialog())
            //{
            //    String data_disk_list = "";
            //    // Update DataDiskDir
            //    if(DataDiskDir.Count > 0)
            //        DataDiskDir.Clear();
            //    for(int i = 0; i < openFileDialog.FileNames.Count(); i++)
            //    {
            //        data_disk_list += openFileDialog.FileNames[i] + ";";
            //        DataDiskDir.Add(openFileDialog.FileNames[i].ToString());
            //    }
            //    Data_Disk_Choose_Text.Text = data_disk_list;
            //}

            // new method in Ookii.Dialogs.Wpf
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "请选择数据盘/文件夹";
            dialog.UseDescriptionForTitle = true;
            
            if ((bool)dialog.ShowDialog())
            {
                // Check the folder

                //Update UI and Save the folder selected
                String data_disk_list_text = Data_Disk_Choose_Text.Text;
                data_disk_list_text += dialog.SelectedPath + ";";
                DataDiskDir.Add(dialog.SelectedPath);
                Data_Disk_Choose_Text.Text = data_disk_list_text;
            }
            
        }

        private void Backup_Disk_Choose_Button_Click(object sender, RoutedEventArgs e)
        {
            // new method in Ookii.Dialogs.Wpf
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "请选择备份盘/文件夹";
            dialog.UseDescriptionForTitle = true;

            if ((bool)dialog.ShowDialog())
            {
                // Check the folder

                //Update UI and Save the folder selected
                String backup_disk_list_text = Backup_Disk_Choose_Text.Text;
                backup_disk_list_text = dialog.SelectedPath;
                BackupDiskDir = dialog.SelectedPath;
                Backup_Disk_Choose_Text.Text = backup_disk_list_text;
            }
        }

        private void Data_Disk_Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            Data_Disk_Choose_Text.Clear();
            DataDiskDir.Clear();
        }

        private void Backup_Disk_Clear_Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            Backup_Disk_Choose_Text.Clear();
            BackupDiskDir = "";
        }

        private void Scan_Data_Disk_Button_Click(object sender, RoutedEventArgs e)
        {
            Thread scan_data_disk_t = new Thread(Scan_Data_Disk_t);
            scan_data_disk_t.Start();
            // scan_data_disk_t.Join();
        }

        private void Scan_Data_Disk_t()
        {
            if (DataDiskDir.Count < 1)
            {
                DebugWriteLine("请先选择数据盘路径");
                return;
            }

            DebugWriteLine("开始扫描数据盘");
            Dispatcher.Invoke(new Action(() => { Data_Record_List.Items.Clear(); }));
            record_dict.Clear();
            MainProcessBarReset();

            int[] process_by_step = { 50, 50 };

            int process_per_i = process_by_step[0] / DataDiskDir.Count;

            // Query multiple data disk
            for (int i = 0; i < DataDiskDir.Count; i++)
            {
                // Query time folder
                List<FileNames> time_folder_list = new List<FileNames>();
                GetSystemAllPath.GetallDirectory(time_folder_list, DataDiskDir[i]);
                if (time_folder_list.Count < 1)
                {
                    DebugWriteLine("数据盘" + DataDiskDir[i] + "内未包含数据文件");
                    continue;
                }
                for (int j = 0; j < time_folder_list.Count; j++)
                {
                    DateTime record_time = new DateTime();
                    bool ret = DataAnalyze.TimeFolderToTime(time_folder_list[j].text, ref record_time);
                    if (!ret)
                    {
                        DebugWriteLine("数据盘" + DataDiskDir[i] + "内未包含数据文件");
                        break;
                    }
                    if (record_dict.ContainsKey(record_time))
                    {
                        record_dict[record_time].AddDiskDir(DataDiskDir[i] + "/" + time_folder_list[j].text);
                    }
                    else
                    {
                        record_dict.Add(record_time, new DataRecord(record_dict.Count + 1,
                            DataDiskDir[i] + "/" + time_folder_list[j].text, 0, record_time));
                    }
                    DebugWriteLine("扫描" + DataDiskDir[i] + "/" + time_folder_list[j].text);
                    MainProcessBarSet((int)((i + 1) * process_per_i + j * process_per_i / time_folder_list.Count));
                }
                MainProcessBarSet((i + 1) * process_per_i);
            }
            int process_per_record = process_by_step[1] / record_dict.Count;
            int dict_counter = 0;
            DebugWriteLine("开始检查扫描文件");
            // Query all datarecord for File Size
            foreach (DataRecord dataRecord in record_dict.Values)
            {
                Single file_size = 0;
                for (int i = 0; i < dataRecord.DataDiskDirList.Count; i++)
                {
                    file_size += GetSystemAllPath.GetDirectorySize(dataRecord.DataDiskDirList[i]) / 1000000;
                    MainProcessBarSet(dict_counter * process_per_record + i * process_per_record / dataRecord.DataDiskDirList.Count);
                }
                dataRecord.DataSize = file_size.ToString() + "M";
                Dispatcher.Invoke(new Action(() => { Data_Record_List.Items.Add(dataRecord); }));
                dict_counter++;
                MainProcessBarSet(dict_counter * process_per_record);   
            }
            MainProcessBarSet(100);
            DebugWriteLine("数据盘扫描完成");
        }
    }

    // ListView binding class
    class LineInfo
    {
        public String LineNum { set; get; }
        public String LineName { set; get; }
        public float TotalMileage { set; get; }
        public String CreateTime { set; get; }
        public LineInfo(String _LineNum, String _LineName, float _TotalMileage, DateTime _CreateTime)
        {
            LineNum = _LineNum;
            LineName = _LineName;
            TotalMileage = _TotalMileage;
            //CreateTime = String.Format("{0}/{1}/{2}  {3}:{4}:{5}", _CreateTime.Year, _CreateTime.Month,
            //        _CreateTime.Date, _CreateTime.Hour, _CreateTime.Minute, _CreateTime.Second);
            CreateTime = _CreateTime.ToString();
        }
        public libMetroTunnelDB.Line ToLine()
        {
            libMetroTunnelDB.Line line = new libMetroTunnelDB.Line(LineNum, LineName, TotalMileage, Convert.ToDateTime(CreateTime));
            return line;
        }
    }

    class DetectDeviceInfo
    {
        public String DetectDeviceNumber { set; get; }
        public String DetectDeviceName { set; get; }
        public int LineID { set; get; } // abandoned
        public String CreateTime { set; get; }
        public DetectDeviceInfo(String _DetectDeviceNumber, String _DetectDeviceName, DateTime _CreateTime, int _LineID = 0)
        {
            DetectDeviceNumber = _DetectDeviceNumber;
            DetectDeviceName = _DetectDeviceName;
            LineID = _LineID;
            CreateTime = _CreateTime.ToString();
        }
        public libMetroTunnelDB.DetectDevice ToDevice()
        {
            libMetroTunnelDB.DetectDevice device = new libMetroTunnelDB.DetectDevice(DetectDeviceNumber, DetectDeviceName, LineID, Convert.ToDateTime(CreateTime));
            return device;
        }
    }

    class DataRecord
    {
        public String RecordNum { set; get; }

        public List<String> DataDiskDirList = new List<String>();
        public String DataDiskDir { set; get; }
        public String DataSize { set; get; }
        public String CreateTime { set; get; }

        public DataRecord(int _RecordNum, String _DataDiskDir, Single _DataSize, DateTime _CreateTime)
        {
            RecordNum = _RecordNum.ToString();
            DataDiskDir = _DataDiskDir + "; ";
            DataDiskDirList.Add(_DataDiskDir);
            DataSize = _DataSize.ToString();
            CreateTime = _CreateTime.ToString();
        }

        public void AddDiskDir(String _DataDiskDir)
        {
            DataDiskDir += _DataDiskDir + "; ";
            DataDiskDirList.Add(_DataDiskDir);
        }
    }


}
