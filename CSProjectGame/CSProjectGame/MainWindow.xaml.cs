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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CSProjectGame;

namespace CSProjectGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declarations
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
        Grid[] gridsRegWires;
        List<StackPanel> stackpanels_Registers;
        List<TextBlock> texts_RegisterNames;
        List<TextBlock> texts_Registers;
        TextBlock text_AddressBus, text_DataBus, text_ToALU, text_PC, text_PCName, text_CIR, text_CIRName, text_ToRegister;
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

        Dictionary<string, Brush> myBrushes;
        byte[] listQuestsStatus;//0 => to be completed, 1 => completed, 2 => completed and redeemed
        //Quests declared in MainWindow()
        Tuple<string, int>[] lookup_Quests;
        Dictionary<Button, int> IndexOfQuestFromRedeemButton = new Dictionary<Button, int>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Title = "Inside Your Computer";
            if (!Directory.Exists(sGameFilesPath))
                Directory.CreateDirectory(sGameFilesPath);
            lookup_MemorySpec = new int[] { 20, 25, 30, 35, 40, 45, 50 };   //lookup_MemorySpec[memoryspec] will give the number of bytes of memory that the player has
            lookup_ClockSpeedSpec = new int[] { 8000/*3000*/, 2540, 2080, 1620, 1160, 700, 240 }; //lookup_ClockSpeedSpec[clockspeedspec] will give the number of milliseconds to take per operation

            shapes_ProcessorParts = new List<Shape>();
            stackpanels_Registers = new List<StackPanel>();
            texts_Registers = new List<TextBlock>();
            texts_RegisterNames = new List<TextBlock>();
            texts_TabNames = new List<TextBox>();
            texts_Tabs = new List<TextBox>();

            SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged_ResizeElements);
            NumRegisters = 6;//DEBUG TO SHOW WIRES WHEN MAKING, THASAL

            gridsRegWires = new Grid[] { gridReg1Wire, gridReg2Wire, gridReg3Wire, gridReg4Wire, gridReg5Wire, gridReg6Wire };

            myUniversalRand = new Random(DateTime.Today.Millisecond);
            myBrushes = new Dictionary<string, Brush>();

            //Contains all quests in order of difficulty and reward, can be referenced using listQuests[QuestNumber][0]. Item1 contains the 'message' or 'challenge' in English. Item2 contains the number of 'portions' that are attained upon completion
            lookup_Quests = new Tuple<string, int>[] { new Tuple<string, int>("Store the numbers 1 to 5 in memory locations 10 to 14", 100), new Tuple<string, int>("Challenge 2", 100), new Tuple<string, int>("Challenge 3", 150), new Tuple<string, int>("Challenge 4", 200), new Tuple<string, int>("Challenge 5", 200), new Tuple<string, int>("Challenge 6", 250), new Tuple<string, int>("Challenge 7", 300), new Tuple<string, int>("Challenge 8", 350), new Tuple<string, int>("Challenge 9", 400), new Tuple<string, int>("Challenge 10", 450) };
            listQuestsStatus = new byte[lookup_Quests.Length];

            
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
            tb.Text = "Your computer is here! You can use this side panel to go through the runtime information and notifications that will be displayed here, or click on the button below to toggle to and from your code...\nThe play button will run the code loaded in memory\nThe white bar the you see on the right hand side can be used to open quests and to save your progress,\nremember to always save!\n\npress any to continue...";
            button_ToggleCode.Visibility = Visibility.Visible;
            button_PlayRun.Visibility = Visibility.Visible;
            for (int i = 0; i < lookup_Quests.Length; i++)
                listQuestsStatus[i] = 0;
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
            AddTab.Click += DockPanelButton_Click_AddNewTab;
            button_DeleteTab.Click -= DockButton_Click_DeleteTab_Tutorial;
            button_DeleteTab.Click += DockButton_Click_DeleteTab;
            button_CodeManual.Click -= Button_CodeManual_Click_Tutorial_Open;
            button_CodeManual.Click += DockButton_Click_CodeManual_Open;
            button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory_Tab1;
            (myDockPanel.Children[0] as Button).Click -= MainTab_Click_Tutorial;
            (myDockPanel.Children[0] as Button).Click += MainTab_Click;

            //Prepare some sample code for the user
            texts_TabNames[0].Text = "Sample Code";
            texts_Tabs[0].Text = "Some sample code to store the sum of two values in memory to location 2:\n\nLDR 0, 0\r\nLDR 1, 1\r\nADD 0, 0, 1\r\nSTR 0, 2";
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
                if (binaryReader.BaseStream.Length == 0 || (b = binaryReader.ReadByte()) == (byte)'x')
                {
                    ingraph_InitialiseTabs_Tutorial();
                    ingraph_FirstTime_00();
                }
                else
                {
                    //Has been played before, tutorial has been watched
                    ingraph_InitialiseFromFile(binaryReader);
                    ingraph_SetEventHandlers();
                    GraphicsForMotherBoard();
                    curTab = 0;
                    CodeTab_Click(myDockPanel.Children[1] as Button, new RoutedEventArgs());
                }

            }

        }
        
        private void ingraph_SetEventHandlers()
        {
            for (int i = 1; i < myDockPanel.Children.Count - 1; i++)
            {
                (myDockPanel.Children[i] as Button).Click += CodeTab_Click;
                texts_TabNames[i - 1].TextChanged += new TextChangedEventHandler(text_TabName_TextChanged);
            }
            Button AddTab = (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button);
            AddTab.Click -= DockButton_Click_AddNewTab_Tutorial;
            AddTab.Click += DockPanelButton_Click_AddNewTab;
            //button_Quests.Click += button_Quests_Click_Open;
            //button_SaveProgress.Click += button_SaveProgress_Click;
            button_DeleteTab.Click -= DockButton_Click_DeleteTab_Tutorial;
            button_DeleteTab.Click += DockButton_Click_DeleteTab;
            button_CodeManual.Click -= Button_CodeManual_Click_Tutorial_Open;
            button_CodeManual.Click += DockButton_Click_CodeManual_Open;
            button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory_Tab1;
            (myDockPanel.Children[0] as Button).Click -= MainTab_Click_Tutorial;
            (myDockPanel.Children[0] as Button).Click += MainTab_Click;
        }

        private void ingraph_InitialiseTabs_Tutorial()
        {
            myDockPanel.Visibility = Visibility.Visible;
            myDockPanel.Children.Add(new Button() { FontSize = 14F, Style = (Style)Resources["ButtonStyle4"], Width = ActualWidth / 14 });    //Main button
            AddNewTab("P1");   //Tab 1
            myDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });   //+ tab
            (myDockPanel.Children[myDockPanel.Children.Count - 1] as Button).Click += new RoutedEventHandler(DockPanelButton_Click_AddNewTab);   //+ tab event handler
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
            myDockPanel.Children.Add(new Button() { FontSize = 14F, Style = (Style)Resources["ButtonStyle4"], Width = ActualWidth / 14 });    //Main button
            for (int i = 0; i < numtabs; i++)
            {
                texts_TabNames.Add(new TextBox { FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.White, Background = Brushes.DarkMagenta, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Text = tnames[i], TextWrapping = TextWrapping.Wrap, AcceptsReturn = false, Visibility = Visibility.Collapsed });
                texts_TabNames[i].TextChanged += text_TabName_TextChanged;
                myStackPanel.Children.Add(texts_TabNames[i]);
                texts_Tabs.Add(new TextBox { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Text = texts[i], TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Visibility = Visibility.Collapsed });
                myStackPanel.Children.Add(texts_Tabs[i]);
                AddNewTab((i + 1).ToString());
            }
            myDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });
            NumRegisters = KSFileManagement.NumRegFromFile;
            MemorySpec = KSFileManagement.MemSpecFromFile;
            texts_MemoryCells = new TextBlock[lookup_MemorySpec[MemorySpec]];
            texts_MemoryCellNames = new TextBlock[lookup_MemorySpec[MemorySpec]];
            charars_Commands = new char[lookup_MemorySpec[MemorySpec]][];
            for (int i = 0; i < lookup_MemorySpec[MemorySpec]; i++)
            {
                texts_MemoryCells[i] = new TextBlock() { Text = "000000", Background = Brushes.Transparent, FontSize = (memoryStackPanel1.Width / 6.5 < memoryStackPanel1.Height / 20) ? memoryStackPanel1.Width / 6.5 : memoryStackPanel1.Height / 20 };
                texts_MemoryCellNames[i] = new TextBlock() { Text = i.ToString() + ":", FontSize = (memoryStackPanel1.Width / 6.5 < memoryStackPanel1.Height / 20) ? memoryStackPanel1.Width / 6.5 : memoryStackPanel1.Height / 20 };
                charars_Commands[i] = new char[8];
            }
            ALUSpec = KSFileManagement.ALUSpecFromFile;
            ClockSpeedSpec = KSFileManagement.ClockSpeedSpecFromFile;
            texts_TabNames[0].Visibility = Visibility.Visible;
            texts_Tabs[0].Visibility = Visibility.Visible;
            myStackPanel.Visibility = Visibility.Visible;
            toolsDockPanel.Visibility = Visibility.Visible;
            toolsDockPanel.Children.ShowAllElements();
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
            add.Click -= DockPanelButton_Click_AddNewTab;
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
            DockPanelButton_Click_AddNewTab(sender, e);
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
            if (texts_Tabs[0].Text == "Some sample code to store the sum of two values in memory to location 2:\n\nLDR 0, 0\r\nLDR 1, 1\r\nADD 0, 0, 1\r\nSTR 0, 2")
            {
                texts_Tabs[0].Text = "LDR 0, 0\r\nLDR 1, 1\r\nADD 0, 0, 1\r\nSTR 0, 2";
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
            texts_Tabs[0].Text = "LDR 0, 0\r\nLDR 1, 1\r\nADD 0, 0, 1\r\nSTR 0, 2";
            texts_Tabs[0].TextChanged -= CodeTab_TextChanged_TutorialTemporary;
            button_LoadIntoMem.Click -= DockButton_Click_LoadIntoMemory_Tab1;
            button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory;
        }
        #endregion

        #region Tabs Functions

        private void AddNewTab(string TabContent)
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
                    gridsRegWires[i].Visibility = Visibility.Collapsed;
                gridToALU.Visibility = Visibility.Collapsed;
                text_ALU.Visibility = Visibility.Collapsed;
                gridProcToMem.Visibility = Visibility.Collapsed;
                button_QstSave.Visibility = Visibility.Collapsed;
                button_Quests.Visibility = Visibility.Collapsed;
                button_SaveProgress.Visibility = Visibility.Collapsed;
            }
            curTab = myDockPanel.Children.IndexOf(sender as Button);
            myStackPanel.Children.CollapseElements();
            texts_TabNames[curTab - 1].Visibility = Visibility.Visible;
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
            for (int i = 1; i < myDockPanel.Children.Count; i++)
            {
                (myDockPanel.Children[i] as Button).Background = Brushes.White;
            }
            (myDockPanel.Children[curTab] as Button).Background = Brushes.SteelBlue;
        }

        private void DockPanelButton_Click_AddNewTab(object sender, RoutedEventArgs e)
        {
            if (curTab == 0)
                CodeTab_Click(myDockPanel.Children[1], e);  //If the user is currently on the main tab, change to show tab 1 so that all main tab elements disappear. Putting this function here simplifies such that you don't have to worry which elements to clear, as this function will already clear everything and show tab 1's elements
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
            TextBox newTabName;
            texts_TabNames.Add(newTabName = new TextBox() { Text = "Project " + curTab, FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.White, Background = Brushes.DarkMagenta, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap, AcceptsReturn = false });
            newTabName.TextChanged += text_TabName_TextChanged;
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

        #region Main Tab
        private void MainTab_Click(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.CollapseElements();
            myStackPanel.Visibility = Visibility.Collapsed;
            toolsDockPanel.Visibility = Visibility.Collapsed;
            GraphicsForMotherBoard();
            curTab = 0;

            //Debugging animations for text_ToRegister
            //text_ToRegister.Visibility = Visibility.Visible;
            //gridsRegWires[0].Children.Add(text_ToRegister);
            //Grid.SetRow(text_ToRegister, 1);
            //Grid.SetColumn(text_ToRegister, 0);
            //Grid.SetRowSpan(text_ToRegister, 2);
            //Grid.SetColumnSpan(text_ToRegister, 2);
            //text_ToRegister.Margin = new Thickness(gridsRegWires[0].ColumnDefinitions[0].Width.Value + gridsRegWires[0].ColumnDefinitions[1].Width.Value - text_ToRegister.Width, gridsRegWires[0].RowDefinitions[1].Height.Value, text_ToRegister.Width, gridsRegWires[0].RowDefinitions[2].Height.Value - text_ToRegister.Height);
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

                text_AddressBus = new TextBlock(); text_DataBus = new TextBlock(); text_ToALU = new TextBlock(); text_ToRegister = new TextBlock();
                text_AddressBus.Width = text_ToALU.Width = text_ToRegister.Width = 25;
                text_DataBus.Width = 50;
                text_AddressBus.Height = text_DataBus.Height = text_ToALU.Height = text_ToRegister.Height = 15;
                text_AddressBus.FontSize = text_DataBus.FontSize = text_ToALU.FontSize = text_ToRegister.FontSize = 12;
                text_AddressBus.Background = text_DataBus.Background = text_ToALU.Background = text_ToRegister.Background = Brushes.White;
                //Register their names for storyboard animations
                RegisterName("text_AddressBus", text_AddressBus);
                RegisterName("text_DataBus", text_DataBus);
                RegisterName("text_ToALU", text_ToALU);
                RegisterName("text_ToRegister", text_ToRegister);
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
                gridsRegWires[i].Visibility = Visibility.Visible;
            gridToALU.Visibility = Visibility.Visible;
            text_ALU.Visibility = Visibility.Visible;
            gridProcToMem.Visibility = Visibility.Visible;
            button_QstSave.Visibility = Visibility.Visible;
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
            if (runTab == 0)
            {
                NoProgramLoaded_CommunicateToUser();
                return;
            }
            TextBox tb;
            runtimeStackPanel.Children.Add(tb = new TextBox() { Text = texts_Tabs[runTab - 1].Text, FontFamily = new FontFamily("Courier New"), Foreground = Brushes.LightGreen, Visibility = Visibility.Visible, Background = Brushes.Black, AcceptsReturn = true });
            tb.TextChanged += text_ToggleCode_TextChanged;
            sTempStoreRuntimeInfo = (runtimeStackPanel.Children[0] as TextBlock).Text;
            runtimeStackPanel.Children[0].Visibility = Visibility.Collapsed;
            IsCodeChangedRuntime = false;
            button_ToggleCode.Click -= button_ToggleCode_Click_Open;
            button_ToggleCode.Click += button_ToggleCode_Click_Close;
        }

        private void NoProgramLoaded_CommunicateToUser()
        {
            (runtimeStackPanel.Children[0] as TextBlock).Foreground = Brushes.Red;
            DispatcherTimer dt = new DispatcherTimer(DispatcherPriority.Normal);
            dt.Interval = TimeSpan.FromMilliseconds(1500);
            dt.Tick += RuntimeStackpanelTimer_Tick_SetForegroundBack;
            dt.Start();
        }

        private void RuntimeStackpanelTimer_Tick_SetForegroundBack(object sender, EventArgs e)
        {
            (runtimeStackPanel.Children[0] as TextBlock).Foreground = Brushes.LightGreen;
            (sender as DispatcherTimer).Stop();
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

        private void button_QstSave_Click_Open(object sender, RoutedEventArgs e)
        {
            DoubleAnimation opacityfadeForRuntimeStackpanel = new DoubleAnimation(0.3, TimeSpan.FromMilliseconds(200));
            runtimeStackPanel.BeginAnimation(OpacityProperty, opacityfadeForRuntimeStackpanel);

            button_Quests.Opacity = button_SaveProgress.Opacity = 0;
            button_Quests.Visibility = button_SaveProgress.Visibility = Visibility.Visible;
            DoubleAnimation opacityappearForButtons = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
            button_Quests.BeginAnimation(OpacityProperty, opacityappearForButtons);
            button_SaveProgress.BeginAnimation(OpacityProperty, opacityappearForButtons);

            button_ToggleCode.Click += button_QstSave_Click_Close;
            button_PlayRun.Click += button_QstSave_Click_Close;
            button_QstSave.Click -= button_QstSave_Click_Open;
            button_QstSave.Click += button_QstSave_Click_Close;
        }

        private void button_QstSave_Click_Close(object sender, RoutedEventArgs e)
        {
            DoubleAnimation opacityfadeForButtons = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            button_Quests.BeginAnimation(OpacityProperty, opacityfadeForButtons);
            button_SaveProgress.BeginAnimation(OpacityProperty, opacityfadeForButtons);

            DoubleAnimation opacityappearForRuntimeStackpanel = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
            runtimeStackPanel.BeginAnimation(OpacityProperty, opacityappearForRuntimeStackpanel);
            button_Quests.Visibility = button_SaveProgress.Visibility = Visibility.Collapsed;

            button_ToggleCode.Click -= button_QstSave_Click_Close;
            button_PlayRun.Click -= button_QstSave_Click_Close;
            button_QstSave.Click -= button_QstSave_Click_Close;
            button_QstSave.Click += button_QstSave_Click_Open;
        }

        private void button_Quests_Click_Open(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < shapes_ProcessorParts.Count; i++)
                shapes_ProcessorParts[i].Visibility = Visibility.Collapsed;
            registersStackPanel.Visibility = Visibility.Collapsed;
            rect_MotherBoardBackGround.Visibility = Visibility.Collapsed;
            processorStackPanel.Visibility = Visibility.Collapsed;
            runtimeStackPanel.Visibility = Visibility.Collapsed;
            runtimeDockPanel.Visibility = Visibility.Collapsed;
            runtimestackpanelBorder.Visibility = Visibility.Collapsed;
            memoryDockPanel.Visibility = Visibility.Collapsed;
            myDockPanel.Visibility = Visibility.Collapsed;
            for (int i = 0; i < NumRegisters; i++)
                gridsRegWires[i].Visibility = Visibility.Collapsed;
            gridToALU.Visibility = Visibility.Collapsed;
            text_ALU.Visibility = Visibility.Collapsed;
            gridProcToMem.Visibility = Visibility.Collapsed;
            button_QstSave.Visibility = Visibility.Collapsed;
            button_SaveProgress.Visibility = Visibility.Collapsed;

            button_Quests.Content = "Back";

            //Opening quests page
            if (!myBrushes.ContainsKey("myGrid.DefaultBackground"))
                myBrushes.Add("myGrid.DefaultBackground", myGrid.Background);
            if (!myBrushes.ContainsKey("myStackPanel.DefaultBackground"))
                myBrushes.Add("myStackPanel.DefaultBackground", myStackPanel.Background);
            myStackPanel.Background = new SolidColorBrush(Color.FromArgb(255, 170, 115, 30));
            myStackPanel.Visibility = Visibility.Visible;
            Color backcol1 = Color.FromArgb(255, 148, 95, 15);
            Color backcol2 = Color.FromArgb(255, 126, 65, 2);
            myGrid.Background = new LinearGradientBrush(backcol1, backcol2, 90);
            //Display quests
            StackPanel stackpanelQuestspanel = new StackPanel() { Visibility = Visibility.Visible };
            myStackPanel.Children.Add(stackpanelQuestspanel);
            for (int i = 0; i < listQuestsStatus.Length; i++)
            {
                if (listQuestsStatus[i] == 0)//Quests not yet completed, textblock is sufficient
                    stackpanelQuestspanel.Children.Add(new TextBlock() { Text = lookup_Quests[i].Item1, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Lucida Calligraphy") });
                else if (listQuestsStatus[i] == 1)//Dockpanel required, with button to redeem reward
                {
                    DockPanel ToAdd = new DockPanel() { Width = stackpanelQuestspanel.ActualWidth, Height = 25 };
                    ToAdd.Children.Add(new TextBlock() { Text = lookup_Quests[i].Item1, Width = ToAdd.ActualWidth / 2, Height = ToAdd.Height, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Lucida Calligraphy"), Visibility = Visibility.Visible });
                    Button button_Redeem = new Button() { Content = "Redeem " + lookup_Quests[i].Item2 + " portions", Width = ToAdd.ActualWidth / 2, Height = ToAdd.ActualHeight, Background = new LinearGradientBrush(Color.FromArgb(255, 0, 143, 143), Color.FromArgb(255, 0, 52, 143), 90), Visibility = Visibility.Visible };
                    button_Redeem.Click += Button_Redeem_Click;
                    ToAdd.Children.Add(button_Redeem);
                    stackpanelQuestspanel.Children.Add(ToAdd);
                }
                else
                {
                    DockPanel ToAdd = new DockPanel() { Width = stackpanelQuestspanel.ActualWidth, Height = 25 };
                    ToAdd.Children.Add(new TextBlock() { Text = lookup_Quests[i].Item1, Width = ToAdd.ActualWidth / 2, Height = ToAdd.Height, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Lucida Calligraphy"), Visibility = Visibility.Visible });
                    TextBlock text_Redeemed = new TextBlock() { Text = "Redeemed " + lookup_Quests[i].Item2 + " portions!", Width = ToAdd.ActualWidth / 2, Height = ToAdd.ActualHeight, Background = new LinearGradientBrush(Color.FromArgb(255, 0, 200, 143), Color.FromArgb(255, 0, 162, 143), 90), Visibility = Visibility.Visible };
                    ToAdd.Children.Add(text_Redeemed);
                    stackpanelQuestspanel.Children.Add(ToAdd);
                }
            }
            button_Quests.Click -= button_Quests_Click_Open;
            button_Quests.Click += button_Quests_Click_Close;
        }

        /// <summary>
        /// MUST BE TESTED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Redeem_Click(object sender, RoutedEventArgs e)
        {
            DockPanel Parent = (sender as Button).Parent as DockPanel;
            int index = (myStackPanel.Children[myStackPanel.Children.Count - 1] as StackPanel).Children.IndexOf(Parent);
            listQuestsStatus[index] = 2;
            Parent.Children.Remove(sender as Button);
            Parent.Children.Add(new TextBlock() { Text = "Redeemed " + lookup_Quests[index].Item2 + " portions!", Width = Parent.ActualWidth / 2, Height = Parent.ActualHeight, Background = new LinearGradientBrush(Color.FromArgb(255, 0, 200, 143), Color.FromArgb(255, 0, 162, 143), 90), Visibility = Visibility.Visible });
        }

        private void button_Quests_Click_Close(object sender, RoutedEventArgs e)
        {
            //CLOSE QUESTS
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            myStackPanel.Background = myBrushes["myStackPanel.DefaultBackground"];
            myStackPanel.Visibility = Visibility.Collapsed;
            myGrid.Background = myBrushes["myGrid.DefaultBackground"];

            button_Quests.Content = "Quests";

            for (int i = 0; i < shapes_ProcessorParts.Count; i++)
                shapes_ProcessorParts[i].Visibility = Visibility.Visible;
            registersStackPanel.Visibility = Visibility.Visible;
            rect_MotherBoardBackGround.Visibility = Visibility.Visible;
            processorStackPanel.Visibility = Visibility.Visible;
            runtimeStackPanel.Visibility = Visibility.Visible;
            runtimeDockPanel.Visibility = Visibility.Visible;
            runtimestackpanelBorder.Visibility = Visibility.Visible;
            memoryDockPanel.Visibility = Visibility.Visible;
            for (int i = 0; i < NumRegisters; i++)
                gridsRegWires[i].Visibility = Visibility.Visible;
            gridToALU.Visibility = Visibility.Visible;
            text_ALU.Visibility = Visibility.Visible;
            gridProcToMem.Visibility = Visibility.Visible;
            button_QstSave.Visibility = Visibility.Visible;
            button_SaveProgress.Visibility = Visibility.Visible;
            myDockPanel.Visibility = Visibility.Visible;

            button_Quests.Click -= button_Quests_Click_Close;
            button_Quests.Click += button_Quests_Click_Open;
        }

        private void button_SaveProgress_Click(object sender, RoutedEventArgs e)
        {
            BinaryWriter binaryWrite = new BinaryWriter(new FileStream(System.IO.Path.Combine(sGameFilesPath, sAccountFileName), FileMode.Truncate), Encoding.UTF8);
            string[][] TabInfo = new string[2][];//[0] - tab names, [1] - tab texts
            TabInfo[0] = new string[texts_TabNames.Count]; TabInfo[1] = new string[texts_Tabs.Count];
            for (int i = 0; i < texts_Tabs.Count; i++)
            {
                TabInfo[0][i] = texts_TabNames[i].Text;
                TabInfo[1][i] = texts_Tabs[i].Text;
            }
            KSFileManagement.SaveProgress(binaryWrite, texts_Tabs.Count, TabInfo[0], TabInfo[1], NumRegisters, ALUSpec, ClockSpeedSpec, MemorySpec);
        }

        #region PlayRun and Animations
        private void button_PlayRun_Click(object sender, RoutedEventArgs e)
        {
            if (runTab == 0)
            {
                NoProgramLoaded_CommunicateToUser();
                return;
            }
            Fetch();
        }

        #region Fetch
        private void Fetch()
        {
            //sbPCIncr will consist of two animations - PC value to ALU, incremented value back to processor.
            Storyboard sbPCIncr = new Storyboard();
            #region sbPCIncr
            #region Initialise text_ToALU
            text_ToALU.Text = int.Parse(text_PC.Text).ToString();
            gridToALU.Children.Add(text_ToALU);
            Grid.SetColumn(text_ToALU, 0);
            Grid.SetColumnSpan(text_ToALU, 2);
            text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            text_ToALU.Visibility = Visibility.Visible;
            #endregion
            //text_ToALU is in the correct place, begin making animation instances

            #region Create tanimationToALU
            ThicknessAnimation tanimationToALU = new ThicknessAnimation();
            tanimationToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            tanimationToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
            tanimationToALU.Duration = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 6);
            tanimationToALU.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimationToALU, "text_ToALU");
            Storyboard.SetTargetProperty(tanimationToALU, new PropertyPath(MarginProperty));
            #endregion
            sbPCIncr.Children.Add(tanimationToALU);

            //Make a dispatchertimer that will increment the textblock's value when it reaches the ALU
            DispatcherTimer TimerIncr = new DispatcherTimer() { Interval = tanimationToALU.Duration.TimeSpan };
            TimerIncr.Tick += TimerIncr_Tick;

            #region Create tanimationFromALU
            ThicknessAnimation tanimationFromALU = new ThicknessAnimation();
            tanimationFromALU.To = new Thickness(text_ToALU.Margin.Left, 0, text_ToALU.Margin.Right, gridToALU.Height - text_ToALU.Height);
            tanimationFromALU.Duration = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 6);
            tanimationFromALU.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimationFromALU, "text_ToALU");
            Storyboard.SetTargetProperty(tanimationFromALU, new PropertyPath(MarginProperty));
            tanimationFromALU.BeginTime = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 6);
            #endregion
            sbPCIncr.Children.Add(tanimationFromALU);

            sbPCIncr.Completed += delegate (object sbPCIncr_sender, EventArgs sbPCIncr_e)
            {
                text_PC.Text = new string(KSConvert.IntTo2DigCharArray(int.Parse(text_ToALU.Text)));
                gridToALU.Children.Remove(text_ToALU);
            };
            #endregion
            //sbPCIncr ready to Begin()

            //sbFetchToCIR will consist of three animations - address to memory, memory value highlighted, data back to processor
            Storyboard sbFetchToCIR = new Storyboard();
            #region sbFetchToCIR

            #region Initialise text_AddressBus
            text_AddressBus.Text = int.Parse(text_PC.Text).ToString();
            gridProcToMem.Children.Add(text_AddressBus);
            Grid.SetRow(text_AddressBus, 1);
            #endregion
            //text_AddressBus in correct initial place, can begin making animation instances, each duration = clockspeed duration / 6

            #region Create tanimationAddressBus
            ThicknessAnimation tanimationAddressBus = new ThicknessAnimation();
            tanimationAddressBus.From = new Thickness(0, 0, gridProcToMem.Width - text_AddressBus.Width, gridProcToMem.RowDefinitions[1].MyHeight() - text_AddressBus.Height);
            tanimationAddressBus.To = new Thickness(gridProcToMem.Width - text_AddressBus.Width, 0, 0, gridProcToMem.RowDefinitions[1].MyHeight() - text_AddressBus.Height);
            tanimationAddressBus.Duration = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 6);
            tanimationAddressBus.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimationAddressBus, "text_AddressBus");
            Storyboard.SetTargetProperty(tanimationAddressBus, new PropertyPath(MarginProperty));
            #endregion
            sbFetchToCIR.Children.Add(tanimationAddressBus);

            #region Create EmphasisInstruction
            DispatcherTimer EmphasisInstruction = new DispatcherTimer();
            EmphasisInstruction.Interval = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 6 + 10);//delay for variables to work everytime
            EmphasisInstruction.Tick += EmphasisInstruction_Tick_1;
            #endregion
            //Ready for EmphasisData to be started as soon as sbFetchToCIR is begun

            #region Initialise text_DataBus
            int InstNumber = int.Parse(text_PC.Text);
            StackPanel MemPanel = (InstNumber > lookup_MemorySpec[MemorySpec] / 2) ? memoryStackPanel2 : memoryStackPanel1;
            text_DataBus.Text = ((MemPanel.Children[InstNumber % 10] as DockPanel).Children[1] as TextBlock).Text;
            gridProcToMem.Children.Add(text_DataBus);
            Grid.SetRow(text_DataBus, 2);
            text_DataBus.Visibility = Visibility.Collapsed;
            #endregion

            #region Create tanimationDataBus
            ThicknessAnimation tanimationDataBus = new ThicknessAnimation();
            tanimationDataBus.From = new Thickness(gridProcToMem.Width - text_DataBus.Width, 0, 0, gridProcToMem.RowDefinitions[2].ActualHeight - text_DataBus.Height);
            tanimationDataBus.To = new Thickness(0, 0, gridProcToMem.Width - text_DataBus.Width, gridProcToMem.RowDefinitions[2].ActualHeight - text_DataBus.Height);
            tanimationDataBus.Duration = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 6);
            tanimationDataBus.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimationDataBus, "text_DataBus");
            Storyboard.SetTargetProperty(tanimationDataBus, new PropertyPath(MarginProperty));
            tanimationDataBus.BeginTime = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 3);
            #endregion
            sbFetchToCIR.Children.Add(tanimationDataBus);

            sbFetchToCIR.Completed += delegate (object sbFetchToCIR_sender, EventArgs sbFetchToCIR_e)
            {
                text_CIR.Text = text_DataBus.Text;
                gridProcToMem.Children.Remove(text_DataBus);
                (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction fetched";
                Execute();
            };
            #endregion
            //sbFetchToCIR ready to Begin()
            //sbFetchToCIR's Completed event handler calls the function Execute()

            sbPCIncr.Begin(this);
            TimerIncr.Start();
            sbFetchToCIR.Begin(this);
            EmphasisInstruction.Start();
        }

        private void TimerIncr_Tick(object sender, EventArgs e)
        {
            text_ToALU.Text = (int.Parse(text_PC.Text) + 1).ToString();
            (sender as DispatcherTimer).Stop();
        }

        private void EmphasisInstruction_Tick_1(object sender, EventArgs e)
        {
            int InstNumber = int.Parse(text_PC.Text);
            StackPanel MemPanel = (InstNumber > lookup_MemorySpec[MemorySpec] / 2) ? memoryStackPanel2 : memoryStackPanel1;
            ((MemPanel.Children[InstNumber % 10] as DockPanel).Children[1] as TextBlock).Background = Brushes.Red;
            gridProcToMem.Children.Remove(text_AddressBus);
            (sender as DispatcherTimer).Tick -= EmphasisInstruction_Tick_1;
            (sender as DispatcherTimer).Tick += EmphasisInstruction_Tick_2;
        }

        private void EmphasisInstruction_Tick_2(object sender, EventArgs e)
        {
            for (int panelnum = 0; panelnum < 2; panelnum++)
            {
                for (int memnum = 0; memnum < lookup_MemorySpec[MemorySpec] / 2; memnum++)
                    texts_MemoryCells[panelnum * lookup_MemorySpec[MemorySpec] / 2 + memnum].Background = Brushes.Transparent;
            }
            (sender as DispatcherTimer).Tick -= EmphasisInstruction_Tick_1;
            (sender as DispatcherTimer).Stop();
            text_DataBus.Visibility = Visibility.Visible;
        }
        #endregion

        #region Execute
        /* How instructions are stored:
         * 
         * First digit is the index of the instruction (or instruction type) in the below array
         *              { "LDR", "STR", "MOV", "CMP", "B", "ADD", "SUB", "AND", "ORR" }
         *  If the first digit indicates an instruction with index less than 4 (either of the first four instructions), the first parameter is a register number, and the second one is always an <operand>/<mem>, so:
         *      digit 2: register number
         *      digit 3: type of addressing (0 = immediate, 1 = direct, 2 = indirect)
         *      digit 4: memory location/number - 1st digit
         *      digit 5: memory location/number - 2nd digit
         *      
         *  If the first digit indicates a branching instruction (index 4)
         *      digit 2: type of branch (condition) - 
         *          0: branch no matter what
         *          1: branch if last comparison result was: equal
         *          2: branch if last comparison result was: not equal
         *          3: branch if last comparison result was: greater than
         *          4: branch if last comparison result was: lesser than
         *      digit 3: line number - 1st digit
         *      digit 4: line number - 2nd digit
         *          
         *  If the first digit was more than 4, the first and second parameters are both register numbers, then the third parameter is an <operand>
         *      digit 2: target register number
         *      digit 3: parameter register number
         *      digit 4: type of addressing (look at case 4)
         *      digit 5: memory location/number - 1st digit
         *      digit 6: memory location/number - 2nd digit
         * 
         */
        private void Execute()
        {
            switch (text_CIR.Text.ToCharArray()[0])
            {
                case '0':
                    ExecuteInstruction_LDR(text_CIR.Text);
                    break;
                case '1':
                    ExecuteInstruction_STR(text_CIR.Text);
                    break;
                case '2':
                    ExecuteInstruction_MOV(text_CIR.Text);
                    break;
                case '3':
                    ExecuteInstruction_CMP(text_CIR.Text);
                    break;
                case '4':
                    ExecuteInstruction_Branch(text_CIR.Text);
                    break;
                case '5':
                    ExecuteInstruction_ADD(text_CIR.Text);
                    break;
                case '6':
                    ExecuteInstruction_SUB(text_CIR.Text);
                    break;
                case '7':
                    ExecuteInstruction_AND(text_CIR.Text);
                    break;
                case '8':
                    ExecuteInstruction_ORR(text_CIR.Text);
                    break;
                case '9':
                    if (text_CIR.Text == "999999")
                        ExecuteInstruction_HALT();
                    return;
            }
            (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction executing...";
        }

        #region Instruction Executing Functions
        private ThicknessAnimation[] GetAnimationsNumberToRegister(int iRegisterIndex, string sContent, double doubleDurationInMilliseconds)
        {
            ThicknessAnimation[] ToReturn = new ThicknessAnimation[3];
            #region Initialise text_ToRegister
            text_ToRegister.Text = sContent;
            Grid Parentgrid = gridsRegWires[iRegisterIndex];
            Parentgrid.Children.Add(text_ToRegister);
            Grid.SetRow(text_ToRegister, 1);
            Grid.SetColumn(text_ToRegister, 0);
            Grid.SetRowSpan(text_ToRegister, 2);
            Grid.SetColumnSpan(text_ToRegister, 2);
            text_ToRegister.Margin = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (iRegisterIndex < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (iRegisterIndex >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
            #endregion

            #region Create animation1
            ThicknessAnimation animation1 = new ThicknessAnimation();
            animation1.From = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (iRegisterIndex < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (iRegisterIndex >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
            animation1.By = new Thickness(-Parentgrid.ColumnDefinitions[1].MyWidth() + text_ToRegister.Width, 0, Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, 0);
            animation1.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation1.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation1, "text_ToRegister");
            Storyboard.SetTargetProperty(animation1, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[0] = animation1;

            #region Create animation2
            ThicknessAnimation animation2 = new ThicknessAnimation();
            if (iRegisterIndex < 3)
            {
                animation2.By = new Thickness(0, -Parentgrid.RowDefinitions[1].MyHeight(), 0, Parentgrid.RowDefinitions[1].MyHeight());
            }
            else
            {
                animation2.By = new Thickness(0, Parentgrid.RowDefinitions[1].MyHeight(), 0, -Parentgrid.RowDefinitions[1].MyHeight());
            }
            animation2.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation2.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(animation2, "text_ToRegister");
            Storyboard.SetTargetProperty(animation2, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[1] = animation2;

            #region Create animation3
            ThicknessAnimation animation3 = new ThicknessAnimation();
            animation3.By = new Thickness(-Parentgrid.ColumnDefinitions[0].MyWidth(), 0, Parentgrid.ColumnDefinitions[0].MyWidth(), 0);
            animation3.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation3.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation3, "text_ToRegister");
            Storyboard.SetTargetProperty(animation3, new PropertyPath(MarginProperty));
            animation3.Completed += delegate (object sender, EventArgs e)
            {
                if (iRegisterIndex > NumRegisters - 1)
                    ;//messagebox; message: Your computer chip does not have a (iRegisterIndex)th register. Program halting...
                else
                    texts_Registers[iRegisterIndex].Text = new string(KSConvert.DecimalToBinaryForRegisters(int.Parse(text_ToRegister.Text)));
                text_ToRegister.Margin = new Thickness(0);
                (text_ToRegister.Parent as Grid).Children.Remove(text_ToRegister);
            };
            #endregion
            ToReturn[2] = animation3;

            return ToReturn;
        }
        private ThicknessAnimation[] GetAnimationsNumberFromRegister(int iRegisterIndex, double doubleDurationInMilliseconds)
        {
            ThicknessAnimation[] ToReturn = new ThicknessAnimation[3];
            #region Initialise text_ToRegister
            text_ToRegister.Text = KSConvert.BinaryToDecimal(texts_Registers[iRegisterIndex].Text.ToCharArray()).ToString();
            Grid Parentgrid = gridsRegWires[iRegisterIndex];
            Parentgrid.Children.Add(text_ToRegister);
            Grid.SetRow(text_ToRegister, 1);
            Grid.SetColumn(text_ToRegister, 0);
            Grid.SetRowSpan(text_ToRegister, 2);
            Grid.SetColumnSpan(text_ToRegister, 2);
            text_ToRegister.Margin = new Thickness(0, (iRegisterIndex < 3) ? 0 : Parentgrid.RowDefinitions[1].MyHeight(), Parentgrid.ColumnDefinitions[0].MyWidth() - text_ToRegister.Width + Parentgrid.ColumnDefinitions[1].MyWidth(), 0);
            text_ToRegister.Visibility = Visibility.Visible;
            #endregion

            #region Create animation1
            ThicknessAnimation animation1 = new ThicknessAnimation();
            animation1.By = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth(), 0, -Parentgrid.ColumnDefinitions[0].MyWidth(), 0);
            animation1.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation1.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation1, "text_ToRegister");
            Storyboard.SetTargetProperty(animation1, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[0] = animation1;

            #region Create animation2
            ThicknessAnimation animation2 = new ThicknessAnimation();
            if (iRegisterIndex < 3)
            {
                animation2.By = new Thickness(0, Parentgrid.RowDefinitions[1].MyHeight(), 0, -Parentgrid.RowDefinitions[1].MyHeight());
            }
            else
            {
                animation2.By = new Thickness(0, -Parentgrid.RowDefinitions[1].MyHeight(), 0, Parentgrid.RowDefinitions[1].MyHeight());
            }
            animation2.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation2.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(animation2, "text_ToRegister");
            Storyboard.SetTargetProperty(animation2, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[1] = animation2;

            #region Create animation3
            ThicknessAnimation animation3 = new ThicknessAnimation();
            animation3.By = new Thickness(Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, 0, -Parentgrid.ColumnDefinitions[1].MyWidth() + text_ToRegister.Width, 0);
            animation3.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation3.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation3, "text_ToRegister");
            Storyboard.SetTargetProperty(animation3, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[2] = animation3;

            return ToReturn;
        }

        #region Dispatcher Timer Eventhandlers
        private void dtRevealtext_ToRegister_Tick(object sender, EventArgs e)
        {
            text_ToRegister.Visibility = Visibility.Visible;
            //DEBUG - ((memoryDockPanel.Children[0] as StackPanel).Children[int.Parse(text_PC.Text) + 6] as DockPanel).Background = Brushes.SlateGray;
            (sender as DispatcherTimer).Stop();
        }
        private void dtRemovetext_ToRegister_Tick(object sender, EventArgs e)
        {
            (text_ToRegister.Parent as Grid).Children.Remove(text_ToRegister);
            (sender as DispatcherTimer).Stop();
        }
        private void dtRevealtext_AddressBus_Tick(object sender, EventArgs e)
        {
            text_AddressBus.Visibility = Visibility.Visible;
            (sender as DispatcherTimer).Stop();
        }
        private void dtRemovetext_AddressBus_Tick(object sender, EventArgs e)
        {
            (text_AddressBus.Parent as Grid).Children.Remove(text_AddressBus);
            (sender as DispatcherTimer).Stop();
        }
        private void dtRevealtext_DataBus_Tick(object sender, EventArgs e)
        {
            text_DataBus.Visibility = Visibility.Visible;
            (sender as DispatcherTimer).Stop();
        }
        private void dtRemovetext_DataBus_Tick(object sender, EventArgs e)
        {
            (text_DataBus.Parent as Grid).Children.Remove(text_DataBus);
            (sender as DispatcherTimer).Stop();
        }
        private void dtEmphasisData_Tick_1(object sender, EventArgs e)
        {
            int MemLocationNum = int.Parse(text_AddressBus.Text);
            StackPanel MemPanel = (MemLocationNum > lookup_MemorySpec[MemorySpec] / 2) ? memoryStackPanel2 : memoryStackPanel1;
            ((MemPanel.Children[MemLocationNum % 10] as DockPanel).Children[1] as TextBlock).Background = Brushes.LightGoldenrodYellow;
            gridProcToMem.Children.Remove(text_AddressBus);
            (sender as DispatcherTimer).Tick -= dtEmphasisData_Tick_1;
            (sender as DispatcherTimer).Tick += dtEmphasisData_Tick_2;
        }
        private void dtEmphasisData_Tick_2(object sender, EventArgs e)
        {
            for (int panelnum = 0; panelnum < 2; panelnum++)
            {
                for (int memnum = 0; memnum < lookup_MemorySpec[MemorySpec] / 2; memnum++)
                    texts_MemoryCells[panelnum * lookup_MemorySpec[MemorySpec] / 2 + memnum].Background = Brushes.Transparent;
            }
            (sender as DispatcherTimer).Stop();
            text_DataBus.Visibility = Visibility.Visible;
        }
        private void dtRemovetext_ToALU_Tick(object sender, EventArgs e)
        {
            gridToALU.Children.Remove(text_ToALU);
            (sender as DispatcherTimer).Stop();
        }
        private void dtRevealtext_ToALU_Tick(object sender, EventArgs e)
        {
            text_ToALU.Visibility = Visibility.Visible;
            (sender as DispatcherTimer).Stop();
        }
        #endregion

        private void ExecuteInstruction_LDR(string AssemblyLine)
        {
            char[] cArLine = AssemblyLine.ToCharArray();
            int RegisterNumber = cArLine[1] - '0';
            string ContentToLoad = "";
            Storyboard ToPlay = new Storyboard();
            //ToPlay must consist of four animations, address from processor to memory, picking the value in the specified location, data from memory to processor, same data to correct register
            if (cArLine[2] == '0')//immediate addressing
            {
                DispatcherTimer dtToRemovetext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
                #region Initialise text_ToALU
                text_ToALU.Text = new string(new char[] { cArLine[3], cArLine[4] });
                gridToALU.Children.Add(text_ToALU);
                Grid.SetColumn(text_ToALU, 0);
                Grid.SetColumnSpan(text_ToALU, 2);
                text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, 0);
                text_ToALU.Visibility = Visibility.Visible;
                #endregion

                #region Create tanimNumberFromALU
                ThicknessAnimation tanimNumberFromALU = new ThicknessAnimation();
                tanimNumberFromALU.From = new Thickness(text_ToALU.Margin.Left, gridToALU.ActualHeight - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimNumberFromALU.To = new Thickness(text_ToALU.Margin.Left, 0, text_ToALU.Margin.Right, gridToALU.ActualHeight - text_ToALU.Height);
                tanimNumberFromALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
                tanimNumberFromALU.EasingFunction = new CubicEase();
                Storyboard.SetTargetName(tanimNumberFromALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimNumberFromALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimNumberFromALU);
                dtToRemovetext_ToALU.Tick += dtRemovetext_ToALU_Tick;

                //#region Make Number Animate To Register
                //int RegisterNumber = cArLine[1] - '0';
                //string ContentToLoad = new string(new char[] { cArLine[3], cArLine[4] });
                //ToPlay.Completed += delegate (object sender, EventArgs e)
                //{
                //    AnimateNumberToRegister(RegisterNumber, ContentToLoad, lookup_ClockSpeedSpec[ClockSpeedSpec] / 4, delegate (object senderR, EventArgs eR) { Fetch(); });
                //};
                //#endregion

                ContentToLoad = new string(new char[] { cArLine[3], cArLine[4] });

                dtToRemovetext_ToALU.Start();
            }
            else if (cArLine[2] == '1')//direct addressing
            {
                string sAddressFromCIR = new string(new char[] { cArLine[3], cArLine[4] });
                #region Initialise text_AddressBus
                text_AddressBus.Text = new string(new char[] { cArLine[3], cArLine[4] });
                gridProcToMem.Children.Add(text_AddressBus);
                Grid.SetRow(text_AddressBus, 1);
                text_AddressBus.Margin = new Thickness(0, 0, gridProcToMem.Width, gridProcToMem.RowDefinitions[1].MyHeight() - text_ToRegister.Height);
                #endregion

                #region Create tanimAddressToMemory
                ThicknessAnimation tanimAddressToMemory = new ThicknessAnimation();
                tanimAddressToMemory.From = new Thickness(0, 0, gridProcToMem.Width - text_AddressBus.Width, gridProcToMem.RowDefinitions[1].MyHeight() - text_AddressBus.Height);
                tanimAddressToMemory.To = new Thickness(gridProcToMem.Width - text_AddressBus.Width, 0, 0, gridProcToMem.RowDefinitions[1].MyHeight() - text_AddressBus.Height);
                tanimAddressToMemory.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
                tanimAddressToMemory.EasingFunction = new CubicEase();
                Storyboard.SetTargetName(tanimAddressToMemory, "text_AddressBus");
                Storyboard.SetTargetProperty(tanimAddressToMemory, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimAddressToMemory);

                #region Create dtEmphasisData
                DispatcherTimer dtEmphasisData = new DispatcherTimer();
                dtEmphasisData.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 12 + 10);//delay for variables to work everytime
                dtEmphasisData.Tick += dtEmphasisData_Tick_1;
                #endregion
                //dtEmphasisData can be begun as soon as toplay is begun (at the end of this else if clause)

                #region Initialise text_DataBus
                int InstNumber = (cArLine[3] - '0') * 10 + cArLine[4] - '0';
                StackPanel MemPanel = (InstNumber > lookup_MemorySpec[MemorySpec] / 2) ? memoryStackPanel2 : memoryStackPanel1;
                text_DataBus.Text = ((MemPanel.Children[InstNumber % 10] as DockPanel).Children[1] as TextBlock).Text;
                gridProcToMem.Children.Add(text_DataBus);
                Grid.SetRow(text_DataBus, 2);
                text_DataBus.Visibility = Visibility.Collapsed;
                #endregion

                #region Create tanimDataToProc
                ThicknessAnimation tanimDataToProc = new ThicknessAnimation();
                tanimDataToProc.From = new Thickness(gridProcToMem.Width - text_DataBus.Width, 0, 0, gridProcToMem.RowDefinitions[2].MyHeight() - text_DataBus.Height);
                tanimDataToProc.To = new Thickness(0, 0, gridProcToMem.Width - text_DataBus.Width, gridProcToMem.RowDefinitions[2].MyHeight() - text_DataBus.Height);
                tanimDataToProc.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
                tanimDataToProc.BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 6);
                tanimDataToProc.EasingFunction = new CubicEase();
                Storyboard.SetTargetName(tanimDataToProc, "text_DataBus");
                Storyboard.SetTargetProperty(tanimDataToProc, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimDataToProc);

                #region Remove text_DataBus at desired time using dtRemoveDataBus
                DispatcherTimer dtRemoveDataBus = new DispatcherTimer();
                dtRemoveDataBus.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4 - 10);//delay for variables to work
                dtRemoveDataBus.Tick += dtRemovetext_DataBus_Tick;
                #endregion
                ContentToLoad = text_DataBus.Text;

                dtEmphasisData.Start();
                dtRemoveDataBus.Start();
            }

            #region Get tanimsNumberToRegister
            ThicknessAnimation[] tanimsNumberToRegister = GetAnimationsNumberToRegister(RegisterNumber, ContentToLoad, lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            for (int i = 0; i < tanimsNumberToRegister.Length; i++)
            {
                tanimsNumberToRegister[i].BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4 + i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            }
            #endregion
            text_ToRegister.Visibility = Visibility.Collapsed;
            ToPlay.Children.Add(tanimsNumberToRegister[0]);
            ToPlay.Children.Add(tanimsNumberToRegister[1]);
            ToPlay.Children.Add(tanimsNumberToRegister[2]);

            DispatcherTimer dtRevealtext_ToRegister = new DispatcherTimer();
            dtRevealtext_ToRegister.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            dtRevealtext_ToRegister.Tick += dtRevealtext_ToRegister_Tick;

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction executed...\n\tNext instruction";
                Fetch();
            };
            ToPlay.Begin(this);
            dtRevealtext_ToRegister.Start();
        }
        
        private void ExecuteInstruction_STR(string AssemblyLine)
        {
            char[] cAr = AssemblyLine.ToCharArray();
            int RegisterNumber = cAr[1] - '0';
            int MemoryLocIndex = int.Parse(new string(new char[] { cAr[3], cAr[4] }));
            string ToStore = KSConvert.BinaryToDecimal(texts_Registers[RegisterNumber].Text.ToCharArray()).ToString();
            Storyboard ToPlay = new Storyboard();

            #region Get tanimsFromReg
            ThicknessAnimation[] tanimsFromReg = GetAnimationsNumberFromRegister(RegisterNumber, lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            for (int i = 0; i < tanimsFromReg.Length; i++)
            {
                tanimsFromReg[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            }
            #endregion
            ToPlay.Children.Add(tanimsFromReg[0]);
            ToPlay.Children.Add(tanimsFromReg[1]);
            ToPlay.Children.Add(tanimsFromReg[2]);

            #region Create dtRemovetext_ToRegister
            DispatcherTimer dtRemovetext_ToRegister = new DispatcherTimer();
            dtRemovetext_ToRegister.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            dtRemovetext_ToRegister.Tick += dtRemovetext_ToRegister_Tick;
            #endregion

            #region Initialise text_AddressBus
            text_AddressBus.Text = new string(new char[] { cAr[3], cAr[4] });
            gridProcToMem.Children.Add(text_AddressBus);
            Grid.SetRow(text_AddressBus, 1);
            text_AddressBus.Margin = new Thickness(0, 0, gridProcToMem.Width - text_AddressBus.Width, gridProcToMem.RowDefinitions[1].MyHeight() - text_AddressBus.Height);
            text_AddressBus.Visibility = Visibility.Collapsed;
            #endregion

            #region Create dtRevealtext_AddressBus
            DispatcherTimer dtRevealtext_AddressBus = new DispatcherTimer();
            dtRevealtext_AddressBus.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            dtRevealtext_AddressBus.Tick += dtRevealtext_AddressBus_Tick;
            #endregion

            #region Create tanimAddressTransfer
            ThicknessAnimation tanimAddressTransfer = new ThicknessAnimation();
            tanimAddressTransfer.From = new Thickness(0, 0, gridProcToMem.Width - text_AddressBus.Width, gridProcToMem.RowDefinitions[1].MyHeight() - text_AddressBus.Height);
            tanimAddressTransfer.By = new Thickness(gridProcToMem.Width - text_AddressBus.Width, 0, text_AddressBus.Width - gridProcToMem.Width, 0);
            tanimAddressTransfer.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 8);
            tanimAddressTransfer.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimAddressTransfer, "text_AddressBus");
            Storyboard.SetTargetProperty(tanimAddressTransfer, new PropertyPath(MarginProperty));
            tanimAddressTransfer.BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            #endregion
            ToPlay.Children.Add(tanimAddressTransfer);

            #region Create dtRemovetext_AddressBus
            DispatcherTimer dtRemovetext_AddressBus = new DispatcherTimer();
            dtRemovetext_AddressBus.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 8);
            dtRemovetext_AddressBus.Tick += dtRemovetext_AddressBus_Tick;
            #endregion

            #region Initialize text_DataBus
            text_DataBus.Text = ToStore;
            gridProcToMem.Children.Add(text_DataBus);
            Grid.SetRow(text_DataBus, 2);
            text_DataBus.Visibility = Visibility.Collapsed;
            #endregion

            #region Create dtRevealtext_DataBus
            DispatcherTimer dtRevealtext_DataBus = new DispatcherTimer();
            dtRevealtext_DataBus.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            dtRevealtext_DataBus.Tick += dtRevealtext_DataBus_Tick;
            #endregion

            #region Create tanimDataTransfer
            ThicknessAnimation tanimDataTransfer = new ThicknessAnimation();
            tanimDataTransfer.From = new Thickness(0, 0, gridProcToMem.Width - text_DataBus.Width, gridProcToMem.RowDefinitions[2].MyHeight() - text_DataBus.Height);
            tanimDataTransfer.By = new Thickness(gridProcToMem.Width - text_DataBus.Width, 0, text_DataBus.Width - gridProcToMem.Width, 0);
            tanimDataTransfer.Duration = tanimAddressTransfer.Duration;
            tanimDataTransfer.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimDataTransfer, "text_DataBus");
            Storyboard.SetTargetProperty(tanimDataTransfer, new PropertyPath(MarginProperty));
            tanimDataTransfer.BeginTime = tanimAddressTransfer.BeginTime;
            #endregion
            ToPlay.Children.Add(tanimDataTransfer);

            #region Create dtRemovetext_DataBus
            DispatcherTimer dtRemovetext_DataBus = new DispatcherTimer();
            dtRemovetext_DataBus.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 8);
            dtRemovetext_DataBus.Tick += dtRemovetext_DataBus_Tick;
            #endregion

            #region Create dtStoreAndEmphasise
            DispatcherTimer dtStoreAndEmphasise = new DispatcherTimer();
            dtStoreAndEmphasise.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 8 - 20);//delay for variables to work
            dtStoreAndEmphasise.Tick += dtStoreAndEmphasise_Tick;
            #endregion

            #region Make filler animation to end ToPlay after information has been stored
            DoubleAnimation filler = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 8), BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 8)};
            Storyboard.SetTargetName(filler, "text_ToRegister");//doesn't matter
            Storyboard.SetTargetProperty(filler, new PropertyPath(OpacityProperty));//doesn't matter
            #endregion
            ToPlay.Children.Add(filler);

            ToPlay.Completed += delegate (object csender, EventArgs c)
            {
                (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction executed...\n\tNext instruction";
                Fetch();
            };
            ToPlay.Begin(this);
            dtRemovetext_ToRegister.Start();
            dtRevealtext_AddressBus.Start();
            dtRemovetext_AddressBus.Start();
            dtRevealtext_DataBus.Start();
            dtRemovetext_DataBus.Start();
            dtStoreAndEmphasise.Start();
        }

        private void dtStoreAndEmphasise_Tick(object sender, EventArgs e)
        {
            int MemLocationNum = int.Parse(text_AddressBus.Text);
            StackPanel MemPanel = (MemLocationNum > lookup_MemorySpec[MemorySpec] / 2) ? memoryStackPanel2 : memoryStackPanel1;
            (((MemPanel.Children[MemLocationNum % 10]) as DockPanel).Children[1] as TextBlock).Text = KSConvert.BinaryToDecimal(text_AddressBus.Text.ToCharArray()).ToString();
            (((MemPanel.Children[MemLocationNum % 10]) as DockPanel).Children[1] as TextBlock).Background = Brushes.Goldenrod;
            DispatcherTimer dtEmphasiseStoredData = new DispatcherTimer();
            dtEmphasiseStoredData.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 8);
            dtEmphasiseStoredData.Tick += dtEmphasisData_Tick_2;
            dtEmphasiseStoredData.Start();
            (sender as DispatcherTimer).Stop();
        }

        private void ExecuteInstruction_MOV(string AssemblyLine)
        {
            #region Initialise text_AddressBus
            text_AddressBus.Text = int.Parse(text_PC.Text).ToString();
            gridProcToMem.Children.Add(text_AddressBus);
            Grid.SetRow(text_AddressBus, 1);
            #endregion
        }

        private void ExecuteInstruction_CMP(string AssemblyLine)
        {
            #region Initialise text_AddressBus
            text_AddressBus.Text = int.Parse(text_PC.Text).ToString();
            gridProcToMem.Children.Add(text_AddressBus);
            Grid.SetRow(text_AddressBus, 1);
            #endregion
        }

        private void ExecuteInstruction_Branch(string AssemblyLine)
        {

        }

        private void ExecuteInstruction_ADD(string AssemblyLine)
        {

        }

        private void ExecuteInstruction_SUB(string AssemblyLine)
        {

        }

        private void ExecuteInstruction_AND(string AssemblyLine)
        {

        }

        private void ExecuteInstruction_ORR(string AssemblyLine)
        {

        }

        private void ExecuteInstruction_HALT()
        {
            (runtimeStackPanel.Children[0] as TextBlock).Text += "\n\n>>Halt instruction decoded, program halted\n\n-----";
            text_PC.Text = "00";
            text_CIR.Text = "000000";
            for (int i = 0; i < NumRegisters; i++)
            {
                texts_Registers[i].Text = "0000 0000";
            }
            return;
        }
        #endregion
        #endregion

        #endregion
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
            myDockPanel.Children[myDockPanel.Children.Count - 1].Visibility = Visibility.Visible;   //make visible the 'add new tab' button
            curTab = (curTab == 1) ? 1 : curTab - 1;
            myStackPanel.Children.CollapseElements();
            texts_TabNames[curTab - 1].Visibility = Visibility.Visible;
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
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

            #region Register Wire Grid Sizing (confusing at first sight)
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
            #endregion

            gridToALU.Width = ActualWidth * 3 / 14;
            gridToALU.Height = rect_ToALUWire.Height = ActualHeight * 51 / 322;
            text_ALU.Width = ActualWidth * 3 / 14;
            text_ALU.Height = ActualHeight * 51 / 322;
            text_ALU.FontSize = (ActualWidth * (3 * 2) / (14 * 11) < ActualHeight * (51 * 2) / (322 * 5)) ? ActualWidth * (3 * 2) / (14 * 11) : ActualHeight * (51 * 2) / (322 * 5);

            gridProcToMem.Width = rect_AddressBusWire.Width = rect_DataBusWire.Width = ActualWidth / 14;
            gridProcToMem.Height = ActualHeight * 102 / 322;


            button_Quests.Width = ActualWidth / 14;
            button_Quests.Height = ActualHeight * 44 / 322;
            button_Quests.FontSize = (111 * ActualHeight / 483 < ActualWidth / 70) ? 111 * ActualHeight / 483 : ActualWidth / 70;

            button_SaveProgress.Width = button_Quests.Width;
            button_SaveProgress.Height = button_Quests.Height;
            button_SaveProgress.FontSize = (22 * ActualHeight / 805 < ActualWidth / 56) ? 22 * ActualHeight / 805 : ActualWidth / 56;

            int button_QstSave_Width = 5;
            button_QstSave.Height = ActualHeight * 44 / 322;
            button_QstSave.Width = button_QstSave_Width;
            button_QstSave.Margin = new Thickness(ActualWidth / 14 - button_QstSave_Width, 0, 0, 0);
        }
        #endregion

        #endregion
    }
}
