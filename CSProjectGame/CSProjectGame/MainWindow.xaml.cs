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

        //FIGURE OUR UIELEMENTCOLLECTION
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
            } }

        const int MAXTABS = 12;

        TextBlock text_Welcome;
        
        List<Shape> shapes_ProcessorParts;
        List<TextBlock> texts_RuntimeAnimation;

        enum RuntimeLabel { AddressBus, DataBus, ToALU, RegIndex}
        
        TextBlock text_MemoryController;
        List<TextBlock> texts_MemoryCells;

        UIElementCollection uiec_Tabs;
        List<TextBox> texts_Tabs;

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
            tab_Main.Visibility = Visibility.Collapsed;
            tab_Code1.Visibility = Visibility.Collapsed;
        }

        private void ingraph_FirstTime_01_Tutorial_Tabs()
        {
            text_Welcome.Text = "Use the tabs above to switch between your computer and your code...\n'Main' will show you the computer\nNumbered tabs can be used for multiple coding solutions\n\npress any key to continue...";
            //uiec_Tabs.ShowAllElements();
            tab_Main.Visibility = Visibility.Visible;
            tab_Code1.Visibility = Visibility.Visible;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_02);
        }

        private void ingraph_FirstTime_02_Tutorial_Coding_01()
        {
            text_Welcome.Text = "Click on the coding tab to continue..";
            tab_Code1.Click += new RoutedEventHandler(Code1_Tutorial_01);
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
            rect_Tabs.Visibility = Visibility.Visible;
            //uiec_Tabs.Add(tab_Main);
            tab_Main.Visibility = Visibility.Visible;
            //uiec_Tabs.Add(tab_Code1);
            tab_Code1.Visibility = Visibility.Visible;
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
            texts_Tabs.Add(new TextBox() { Text = "Enter your code here and click 'Load To Memory' below to\nload it into your computer's RAM", Visibility = Visibility.Visible, FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black });
            myStackPanel.Children.CollapseElements();
            myStackPanel.Children.Add(texts_Tabs[0]);
            (sender as Button).Click -= Code1_Tutorial_01;
        }
        #endregion
    }
}
