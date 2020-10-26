using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using libMetroTunnelDB;

namespace FileIO_UI
{
    /// <summary>
    /// NewLineDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NewLineDialog : Window
    {
        //public libMetroTunnelDB.Line line;
        public String LineNum_ori;
        public String LineNum;
        public String LineName;
        public Single TotalMileage;
        public DateTime CreateTime;
        public MetroTunnelDB Database;

        public bool isEdit = false;

        public delegate void Dlg_Closed_Event();

        public event Dlg_Closed_Event dlg_closed_event;
        public NewLineDialog(MetroTunnelDB _Database, libMetroTunnelDB.Line _line)
        {
            InitializeComponent();
            Database = _Database;
            if(_line != null)
            {
                LineNum = _line.LineNumber;
                LineNum_ori = _line.LineNumber;
                LineName = _line.LineName;
                TotalMileage = _line.TotalMileage;
                // Display Patam
                LineNumText.Text = LineNum;
                LineNameText.Text = LineName;
                TotalMileageText.Text = TotalMileage.ToString();
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
                // Get Text
                LineNum = LineNumText.Text.ToString();
                LineName = LineNameText.Text.ToString();
                TotalMileage = Convert.ToSingle(TotalMileageText.Text);
                CreateTime = DateTime.Now;
            }
            catch(System.FormatException)
            {
                WarningLabel.Content = "输入内容无效, 请重新输入";
                return;
            }
            if(!isEdit)
            {
                // Check LineNum is New
                List<libMetroTunnelDB.Line> line_list = new List<libMetroTunnelDB.Line>();
                Database.QueryLine(ref line_list, LineNum);
                if (line_list.Count > 0)
                {
                    WarningLabel.Content = "线路已存在，请直接选择该线路";
                    return;
                }
            }
            // Confirm Dialog
            List<String> confirm_list = new List<String>();
            if(!isEdit)
            {
                confirm_list.Add("请确认以下输入信息无误：");
            }
            else
            {
                confirm_list.Add("请确认以下修改信息无误：");
            }           
            confirm_list.Add("线路编号： " + LineNum);
            confirm_list.Add("线路名称： " + LineName);
            confirm_list.Add("总里程： " + TotalMileage + "公里");
            confirm_list.Add("记录创建时间：" + CreateTime.ToString());
            ConfirmDialog confirmDialog = new ConfirmDialog(confirm_list);
            confirmDialog.true_false_event += new ConfirmDialog.TrueFalseDelegate(Confirm_Process);
            confirmDialog.ShowDialog();
        }

        private void Confirm_Process(bool value)
        {
            if(value)
            {
                if(!isEdit)
                {
                    Database.InsertIntoLine(new libMetroTunnelDB.Line(LineNum, LineName, TotalMileage, CreateTime));
                    // Refresh LineInfo
                    dlg_closed_event?.Invoke();
                }
                else
                {
                    Database.DeleteLine(LineNum_ori);
                    Database.InsertIntoLine(new libMetroTunnelDB.Line(LineNum, LineName, TotalMileage, CreateTime));
                    // Refresh LineInfo
                    dlg_closed_event?.Invoke();
                }
                
            }
            Close();
        }
    }
}
