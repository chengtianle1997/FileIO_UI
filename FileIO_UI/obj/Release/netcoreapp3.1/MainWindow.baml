   M S B A M L   `   `   `  ���� D  @FileIO_UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:FileIO_UI	FileIO_UI      FileIO_UI.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     )localclr-namespace:FileIO_UI       � Title$+  %沃翌智能-隧检数图处理软件��$	��864��$
��1450�� � 	Resources    ListViewItemStyle �� 
TargetType �� Property �� Value  LineListItemStyle  RecordListItemStyle  DataListItemStyle��%�   &	        &	 '     &	 S     &	      �� " ��.���  � ��$ Center=��� " ��.���  � ��$ Center=�-   �� " ��.���  � ��$ Center=�-   �� " ��.���  � ��$ Center=�-   .��(�  (� Stretch$
 Fill=�.��]� $
��1450��$	��850��.+�� .+��� .@��� $��数图处理��.��� ���	#FFE5E5E5$	��823��$��0,0,0,0q� � RowDefinitions �� $	��59* ��� $
��764* � � ColumnDefinitions �� $	��93* ��� $	��19* ��� $	��28* ��� $	��33* ��� $��4* ��� $��1353* �.+��� -   $��Data_Disk_Choose_Button��$��添加��$��Right=�$��0,311,1078,0q�$	��Top=�$��1��$��5��$��39���� $��Center=�$��	0,306,0,0q�$��	数据盘��$
��Wrap=�$	��Top=�$��1���� -   $��Data_Disk_Choose_Text��$
��Left=�$��
28,325,0,0q�$�� �� �� TextWrapping$
 Wrap=�$	��Top=�$	��289��$��1��$��6��$��18���� -   $��Backup_Disk_Choose_Button��$��浏览��$
��Left=�$��	159,0,0,0q�$��Center=�$��1��$��5��$��84���� $��Center=�$��	0,352,0,0q�$��	备份盘��$
��Wrap=�$	��Top=�$��1���� -   $��Backup_Disk_Choose_Text��$
��Left=�$��28,0,0,0q�$�� ��$
 Wrap=�$��Center=�$	��289��$��1��$��6��$��20���� $
��Left=�$��
24,207,0,0q�$
��Wrap=�$	��Top=�$��1���� $�导入���� 	 � Language$	 zh-cn�$�参数���� $
��Left=�$��	25,43,0,0q�$
��Wrap=�$	��Top=��� $�线路���� $	 zh-cn�$�选择���� -   $��LineInfoList��$��25,3,0,0q�$��6��$
��Left=�$	��375��$	��118��$	��Top=�$��1��$��Disabled=�$��Visible=�$��Center=�#��[  
 �� View
 �� .P��� $��编号�� �� Width$ 30�� �� DisplayMemberBinding �� *	LineNum+�� $��线路名称��$	 100�� �� *
LineName+�� $��	总里程��$ 70�� �� *TotalMileage+�� $��记录创建时间��$	 150�� �� *
CreateTime+ � ContextMenu �� .@�m� -	   $��编辑�� m� Icon �� $��/edit_icon.png��$��12��$��12��m� $��删除��$y�	Collapsed=� �� $��/delete_icon.png��$��12��$��12��.@��� $	��100��$	��100��
 �� .P��� �� -
   $��新建线路��$
��Left=�$��
116,35,0,0q�$	��Top=�$��5���� -   $��刷新线路��$
��Left=�$��
176,35,0,0q�$	��Top=�$��5���� $
��Left=�$��
26,130,0,0q�$
��Wrap=�$	��Top=�$��1��$��2���� $	 zh-cn�$�检测设备���� $�选择���� -   $��DetectDeviceInfoList��$��
26,150,0,0q�$��52��$	��Top=�$
��Left=�$	��375��$��1��$��6��$��Disabled=�$��Visible=�$��Center=�#��[  
 �� .P��� $��编号��$ 30�� �� *DetectDeviceNumber+�� $��设备名称��$	 150�� �� *DetectDeviceName+�� $��记录创建时间��$	 170�� �� *
CreateTime+ �� .@�m� -   $��编辑�� �� $��/edit_icon.png��$��12��$��12��m� $��删除��$y�	Collapsed=� �� $��/delete_icon.png��$��12��$��12���� -   $��Add_New_Device_Button��$��新建设备��$
��Left=�$��124,126,0,0q�$	��Top=�$��1��$��5���� -   $��Refresh_Device_Button��$��刷新设备��$
��Left=�$��181,126,0,0q�$	��Top=�$��1��$��5���� -   $��Data_Record_List��$��
27,423,0,0q�$	��143��$	��Top=�$
��Left=�$	��375��$��1��$��6��$
��Auto=�$��Visible=�$��Center=�#��[  
 �� .P��� $��编号��$ 30�� �� *	RecordNum+�� $��数据创建时间��$	 150�� �� *
CreateTime+�� $��数据大小��$ 70�� �� *
DataSize+�� $��数据盘路径��$	 160�� �� *DataDiskDir+�� $
��Left=�$��
25,403,0,0q�$
��Wrap=�$	��Top=�$��1���� $�数据���� $	 zh-cn�$	�盘���� $�记录���� $
��Left=�$��
24,230,0,0q�$
��Wrap=�$	��Top=�$��1���� $	 zh-cn�$�检测长度���� -   $��Detect_Distance_Text��$
��Left=�$��
86,229,0,0q�$�� ��$
 Wrap=�$	��Top=�$	��107��$��1��$��6���� $
��Left=�$��
25,256,0,0q�$
��Wrap=�$	��Top=�$��1���� $	�起���� $	 zh-cn�$	�点���� $�里程���� $
��Left=�$��
24,280,0,0q�$
��Wrap=�$	��Top=�$��1���� $	 zh-cn�$�终点里程���� -   $��Start_Mileage_Text��$
��Left=�$��
86,255,0,0q�$�� ��$
 Wrap=�$	��Top=�$	��107��$��1��$��6���� -   $��End_Mileage_Text��$
��Left=�$��
86,280,0,0q�$�� ��$
 Wrap=�$	��Top=�$	��107��$��1��$��6���� -   $��Reset_Disk_Record_Button��$��重置��$
��Left=�$��
28,571,0,0q�$	��Top=�$��1��$	��165��$��25��$��6���� $
��Left=�$��
58,233,0,0q�$
��Wrap=�$	��Top=�$��1��$��5���� $	 zh-cn�$�数据库配置���� -   $!��Change_MySQL_Setting_Button��$��修改��$
��Left=�$��162,231,0,0q�$	��Top=�$��1��$��38��$��5���� $
��Left=�$��
58,253,0,0q�$
��Wrap=�$	��Top=�$��1��$	��186��$��5���� $	 zh-cn�$� ���� -    �Name$ MySQL_Address_Text��$�localhost:3306���� $�, user: ���� -   $ MySQL_User_Text��$
�root���� -   $��Analyze_All_Button��$��全部导入��$
��Left=�$��
31,571,0,0q�$	��Top=�$��1��$	��203��$��25��$��5���� $
��Left=�$��19,0,0,0q�$
��Wrap=�$��Center=�$��3���� $
��Bold"�$��14(�$�数据导入工作区���� $
��Left=�$��
295,24,0,0q�$
��Wrap=�$	��Top=�$��5���� $
��Bold"�$��14(�$�数据处理工作���� $
��Bold"�$��14(�$	 zh-cn�$	�区���� -   $��	Line_List��$��301,104,0,0q�$
��Left=�$	��375��$	��468��$	��Top=�$��1��$��5��$
��Auto=�$��Visible=�$��Center=�#��[ $y�Hidden=�
 �� .P��� $��编号��$ 40�� �� *	LineNum+�� $��线路名称��$	 120�� �� *
LineName+�� $��	总里程��$ 60�� �� *TotalMileage+�� $��创建时间��$	 140�� �� *
CreateTime+�� -   $��Record_List��$��301,104,0,0q�$
��Left=�$	��375��$	��468��$	��Top=�$��1��$��5��$
��Auto=�$��Visible=�$��Center=�#��[ $y�Visible=�
 �� .P��� $��编号��$ 40�� �� *	RecordNum+�� $��检测线路��$	 125�� �� *
LineName+�� $��检测时间��$	 130�� �� *
CreateTime+�� $��检测里程��$ 60�� �� *DetectDistance+�� $��起点里程��$ 60�� �� *
StartLoc+�� $��终点里程��$ 60�� �� *	StopLoc+�� $��检测设备��$	 140�� �� *
DeviceName+ �� .@�m� -   $��删除��$y�Visible=� �� $��/delete_icon.png��$��12��$��12���� -   $��	Data_List��$��301,104,0,0q�$
��Left=�$	��375��$	��468��$	��Top=�$��1��$��5��$
��Auto=�$��Visible=�$��Center=�#��[ $y�Hidden=�
 �� .P��� $��里程位置��$ 90�� �� *	DataLoc+�� $��截面长轴��$ 70�� �� *
LongAxis+�� $��截面短轴��$ 70�� �� *	ShortAxis+�� $��是否收敛��$ 60�� �� *Constriction+�� $��存在裂缝��$ 60�� �� *Crack+�� $��	水平轴��$ 70�� �� *HorizontalAxis+�� $��	滚转角��$ 60�� �� *
Rotation+�� $
��Left=�$��301,46,0,762q�$
��Wrap=�$��5��$��2���� $	 zh-cn�$�时间范围���� $
��Left=�$��441,12,0,737q�$
��Wrap=�$��5��$��1���� $	 zh-cn�$	�到��(  "System.Windows.Controls.DatePicker  -   $��Start_Date_Picker��$��5��$
��Left=�$��	301,9,0,0q�$��1��$	��Top=�$	��135��  -   $��End_Date_Picker��$��5��$
��Left=�$��	461,9,0,0q�$��1��$	��Top=�$	��135���� -   $��Search_Record_Button��$��查询记录��$
��Left=�$��
605,11,0,0q�$	��Top=�$��1��$��5��$��72���� -    $��	DebugList��$��301,608,0,0q�$	��109��$	��Top=�$
��Left=�$	��376��$��1��$��5���� $
��Left=�$��
24,603,0,0q�$
��Wrap=�$	��Top=�$��1���� $	 zh-cn�$�导入进度��� -!   $��MainPbar��$
��Left=�$��19��$��
28,645,0,0q�$��1��$	��Top=�$	��372��$��6��� -"   $��SubPbar��$
��Left=�$��18��$��
28,699,0,0q�$��1��$	��Top=�$	��372��$��6���� -#   $��MainPbarText��$
��Left=�$��	2,623,0,0q�$
��Wrap=�$	��Top=�$��1��$	��310��$��5��$��1���� -$   $ MainPercentage��$�0���� $�%���� -%   $ MainTimeRemain��$	 zh-cn�$� ���� $
��Left=�$��
28,623,0,0q�$
��Wrap=�$	��Top=�$��1��$	��141��$��4���� $	 zh-cn�$�	总进度���� -&   $��SubPBarInfo��$
��Left=�$��
28,678,0,0q�$
��Wrap=�$	��Top=�$��1��$	��315��$��6���� $	 zh-cn�$	�子���� $�进度���� $
��Left=�$��191,677,0,0q�$
��Wrap=�$	��Top=�$��1��$��53��$��5��$��17���� -'   $ SubPbarText��$�0���� $�%���� $
��Left=�$��301,588,0,0q�$
��Wrap=�$	��Top=�$��1��$��5���� $	 zh-cn�$�日志列表���� -(   $��
Cam1_Image��$��5��$
��Left=�$��96��$��
759,11,0,0q�$	��Top=�$	��121��$��1��$��/sample.jpg���� $
��Left=�$��
752,23,0,0q�$
��Wrap=�$	��Top=�$��5���� $
��Bold"�$��14(�$�数据���� $
��Bold"�$��14(�$	 zh-cn�$�可视化预览���� $
��Left=�$��759,48,0,760q�$
��Wrap=�$��5��$��2���� $	 zh-cn�$�Cam 1���� -)   $��
Cam2_Image��$��5��$
��Left=�$��96��$��
919,11,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��919,49,0,759q�$
��Wrap=�$��5��$��2���� $
�Cam ���� $	 zh-cn�$�2���� -*   $��
Cam3_Image��$��5��$
��Left=�$��96��$��1079,11,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��1079,49,0,759q�$
��Wrap=�$��5��$��2���� $
�Cam ���� $	 zh-cn�$�3���� -+   $��
Cam4_Image��$��5��$
��Left=�$��96��$��759,135,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��759,113,0,636q�$
��Wrap=�$��5��$��1���� $
�Cam ���� $	 zh-cn�$�4���� -,   $��
Cam5_Image��$��5��$
��Left=�$��96��$��919,135,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��919,115,0,634q�$
��Wrap=�$��5��$��1���� $
�Cam ���� $	 zh-cn�$�5���� --   $��
Cam6_Image��$��5��$
��Left=�$��96��$��1079,135,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��1079,115,0,634q�$
��Wrap=�$��5��$��1���� $
�Cam ���� $	 zh-cn�$�6���� -.   $��
Cam7_Image��$��5��$
��Left=�$��96��$��759,269,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��759,247,0,502q�$
��Wrap=�$��5��$��1���� $
�Cam ���� $	 zh-cn�$�7���� -/   $��
Cam8_Image��$��5��$
��Left=�$��96��$��919,269,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��919,248,0,501q�$
��Wrap=�$��5��$��1���� $
�Cam ���� $	 zh-cn�$�8���� -0   $��CamVO_Image��$��5��$
��Left=�$��96��$��1079,269,0,0q�$	��Top=�$	��121��$��1��$��
sample.jpg���� $
��Left=�$��1079,248,0,501q�$
��Wrap=�$��5��$��1���� $
�Cam ���� $	 zh-cn�$�VO���� -1   $��Section_Image��$��5��$
��Left=�$	��320��$��759,397,0,0q�$	��Top=�$	��320��$��1��$��sample_2d.jpg���� $
��Left=�$��760,374,0,375q�$
��Wrap=�$��5��$��1���� $	 zh-cn�$�二维截面���� -2   $��Section_Detail_List��$��1091,397,0,0q�$	��320��$	��Top=�$
��Left=�$	��109��$��1��$��5���� -3   $��Clear_DebugList_Button��$��清空��$
��Left=�$��608,583,0,0q�$	��Top=�$��1��$��69��$��20��$��5���� -4    ��Name$ Data_Disk_Clear_Button��$��清空��$��Right=�$��0,311,1034,0q�$	��Top=�$��1��$��5��$��39���� -5   $ Scan_Data_Disk_Button��$��扫描数据盘��$
��Left=�$��160,336,0,0q�$	��Top=�$��1��$��5��$��84���� -6   $��Line_List_View_Button�����	#FFDDDDDD$��	线路表��$
��Left=�$��
363,47,0,0q�$	��Top=�$��1��$��5��$��70���� -7   $��Record_List_View_Button�����	#AF535353$��	记录表��$
��Left=�$��
442,47,0,0q�$	��Top=�$��1��$��5��$��70���� $
��Left=�$��
301,49,0,0q�$
��Wrap=�$	��Top=�$��5��$��1���� $	 zh-cn�$�视图选择���� -8   $��Data_List_View_Button�����	#FFDDDDDD$��	数据表��$
��Left=�$��
520,47,0,0q�$	��Top=�$��1��$��5��$��70���� -9   $��Reset_Date_Picker_Button��$��重置��$��Center=�$	��Top=�$��5��$��76��$��0,47,0,0q�$��1���� -:   $��Selected_Condition_Text��$
��Left=�$��
301,77,0,0q�$
��Wrap=�$	��Top=�$��5��$��1��$	��375��$��当前选项:���� $��数据概览��.��� ���	#FFE5E5E5�� $��图像概览��.��� ���	#FFE5E5E5