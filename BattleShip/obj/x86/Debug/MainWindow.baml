   M S B A M L   `   `   `  ����E  ABattleShip, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:BattleShip
BattleShip  5   	       BattleShip.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   &cclr-namespace:BattleShip     5   	   -   5          �Name$  
mainWindow��5   	    � Title$ 
Battleship��5   	   $	��625��6   $
��1125��6)   $	��675��66   $
��1125��6F    � WindowStartupLocation$ CenterScreen=�6V   .��5      � -   5         �Name$ MainGrid��5      ��5   
   �� 5	       �� ImageSource$/ )resources\images\mainWindowBackground.jpg��6    j� Stretch$
 Fill=�6Q   .+�5   
   ]� .+�5      ]� -   5         ]�Name$ TopPanel��5      $	��Top=�6*   $��20, 5, 20, 5q�6?   .+�5      �� $
��Left=�6   .+�5      �� -   5         ��Name$ newGame��5      .��6D   
New Game6N   5      �� 5      $��Center=�6   $��20(�65   $
��Bold"�6C   $��
BATTLESHIP��6U   {�.  �� Target 6j   �� 6�   5      ]� 5      -   5        $ Player1_Panel��5      $��20, 0, 20, 20q�6/   $	��150��6F   ��5      �� 5      $0 *resources\images\playerPanelBackground.png��6!   $ UniformToFill=�6Z   .+�5      �� -   5        $ 
resetShips��5      $��Bottom=�6-   .��6^   Reset Ships6k   �� 5      -   5        $ 
placeShips��5      $��Bottom=�6-   .��6^   Place Ships6k   �� 5      	 �� Orientation$	 Vertical=�6   $
��Left=�64   .+�5      �� 
 �� CornerRadius$
 10��6   $��5q�6/   
���   �._�5      �� -   5         ��Name$ player1_Image��5       $��Center=�67   $��Center=�6T   $��UniformToFill=�6p   5      �� 5      -	   5         ��Name$ player1_Name��5      $��Center=�62   $
��Bold"�6O   $��12(�6a   �� 5      -
   5         ��Name$ player1_Ships��5   !   $	 Vertical=�68   5      5      �� 5       -   5        $ OpponentPanel��5       $	 Vertical=�60   $��Right=�6G   $��20, 0, 20, 20q�6^   $	��150��6u   ��5!      �� 5"      $0 *resources\images\playerPanelBackground.png��6!   $ UniformToFill=�6Z   .+�5$      �� $
 10��6   $��5q�6+   
���   �._�5%      �� -   5        $ player2_Image��5%      $��Center=�63   $��UniformToFill=�6P   5&      �� 5'      -   5        $ player2_Name��5'      $��Center=�6.   $
��Bold"�6K   $��12(�6]   �� 5(      -   5        $ player2_Ships��5(      $	 Vertical=�64   5)      �� 5*      -   5        $ notificationPanel��5*      $	 Vertical=�64   $��Bottom=�6K   $��0,0,0,20q�6c   $��80��6u   ��5+      �� 5,      $6 0resources\images\notificationPanelBackground.png��6   $ UniformToFill=�6\   5.      � 5/       � ColumnDefinitions 50      �� 51      $��1* �6'   �� 52      $��20 �6'   �� 53      $��1* �6'   .+�56      � -   5        $ 	shipBoard��56      $��0��6*   $��Center=�6:   ��6U   ��  �� ElementName$ 	shipBoard�� �� Path$ ActualWidth���57      �� 58      $  resources\images\board.png��6%   $ Uniform=�6N   5:      � 5<      -   5        $ hitBoard��5<      $��2��6)   $��Center=�69   ��6T   �� $ hitBoard��$ ActualWidth���5=      �� 5>      $  resources\images\board.png��6%   $ Uniform=�6N   5@      5A      5B      5C      5D      