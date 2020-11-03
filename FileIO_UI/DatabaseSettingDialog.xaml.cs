using libMetroTunnelDB;
using System;
using System.Collections.Generic;
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
    /// DatabaseSettingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DatabaseSettingDialog : Window
    {
        private String DatabaseAddress;
        private String DatabaseIP;
        private String DatabasePort;
        private String DatabaseUser;
        private String DatabaseKey;

        public MetroTunnelDB Database;

        public delegate void Dlg_Closed_Event(String address, String user);

        public event Dlg_Closed_Event dlg_closed_event;

        public DatabaseSettingDialog(ref MetroTunnelDB _Database)
        {
            InitializeComponent();
            Database = _Database;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatabaseAddress = DatabaseAddressText.Text;
                DatabaseUser = DatabaseUserText.Text;
                DatabaseKey = DatabaseKeyText.Password;
            }
            catch(System.Exception)
            {
                WarningLabel.Content = "输入内容无效，请重新输入";
                return;
            }
            // Address processing
            String[] Address_list = DatabaseAddress.Split(":");
            if (Address_list.Length == 2)
            {
                DatabaseIP = Address_list[0];
                DatabasePort = Address_list[1];
            }
            else
            {
                WarningLabel.Content = "输入地址无效，请按如127.0.0.1:3306的格式输入";
                return;
            }
            
            List<String> confirm_list = new List<string>();
            confirm_list.Add("请确认以下输入信息无误： ");
            confirm_list.Add("数据库地址：" + DatabaseIP);
            confirm_list.Add("用户名：" + DatabasePort);
            ConfirmDialog confirmDialog = new ConfirmDialog(confirm_list);
            confirmDialog.true_false_event += new ConfirmDialog.TrueFalseDelegate(ConfirmProcess);
            confirmDialog.ShowDialog();
        }
        private void ConfirmProcess(bool value)
        {
            if (value)
            {
                try
                {
                    Database = new MetroTunnelDB(DatabaseIP, Convert.ToInt32(DatabasePort), DatabaseUser, DatabaseKey);
                    MessageBox.Show("数据库配置修改成功！", "确认", MessageBoxButton.OK);
                    dlg_closed_event?.Invoke(DatabaseAddress, DatabaseUser);
                }
                catch(System.Exception)
                {
                    MessageBox.Show("数据库配置失败，请检查参数！", "确认", MessageBoxButton.OK);
                }
            }
            Close();
        }
    }
}
