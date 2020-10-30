﻿using FileIO;
using libMetroTunnelDB;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

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

        public static bool MySQL_lock = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += Window_Closing;

            // Initialize
            DataAnalyze.DataAnalyzeInit(Database, this);
            ConfigHandler.ConfigInit();
            
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
            if(!MySQL_is_valid())
            {
                if (MessageBox.Show("任务正在进行，关闭窗口将导致数据丢失", "确认", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    e.Cancel = false;
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
            Wait_MySQL();
            List<libMetroTunnelDB.Line> line = new List<libMetroTunnelDB.Line>();
            Database.QueryLine(ref line);
            Dispatcher.Invoke(new Action(() => { LineInfoList.Items.Clear(); }));
            for(int i = 0; i < line.Count; i++)
            {
                Dispatcher.Invoke(new Action(() => { LineInfoList.Items.Add(
                    new LineInfo(line[i].LineNumber, line[i].LineName, line[i].TotalMileage, line[i].CreateTime)); }));
            }
            Release_MySQL();
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
            Wait_MySQL();
            List<DetectDevice> detectDevices = new List<DetectDevice>();
            Database.QueryDetectDevice(ref detectDevices);
            Dispatcher.Invoke(new Action(() => { DetectDeviceInfoList.Items.Clear(); }));
            for(int i = 0; i < detectDevices.Count(); i++)
            {
                Dispatcher.Invoke(new Action(() => { DetectDeviceInfoList.Items.Add(
                    new DetectDeviceInfo(detectDevices[i].DetectDeviceNumber, 
                    detectDevices[i].DetectDeviceName, detectDevices[i].CreateTime)); }));
            }
            Release_MySQL();
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
            Dispatcher.Invoke(new Action(() => { SubPbarText.Text = percentage.ToString("0.#"); }));
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
                        MainProcessBarSet(dict_counter * process_per_record + i * process_per_record / dataRecord.DataDiskDirList.Count);
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
                    MainProcessBarSet(dict_counter * process_per_record);
                }
            }
            
            MainProcessBarSet(100);
            DebugWriteLine("数据盘扫描完成");
        }

        private static DetectRecordSelect detectRecordSelect;

        internal static DetectRecordSelect DetectRecordSelect { get => detectRecordSelect; set => detectRecordSelect = value; }

        private static List<bool> selected_data_record_valid = new List<bool>();
        
        private static List<DataRecord> selected_data_record = new List<DataRecord>();

        private void Analyze_All_Button_Click(object sender, RoutedEventArgs e)
        {
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
            Wait_MySQL();
            
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
                    line_sum = CSVHandler.GetLineCount(csvpath, this);                   
                    DebugReWriteLine("扫描数据文件" + "完成");

                    MainProcessReport("分析数据", (int)(line_sum * 8.1), i + 1, stage_counter);
                    DataAnalyze.ScanCalResult(CalResFolder, record_id_max);
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

                    MainProcessReport("解析视频" + dataRecord.DataDiskDirList[j], (int)(enc_line_sum * 8.1) + jpeg_size_est, i + 1, stage_counter);
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
            DebugWriteLine("全部导入成功");
            Release_MySQL();
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
            Wait_MySQL();
            List<libMetroTunnelDB.Line> line = new List<libMetroTunnelDB.Line>();
            Dispatcher.Invoke(new Action(() => { Line_List.Items.Clear(); }));
            try
            {
                Database.QueryLine(ref line);  
            }
            catch(SystemException)
            {
                Release_MySQL();
                return;
            }
            for (int i = 0; i < line.Count; i++)
            {
                Dispatcher.Invoke(new Action(() => { Line_List.Items.Add(
                    new LineListItem(line[i].LineNumber, line[i].LineName, line[i].TotalMileage, line[i].CreateTime)); }));
            }
            Release_MySQL();
        }

        public void GetRecordList()
        {
            Thread get_detect_record_thread = new Thread(GetRecordList_t);
            get_detect_record_thread.Start();
        }
        private void GetRecordList_t()
        {
            Wait_MySQL();
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
                Release_MySQL();
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
            Release_MySQL();
        }
        public void GetDataList()
        {
            Thread get_data_list_thread = new Thread(GetDataList_t);
            get_data_list_thread.Start();
        }

        private void GetDataList_t()
        {
            Wait_MySQL();
            List<libMetroTunnelDB.DataConv> datas = new List<DataConv>();
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
                QueryFrom += maxQuery;
            }
            catch(SystemException)
            {
                Release_MySQL();
                return;
            }                       
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
            Release_MySQL();
        }

        public void GetMoreData()
        {
            Thread get_more_data_thread = new Thread(GetMoreData_t);
            get_more_data_thread.Start();
        }

        private void GetMoreData_t()
        {
            Wait_MySQL();
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
                Release_MySQL();
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
            Release_MySQL();
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
                Wait_MySQL();
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
                Release_MySQL();
            }
            catch(System.Exception)
            {
                Release_MySQL();
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
            selected_data = Data_List.SelectedItem as DataListItem;
            if (selected_data.DataLoc == "...")
            {
                GetMoreData();
            }
            else
            {
                RefreshSelectedConditionText();
                // visualize info
            }
            
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

    class DataListItem
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
