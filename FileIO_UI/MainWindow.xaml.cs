using FileIO;
using libMetroTunnelDB;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileIO_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public static List<String> DataDiskDir = new List<string>();

        Dictionary<DateTime, DataRecord> record_dict = new Dictionary<DateTime, DataRecord>();

        public static String BackupDiskDir;

        public static bool MySQL_lock = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += Window_Closing;

            // Initialize
            DataAnalyze.DataAnalyzeInit(this);

            if(!ConfigHandler.ConfigInit())
            {
                Application.Current.Shutdown();
                return;
            }

            MouseDown += DataTab_MouseDown;
            MouseMove += DataTab_MouseMove;
            MouseUp += DataTab_MouseUp;

            DebugWriteLine("程序初始化完成");

            // Update UI
            Thread ui_refresh = new Thread(Refresh_UI);
            ui_refresh.Start();
            //Dispatcher.Invoke(Refresh_UI);
            //DateTime dateTime = DateTime.Now;
            //LineInfoList.Items.Add(new LineInfo("1", "111", Convert.ToSingle(234.5), dateTime));

        }

        // Window closing warning
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(isAnalyzing)
            {
                if (MessageBox.Show("任务正在进行，关闭窗口将导致数据丢失", "取消", MessageBoxButton.OK) == MessageBoxResult.OK)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            
        }

        // UI Refresh
        public void Refresh_UI()
        {
            // Update LineInfo 
            Refresh_LineInfo();
            Refresh_DeviceInfo();
            ResetDatePicker();
            ShowRecordList();
        }

        public void Refresh_LineInfo()
        {
            Thread refresh_lineinfo = new Thread(Refresh_LineInfo_t);
            refresh_lineinfo.Start();
            //refresh_lineinfo.Join();
            //Thread.Sleep(300);
        }

        private void Refresh_LineInfo_t()
        {
            List<libMetroTunnelDB.Line> line = new List<libMetroTunnelDB.Line>();
            MetroTunnelDB Database = new MetroTunnelDB();
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
            //Thread.Sleep(300);
        }

        private void Refresh_DeviceInfo_t()
        {
            List<DetectDevice> detectDevices = new List<DetectDevice>();
            MetroTunnelDB Database = new MetroTunnelDB();
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

        public void DebugReWriteLine(String debug_string)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                DebugList.Items[DebugList.Items.Count - 1] = DateTime.Now.ToString() + ": " + debug_string;
            }));
        }

        // Process Bar
        private static List<int> stage_finished_percent = new List<int>();
        private static int same_process_num = 1;

        private static int process_num = 0;
        private static int task_sum = 0;
        private static int stage_num = 1;

        // _stage_finished_percent list start from 0 and end at 100
        // same_process_num: several process have the same name and same stage_percent
        public void ProcessInit(string process_name, List<int> _stage_finished_percent, int _same_process_num = 1)
        {
            if (_stage_finished_percent.Count < 2 || _stage_finished_percent[0] != 0 || _stage_finished_percent[_stage_finished_percent.Count - 1] != 100 || _same_process_num < 1)
            {
                stage_finished_percent.Clear();
                return;
            }
            if (!IsCheckStageListSorted(_stage_finished_percent))
            {
                stage_finished_percent.Clear();
                return;
            }               
            stage_finished_percent = _stage_finished_percent;
            same_process_num = _same_process_num;
            process_num = 0;
            task_sum = 0;
            MainProcessBarReset();
            SubProcessBarReset();
        }

        // Init stage with its number, process number( >= 1 ) and sum of tasks
        public void MainProcessReport(string stage_name, int _task_sum, int _process_num, int _stage_num)
        {
            if (_process_num < 1 || _task_sum < 1)
                return;
            task_sum = _task_sum;
            process_num = _process_num;
            Dispatcher.Invoke(new Action(() => { SubPBarInfo.Text = stage_name; }));
            if (_stage_num > 0 || _stage_num < stage_finished_percent.Count)
                stage_num = _stage_num;
        }

        // stage_num start from 1 and end at n - 1
        public void SubProcessReport(int task_finished_now, int _task_sum = 0, string stage_name = null, int _stage_num = 0)
        {
            if (stage_finished_percent.Count < 1 || stage_num < 1 || stage_num >= stage_finished_percent.Count)
                return;
            if (_stage_num > 0 && _stage_num < stage_finished_percent.Count)
                stage_num = _stage_num;
            float process_finished_now = (float)(100 * (process_num - 1) / same_process_num + 
                (stage_finished_percent[stage_num - 1] + (stage_finished_percent[stage_num] - stage_finished_percent[stage_num - 1]) * Math.Min((float)task_finished_now / task_sum, 1)) / same_process_num);
            float stage_finished_now = Math.Min((float)task_finished_now / task_sum, 1) * 100;
            if (process_finished_now > 100)
                process_finished_now = 100;
            MainProcessBarSet(process_finished_now);
            if (stage_finished_now > 100)
                stage_finished_now = 100;
            SubProcessBarSet(stage_finished_now);
            if(stage_name != null)
                Dispatcher.Invoke(new Action(() => { SubPBarInfo.Text = stage_name; }));            
        }
        public void MainProcessFinished(int process_num)
        {
            float process_finished_now = 100 * (float)(process_num) / same_process_num;
            if (process_finished_now > 100)
                process_finished_now = 100;
            MainProcessBarSet(process_finished_now);
        }
        public void SubProcessFinished(int stage_num)
        {
            if (stage_finished_percent.Count < 1 || stage_num < 1 || stage_num >= stage_finished_percent.Count)
                return;
            float process_finished_now = 100 * (float)(process_num - 1) / same_process_num + stage_finished_percent[stage_num] / same_process_num;
            int stage_finished_now = 100;
            if (process_finished_now > 100)
                process_finished_now = 100;
            MainProcessBarSet(process_finished_now);
            SubProcessBarSet(stage_finished_now);
        }
        public void ProcessFinished()
        {
            stage_finished_percent.Clear();
            process_num = 0;
            task_sum = 0;
            MainProcessBarSet(100);
        }

        private bool IsCheckStageListSorted(List<int> _stage_finished_percent)
        {
            for(int i = 1; i < _stage_finished_percent.Count; i++)
            {
                if ((_stage_finished_percent[i] - _stage_finished_percent[i - 1]) < 0)
                    return false;
            }
            return true;
        }

        public void MainProcessBarSet(float percentage)
        {
            Dispatcher.Invoke(new Action(() => { MainPbar.Value = percentage; }));
            Dispatcher.Invoke(new Action(() => { MainPercentage.Text = percentage.ToString("0.#"); }));
        }

        public void MainProcessBarReset()
        {
            Dispatcher.Invoke(new Action(() => { MainPbar.Value = 0; }));
            Dispatcher.Invoke(new Action(() => { MainPercentage.Text = "0"; }));
        }

        public void SubProcessBarSet(float percentage)
        {
            Dispatcher.Invoke(new Action(() => { SubPbar.Value = percentage; }));
            Dispatcher.Invoke(new Action(() => { SubPbarText.Text = percentage.ToString("0.##"); }));
        }

        public void SubProcessBarReset()
        {
            Dispatcher.Invoke(new Action(() => { SubPbar.Value = 0; }));
            Dispatcher.Invoke(new Action(() => { SubPbarText.Text = "0"; }));
        }



        // MySQL Lock Manager
        // Check if MySQL is valid
        // return: true - valid; false - invalid
        public bool MySQL_is_valid()
        {
            return !MySQL_lock;
        }

        // Wait until MySQL is valid and manipulate it
        // return: true - success; false - timeout
        public bool Wait_MySQL()
        {
            int time_counter = 0;
            while (MySQL_lock)
            {
                time_counter++;
                Thread.Sleep(50);
                // Timeout: 3000ms
                if (time_counter > 60)
                    return false;
            }
            MySQL_lock = true;
            return true;
        }

        // Release MySQL
        public void Release_MySQL()
        {
            MySQL_lock = false;
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
            MetroTunnelDB Database = new MetroTunnelDB();
            NewLineDialog new_line_dlg = new NewLineDialog(Database, null);
            new_line_dlg.dlg_closed_event += new NewLineDialog.Dlg_Closed_Event(Refresh_LineInfo);
            new_line_dlg.ShowDialog();
        }

        private void Edit_Line_Option_Click(object sender, RoutedEventArgs e)
        {
            MetroTunnelDB Database = new MetroTunnelDB();
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
            MetroTunnelDB Database = new MetroTunnelDB();
            if (DetectDeviceInfoList.SelectedItems.Count > 1)
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

        // Device List

        private void Refresh_Device_Button_Click_1(object sender, RoutedEventArgs e)
        {
            Refresh_DeviceInfo();
        }

        private void Add_New_Device_Button_Click(object sender, RoutedEventArgs e)
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            NewDeviceDialog new_device_dlg = new NewDeviceDialog(Database, null);
            new_device_dlg.dlg_closed_event += new NewDeviceDialog.Dlg_Closed_Event(Refresh_DeviceInfo);
            new_device_dlg.ShowDialog();
        }

        private void Clear_DebugList_Button_Click(object sender, RoutedEventArgs e)
        {
            DebugList.Items.Clear();
        }

        // MySQl Setting
        private void Change_MySQL_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            if (MessageBox.Show("危险，修改配置可能造成程序不可用！", "确认", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseSettingDialog databaseSettingDialog = new DatabaseSettingDialog(ref Database);
                databaseSettingDialog.dlg_closed_event += new DatabaseSettingDialog.Dlg_Closed_Event(Show_MySQL_Setting);
                databaseSettingDialog.ShowDialog();
            }
        }

        public void Show_MySQL_Setting(String address, String user)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                MySQL_Address_Text.Text = address;
                MySQL_User_Text.Text = user;
            }));          
        }

        // File choice
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
                    //MainProcessBarSet((int)((i + 1) * process_per_i + j * process_per_i / time_folder_list.Count));
                }
                //MainProcessBarSet((i + 1) * process_per_i);
            }
            if (record_dict.Count < 1)
            {
                DebugWriteLine("未检索到数据");
            }
            else
            {
                int process_per_record = process_by_step[1] / record_dict.Count;
                int dict_counter = 0;
                DebugWriteLine("开始检查扫描文件");
                // Query all datarecord for File Size
                foreach (DataRecord dataRecord in record_dict.Values)
                {
                    Single file_size = 0;
                    for (int i = 0; i < dataRecord.DataDiskDirList.Count; i++)
                    {
                        file_size += GetSystemAllPath.GetDirectorySize(dataRecord.DataDiskDirList[i]) / (1024 * 1024);
                        //MainProcessBarSet(dict_counter * process_per_record + i * process_per_record / dataRecord.DataDiskDirList.Count);
                    }
                    // Decide the unit (M or G)
                    if(file_size > 1024)
                    {
                        dataRecord.DataSize = ((float)(file_size / 1024)).ToString("0.#") + "G";
                    }
                    else
                    {
                        dataRecord.DataSize = file_size.ToString("0.#") + "M";
                    }                    
                    Dispatcher.Invoke(new Action(() => { Data_Record_List.Items.Add(dataRecord); }));
                    dict_counter++;
                    //MainProcessBarSet(dict_counter * process_per_record);
                }
            }
            
            //MainProcessBarSet(100);
            DebugWriteLine("数据盘扫描完成");
        }

        private static DetectRecordSelect detectRecordSelect;

        internal static DetectRecordSelect DetectRecordSelect { get => detectRecordSelect; set => detectRecordSelect = value; }

        private static List<bool> selected_data_record_valid = new List<bool>();
        
        private static List<DataRecord> selected_data_record = new List<DataRecord>();

        private void Reset_Disk_Record_Button_Click(object sender, RoutedEventArgs e)
        {
            Data_Record_List.Items.Clear();
        }

        public static bool isAnalyzing = false;
        public static bool isDeleting = false;

        private void Analyze_All_Button_Click(object sender, RoutedEventArgs e)
        {
            if (isAnalyzing)
            {
                MessageBox.Show("数据正在导入，请等待完成后重试", "确认", MessageBoxButton.OK);
                return;
            }
            if (isDeleting)
            {
                MessageBox.Show("正在删除数据，请等待完成后重试", "确认", MessageBoxButton.OK);
                return;
            }
            MetroTunnelDB Database = new MetroTunnelDB();
            // Detect_record information collect
            if (LineInfoList.SelectedItems.Count > 1)
            {
                DebugWriteLine("至多选择一个线路");
                return;
            }
            if (LineInfoList.SelectedItems.Count < 1)
            {
                DebugWriteLine("未选中任何线路");
                return;
            }
            LineInfo selected_line = LineInfoList.SelectedItem as LineInfo;

            if (DetectDeviceInfoList.SelectedItems.Count > 1)
            {
                DebugWriteLine("至多选择一个检测设备");
                return;
            }
            if (DetectDeviceInfoList.SelectedItems.Count < 1)
            {
                DebugWriteLine("未选中任何检测设备");
                return;
            }
            DetectDeviceInfo selected_device = DetectDeviceInfoList.SelectedItem as DetectDeviceInfo;

            Single Detect_Distance = 0;
            try
            {
                Detect_Distance = Convert.ToSingle(Detect_Distance_Text.Text);
            }
            catch (System.FormatException)
            {
                ;
            }
            Single Start_Loc = 0;
            try
            {
                Start_Loc = Convert.ToSingle(Start_Mileage_Text.Text);
            }
            catch (System.FormatException)
            {
                ;
            }
            Single Stop_Loc = 0;
            try
            {
                Stop_Loc = Convert.ToSingle(End_Mileage_Text.Text);
            }
            catch (System.FormatException)
            {
                ;
            }

            if (Data_Record_List.SelectedItems.Count < 1)
            {
                DebugWriteLine("至少选中一条数据记录");
                return;
            }
            DetectRecordSelect = new DetectRecordSelect(selected_line, selected_device, Detect_Distance, Start_Loc, Stop_Loc);
            selected_data_record.Clear();
            selected_data_record_valid.Clear();
            for (int i = 0; i < Data_Record_List.SelectedItems.Count; i++)
            {
                selected_data_record.Add(Data_Record_List.SelectedItems[i] as DataRecord);
                //Check DataRecord Existence
                DetectRecord detectRecord = null;
                Database.QueryDetectRecord(ref detectRecord, selected_data_record[i].CreateTime);
                if(detectRecord != null)
                {
                    // Duplicated
                    selected_data_record_valid.Add(false);
                }
                else
                {
                    selected_data_record_valid.Add(true);
                }
            }
            
            //selected_data_record = Data_Record_List.SelectedItems as List<DataRecordDisp>;
            // Confirmation Dialog
            List<String> confirm_info_list = new List<string>();
            confirm_info_list.Add("请确认以下导入数据无误：");
            confirm_info_list.Add("线路名称: " + selected_line.LineName);
            confirm_info_list.Add("设备名称: " + selected_device.DetectDeviceName);
            for(int i = 0; i < selected_data_record.Count; i++)
            {
                if (selected_data_record_valid[i])
                    confirm_info_list.Add("数据记录" + (i + 1).ToString() + ": "
                        + selected_data_record[i].CreateTime + "  " + selected_data_record[i].DataSize);
                else
                    confirm_info_list.Add("数据记录" + (i + 1).ToString() + ": "
                        + selected_data_record[i].CreateTime + "  已存在");
            }
            ConfirmDialog confirmDialog = new ConfirmDialog(confirm_info_list);
            confirmDialog.true_false_event += new ConfirmDialog.TrueFalseDelegate(Analyze_All_Confirm_Process);
            confirmDialog.ShowDialog();           
        }

        private void Analyze_All_Confirm_Process(bool value)
        {
            if (value)
            {
                Thread analyze_all_thread = new Thread(Analyze_All_Button_Click_t);
                analyze_all_thread.Start();
            }
            else
            {
                return;
            }
        }

        public int line_counter = 1;

        private void Analyze_All_Button_Click_t()
        {
            isAnalyzing = true;
            MetroTunnelDB Database = new MetroTunnelDB();
            // Query to find new record_id (record_id start from 1)
            int record_id_max = 0;
            try
            {
                Database.GetMaxDetectRecordID(ref record_id_max);
            }
            catch(System.Exception)
            {
                ;
            }
            record_id_max++;
            
            //Database.InsertIntoDetectRecord(new DetectRecord())
            int query_num = 0;
            int query_all = record_dict.Count;

            bool isFirstDataRecord = true;

            for(int i = 0; i < selected_data_record.Count; i++)
            {
                if (!selected_data_record_valid[i])
                {
                    DebugWriteLine("已存在的记录 " + selected_data_record[i].CreateTime + " 已跳过");
                    continue;
                }

                DataRecord dataRecord = record_dict[Convert.ToDateTime(selected_data_record[i].CreateTime)];

                // Process Management Design --------------------------------------------------------
                // Step 1 : Data CSV reading-------------------------------------- 25% for all cameras
                // Step 2 : Save Timestamp and Decode mjpeg----------------------- 45% for all cameras
                // Step 3 : Merge data from all cameras--------------------------- 15%
                // Step 4 : Merge mjpeg timestamp from all cameras---------------- 15%

                if(isFirstDataRecord)
                {
                    List<int> stage_percent = new List<int>();
                    stage_percent.Add(0);
                    for (int h = 0; h < dataRecord.DataDiskDirList.Count; h++)
                    {
                        // CSV read
                        stage_percent.Add(stage_percent[stage_percent.Count - 1] + 25 / dataRecord.DataDiskDirList.Count);
                        // Save timestamp and Decode
                        stage_percent.Add(stage_percent[stage_percent.Count - 1] + 45 / dataRecord.DataDiskDirList.Count);
                        // Decode
                        // stage_percent.Add(stage_percent[stage_percent.Count - 1] + 25 / dataRecord.DataDiskDirList.Count);
                    }
                    stage_percent.Add(stage_percent[stage_percent.Count - 1] + 15);
                    stage_percent.Add(100);
                    ProcessInit("分析数据和图像", stage_percent, selected_data_record.Count);
                    isFirstDataRecord = false;
                }
                
                // Create DetectRecord
                DebugWriteLine("创建记录 " + dataRecord.CreateTime);
                Database.InsertIntoDetectRecord(new DetectRecord(Convert.ToInt32(DetectRecordSelect.line.LineNum), Convert.ToDateTime(dataRecord.CreateTime),
                    DetectRecordSelect.device.DetectDeviceNumber, DetectRecordSelect.Detect_Distance, DetectRecordSelect.Start_Loc, DetectRecordSelect.Stop_Loc, record_id_max));
                
                DebugWriteLine("扫描 " + dataRecord.CreateTime + " 文件...(" + query_num + "/" + query_all + ")");

                int line_sum = 0, enc_line_sum = 0;
                int stage_counter = 1;

                for (int j = 0; j < dataRecord.DataDiskDirList.Count; j++)
                {
                    DebugWriteLine("开始分析 " + dataRecord.DataDiskDirList[j] + "...");
        
                    //DataAnalyze.ScanFolder(dataRecord.DataDiskDirList[j], record_id_max);
                    string CalResFolder = dataRecord.DataDiskDirList[j] + "\\CalResult";
                    string EncodeFolder = dataRecord.DataDiskDirList[j];
                    // Scan a csv CalResult file for line counting                   
                    List<FileNames> Filelist = new List<FileNames>();
                    GetSystemAllPath.GetallDirectory(Filelist, CalResFolder);
                    if (Filelist.Count() < 1)
                    {
                        DebugWriteLine("未发现数据文件");
                        return;
                    }
                    DebugWriteLine("扫描数据文件" + Filelist[0].text + "...");
                    line_counter = 1;
                    string csvpath = CalResFolder + "\\" + Filelist[0].text;
                    string metercsv = dataRecord.DataDiskDirList[j] + "\\MeterCounter.csv";
                    line_sum = CSVHandler.GetLineCount(csvpath, this);                   
                    DebugReWriteLine("扫描数据文件" + "完成");

                    MainProcessReport("分析数据", (int)(line_sum * 8.1), i + 1, stage_counter);
                    DataAnalyze.ScanCalResult(CalResFolder, metercsv, record_id_max);
                    SubProcessFinished(stage_counter++);
                    // MainProcessFinished(i + 1);

                    // Scan a timestamp csvfile for line counting
                    string EncodeResult = EncodeFolder + "\\EncodeResult";
                    List<FileNames> EncFilelist = new List<FileNames>();
                    GetSystemAllPath.GetallDirectory(EncFilelist, EncodeResult);
                    if(EncFilelist.Count() < 1)
                    {
                        DebugWriteLine("未发现视频文件");
                        return;
                    }
                    DebugWriteLine("扫描视频文件" + EncFilelist[0].text + "...");
                    string mjpeg_csv_path = EncodeResult + "\\" + EncFilelist[0].text + "\\" + EncFilelist[0].children[1].text;
                    enc_line_sum = CSVHandler.GetLineCount(mjpeg_csv_path, this);
                    // Get mjpeg folder size
                    int mjpeg_size = (int)(GetSystemAllPath.GetDirectorySize(EncodeResult)/1000000);
                    int jpeg_size_est = mjpeg_size * 2;

                    MainProcessReport("解析视频", (int)(enc_line_sum * 8.1) + jpeg_size_est, i + 1, stage_counter);
                    line_counter = 1;
                    DataAnalyze.ScanEncodeResult(EncodeFolder, record_id_max);
                    SubProcessFinished(stage_counter++);
                    // MainProcessFinished(i + 1);

                    DebugWriteLine("分析完成 " + dataRecord.DataDiskDirList[j]);
                }
                MainProcessReport("合并数据", line_sum, i + 1, stage_counter);
                DataAnalyze.MergeCalResult(record_id_max);
                SubProcessFinished(stage_counter++);

                MainProcessReport("合并图像", enc_line_sum, i + 1, stage_counter);
                DataAnalyze.MergeEncodeResult(record_id_max);
                SubProcessFinished(stage_counter++);

                query_num++;
                record_id_max++;


            }

            //foreach (DataRecord dataRecord in record_dict.Values)
            //{
            //    // Create DetectRecord
            //    DebugWriteLine("创建记录" + dataRecord.CreateTime);
            //    Database.InsertIntoDetectRecord(new DetectRecord(Convert.ToInt32(DetectRecordSelect.line.LineNum), Convert.ToDateTime(dataRecord.CreateTime), 
            //        DetectRecordSelect.device.DetectDeviceNumber, DetectRecordSelect.Detect_Distance, DetectRecordSelect.Start_Loc, DetectRecordSelect.Stop_Loc, record_id_max));                

            //    DebugWriteLine("扫描" + dataRecord.CreateTime + "文件...(" + query_num + "/" + query_all + ")");
            //    for(int i = 0; i < dataRecord.DataDiskDirList.Count; i++)
            //    {
            //        DebugWriteLine("开始分析" + dataRecord.DataDiskDirList[i] + "...");
            //        DataAnalyze.ScanFolder(dataRecord.DataDiskDirList[i], record_id_max);                   
            //    }
            //    query_num++;
            //    record_id_max++;
            //}
            isAnalyzing = false;
            DebugWriteLine("全部导入成功");           
        }

        // Multi-view list manager
        int list_index_status = 1; // indicator of view index: <=0 -> LineList, 1 -> RecordList(main view), >=2 -> DataList
        DateTime start_date = new DateTime(), end_date = new DateTime(), null_date = new DateTime(); // date choice
        LineListItem selected_line = null;
        RecordListItem selected_record = null;
        DataListItem selected_data = null;
        int maxQuery = 200;
        int QueryFrom = 0;

        // view button background
        SolidColorBrush SelectedColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xAF, 0x53, 0x53, 0x53));
        SolidColorBrush UnSelectedColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        public void ShowLineList()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Line_List.Visibility = System.Windows.Visibility.Visible;
                Record_List.Visibility = System.Windows.Visibility.Hidden;
                Data_List.Visibility = System.Windows.Visibility.Hidden;

                Line_List_View_Button.Background = (System.Windows.Media.Brush)SelectedColor;
                Record_List_View_Button.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Data_List_View_Button.Background = (System.Windows.Media.Brush)UnSelectedColor;

            }));
            
            list_index_status = 0;
            GetLineList();
        }
        public void ShowRecordList()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Record_List.Visibility = System.Windows.Visibility.Visible;
                Line_List.Visibility = System.Windows.Visibility.Hidden;
                Data_List.Visibility = System.Windows.Visibility.Hidden;

                Line_List_View_Button.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Record_List_View_Button.Background = (System.Windows.Media.Brush)SelectedColor;
                Data_List_View_Button.Background = (System.Windows.Media.Brush)UnSelectedColor;
            }));

            list_index_status = 1;
            GetRecordList();
        }
        public void ShowDataList()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List.Visibility = System.Windows.Visibility.Visible;
                Line_List.Visibility = System.Windows.Visibility.Hidden;
                Record_List.Visibility = System.Windows.Visibility.Hidden;

                Line_List_View_Button.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Record_List_View_Button.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Data_List_View_Button.Background = (System.Windows.Media.Brush)SelectedColor;
            }));

            list_index_status = 2;
            GetDataList();
        }

        public void GetLineList()
        {
            Thread get_line_list_thread = new Thread(GetLineList_t);
            get_line_list_thread.Start();
        }

        private void GetLineList_t()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.Line> line = new List<libMetroTunnelDB.Line>();
            Dispatcher.Invoke(new Action(() => { Line_List.Items.Clear(); }));
            try
            {
                Database.QueryLine(ref line);  
            }
            catch(SystemException)
            {
                return;
            }
            for (int i = 0; i < line.Count; i++)
            {
                Dispatcher.Invoke(new Action(() => { Line_List.Items.Add(
                    new LineListItem(line[i].LineNumber, line[i].LineName, line[i].TotalMileage, line[i].CreateTime)); }));
            }
        }

        public void GetRecordList()
        {
            Thread get_detect_record_thread = new Thread(GetRecordList_t);
            get_detect_record_thread.Start();
        }
        private void GetRecordList_t()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.DetectRecord> detectRecords = new List<DetectRecord>();
            Dispatcher.Invoke(new Action(() => { Record_List.Items.Clear(); }));
            try
            {
                if (selected_line == null)
                {
                    if (start_date == null || end_date == null)
                    {
                        Database.QueryDetectRecord(ref detectRecords);
                    }
                    else
                    {
                        Database.QueryDetectRecord(ref detectRecords, start_date, end_date);
                    }
                }
                else
                {
                    List<libMetroTunnelDB.Line> select_line_list = new List<libMetroTunnelDB.Line>();
                    Database.QueryLine(ref select_line_list, selected_line.LineNum);
                    int selected_line_id = (int)select_line_list[0].LineID;
                    if (start_date == null || end_date == null)
                    {
                        Database.QueryDetectRecord(ref detectRecords, selected_line_id);
                    }
                    else
                    {
                        Database.QueryDetectRecord(ref detectRecords, selected_line_id, start_date, end_date);
                    }
                }
            }
            catch(SystemException)
            {
                return;
            }
            
            for (int i = 0; i < detectRecords.Count; i++)
            {
                libMetroTunnelDB.Line detect_line = null;
                libMetroTunnelDB.DetectDevice detect_device = null;
                try
                {                   
                    Database.QueryLine(ref detect_line, detectRecords[i].LineID);                    
                    Database.QueryDetectDevice(ref detect_device, detectRecords[i].DeviceID);
                }
                catch(SystemException)
                {
                    continue;
                }
                Dispatcher.Invoke(new Action(() => { Record_List.Items.Add(
                    new RecordListItem(detectRecords[i].RecordID.ToString(), detect_line.LineName, 
                    detectRecords[i].DetectTime, detectRecords[i].Length, detectRecords[i].Start_Loc, detectRecords[i].Stop_Loc, detect_device.DetectDeviceName)); }));
            }
        }
        public void GetDataList()
        {
            Thread get_data_list_thread = new Thread(GetDataList_t);
            get_data_list_thread.Start();
        }

        private void GetDataList_t()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.DataConv> datas = new List<DataConv>();
            List<libMetroTunnelDB.DataOverview> dataOverviews = new List<DataOverview>();
            if (selected_record == null)
                return;
            QueryFrom = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List.Items.Clear();
            }));
            try
            {
                Database.QueryDataConv(ref datas, Convert.ToInt32(selected_record.RecordNum), QueryFrom, maxQuery);
                Database.QueryDataOverview(ref dataOverviews, Convert.ToInt32(selected_record.RecordNum), QueryFrom, maxQuery);
                QueryFrom += maxQuery;
            }
            catch(SystemException)
            {
                return;
            }                       
            for (int i = 0; i < datas.Count; i++)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Data_List.Items.Add(new DataListItem(dataOverviews[i].Distance, dataOverviews[i].LongAxis, dataOverviews[i].ShortAxis
                        , dataOverviews[i].HorizontalAxis, dataOverviews[i].Rotation, dataOverviews[i].Constriction, dataOverviews[i].Crack));
                }));
            }
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List.Items.Add(new DataListItem("..."));
            }));
        }

        public void GetMoreData()
        {
            Thread get_more_data_thread = new Thread(GetMoreData_t);
            get_more_data_thread.Start();
        }

        private void GetMoreData_t()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.DataConv> datas = new List<DataConv>();
            if (selected_record == null)
                return;
            try
            {
                Database.QueryDataConv(ref datas, Convert.ToInt32(selected_record.RecordNum), QueryFrom, maxQuery);
                QueryFrom += maxQuery;
            }
            catch (SystemException)
            {
                return;
            }
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List.Items.RemoveAt(Data_List.Items.Count - 1);
            }));
            for (int i = 0; i < datas.Count; i++)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Data_List.Items.Add(new DataListItem(datas[i].Distance, 0, 0, 0, 0, false, false));
                }));
            }
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List.Items.Add(new DataListItem("..."));
            }));
        }

        public void ShowViewByIndex(int view_index)
        {
            if (view_index <= 0)
                ShowLineList();
            else if (view_index == 1)
                ShowRecordList();
            else if (view_index >= 2)
                ShowDataList();
        }

        private void Line_List_View_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowLineList();
        }
        
        private void Record_List_View_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowRecordList();
        }

        private void Data_List_View_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowDataList();
        }

        private void Reset_Date_Picker_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetDatePicker();
        }

        private void ResetDatePicker()
        {
            try
            {
                MetroTunnelDB Database = new MetroTunnelDB();
                Database.GetMaxMinDetectRecordTime(ref start_date, ref end_date);
                Dispatcher.Invoke(new Action(() =>
               {
                   Start_Date_Picker.DisplayDateStart = start_date;
                   Start_Date_Picker.DisplayDateEnd = end_date;
                   End_Date_Picker.DisplayDateStart = start_date;
                   End_Date_Picker.DisplayDateEnd = end_date;
                   Start_Date_Picker.SelectedDate = start_date;
                   End_Date_Picker.SelectedDate = end_date;
               }));
                selected_line = null;
                selected_record = null;
                selected_data = null;
                RefreshSelectedConditionText();
            }
            catch(System.Exception)
            {
                ;
            }
            
        }

        private void Start_Date_Picker_Changed(object sender, RoutedEventArgs e)
        {
            start_date = (DateTime)Start_Date_Picker.SelectedDate;
        }

        private void End_Date_Picker_Changed(object sender, RoutedEventArgs e)
        {
            end_date = ((DateTime)End_Date_Picker.SelectedDate).AddDays(1);
        }

        private void Search_Record_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowRecordList();
        }

        public void RefreshSelectedConditionText()
        {
            String text_str = "当前选项: ";
            if (selected_line != null)
            {
                text_str += selected_line.LineName;
            }
            if (selected_record != null)
            {
                text_str += "  " + selected_record.CreateTime;
            }
            if (selected_data != null)
            {
                text_str += "  " + selected_data.DataLoc;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                Selected_Condition_Text.Text = text_str;
            }));
        }

        private void Line_List_Item_DoubleClick(object sender, RoutedEventArgs e)
        {
            selected_line = Line_List.SelectedItem as LineListItem;
            RefreshSelectedConditionText();
            ShowRecordList();
        }

        private void Record_List_Item_DoubleClick(object sender, RoutedEventArgs e)
        {
            selected_record = Record_List.SelectedItem as RecordListItem;
            RefreshSelectedConditionText();
            ShowDataList();
        }

        private void Data_List_Item_DoubleClick(object sender, RoutedEventArgs e)
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            selected_data = Data_List.SelectedItem as DataListItem;
            if (selected_data.DataLoc == "...")
            {
                GetMoreData();
            }
            else
            {
                RefreshSelectedConditionText();
                if (selected_record == null)
                    return;
                // visualize info
                try
                {
                    // Get alive camera index
                    List<int> cam_alive = new List<int>();
                    Database.GetAliveCamEnc(Convert.ToInt32(selected_record.RecordNum), ref cam_alive);
                    // Get frame interval
                    int interval = Database.GetFrameIntervalEnc(Convert.ToInt32(selected_record.RecordNum), cam_alive[0]);
                    if (interval == 0)
                        return;
                    int max_interval = (int)(interval * 1.5);
                    // Find nearest imagedisp
                    List<libMetroTunnelDB.ImageDisp> image_url_list = new List<ImageDisp>();
                    Database.QueryImageDisp(ref image_url_list, Convert.ToInt32(selected_record.RecordNum), 
                        Convert.ToDouble(selected_data.DataLoc) - interval, Convert.ToDouble(selected_data.DataLoc) + interval);
                    
                    List<libMetroTunnelDB.DataConv> data_conv_list = new List<DataConv>();
                    Database.QueryDataConv(ref data_conv_list, Convert.ToInt32(selected_record.RecordNum), 
                        Convert.ToDouble(selected_data.DataLoc), Convert.ToDouble(selected_data.DataLoc));
                    if (data_conv_list.Count >= 1)
                    {
                        ShowSectionImage(data_conv_list[0]);
                        ShowDetail(selected_data);
                    }
                    if (image_url_list.Count >= 1)
                    {
                        // Show image
                        ShowImage(new ImageUrlInput(image_url_list[0].FileUrl, null));
                    }
                }
                catch(System.Exception)
                {
                    return;
                }

            }
            
        }

        private RecordListItem record_to_delete = null;
        private void Delete_Record_Option_Click(object sender, RoutedEventArgs e)
        {
            if(isAnalyzing)
            {
                MessageBox.Show("数据正在导入，请等待完成后重试", "确认", MessageBoxButton.OK);
                return;
            }
            if(isDeleting)
            {
                MessageBox.Show("正在删除数据，请等待完成后重试", "确认", MessageBoxButton.OK);
                return;
            }
            MetroTunnelDB Database = new MetroTunnelDB();
            if (Record_List.SelectedItems.Count > 1)
            {
                DebugWriteLine("记录仅支持逐个删除");
                return;
            }
            if (Record_List.SelectedItems.Count < 1)
            {
                return;
            }
            record_to_delete = Record_List.SelectedItem as RecordListItem;
            if (record_to_delete == null)
                return;
            List<DataConv> data_list_delete = new List<DataConv>();
            try
            {
                Database.QueryDataConv(ref data_list_delete, Convert.ToInt32(record_to_delete.RecordNum));
            }
            catch(System.Exception)
            {
                return;               
            }
            List<String> confirm_list = new List<String>();
            confirm_list.Add("记录删除后不可恢复，请谨慎操作!");
            confirm_list.Add("将被删除的记录信息：");
            confirm_list.Add("记录编号：" + record_to_delete.RecordNum);
            confirm_list.Add("采集时间：" + record_to_delete.CreateTime);
            confirm_list.Add("数据条数：" + data_list_delete.Count);
            ConfirmDialog confirmDialog = new ConfirmDialog(confirm_list);
            confirmDialog.true_false_event += new ConfirmDialog.TrueFalseDelegate(Delete_Record_Process);
            confirmDialog.ShowDialog();
        }

        private void Delete_Record_Process(bool value)
        {
            if (value)
            {
                Thread delete_record_process = new Thread(Delete_Record_Process_t);               
                delete_record_process.Start();               
            }
        }

        // private bool Delete_Process_EndSign = true;
        private void Delete_Record_Process_t()
        {
            isDeleting = true;
            DebugWriteLine("正在删除记录,请不要执行其他操作...");
            MetroTunnelDB Database = new MetroTunnelDB();
            try
            {
                Database.DeleteDetectRecord(Convert.ToInt32(record_to_delete.RecordNum));
            }
            catch (System.Exception)
            {
                DebugReWriteLine("记录删除失败，请重试");
                isDeleting = false;
                return;
            }
            DebugReWriteLine("记录删除成功");
            isDeleting = false;
            ShowRecordList();
        }

        // Preview Area

        public const int section_view_width = 320;
        public const int section_view_height = 320;
        public const int section_view_cx = section_view_width / 2;
        public const int section_view_cy = section_view_height / 2;
        public const int downsample_rate = 8;
        public const int zoom_rate = 20;

        public void ShowSectionImage(libMetroTunnelDB.DataConv dataConv)
        {
            // Generate 2D image
            Bitmap bmp = new Bitmap(section_view_width, section_view_height);
            Graphics g_bmp = Graphics.FromImage(bmp);
            g_bmp.Clear(System.Drawing.Color.Gray);
            for (int i = 0; i < libMetroTunnelDB.DataConv.floatArrLength; i++)
            {
                if (i % downsample_rate == 0)
                {
                    float a_rotate = (float)(270 * Math.PI / 180 - dataConv.a[i]);
                    int px = section_view_cx + (int)(dataConv.s[i] * Math.Cos(a_rotate) / zoom_rate);
                    int py = section_view_cy - (int)(dataConv.s[i] * Math.Sin(a_rotate) / zoom_rate);
                    if (px > 0 && py > 0 && px < section_view_width && py < section_view_height)
                        bmp.SetPixel(px, py, System.Drawing.Color.Blue);
                }
            }

            BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Section_Image.Source = bmpSource;
        }

        public void ShowImage(ImageUrlInput image_url)
        {           
            if (image_url.camn_image[0] != "")
                Cam1_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[0]) as ImageSource;
            if (image_url.camn_image[1] != "")
                Cam2_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[1]) as ImageSource;
            if (image_url.camn_image[2] != "")
                Cam3_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[2]) as ImageSource;
            if (image_url.camn_image[3] != "") 
                Cam4_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[3]) as ImageSource;
            if (image_url.camn_image[4] != "") 
                Cam5_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[4]) as ImageSource;
            if (image_url.camn_image[5] != "") 
                Cam6_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[5]) as ImageSource;
            if (image_url.camn_image[6] != "") 
                Cam7_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[6]) as ImageSource;
            if (image_url.camn_image[7] != "") 
                Cam8_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camn_image[7]) as ImageSource;
            if (image_url.camVO_image != "")
                CamVO_Image.Source = new ImageSourceConverter().ConvertFromString(image_url.camVO_image) as ImageSource;            
        }

        public void ShowDetail(DataListItem dataitem)
        {
            Section_Detail_List.Items.Clear();
            Section_Detail_List.Items.Add(String.Format("里程位置: \n {0}", dataitem.DataLoc));
            Section_Detail_List.Items.Add(String.Format("截面长轴: \n {0} mm", dataitem.LongAxis));
            Section_Detail_List.Items.Add(String.Format("截面短轴: \n {0} mm", dataitem.ShortAxis));
            Section_Detail_List.Items.Add(String.Format("是否收敛: \n {0}", dataitem.Constriction));
            Section_Detail_List.Items.Add(String.Format("存在裂缝: \n {0}", dataitem.Crack));
            Section_Detail_List.Items.Add(String.Format("水平轴: \n {0} mm", dataitem.HorizontalAxis));
            Section_Detail_List.Items.Add(String.Format("滚转角: \n {0}°", dataitem.Rotation));
        }


        // Multi-view list manager in DataTab
        int list_index_status_datatab = 1; // indicator of view index: <=0 -> LineList, 1 -> RecordList(main view), >=2 -> DataList
        DateTime start_date_datatab = new DateTime(), end_date_datatab = new DateTime(); // date choice
        double start_distance_record = 0, end_distance_record = 0;
        double start_distance_datatab = 0, end_distance_datatab = 0, selected_distance_datatab = 0;
        LineListItem selected_line_datatab = null;
        RecordListItem selected_record_datatab = null;
        DataListItem selected_data_datatab = null;
        int QueryFrom_datatab = 0;
        
        public void ShowLineList_DataTab()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Line_List_DataTab.Visibility = System.Windows.Visibility.Visible;
                Record_List_DataTab.Visibility = System.Windows.Visibility.Hidden;
                Data_List_DataTab.Visibility = System.Windows.Visibility.Hidden;

                Line_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)SelectedColor;
                Record_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Data_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)UnSelectedColor;

            }));

            list_index_status_datatab = 0;
            GetLineList_DataTab();
        }
        public void ShowRecordList_DataTab()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Record_List_DataTab.Visibility = System.Windows.Visibility.Visible;
                Line_List_DataTab.Visibility = System.Windows.Visibility.Hidden;
                Data_List_DataTab.Visibility = System.Windows.Visibility.Hidden;

                Line_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Record_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)SelectedColor;
                Data_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)UnSelectedColor;
            }));

            list_index_status_datatab = 1;
            GetRecordList_DataTab();
        }
        public void ShowDataList_DataTab()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List_DataTab.Visibility = System.Windows.Visibility.Visible;
                Line_List_DataTab.Visibility = System.Windows.Visibility.Hidden;
                Record_List_DataTab.Visibility = System.Windows.Visibility.Hidden;

                Line_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Record_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)UnSelectedColor;
                Data_List_View_Button_DataTab.Background = (System.Windows.Media.Brush)SelectedColor;
            }));

            RefreshDistancePicker_DataTab();
            list_index_status_datatab = 2;
            GetDataList_DataTab();
        }

        public void GetLineList_DataTab()
        {
            Thread get_line_list_thread = new Thread(GetLineList_t_DataTab);
            get_line_list_thread.Start();
        }

        private void GetLineList_t_DataTab()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.Line> line = new List<libMetroTunnelDB.Line>();
            Dispatcher.Invoke(new Action(() => { Line_List_DataTab.Items.Clear(); }));
            try
            {
                Database.QueryLine(ref line);
            }
            catch (SystemException)
            {
                return;
            }
            for (int i = 0; i < line.Count; i++)
            {
                Dispatcher.Invoke(new Action(() => {
                    Line_List_DataTab.Items.Add(
                        new LineListItem(line[i].LineNumber, line[i].LineName, line[i].TotalMileage, line[i].CreateTime));
                }));
            }
        }

        public void GetRecordList_DataTab()
        {
            Thread get_detect_record_thread = new Thread(GetRecordList_t_DataTab);
            get_detect_record_thread.Start();
        }
        private void GetRecordList_t_DataTab()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.DetectRecord> detectRecords = new List<DetectRecord>();
            Dispatcher.Invoke(new Action(() => { Record_List_DataTab.Items.Clear(); }));
            try
            {
                if (selected_line_datatab == null)
                {
                    if (start_date_datatab == null || end_date_datatab == null)
                    {
                        Database.QueryDetectRecord(ref detectRecords);
                    }
                    else
                    {
                        Database.QueryDetectRecord(ref detectRecords, start_date_datatab, end_date_datatab);
                    }
                }
                else
                {
                    List<libMetroTunnelDB.Line> select_line_list = new List<libMetroTunnelDB.Line>();
                    Database.QueryLine(ref select_line_list, selected_line_datatab.LineNum);
                    int selected_line_id = (int)select_line_list[0].LineID;
                    if (start_date_datatab == null || end_date_datatab == null)
                    {
                        Database.QueryDetectRecord(ref detectRecords, selected_line_id);
                    }
                    else
                    {
                        Database.QueryDetectRecord(ref detectRecords, selected_line_id, start_date_datatab, end_date_datatab);
                    }
                }
            }
            catch (SystemException)
            {
                return;
            }

            for (int i = 0; i < detectRecords.Count; i++)
            {
                libMetroTunnelDB.Line detect_line = null;
                libMetroTunnelDB.DetectDevice detect_device = null;
                try
                {
                    Database.QueryLine(ref detect_line, detectRecords[i].LineID);
                    Database.QueryDetectDevice(ref detect_device, detectRecords[i].DeviceID);
                }
                catch (SystemException)
                {
                    continue;
                }
                Dispatcher.Invoke(new Action(() => {
                    Record_List_DataTab.Items.Add(
                        new RecordListItem(detectRecords[i].RecordID.ToString(), detect_line.LineName,
                        detectRecords[i].DetectTime, detectRecords[i].Length, detectRecords[i].Start_Loc, detectRecords[i].Stop_Loc, detect_device.DetectDeviceName));
                }));
            }
        }
        public void GetDataList_DataTab()
        {
            Thread get_data_list_thread = new Thread(GetDataList_t_DataTab);
            get_data_list_thread.Start();
        }

        private void GetDataList_t_DataTab()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.DataOverview> dataOverviews = new List<DataOverview>();
            if (selected_record_datatab == null)
                return;
            QueryFrom = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List_DataTab.Items.Clear();
            }));
            try
            {
                Database.QueryDataOverview(ref dataOverviews, Convert.ToInt32(selected_record_datatab.RecordNum), QueryFrom_datatab, maxQuery);
                QueryFrom += maxQuery;
            }
            catch (SystemException)
            {
                return;
            }
            for (int i = 0; i < dataOverviews.Count; i++)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Data_List_DataTab.Items.Add(new DataListItem(dataOverviews[i].Distance, dataOverviews[i].LongAxis, dataOverviews[i].ShortAxis
                        , dataOverviews[i].HorizontalAxis, dataOverviews[i].Rotation, dataOverviews[i].Constriction, dataOverviews[i].Crack));
                }));
            }
            Dispatcher.Invoke(new Action(() =>
            {
                Data_List_DataTab.Items.Add(new DataListItem("..."));
            }));
        }

        public void GetMoreData_DataTab()
        {
            GetMoreData_t_DataTab();
            //Thread get_more_data_thread = new Thread(GetMoreData_t_DataTab);
            //get_more_data_thread.Start();
        }

        private void GetMoreData_t_DataTab()
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            List<libMetroTunnelDB.DataOverview> dataOverviews = new List<DataOverview>();
            if (selected_data_datatab == null)
                return;
            if (Data_List_DataTab.SelectedIndex == 0)
            {
                // Query more from the top
                try
                {
                    DataListItem dataListItem = Data_List_DataTab.Items[1] as DataListItem;
                    Database.QueryDataOverview(ref dataOverviews, Convert.ToInt32(selected_record_datatab.RecordNum), 
                        Convert.ToDouble(dataListItem.DataLoc) - 2 * distance_query_range, Convert.ToDouble(dataListItem.DataLoc));
                    //QueryFrom += maxQuery;
                }
                catch (SystemException)
                {
                    return;
                }
                //Dispatcher.Invoke(new Action(() =>
                //{
                //    Data_List_DataTab.Items.RemoveAt(0);
                //}));
                for (int i = 0; i < dataOverviews.Count; i++)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Data_List_DataTab.Items.Insert(i + 1, new DataListItem(dataOverviews[i].Distance, dataOverviews[i].LongAxis, dataOverviews[i].ShortAxis
                            , dataOverviews[i].HorizontalAxis, dataOverviews[i].Rotation, dataOverviews[i].Constriction, dataOverviews[i].Crack));
                    }));
                }
                //Dispatcher.Invoke(new Action(() =>
                //{
                //    Data_List_DataTab.Items.Add(new DataListItem("..."));
                //}));
            }
            else if (Data_List_DataTab.SelectedIndex == Data_List_DataTab.Items.Count - 1)
            {
                // Query more from the bottom
                try
                {
                    DataListItem dataListItem = Data_List_DataTab.Items[Data_List_DataTab.Items.Count - 2] as DataListItem;
                    Database.QueryDataOverview(ref dataOverviews, Convert.ToInt32(selected_record_datatab.RecordNum), Convert.ToDouble(dataListItem.DataLoc), Convert.ToDouble(dataListItem.DataLoc) + 2 * distance_query_range);
                    //QueryFrom += maxQuery;
                }
                catch (SystemException)
                {
                    return;
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    Data_List_DataTab.Items.RemoveAt(Data_List_DataTab.Items.Count - 1);
                }));
                for (int i = 0; i < dataOverviews.Count; i++)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Data_List_DataTab.Items.Add(new DataListItem(dataOverviews[i].Distance, dataOverviews[i].LongAxis, dataOverviews[i].ShortAxis
                            , dataOverviews[i].HorizontalAxis, dataOverviews[i].Rotation, dataOverviews[i].Constriction, dataOverviews[i].Crack));
                    }));
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    Data_List_DataTab.Items.Add(new DataListItem("..."));
                }));
            }
        }

        private void Line_List_View_Button_DataTab_Click(object sender, RoutedEventArgs e)
        {
            ShowLineList_DataTab();
        }

        private void Record_List_View_Button_DataTab_Click(object sender, RoutedEventArgs e)
        {
            ShowRecordList_DataTab();
        }

        private void Data_List_View_Button_DataTab_Click(object sender, RoutedEventArgs e)
        {
            ShowDataList_DataTab();
        }

        private void Reset_All_Button_DataTab_Click(object sender, RoutedEventArgs e)
        {
            ResetDatePicker_DataTab();
        }

        private void ResetDatePicker_DataTab()
        {
            try
            {
                MetroTunnelDB Database = new MetroTunnelDB();
                Database.GetMaxMinDetectRecordTime(ref start_date_datatab, ref end_date_datatab);
                Dispatcher.Invoke(new Action(() =>
                {
                    Start_Date_Picker_DataTab.DisplayDateStart = start_date_datatab;
                    Start_Date_Picker_DataTab.DisplayDateEnd = end_date_datatab;
                    End_Date_Picker_DataTab.DisplayDateStart = start_date_datatab;
                    End_Date_Picker_DataTab.DisplayDateEnd = end_date_datatab;
                    Start_Date_Picker_DataTab.SelectedDate = start_date_datatab;
                    End_Date_Picker_DataTab.SelectedDate = end_date_datatab;
                }));
                selected_line_datatab = null;
                selected_record_datatab = null;
                selected_data_datatab = null;
                RefreshSelectedConditionText_DataTab();
            }
            catch (System.Exception)
            {
                ;
            }

        }

        private void Start_Date_Picker_DataTab_Changed(object sender, RoutedEventArgs e)
        {
            start_date_datatab = (DateTime)Start_Date_Picker_DataTab.SelectedDate;
        }

        private void End_Date_Picker_DataTab_Changed(object sender, RoutedEventArgs e)
        {
            end_date_datatab = ((DateTime)End_Date_Picker_DataTab.SelectedDate).AddDays(1);
        }

        private void Start_Distance_Text_Changed(object sender, TextChangedEventArgs e)
        {
            if (Start_Distance_Text.Text != null)
            {
                try
                {
                    double start_dist = Convert.ToDouble(Start_Distance_Text.Text);
                    if (start_dist < start_distance_record)
                    {
                        Distance_Input_Warning_Label.Text = "里程范围起点值越界";
                        return;
                    }
                    else
                    {
                        Distance_Input_Warning_Label.Text = "";
                    }
                    start_distance_datatab = start_dist;
                    Distance_Slider.Minimum = start_distance_datatab;
                }
                catch (System.Exception)
                {
                    return;
                }
            }            
        }
        private void End_Distance_Text_Changed(object sender, TextChangedEventArgs e)
        {
            if (End_Distance_Text.Text != null)
            {
                try
                {
                    double end_dist = Convert.ToDouble(End_Distance_Text.Text);
                    if (end_dist > end_distance_record)
                    {
                        Distance_Input_Warning_Label.Text = "里程范围终点值越界";
                        return;
                    }
                    else
                    {
                        Distance_Input_Warning_Label.Text = "";
                    }
                    end_distance_datatab = end_dist;
                    Distance_Slider.Maximum = end_distance_datatab;
                }
                catch (System.Exception)
                {
                    return;
                }
            }
        }

        double distance_query_range = 1500;
        private void Distance_Slider_Dragged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            try
            {
                selected_distance_datatab = Distance_Slider.Value;
                Thread data_slider_query_thread = new Thread(Data_Slider_Query_t);
                data_slider_query_thread.Start();
            }
            catch (System.Exception)
            {
                return;
            }
            
        }

        private void Data_Slider_Query_t()
        {
            try
            {
                if (selected_record_datatab == null)
                    return;
                MetroTunnelDB Database = new MetroTunnelDB();
                List<DataOverview> dataOverviews = new List<DataOverview>();
                Database.QueryDataOverview(ref dataOverviews, Convert.ToInt32(selected_record_datatab.RecordNum), selected_distance_datatab - distance_query_range, selected_distance_datatab + distance_query_range);
                if (dataOverviews.Count < 1)
                {
                    return;
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    Data_List_DataTab.Items.Clear();
                    Data_List_DataTab.Items.Add(new DataListItem("..."));
                }));
                for (int i = 0; i < dataOverviews.Count; i++)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Data_List_DataTab.Items.Add(new DataListItem(dataOverviews[i].Distance, dataOverviews[i].LongAxis, dataOverviews[i].ShortAxis,
                            dataOverviews[i].HorizontalAxis, dataOverviews[i].Rotation, dataOverviews[i].Constriction, dataOverviews[i].Crack));
                    }));
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    Data_List_DataTab.Items.Add(new DataListItem("..."));
                }));
            }
            catch (System.NullReferenceException)
            {
                return;
            }
            catch (System.Exception)
            {
                return;
            }
        }

        private void Search_Record_Button_DataTab_Click(object sender, RoutedEventArgs e)
        {
            ShowRecordList_DataTab();
        }

        public void RefreshSelectedConditionText_DataTab()
        {
            String text_str = "当前选项: ";
            if (selected_line_datatab != null)
            {
                text_str += selected_line_datatab.LineName;
            }
            if (selected_record_datatab != null)
            {
                text_str += "  " + selected_record_datatab.CreateTime;
            }
            if (selected_data_datatab != null)
            {
                text_str += "  " + selected_data_datatab.DataLoc;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                Selected_Condition_Text_DataTab.Text = text_str;
            }));
        }

        public void RefreshDistancePicker_DataTab()
        {
            if (selected_record_datatab == null)
                return;
            try
            {
                MetroTunnelDB Database = new MetroTunnelDB();
                Database.GetMaxMinDetectDataDistance(Convert.ToInt32(selected_record_datatab.RecordNum), ref start_distance_record, ref end_distance_record);
                start_distance_datatab = start_distance_record;
                end_distance_datatab = end_distance_record;
                Start_Distance_Text.Text = start_distance_datatab.ToString();
                End_Distance_Text.Text = end_distance_datatab.ToString();
                Distance_Slider.Minimum = start_distance_datatab;
                Distance_Slider.Maximum = end_distance_datatab;
            }
            catch(System.Exception)
            {
                return;
            }
        }

        private void Line_List_Item_DataTab_DoubleClick(object sender, RoutedEventArgs e)
        {
            selected_line_datatab = Line_List_DataTab.SelectedItem as LineListItem;
            RefreshSelectedConditionText_DataTab();
            ShowRecordList_DataTab();
        }

        private void Record_List_Item_DataTab_DoubleClick(object sender, RoutedEventArgs e)
        {
            selected_record_datatab = Record_List_DataTab.SelectedItem as RecordListItem;
            RefreshSelectedConditionText_DataTab();
            ShowDataList_DataTab();
        }

        private void Data_List_Item_DataTab_DoubleClick(object sender, RoutedEventArgs e)
        {
            MetroTunnelDB Database = new MetroTunnelDB();
            selected_data_datatab = Data_List_DataTab.SelectedItem as DataListItem;
            if (selected_data_datatab.DataLoc == "...")
            {
                GetMoreData_DataTab();
            }
            else
            {
                RefreshSelectedConditionText_DataTab();
                if (selected_record_datatab == null)
                    return;
                // visualize info
                try
                {
                    // Get alive camera index
                    List<int> cam_alive = new List<int>();
                    Database.GetAliveCam(Convert.ToInt32(selected_record_datatab.RecordNum), ref cam_alive);
                    // Get frame interval
                    int interval = Database.GetFrameInterval(Convert.ToInt32(selected_record_datatab.RecordNum), cam_alive[0]);
                    if (interval == 0)
                        return;
                    int max_interval = (int)(interval * 1.5);
                    List<libMetroTunnelDB.DataConv> data_conv_list = new List<DataConv>();
                    Database.QueryDataConv(ref data_conv_list, Convert.ToInt32(selected_record_datatab.RecordNum),
                        Convert.ToDouble(selected_data_datatab.DataLoc), Convert.ToDouble(selected_data_datatab.DataLoc));
                    if (data_conv_list.Count >= 1)
                    {
                        ShowSectionImage_DataTab(data_conv_list[0]);
                        ShowDetail_DataTab(selected_data_datatab);
                    }                    
                }
                catch (System.Exception)
                {
                    return;
                }

            }

        }

        // Preview area
        // Preview area button controller
        // mode 0: preview, mode 1: single point, mode 2: rectangle box, mode 3: ruler
        public static int datatab_mode_index = 0;

        private void Preview_Mode_Button_Click(object sender, RoutedEventArgs e)
        {
            datatab_mode_index = 0;
            Preview_Mode_Button.Background = SelectedColor;
            Single_Point_Mode_Button.Background = UnSelectedColor;
            Rectangle_Box_Mode_Button.Background = UnSelectedColor;
            Ruler_Mode_Button.Background = UnSelectedColor;
            rect_step = 0;
            ruler_step = 0;
            Preview_Mode_Instruction_Text.Text = "预览模式：显示二维截面及其详细信息";
            try
            {
                ShowDetail_DataTab(selected_data_datatab);
                BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Section_Image_DataTab.Source = bmpSource;
            }
            catch(System.Exception)
            {
                return;
            }
            
        }
        private void Single_Point_Mode_Button_Click(object sender, RoutedEventArgs e)
        {
            datatab_mode_index = 1;
            Preview_Mode_Button.Background = UnSelectedColor;
            Single_Point_Mode_Button.Background = SelectedColor;
            Rectangle_Box_Mode_Button.Background = UnSelectedColor;
            Ruler_Mode_Button.Background = UnSelectedColor;
            rect_step = 0;
            ruler_step = 0;
            Preview_Mode_Instruction_Text.Text = "单点采样：单击需要测算的位置";
            try
            {
                Section_Detail_List_DataTab.Items.Clear();
                BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Section_Image_DataTab.Source = bmpSource;
            }
            catch (System.Exception)
            {
                return;
            }
        }
        private void Rectangle_Box_Mode_Button_Click(object sender, RoutedEventArgs e)
        {
            datatab_mode_index = 2;
            Preview_Mode_Button.Background = UnSelectedColor;
            Single_Point_Mode_Button.Background = UnSelectedColor;
            Rectangle_Box_Mode_Button.Background = SelectedColor;
            Ruler_Mode_Button.Background = UnSelectedColor;
            rect_step = 0;
            ruler_step = 0;
            Preview_Mode_Instruction_Text.Text = "框选采样：绘制选框以计算框内点的平均中心距";
            try
            {
                Section_Detail_List_DataTab.Items.Clear();
                BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Section_Image_DataTab.Source = bmpSource;
            }
            catch (System.Exception)
            {
                return;
            }
        }
        private void Ruler_Mode_Button_Click(object sender, RoutedEventArgs e)
        {
            datatab_mode_index = 3;
            Preview_Mode_Button.Background = UnSelectedColor;
            Single_Point_Mode_Button.Background = UnSelectedColor;
            Rectangle_Box_Mode_Button.Background = UnSelectedColor;
            Ruler_Mode_Button.Background = SelectedColor;
            rect_step = 0;
            ruler_step = 0;
            Preview_Mode_Instruction_Text.Text = "距离标尺：单击选点，测量两点间距";
            try
            {
                Section_Detail_List_DataTab.Items.Clear();
                BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Section_Image_DataTab.Source = bmpSource;
            }
            catch (System.Exception)
            {
                return;
            }
        }

        private void Reset_Mode_Button_Click(object sender, RoutedEventArgs e)
        {
            if (datatab_mode_index == 0)
                Preview_Mode_Button_Click(sender, e);
            else if (datatab_mode_index == 1)
                Single_Point_Mode_Button_Click(sender, e);
            else if (datatab_mode_index == 2)
            {
                Rectangle_Box_Mode_Button_Click(sender, e);
                rect_step = 0;
            }               
            else if (datatab_mode_index == 3)
            {
                Ruler_Mode_Button_Click(sender, e);
                ruler_step = 0;
            }

        }

        // Preview area content
        public const int section_view_width_datatab = 650;
        public const int section_view_height_datatab = 650;
        public const int section_view_cx_datatab = section_view_width_datatab / 2;
        public const int section_view_cy_datatab = section_view_height_datatab / 2;
        public const int zoom_rate_datatab = 10;
        public const int downsample_rate_datatab = 4;

        public LLRBTree tree = new LLRBTree();

        Bitmap bmp = new Bitmap(section_view_width_datatab, section_view_height_datatab);

        System.Drawing.Pen CenterPen = new System.Drawing.Pen(System.Drawing.Color.DarkGreen, 3);
        System.Drawing.Pen LinePen = new System.Drawing.Pen(System.Drawing.Color.Red, 2);
        System.Drawing.Pen RectPen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 2);
        System.Drawing.Pen RulerPen = new System.Drawing.Pen(System.Drawing.Color.Red, 2);

        public float GetRealAngle(double px, double py)
        {
            double rx = px - section_view_cx_datatab;
            double ry = section_view_cy_datatab - py;
            double acos = Math.Acos(rx / (Math.Sqrt(rx * rx + ry * ry)));
            if (ry >= 0)
            {
                return (float)acos;
            }
            else
            {
                return (float)(2 * Math.PI - acos);
            }
        }

        public void ShowSectionImage_DataTab(libMetroTunnelDB.DataConv dataConv)
        {
            // Generate 2D image           
            Graphics g_bmp = Graphics.FromImage(bmp);
            g_bmp.Clear(System.Drawing.Color.Gray);
            for (int i = 0; i < libMetroTunnelDB.DataConv.floatArrLength; i++)
            {
                g_bmp.DrawEllipse(CenterPen, section_view_cx_datatab, section_view_cy_datatab, 3, 3);
                if (i % downsample_rate_datatab == 0)
                {
                    if (dataConv.s[i] == 0)
                        continue;
                    float a_rotate = (float)(270 * Math.PI / 180 - dataConv.a[i]);
                    int px = section_view_cx_datatab + (int)(dataConv.s[i] * Math.Cos(a_rotate) / zoom_rate_datatab);
                    int py = section_view_cy_datatab - (int)(dataConv.s[i] * Math.Sin(a_rotate) / zoom_rate_datatab);
                    if (px > 0 && py > 0 && px < section_view_width_datatab && py < section_view_height_datatab)
                        bmp.SetPixel(px, py, System.Drawing.Color.Blue);
                    float ra = GetRealAngle((double)px, (double)py);
                    tree.put(ra, px, py, dataConv.s[i]);
                }
            }

            BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Section_Image_DataTab.Source = bmpSource;
        }      

        public void ShowDetail_DataTab(DataListItem dataitem)
        {
            Section_Detail_List_DataTab.Items.Clear();
            Section_Detail_List_DataTab.Items.Add(String.Format("里程位置: \n {0}", dataitem.DataLoc));
            Section_Detail_List_DataTab.Items.Add(String.Format("截面长轴: \n {0} mm", dataitem.LongAxis));
            Section_Detail_List_DataTab.Items.Add(String.Format("截面短轴: \n {0} mm", dataitem.ShortAxis));
            Section_Detail_List_DataTab.Items.Add(String.Format("是否收敛: \n {0}", dataitem.Constriction));
            Section_Detail_List_DataTab.Items.Add(String.Format("存在裂缝: \n {0}", dataitem.Crack));
            Section_Detail_List_DataTab.Items.Add(String.Format("水平轴: \n {0} mm", dataitem.HorizontalAxis));
            Section_Detail_List_DataTab.Items.Add(String.Format("滚转角: \n {0}°", dataitem.Rotation));
        }

        // Mouse action controller
        public static System.Windows.Point rect_downpoint = new System.Windows.Point(0, 0);
        public static System.Windows.Point ruler_downpoint = new System.Windows.Point(0, 0);
        public static int rect_step = 0; // 0: initial condition, 1: first point chosen
        public static int ruler_step = 0; // 0: initial condition, 1: first point chosen
        public const int rect_point_size = 2;
        public const int ruler_point_size = 2;
        public void DataTab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point downpoint = e.GetPosition(Section_Image_DataTab);
            if (datatab_mode_index == 1)
            {
                if (tree.root == null)
                    return;
                Section_Detail_List_DataTab.Items.Clear();
                Section_Detail_List_DataTab.Items.Add(String.Format("点位置：{0}mm，{1}mm", 
                    ((downpoint.X - section_view_cx_datatab) * zoom_rate_datatab).ToString("#.00"), ((section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab).ToString("#.00")));
                Section_Detail_List_DataTab.Items.Add(String.Format("相对角度：{0}", GetRealAngle(downpoint.X, downpoint.Y) * 180 / Math.PI));
                LLRBTree.Point point_oncloud = tree.floor(GetRealAngle(downpoint.X, downpoint.Y));
                Bitmap bmp_copy = (Bitmap)bmp.Clone();
                BitmapSource bmpSource;
                if (point_oncloud == null)
                {
                    bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Section_Image_DataTab.Source = bmpSource;
                    return;
                }
                Section_Detail_List_DataTab.Items.Add(String.Format("距离：{0}mm", point_oncloud.s));              
                Graphics g_bmp = Graphics.FromImage(bmp_copy);
                g_bmp.DrawLine(LinePen, new PointF(section_view_cx_datatab, section_view_cy_datatab), new PointF(point_oncloud.x, point_oncloud.y));
                bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Section_Image_DataTab.Source = bmpSource;
            }
            
        }

        public void DataTab_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point downpoint = e.GetPosition(Section_Image_DataTab);
            if (datatab_mode_index == 2)
            {
                if (rect_step == 1)
                {
                    if (downpoint.X <= 0 || downpoint.Y <= 0
                       || downpoint.X >= section_view_width_datatab || downpoint.Y >= section_view_height_datatab)
                        return;
                    if (Section_Detail_List_DataTab.Items.Count < 2)
                    {
                        Section_Detail_List_DataTab.Items.Add(String.Format("点 2 位置：{0}mm，{1}mm",
                                ((downpoint.X - section_view_cx_datatab) * zoom_rate_datatab).ToString("#.00"), ((section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab).ToString("#.00")));
                    }
                    else
                    {
                        Section_Detail_List_DataTab.Items[1] = (String.Format("点 2 位置：{0}mm，{1}mm",
                                ((downpoint.X - section_view_cx_datatab) * zoom_rate_datatab).ToString("#.00"), ((section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab).ToString("#.00")));
                    }
                    Bitmap bmp_copy = (Bitmap)bmp.Clone();
                    BitmapSource bmpSource;
                    Graphics g_bmp = Graphics.FromImage(bmp_copy);
                    g_bmp.DrawRectangle(RectPen, GetRect(downpoint, rect_downpoint));
                    bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Section_Image_DataTab.Source = bmpSource;
                }              
            }
            else if (datatab_mode_index == 3)
            {
                if (tree.root == null)
                    return;
                if (ruler_step == 1)
                {
                    double real_x1 = (ruler_downpoint.X - section_view_cx_datatab) * zoom_rate_datatab;
                    double real_y1 = (section_view_cy_datatab - ruler_downpoint.Y) * zoom_rate_datatab;
                    double real_x2 = (downpoint.X - section_view_cx_datatab) * zoom_rate_datatab;
                    double real_y2 = (section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab;
                    double dist = Math.Sqrt(Math.Pow((real_x1 - real_x2), 2) + Math.Pow((real_y1 - real_y2), 2));
                    if (Section_Detail_List_DataTab.Items.Count < 2)
                    {
                        Section_Detail_List_DataTab.Items.Add(String.Format("点 2 位置：{0}mm，{1}mm",
                        (real_x2).ToString("#.00"), (real_y2).ToString("#.00")));
                    }
                    else
                    {
                        Section_Detail_List_DataTab.Items[1] = (String.Format("点 2 位置：{0}mm，{1}mm",
                        (real_x2).ToString("#.00"), (real_y2).ToString("#.00")));
                    }
                    if (Section_Detail_List_DataTab.Items.Count < 3)
                    {
                        Section_Detail_List_DataTab.Items.Add(String.Format("距离：{0}mm", dist.ToString("#.00")));
                    }
                    else
                    {
                        Section_Detail_List_DataTab.Items[2] = (String.Format("距离：{0}mm", dist.ToString("#.00")));
                    }
                    Bitmap bmp_copy = (Bitmap)bmp.Clone();
                    Graphics g_bmp = Graphics.FromImage(bmp_copy);
                    g_bmp.DrawEllipse(LinePen, (int)ruler_downpoint.X, (int)ruler_downpoint.Y, ruler_point_size, ruler_point_size);
                    g_bmp.DrawLine(LinePen, new PointF((float)ruler_downpoint.X, (float)ruler_downpoint.Y), new PointF((float)downpoint.X, (float)downpoint.Y));
                    g_bmp.DrawEllipse(LinePen, (int)downpoint.X, (int)downpoint.Y, ruler_point_size, ruler_point_size);
                    BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Section_Image_DataTab.Source = bmpSource;
                }              
            }
        }

        private void End_Distance_Text_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        public void DataTab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point downpoint = e.GetPosition(Section_Image_DataTab);
            if (downpoint.X <= 0 || downpoint.Y <= 0 || downpoint.X >= section_view_width_datatab || downpoint.Y >= section_view_height_datatab)
                return;
            if (datatab_mode_index == 2)
            {
                if (rect_step == 0)
                {
                    if (tree.root == null)
                        return;
                    Section_Detail_List_DataTab.Items.Clear();
                    Section_Detail_List_DataTab.Items.Add(String.Format("点 1 位置：{0}mm，{1}mm",
                        ((downpoint.X - section_view_cx_datatab) * zoom_rate_datatab).ToString("#.00"), ((section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab).ToString("#.00")));                   
                    rect_downpoint = downpoint;
                    rect_step = 1;
                    BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Section_Image_DataTab.Source = bmpSource;
                }
                else if (rect_step == 1)
                {
                    if (tree.root == null)
                        return;                   
                    Section_Detail_List_DataTab.Items[1] = (String.Format("点 2 位置：{0}mm，{1}mm",
                        ((downpoint.X - section_view_cx_datatab) * zoom_rate_datatab).ToString("#.00"), ((section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab).ToString("#.00")));                   
                    rect_step = 0;
                    Bitmap bmp_copy = (Bitmap)bmp.Clone();
                    BitmapSource bmpSource;
                    if(isOPointIncluded(downpoint, rect_downpoint))
                    {
                        bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        Section_Image_DataTab.Source = bmpSource;
                        return;
                    }
                    float[] angle_range = GetRealAngleRange(downpoint, rect_downpoint);
                    if (angle_range == null)
                    {
                        bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        Section_Image_DataTab.Source = bmpSource;
                        return;
                    }
                    List<LLRBTree.Point> points = new List<LLRBTree.Point>();
                    if (angle_range.Length == 2)
                    {
                        points = tree.between(angle_range[0], angle_range[1]);
                    }
                    else if (angle_range.Length == 4)
                    {
                        points = tree.between(angle_range[0], angle_range[1]);
                        List<LLRBTree.Point> points_add = tree.between(angle_range[2], angle_range[3]);
                        if (points_add != null)
                            points = points.Concat(points_add).ToList<LLRBTree.Point>();
                    }
                    float[] weight_center = GetWeightCenter(points, GetRect(downpoint, rect_downpoint));
                    if (weight_center == null || weight_center[0] <= 0 || weight_center[0] >= section_view_width_datatab
                        || weight_center[1] <= 0 || weight_center[1] >= section_view_height_datatab)
                    {
                        bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        Section_Image_DataTab.Source = bmpSource;
                        return;
                    }
                    float real_x = (weight_center[0] - section_view_cx_datatab) * zoom_rate_datatab;
                    float real_y = (section_view_cy_datatab - weight_center[1]) * zoom_rate_datatab;
                    float dist = (float)Math.Sqrt(real_x * real_x + real_y * real_y);
                    Section_Detail_List_DataTab.Items.Add(String.Format("重心坐标: {0}mm, {1}mm", (real_x).ToString("#.00"),
                        (real_y).ToString("#.00")));
                    Section_Detail_List_DataTab.Items.Add(String.Format("中心距: {0}mm", dist.ToString("#.00")));
                    Graphics g_bmp = Graphics.FromImage(bmp_copy);
                    g_bmp.DrawRectangle(RectPen, GetRect(downpoint, rect_downpoint));
                    g_bmp.DrawEllipse(LinePen, (int)weight_center[0], (int)weight_center[1], rect_point_size, rect_point_size);
                    g_bmp.DrawLine(LinePen, new PointF(weight_center[0], weight_center[1]), new PointF(section_view_cx_datatab, section_view_cy_datatab));
                    bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Section_Image_DataTab.Source = bmpSource; 
                }                
            }
            else if (datatab_mode_index == 3)
            {
                if (ruler_step == 0)
                {
                    if (tree.root == null)
                        return;
                    Section_Detail_List_DataTab.Items.Clear();
                    Section_Detail_List_DataTab.Items.Add(String.Format("点 1 位置：{0}mm，{1}mm",
                        ((downpoint.X - section_view_cx_datatab) * zoom_rate_datatab).ToString("#.00"), ((section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab).ToString("#.00")));
                    ruler_downpoint = downpoint;
                    ruler_step = 1;
                    Bitmap bmp_copy = (Bitmap)bmp.Clone();
                    Graphics g_bmp = Graphics.FromImage(bmp_copy);
                    g_bmp.DrawEllipse(LinePen, (int)downpoint.X, (int)downpoint.Y, ruler_point_size, ruler_point_size);
                    BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Section_Image_DataTab.Source = bmpSource;
                }
                else if (ruler_step == 1)
                {
                    if (tree.root == null)
                        return;
                    double real_x1 = (ruler_downpoint.X - section_view_cx_datatab) * zoom_rate_datatab;
                    double real_y1 = (section_view_cy_datatab - ruler_downpoint.Y) * zoom_rate_datatab;
                    double real_x2 = (downpoint.X - section_view_cx_datatab) * zoom_rate_datatab;
                    double real_y2 = (section_view_cy_datatab - downpoint.Y) * zoom_rate_datatab;
                    double dist = Math.Sqrt(Math.Pow((real_x1 - real_x2), 2) + Math.Pow((real_y1 - real_y2), 2));
                    if (Section_Detail_List_DataTab.Items.Count < 2)
                    {
                        Section_Detail_List_DataTab.Items.Add(String.Format("点 2 位置：{0}mm，{1}mm",
                        (real_x2).ToString("#.00"), (real_y2).ToString("#.00")));
                    }
                    else
                    {
                        Section_Detail_List_DataTab.Items[1] = (String.Format("点 2 位置：{0}mm，{1}mm",
                        (real_x2).ToString("#.00"), (real_y2).ToString("#.00")));
                    }
                    if (Section_Detail_List_DataTab.Items.Count < 3)
                    {
                        Section_Detail_List_DataTab.Items.Add(String.Format("距离：{0}mm", dist.ToString("#.00")));
                    }
                    else
                    {
                        Section_Detail_List_DataTab.Items[2] = (String.Format("距离：{0}mm", dist.ToString("#.00")));
                    }
                    ruler_step = 0;
                    Bitmap bmp_copy = (Bitmap)bmp.Clone();
                    Graphics g_bmp = Graphics.FromImage(bmp_copy);
                    g_bmp.DrawEllipse(LinePen, (int)ruler_downpoint.X, (int)ruler_downpoint.Y, ruler_point_size, ruler_point_size);
                    g_bmp.DrawLine(LinePen, new PointF((float)ruler_downpoint.X, (float)ruler_downpoint.Y), new PointF((float)downpoint.X, (float)downpoint.Y));
                    g_bmp.DrawEllipse(LinePen, (int)downpoint.X, (int)downpoint.Y, ruler_point_size, ruler_point_size);
                    BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmp_copy.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Section_Image_DataTab.Source = bmpSource;
                }
            }
        }

        private float[] GetWeightCenter(List<LLRBTree.Point> points, Rectangle rect)
        {
            if (points == null || points.Count < 1)
                return null;
            float sum_x = 0, sum_y = 0, count = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].x > rect.X && points[i].x < (rect.X + rect.Width) 
                    && points[i].y > rect.Y && points[i].y < (rect.Y + rect.Height))
                {
                    sum_x += points[i].x;
                    sum_y += points[i].y;
                    count++;
                }
            }
            if (count < 1)
                return null;
            float[] weight_center = new float[2];
            weight_center[0] = sum_x / count;
            weight_center[1] = sum_y / count;
            return weight_center;
        }

        private Rectangle GetRect(System.Windows.Point pt1, System.Windows.Point pt2)
        {
            int x = (int)Math.Min(pt1.X, pt2.X);
            int y = (int)Math.Min(pt1.Y, pt2.Y);
            int width = (int)Math.Abs(pt1.X - pt2.X);
            int height = (int)Math.Abs(pt1.Y - pt2.Y);
            return new Rectangle(x, y, width, height);
        }

        private bool isOPointIncluded(System.Windows.Point pt1, System.Windows.Point pt2)
        {
            if (pt1.X * pt2.X <= 0 && pt1.Y * pt2.Y <= 0)
                return true;
            else
                return false;
        }

        private float[] GetRealAngleRange(System.Windows.Point pt1, System.Windows.Point pt2)
        {
            double x1 = pt1.X;
            double x2 = pt2.X;
            double y1 = pt1.Y;
            double y2 = pt2.Y;
            double[] a = new double[4];
            a[0] = GetRealAngle(x1, y1);
            a[1] = GetRealAngle(x1, y2);
            a[2] = GetRealAngle(x2, y1);
            a[3] = GetRealAngle(x2, y2);
            double min_a = double.MaxValue;
            double max_a = double.MinValue;
            for (int i = 0; i < 4; i++)
            {
                min_a = Math.Min(min_a, a[i]);
                max_a = Math.Max(max_a, a[i]);
            }
            if (max_a <= min_a)
                return null;
            if (max_a - min_a < Math.PI / 4)
            {
                float[] ret = new float[2];
                ret[0] = (float)min_a;
                ret[1] = (float)max_a;
                return ret;
            }
            else if ((min_a - max_a) % (2 * Math.PI) < Math.PI / 4)
            {
                float[] ret = new float[4];
                ret[0] = 0;
                ret[1] = (float)min_a;
                ret[2] = (float)max_a;
                ret[3] = (float)(2 * Math.PI);
                return ret;
            }
            return null;
        }
    }

    public class ImageUrlInput
    {
        public String[] camn_image { set; get; }
        public String camVO_image { set; get; }

        public const int StringArrLength = 8;

        public ImageUrlInput(String[] _camn_image, String _camVO_image)
        {
            if (_camn_image.Length != StringArrLength)
                return;
            camn_image = new string[StringArrLength];
            for (int i = 0; i < StringArrLength; i++)
            {
                camn_image[i] = _camn_image[i];
            }
            camVO_image = _camVO_image;
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

    class DetectRecordSelect
    {
        public LineInfo line;
        public DetectDeviceInfo device;
        public Single Detect_Distance;
        public Single Start_Loc;
        public Single Stop_Loc;

        public DetectRecordSelect(LineInfo _line, DetectDeviceInfo _device, Single _Detect_Distance, Single _Start_Loc, Single _Stop_Loc)
        {
            line = _line;
            device = _device;
            Detect_Distance = _Detect_Distance;
            Start_Loc = _Start_Loc;
            Stop_Loc = _Stop_Loc;
        }
    }

    // Multi-list view
    class LineListItem
    {
        public String LineNum { set; get; }
        public String LineName { set; get; }
        public String TotalMileage { set; get; }
        public String CreateTime { set; get; }
        public LineListItem(String _LineNum, String _LineName, float _TotalMileage, DateTime _CreateTime)
        {
            LineNum = _LineNum;
            LineName = _LineName;
            TotalMileage = _TotalMileage.ToString();
            CreateTime = _CreateTime.ToString();
        }
    }

    class RecordListItem
    {
        public String RecordNum { set; get; }
        public String LineName { set; get; }
        public String CreateTime { set; get; }
        public String DetectDistance { set; get; }
        public String StartLoc { set; get; }
        public String StopLoc { set; get; }
        public String DeviceName { set; get; }
        public RecordListItem(String _RecordNum, String _LineName, DateTime _CreateTime,
            float _DetectDistance, float _StartLoc, float _StopLoc, String _DeviceName)
        {
            RecordNum = _RecordNum;
            LineName = _LineName;
            CreateTime = _CreateTime.ToString();
            DetectDistance = _DetectDistance.ToString();
            StartLoc = _StartLoc.ToString();
            StopLoc = _StopLoc.ToString();
            DeviceName = _DeviceName;
        }
    }

    public class DataListItem
    {
        public String DataLoc { set; get; }
        public String LongAxis { set; get; }
        public String ShortAxis { set; get; }
        public String HorizontalAxis { set; get; }
        public String Rotation { set; get; }
        public String Constriction { set; get; }
        public String Crack { set; get; }
        public DataListItem(double _DataLoc, float _LongAxis, float _ShortAxis, float _HorizontalAxis, float _Rotation, bool _Constriction, bool _Crack)
        {
            DataLoc = _DataLoc.ToString();
            LongAxis = _LongAxis.ToString("0.#");
            ShortAxis = _ShortAxis.ToString("0.#");
            HorizontalAxis = _HorizontalAxis.ToString("0.#");
            Rotation = _Rotation.ToString("0.##");
            Constriction = _Constriction.ToString();
            Crack = _Crack.ToString();
        }

        public DataListItem(String text)
        {
            DataLoc = text;
            LongAxis = text;
        }
    }

    
}
