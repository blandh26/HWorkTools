   M S B A M L   `   `   `  ���� G  CH_WorkTools, Version=1.9.8.26, Culture=neutral, PublicKeyToken=null)clr-namespace:H_WorkToolsH_WorkTools      H_WorkTools.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     +localclr-namespace:H_WorkTools     _ [MaterialDesignThemes.Wpf, Version=4.6.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  -     � 
ResizeMode$  CanMinimize=� � Title$ 
MainWindow��$	��700��$	��480��.��� .+�#  MaterialDesignThemes.Wpf.Card  $��1��.����  J� SnapsToDevicePixels .  *  &MaterialDesignNavigationRailTabControl#��[   �� TabStripPlacement$
 Left=�.@��� $��48��$��48������ $
��auto��$
��auto��.+�'  !MaterialDesignThemes.Wpf.PackIcon  $��24��$��24��$��Center=�   Kind$ ImageSizeSelectActual=�.���� $��12q�.+��� $��0,0,0,8q� $  MaterialDesignHeadline5TextBlock#��[ $��截图、录屏���� $��截图快捷键��.���  � ColumnDefinitions �� $	��100 ��� $��60 ��� $��60 ��� $��60 ��� $��60 � � RowDefinitions �� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=�$��35 ��� $��RightToLeft=��� $��RightToLeft=�.+��� $��0��$��0��$��全屏截图���� $��0��$��1�� d� 	IsChecked$ FalseY�$
��Ctrl��  MaterialDesignLightCheckBox#��[ �� $��0��$��2��$
 TrueY�$	��Alt��#��[ �� $��0��$��3��$
 TrueY�$��Shift��#��[ �� -    ��Name$ PhoneTextBox1��$��0��$��4��)  #MaterialDesignThemes.Wpf.HintAssist	   Hint$		 Key��$��A���� $��1��$��0��$��截图���� $��1��$��1��$ FalseY�$
��Ctrl��#��[ �� $��1��$��2��$
 TrueY�$	��Alt��#��[ �� $��1��$��3��$
 TrueY�$��Shift��#��[ �� -   $ PhoneTextBox��$��1��$��4��$		 Key��$��S���� $��2��$��0��$��截图对比���� $��2��$��1��$ FalseY�$
��Ctrl��#��[ �� $��2��$��2��$
 TrueY�$	��Alt��#��[ �� $��2��$��3��$
 TrueY�$��Shift��#��[ �� -   $ PhoneTextBox2��$��2��$��4��$		 Key��$��D���� $��3��$��0��$��对比窗体标题���� -   $ PhoneTextBo1x��$��3��$��1��$��4��$		 Key��$��A���� $��4��$��0��$��	透明度���� $��4��$��1��$��4��
 � Value$
 40Q� � Minimum$ 0Q�   MaterialDesignDiscreteSlider#��[  �� TickPlacement$ BottomRight=� � Maximum �� *DiscreteHorizontal.Maximum+ �� TickFrequency �� *" DiscreteHorizontal.TickFrequency+�� -   $��5��$��0��$��2��$
��Auto��  btn_SetLanguage_label#���  $��14(� � Cursor$
 Hand���� $��6��$��0��$��5��$
��Wrap=�$��0,10,0,0q�ecquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA�� $��录屏快捷键��$��0,12,0,0q�.���  �� $	��100 ��� $��60 ��� $��60 ��� $��60 ��� $��60 � �� $��RightToLeft=�$��40 ��� $��RightToLeft=�$��40 ��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=�.+��� $��0��$��0��$��
帧率FPS:��$��0,10,0,0q��� -    ��Name$ slFps��$��0��$��1��$��3��$
 15Q�$ 15Q�$ 60Q�$ 5Q�$��0,10,0,0q�#��[ $ BottomRight=��� -   $��0��$��4��$��1�� � ToolTip$ 打开目录��$��10,0,0,0q�$
 Hand����0  *MaterialDesignThemes.Wpf.PackIconExtension  *FolderOpenOutline+   Size$ 20X��� $��1��$��0��$��生产路径:��$��0,10,0,0q��� -	   $ txtPath��$��1��$��1��$��3��$
	 Path��$��R��$��25��$��0,10,0,0q�$	��Top=��� -
   $��1��$��4��$��1��$ 设置路径��$��10,0,0,0q���  *FolderOutline+$ 20X��� -    ��Name$ LabMins��$��2��$��0��$��2��$��00 : 00 : 00 s��$��	0,15,0,10q�$��18(��� -    ��Name$ Btn_ScreenRecording_Start��$��2��$��2��$��1��$ 开始��$��10,0,0,0q�{��� *ScreenRecording_Start_Enabled+��  *
VideoBox+$ 20X��� -   $ Btn_ScreenRecording_Pause��$��2��$��3��$��1��$ 暂停��$��10,0,0,0q�{��� *ScreenRecording_Pause_Enabled+��  *	PlayPause+$ 20X��� -   $ Btn_ScreenRecording_Stop��$��2��$��5��$��1��$ 停止��$��10,0,0,0q�{��� *ScreenRecording_Stop_Enabled+��  *Stop+$ 20X��� $��3��$��0��$��5��$
��Wrap=�$��0,10,0,0q�ecquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA�� $��48��$��48������ $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ Laptop=�.���� $��12q�.+��� $��0,0,0,8q�#��[ $��共享桌面���  �� $	��100 ��� $��60 ��� $��60 ��� $��60 ��� $��60 � �� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=�.+��� $��0��$��0��$��NiceName��$��0,10,0,0q��� -   $ txtNiceName��$��0��$��1��$��4��$		 Key��$��bland��$��0,10,0,0q��� $��1��$��0��$��加入IP��$��0,10,0,0q��� -   $ txtIp��$��1��$��1��$��4��$		 Key��$��192.168.3.132��$��0,10,0,0q��� $��2��$��0��$��本地IP��$��0,10,0,0q��� -    ��Name$
 cbIp��$��2��$��1��$��4�� �� DisplayMemberPath$ title�� �� SelectedValuePath$ id��{���  �� Path$ 	IsChecked� �� ElementName$# FilledComboBoxEnabledCheckBox���� $��3��$��0��$��密码��$��0,10,0,0q��� -   $ 	txtRdpKey��$��3��$��1��$��4��$		 Key�� �� 	MaxLength$ 4��$
��1111���� $��4��$��0��$��模式��$��0,10,0,0q�	� -    	�Name$ rbMode1��$��4��$��1��$��共享��  MaterialDesignRadioButton#��[ $
 TrueY�$��0,10,0,0q�	� -   $ rbMode2��$��4��$��3��$��控制��#��[ $��0,10,0,0q��� -   $ BtnDeskShare��$��5��$��0��$��2��$	��120��$��开始共享��$��14(�$
 Hand��$��0,10,0,0q��� -   $ BtnDeskShareJoin��$��5��$��3��$��2��$	��120��$��加入��$��14(�$
 Hand��$��0,10,0,0q��� $��6��$��0��$��5��$
��Wrap=�$��0,10,0,0q�ecquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA�� $��观众列表��$��0,12,0,0q�.���� -    ��Name$ 
LvAudience��$	��280���� $��48��$��48������ $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ Airport=�.���� $��12q�.+��� $��0,0,0,8q�#��[ $��端口工具���  �� $	��100 ��� $��60 ��� $��60 ��� $��60 ��� $��60 � �� $��RightToLeft=��� $��RightToLeft=�.+��� -   $ txtPort��$��0��$��0��$�� ��$ 8���� -   $ BtnPortSelect��$��0��$��1��$��2��$��80��$��查询��$��14(�$
 Hand��$��0,10,0,0q��� -   $ BtnPortKill��$��0��$��3��$��2��$��80��$��杀掉��$��14(�$
 Hand��$��0,10,0,0q��� -   $ LvPort��$��1��$��0��$��5��$	��530��$��0,10,0,0q��� $��48��$��48������ $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ ContentPaste=�.���� $��12q�.+��� $��0,0,0,8q�#��[ $��粘贴助手��� $��0,0,0,10q� �� �� �� �� ��  �� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=�.+�d� -    d�Name$ 
BtnTopMost��$��0��$��0��$��1��$��40��$��40�� $  MaterialDesignActionToggleButton#��[ $ 置顶��$
 Hand����     Kind$	 Pin=�1  +MaterialDesignThemes.Wpf.ToggleButtonAssist   	OnContent   $ PinOff=�$ 20X�d� -   $ BtnClipboard��$��0��$��1��$��1��$��40��$��40��#��[ $ 监听��$
 Hand����  $ ClipboardPlus=�   $ ClipboardOff=�d� -   $ 	BtnDelete��$��0��$��5��$��1��$��40��$��40��#��[ $ 删除��$
 Hand����  $ DeleteEmptyOutline=�   $ DeleteOffOutline=��� -   $ LvClipboard��$��1��$��0��$��5��$	��500��$��0,10,0,0q�  J� 	AllowDrop  . �� $��2��$��0��$��5��$
��Wrap=�$��0,10,0,0q�ecquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaquiaAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA�� $��48��$��48������ $
��auto��$
��auto��.+�  $��24��$��24��$��Center=�$ 	Translate=�.���� $��12q�.+��� $��0,0,0,8q�#��[   btn_Language#���  �  �� �� �� �� ��  �� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=��� $��RightToLeft=�.+��� $��0��$��0��  btn_Language_label#���  $��2���� -    $ 
cbLanguage��$��0��$��1��$��4��$ title��$ id��$	 Language��{��� $ 	IsChecked�$# FilledComboBoxEnabledCheckBox���� $��1��$��0��$��剪贴板密码��$��2��$��0,15,0,0q��� -!   $ txtClipboardPassword��$��1��$��1��$��4��$��0,15,0,0q�$ 32��$	 Password���� $��2��$��0��$��开机启动��$��2��$��0,15,0,0q�	� -"   $ rbStart1��$��2��$��1��$	��是��#��[ $
 TrueY�$��0,10,0,0q�	� -#   $ rbStart2��$��2��$��3��$	��否��#��[ $��0,10,0,0q��� -$   $��3��$��0��$��2��$
��Auto��#���  $��14(�$
 Hand��$��
0,10,0,-10q�