   M S B A M L   `   `   `  ����D  @FileIO_UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:FileIO_UI	FileIO_UI  5   	   %    FileIO_UI.DatabaseSettingDialog   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:FileIO_UI     5   	     � Title$  配置数据库参数��5   	   $	��202��6   $	��300��6&    � 	Resources 5
      ��%    5      .��5      (�  (� Stretch$ UniformToFill=�6   .��5   
   ]� $	��220��6   $	��300��6!   .+�5      � ���	#FFE5E5E5.+�5      �� $�� ��6   $
��Left=�6#   $��	25,32,0,0q�6>   $	��Top=�6Q   $��26��6i   �� 5      $
��Left=�6   $��	88,36,0,0q�65    �� TextWrapping$
 Wrap=�6H   $	��Top=�6\   $	��120��6t   � 5      ���	#FFE5E5E5$��0,0,0,34q�6.   .+�5      �� $��数据库地址��6   $
��Left=�6,   $��	50,39,0,0q�6G   $	��Top=�6Z   $��26��6r   �� 5      -   5        $��DatabaseAddressText��5      $
��Left=�69   $��
128,44,0,0q�6T   $
 Wrap=�6h   $	��Top=�6|   $	��120��6�   �� 5      $��	用户名��6   $
��Left=�6*   $��	61,69,0,0q�6E   $	��Top=�6X   $��26��6p   �� 5      -   5        $��DatabaseUserText��5      $
��Left=�66   $��
127,73,0,0q�6Q   $
 Wrap=�6e   $	��Top=�6y   $	��120��6�   �� 5      -   5        $��Cancel_Button��5      $��取消��62   $
��Left=�6?   $��
69,141,0,0q�6Z   $	��Top=�6n   $��24��6�   $��66��6�   6�   �� 5      -   5        $��Confirm_Button��5      $��确认��63   $
��Left=�6@   $��172,141,0,0q�6[   $	��Top=�6p   $��24��6�   $��66��6�   6�   �� 5      -   5        $��WarningLabel��5      $�� ��60   $
��Left=�6;   $��40,14,0,150q�6V   $	��236��6k   
���  ��6w   �� 5      $��密码��6   $
��Left=�6)   $��	67,98,0,0q�6D   $	��Top=�6W   $��26��6o   G� 5      -   5        $��DatabaseKeyText��5   "   $
��Left=�69   $��127,102,0,0q�6T   $	��Top=�6i   $	��120��6�   5      5      5       5!      5"      