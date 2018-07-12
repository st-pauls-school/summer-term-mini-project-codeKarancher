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

        Random myUniversalRand;

        const int MAXTABS = 12;

        TextBlock text_Welcome;

        int NumRegisters, MemorySpec, ALUSpec, ClockSpeedSpec;
        int[] lookup_MemorySpec;
        int[] lookup_ClockSpeedSpec;
        
        List<Shape> shapes_ProcessorParts;
        Grid[] gridRegWires;
        List<StackPanel> stackpanels_Registers;
        List<TextBlock> texts_RegisterNames;
        List<TextBlock> texts_Registers;
        TextBlock text_AddressBus, text_DataBus, text_ToALU, text_PC, text_PCName, text_CIR, text_CIRName;
        string sTempStoreRuntimeInfo;
        bool IsCodeChangedRuntime;
        
        TextBlock text_MemoryController;
        TextBlock[] texts_MemoryCellNames;
        TextBlock[] texts_MemoryCells;
        char[][] charars_Commands;

        List<TextBox> texts_TabNames;
        List<TextBox> texts_Tabs;
        int curTab = 1;
        int runTab = 0;

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(sGameFilesPath))
                Directory.CreateDirectory(sGameFilesPath);
            lookup_MemorySpec = new int[] { 20, 25, 30, 35, 40, 45, 50 };   //lookup_MemorySpec[memoryspec] will give the number of bytes of memory that the player has
            lookup_ClockSpeedSpec = new int[] { 1500, 1270, 1040, 810, 580, 350, 120 }; //lookup_ClockSpeedSpec[clockspeedspec] will give the number of milliseconds to take per operation

            shapes_ProcessorParts = new List<Shape>();
            stackpanels_Registers = new List<StackPanel>();
            texts_Registers = new List<TextBlock>();
            texts_RegisterNames = new List<TextBlock>();
            texts_TabNames = new List<TextBox>();
            texts_Tabs = new List<TextBox>();

            SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged_ResizeElements);
            NumRegisters = 6;//DEBUG TO SHOW WIRES WHEN MAKING, THASAL

            gridRegWires = new Grid[] { gridReg1Wire, gridReg2Wire, gridReg3Wire, gridReg4Wire, gridReg5Wire, gridReg6Wire };

            myUniversalRand = new Random(DateTime.Today.Millisecond);
        }

        #region Initialise Graphics

        #region First Time
        private void ingraph_FirstTime_00()
        {
            text_Welcome = new TextBlock { FontFamily = new FontFamily("HP Simplified"), FontSize = 14F };
            text_Welcome.TextWrapping = TextWrapping.Wrap;
            text_Welcome.Text = "Hello and welcome to Inside Your Computer! This game is all about programming at a very basic level, using assembly language! As well as being educative, it’s gonna be great fun trying to beat the challenges and quests that will come your way..\n\nSo, ready to get started?\npress any key to continue...";
            myStackPanel.Children.Add(text_Welcome);
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_01);
            myStackPanel.Visibility = Visibility.Visible;
            myDockPanel.Children.CollapseElements();
            toolsDockPanel.Visibility = Visibility.Collapsed;
        }

        private void ingraph_FirstTime_01_Tabs()
        {
            text_Welcome.Text = "Use the tabs above to switch between your computer and your code... 'Main' will show you the computer. Numbered tabs can be used for multiple coding solutions\n\npress any key to continue...";
            myDockPanel.Children.ShowAllElements();
            myDockPanel.Children[myDockPanel.Children.Count - 1].Visibility = Visibility.Collapsed;
            (myDockPanel.Children[myDockPanel.Children.Count - 2] as Button).Click -= CodeTab_Click;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_02_1);
        }

        private void ingraph_FirstTime_02_Coding_01()
        {
            text_Welcome.Text = "Click on the coding tab to continue..";
            (myDockPanel.Children[1] as Button).Click += new RoutedEventHandler(Code1_Tutorial_01);
        }

        private void ingraph_FirstTime_02_Coding_02()
        {
            text_Welcome.Text = "Click on the delete button in the top right to delete the current tab\n\npress any key to continue...";
            button_DeleteTab.Visibility = Visibility.Visible;
            button_DeleteTab.Click -= DockButton_Click_DeleteTab;
            button_DeleteTab.Click += DockButton_Click_DeleteTab_Tutorial;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_04);
        }

        private void ingraph_FirstTime_03()
        {
            text_Welcome.Text = "Use the button on the top right to toggle to and from the assembly code guidlines\n\npress any key to continue...";
            button_CodeManual.Visibility = Visibility.Visible;
            button_CodeManual.Click += Button_CodeManual_Click_Tutorial_Open;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_05);
        }

        private void ingraph_FirstTime_04_ComputerAnimations()
        {
            NumRegisters = 1;
            shapes_ProcessorParts.Add(new Rectangle() { Width = ActualWidth / 8, Height = ActualHeight / 8, Fill = Brushes.Gray });
            MemorySpec = 0;
            texts_MemoryCells = new TextBlock[lookup_MemorySpec[MemorySpec]];
            texts_MemoryCellNames = new TextBlock[lookup_MemorySpec[MemorySpec]];
            for (int curMem = 0; curMem < lookup_MemorySpec[MemorySpec]; curMem++)
            {
                texts_MemoryCells[curMem] = new TextBlock() { Text = "000000" };
                texts_MemoryCellNames[curMem] = new TextBlock() { Text = curMem.ToString() + ":" };
                texts_MemoryCellNames[curMem].Width = memoryStackPanel1.Width / 3;
                texts_MemoryCellNames[curMem].FontSize = (memoryStackPanel1.Width / 6.5 < memoryStackPanel1.Height / 20) ? memoryStackPanel1.Width / 6.5 : memoryStackPanel1.Height / 20;
                texts_MemoryCells[curMem].FontSize = (memoryStackPanel1.Width / 6.5 < memoryStackPanel1.Height / 20) ? memoryStackPanel1.Width / 6.5 : memoryStackPanel1.Height / 20;
            }
            ALUSpec = 0;
            ClockSpeedSpec = 0;
            (myDockPanel.Children[0] as Button).Click += new RoutedEventHandler(MainTab_Click_Tutorial);
            text_Welcome.Text = "You will start with just 1 register, " + lookup_MemorySpec[MemorySpec] + " memory locations, a basic ALU and minimal clock speed... Click on the 'Main' tab to see your brand new computer hot out of the oven!";
        }

        private void ingraph_FirstTime_05_RuntimeButtons()
        {
            TextBlock tb = runtimeStackPanel.Children[0] as TextBlock;
            tb.Text = "Your computer is here! You can use this side panel to go through the runtime information and notifications that will be displayed here, or click on the button below to toggle to and from your code...\nThe play button will run the code loaded in memory\n\npress any to continue...";
            button_ToggleCode.Visibility = Visibility.Visible;
            button_PlayRun.Visibility = Visibility.Visible;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_06);
        }

        private void ingraph_FirstTime_06_Final()
        {
            (runtimeStackPanel.Children[0] as TextBlock).Text = "Well, that's all you need to know to enjoy Inside Your Computer! Get cracking!\n\npress any key to begin...";
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_07);
        }

        private void ingraph_FirstTime_07_Reset()
        {
            (runtimeStackPanel.Children[0] as TextBlock).Text = ">>No program loaded";
            text_Welcome.Visibility = Visibility.Collapsed;
            (myDockPanel.Children[0] as Button).Click -= Code1_Tutorial_01;
            texts_TabNames[0].Text = "Sample Code";
            for (int i = 1; i < myDockPanel.Children.Count - 1; i++)
            {
                (myDockPanel.Children[i] as Button).Click += CodeTab_Click;
                texts_TabNames[i - 1].TextChanged += new TextChangedEventHandler(text_TabName_TextChanged);
            }
            Button AddTab = (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button);
            AddTab.Click -= DockButton_Click_AddNewTab_Tutorial;
            AddTab.Click += DockButton_Click_AddNewTab;
            button_DeleteTab.Click -= DockButton_Click_DeleteTab_Tutorial;
            button_DeleteTab.Click += DockButton_Click_DeleteTab;
            button_CodeManual.Click -= Button_CodeManual_Click_Tutorial_Open;
            button_CodeManual.Click += DockButton_Click_CodeManual_Open;
            button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory_Tab1;
            (myDockPanel.Children[0] as Button).Click -= MainTab_Click_Tutorial;
            (myDockPanel.Children[0] as Button).Click += MainTab_Click;

            //Prepare some sample code for the user
            texts_TabNames[0].Text = "Sample Code";
            texts_Tabs[0].Text = "Some sample code to store the sum of two values in memory to location 2:\n\nLDR 0, 0\nLDR 1, 1\nADD 0, 0, 1\nSTR 0, 2";
            texts_Tabs[0].TextChanged += new TextChangedEventHandler(CodeTab_TextChanged_TutorialTemporary);
            (myDockPanel.Children[1] as Button).Content = TabTextFromProjectName(texts_TabNames[0].Text);
        }
        #endregion

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

        //Probably unnecessary, currently not implemented
        private void ingraph_RenderComputer()
        {
            throw new NotImplementedException();
        }

        private void ingraph_InitialiseTabs_Tutorial()
        {
            myDockPanel.Visibility = Visibility.Visible;
            myDockPanel.Children.Add(new Button() { FontSize = 14F, Style = (Style)Resources["ButtonStyle4"], Width = ActualWidth / 14 });    //Main button
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
            KSFileManagement.RetrieveProgress(binaryReader);
            int numtabs = KSFileManagement.NumTabsFromFile;
            string[] tnames = KSFileManagement.TabNamesFromFile;
            string[] texts = KSFileManagement.TabTextsFromFile;
            for (int i = 0; i < numtabs; i++)
            {
                texts_TabNames.Add(new TextBox { FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.White, Background = Brushes.DarkMagenta, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Text = tnames[i], TextWrapping = TextWrapping.Wrap, AcceptsReturn = false });
                myStackPanel.Children.Add(texts_TabNames[i]);
                texts_TabNames[i].Visibility = Visibility.Collapsed;
                texts_Tabs.Add(new TextBox { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Text = texts[i], TextWrapping = TextWrapping.Wrap, AcceptsReturn = true });
                myStackPanel.Children.Add(texts_Tabs[i]);
                texts_Tabs[i].Visibility = Visibility.Collapsed;
                NewTab((i + 1).ToString());
            }
            myDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Click += new RoutedEventHandler(DockButton_Click_AddNewTab);
            NumRegisters = KSFileManagement.NumRegFromFile;
            MemorySpec = KSFileManagement.MemSpecFromFile;
            texts_MemoryCells = new TextBlock[lookup_MemorySpec[MemorySpec]];
            texts_MemoryCellNames = new TextBlock[lookup_MemorySpec[MemorySpec]];
            charars_Commands = new char[lookup_MemorySpec[MemorySpec]][];
            for (int i = 0; i < lookup_MemorySpec[MemorySpec]; i++)
            {
                texts_MemoryCells[i] = new TextBlock() { Text = "000000" };
                texts_MemoryCellNames[i] = new TextBlock() { Text = i.ToString() + ":" };
                charars_Commands[i] = new char[8];
            }
            ALUSpec = KSFileManagement.ALUSpecFromFile;
            ClockSpeedSpec = KSFileManagement.ClockSpeedSpecFromFile;
            texts_TabNames[0].Visibility = Visibility.Visible;
            texts_Tabs[0].Visibility = Visibility.Visible;
        }
        #endregion

        #region Tutorial Event Handlers
        private void KeyDown_PressAnyToContinue_FirstTime_01(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_01;
            ingraph_FirstTime_01_Tabs();
        }

        private void KeyDown_PressAnyToContinue_FirstTime_02_1(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_02_1;
            ingraph_FirstTime_02_Coding_01();
        }

        private void Code1_Tutorial_01(object sender, RoutedEventArgs e)
        {
            texts_TabNames.Add(new TextBox() { Text = "Your tab can be named here", FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.White, Background = Brushes.DarkMagenta, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap, AcceptsReturn = false });
            texts_Tabs.Add(new TextBox() { Text = "Enter your code here and click 'Load To Memory' on the top right panel to load it into your computer's RAM\n\npress any key to continue...", Visibility = Visibility.Visible, FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Background = Brushes.White, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true });
            myStackPanel.Children.CollapseElements();
            myStackPanel.Children.Add(texts_TabNames[0]);
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
            ingraph_FirstTime_02_Coding_02();
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
            listInstructions.Items.Add(new TextBlock() { Text = "The following statements can be used in the coding tabs. <op> can be a register, number or memory cell. Rn represents the nth register. Rx will generally be used for the target register.", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "LDR Rx, <mem>\tCopies value in location <mem> into Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "STR Rx, <mem>\tCopies the value in Rx into location <mem>", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "ADD Rx, Rn, <op> Add the value specified in <op> to the value in Rn, store in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "SUB Rx, Rn, <op> Subtract the value specified by <op> from the value in Rn, store in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "MOV Rx, <op> Copy the value specified by <op> int Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "CMP Rn, <op> Compare the value stored in Rn with <op>", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "B <num> Always branch to line number <num>", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "B<condition> <num> Branch to line number <num> if the last comparison met the criterion specified by <condition>.\n\tPossible values for <condition> and their meanings are:\n\tEQ: equal to NE: not equal to\n\tGT: greater than LT: less than", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "AND Rx, Rn, <op> Perform a bitwise logical AND operation between the value in Rn and the value specified by <op>, storing in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "ORR Rx, Rn, <op> Perform a bitwise logical OR operation between the value in Rn and the value specified by<op>, storing in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "HALT Ends the fetch execute cycle", TextWrapping = TextWrapping.Wrap });
            myStackPanel.Children.Add(listInstructions);
            myDockPanel.Visibility = Visibility.Collapsed;
            (sender as Button).Click -= Button_CodeManual_Click_Tutorial_Open;
            (sender as Button).Click += Button_CodeManual_Click_Tutorial_Close;
        }

        private void Button_CodeManual_Click_Tutorial_Close(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            myDockPanel.Visibility = Visibility.Visible;
            text_Welcome.Visibility = Visibility.Visible;
            (sender as Button).Click -= Button_CodeManual_Click_Tutorial_Close;
            (sender as Button).Click += Button_CodeManual_Click_Tutorial_Open;
        }

        private void DockButton_Click_AddNewTab_Tutorial(object sender, RoutedEventArgs e)
        {
            DockButton_Click_AddNewTab(sender, e);
            texts_TabNames[curTab - 1].Visibility = Visibility.Collapsed;
            texts_Tabs[curTab - 1].Visibility = Visibility.Collapsed;
            text_Welcome.Visibility = Visibility.Visible;
        }

        private void DockButton_Click_DeleteTab_Tutorial(object sender, RoutedEventArgs e)
        {
            if (texts_Tabs.Count == 1)
                return;
            texts_TabNames.RemoveAt(curTab - 1);
            texts_Tabs.RemoveAt(curTab - 1);
            myDockPanel.Children.RemoveAt(curTab);
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Visibility = Visibility.Visible;
            curTab--;
            if (curTab == 0)
                curTab = 1;
        }

        private void DockButton_Click_LoadIntoMemory_Tab1(object sender, RoutedEventArgs e)
        {
            if (texts_Tabs[0].Text == "Some sample code to store the sum of two values in memory to location 2:\n\nLDR 0, 0\nLDR 1, 1\nADD 0, 0, 1\nSTR 0, 2")
            {
                texts_Tabs[0].Text = "LDR 0, 0\nLDR 1, 1\nADD 0, 0, 1\nSTR 0, 2";
                texts_Tabs[0].TextChanged -= CodeTab_TextChanged_TutorialTemporary;
                button_LoadIntoMem.Click -= DockButton_Click_LoadIntoMemory_Tab1;
                button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory;
            }
            DockButton_Click_LoadIntoMemory(sender, e);
        }

        private void KeyDown_PressAnyToContinue_FirstTime_05(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_05;
            ingraph_FirstTime_04_ComputerAnimations();
        }

        private void MainTab_Click_Tutorial(object sender, RoutedEventArgs e)
        {
            MainTab_Click(sender, e);
            ingraph_FirstTime_05_RuntimeButtons();
        }

        private void KeyDown_PressAnyToContinue_FirstTime_06(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_06;
            ingraph_FirstTime_06_Final();
        }

        private void KeyDown_PressAnyToContinue_FirstTime_07(object sender, KeyEventArgs e)
        {
            KeyDown -= KeyDown_PressAnyToContinue_FirstTime_07;
            ingraph_FirstTime_07_Reset();
        }

        private void CodeTab_TextChanged_TutorialTemporary(object sender, TextChangedEventArgs e)
        {
            texts_Tabs[0].Text = "LDR 0, 0\nLDR 1, 1\nADD 0, 0, 1\nSTR 0, 2";
            texts_Tabs[0].TextChanged -= CodeTab_TextChanged_TutorialTemporary;
            button_LoadIntoMem.Click -= DockButton_Click_LoadIntoMemory_Tab1;
            button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory;
        }
        #endregion

        #region Tabs Functions

        private void NewTab(string TabContent)
        {
            Button NewTab = new Button() { Width = ActualWidth / 14 };
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
                registersStackPanel.Visibility = Visibility.Collapsed;
                rect_MotherBoardBackGround.Visibility = Visibility.Collapsed;
                processorStackPanel.Visibility = Visibility.Collapsed;
                runtimeStackPanel.Visibility = Visibility.Collapsed;
                runtimeDockPanel.Visibility = Visibility.Collapsed;
                runtimestackpanelBorder.Visibility = Visibility.Collapsed;
                myStackPanel.Visibility = Visibility.Visible;
                toolsDockPanel.Visibility = Visibility.Visible;
                memoryDockPanel.Visibility = Visibility.Collapsed;
                for (int i = 0; i < NumRegisters; i++)
                    gridRegWires[i].Visibility = Visibility.Collapsed;
            }
            curTab = myDockPanel.Children.IndexOf(sender as Button);
            myStackPanel.Children.CollapseElements();
            texts_TabNames[curTab - 1].Visibility = Visibility.Visible;
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
            (myDockPanel.Children[curTab] as Button).Background = Brushes.SteelBlue;
        }

        private void DockButton_Click_AddNewTab(object sender, RoutedEventArgs e)
        {
            if (curTab == 0)
                CodeTab_Click(myDockPanel.Children[1], e);
            int numtabs = myDockPanel.Children.Count;
            Button NewTab = new Button() { Width = ActualWidth / 14 };
            NewTab.Content = TabTextFromProjectName("Project " + (curTab = numtabs - 1).ToString());
            NewTab.Click += CodeTab_Click;
            Button AddTab = myDockPanel.Children[myDockPanel.Children.Count - 1] as Button;
            myDockPanel.Children.RemoveAt(myDockPanel.Children.Count - 1);
            myDockPanel.Children.Add(NewTab);
            myDockPanel.Children.Add(AddTab);
            myDockPanel.Children.ShowAllElements();
            if (numtabs - 1 == MAXTABS)
            {
                AddTab.Visibility = Visibility.Collapsed;
            }

            int i = myUniversalRand.Next() % 3;
            texts_TabNames.Add(new TextBox() { Text = "Project " + curTab, FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.White, Background = Brushes.DarkMagenta, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap, AcceptsReturn = false });
            texts_Tabs.Add(new TextBox() { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Background = Brushes.White, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true });
            texts_Tabs[curTab - 1].Text = (i == 0) ? "Enter code here" : ((i == 1) ? "Making something new?" : "New idea? Put it into code here");
            myStackPanel.Children.Add(texts_TabNames[curTab - 1]);
            myStackPanel.Children.Add(texts_Tabs[curTab - 1]);
            myStackPanel.Children.CollapseElements();
            texts_TabNames[curTab - 1].Visibility = Visibility.Visible;
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
        }

        private string TabTextFromProjectName(string ProjectName)
        {
            if (ProjectName.Length == 0)
                return "Project " + curTab;
            List<char> ToReturn = new List<char>();
            char[] cArProjName = ProjectName.ToCharArray();
            if (cArProjName[0] >= 'A' && cArProjName[0] <= 'Z')
                ToReturn.Add(cArProjName[0]);
            int num = 0;
            for (int i = 0; i < cArProjName.Length; i++)
            {
                if (cArProjName[i] >= '0' && cArProjName[i] <= '9')
                    num = num * 10 + (cArProjName[i] - '0');
                else if (i > 0)
                {
                    if (cArProjName[i - 1] == ' ')
                        ToReturn.Add(cArProjName[i]);
                }
            }
            return new string(ToReturn.ToArray()) + num.ToString();
        }

        #endregion

        #region All Other Event Handlers

        #region Tools Dockpanel - DockButtons
        private void DockButton_Click_LoadIntoMemory(object sender, RoutedEventArgs e)
        {
            runTab = curTab;
            (runtimeStackPanel.Children[0] as TextBlock).Text = texts_Tabs[curTab - 1].Text;
            char[][] charars_Instructions = KSAssemblyCode.Interpret(texts_Tabs[curTab - 1].Text);
            for (int i = 0; i < texts_MemoryCells.Length; i++)
            {
                if (i < charars_Instructions.Length)
                    texts_MemoryCells[i].Text = new string(charars_Instructions[i]);
                else
                    texts_MemoryCells[i].Text = "000000";
            }
            charars_Commands = new char[charars_Instructions.Length][];
            for (int i = 0; i < charars_Instructions.Length; i++)
                charars_Instructions[i].CopyTo((charars_Commands[i] = new char[8]), 0);
        }

        private void DockButton_Click_DeleteTab(object sender, RoutedEventArgs e)
        {
            //texts_Tabs, myDockPanel must be edited
            if (texts_Tabs.Count == 1)
                return;
            texts_TabNames.RemoveAt(curTab - 1);
            texts_Tabs.RemoveAt(curTab - 1);
            myDockPanel.Children.RemoveAt(curTab);
            myDockPanel.Children[myDockPanel.Children.Count - 1].Visibility = Visibility.Visible;
            curTab--;
            if (curTab == 0)
                curTab = 1;
        }

        private void DockButton_Click_CodeManual_Open(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.CollapseElements();
            ListView listInstructions = new ListView();
            listInstructions.Items.Add(new TextBlock() { Text = "The following statements can be used in the coding tabs. <op> can be a register, number or memory cell. Rn represents the nth register. Rx will generally be used for the target register.", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "LDR Rx, <mem>\tCopies value in location <mem> into Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "STR Rx, <mem>\tCopies the value in Rx into location <mem>", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "ADD Rx, Rn, <op> Add the value specified in <op> to the value in Rn, store in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "SUB Rx, Rn, <op> Subtract the value specified by <op> from the value in Rn, store in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "MOV Rx, <op> Copy the value specified by <op> int Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "CMP Rn, <op> Compare the value stored in Rn with <op>", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "B <num> Always branch to line number <num>", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "B<condition> <num> Branch to line number <num> if the last comparison met the criterion specified by <condition>.\n\tPossible values for <condition> and their meanings are:\n\tEQ: equal to NE: not equal to\n\tGT: greater than LT: less than", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "AND Rx, Rn, <op> Perform a bitwise logical AND operation between the value in Rn and the value specified by <op>, storing in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "ORR Rx, Rn, <op> Perform a bitwise logical OR operation between the value in Rn and the value specified by<op>, storing in Rx", TextWrapping = TextWrapping.Wrap });
            listInstructions.Items.Add(new TextBlock() { Text = "HALT Ends the fetch execute cycle", TextWrapping = TextWrapping.Wrap });
            myStackPanel.Children.Add(listInstructions);
            myDockPanel.Visibility = Visibility.Collapsed;
            toolsDockPanel.Visibility = Visibility.Collapsed;
            (sender as Button).Click -= DockButton_Click_CodeManual_Open;
            (sender as Button).Click += DockButton_Click_CodeManual_Close;
        }

        private void DockButton_Click_CodeManual_Close(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            texts_TabNames[curTab - 1].Visibility = Visibility.Visible;
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
            myDockPanel.Visibility = Visibility.Visible;
            toolsDockPanel.Visibility = Visibility.Visible;
            (sender as Button).Click -= DockButton_Click_CodeManual_Close;
            (sender as Button).Click += DockButton_Click_CodeManual_Open;
        }
        #endregion

        #region Main Tab
        private void MainTab_Click(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.CollapseElements();
            myStackPanel.Visibility = Visibility.Collapsed;
            toolsDockPanel.Visibility = Visibility.Collapsed;
            GraphicsForMotherBoard();
            curTab = 0;
        }

        private void GraphicsForMotherBoard()
        {
            rect_MotherBoardBackGround.Visibility = Visibility.Visible;
            registersStackPanel.Visibility = Visibility.Visible;
            processorStackPanel.Visibility = Visibility.Visible;
            memoryDockPanel.Visibility = Visibility.Visible;
            runtimeDockPanel.Visibility = Visibility.Visible;
            bool FirstTimeShowing = texts_Registers.Count == 0 ? true : false;
            for (int curReg = 0; curReg < NumRegisters; curReg++)
            {
                if (FirstTimeShowing)
                {
                    stackpanels_Registers.Add(new StackPanel() { Background = Brushes.DarkGray, Height = registersStackPanel.Height / 6, Width = registersStackPanel.Width });
                    registersStackPanel.Children.Add(stackpanels_Registers[curReg]);
                    texts_RegisterNames.Add(new TextBlock() { Text = "Register " + curReg.ToString(), FontSize = registersStackPanel.Width / 8 < stackpanels_Registers[curReg].Height / 4 ? registersStackPanel.Width / 8 : stackpanels_Registers[curReg].Height / 4, FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.Black, Height = stackpanels_Registers[curReg].Height / 2.5 });
                    stackpanels_Registers[curReg].Children.Add(texts_RegisterNames[curReg]);
                    texts_Registers.Add(new TextBlock() { FontSize = registersStackPanel.Width / 8 < stackpanels_Registers[curReg].Height / 4 ? registersStackPanel.Width / 8 : stackpanels_Registers[curReg].Height / 4, FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.White, Height = stackpanels_Registers[curReg].Height / 3 });
                    stackpanels_Registers[curReg].Children.Add(texts_Registers[curReg]);

                    stackpanels_Registers[curReg].IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(stackpanels_Registers_IsMouseDirectlyOverChanged);
                }
                texts_Registers[curReg].Text = "0000 0000";
                stackpanels_Registers[curReg].Visibility = Visibility.Visible;
                texts_RegisterNames[curReg].Visibility = Visibility.Visible;
                texts_Registers[curReg].Visibility = Visibility.Visible;
            }
            if (FirstTimeShowing)
            {
                text_PCName = new TextBlock() { Text = "Program Counter: ", Width = processorStackPanel1.Width, Height = processorStackPanel1.Height / 2, FontSize = processorStackPanel1.Width / 9 < processorStackPanel1.Height / 3 ? processorStackPanel1.Width / 9 : processorStackPanel1.Height / 3, TextWrapping = TextWrapping.Wrap };
                processorStackPanel1.Children.Add(text_PCName);
                text_PC = new TextBlock() { Width = processorStackPanel1.Width, Height = processorStackPanel1.Height / 2, FontSize = processorStackPanel1.Width / 9 < processorStackPanel1.Height / 3 ? processorStackPanel1.Width / 9 : processorStackPanel1.Height / 3, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap };
                processorStackPanel1.Children.Add(text_PC);

                text_CIRName = new TextBlock() { Text = "Current Instruction Register: ", Width = processorStackPanel2.Width, Height = processorStackPanel2.Height / 2, FontSize = processorStackPanel2.Width / 10.5 < processorStackPanel2.Height / 4.85 ? processorStackPanel2.Width / 10.5 : processorStackPanel2.Height / 4.85, TextWrapping = TextWrapping.Wrap };
                processorStackPanel2.Children.Add(text_CIRName);
                text_CIR = new TextBlock() { Width = processorStackPanel2.Width, Height = processorStackPanel2.Height / 2, FontSize = text_PC.FontSize, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap };
                processorStackPanel2.Children.Add(text_CIR);

                runtimeStackPanel.Children.Add(new TextBlock() { TextWrapping = TextWrapping.Wrap, FontSize = 13, FontFamily = new FontFamily("Courier New"), Foreground = Brushes.LightGreen });//Add scroll bar

                button_ToggleCode.Click += new RoutedEventHandler(button_ToggleCode_Click_Open);
                button_PlayRun.Click += new RoutedEventHandler(button_PlayRun_Click);
                for (int a = 0; a < lookup_MemorySpec[MemorySpec]; a++)
                {
                    if (a < lookup_MemorySpec[MemorySpec] / 2)//add textblock to first stackpanel
                        memoryStackPanel1.Children.Add(new DockPanel() { Children = { texts_MemoryCellNames[a], texts_MemoryCells[a] } });
                    else
                    {
                        memoryStackPanel2.Children.Add(new DockPanel() { Children = { texts_MemoryCellNames[a], texts_MemoryCells[a] } });
                    }
                }
            }
            text_PC.Text = "00";
            text_CIR.Text = "000000";
            text_PCName.Visibility = Visibility.Visible;
            text_PC.Visibility = Visibility.Visible;
            processorStackPanel1.Visibility = Visibility.Visible;
            text_CIRName.Visibility = Visibility.Visible;
            text_CIR.Visibility = Visibility.Visible;
            processorStackPanel2.Visibility = Visibility.Visible;
            runtimeStackPanel.Visibility = Visibility.Visible;
            runtimestackpanelBorder.Visibility = Visibility.Visible;
            (runtimeStackPanel.Children[0] as TextBlock).Text = ">>" + (runTab == 0 ? "No program loaded" : (texts_TabNames[runTab - 1].Text + " program loaded"));
            for (int i = 0; i < NumRegisters; i++)
                gridRegWires[i].Visibility = Visibility.Visible;
        }

        private void stackpanels_Registers_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            int index;
            string[] DigitGroups = texts_Registers[index = stackpanels_Registers.IndexOf(sender as StackPanel)].Text.Split(' ');
            char[][] binGroups = new char[][] { DigitGroups[0].ToCharArray(), DigitGroups[1].ToCharArray() };
            int DecimalEq = KSConvert.BinaryToDecimal(binGroups[0]) * 16 + KSConvert.BinaryToDecimal(binGroups[1]);
            stackpanels_Registers[index].ToolTip = "This register currently contains " + DecimalEq + " in decimal";
        }
        
        private void button_ToggleCode_Click_Open(object sender, RoutedEventArgs e)
        {
            if (runTab == 0) return;
            TextBox tb;
            runtimeStackPanel.Children.Add(tb = new TextBox() { Text = texts_Tabs[runTab - 1].Text, FontFamily = new FontFamily("Courier New"), Foreground = Brushes.LightGreen, Visibility = Visibility.Visible, Background = Brushes.Black, AcceptsReturn = true });
            tb.TextChanged += text_ToggleCode_TextChanged;
            sTempStoreRuntimeInfo = (runtimeStackPanel.Children[0] as TextBlock).Text;
            runtimeStackPanel.Children[0].Visibility = Visibility.Collapsed;
            IsCodeChangedRuntime = false;
            button_ToggleCode.Click -= button_ToggleCode_Click_Open;
            button_ToggleCode.Click += button_ToggleCode_Click_Close;
        }

        private void button_ToggleCode_Click_Close(object sender, RoutedEventArgs e)
        {
            runtimeStackPanel.Children.RemoveAt(runtimeStackPanel.Children.Count - 1);
            runtimeStackPanel.Children[0].Visibility = Visibility.Visible;
            button_ToggleCode.Click -= button_ToggleCode_Click_Close;
            button_ToggleCode.Click += button_ToggleCode_Click_Open;
            curTab = runTab;    //for DockButton_Click_LoadIntoMemory
            DockButton_Click_LoadIntoMemory(button_LoadIntoMem, e);
            curTab = 0;         //because in reality, the user is on the main tab
            TextBlock tb = runtimeStackPanel.Children[0] as TextBlock;
            tb.Text = sTempStoreRuntimeInfo + ((IsCodeChangedRuntime) ? "\n>>Code changed\n>>Program re-loaded into memory" : "");
        }

        private void text_ToggleCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            texts_Tabs[runTab - 1].Text = (sender as TextBox).Text;
            IsCodeChangedRuntime = true;
        }

        private void button_PlayRun_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Miscellaneous
        private void text_TabName_TextChanged(object sender, TextChangedEventArgs e)
        {
            (myDockPanel.Children[curTab] as Button).Content = TabTextFromProjectName((sender as TextBox).Text);
        }

        private void button_Go_Click(object sender, RoutedEventArgs e)
        {
            text_Title.Visibility = Visibility.Collapsed;
            button_Go.Visibility = Visibility.Collapsed;
            ingraph_Initialise();
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
            button_DeleteTab.FontSize = button_DeleteTab.Width / 4;
            myDockPanel.Width = ActualWidth;
            if (myDockPanel.Children.Count > 0)
            {
                (myDockPanel.Children[0] as Button).Width = ActualWidth / 7;
                for (int i = 1; i < myDockPanel.Children.Count; i++)
                    (myDockPanel.Children[i] as Button).Width = ActualWidth / 14;
            }

            rect_MotherBoardBackGround.Width = 11 * ActualWidth / 14;
            rect_MotherBoardBackGround.Height = 299 * ActualHeight / 322;
            registersStackPanel.Width = 3 * ActualWidth / 14;
            registersStackPanel.Height = rect_MotherBoardBackGround.Height - 10;
            if (stackpanels_Registers.Count != 0)
            {
                for (int i = 0; i < stackpanels_Registers.Count; i++)
                {
                    stackpanels_Registers[i].Height = registersStackPanel.Height / 6;
                    texts_RegisterNames[i].Height = stackpanels_Registers[i].Height / 2.5;
                    texts_Registers[i].Height = stackpanels_Registers[i].Height / 3;
                    texts_RegisterNames[i].Width = texts_Registers[i].Width = stackpanels_Registers[i].Width = registersStackPanel.Width;
                    texts_RegisterNames[i].FontSize = texts_Registers[i].FontSize = registersStackPanel.Width / 8 < stackpanels_Registers[i].Height / 4 ? registersStackPanel.Width / 8 : stackpanels_Registers[i].Height / 4;
                }
            }

            processorStackPanel.Width = ActualWidth * 3 / 14;
            processorStackPanel.Height = ActualHeight * 102 / 322;
            processorStackPanel1.Height = processorStackPanel.Height / 2;
            processorStackPanel1.Width = processorStackPanel.Width;
            processorStackPanel2.Height = processorStackPanel.Height / 2;
            processorStackPanel2.Width = processorStackPanel.Width;
            if (text_PCName != null)
            {
                text_PCName.FontSize = processorStackPanel1.Width / 9 < processorStackPanel1.Height / 3 ? processorStackPanel1.Width / 9 : processorStackPanel1.Height / 3;
                text_PCName.Width = processorStackPanel1.Width;
                text_PCName.Height = processorStackPanel1.Height / 2;
                text_PC.FontSize = processorStackPanel1.Width / 9 < processorStackPanel1.Height / 3 ? processorStackPanel1.Width / 9 : processorStackPanel1.Height / 3;
                text_PC.Width = processorStackPanel1.Width;
                text_PC.Height = processorStackPanel1.Height / 2;
                text_CIRName.FontSize = processorStackPanel2.Width / 10.5 < processorStackPanel2.Height / 4.85 ? processorStackPanel2.Width / 10.5 : processorStackPanel2.Height / 4.85;
                text_CIRName.Width = processorStackPanel2.Width;
                text_CIRName.Height = processorStackPanel2.Height / 2;
                text_CIR.FontSize = text_PC.FontSize;
                text_CIR.Width = processorStackPanel2.Width;
                text_CIR.Height = processorStackPanel2.Height / 2;
            }
            runtimeStackPanel.Width = 3 * ActualWidth / 14;
            runtimestackpanelBorder.Width = runtimeStackPanel.Width;
            runtimeStackPanel.Height = ActualHeight * 299 / 322;
            runtimestackpanelBorder.Height = runtimeStackPanel.Height;

            runtimeDockPanel.Width = runtimeStackPanel.Width;
            runtimeDockPanel.Height = 51 * ActualHeight / 322;
            button_ToggleCode.Width = button_PlayRun.Width = runtimeDockPanel.Width / 2;
            button_ToggleCode.Height = button_PlayRun.Height = runtimeDockPanel.Height;

            memoryDockPanel.Width = ActualWidth * 3 / 14 - 5;
            memoryDockPanel.Height = memoryStackPanel1.Height = memoryStackPanel2.Height = ActualHeight * 289 / 322;
            memoryStackPanel1.Width = memoryStackPanel2.Width = memoryDockPanel.Width / 2;
            if (texts_MemoryCellNames != null)
            {
                for (int curMem = 0; curMem < lookup_MemorySpec[MemorySpec]; curMem++)
                {
                    texts_MemoryCellNames[curMem].Width = memoryStackPanel1.Width / 3;
                    texts_MemoryCellNames[curMem].FontSize = (memoryStackPanel1.Width / 6.5 < memoryStackPanel1.Height / 20) ? memoryStackPanel1.Width / 6.5 : memoryStackPanel1.Height / 20;
                    texts_MemoryCells[curMem].FontSize = (memoryStackPanel1.Width / 6.5 < memoryStackPanel1.Height / 20) ? memoryStackPanel1.Width / 6.5 : memoryStackPanel1.Height / 20;
                }
            }

            gridReg1Wire.Width = ActualWidth * 3 / 28;
            gridReg1Wire.Height = ActualHeight * 73 / 161;
            rect_Reg1Wire_1.Width = ActualWidth * (43 * 3) / (73 * 28);
            rect_Reg1Wire_2.Height = ActualHeight * (73 * 5) / (161 * 9);
            rect_Reg1Wire_3.Width = ActualWidth * (30 * 3) / (73 * 28);

            if (NumRegisters > 1)
            {
                gridReg2Wire.Width = gridReg1Wire.Width;
                gridReg2Wire.Height = gridReg1Wire.Height;
                rect_Reg2Wire_1.Width = ActualWidth * (39 * 3) / (73 * 28);
                rect_Reg2Wire_2.Height = ActualHeight * 73 / (161 * 3);
                rect_Reg2Wire_3.Width = ActualWidth * (34 * 3) / (73 * 28);
                if (NumRegisters > 2)
                {
                    gridReg3Wire.Width = gridReg2Wire.Width;
                    gridReg3Wire.Height = gridReg2Wire.Height;
                    rect_Reg3Wire_1.Width = ActualWidth * (35 * 3) / (73 * 28);
                    rect_Reg3Wire_2.Height = ActualHeight * 73 / (161 * 9);
                    rect_Reg3Wire_3.Width = ActualWidth * (38 * 3) / (73 * 28);
                    if (NumRegisters > 3)
                    {
                        gridReg4Wire.Width = gridReg3Wire.Width;
                        gridReg4Wire.Height = gridReg3Wire.Height;
                        rect_Reg4Wire_1.Width = rect_Reg3Wire_1.Width + 1.5;
                        rect_Reg4Wire_2.Height = rect_Reg3Wire_2.Height;
                        rect_Reg4Wire_3.Width = rect_Reg3Wire_3.Width;
                        if (NumRegisters > 4)
                        {
                            gridReg5Wire.Width = gridReg4Wire.Width;
                            gridReg5Wire.Height = gridReg4Wire.Height;
                            rect_Reg5Wire_1.Width = rect_Reg2Wire_1.Width + 1.5;
                            rect_Reg5Wire_2.Height = rect_Reg2Wire_2.Height;
                            rect_Reg5Wire_3.Width = rect_Reg2Wire_3.Width;
                            if (NumRegisters == 6)//Limit
                            {
                                gridReg6Wire.Width = gridReg5Wire.Width;
                                gridReg6Wire.Height = gridReg5Wire.Height;
                                rect_Reg6Wire_1.Width = rect_Reg1Wire_1.Width + 1.5;
                                rect_Reg6Wire_2.Height = rect_Reg1Wire_2.Height;
                                rect_Reg6Wire_3.Width = rect_Reg1Wire_3.Width;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
