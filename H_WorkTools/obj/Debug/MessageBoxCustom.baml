   M S B A M L   `   `   `  ����  H_WorkTools"    H_WorkTools.MessageBoxCustom   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   _ [MaterialDesignThemes.Wpf, Version=4.6.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5
   	   -   5          � Title$  MessageBoxWindow��5   	   $	��220��6"   $��386.006��6/    � WindowStartupLocation$ CenterScreen=�5   	    � WindowStyle$
 None=�6.   
������� � 
ResizeMode$ 	CanResize=�5	   	    � Topmost .  � ShowInTaskbar .     MaterialDesignBody#���   5   	   $��Regular"�5   	   $��10(�5   	   &   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=�5   	      TextRenderingMode$
 Auto=�5   	     MaterialDesignFont#���  5   	   .��5      �  � ColumnDefinitions 5   
   �� 5      $
��197* �6   �� 5      $
��295* �6   	 � RowDefinitions	 5   
   �� 5      $
��auto �6   �� 5      $��* �6   .+�5   
   #  MaterialDesignThemes.Wpf.Card  -   5        
  Name$
 
cardHeader��5      $��0��62   $��10q�6?   $��0q�6L   +  %MaterialDesignThemes.Wpf.ShadowAssist   ShadowDepth$ Depth5=�6W     PrimaryHueLightBrush#���  6�    ! PrimaryHueDarkForegroundBrush#���  6�   $y�Visible=�6�   $��2��6  .��5      �� .+�5      �� -   5         ��Name$ txtTitle��5      $��Center=�6.   $��Stretch=�6K      MaterialDesignTitleTextBlock#���  6g   $��20(�6�   Message Title6�   5      5      �� 5    
   $��Right=�6   $��0,5,5,0q�62   $	��Top=�6D   $��1��6\   .+�5!      �� -   5         ��Name$ btnClose��5!      $��25��6>   $��25��6I   
�������  PrimaryHueDarkBrush#���  6i    * &MaterialDesignFloatingActionDarkButton#��[ 6�   .��5"      '  !MaterialDesignThemes.Wpf.PackIcon     Kind$ Close=�6*   5#      5$      � 5%   
   $��1��6   $��2��6   .+�5&      � $��20q�6   	 5'      �� 5(      $
��auto �6$   �� 5)      $
��auto �6$   �� 5*      $��* �6$   .+�5-      � $��0��6   	 5.      �� 5/      $
��auto �6(   �� 50      $��* �6(   .+�53      ?  9MaterialDesignThemes.Wpf.Transitions.TransitioningContent  C  =MaterialDesignThemes.Wpf.Transitions.TransitioningContentBase   OpeningEffects 54      ;  5MaterialDesignThemes.Wpf.Transitions.TransitionEffect  55         Kind$ FadeIn=�6>     56      $ SlideInFromBottom=�6>   .��58      �� -   5         ��Name$ 
txtMessage��58   "   $��Center=�66    �� TextWrapping$
 Wrap=�6S    �� 
IsReadOnly . $��0��6y   $��5q�6�   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$  ��6�   $��18(�6�    % !MaterialDesignFloatingHintTextBox#��[ 6�   59      5;      � 5<      $��1��6   $��0,20,0,5q�6$    5=      �� 5>      $��* �6+   �� 5?      $
��auto �6+   �� 5@      $
��auto �6+   �� 5A      $��* �6+   .+�5D      �� -   5        $ btnOk��5D      $��1��6@     MaterialDesignRaisedButton#��[ 6P   $��5q�6�   $	��100��6�   $��OK��6�    � ToolTip$ Ok��6�   �� 5E      -   5        $ 	btnCancel��5E      $��2��6H   #��[ 6X   $��5q�6�   $	��100��6�   $��Cancel��6�   $ Cancel��6�   �� 5F      -   5        $ btnYes��5F      $��1��6B   #��[ 6R   $��5q�6�   $	��100��6�   $	��Yes��6�   $	 Yes��6�   �� 5G      -	   5        $ btnNo��5G      $��2��6@   #��[ 6P   $��5q�6�   $	��100��6�   $��No��6�   $ No��6�   5H      5J      5K      5L      5M      