   M S B A M L   `   `   `  ����  
BattleShip    BattleShip.SetupGame   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Battleship Setup��5   	   $	��500��6"   $	��700��6/    � WindowStartupLocation$ CenterScreen=�6;   .��5      ]� ��5   
   �� 5       �� ImageSource$* $resources\images\setupBackground.jpg��6    j� Stretch$
 Fill=�6L   .+�5	   
   ]� $	��Top=�6   .+�5
      ��  �� Orientation$ 
Horizontal=�6   $��Center=�62   $
��Left=�6O   .+�5      �� $��Center=�6   $��14(�65   $
��Bold"�6C   .��6U   Player Name:6c   �� 5      -   5         ��Name$ playerNameTextBox��5      $	��140��65   $��3q�6A    �� 	MaxLength$ 20��6L   5      �� 5      $ 
Horizontal=�6   $��Right=�62   $��Right=�6N   .+�5      �� -   5         ��Name$ startButton��5      $��3q�6.   $��10, 5, 10, 5q�69   .��6j   
Start Game6v   5      5      � 5   
    � 	Resources 5      	 �� 
TargetType
 �� 	Resources �� Color �� Opacity��%  '��       �� 5      "	 ��6   
 5      ���� 5      $ DarkCyan��6\   $ .8Q�6m   (��       6*   * SystemColors.HighlightBrushKey+)6m   �� 5      $ DarkCyan��6Z   $ .8Q�6k   (��       6*   *SystemColors.ControlBrushKey+)6k   5      5      5       � ColumnDefinitions 5      �� 5      $��3* �6#   �� 5      $��3* �6#   .+�5!      ]� $��0��6   .+�5"      �� $	��Top=�6   $��Center=�6-   $��28(�6J   $
��Bold"�6X   .��6j   CHOOSE AN IMAGE6{   �� 5#      
���   �$��5q�6-   $��3q�6A   ._�5$      �� -   5         ��Name$ choosePlayerImage��5$      
������ ��5%      �� 5&      .R�5'   "   � ��6,   �� * (FrameworkElement.ActualWidth)+ �� RelativeSource ��  �� AncestorType$ ScrollContentPresenterL� � 	ItemWidth 5(   (   �� *(ListView.View).ItemWidth+ �� $ ListViewL���5)   (   �� *	ItemWidth+ �� *Self+ � 
ItemHeight 5*   (   �� *(ListView.View).ItemHeight+ �� $ ListViewL�5+      5-      5.      5/      ]� 50      $��1��6   .+�51      �� $	��Top=�6   $��Center=�6-   $��28(�6J   $
��Bold"�6X   .��6j   CHOOSE AN OPPONENT6~   �� 52      
���   �$��5q�6-   $��3q�6A   ._�53      �� -   5        $ chooseOpponent��53      
������ 54      55      56      57      58      59      