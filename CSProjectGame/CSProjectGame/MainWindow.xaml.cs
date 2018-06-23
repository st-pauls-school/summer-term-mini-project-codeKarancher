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

        int NumRegisters, MemorySpec, ALUSpec, ClockSpeedSpec;
        int[] lookup_MemorySpec;
        int[] lookup_ClockSpeedSpec;
        
        List<Shape> shapes_ProcessorParts;
        List<TextBlock> texts_RuntimeAnimation;

        enum RuntimeLabel { AddressBus, DataBus, ToALU, RegIndex}
        
        TextBlock text_MemoryController;
        TextBlock[] texts_MemoryCells;
        byte[] bytes_Commands;
        
        List<TextBox> texts_Tabs;
        int curTab = 1;

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(sGameFilesPath))
                Directory.CreateDirectory(sGameFilesPath);
            lookup_MemorySpec = new int[] { 20, 25, 30, 35, 40, 45, 50 };   //lookup_MemorySpec[memoryspec] will give the number of bytes of memory that the player has
            lookup_ClockSpeedSpec = new int[] { 1500, 1270, 1040, 810, 580, 350, 120 }; //lookup_ClockSpeedSpec[clockspeedspec] will give the number of milliseconds to take per operation

            shapes_ProcessorParts = new List<Shape>();
            texts_RuntimeAnimation = new List<TextBlock>();
            texts_Tabs = new List<TextBox>();

            SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged_ResizeElements);
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
            text_Welcome = new TextBlock { FontFamily = new FontFamily("HP Simplified"), FontSize = 14F };
            text_Welcome.TextWrapping = TextWrapping.WrapWithOverflow;
            text_Welcome.Text = "Hello and welcome to Inside Your Computer! This game is all about programming at a very basic level, using assembly language! As well as being educative, it’s gonna be great fun trying to beat the challenges and quests that will come your way..\n\nSo, ready to get started?\npress any key to continue...";
            myStackPanel.Children.Add(text_Welcome);
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_01);
            myStackPanel.Visibility = Visibility.Visible;
            myDockPanel.Children.CollapseElements();
            toolsDockPanel.Visibility = Visibility.Collapsed;
        }

        private void ingraph_FirstTime_01_Tutorial_Tabs()
        {
            text_Welcome.Text = "Use the tabs above to switch between your computer and your code... 'Main' will show you the computer. Numbered tabs can be used for multiple coding solutions\n\npress any key to continue...";
            myDockPanel.Children.ShowAllElements();
            myDockPanel.Children[myDockPanel.Children.Count - 1].Visibility = Visibility.Collapsed;
            (myDockPanel.Children[myDockPanel.Children.Count - 2] as Button).Click -= CodeTab_Click;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_02_1);
        }

        private void ingraph_FirstTime_02_Tutorial_Coding_01()
        {
            text_Welcome.Text = "Click on the coding tab to continue..";
            (myDockPanel.Children[1] as Button).Click += new RoutedEventHandler(Code1_Tutorial_01);
        }

        private void ingraph_FirstTime_02_Tutorial_Coding_02()
        {
            text_Welcome.Text = "Click on the delete button in the top right to delete the current tab\n\npress any key to continue...";
            button_DeleteTab.Visibility = Visibility.Visible;
            button_DeleteTab.Click -= DockButton_Click_DeleteTab;
            button_DeleteTab.Click += DockButton_Click_DeleteTab_Tutorial;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_04);
        }

        private void ingraph_FirstTime_03()
        {
            text_Welcome.Text = "Use the button on the top right to toggle to and fro the assembly code guidlines\n\npress any key to continue...";
            button_CodeManual.Visibility = Visibility.Visible;
            button_CodeManual.Click += Button_CodeManual_Click_Tutorial_Open;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_05);
        }

        private void ingraph_FirstTime_04_Tutorial_ComputerAnimations()
        {
            text_Welcome.Text = "You will start with just 1 register, 10 bytes of memory, a basic ALU and minimal clock speed... Click on the 'Main' tab to see your brand new computer hot out of the oven!";
            NumRegisters = 1;
            shapes_ProcessorParts.Add(new Rectangle() { Width = ActualWidth / 8, Height = ActualHeight / 8, Fill = Brushes.Gray });
            MemorySpec = 0;
            ALUSpec = 0;
            ClockSpeedSpec = 0;
            (myDockPanel.Children[0] as Button).Click += new RoutedEventHandler(MainTab_Click);
        }

        private void ingraph_Initialise()
        {
            if (!File.Exists(System.IO.Path.Combine(sGameFilesPath, sAccountFileName)))
            {
                BinaryWriter binaryWriter = new BinaryWriter(new FileStream(System.IO.Path.Combine(sGameFilesPath, sAccountFileName), FileMode.Create));
                binaryWriter.BaseStream.Position = 0;
                binaryWriter.Write((byte)'x');
                ingraph_InitialiseTabs_Tutorial();
                ingraph_FirstTime_00();
            }
            else
            {
                BinaryReader binaryReader = new BinaryReader(new FileStream(System.IO.Path.Combine(sGameFilesPath, sAccountFileName), FileMode.Open));
                binaryReader.BaseStream.Position = 0;
                byte b;
                if ((b = binaryReader.ReadByte()) == (byte)'x')
                {
                    ingraph_InitialiseTabs_Tutorial();
                    ingraph_FirstTime_00();
                }
                else
                {
                    //Has been played before, tutorial has been watched
                    ingraph_InitialiseFromFile(binaryReader);
                    ingraph_RenderComputer();
                }

            }

        }

        private void ingraph_RenderComputer()
        {
            throw new NotImplementedException();
        }

        private void ingraph_InitialiseTabs_Tutorial()
        {
            myDockPanel.Visibility = Visibility.Visible;
            myDockPanel.Children.Add(new Button() { FontSize = 14F, Style = (Style)Resources["ButtonStyle4"], Width = 72 });    //Main button
            NewTab("1");   //Tab 1
            myDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });   //+ tab
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Click += new RoutedEventHandler(DockButton_Click_AddNewTab);   //+ tab event handler
            myDockPanel.Children.ShowAllElements();
            button_CodeManual.Visibility = Visibility.Collapsed;
            button_DeleteTab.Click += new RoutedEventHandler(DockButton_Click_DeleteTab);
            button_LoadIntoMem.Click += new RoutedEventHandler(DockButton_Click_LoadIntoMemory);
        }

        private void ingraph_InitialiseFromFile(BinaryReader binaryReader)
        {
            //ALWAYS OPEN INTO TAB 1, BECAUSE CURTAB = 1
            myStackPanel.Children.CollapseElements();
            binaryReader.BaseStream.Position = 0;
            int numtabs = KSFileManagement.GetNumTabs(binaryReader);
            string[] texts = KSFileManagement.GetTabTexts(binaryReader);
            for (int i = 0; i < numtabs; i++)
            {
                texts_Tabs.Add(new TextBox { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Text = texts[i], TextWrapping = TextWrapping.WrapWithOverflow });
                myStackPanel.Children.Add(texts_Tabs[i]);
                texts_Tabs[i].Visibility = Visibility.Collapsed;
                NewTab((i + 1).ToString());
            }
            myDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Click += new RoutedEventHandler(DockButton_Click_AddNewTab);
            NumRegisters = KSFileManagement.GetNumRegisters(binaryReader);
            MemorySpec = KSFileManagement.GetMemorySpec(binaryReader);
            texts_MemoryCells = new TextBlock[lookup_MemorySpec[MemorySpec]];
            ALUSpec = KSFileManagement.GetALUSpec(binaryReader);
            ClockSpeedSpec = KSFileManagement.GetClockSpeedSpec(binaryReader);
            texts_Tabs[0].Visibility = Visibility.Visible;
        }
        #endregion

        #region Tabs Functions
        private void DockButton_Click_AddNewTab(object sender, RoutedEventArgs e)
        {
            int numtabs = myDockPanel.Children.Count;
            Button NewTab = new Button() { Width = ActualWidth / 14 };
            NewTab.Content = numtabs - 1;
            Button AddTab = myDockPanel.Children[myDockPanel.Children.Count - 1] as Button;
            myDockPanel.Children.RemoveAt(myDockPanel.Children.Count - 1);
            myDockPanel.Children.Add(NewTab);
            myDockPanel.Children.Add(AddTab);
            myDockPanel.Children.ShowAllElements();
            if (numtabs - 1 == MAXTABS)
            {
                AddTab.Visibility = Visibility.Collapsed;
            }

            Random r = new Random(DateTime.Today.Millisecond);
            int i = r.Next(2);
            texts_Tabs.Add(new TextBox() { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Background = Brushes.LightSkyBlue, TextWrapping = TextWrapping.WrapWithOverflow });

            texts_Tabs[(curTab = numtabs - 1) - 1].Text = (i == 0) ? "Enter code here" : ((i == 1) ? "Making something new?" : "New idea? Put it into code here");
            myStackPanel.Children.CollapseElements();
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
        }

        private void DockButton_Click_DeleteTab(object sender, RoutedEventArgs e)
        {
            //texts_Tabs, myDockPanel must be edited
            if (texts_Tabs.Count == 1)
                return;
            texts_Tabs.RemoveAt(curTab - 1);
            myDockPanel.Children.RemoveAt(curTab);
            myDockPanel.Children[myDockPanel.Children.Count - 1].Visibility = Visibility.Visible;
            curTab--;
            if (curTab == 0)
                curTab = 1;
        }

        private void NewTab(string TabContent)
        {
            Button NewTab = new Button() { Width = 36};
            NewTab.Content = TabContent;
            NewTab.Click += new RoutedEventHandler(CodeTab_Click);
            myDockPanel.Children.Add(NewTab);
        }

        private void CodeTab_Click(object sender, RoutedEventArgs e)
        {
            if (curTab == 0)
            {
                for (int i = 0; i < shapes_ProcessorParts.Count; i++)
                    shapes_ProcessorParts[i].Visibility = Visibility.Collapsed;
                myStackPanel.Visibility = Visibility.Visible;
                toolsDockPanel.Visibility = Visibility.Visible;
            }
            curTab = myDockPanel.Children.IndexOf(sender as Button);
            myStackPanel.Children.CollapseElements();
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
            (myDockPanel.Children[curTab] as Button).Background = Brushes.SteelBlue;
        }

        private void MainTab_Click(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.CollapseElements();
            myStackPanel.Visibility = Visibility.Collapsed;
            for (int i = 0; i < shapes_ProcessorParts.Count; i++)
                shapes_ProcessorParts[i].Visibility = Visibility.Visible;
            curTab = 0;
        }
        #endregion

        #region Tutorial Event Handlers
        private void KeyDown_PressAnyToContinue_FirstTime_01(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_01;
            ingraph_FirstTime_01_Tutorial_Tabs();
        }

        private void KeyDown_PressAnyToContinue_FirstTime_02_1(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_02_1;
            ingraph_FirstTime_02_Tutorial_Coding_01();
        }
        
        private void Code1_Tutorial_01(object sender, RoutedEventArgs e)
        {
            texts_Tabs.Add(new TextBox() { Text = "Enter your code here and click 'Load To Memory' on the top right panel to load it into your computer's RAM\n\npress any key to continue...", Visibility = Visibility.Visible, FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Background = Brushes.White, TextWrapping = TextWrapping.WrapWithOverflow });
            myStackPanel.Children.CollapseElements();
            myStackPanel.Children.Add(texts_Tabs[0]);
            toolsDockPanel.Visibility = Visibility.Visible;
            button_LoadIntoMem.Visibility = Visibility.Visible;
            button_LoadIntoMem.Click -= DockButton_Click_LoadIntoMemory;
            (sender as Button).Click -= Code1_Tutorial_01;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_02_2);
        }

        private void KeyDown_PressAnyToContinue_FirstTime_02_2(object sender, KeyEventArgs e)
        {
            Button add = myDockPanel.Children[myDockPanel.Children.Count - 1] as Button;
            add.Visibility = Visibility.Visible;
            add.Click -= DockButton_Click_AddNewTab;
            add.Click += DockButton_Click_AddNewTab_Tutorial;
            myStackPanel.Children.CollapseElements();
            text_Welcome.Text = "Use the ‘+’ button above to add more tabs. You can have a maximum of " + MAXTABS + " tabs running at once...\n\npress any key to continue...";
            text_Welcome.Visibility = Visibility.Visible;
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_02_2;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_03);
        }

        private void KeyDown_PressAnyToContinue_FirstTime_03(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_03;
            ingraph_FirstTime_02_Tutorial_Coding_02();
        }

        private void KeyDown_PressAnyToContinue_FirstTime_04(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_04;
            ingraph_FirstTime_03();
        }

        private void Button_CodeManual_Click_Tutorial_Open(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.CollapseElements();
            ListView listInstructions = new ListView();
            listInstructions.Items.Add("The following statements can be used in the coding tabs. <op> can be a register, number or memory cell. Rn represents the nth register. Rx will generally be used for the target register.");
            listInstructions.Items.Add("LDR Rx, <op>\t Copies <op>’s value into Rx");
            listInstructions.Items.Add("STR Rx, <op>\tCopies the value in Rx into the memory cell of <op>");
            listInstructions.Items.Add("ADD Rx, Rn, <op> Add the value specified in <op> to the value in Rn, store in Rx");
            listInstructions.Items.Add("SUB Rx, Rn, <op> Subtract the value specified by <op> from the value in Rn, store in Rx");
            listInstructions.Items.Add("MOV Rx, <op> Copy the value specified by <op> int Rx");
            listInstructions.Items.Add("CMP Rn, <op> Compare the value stored in Rn with <op>");
            listInstructions.Items.Add("B <label> Always branch to <label>");
            listInstructions.Items.Add("B<condition> <label> Branch to <label> if the last comparison met the criterion specified by <condition>.\n\tPossible values for <condition> and their meanings are:\n\tEQ: equal to NE: not equal to\n\tGT: greater than LT: less than");
            listInstructions.Items.Add("AND Rx, Rn, <op> Perform a bitwise logical AND operation between the value in Rn and the value specified by <op>, storing in Rx");
            listInstructions.Items.Add("ORR Rx, Rn, < op > Perform a bitwise logical OR operation between the value in Rn and the value specified by<op>, storing in Rx");
            myStackPanel.Children.Add(listInstructions);
            (sender as Button).Click -= Button_CodeManual_Click_Tutorial_Open;
            (sender as Button).Click += Button_CodeManual_Click_Tutorial_Close;
        }

        private void Button_CodeManual_Click_Tutorial_Close(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            text_Welcome.Visibility = Visibility.Visible;
            (sender as Button).Click -= Button_CodeManual_Click_Tutorial_Close;
            (sender as Button).Click += Button_CodeManual_Click_Tutorial_Open;
        }

        private void DockButton_Click_AddNewTab_Tutorial(object sender, RoutedEventArgs e)
        {
            DockButton_Click_AddNewTab(sender, e);
            texts_Tabs[curTab - 1].Visibility = Visibility.Collapsed;
            text_Welcome.Visibility = Visibility.Visible;
        }

        private void DockButton_Click_DeleteTab_Tutorial(object sender, RoutedEventArgs e)
        {
            if (texts_Tabs.Count == 1)
                return;
            texts_Tabs.RemoveAt(curTab - 1);
            myDockPanel.Children.RemoveAt(curTab);
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Visibility = Visibility.Visible;
            curTab--;
            if (curTab == 0)
                curTab = 1;
        }

        private void KeyDown_PressAnyToContinue_FirstTime_05(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_05;
            ingraph_FirstTime_04_Tutorial_ComputerAnimations();
        }
        #endregion

        #region Other Event Handlers
        private void DockButton_Click_LoadIntoMemory(object sender, RoutedEventArgs e)
        {
            byte[] bytes_Instructions = KSAssemblyCode.Interpret(texts_Tabs[curTab - 1].Text);
            for (int i = 0; i < bytes_Instructions.Length; i++)
                texts_MemoryCells[i] = new TextBlock() { Text = bytes_Instructions[i].ToString(), Foreground = Brushes.Black, Background = Brushes.LightGray };
            bytes_Instructions.CopyTo(bytes_Commands, 0);
        }

        private void Button_CodeManual_Click_Open(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.CollapseElements();
            ListView listInstructions = new ListView();
            listInstructions.Items.Add("The following statements can be used in the coding tabs. <op> can be a register, number or memory cell. Rn represents the nth register. Rx will generally be used for the target register.");
            listInstructions.Items.Add("LDR Rx, <op>\t Copies <op>’s value into Rx");
            listInstructions.Items.Add("STR Rx, <op>\tCopies the value in Rx into the memory cell of <op>");
            listInstructions.Items.Add("ADD Rx, Rn, <op> Add the value specified in <op> to the value in Rn, store in Rx");
            listInstructions.Items.Add("SUB Rx, Rn, <op> Subtract the value specified by <op> from the value in Rn, store in Rx");
            listInstructions.Items.Add("MOV Rx, <op> Copy the value specified by <op> int Rx");
            listInstructions.Items.Add("CMP Rn, <op> Compare the value stored in Rn with <op>");
            listInstructions.Items.Add("B <label> Always branch to <label>");
            listInstructions.Items.Add("B<condition> <label> Branch to <label> if the last comparison met the criterion specified by <condition>.\n\tPossible values for <condition> and their meanings are:\n\tEQ: equal to NE: not equal to\n\tGT: greater than LT: less than");
            listInstructions.Items.Add("AND Rx, Rn, <op> Perform a bitwise logical AND operation between the value in Rn and the value specified by <op>, storing in Rx");
            listInstructions.Items.Add("ORR Rx, Rn, < op > Perform a bitwise logical OR operation between the value in Rn and the value specified by<op>, storing in Rx");
            myStackPanel.Children.Add(listInstructions);
            (sender as Button).Click -= Button_CodeManual_Click_Open;
            (sender as Button).Click += new RoutedEventHandler(Button_CodeManual_Click_Close);
        }

        private void Button_CodeManual_Click_Close(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
            (sender as Button).Click -= Button_CodeManual_Click_Close;
            (sender as Button).Click += Button_CodeManual_Click_Open;
        }

        private void MainWindow_SizeChanged_ResizeElements(object sender, SizeChangedEventArgs e)
        {
            myStackPanel.Width = ActualWidth * 6 / 7;
            myStackPanel.Height = ActualHeight * 5 / 6;
            toolsDockPanel.Width = ActualWidth * 0.225;
            toolsDockPanel.Height = ActualHeight / 10;
            button_CodeManual.Height = button_LoadIntoMem.Height = button_DeleteTab.Height = toolsDockPanel.Height;
            button_CodeManual.Width = toolsDockPanel.Width * 0.375;
            button_LoadIntoMem.Width = button_DeleteTab.Width = toolsDockPanel.Width * 5 / 16;
            myDockPanel.Width = ActualWidth;
            if (myDockPanel.Children.Count > 0)
            {
                (myDockPanel.Children[0] as Button).Width = ActualWidth / 7;
                for (int i = 1; i < myDockPanel.Children.Count; i++)
                    (myDockPanel.Children[i] as Button).Width = ActualWidth / 14;
            }
        }
        #endregion
    }
}
