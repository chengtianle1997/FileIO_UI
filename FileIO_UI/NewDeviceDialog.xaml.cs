using libMetroTunnelDB;
using System;
using System.Collections.Generic;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileIO_UI
{
    /// <summary>
    /// NewDeviceDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NewDeviceDialog : Window
    {
        public libMetroTunnelDB.DetectDevice detectDevice;
        public String DetectDeviceNum;
        public String DetectDeviceNum_ori;
        public String DetectDeviceName;
        public DateTime CreateTime;
        public MetroTunnelDB Database;

        public bool isEdit = false;

        public delegate void Dlg_Closed_Event();

        public event Dlg_Closed_Event dlg_closed_event;

        public NewDeviceDialog(MetroTunnelDB _Database, libMetroTunnelDB.DetectDevice _detectDevice)
        {
            InitializeComponent();
            Database = _Database;
            if(_detectDevice != null)
            {
                DetectDeviceNum = _detectDevice.DetectDeviceNumber;
                DetectDeviceNum_ori = _detectDevice.DetectDeviceNumber;
                DetectDeviceName = _detectDevice.DetectDeviceName;
                // Display Param
                DetectDeviceNumText.Text = DetectDeviceNum;
                DetectDeviceNameText.Text = DetectDeviceName;
                isEdit = true;
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DetectDeviceNum = DetectDeviceNumText.Text.ToString();
                DetectDeviceName = DetectDeviceNameText.Text.ToString();
                CreateTime = DateTime.Now;
            }
            catch(System.FormatException)
            {
                WarningLabel.Content = "输入内容无效，请重新输入";
                return;
            }
            if(!isEdit)
            {
                // Check DetectDeviceNum is New
                List<libMetroTunnelDB.DetectDevice> detectDevices = new List<DetectDevice>();
                Database.QueryDetectDevice(ref detectDevices, DetectDeviceNum);
                if (detectDevices.Count > 0)
                {
                    WarningLabel.Content = "设备已存在，请直接选择该设备";
                    return;
                }
            }
            List<String> confirm_list = new List<String>();
            if(!isEdit)
            {
                confirm_list.Add("请确认以下输入信息无误：");
            }
            else
            {
                confirm_list.Add("请确认以下修改信息无误：");
            }
            confirm_list.Add("线路编号： " + DetectDeviceNum);
            confirm_list.Add("线路名称： " + DetectDeviceName);
            confirm_list.Add("记录创建时间：" + CreateTime.ToString());
            ConfirmDialog confirmDialog = new ConfirmDialog(confirm_list);
            confirmDialog.true_false_event += new ConfirmDialog.TrueFalseDelegate(Confirm_Process);
            confirmDialog.ShowDialog();
        }
        private void Confirm_Process(bool value)
        {
            if (value)
            {
                if (!isEdit)
                {
                    Database.InsertIntoDetectDevice(new DetectDevice(DetectDeviceNum, DetectDeviceName, 0, CreateTime));
                    // Refresh LineInfo
                    dlg_closed_event?.Invoke();
                }
                else
                {
                    Database.DeleteDetectDevice(DetectDeviceNum_ori);
                    Database.InsertIntoDetectDevice(new DetectDevice(DetectDeviceNum, DetectDeviceName, 0, CreateTime));
                    // Refresh LineInfo
                    dlg_closed_event?.Invoke();
                }

            }
            Close();
        }
    }
}
