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
    /// ConfirmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public delegate void TrueFalseDelegate(bool value);

        public event TrueFalseDelegate true_false_event;
        public ConfirmDialog(List<String> info_list)
        {
            InitializeComponent();
            if(info_list.Count < 1)
            {
                ConfirmList.Items.Add("确认信息有误，请返回上一级");
                ConfirmButton.IsEnabled = false;
            }
            else
            {
                for (int i = 0; i < info_list.Count; i++)
                {
                    ConfirmList.Items.Add(info_list[i]);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            true_false_event?.Invoke(false);
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            true_false_event?.Invoke(true);
            Close();
        }
    }
}
