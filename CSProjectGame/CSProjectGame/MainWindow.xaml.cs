using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSProjectGame;

namespace CSProjectGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
        
        //ADD BUTTON 'LOAD INTO MEMORY'
        //MAKE TABS INTO UIELEMENTCOLLECTION OR LIST<BUTTON>
        //FILEMANAGEMENT
    {
        string sGameFilesPath { get
            {
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InsideYourComputer", "GameFiles");
            }
        }
        string sAccountFileName { get
            {
                return "acc.bin";
            }
        }

        const int MAXTABS = 12;

        TextBlock text_Welcome;
        
        List<Shape> shapes_ProcessorParts;
        List<TextBlock> texts_RuntimeAnimation;

        enum RuntimeLabel { AddressBus, DataBus, ToALU, RegIndex}
        
        TextBlock text_MemoryController;
        List<TextBlock> texts_MemoryCells;
        
        List<TextBox> texts_Tabs;
        int curTab;

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(sGameFilesPath))
                Directory.CreateDirectory(sGameFilesPath);
            shapes_ProcessorParts = new List<Shape>();
            texts_RuntimeAnimation = new List<TextBlock>();
            texts_MemoryCells = new List<TextBlock>();
            texts_Tabs = new List<TextBox>();
        }

        private void button_Go_Click(object sender, RoutedEventArgs e)
        {
            text_Title.Visibility = Visibility.Collapsed;
            button_Go.Visibility = Visibility.Collapsed;
            ingraph_Initialise();
        }

        #region Initialise Graphics

        private void ingraph_FirstTime_00()
        {
            text_Welcome = new TextBlock { FontFamily = new FontFamily("HP Simplified Light"), FontSize = 14F };
            text_Welcome.Text = "Hello and welcome to Inside Your Computer!\nThis game is all about programming at a very basic level,\nusing assembly language!\nAs well as being educative, it’s gonna be great fun\ntrying to beat the challenges and quests that will come your way..\n\nSo, ready to get started?\npress any key to continue...";
            myStackPanel.Children.Add(text_Welcome);
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_01);
            myStackPanel.Visibility = Visibility.Visible;
            myDockPanel.Children.CollapseElements();
        }

        private void ingraph_FirstTime_01_Tutorial_Tabs()
        {
            text_Welcome.Text = "Use the tabs above to switch between your computer and your code...\n'Main' will show you the computer\nNumbered tabs can be used for multiple coding solutions\n\npress any key to continue...";
            myDockPanel.Children.ShowAllElements();
            myDockPanel.Children[myDockPanel.Children.Count - 1].Visibility = Visibility.Collapsed;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_02);
        }

        private void ingraph_FirstTime_02_Tutorial_Coding_01()
        {
            text_Welcome.Text = "Click on the coding tab to continue..";
            (myDockPanel.Children[1] as Button).Click += new RoutedEventHandler(Code1_Tutorial_01);
        }

        private void ingraph_Initialise()
        {
            ingraph_InitialiseTabs();
            if (!File.Exists(System.IO.Path.Combine(sGameFilesPath, sAccountFileName)))
            {
                File.Create(System.IO.Path.Combine(sGameFilesPath, sAccountFileName));
                ingraph_FirstTime_00();
            }
            else
            {
                //This game has been played before..
                //BinaryReader binaryReader = new BinaryReader(new FileStream(System.IO.Path.Combine(sGameFilesPath, sAccountFileName), FileMode.Open));
            }

        }

        private void ingraph_InitialiseTabs()
        {
            myDockPanel.Visibility = Visibility.Visible;
            myDockPanel.Children.Add(new Button() { FontSize = 14F, Style = (Style)Resources["ButtonStyle4"], Width = 72 });
            gengraph_NewTab("1");
            myDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Click += new RoutedEventHandler(DockButton_Click_AddNewTab);
            myDockPanel.Children.ShowAllElements();
            button_DeleteTab.Click += new RoutedEventHandler(DockButton_Click_DeleteTab);
        }
        #endregion

        #region Genreal Graphics
        private void gengraph_NewTab(string TabContent)
        {
            Button NewTab = new Button() { Width = 36};
            NewTab.Content = TabContent;
            NewTab.Click += new RoutedEventHandler(CodeTab_Click);
            myDockPanel.Children.Add(NewTab);
        }

        private void CodeTab_Click(object sender, RoutedEventArgs e)
        {
            curTab = myDockPanel.Children.IndexOf(sender as Button);
            myStackPanel.Children.CollapseElements();
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
        }
        #endregion

        #region Tutorial Event Handlers
        private void KeyDown_PressAnyToContinue_FirstTime_01(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_01;
            ingraph_FirstTime_01_Tutorial_Tabs();
        }

        private void KeyDown_PressAnyToContinue_FirstTime_02(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_02;
            ingraph_FirstTime_02_Tutorial_Coding_01();
        }
        
        private void Code1_Tutorial_01(object sender, RoutedEventArgs e)
        {
            texts_Tabs.Add(new TextBox() { Text = "Enter your code here and click 'Load To Memory' below to\nload it into your computer's RAM\n\npress any key to continue...", Visibility = Visibility.Visible, FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Background = Brushes.White });
            myStackPanel.Children.CollapseElements();
            myStackPanel.Children.Add(texts_Tabs[0]);
            (sender as Button).Click -= Code1_Tutorial_01;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_03);
        }

        private void KeyDown_PressAnyToContinue_FirstTime_03(object sender, KeyEventArgs e)
        {
            myDockPanel.Children[myDockPanel.Children.Count - 1].Visibility = Visibility.Visible;
            myStackPanel.Children.CollapseElements();
            text_Welcome.Text = "Use the ‘+’ button above to add more tabs.\nYou can have a maximum of\n" + MAXTABS + " tabs running at once...\n\npress any key to continue...";
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_03;
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Click += new RoutedEventHandler(DockButton_Click_AddNewTab);
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_04);
        }

        private void KeyDown_PressAnyToContinue_FirstTime_04(object sender, KeyEventArgs e)
        {
            
        }
        #endregion

        private void DockButton_Click_AddNewTab(object sender, RoutedEventArgs e)
        {
            Button NewTab = new Button() { Width = 36 };
            int numtabs = myDockPanel.Children.Count;
            NewTab.Content = numtabs;
            if (numtabs < MAXTABS)
            {
                myDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });
                (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Click += DockButton_Click_AddNewTab;
            }
            myDockPanel.Children.ShowAllElements();
        }

        private void DockButton_Click_DeleteTab(object sender, RoutedEventArgs e)
        {
            //texts_Tabs, myDockPanel
            for (int tabtomove = curTab; tabtomove < texts_Tabs.Count; tabtomove++)
                texts_Tabs[tabtomove - 1] = texts_Tabs[tabtomove];
            texts_Tabs[texts_Tabs.Count - 1] = null;
        }
    }
}
