   M S B A M L   `   `   `  ����"  Haeahn.Performance.Transaction<    6Haeahn.Performance.Transaction.View.RegistrationWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          �Name$  window��5   O    � Title$ RegistrationWindow��6   $	��450��6    $	��500��6-    � 
ResizeMode$ NoResize=�69    J� InputBindings 5
      �� 5   
   
 �� Key$ Escape��6    �� Command 6"   �� *CancelCommand+ �� CommandParameter 6D   ��  �� ElementName$ window��.��5      �  � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   	 � ColumnDefinitions	 5   
   �� 5      $��* �6   .+�5   
   �� $
��Wrap=�6   $)��#작업 정보 수집 시스템 v1.0��6(   $��Center=�6A   $	��Top=�6^   $��14(�6v   $��0,35,0,0q�6�   $��1��6�   �� 5   
   -   5        
 ��Name$
 IdBox��5   �    �� TextWrapping$
 Wrap=�6   $	��Top=�6&   $��25��6>   $
��Left=�6J    �� TextAlignment$
 Left=�6e   $	��221��6z   $��156,101,0,0q�6�   $��12(�6�   $��1��6�   $��2��6�   ��6�   �� *UserId+6�   G� 5   
   -   5        $
��Left=�5   D   $��
156,37,0,0q�6_   $	��Top=�6s   $	��221��6�   $��25��6�   $��2��6�   �� 5   
   -   5         ��Name$ connectButton��5      $��Connect��6)   $��34��6W   $��100,10,100,0q�6c   $	��Top=�6y   #��� ��6�   
�������$��3��6�   �� 5   
   $��HUB ID:��6   $
��Left=�6"   $��
90,101,0,0q�6=   $��1��6Q   $	��Top=�6^   $��55��6v   $��2��6�   �� 5   
   $��	Password:��6   $
��Left=�6$   $��	86,36,0,0q�6?   $��2��6R   $	��Top=�6_   �� 5   
   $��Center=�6   $��52��6-   $��0,77,0,0q�69   $��2��6K   $	��Top=�6\   $	��172��6t   $I��C/Haeahn.Performance.Transaction;component/Resources/HAEAHN_LOGO.png��6�   �� 5    
   $+��%*HUB ID: @ 앞 부분 (ex. kd.hong)
��6   $
��Left=�6B   $��2��6]   $	��Top=�6j   $��28��6�   $	��208��6�   $��
162,71,0,0q�6�   5"      5#      