using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
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
        string sAccountFileName
        {
            get
            {
                return "acc.bin";
            }
        }

        string sUsername;
        string sPassword = "";

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
        TextBlock text_AddressBus, text_DataBus, text_ToALU, text_CMP, text_PC, text_PCName, text_CIR, text_CIRName;
        TextBlock[] texts_ToRegister;
        string sTempStoreRuntimeInfo;
        bool IsCodeChangedRuntime;

        TextBlock[] texts_MemoryCellNames;
        TextBlock[] texts_MemoryCells;
        char[][] charars_Commands;

        List<TextBox> texts_TabNames;
        List<TextBox> texts_Tabs;
        int curTab = 1;
        int runTab = 0;
        bool InSecondaryMenu = false;
        
        Dictionary<string, Brush> myBrushes;
        byte[] listQuestsStatus;//0 => to be completed, 1 => completed, 2 => completed and redeemed
        //Quests declared in MainWindow()
        Tuple<string, int>[] lookup_Quests;
        Dictionary<Button, int> IndexOfQuestFromRedeemButton = new Dictionary<Button, int>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            secmenuGrid.Margin = new Thickness(-ActualWidth, secmenuGrid.Margin.Top, ActualWidth, secmenuGrid.Margin.Bottom);
            Title = "Inside Your Computer";
            if (!Directory.Exists(sGameFilesPath))
                Directory.CreateDirectory(sGameFilesPath);
            lookup_MemorySpec = new int[] { 20, 25, 30, 35, 40, 45, 50 };   //lookup_MemorySpec[memoryspec] will give the number of bytes of memory that the player has
            lookup_ClockSpeedSpec = new int[] { 3000, 2540, 2080, 1620, 1160, 700, 240 }; //lookup_ClockSpeedSpec[clockspeedspec] will give the number of milliseconds to take per operation
            
            shapes_ProcessorParts = new List<Shape>();
            stackpanels_Registers = new List<StackPanel>();
            texts_Registers = new List<TextBlock>();
            texts_RegisterNames = new List<TextBlock>();
            texts_TabNames = new List<TextBox>();
            texts_Tabs = new List<TextBox>();

            SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged_ResizeElements);

            gridsRegWires = new Grid[] { gridReg1Wire, gridReg2Wire, gridReg3Wire, gridReg4Wire, gridReg5Wire, gridReg6Wire };

            myUniversalRand = new Random(DateTime.Today.Millisecond);
            myBrushes = new Dictionary<string, Brush>();

            //Contains all quests in order of difficulty and reward, can be referenced using listQuests[QuestNumber][0]. Item1 contains the 'message' or 'challenge' in English. Item2 contains the number of 'portions' that are attained upon completion
            lookup_Quests = new Tuple<string, int>[] { new Tuple<string, int>("Store the numbers 1 to 5 in memory locations 10 to 14", 100), new Tuple<string, int>("Challenge 2", 100), new Tuple<string, int>("Challenge 3", 150), new Tuple<string, int>("Challenge 4", 200), new Tuple<string, int>("Challenge 5", 200), new Tuple<string, int>("Challenge 6", 250), new Tuple<string, int>("Challenge 7", 300), new Tuple<string, int>("Challenge 8", 350), new Tuple<string, int>("Challenge 9", 400), new Tuple<string, int>("Challenge 10", 450) };
            listQuestsStatus = new byte[lookup_Quests.Length];

            canvas_LoginDetails_Username.Opacity = 0;
            RegisterName("canvas_LoginDetails_Username", canvas_LoginDetails_Username);
            canvas_LoginDetails_Password.Opacity = 0;
            RegisterName("canvas_LoginDetails_Password", canvas_LoginDetails_Password);
            LoginBoxAnimations(canvas_LoginDetails_Username, "canvas_LoginDetails_Username", canvas_LoginDetails_Username.Margin).Begin(this);
            LoginBoxAnimations(canvas_LoginDetails_Password, "canvas_LoginDetails_Password", canvas_LoginDetails_Password.Margin).Begin(this);
            GoButtonAnimation();
            NewAccountButtonAnimation();
            text_LoginDetails_Username.GotMouseCapture += text_LoginDetails_ClearText;
            text_LoginDetails_Password.GotMouseCapture += text_LoginDetails_ClearText;
            text_LoginDetails_Password.TextChanged += text_LoginDetails_Password_TextChanged;

            RegisterName("secmenuGrid", secmenuGrid);
        }

        #region Start Screen
        private void button_Go_Click(object sender, RoutedEventArgs e)
        {
            text_NoAccountFound.Margin = new Thickness(-ActualWidth, 0, ActualWidth, 0);
            text_NoAccountFound.Visibility = Visibility.Visible;
            sUsername = text_LoginDetails_Username.Text;
            char[] carPasswordEntered = text_LoginDetails_Password.Text.ToCharArray();
            byte[] bPasswordEntered = new byte[carPasswordEntered.Length];
            for (int i = 0; i < bPasswordEntered.Length; i++)
                bPasswordEntered[i] = (byte)carPasswordEntered[i];
            if (File.Exists(System.IO.Path.Combine(sGameFilesPath, sUsername + sAccountFileName)))
            {
                //Get hash of correct password
                BinaryReader bReader = new BinaryReader(new FileStream(System.IO.Path.Combine(sGameFilesPath, sUsername + sAccountFileName), FileMode.Open));
                byte[] HashComputed = new SHA1CryptoServiceProvider().ComputeHash(bPasswordEntered);
                byte[] HashOfCorrectPassword = KSFileManagement.HashOfCorrectPasscode(bReader);
                bool PasswordIsCorrect = true;
                for (int i = 0; i < 20; i++)
                {
                    if (HashComputed[i] != HashOfCorrectPassword[i])
                    {
                        PasswordIsCorrect = false;
                        break;
                    }
                }
                bReader.Close();

                if (PasswordIsCorrect)//Begin normal running
                {
                    text_Title.Visibility = Visibility.Collapsed;
                    text_LoginDetails_FormTitle.Visibility = Visibility.Collapsed;
                    canvas_LoginDetails_Username.Visibility = Visibility.Collapsed;
                    canvas_LoginDetails_Password.Visibility = Visibility.Collapsed;
                    button_Go.Visibility = Visibility.Collapsed;
                    button_NewAccount.Visibility = Visibility.Collapsed;
                    ingraph_Initialise();
                }
                else//Show error message
                {
                    text_NoAccountFound.Text = "Incorrect Password";
                    text_NoAccountFound.TextAlignment = TextAlignment.Center;
                    sbtext_NoAccountFound(7).Begin(this);
                }
            }
            else//Show error message
            {
                text_NoAccountFound.Text = "Sorry, we couldn’t find an account with that username on this computer’s game file folder.";
                text_NoAccountFound.TextAlignment = TextAlignment.Center;
                sbtext_NoAccountFound(0).Begin(this);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="speed">A linear duration decreaser ranging from 1 to 10</param>
        /// <returns></returns>
        private Storyboard sbtext_NoAccountFound(int speed)
        {
            Storyboard ToPlay = new Storyboard();

            int MilliDur = 400 - speed * 35;
            int MilliReadDelay = 2000 - speed * 130;
            #region Create tanimtext_NoAccountFound
            ThicknessAnimation tanimtext_NoAccountFound = new ThicknessAnimation();
            tanimtext_NoAccountFound.From = new Thickness(-1 * ActualWidth, 0, ActualWidth, 0);
            tanimtext_NoAccountFound.To = new Thickness(0.65 * ActualWidth / 14, 17 * ActualHeight / 322, 0.65 * ActualWidth / 14, 8 * ActualHeight / 322);
            tanimtext_NoAccountFound.Duration = TimeSpan.FromMilliseconds(MilliDur);
            tanimtext_NoAccountFound.EasingFunction = new CubicEase();
            tanimtext_NoAccountFound.BeginTime = TimeSpan.FromMilliseconds(0);
            Storyboard.SetTarget(tanimtext_NoAccountFound, text_NoAccountFound);
            Storyboard.SetTargetProperty(tanimtext_NoAccountFound, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimtext_NoAccountFound);

            #region Create tanimtext_NoAccountFound2
            ThicknessAnimation tanimtext_NoAccountFound2 = new ThicknessAnimation();
            tanimtext_NoAccountFound2.To = new Thickness(ActualWidth, 0, -1 * ActualWidth, 0);
            tanimtext_NoAccountFound2.Duration = TimeSpan.FromMilliseconds(MilliDur);
            tanimtext_NoAccountFound2.EasingFunction = new CubicEase();
            tanimtext_NoAccountFound2.BeginTime = TimeSpan.FromMilliseconds(MilliDur + MilliReadDelay);
            Storyboard.SetTarget(tanimtext_NoAccountFound2, text_NoAccountFound);
            Storyboard.SetTargetProperty(tanimtext_NoAccountFound2, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimtext_NoAccountFound2);

            ToPlay.Completed += delegate (object o, EventArgs ea)
            {
                text_NoAccountFound.Margin = new Thickness(-1 * ActualWidth, 0, ActualWidth, 0);
            };

            return ToPlay;
        }

        private void button_Go_NewAccount_Click(object sender, RoutedEventArgs e)
        {
            //Storing password in bytes using unicodes to compute hash
            byte[] bytesPassword = new byte[text_LoginDetails_Password.Text.Length];
            char[] carPassword = text_LoginDetails_Password.Text.ToCharArray();
            for (int i = 0; i < bytesPassword.Length; i++)
                bytesPassword[i] = (byte)carPassword[i];
            //Computing password's hash
            HashAlgorithm Hasher = new SHA1CryptoServiceProvider();
            byte[] Hash = Hasher.ComputeHash(bytesPassword);

            string sNewAccountFilePath = System.IO.Path.Combine(sGameFilesPath, (sUsername = text_LoginDetails_Username.Text) + sAccountFileName);
            if (File.Exists(sNewAccountFilePath))//There is already an account with this username on this computer
            {
                text_NoAccountFound.Text = "This username already exists";
                text_NoAccountFound.TextAlignment = TextAlignment.Center;
                text_NoAccountFound.Visibility = Visibility.Visible;
                sbtext_NoAccountFound(5).Begin(this);
                return;
            }
            BinaryWriter bwrite = new BinaryWriter(new FileStream(sNewAccountFilePath, FileMode.CreateNew));
            for (int curbyte = 0; curbyte < 20; curbyte++)//Write the hash of the password to file
            {
                bwrite.Write(Hash[curbyte]);
            }
            bwrite.Close();
            text_LoginDetails_FormTitle.Visibility = Visibility.Collapsed;
            canvas_LoginDetails_Username.Visibility = Visibility.Collapsed;
            canvas_LoginDetails_Password.Visibility = Visibility.Collapsed;
            button_Go.Visibility = Visibility.Collapsed;
            button_NewAccount.Visibility = Visibility.Collapsed;
            text_Title.Visibility = Visibility.Collapsed;
            ingraph_InitialiseTabs_Tutorial();
            ingraph_FirstTime_00();
        }

        private void text_LoginDetails_ClearText(object sender, MouseEventArgs e)
        {
            (sender as TextBox).Text = "";
            (sender as TextBox).GotMouseCapture -= text_LoginDetails_ClearText;
        }

        Storyboard LoginBoxAnimations(Canvas loginbox, string regname, Thickness marginBeforeAnimation)
        {
            const int iTimespan = 1200;
            const int iStartTime = 900;
            Storyboard ToReturn = new Storyboard();

            #region Create tanimLB
            ThicknessAnimation tanimLB = new ThicknessAnimation();
            tanimLB.From = new Thickness(marginBeforeAnimation.Left, marginBeforeAnimation.Top + this.Height / 16, marginBeforeAnimation.Right, marginBeforeAnimation.Bottom - this.Height / 16);
            tanimLB.To = marginBeforeAnimation;
            tanimLB.EasingFunction = new QuadraticEase();
            tanimLB.Duration = TimeSpan.FromMilliseconds(iTimespan);
            tanimLB.BeginTime = TimeSpan.FromMilliseconds(iStartTime);
            Storyboard.SetTargetName(tanimLB, regname);
            Storyboard.SetTargetProperty(tanimLB, new PropertyPath(MarginProperty));
            #endregion
            ToReturn.Children.Add(tanimLB);

            #region Create danimOpacityLB
            DoubleAnimation danimOpacityLB = new DoubleAnimation();
            danimOpacityLB.From = 0;
            danimOpacityLB.To = 1;
            danimOpacityLB.EasingFunction = new SineEase();
            danimOpacityLB.Duration = TimeSpan.FromMilliseconds(iTimespan);
            danimOpacityLB.BeginTime = TimeSpan.FromMilliseconds(iStartTime);
            Storyboard.SetTargetName(danimOpacityLB, regname);
            Storyboard.SetTargetProperty(danimOpacityLB, new PropertyPath(OpacityProperty));
            #endregion
            ToReturn.Children.Add(danimOpacityLB);

            return ToReturn;
        }

        void GoButtonAnimation()
        {
            button_Go.Opacity = 0;
            DoubleAnimation danim = new DoubleAnimation();
            danim.From = 0;
            danim.To = 1;
            danim.Duration = TimeSpan.FromMilliseconds(2000);
            danim.EasingFunction = new CubicEase();
            Storyboard.SetTarget(danim, button_Go);
            Storyboard.SetTargetProperty(danim, new PropertyPath(OpacityProperty));
            danim.BeginTime = TimeSpan.FromMilliseconds(2300);
            Storyboard s = new Storyboard() { Children = new TimelineCollection() { danim } };
            s.Begin(this);
        }

        void NewAccountButtonAnimation()
        {
            button_NewAccount.Opacity = 0;
            DoubleAnimation danim = new DoubleAnimation();
            danim.From = 0;
            danim.To = 1;
            danim.Duration = TimeSpan.FromMilliseconds(2000);
            danim.EasingFunction = new CubicEase();
            Storyboard.SetTarget(danim, button_NewAccount);
            Storyboard.SetTargetProperty(danim, new PropertyPath(OpacityProperty));
            danim.BeginTime = TimeSpan.FromMilliseconds(2300);
            Storyboard s = new Storyboard() { Children = new TimelineCollection() { danim } };
            s.Begin(this);
        }

        private void button_NewAccount_Click(object sender, RoutedEventArgs e)
        {
            button_NewAccount.Style = (Style)Resources["ButtonStyleBackToLogin"];
            text_LoginDetails_Username.Foreground = text_LoginDetails_Password.Foreground = Brushes.Red;

            button_Go.Click -= button_Go_Click;
            button_Go.Click += button_Go_NewAccount_Click;

            button_NewAccount.Click -= button_NewAccount_Click;
            button_NewAccount.Click += button_NewAccount_BackToLogin_Click;
        }

        private void button_NewAccount_BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            button_NewAccount.Style = (Style)Resources["ButtonStyleNewAccount"];
            text_LoginDetails_Username.Foreground = text_LoginDetails_Password.Foreground = new SolidColorBrush(Color.FromArgb(255, 42, 151, 126));

            button_Go.Click -= button_Go_NewAccount_Click;
            button_Go.Click += button_Go_Click;

            button_NewAccount.Click -= button_NewAccount_BackToLogin_Click;
            button_NewAccount.Click += button_NewAccount_Click;
        }
        
        private void text_LoginDetails_Password_TextChanged(object sender, TextChangedEventArgs e)
        {
            const char HiderChar = '*';
            TextBox tb = sender as TextBox;
            if (tb.Text.Length == sPassword.Length + 1)
            {
                char[] car = tb.Text.ToCharArray();
                char newc = car[car.Length - 1];

                //Validate character entered
                if (!((newc >= 'A' && newc <= 'Z') || (newc >= 'a' && newc <= 'z') || (newc >= '0' && newc <= '9')))//if (Invalid Character)
                {
                    text_NoAccountFound.Text = "Passwords can only consist of letters or numbers";
                    text_NoAccountFound.Visibility = Visibility.Visible;
                    sbtext_NoAccountFound(5).Begin(this);
                }

                sPassword += newc;
                car[car.Length - 1] = HiderChar;
                tb.Text = new string(car);//causes new textchanged eventhandler to be triggered (handled below)
                tb.CaretIndex = tb.Text.Length;
            }
            else if (tb.Text.Length == sPassword.Length - 1)
            {
                char[] passw = sPassword.ToCharArray();
                sPassword = "";
                for (int i = 0; i < passw.Length - 1; i++)
                    sPassword += passw[i];
            }
            else if (tb.Text.Length != sPassword.Length)
            {
                sPassword = tb.Text;
                List<char> asterisks = new List<char>();
                for (int i = 0; i < tb.Text.Length; i++)
                    asterisks.Add('*');
                tb.Text = new string(asterisks.ToArray());
                tb.CaretIndex = tb.Text.Length;
            }
            else
                return;//this occurs when executing 'tb.Text = new string(car);' (line 8 of the function)
        }
        #endregion

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
            tabsDockPanel.Children.CollapseElements();
            toolsDockPanel.Visibility = Visibility.Collapsed;
        }

        private void ingraph_FirstTime_01_Tabs()
        {
            text_Welcome.Text = "Use the tabs above to switch between your computer and your code... 'Main' will show you the computer. Numbered tabs can be used for multiple coding solutions\n\npress any key to continue...";
            tabsDockPanel.Children.ShowAllElements();
            tabsDockPanel.Children[tabsDockPanel.Children.Count - 1].Visibility = Visibility.Collapsed;
            (tabsDockPanel.Children[tabsDockPanel.Children.Count - 2] as Button).Click -= CodeTab_Click;
            KeyDown += new KeyEventHandler(KeyDown_PressAnyToContinue_FirstTime_02_1);
        }

        private void ingraph_FirstTime_02_Coding_01()
        {
            text_Welcome.Text = "Click on the coding tab to continue..";
            (tabsDockPanel.Children[1] as Button).Click += new RoutedEventHandler(Code1_Tutorial_01);
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
            (tabsDockPanel.Children[0] as Button).Click += new RoutedEventHandler(MainTab_Click_Tutorial);
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
            (tabsDockPanel.Children[0] as Button).Click -= Code1_Tutorial_01;
            for (int i = 1; i < tabsDockPanel.Children.Count - 1; i++)
            {
                (tabsDockPanel.Children[i] as Button).Click += CodeTab_Click;
                texts_TabNames[i - 1].TextChanged += new TextChangedEventHandler(text_TabName_TextChanged);
            }
            Button AddTab = (tabsDockPanel.Children[tabsDockPanel.Children.Count - 1] as Button);
            AddTab.Click -= DockButton_Click_AddNewTab_Tutorial;
            AddTab.Click += DockPanelButton_Click_AddNewTab;
            button_DeleteTab.Click -= DockButton_Click_DeleteTab_Tutorial;
            button_DeleteTab.Click += DockButton_Click_DeleteTab;
            button_CodeManual.Click -= Button_CodeManual_Click_Tutorial_Open;
            button_CodeManual.Click += DockButton_Click_CodeManual_Open;
            button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory_Tab1;
            (tabsDockPanel.Children[0] as Button).Click -= MainTab_Click_Tutorial;
            (tabsDockPanel.Children[0] as Button).Click += MainTab_Click;

            //Prepare some sample code for the user
            curTab = 1;
            texts_TabNames[0].Text = "Sample Code";
            curTab = 0;
            texts_Tabs[0].Text = "Some sample code to store the first few natural numbers in memory:\n\nLDR 0, #1\r\nSTR 0, 10\r\nLDR 0, #2\r\nSTR 0, 11\r\nLDR 0, #3\r\nSTR 0, 12\r\nHALT";
            texts_Tabs[0].TextChanged += new TextChangedEventHandler(CodeTab_TextChanged_TutorialTemporary);
            (tabsDockPanel.Children[1] as Button).Content = TabTextFromProjectName(texts_TabNames[0].Text);

            //Save the new account’s base specs into the file
            button_SaveProgress_Click(button_SaveProgress, new RoutedEventArgs());
        }
        #endregion

        private void ingraph_Initialise()
        {
            BinaryReader binaryReader = new BinaryReader(new FileStream(System.IO.Path.Combine(sGameFilesPath, sUsername + sAccountFileName), FileMode.Open));
            binaryReader.BaseStream.Position = 0;
            byte b;
            if (binaryReader.BaseStream.Length == 20 || (b = binaryReader.ReadByte()) == (byte)'x')
            {
                ingraph_InitialiseTabs_Tutorial();
                ingraph_FirstTime_00();
            }
            else
            {
                //Has been played before, tutorial has been watched
                KSFileManagement.RetrieveProgress(binaryReader);
                ingraph_InitialiseFromFile();
                ingraph_SetEventHandlers();
                GraphicsForMotherBoard();
                curTab = 0;
                CodeTab_Click(tabsDockPanel.Children[1] as Button, new RoutedEventArgs());
            }

        }
        
        private void ingraph_SetEventHandlers()
        {
            for (int i = 1; i < tabsDockPanel.Children.Count - 1; i++)
            {
                (tabsDockPanel.Children[i] as Button).Click += CodeTab_Click;
                texts_TabNames[i - 1].TextChanged += new TextChangedEventHandler(text_TabName_TextChanged);
            }
            Button AddTab = (tabsDockPanel.Children[tabsDockPanel.Children.Count - 1] as Button);
            AddTab.Click -= DockButton_Click_AddNewTab_Tutorial;
            AddTab.Click += DockPanelButton_Click_AddNewTab;
            //button_Quests.Click += button_Quests_Click_Open;
            //button_SaveProgress.Click += button_SaveProgress_Click;
            button_DeleteTab.Click -= DockButton_Click_DeleteTab_Tutorial;
            button_DeleteTab.Click += DockButton_Click_DeleteTab;
            button_CodeManual.Click -= Button_CodeManual_Click_Tutorial_Open;
            button_CodeManual.Click += DockButton_Click_CodeManual_Open;
            button_LoadIntoMem.Click += DockButton_Click_LoadIntoMemory_Tab1;
            (tabsDockPanel.Children[0] as Button).Click -= MainTab_Click_Tutorial;
            (tabsDockPanel.Children[0] as Button).Click += MainTab_Click;
        }

        private void ingraph_InitialiseTabs_Tutorial()
        {
            tabsDockPanel.Visibility = Visibility.Visible;
            tabsDockPanel.Children.Add(new Button() { FontSize = 14F, Style = (Style)Resources["ButtonStyleMainTab"], Width = ActualWidth / 14 });    //Main button
            AddNewTab("P1");   //Tab 1
            tabsDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });   //+ tab
            (tabsDockPanel.Children[tabsDockPanel.Children.Count - 1] as Button).Click += new RoutedEventHandler(DockPanelButton_Click_AddNewTab);   //+ tab event handler
            tabsDockPanel.Children.ShowAllElements();
            button_CodeManual.Visibility = Visibility.Collapsed;
            button_DeleteTab.Click += new RoutedEventHandler(DockButton_Click_DeleteTab);
            button_LoadIntoMem.Click += new RoutedEventHandler(DockButton_Click_LoadIntoMemory);
        }

        private void ingraph_InitialiseFromFile()
        {
            //ALWAYS OPEN INTO TAB 1, BECAUSE CURTAB = 1
            myStackPanel.Children.CollapseElements();
            int numtabs = KSFileManagement.NumTabsFromFile;
            string[] tnames = KSFileManagement.TabNamesFromFile;
            string[] texts = KSFileManagement.TabTextsFromFile;
            tabsDockPanel.Children.Add(new Button() { FontSize = 14F, Style = (Style)Resources["ButtonStyleMainTab"], Width = ActualWidth / 14 });    //Main button
            for (int i = 0; i < numtabs; i++)
            {
                texts_TabNames.Add(new TextBox { FontFamily = new FontFamily("HP Simplified"), Foreground = Brushes.White, Background = Brushes.DarkMagenta, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Text = tnames[i], TextWrapping = TextWrapping.Wrap, AcceptsReturn = false, Visibility = Visibility.Collapsed });
                texts_TabNames[i].TextChanged += text_TabName_TextChanged;
                myStackPanel.Children.Add(texts_TabNames[i]);
                texts_Tabs.Add(new TextBox { FontFamily = new FontFamily("Courier New"), Foreground = Brushes.Black, Text = texts[i], TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Visibility = Visibility.Collapsed });
                myStackPanel.Children.Add(texts_Tabs[i]);
                AddNewTab(TabTextFromProjectName(texts_TabNames[i].Text));
            }
            tabsDockPanel.Children.Add(new Button() { Content = "+", Width = 36, FontSize = 16F });
            //DEBUGNumRegisters = KSFileManagement.NumRegFromFile;
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
            Button add = tabsDockPanel.Children[tabsDockPanel.Children.Count - 1] as Button;
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
            tabsDockPanel.Visibility = Visibility.Collapsed;
            (sender as Button).Click -= Button_CodeManual_Click_Tutorial_Open;
            (sender as Button).Click += Button_CodeManual_Click_Tutorial_Close;
        }

        private void Button_CodeManual_Click_Tutorial_Close(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            tabsDockPanel.Visibility = Visibility.Visible;
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
            tabsDockPanel.Children.RemoveAt(curTab);
            (tabsDockPanel.Children[tabsDockPanel.Children.Count - 1] as Button).Visibility = Visibility.Visible;
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
            tabsDockPanel.Children.Add(NewTab);
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
                button_SaveProgress.Visibility = Visibility.Collapsed;
            }
            curTab = tabsDockPanel.Children.IndexOf(sender as Button);
            myStackPanel.Children.CollapseElements();
            texts_TabNames[curTab - 1].Visibility = Visibility.Visible;
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
            for (int i = 1; i < tabsDockPanel.Children.Count; i++)
            {
                (tabsDockPanel.Children[i] as Button).Background = Brushes.White;
            }
            (tabsDockPanel.Children[curTab] as Button).Background = Brushes.SteelBlue;
        }

        private void DockPanelButton_Click_AddNewTab(object sender, RoutedEventArgs e)
        {
            if (curTab == 0)
                CodeTab_Click(tabsDockPanel.Children[1], e);  //If the user is currently on the main tab, change to show tab 1 so that all main tab elements disappear. Putting this function here simplifies such that you don't have to worry which elements to clear, as this function will already clear everything and show tab 1's elements
            int numtabs = tabsDockPanel.Children.Count;
            Button NewTab = new Button() { Width = ActualWidth / 14 };
            NewTab.Content = TabTextFromProjectName("Project " + (curTab = numtabs - 1).ToString());
            NewTab.Click += CodeTab_Click;
            Button AddTab = tabsDockPanel.Children[tabsDockPanel.Children.Count - 1] as Button;
            tabsDockPanel.Children.RemoveAt(tabsDockPanel.Children.Count - 1);
            tabsDockPanel.Children.Add(NewTab);
            tabsDockPanel.Children.Add(AddTab);
            tabsDockPanel.Children.ShowAllElements();
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

        private void text_TabName_TextChanged(object sender, TextChangedEventArgs e)
        {
            (tabsDockPanel.Children[curTab] as Button).Content = TabTextFromProjectName((sender as TextBox).Text);
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

                text_CMP = new TextBlock();
                stackpanel_CMP.Children.Add(text_CMP);
                text_CMP.Width = stackpanel_CMP.Width;
                text_CMP.Height = stackpanel_CMP.Height;
                text_CMP.FontFamily = new FontFamily("HP Simplified Light");
                text_CMP.Foreground = Brushes.Black;

                text_AddressBus = new TextBlock(); text_DataBus = new TextBlock(); text_ToALU = new TextBlock();
                texts_ToRegister = new TextBlock[NumRegisters];
                for (int i = 0; i < NumRegisters; i++)
                {
                    texts_ToRegister[i] = new TextBlock();
                    texts_ToRegister[i].Width = 25;
                    texts_ToRegister[i].Height = 15;
                    texts_ToRegister[i].FontSize = 12;
                    texts_ToRegister[i].Background = Brushes.White;
                    RegisterName("texts_ToRegister" + i, texts_ToRegister[i]);
                }
                text_AddressBus.Width = text_ToALU.Width = 25;
                text_DataBus.Width = 50;
                text_AddressBus.Height = text_DataBus.Height = text_ToALU.Height = 15;
                text_AddressBus.FontSize = text_DataBus.FontSize = text_ToALU.FontSize = 12;
                text_AddressBus.Background = text_DataBus.Background = text_ToALU.Background = Brushes.White;
                //Register their names for storyboard animations
                RegisterName("text_AddressBus", text_AddressBus);
                RegisterName("text_DataBus", text_DataBus);
                RegisterName("text_ToALU", text_ToALU);

                stackpanel_CMP.Width = 0.5 * text_ALU.Width;
                stackpanel_CMP.Height = 0.15 * text_ALU.Height;
                if (text_CMP != null)
                {
                    text_CMP.Width = stackpanel_CMP.Width;
                    text_CMP.Height = stackpanel_CMP.Height;
                    text_CMP.FontSize = Math.Min(text_CMP.Width * 2.3, text_CMP.Height * 0.9);
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
                gridsRegWires[i].Visibility = Visibility.Visible;
            gridToALU.Visibility = Visibility.Visible;
            text_ALU.Visibility = Visibility.Visible;
            gridProcToMem.Visibility = Visibility.Visible;
            button_QstSave.Visibility = Visibility.Visible;
        }

        private void stackpanels_Registers_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            int DecimalEq = KSConvert.BinaryToDecimalForRegisters(texts_Registers[stackpanels_Registers.IndexOf(sender as StackPanel)].Text.ToCharArray());
            stackpanels_Registers[stackpanels_Registers.IndexOf(sender as StackPanel)].ToolTip = "This register currently contains " + DecimalEq + " in decimal";
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
            DispatcherTimer dtSetMargintext_DataBus = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 3 - 25) };//delay to ensure smooth running
            Action<object, EventArgs> SetMargin_Tick = (object sender, EventArgs e) =>
            {
                text_DataBus.Margin = new Thickness(gridProcToMem.Width - text_DataBus.Width, 0, 0, gridProcToMem.RowDefinitions[2].ActualHeight - text_DataBus.Height);
                (sender as DispatcherTimer).Stop();
            };
            dtSetMargintext_DataBus.Tick += new EventHandler(SetMargin_Tick);
            dtSetMargintext_DataBus.Start();
            text_DataBus.Margin = new Thickness(gridProcToMem.Width - text_DataBus.Width, 0, 0, gridProcToMem.RowDefinitions[2].ActualHeight - text_DataBus.Height);
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
         *      digit 4: memory/operand location/number - 1st digit
         *      digit 5: memory/operand location/number - 2nd digit
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
         *      digit 4: type of addressing (0 = immediate, 1 = direct, 2 = indirect)
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
            #region Initialise Appropriate text_ToRegister
            TextBlock text_ToRegister = texts_ToRegister[iRegisterIndex];
            string text_ToRegister_RegisteredName = "texts_ToRegister" + iRegisterIndex;
            text_ToRegister.Text = sContent;
            Grid Parentgrid = gridsRegWires[iRegisterIndex];
            if (!(Parentgrid.Children.Contains(text_ToRegister)))
                Parentgrid.Children.Add(text_ToRegister);
            Grid.SetRow(text_ToRegister, 1);
            Grid.SetColumn(text_ToRegister, 0);
            Grid.SetRowSpan(text_ToRegister, 2);
            Grid.SetColumnSpan(text_ToRegister, 2);
            #endregion

            #region Create animation1
            ThicknessAnimation animation1 = new ThicknessAnimation();
            animation1.From = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (iRegisterIndex < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (iRegisterIndex >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
            animation1.By = new Thickness(-Parentgrid.ColumnDefinitions[1].MyWidth() + text_ToRegister.Width, 0, Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, 0);
            animation1.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation1.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation1, text_ToRegister_RegisteredName);
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
            Storyboard.SetTargetName(animation2, text_ToRegister_RegisteredName);
            Storyboard.SetTargetProperty(animation2, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[1] = animation2;

            #region Create animation3
            ThicknessAnimation animation3 = new ThicknessAnimation();
            animation3.By = new Thickness(-Parentgrid.ColumnDefinitions[0].MyWidth(), 0, Parentgrid.ColumnDefinitions[0].MyWidth(), 0);
            animation3.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation3.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation3, text_ToRegister_RegisteredName);
            Storyboard.SetTargetProperty(animation3, new PropertyPath(MarginProperty));
            animation3.Completed += delegate (object sender, EventArgs e)
            {
                if (iRegisterIndex > NumRegisters - 1)
                    ;//messagebox; message: Your computer chip does not have a (iRegisterIndex)th register. Program halting...
                else
                    texts_Registers[iRegisterIndex].Text = new string(KSConvert.DecimalToBinaryForRegisters(int.Parse(text_ToRegister.Text)));
            };
            #endregion
            ToReturn[2] = animation3;

            return ToReturn;
        }
        private ThicknessAnimation[] GetAnimationsNumberFromRegister(int iRegisterIndex, double doubleDurationInMilliseconds)
        {
            ThicknessAnimation[] ToReturn = new ThicknessAnimation[3];
            #region Initialize Appropriate text_FromRegister
            TextBlock text_FromRegister = texts_ToRegister[iRegisterIndex];
            text_FromRegister.Text = KSConvert.BinaryToDecimalForRegisters(texts_Registers[iRegisterIndex].Text.ToCharArray()).ToString();
            string text_FromRegister_RegisteredName = "texts_ToRegister" + iRegisterIndex;
            Grid ParentGrid = gridsRegWires[iRegisterIndex];
            if (!ParentGrid.Children.Contains(text_FromRegister))
                ParentGrid.Children.Add(text_FromRegister);
            Grid.SetRow(text_FromRegister, 1);
            Grid.SetColumn(text_FromRegister, 0);
            Grid.SetRowSpan(text_FromRegister, 2);
            Grid.SetColumnSpan(text_FromRegister, 2);
            #endregion

            #region Create animation1
            ThicknessAnimation animation1 = new ThicknessAnimation();
            animation1.From = new Thickness(0, ((iRegisterIndex < 3) ? 0 : ParentGrid.RowDefinitions[1].MyHeight()), ParentGrid.Width - text_FromRegister.Width, ParentGrid.RowDefinitions[2].MyHeight() + ((iRegisterIndex < 3) ? ParentGrid.RowDefinitions[1].MyHeight() : 0) - text_FromRegister.Height);
            animation1.By = new Thickness(ParentGrid.ColumnDefinitions[0].MyWidth(), 0, -ParentGrid.ColumnDefinitions[0].MyWidth(), 0);
            animation1.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation1.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation1, text_FromRegister_RegisteredName);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[0] = animation1;

            #region Create animation2
            ThicknessAnimation animation2 = new ThicknessAnimation();
            animation2.By = new Thickness(0, ((iRegisterIndex < 3) ? 1 : -1) * ParentGrid.RowDefinitions[1].MyHeight(), 0, ((iRegisterIndex < 3) ? -1 : 1) * ParentGrid.RowDefinitions[1].MyHeight());
            animation2.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation2.EasingFunction = new SineEase();
            Storyboard.SetTargetName(animation2, text_FromRegister_RegisteredName);
            Storyboard.SetTargetProperty(animation2, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[1] = animation2;

            #region Create animation3
            ThicknessAnimation animation3 = new ThicknessAnimation();
            animation3.To = new Thickness(ParentGrid.Width - text_FromRegister.Width, ((iRegisterIndex < 3) ? ParentGrid.RowDefinitions[1].MyHeight() : 0), 0, ParentGrid.RowDefinitions[2].MyHeight() + ((iRegisterIndex < 3) ? 0 : ParentGrid.RowDefinitions[1].MyHeight()) - text_FromRegister.Height);
            animation3.Duration = TimeSpan.FromMilliseconds(doubleDurationInMilliseconds / 3);
            animation3.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(animation3, text_FromRegister_RegisteredName);
            Storyboard.SetTargetProperty(animation3, new PropertyPath(MarginProperty));
            #endregion
            ToReturn[2] = animation3;

            return ToReturn;
        }

        #region Custom DispatcherTimer event handlers
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
        private void dtStoreAndEmphasise_Tick(object sender, EventArgs e)
        {
            char[] ToStore = new char[6];
            int placevalue = 100000;//placevalue of the 6th digit
            for (int i = 0; i < ToStore.Length; i++)
            {
                ToStore[i] = (char)('0' + (int.Parse(text_DataBus.Text) / placevalue) % 10);
                placevalue /= 10;
            }
            int MemLocationNum = int.Parse(text_AddressBus.Text);
            StackPanel MemPanel = (MemLocationNum >= lookup_MemorySpec[MemorySpec] / 2) ? memoryStackPanel2 : memoryStackPanel1;
            (((MemPanel.Children[MemLocationNum % 10]) as DockPanel).Children[1] as TextBlock).Text = new string(ToStore);
            (((MemPanel.Children[MemLocationNum % 10]) as DockPanel).Children[1] as TextBlock).Background = Brushes.Goldenrod;
            DispatcherTimer dtEmphasiseStoredData = new DispatcherTimer();
            dtEmphasiseStoredData.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 8);
            dtEmphasiseStoredData.Tick += dtEmphasisData_Tick_2;
            dtEmphasiseStoredData.Start();
            (sender as DispatcherTimer).Stop();
        }
        private void dtShowComparison_Tick(object sender, EventArgs e)
        {
            char[] carInstruction = text_CIR.Text.ToCharArray();
            int mainregnum = carInstruction[1] - '0';
            int comp1 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[mainregnum].Text.ToCharArray());
            int secondregnum;
            int comp2;
            if (carInstruction[2] == '1')
            {
                secondregnum = (carInstruction[3] - '0') * 10 + (carInstruction[4] - '0');
                comp2 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[secondregnum].Text.ToCharArray());
            }
            else
                comp2 = (carInstruction[3] - '0') * 10 + (carInstruction[4] - '0');
            text_CMP.Text = comp1 + " CMP " + comp2;
            (sender as DispatcherTimer).Stop();
        }
        private void dtShowComparand2_Tick(object sender, EventArgs e)
        {
            char[] car = text_CIR.Text.ToCharArray();
            text_CMP.Text = ((car[3] - '0') * 10 + (car[4] - '0')).ToString();
            (sender as DispatcherTimer).Stop();
        }
        private void dtEmphasizeCMP_Tick_1(object sender, EventArgs e)
        {
            text_CMP.Background = Brushes.Red;
            (sender as DispatcherTimer).Tick -= dtEmphasizeCMP_Tick_1;
            (sender as DispatcherTimer).Tick += dtEmphasizeCMP_Tick_2;
        }
        private void dtEmphasizeCMP_Tick_2(object sender, EventArgs e)
        {
            text_CMP.Background = Brushes.White;
            (sender as DispatcherTimer).Stop();
        }
        #endregion

        private void ExecuteInstruction_LDR(string AssemblyLine)
        {
            char[] cArLine = AssemblyLine.ToCharArray();
            int RegisterNumber = cArLine[1] - '0';
            TextBlock text_ToRegister = texts_ToRegister[RegisterNumber];
            string ContentToLoad = "";
            Storyboard ToPlay = new Storyboard();
            //ToPlay must consist of four animations, address from processor to memory, picking the value in the specified location, data from memory to processor, same data to correct register
            if (cArLine[2] == '0')//immediate addressing
            {
                ContentToLoad = int.Parse(new string(new char[] { cArLine[3], cArLine[4] })).ToString();

                #region Initialise text_ToALU
                text_ToALU.Text = ContentToLoad;
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

                DispatcherTimer dtToRemovetext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
                dtToRemovetext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToALU));

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
                dtRemoveDataBus.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_DataBus));
                #endregion
                ContentToLoad = int.Parse(text_DataBus.Text).ToString();

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
            Grid Parentgrid = text_ToRegister.Parent as Grid;
            text_ToRegister.Margin = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (RegisterNumber < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (RegisterNumber >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
            ToPlay.Children.Add(tanimsNumberToRegister[0]);
            ToPlay.Children.Add(tanimsNumberToRegister[1]);
            ToPlay.Children.Add(tanimsNumberToRegister[2]);

            #region Create dtRevealtext_ToRegister
            DispatcherTimer dtRevealtext_ToRegister = new DispatcherTimer();
            dtRevealtext_ToRegister.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            dtRevealtext_ToRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_ToRegister));
            #endregion

            #region Create dtRemovetext_ToRegister
            DispatcherTimer dtRemovetext_ToRegister = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            dtRemovetext_ToRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToRegister));
            #endregion

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction executed...\n\tNext instruction";
                Fetch();
            };
            ToPlay.Begin(this);
            dtRevealtext_ToRegister.Start();
            dtRemovetext_ToRegister.Start();
        }

        private void ExecuteInstruction_STR(string AssemblyLine)
        {
            char[] cAr = AssemblyLine.ToCharArray();
            int RegisterNumber = cAr[1] - '0';
            TextBlock text_FromRegister = texts_ToRegister[RegisterNumber];
            int MemoryLocIndex = int.Parse(new string(new char[] { cAr[3], cAr[4] }));
            string ToStore = KSConvert.BinaryToDecimalForRegisters(texts_Registers[RegisterNumber].Text.ToCharArray()).ToString();
            Storyboard ToPlay = new Storyboard();
            
            #region Get tanimsFromReg
            ThicknessAnimation[] tanimsFromReg = GetAnimationsNumberFromRegister(RegisterNumber, lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            for (int i = 0; i < tanimsFromReg.Length; i++)
            {
                tanimsFromReg[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            }
            #endregion
            Grid Parentgrid = text_FromRegister.Parent as Grid;
            text_FromRegister.Margin = new Thickness(0, ((RegisterNumber < 3) ? 0 : Parentgrid.RowDefinitions[1].MyHeight()), Parentgrid.Width - text_FromRegister.Width, Parentgrid.RowDefinitions[2].MyHeight() + ((RegisterNumber < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0) - text_FromRegister.Height);
            ToPlay.Children.Add(tanimsFromReg[0]);
            ToPlay.Children.Add(tanimsFromReg[1]);
            ToPlay.Children.Add(tanimsFromReg[2]);

            #region Create dtRemovetext_FromRegister
            DispatcherTimer dtRemovetext_FromRegister = new DispatcherTimer();
            dtRemovetext_FromRegister.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            dtRemovetext_FromRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_FromRegister));
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
            dtRevealtext_AddressBus.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_AddressBus));
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
            dtRemovetext_AddressBus.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_AddressBus));
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
            dtRevealtext_DataBus.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_DataBus));
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
            dtRemovetext_DataBus.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_DataBus));
            #endregion

            #region Create dtStoreAndEmphasise
            DispatcherTimer dtStoreAndEmphasise = new DispatcherTimer();
            dtStoreAndEmphasise.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 8 - 20);//delay for variables to work
            dtStoreAndEmphasise.Tick += dtStoreAndEmphasise_Tick;
            #endregion

            #region Make filler animation to end ToPlay after information has been stored
            DoubleAnimation filler = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 8), BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 8)};
            Storyboard.SetTargetName(filler, "texts_ToRegister0");//doesn't matter
            Storyboard.SetTargetProperty(filler, new PropertyPath(OpacityProperty));//doesn't matter
            #endregion
            ToPlay.Children.Add(filler);

            ToPlay.Completed += delegate (object csender, EventArgs c)
            {
                (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction executed...\n\tNext instruction";
                Fetch();
            };
            ToPlay.Begin(this);
            dtRemovetext_FromRegister.Start();
            dtRevealtext_AddressBus.Start();
            dtRemovetext_AddressBus.Start();
            dtRevealtext_DataBus.Start();
            dtRemovetext_DataBus.Start();
            dtStoreAndEmphasise.Start();
        }

        private void ExecuteInstruction_MOV(string AssemblyLine)
        {
            char[] cArLine = AssemblyLine.ToCharArray();
            int MainRegisterNumber = cArLine[1] - '0';
            TextBlock text_MainRegister = texts_ToRegister[MainRegisterNumber];
            TextBlock text_ToRegister = texts_ToRegister[MainRegisterNumber];
            string ContentToCopy;
            Storyboard ToPlay = new Storyboard();

            if (cArLine[2] == '0')//immediate addressing
            {
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

                DispatcherTimer dtToRemovetext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
                dtToRemovetext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToALU));

                ContentToCopy = new string(new char[] { cArLine[3], cArLine[4] });

                dtToRemovetext_ToALU.Start();
            }
            else//direct addressing
            {
                int DepartureRegisterNumber = (cArLine[3] - '0') * 10 + (cArLine[4] - '0');
                TextBlock text_FromRegister = texts_ToRegister[DepartureRegisterNumber];

                #region Get tanimsContentFromReg
                ThicknessAnimation[] tanimsContentFromReg = GetAnimationsNumberFromRegister(DepartureRegisterNumber, lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
                for (int i = 0; i < 3; i++)
                    tanimsContentFromReg[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
                #endregion
                ToPlay.Children.Add(tanimsContentFromReg[0]);
                ToPlay.Children.Add(tanimsContentFromReg[1]);
                ToPlay.Children.Add(tanimsContentFromReg[2]);

                #region Create dtRemovetext_FromRegister
                DispatcherTimer dtRemovetext_FromRegister = new DispatcherTimer();
                dtRemovetext_FromRegister.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
                dtRemovetext_FromRegister.Tick +=new EventHandler(KSTimerEvHandlers.Generate("Remove", text_FromRegister));
                #endregion

                ContentToCopy = KSConvert.BinaryToDecimalForRegisters(texts_Registers[DepartureRegisterNumber].Text.ToCharArray()).ToString();

                dtRemovetext_FromRegister.Start();
            }

            #region Get tanimsNumberToRegister
            ThicknessAnimation[] tanimsNumberToRegister = GetAnimationsNumberToRegister(MainRegisterNumber, ContentToCopy, lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            for (int i = 0; i < tanimsNumberToRegister.Length; i++)
            {
                tanimsNumberToRegister[i].BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4 + i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            }
            #endregion
            text_ToRegister.Visibility = Visibility.Collapsed;
            ToPlay.Children.Add(tanimsNumberToRegister[0]);
            ToPlay.Children.Add(tanimsNumberToRegister[1]);
            ToPlay.Children.Add(tanimsNumberToRegister[2]);

            #region Create dtRevealtext_ToRegister
            DispatcherTimer dtRevealtext_ToRegister = new DispatcherTimer();
            dtRevealtext_ToRegister.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            dtRevealtext_ToRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_ToRegister));
            #endregion

            #region Create dtRemovetext_ToRegister
            DispatcherTimer dtRemovetext_ToRegister = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            dtRemovetext_ToRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToRegister));
            #endregion

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction executed...\n\tNext instruction";
                Fetch();
            };
            ToPlay.Begin(this);
            dtRevealtext_ToRegister.Start();
            dtRemovetext_ToRegister.Start();
        }

        private void ExecuteInstruction_CMP(string AssemblyLine)
        {
            // Second parameter is an <op> => could be either a register number (direct addressing) or a raw number (immediate addressing)
            char[] cArLine = AssemblyLine.ToCharArray();
            int MainRegisterNumber = cArLine[1] - '0';
            TextBlock text_MainRegister = texts_ToRegister[MainRegisterNumber];
            int Comparand1 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[MainRegisterNumber].Text.ToCharArray());
            int Comparand2 = 0;
            int SecondRegisterNumber = -1;
            if (cArLine[2] == '1')//direct addressing
                SecondRegisterNumber = (cArLine[3] - '0') * 10 + (cArLine[4] - '0');
            DispatcherTimer dtRemovetext_SecondRegister = new DispatcherTimer();//for direct addressing
            Storyboard ToPlay = new Storyboard();
            /* interval size: lookup_ClockSpeed[ClockSpeed] / 12
             * first three intervals: register data and <op> data transferred
             * fourth interval: moving data to the ALU
             * fifth and sixth intervals: show new cmp value
             */

            #region Create tanimsNumberFromRegister
            ThicknessAnimation[] tanimsNumberFromRegister = GetAnimationsNumberFromRegister(MainRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            for (int i = 0; i < tanimsNumberFromRegister.Length; i++)
            {
                tanimsNumberFromRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            }
            #endregion
            ToPlay.Children.Add(tanimsNumberFromRegister[0]);
            ToPlay.Children.Add(tanimsNumberFromRegister[1]);
            ToPlay.Children.Add(tanimsNumberFromRegister[2]);

            #region Create dtRemovetext_MainRegister
            DispatcherTimer dtRemovetext_MainRegister = new DispatcherTimer();
            dtRemovetext_MainRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            dtRemovetext_MainRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_MainRegister));
            #endregion

            #region Initialize text_ToALU
            text_ToALU.Text = text_MainRegister.Text;
            gridToALU.Children.Add(text_ToALU);
            Grid.SetColumn(text_ToALU, 0);
            Grid.SetColumnSpan(text_ToALU, 2);
            text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].MyWidth() - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].MyWidth() - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
            text_ToALU.Visibility = Visibility.Collapsed;
            #endregion

            #region Create dtRevealtext_ToALU
            DispatcherTimer dtRevealtext_ToALU = new DispatcherTimer();
            dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            dtRevealtext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_ToALU));
            #endregion

            #region Create tanimRegNumToALU
            ThicknessAnimation tanimRegNumToALU = new ThicknessAnimation();
            tanimRegNumToALU.By = new Thickness(0, gridToALU.Height - text_ToALU.Height, 0, text_ToALU.Height - gridToALU.Height);
            tanimRegNumToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            tanimRegNumToALU.EasingFunction = new CubicEase();
            tanimRegNumToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            Storyboard.SetTargetName(tanimRegNumToALU, "text_ToALU");
            Storyboard.SetTargetProperty(tanimRegNumToALU, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimRegNumToALU);

            if (cArLine[2] == '0')//immediate addressing
            {
                #region Create dtShowComparand2
                DispatcherTimer dtShowComparand2 = new DispatcherTimer();
                dtShowComparand2.Interval = TimeSpan.FromMilliseconds(0);
                dtShowComparand2.Tick += dtShowComparand2_Tick;
                #endregion
                //set comparand 2
                Comparand2 = (cArLine[3] - '0') * 10 + (cArLine[4] - '0');
                dtShowComparand2.Start();
            }
            else if (cArLine[2] == '1' && SecondRegisterNumber != MainRegisterNumber && SecondRegisterNumber != -1)//direct addressing
            {
                #region Get tanimsDataFromSecondReg
                ThicknessAnimation[] tanimsDataFromSecondReg = GetAnimationsNumberFromRegister(SecondRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
                for (int i = 0; i < 3; i++)
                {
                    tanimsDataFromSecondReg[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
                }
                #endregion
                ToPlay.Children.Add(tanimsDataFromSecondReg[0]);
                ToPlay.Children.Add(tanimsDataFromSecondReg[1]);
                ToPlay.Children.Add(tanimsDataFromSecondReg[2]);

                #region Assign dtRemovetext_SecondRegister
                dtRemovetext_SecondRegister.Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
                dtRemovetext_SecondRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[SecondRegisterNumber]));
                #endregion
            }
            else if (SecondRegisterNumber == MainRegisterNumber)
                Comparand2 = Comparand1;

            #region Create dtRemovetext_ToALU
            DispatcherTimer dtRemovetext_ToALU = new DispatcherTimer();
            dtRemovetext_ToALU.Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            dtRemovetext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToALU));
            #endregion

            #region Create dtShowComparison
            DispatcherTimer dtShowComparison = new DispatcherTimer();
            dtShowComparison.Interval = TimeSpan.FromMilliseconds(9 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 24);
            dtShowComparison.Tick += dtShowComparison_Tick;
            #endregion

            #region Create tanimFiller (to allow time for user to see change of text_CMP)
            ThicknessAnimation tanimFiller = new ThicknessAnimation();
            tanimFiller.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            tanimFiller.BeginTime = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 12);
            Storyboard.SetTargetName(tanimFiller, "text_ToALU");
            Storyboard.SetTargetProperty(tanimFiller, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimFiller);

            ToPlay.Completed += delegate (object senderc, EventArgs c)
            {
                int cmp = Comparand1.CompareTo(Comparand2);
                if (cmp > 0)
                    text_CMP.Text = ">";//greater than
                else if (cmp == 0)
                    text_CMP.Text = "=";//equal to
                else
                    text_CMP.Text = "<";//less than
                text_CMP.Visibility = Visibility.Visible;
                (runtimeStackPanel.Children[0] as TextBlock).Text += "\n>>Instruction executed...\n\tNext instruction";
                Fetch();
            };
            ToPlay.Begin(this);
            dtRemovetext_MainRegister.Start();
            dtRevealtext_ToALU.Start();
            dtRemovetext_ToALU.Start();
            dtShowComparison.Start();
            dtRemovetext_SecondRegister.Start();
        }

        private void ExecuteInstruction_Branch(string AssemblyLine)
        {
            char[] car = AssemblyLine.ToCharArray();
            Storyboard ToPlay = new Storyboard();
            bool DoBranch = true;
            switch (car[1])
            {
                case '1':
                    if (text_CMP.Text != "=")
                        DoBranch = false;
                    break;
                case '2':
                    if (text_CMP.Text == "=")
                        DoBranch = false;
                    break;
                case '3':
                    if (text_CMP.Text != ">")
                        DoBranch = false;
                    break;
                case '4':
                    if (text_CMP.Text != "<")
                        DoBranch = false;
                    break;
            }
            if (DoBranch)
            {
                DispatcherTimer dtChangePC = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
                dtChangePC.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, ((car[2] - '0') * 10 + (car[3] - '0') - 1).ToString()));
                dtChangePC.Start();
            }
            else
            {
                //Change text_CMP.Background to show that comparison does not satisfy conditions
                DispatcherTimer dtEmphasizeCMP = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 6) };
                dtEmphasizeCMP.Tick += dtEmphasizeCMP_Tick_1;
                dtEmphasizeCMP.Start();
            }

            #region Initialise text_ToALU
            text_ToALU.Text = int.Parse(text_PC.Text).ToString();
            gridToALU.Children.Add(text_ToALU);
            Grid.SetColumn(text_ToALU, 0);
            Grid.SetColumnSpan(text_ToALU, 2);
            text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            text_ToALU.Visibility = Visibility.Visible;
            #endregion

            #region Create tanimationToALU
            ThicknessAnimation tanimationToALU = new ThicknessAnimation();
            tanimationToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            tanimationToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
            tanimationToALU.Duration = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            tanimationToALU.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimationToALU, "text_ToALU");
            Storyboard.SetTargetProperty(tanimationToALU, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimationToALU);

            #region Create tanimationFromALU
            ThicknessAnimation tanimationFromALU = new ThicknessAnimation();
            tanimationFromALU.To = new Thickness(text_ToALU.Margin.Left, 0, text_ToALU.Margin.Right, gridToALU.Height - text_ToALU.Height);
            tanimationFromALU.Duration = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            tanimationFromALU.EasingFunction = new CubicEase();
            Storyboard.SetTargetName(tanimationFromALU, "text_ToALU");
            Storyboard.SetTargetProperty(tanimationFromALU, new PropertyPath(MarginProperty));
            tanimationFromALU.BeginTime = TimeSpan.FromMilliseconds((double)lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            #endregion
            ToPlay.Children.Add(tanimationFromALU);

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                text_PC.Text = new string(KSConvert.IntTo2DigCharArray(int.Parse(text_ToALU.Text)));
                gridToALU.Children.Remove(text_ToALU);
            };

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                Fetch();
            };
            ToPlay.Begin(this);
        }

        private void ExecuteInstruction_ADD(string AssemblyLine)
        {
            string CMPinfo = text_CMP.Text;
            char[] cArLine = AssemblyLine.ToCharArray();
            int MainRegisterNumber = cArLine[1] - '0';
            int ParameterRegisterNumber = cArLine[2] - '0';
            int iOperand1, iOperand2, iSum;
            iOperand1 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[ParameterRegisterNumber].Text.ToCharArray());
            Storyboard ToPlay = new Storyboard();
            //Numbers to processor - 3/16
            //To ALU - 1/16
            //Back - 1/16
            //Back to reg - 3/16

            #region Get tanimstext_ParameterRegister
            TextBlock text_ParameterRegister = texts_ToRegister[ParameterRegisterNumber];
            text_ParameterRegister.Text = iOperand1.ToString();
            ThicknessAnimation[] tanimstext_ParameterRegister;
            if (cArLine[3] == '0')
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                }
            }
            else
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
            }
            #endregion
            ToPlay.Children.Add(tanimstext_ParameterRegister[0]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[1]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[2]);

            #region Create dtRemovetext_ParameterRegister
            DispatcherTimer dtRemovetext_ParameterRegister = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRemovetext_ParameterRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[ParameterRegisterNumber]));
            #endregion

            #region Initialize text_ToALU
            text_ToALU.Text = iOperand1.ToString();
            text_ToALU.Visibility = Visibility.Collapsed;
            gridToALU.Children.Add(text_ToALU);
            Grid.SetColumnSpan(text_ToALU, 2);
            text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            #endregion

            #region Create dtRevealtext_ToALU
            DispatcherTimer dtRevealtext_ToALU = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRevealtext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_ToALU));
            #endregion

            string OldText;
            if (cArLine[3] == '0')//immediate addressing
            {
                OldText = text_CMP.Text;
                text_CMP.Text = (iOperand2 = (cArLine[4] - '0') * 10 + (cArLine[5] - '0')).ToString();

                #region Create tanimFirstNumberToALU 
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                tanimFirstNumberToALU.EasingFunction = new CubicEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);
            }
            else//direct addressing
            {
                int SecondRegisterNumber = (cArLine[4] - '0') * 10 + (cArLine[5] - '0');
                #region Get tanimstext_SecondRegister
                TextBlock text_SecondRegister = texts_ToRegister[SecondRegisterNumber];
                text_SecondRegister.Text = (iOperand2 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[SecondRegisterNumber].Text.ToCharArray())).ToString();
                ThicknessAnimation[] tanimstext_SecondRegister = GetAnimationsNumberFromRegister(SecondRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_SecondRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
                #endregion
                ToPlay.Children.Add(tanimstext_SecondRegister[0]);
                ToPlay.Children.Add(tanimstext_SecondRegister[1]);
                ToPlay.Children.Add(tanimstext_SecondRegister[2]);

                #region Create dtRemovetext_SecondRegister
                DispatcherTimer dtRemovetext_SecondRegister = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtRemovetext_SecondRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_SecondRegister));
                #endregion

                #region Create tanimFirstNumberToALU
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimFirstNumberToALU.EasingFunction = new SineEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);

                #region Create dtChangeContenttext_ToALU
                DispatcherTimer dtChangeContenttext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtChangeContenttext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iOperand2.ToString()));
                #endregion

                #region Create tanimSecondNumberToALU
                ThicknessAnimation tanimSecondNumberToALU = new ThicknessAnimation();
                tanimSecondNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimSecondNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimSecondNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimSecondNumberToALU.EasingFunction = new SineEase();
                tanimSecondNumberToALU.BeginTime = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimSecondNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimSecondNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimSecondNumberToALU);

                OldText = text_CMP.Text;

                dtRemovetext_SecondRegister.Start();
                dtChangeContenttext_ToALU.Start();
            }

            iSum = iOperand1 + iOperand2;

            #region Show ADD operation in ALU using dtShowADD
            DispatcherTimer dtShowADD = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            DispatcherTimer dtBackToCMPInfo = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            double OldFontSize = text_CMP.FontSize;
            Action<object, EventArgs> TickEventHandler1 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = iOperand1.ToString() + " + " + iOperand2.ToString() + " = " + iSum.ToString();
                text_CMP.FontSize *= 0.85;
                (sender as DispatcherTimer).Stop();
            };
            dtShowADD.Tick += new EventHandler(TickEventHandler1);
            Action<object, EventArgs> TickEventHandler2 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = OldText;
                text_CMP.FontSize = OldFontSize;
                (sender as DispatcherTimer).Stop();
            };
            dtBackToCMPInfo.Tick += new EventHandler(TickEventHandler2);
            #endregion

            #region Create dtChangeContentToSum_text_ToALU
            DispatcherTimer dtChangeContentToSum_text_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            dtChangeContentToSum_text_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iSum.ToString()));
            #endregion

            #region Create tanimSumToProc
            ThicknessAnimation tanimSumToProc = new ThicknessAnimation();
            tanimSumToProc.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height, gridToALU.Width / 2 - text_ToALU.Width / 2, 0);
            tanimSumToProc.To = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
            tanimSumToProc.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            tanimSumToProc.EasingFunction = new CubicEase();
            tanimSumToProc.BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            Storyboard.SetTargetName(tanimSumToProc, "text_ToALU");
            Storyboard.SetTargetProperty(tanimSumToProc, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimSumToProc);

            #region Create dtRemovetext_ToALU
            DispatcherTimer dtRemovetext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRemovetext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToALU));
            #endregion

            #region Initialize text_SumCarrier at the right time in animation sequence
            TextBlock text_ToRegister = texts_ToRegister[MainRegisterNumber];
            if (ParameterRegisterNumber != MainRegisterNumber)
                text_ToRegister.Visibility = Visibility.Collapsed;
            Grid Parentgrid = gridsRegWires[MainRegisterNumber];
            Action<object, EventArgs> TickSetMargin = (object sender, EventArgs e) =>
            {
                text_ToRegister.Margin = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (MainRegisterNumber < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (MainRegisterNumber >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
                (sender as DispatcherTimer).Stop();
            };
            DispatcherTimer dtSetMargin = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtSetMargin.Tick += new EventHandler(TickSetMargin);
            DispatcherTimer dtRevealtext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRevealtext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", texts_ToRegister[MainRegisterNumber]));
            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                Action<object, EventArgs> Addtext_ToRegisterToGrid = (object sender, EventArgs e) =>
                {
                    Parentgrid.Children.Add(text_ToRegister);
                };
                dtRevealtext_SumCarrier.Tick += new EventHandler(Addtext_ToRegisterToGrid);
            }
            #endregion

            #region Get tanimsSumToRegister
            ThicknessAnimation[] tanimsSumToRegister = GetAnimationsNumberToRegister(MainRegisterNumber, iSum.ToString(), 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            for (int i = 0; i < 3; i++)
            {
                tanimsSumToRegister[i].BeginTime = TimeSpan.FromMilliseconds((5 + i) * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            }
            #endregion
            ToPlay.Children.Add(tanimsSumToRegister[0]);
            ToPlay.Children.Add(tanimsSumToRegister[1]);
            ToPlay.Children.Add(tanimsSumToRegister[2]);

            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                text_ParameterRegister.Text = iOperand1.ToString();
                DispatcherTimer dtChangeContentToSum = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
                dtChangeContentToSum.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ParameterRegister, iSum.ToString()));
                dtChangeContentToSum.Start();
            }

            #region Create dtRemovetext_SumCarrier
            DispatcherTimer dtRemovetext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            dtRemovetext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[MainRegisterNumber]));
            #endregion

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                texts_Registers[MainRegisterNumber].Text = new string(KSConvert.DecimalToBinaryForRegisters(iSum));
                Fetch();
            };

            ToPlay.Begin(this);
            dtRemovetext_ParameterRegister.Start();
            dtRevealtext_ToALU.Start();
            dtShowADD.Start();
            dtBackToCMPInfo.Start();
            dtChangeContentToSum_text_ToALU.Start();
            dtRemovetext_ToALU.Start();
            dtSetMargin.Start();
            dtRevealtext_SumCarrier.Start();
            dtRemovetext_SumCarrier.Start();
        }

        private void ExecuteInstruction_SUB(string AssemblyLine)
        {
            char[] cArLine = AssemblyLine.ToCharArray();
            int MainRegisterNumber = cArLine[1] - '0';
            int ParameterRegisterNumber = cArLine[2] - '0';
            int iOperand1, iOperand2, iSum;
            iOperand1 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[ParameterRegisterNumber].Text.ToCharArray());
            Storyboard ToPlay = new Storyboard();
            //Numbers to processor - 3/16
            //To ALU - 1/16
            //Back - 1/16
            //Back to reg - 3/16

            #region Get tanimstext_ParameterRegister
            TextBlock text_ParameterRegister = texts_ToRegister[ParameterRegisterNumber];
            text_ParameterRegister.Text = iOperand1.ToString();
            ThicknessAnimation[] tanimstext_ParameterRegister;
            if (cArLine[3] == '0')
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                }
            }
            else
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
            }
            #endregion
            ToPlay.Children.Add(tanimstext_ParameterRegister[0]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[1]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[2]);

            #region Create dtRemovetext_ParameterRegister
            DispatcherTimer dtRemovetext_ParameterRegister = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRemovetext_ParameterRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[ParameterRegisterNumber]));
            #endregion

            #region Initialize text_ToALU
            text_ToALU.Text = iOperand1.ToString();
            text_ToALU.Visibility = Visibility.Collapsed;
            gridToALU.Children.Add(text_ToALU);
            Grid.SetColumnSpan(text_ToALU, 2);
            text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            #endregion

            #region Create dtRevealtext_ToALU
            DispatcherTimer dtRevealtext_ToALU = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRevealtext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_ToALU));
            #endregion

            string OldText;
            if (cArLine[3] == '0')//immediate addressing
            {
                OldText = text_CMP.Text;
                text_CMP.Text = (iOperand2 = (cArLine[4] - '0') * 10 + (cArLine[5] - '0')).ToString();

                #region Create tanimFirstNumberToALU 
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                tanimFirstNumberToALU.EasingFunction = new CubicEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);
            }
            else//direct addressing
            {
                int SecondRegisterNumber = (cArLine[4] - '0') * 10 + (cArLine[5] - '0');
                #region Get tanimstext_SecondRegister
                TextBlock text_SecondRegister = texts_ToRegister[SecondRegisterNumber];
                text_SecondRegister.Text = (iOperand2 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[SecondRegisterNumber].Text.ToCharArray())).ToString();
                ThicknessAnimation[] tanimstext_SecondRegister = GetAnimationsNumberFromRegister(SecondRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_SecondRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
                #endregion
                ToPlay.Children.Add(tanimstext_SecondRegister[0]);
                ToPlay.Children.Add(tanimstext_SecondRegister[1]);
                ToPlay.Children.Add(tanimstext_SecondRegister[2]);

                #region Create dtRemovetext_SecondRegister
                DispatcherTimer dtRemovetext_SecondRegister = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtRemovetext_SecondRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_SecondRegister));
                #endregion

                #region Create tanimFirstNumberToALU
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimFirstNumberToALU.EasingFunction = new SineEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);

                #region Create dtChangeContenttext_ToALU
                DispatcherTimer dtChangeContenttext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtChangeContenttext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iOperand2.ToString()));
                #endregion

                #region Create tanimSecondNumberToALU
                ThicknessAnimation tanimSecondNumberToALU = new ThicknessAnimation();
                tanimSecondNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimSecondNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimSecondNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimSecondNumberToALU.EasingFunction = new SineEase();
                tanimSecondNumberToALU.BeginTime = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimSecondNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimSecondNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimSecondNumberToALU);

                OldText = text_CMP.Text;

                dtRemovetext_SecondRegister.Start();
                dtChangeContenttext_ToALU.Start();
            }

            iSum = iOperand1 - iOperand2;

            #region Show SUB operation in ALU using dtShowSUB
            DispatcherTimer dtShowSUB = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            DispatcherTimer dtBackToCMPInfo = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            double OldFontSize = text_CMP.FontSize;
            Action<object, EventArgs> TickEventHandler1 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = iOperand1.ToString() + " - " + iOperand2.ToString() + " = " + iSum.ToString();
                text_CMP.FontSize *= 0.85;
                (sender as DispatcherTimer).Stop();
            };
            dtShowSUB.Tick += new EventHandler(TickEventHandler1);
            Action<object, EventArgs> TickEventHandler2 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = OldText;
                text_CMP.FontSize = OldFontSize;
                (sender as DispatcherTimer).Stop();
            };
            dtBackToCMPInfo.Tick += new EventHandler(TickEventHandler2);
            #endregion

            #region Create dtChangeContentToSum_text_ToALU
            DispatcherTimer dtChangeContentToSum_text_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            dtChangeContentToSum_text_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iSum.ToString()));
            #endregion

            #region Create tanimSumToProc
            ThicknessAnimation tanimSumToProc = new ThicknessAnimation();
            tanimSumToProc.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height, gridToALU.Width / 2 - text_ToALU.Width / 2, 0);
            tanimSumToProc.To = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
            tanimSumToProc.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            tanimSumToProc.EasingFunction = new CubicEase();
            tanimSumToProc.BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            Storyboard.SetTargetName(tanimSumToProc, "text_ToALU");
            Storyboard.SetTargetProperty(tanimSumToProc, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimSumToProc);

            #region Create dtRemovetext_ToALU
            DispatcherTimer dtRemovetext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRemovetext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToALU));
            #endregion

            #region Initialize text_SumCarrier at the right time in animation sequence
            TextBlock text_ToRegister = texts_ToRegister[MainRegisterNumber];
            if (ParameterRegisterNumber != MainRegisterNumber)
                text_ToRegister.Visibility = Visibility.Collapsed;
            Grid Parentgrid = gridsRegWires[MainRegisterNumber];
            Action<object, EventArgs> TickSetMargin = (object sender, EventArgs e) =>
            {
                text_ToRegister.Margin = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (MainRegisterNumber < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (MainRegisterNumber >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
                (sender as DispatcherTimer).Stop();
            };
            DispatcherTimer dtSetMargin = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtSetMargin.Tick += new EventHandler(TickSetMargin);
            DispatcherTimer dtRevealtext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRevealtext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", texts_ToRegister[MainRegisterNumber]));
            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                Action<object, EventArgs> Addtext_ToRegisterToGrid = (object sender, EventArgs e) =>
                {
                    Parentgrid.Children.Add(text_ToRegister);
                };
                dtRevealtext_SumCarrier.Tick += new EventHandler(Addtext_ToRegisterToGrid);
            }
            #endregion

            #region Get tanimsSumToRegister
            ThicknessAnimation[] tanimsSumToRegister = GetAnimationsNumberToRegister(MainRegisterNumber, iSum.ToString(), 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            for (int i = 0; i < 3; i++)
            {
                tanimsSumToRegister[i].BeginTime = TimeSpan.FromMilliseconds((5 + i) * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            }
            #endregion
            ToPlay.Children.Add(tanimsSumToRegister[0]);
            ToPlay.Children.Add(tanimsSumToRegister[1]);
            ToPlay.Children.Add(tanimsSumToRegister[2]);

            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                text_ParameterRegister.Text = iOperand1.ToString();
                DispatcherTimer dtChangeContentToSum = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
                dtChangeContentToSum.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ParameterRegister, iSum.ToString()));
                dtChangeContentToSum.Start();
            }

            #region Create dtRemovetext_SumCarrier
            DispatcherTimer dtRemovetext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            dtRemovetext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[MainRegisterNumber]));
            #endregion

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                texts_Registers[MainRegisterNumber].Text = new string(KSConvert.DecimalToBinaryForRegisters(iSum));
                Fetch();
            };

            ToPlay.Begin(this);
            dtRemovetext_ParameterRegister.Start();
            dtRevealtext_ToALU.Start();
            dtShowSUB.Start();
            dtBackToCMPInfo.Start();
            dtChangeContentToSum_text_ToALU.Start();
            dtRemovetext_ToALU.Start();
            dtSetMargin.Start();
            dtRevealtext_SumCarrier.Start();
            dtRemovetext_SumCarrier.Start();
        }

        private void ExecuteInstruction_AND(string AssemblyLine)
        {
            char[] cArLine = AssemblyLine.ToCharArray();
            int MainRegisterNumber = cArLine[1] - '0';
            int ParameterRegisterNumber = cArLine[2] - '0';
            int iOperand1, iOperand2, iSum;
            char[] cOperand1, cOperand2;
            iOperand1 = KSConvert.BinaryToDecimalForRegisters(cOperand1 = texts_Registers[ParameterRegisterNumber].Text.ToCharArray());
            Storyboard ToPlay = new Storyboard();
            //Numbers to processor - 3/16
            //To ALU - 1/16
            //Back - 1/16
            //Back to reg - 3/16

            #region Get tanimstext_ParameterRegister
            TextBlock text_ParameterRegister = texts_ToRegister[ParameterRegisterNumber];
            text_ParameterRegister.Text = iOperand1.ToString();
            ThicknessAnimation[] tanimstext_ParameterRegister;
            if (cArLine[3] == '0')//immediate addressing
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                }
            }
            else
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
            }
            #endregion
            ToPlay.Children.Add(tanimstext_ParameterRegister[0]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[1]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[2]);

            #region Create dtRemovetext_ParameterRegister
            DispatcherTimer dtRemovetext_ParameterRegister = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRemovetext_ParameterRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[ParameterRegisterNumber]));
            #endregion

            #region Initialize text_ToALU
            text_ToALU.Text = iOperand1.ToString();
            text_ToALU.Visibility = Visibility.Collapsed;
            gridToALU.Children.Add(text_ToALU);
            Grid.SetColumnSpan(text_ToALU, 2);
            text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            #endregion

            #region Create dtRevealtext_ToALU
            DispatcherTimer dtRevealtext_ToALU = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRevealtext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_ToALU));
            #endregion

            string OldText;
            if (cArLine[3] == '0')//immediate addressing
            {
                OldText = text_CMP.Text;
                text_CMP.Text = (iOperand2 = (cArLine[4] - '0') * 10 + (cArLine[5] - '0')).ToString();

                #region Create tanimFirstNumberToALU 
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                tanimFirstNumberToALU.EasingFunction = new CubicEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);
            }
            else//direct addressing
            {
                int SecondRegisterNumber = (cArLine[4] - '0') * 10 + (cArLine[5] - '0');
                #region Get tanimstext_SecondRegister
                TextBlock text_SecondRegister = texts_ToRegister[SecondRegisterNumber];
                text_SecondRegister.Text = (iOperand2 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[SecondRegisterNumber].Text.ToCharArray())).ToString();
                ThicknessAnimation[] tanimstext_SecondRegister = GetAnimationsNumberFromRegister(SecondRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_SecondRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
                #endregion
                ToPlay.Children.Add(tanimstext_SecondRegister[0]);
                ToPlay.Children.Add(tanimstext_SecondRegister[1]);
                ToPlay.Children.Add(tanimstext_SecondRegister[2]);

                #region Create dtRemovetext_SecondRegister
                DispatcherTimer dtRemovetext_SecondRegister = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtRemovetext_SecondRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_SecondRegister));
                #endregion

                #region Create tanimFirstNumberToALU
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimFirstNumberToALU.EasingFunction = new SineEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);

                #region Create dtChangeContenttext_ToALU
                DispatcherTimer dtChangeContenttext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtChangeContenttext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iOperand2.ToString()));
                #endregion

                #region Create tanimSecondNumberToALU
                ThicknessAnimation tanimSecondNumberToALU = new ThicknessAnimation();
                tanimSecondNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimSecondNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimSecondNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimSecondNumberToALU.EasingFunction = new SineEase();
                tanimSecondNumberToALU.BeginTime = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimSecondNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimSecondNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimSecondNumberToALU);
                
                OldText = text_CMP.Text;

                dtRemovetext_SecondRegister.Start();
                dtChangeContenttext_ToALU.Start();
            }

            cOperand2 = KSConvert.DecimalToBinaryForRegisters(iOperand2);
            char[] cSum = KSConvert.BitwiseAND(cOperand1, cOperand2);
            iSum = KSConvert.BinaryToDecimalForRegisters(cSum);

            #region Show AND operation in ALU using dtShowSUB
            DispatcherTimer dtShowAND = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            DispatcherTimer dtBackToCMPInfo = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            double OldFontSize = text_CMP.FontSize;
            Action<object, EventArgs> TickEventHandler1 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = iOperand1.ToString() + " AND " + iOperand2.ToString() + " = " + iSum.ToString();
                text_CMP.FontSize *= 0.8;
                (sender as DispatcherTimer).Stop();
            };
            dtShowAND.Tick += new EventHandler(TickEventHandler1);
            Action<object, EventArgs> TickEventHandler2 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = OldText;
                text_CMP.FontSize = OldFontSize;
                (sender as DispatcherTimer).Stop();
            };
            dtBackToCMPInfo.Tick += new EventHandler(TickEventHandler2);
            #endregion

            #region Create dtChangeContentToSum_text_ToALU
            DispatcherTimer dtChangeContentToSum_text_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            dtChangeContentToSum_text_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iSum.ToString()));
            #endregion

            #region Create tanimSumToProc
            ThicknessAnimation tanimSumToProc = new ThicknessAnimation();
            tanimSumToProc.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height, gridToALU.Width / 2 - text_ToALU.Width / 2, 0);
            tanimSumToProc.To = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
            tanimSumToProc.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            tanimSumToProc.EasingFunction = new CubicEase();
            tanimSumToProc.BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            Storyboard.SetTargetName(tanimSumToProc, "text_ToALU");
            Storyboard.SetTargetProperty(tanimSumToProc, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimSumToProc);

            #region Create dtRemovetext_ToALU
            DispatcherTimer dtRemovetext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRemovetext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToALU));
            #endregion

            #region Initialize text_SumCarrier at the right time in animation sequence
            TextBlock text_ToRegister = texts_ToRegister[MainRegisterNumber];
            if (ParameterRegisterNumber != MainRegisterNumber)
                text_ToRegister.Visibility = Visibility.Collapsed;
            Grid Parentgrid = gridsRegWires[MainRegisterNumber];
            Action<object, EventArgs> TickSetMargin = (object sender, EventArgs e) =>
            {
                text_ToRegister.Margin = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (MainRegisterNumber < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (MainRegisterNumber >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
                (sender as DispatcherTimer).Stop();
            };
            DispatcherTimer dtSetMargin = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtSetMargin.Tick += new EventHandler(TickSetMargin);
            DispatcherTimer dtRevealtext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRevealtext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", texts_ToRegister[MainRegisterNumber]));
            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                Action<object, EventArgs> Addtext_ToRegisterToGrid = (object sender, EventArgs e) =>
                {
                    Parentgrid.Children.Add(text_ToRegister);
                };
                dtRevealtext_SumCarrier.Tick += new EventHandler(Addtext_ToRegisterToGrid);
            }
            #endregion

            #region Get tanimsSumToRegister
            ThicknessAnimation[] tanimsSumToRegister = GetAnimationsNumberToRegister(MainRegisterNumber, iSum.ToString(), 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            for (int i = 0; i < 3; i++)
            {
                tanimsSumToRegister[i].BeginTime = TimeSpan.FromMilliseconds((5 + i) * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            }
            #endregion
            ToPlay.Children.Add(tanimsSumToRegister[0]);
            ToPlay.Children.Add(tanimsSumToRegister[1]);
            ToPlay.Children.Add(tanimsSumToRegister[2]);

            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                text_ParameterRegister.Text = iOperand1.ToString();
                DispatcherTimer dtChangeContentToSum = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
                dtChangeContentToSum.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ParameterRegister, iSum.ToString()));
                dtChangeContentToSum.Start();
            }

            #region Create dtRemovetext_SumCarrier
            DispatcherTimer dtRemovetext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            dtRemovetext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[MainRegisterNumber]));
            #endregion

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                texts_Registers[MainRegisterNumber].Text = new string(cSum);
                Fetch();
            };

            ToPlay.Begin(this);
            dtRemovetext_ParameterRegister.Start();
            dtRevealtext_ToALU.Start();
            dtShowAND.Start();
            dtBackToCMPInfo.Start();
            dtChangeContentToSum_text_ToALU.Start();
            dtRemovetext_ToALU.Start();
            dtSetMargin.Start();
            dtRevealtext_SumCarrier.Start();
            dtRemovetext_SumCarrier.Start();
        }

        private void ExecuteInstruction_ORR(string AssemblyLine)
        {
            char[] cArLine = AssemblyLine.ToCharArray();
            int MainRegisterNumber = cArLine[1] - '0';
            int ParameterRegisterNumber = cArLine[2] - '0';
            int iOperand1, iOperand2, iSum;
            char[] cOperand1, cOperand2;
            iOperand1 = KSConvert.BinaryToDecimalForRegisters(cOperand1 = texts_Registers[ParameterRegisterNumber].Text.ToCharArray());
            Storyboard ToPlay = new Storyboard();
            //Numbers to processor - 3/16
            //To ALU - 1/16
            //Back - 1/16
            //Back to reg - 3/16

            #region Get tanimstext_ParameterRegister
            TextBlock text_ParameterRegister = texts_ToRegister[ParameterRegisterNumber];
            text_ParameterRegister.Text = iOperand1.ToString();
            ThicknessAnimation[] tanimstext_ParameterRegister;
            if (cArLine[3] == '0')//immediate addressing
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                }
            }
            else
            {
                tanimstext_ParameterRegister = GetAnimationsNumberFromRegister(ParameterRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_ParameterRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
            }
            #endregion
            ToPlay.Children.Add(tanimstext_ParameterRegister[0]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[1]);
            ToPlay.Children.Add(tanimstext_ParameterRegister[2]);

            #region Create dtRemovetext_ParameterRegister
            DispatcherTimer dtRemovetext_ParameterRegister = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRemovetext_ParameterRegister.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRemovetext_ParameterRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[ParameterRegisterNumber]));
            #endregion

            #region Initialize text_ToALU
            text_ToALU.Text = iOperand1.ToString();
            text_ToALU.Visibility = Visibility.Collapsed;
            gridToALU.Children.Add(text_ToALU);
            Grid.SetColumnSpan(text_ToALU, 2);
            text_ToALU.Margin = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
            #endregion

            #region Create dtRevealtext_ToALU
            DispatcherTimer dtRevealtext_ToALU = new DispatcherTimer();
            if (cArLine[3] == '0')
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            else
                dtRevealtext_ToALU.Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
            dtRevealtext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", text_ToALU));
            #endregion

            string OldText;
            if (cArLine[3] == '0')//immediate addressing
            {
                OldText = text_CMP.Text;
                text_CMP.Text = (iOperand2 = (cArLine[4] - '0') * 10 + (cArLine[5] - '0')).ToString();

                #region Create tanimFirstNumberToALU 
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                tanimFirstNumberToALU.EasingFunction = new CubicEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);
            }
            else//direct addressing
            {
                int SecondRegisterNumber = (cArLine[4] - '0') * 10 + (cArLine[5] - '0');
                #region Get tanimstext_SecondRegister
                TextBlock text_SecondRegister = texts_ToRegister[SecondRegisterNumber];
                text_SecondRegister.Text = (iOperand2 = KSConvert.BinaryToDecimalForRegisters(texts_Registers[SecondRegisterNumber].Text.ToCharArray())).ToString();
                ThicknessAnimation[] tanimstext_SecondRegister = GetAnimationsNumberFromRegister(SecondRegisterNumber, 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                for (int i = 0; i < 3; i++)
                {
                    tanimstext_SecondRegister[i].BeginTime = TimeSpan.FromMilliseconds(i * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                }
                #endregion
                ToPlay.Children.Add(tanimstext_SecondRegister[0]);
                ToPlay.Children.Add(tanimstext_SecondRegister[1]);
                ToPlay.Children.Add(tanimstext_SecondRegister[2]);

                #region Create dtRemovetext_SecondRegister
                DispatcherTimer dtRemovetext_SecondRegister = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtRemovetext_SecondRegister.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_SecondRegister));
                #endregion

                #region Create tanimFirstNumberToALU
                ThicknessAnimation tanimFirstNumberToALU = new ThicknessAnimation();
                tanimFirstNumberToALU.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
                tanimFirstNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimFirstNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimFirstNumberToALU.EasingFunction = new SineEase();
                tanimFirstNumberToALU.BeginTime = TimeSpan.FromMilliseconds(3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimFirstNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimFirstNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimFirstNumberToALU);

                #region Create dtChangeContenttext_ToALU
                DispatcherTimer dtChangeContenttext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20) };
                dtChangeContenttext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iOperand2.ToString()));
                #endregion

                #region Create tanimSecondNumberToALU
                ThicknessAnimation tanimSecondNumberToALU = new ThicknessAnimation();
                tanimSecondNumberToALU.From = new Thickness(gridToALU.ColumnDefinitions[0].ActualWidth - text_ToALU.Width / 2, 0, gridToALU.ColumnDefinitions[1].ActualWidth - text_ToALU.Width / 2, gridToALU.ActualHeight - text_ToALU.Height);
                tanimSecondNumberToALU.To = new Thickness(text_ToALU.Margin.Left, gridToALU.Height - text_ToALU.Height, text_ToALU.Margin.Right, 0);
                tanimSecondNumberToALU.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                tanimSecondNumberToALU.EasingFunction = new SineEase();
                tanimSecondNumberToALU.BeginTime = TimeSpan.FromMilliseconds(4 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 20);
                Storyboard.SetTargetName(tanimSecondNumberToALU, "text_ToALU");
                Storyboard.SetTargetProperty(tanimSecondNumberToALU, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimSecondNumberToALU);

                OldText = text_CMP.Text;

                dtRemovetext_SecondRegister.Start();
                dtChangeContenttext_ToALU.Start();
            }

            cOperand2 = KSConvert.DecimalToBinaryForRegisters(iOperand2);
            char[] cSum = KSConvert.BitwiseORR(cOperand1, cOperand2);
            iSum = KSConvert.BinaryToDecimalForRegisters(cSum);

            #region Show ORR operation in ALU using dtShowSUB
            DispatcherTimer dtShowORR = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            DispatcherTimer dtBackToCMPInfo = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            double OldFontSize = text_CMP.FontSize;
            Action<object, EventArgs> TickEventHandler1 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = iOperand1.ToString() + " AND " + iOperand2.ToString() + " = " + iSum.ToString();
                text_CMP.FontSize *= 0.8;
                (sender as DispatcherTimer).Stop();
            };
            dtShowORR.Tick += new EventHandler(TickEventHandler1);
            Action<object, EventArgs> TickEventHandler2 = (object sender, EventArgs e) =>
            {
                text_CMP.Text = OldText;
                text_CMP.FontSize = OldFontSize;
                (sender as DispatcherTimer).Stop();
            };
            dtBackToCMPInfo.Tick += new EventHandler(TickEventHandler2);
            #endregion

            #region Create dtChangeContentToSum_text_ToALU
            DispatcherTimer dtChangeContentToSum_text_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4) };
            dtChangeContentToSum_text_ToALU.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ToALU, iSum.ToString()));
            #endregion

            #region Create tanimSumToProc
            ThicknessAnimation tanimSumToProc = new ThicknessAnimation();
            tanimSumToProc.From = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height, gridToALU.Width / 2 - text_ToALU.Width / 2, 0);
            tanimSumToProc.To = new Thickness(gridToALU.Width / 2 - text_ToALU.Width / 2, 0, gridToALU.Width / 2 - text_ToALU.Width / 2, gridToALU.Height - text_ToALU.Height);
            tanimSumToProc.Duration = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            tanimSumToProc.EasingFunction = new CubicEase();
            tanimSumToProc.BeginTime = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 4);
            Storyboard.SetTargetName(tanimSumToProc, "text_ToALU");
            Storyboard.SetTargetProperty(tanimSumToProc, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimSumToProc);

            #region Create dtRemovetext_ToALU
            DispatcherTimer dtRemovetext_ToALU = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRemovetext_ToALU.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", text_ToALU));
            #endregion

            #region Initialize text_SumCarrier at the right time in animation sequence
            TextBlock text_ToRegister = texts_ToRegister[MainRegisterNumber];
            if (ParameterRegisterNumber != MainRegisterNumber)
                text_ToRegister.Visibility = Visibility.Collapsed;
            Grid Parentgrid = gridsRegWires[MainRegisterNumber];
            Action<object, EventArgs> TickSetMargin = (object sender, EventArgs e) =>
            {
                text_ToRegister.Margin = new Thickness(Parentgrid.ColumnDefinitions[0].MyWidth() + Parentgrid.ColumnDefinitions[1].MyWidth() - text_ToRegister.Width, (MainRegisterNumber < 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0, 0, (MainRegisterNumber >= 3) ? Parentgrid.RowDefinitions[1].MyHeight() : 0 + Parentgrid.RowDefinitions[2].MyHeight() - text_ToRegister.Height);
                (sender as DispatcherTimer).Stop();
            };
            DispatcherTimer dtSetMargin = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtSetMargin.Tick += new EventHandler(TickSetMargin);
            DispatcherTimer dtRevealtext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
            dtRevealtext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Reveal", texts_ToRegister[MainRegisterNumber]));
            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                Action<object, EventArgs> Addtext_ToRegisterToGrid = (object sender, EventArgs e) =>
                {
                    Parentgrid.Children.Add(text_ToRegister);
                };
                dtRevealtext_SumCarrier.Tick += new EventHandler(Addtext_ToRegisterToGrid);
            }
            #endregion

            #region Get tanimsSumToRegister
            ThicknessAnimation[] tanimsSumToRegister = GetAnimationsNumberToRegister(MainRegisterNumber, iSum.ToString(), 3 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            for (int i = 0; i < 3; i++)
            {
                tanimsSumToRegister[i].BeginTime = TimeSpan.FromMilliseconds((5 + i) * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16);
            }
            #endregion
            ToPlay.Children.Add(tanimsSumToRegister[0]);
            ToPlay.Children.Add(tanimsSumToRegister[1]);
            ToPlay.Children.Add(tanimsSumToRegister[2]);

            if (ParameterRegisterNumber == MainRegisterNumber)
            {
                text_ParameterRegister.Text = iOperand1.ToString();
                DispatcherTimer dtChangeContentToSum = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5 * lookup_ClockSpeedSpec[ClockSpeedSpec] / 16) };
                dtChangeContentToSum.Tick += new EventHandler(KSTimerEvHandlers.GenerateValueChange(text_ParameterRegister, iSum.ToString()));
                dtChangeContentToSum.Start();
            }

            #region Create dtRemovetext_SumCarrier
            DispatcherTimer dtRemovetext_SumCarrier = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(lookup_ClockSpeedSpec[ClockSpeedSpec] / 2) };
            dtRemovetext_SumCarrier.Tick += new EventHandler(KSTimerEvHandlers.Generate("Remove", texts_ToRegister[MainRegisterNumber]));
            #endregion

            ToPlay.Completed += delegate (object sender, EventArgs e)
            {
                texts_Registers[MainRegisterNumber].Text = new string(cSum);
                Fetch();
            };

            ToPlay.Begin(this);
            dtRemovetext_ParameterRegister.Start();
            dtRevealtext_ToALU.Start();
            dtShowORR.Start();
            dtBackToCMPInfo.Start();
            dtChangeContentToSum_text_ToALU.Start();
            dtRemovetext_ToALU.Start();
            dtSetMargin.Start();
            dtRevealtext_SumCarrier.Start();
            dtRemovetext_SumCarrier.Start();
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
            text_CMP.Text = "";
            return;
        }
        #endregion
        #endregion

        #endregion
        #endregion

        #region Secondary Menu
        private void button_SecondaryMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            const int iMillisecondsDuration = 700;
            const int iDelay = 200;
            const int iFadeInDuration = 400;
            Storyboard ToPlay = new Storyboard();
            for (int i = 0; i < tabsDockPanel.Children.Count; i++)
            {
                #region Create tanimMoveToRight
                ThicknessAnimation tanimMoveToRight = new ThicknessAnimation();
                tanimMoveToRight.By = new Thickness(ActualWidth, 0, -1 * ActualWidth, 0);
                tanimMoveToRight.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
                tanimMoveToRight.EasingFunction = new QuarticEase();
                tanimMoveToRight.BeginTime = TimeSpan.FromMilliseconds(0);
                Storyboard.SetTarget(tanimMoveToRight, tabsDockPanel.Children[i]);
                Storyboard.SetTargetProperty(tanimMoveToRight, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimMoveToRight);
            }

            #region Create dtCollapsebutton_SecondaryMenu_Open
            DispatcherTimer dtCollapsebutton_SecondaryMenu_Open = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(iMillisecondsDuration) };
            Action<object, EventArgs> CollapseButton_Tick = (object sender2, EventArgs e2) =>
            {
                button_SecondaryMenu_Open.Visibility = Visibility.Collapsed;
                (sender2 as DispatcherTimer).Stop();
            };
            dtCollapsebutton_SecondaryMenu_Open.Tick += new EventHandler(CollapseButton_Tick);
            #endregion

            #region Create tanimBringInSecMenu
            ThicknessAnimation tanimBringInSecMenu = new ThicknessAnimation();
            tanimBringInSecMenu.By = new Thickness(ActualWidth, 0, -1 * ActualWidth, 0);
            tanimBringInSecMenu.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
            tanimBringInSecMenu.EasingFunction = new QuarticEase();
            tanimBringInSecMenu.BeginTime = TimeSpan.FromMilliseconds(iDelay);
            Storyboard.SetTarget(tanimBringInSecMenu, secmenuGrid);
            Storyboard.SetTargetProperty(tanimBringInSecMenu, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimBringInSecMenu);

            #region Create danimFadeInSecMenu
            DoubleAnimation danimFadeInSecMenu = new DoubleAnimation();
            danimFadeInSecMenu.From = 0;
            danimFadeInSecMenu.To = 1;
            danimFadeInSecMenu.Duration = TimeSpan.FromMilliseconds(iFadeInDuration);
            danimFadeInSecMenu.BeginTime = TimeSpan.FromMilliseconds(iDelay - 10);
            Storyboard.SetTarget(danimFadeInSecMenu, secmenuGrid);
            Storyboard.SetTargetProperty(danimFadeInSecMenu, new PropertyPath(OpacityProperty));
            #endregion
            ToPlay.Children.Add(danimFadeInSecMenu);

            #region Create danimIncrOpacityOfCloseButton
            button_SecondaryMenu_Close.Visibility = Visibility.Visible;
            DoubleAnimation danimIncrOpacityOfCloseButton = new DoubleAnimation();
            danimIncrOpacityOfCloseButton.From = 0;
            danimIncrOpacityOfCloseButton.To = 0.4;
            danimIncrOpacityOfCloseButton.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
            danimIncrOpacityOfCloseButton.BeginTime = TimeSpan.FromMilliseconds(iDelay);
            Storyboard.SetTarget(danimIncrOpacityOfCloseButton, button_SecondaryMenu_Close);
            Storyboard.SetTargetProperty(danimIncrOpacityOfCloseButton, new PropertyPath(OpacityProperty));
            #endregion
            ToPlay.Children.Add(danimIncrOpacityOfCloseButton);

            ToPlay.Begin(this);
            dtCollapsebutton_SecondaryMenu_Open.Start();
            
        }

        private void button_SecondaryMenu_Close_Click(object sender, RoutedEventArgs e)
        {
            const int iMillisecondsDuration = 700;
            const int iDelay = 200;
            const int iFadeInDuration = 400;
            Storyboard ToPlay = new Storyboard();
            for (int i = 0; i < tabsDockPanel.Children.Count; i++)
            {
                #region Create tanimMoveToLeft
                ThicknessAnimation tanimMoveToLeft = new ThicknessAnimation();
                tanimMoveToLeft.By = new Thickness(-1 * ActualWidth, 0, ActualWidth, 0);
                tanimMoveToLeft.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
                tanimMoveToLeft.EasingFunction = new QuarticEase();
                tanimMoveToLeft.BeginTime = TimeSpan.FromMilliseconds(iDelay);
                Storyboard.SetTarget(tanimMoveToLeft, tabsDockPanel.Children[i]);
                Storyboard.SetTargetProperty(tanimMoveToLeft, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimMoveToLeft);
            }

            #region Create dtCollapsebutton_SecondaryMenu_Close
            DispatcherTimer dtCollapsebutton_SecondaryMenu_Close = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(iMillisecondsDuration) };
            Action<object, EventArgs> CollapseButton_Tick = (object sender2, EventArgs e2) =>
            {
                button_SecondaryMenu_Close.Visibility = Visibility.Collapsed;
                (sender2 as DispatcherTimer).Stop();
            };
            dtCollapsebutton_SecondaryMenu_Close.Tick += new EventHandler(CollapseButton_Tick);
            #endregion

            #region Create tanimThrowOutSecMenu
            ThicknessAnimation tanimThrowOutSecMenu = new ThicknessAnimation();
            tanimThrowOutSecMenu.By = new Thickness(-1 * ActualWidth, 0, ActualWidth, 0);
            tanimThrowOutSecMenu.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
            tanimThrowOutSecMenu.EasingFunction = new QuarticEase();
            tanimThrowOutSecMenu.BeginTime = TimeSpan.FromMilliseconds(0);
            Storyboard.SetTarget(tanimThrowOutSecMenu, secmenuGrid);
            Storyboard.SetTargetProperty(tanimThrowOutSecMenu, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimThrowOutSecMenu);

            #region Create danimFadeOutSecMenu
            DoubleAnimation danimFadeOutSecMenu = new DoubleAnimation();
            danimFadeOutSecMenu.From = 1;
            danimFadeOutSecMenu.To = 0;
            danimFadeOutSecMenu.Duration = TimeSpan.FromMilliseconds(iFadeInDuration);
            danimFadeOutSecMenu.BeginTime = TimeSpan.FromMilliseconds(10);
            Storyboard.SetTarget(danimFadeOutSecMenu, secmenuGrid);
            Storyboard.SetTargetProperty(danimFadeOutSecMenu, new PropertyPath(OpacityProperty));
            #endregion
            ToPlay.Children.Add(danimFadeOutSecMenu);

            #region Create danimIncrOpacity
            button_SecondaryMenu_Open.Visibility = Visibility.Visible;
            DoubleAnimation danimIncrOpacity = new DoubleAnimation();
            danimIncrOpacity.From = 0;
            danimIncrOpacity.To = 0.4;
            danimIncrOpacity.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
            danimIncrOpacity.BeginTime = TimeSpan.FromMilliseconds(iDelay);
            Storyboard.SetTarget(danimIncrOpacity, button_SecondaryMenu_Open);
            Storyboard.SetTargetProperty(danimIncrOpacity, new PropertyPath(OpacityProperty));
            #endregion
            ToPlay.Children.Add(danimIncrOpacity);

            ToPlay.Begin(this);
            dtCollapsebutton_SecondaryMenu_Close.Start();
        }

        private void button_SecondaryMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            const int iMillisecondsDuration = 200;
            Storyboard ToPlay = new Storyboard();

            #region Create danimIncrOpacity
            DoubleAnimation danimIncrOpacity = new DoubleAnimation();
            danimIncrOpacity.To = 1;
            danimIncrOpacity.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
            danimIncrOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
            Storyboard.SetTarget(danimIncrOpacity, sender as Button);
            Storyboard.SetTargetProperty(danimIncrOpacity, new PropertyPath(OpacityProperty));
            #endregion
            ToPlay.Children.Add(danimIncrOpacity);

            ToPlay.Begin(this);
        }

        private void button_SecondaryMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            const int iMillisecondsDuration = 200;
            Storyboard ToPlay = new Storyboard();

            #region Create danimDecrOpacity
            DoubleAnimation danimDecrOpacity = new DoubleAnimation();
            danimDecrOpacity.To = 0.4;
            danimDecrOpacity.Duration = TimeSpan.FromMilliseconds(iMillisecondsDuration);
            danimDecrOpacity.BeginTime = TimeSpan.FromMilliseconds(0);
            Storyboard.SetTarget(danimDecrOpacity, sender as Button);
            Storyboard.SetTargetProperty(danimDecrOpacity, new PropertyPath(OpacityProperty));
            #endregion
            ToPlay.Children.Add(danimDecrOpacity);

            ToPlay.Begin(this);
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
            InSecondaryMenu = true;

            Storyboard ToPlay = new Storyboard();
            for (int i = 0; i < shapes_ProcessorParts.Count; i++)
                shapes_ProcessorParts[i].Visibility = Visibility.Collapsed;
            registersStackPanel.Visibility = Visibility.Collapsed;
            rect_MotherBoardBackGround.Visibility = Visibility.Collapsed;
            processorStackPanel.Visibility = Visibility.Collapsed;
            runtimeStackPanel.Visibility = Visibility.Collapsed;
            runtimeDockPanel.Visibility = Visibility.Collapsed;
            runtimestackpanelBorder.Visibility = Visibility.Collapsed;
            memoryDockPanel.Visibility = Visibility.Collapsed;
            tabsDockPanel.Visibility = Visibility.Collapsed;
            for (int i = 0; i < NumRegisters; i++)
                gridsRegWires[i].Visibility = Visibility.Collapsed;
            gridToALU.Visibility = Visibility.Collapsed;
            text_ALU.Visibility = Visibility.Collapsed;
            gridProcToMem.Visibility = Visibility.Collapsed;
            button_QstSave.Visibility = Visibility.Collapsed;
            button_SaveProgress.Visibility = Visibility.Collapsed;
            button_SecondaryMenu_Close.Visibility = Visibility.Collapsed;

            if (curTab != 0)
            {
                myStackPanel.Children.CollapseElements();
                toolsDockPanel.Visibility = Visibility.Collapsed;
            }

            Brush OldBackgroundsecmenu = secmenuGrid.Background;
            secmenuGrid.Background = new SolidColorBrush(Color.FromArgb(215, 130, 75, 0));

            const int MilliDuration = 400;
            const int MilliBeginT = 150;
            Thickness[] InitialMargins = new Thickness[] { button_Quests.Margin, button_Tasks2.Margin, button_Store2.Margin, button_Save2.Margin };
            #region Create tanimButtonToCentre
            ThicknessAnimation tanimButtonToCentre = new ThicknessAnimation();
            tanimButtonToCentre.By = new Thickness(2 * ActualWidth / 5, 0, -2 * ActualWidth / 5, 0);
            tanimButtonToCentre.Duration = TimeSpan.FromMilliseconds(MilliDuration);
            tanimButtonToCentre.EasingFunction = new QuarticEase();
            tanimButtonToCentre.BeginTime = TimeSpan.FromMilliseconds(MilliBeginT);
            Storyboard.SetTarget(tanimButtonToCentre, button_Quests);
            Storyboard.SetTargetProperty(tanimButtonToCentre, new PropertyPath(MarginProperty));
            #endregion
            ToPlay.Children.Add(tanimButtonToCentre);

            #region Create tanims to move all other menu buttons out
            for (int i = 1; i < 4; i++)
            {
                #region Create tanimEverythingElseOut
                ThicknessAnimation tanimEverythingElseOut = new ThicknessAnimation();
                tanimEverythingElseOut.By = new Thickness(ActualWidth, 0, -1 * ActualWidth, 0);
                tanimEverythingElseOut.Duration = TimeSpan.FromMilliseconds(MilliDuration);
                tanimEverythingElseOut.EasingFunction = new CubicEase();
                tanimEverythingElseOut.BeginTime = TimeSpan.FromMilliseconds(MilliBeginT);
                Storyboard.SetTarget(tanimEverythingElseOut, secmenuGrid.Children[i]);
                Storyboard.SetTargetProperty(tanimEverythingElseOut, new PropertyPath(MarginProperty));
                #endregion
                ToPlay.Children.Add(tanimEverythingElseOut);
            }
            #endregion

            #region Add an eventhandler to return state when quests are closed
            int garbage;
            Action RemoveAnimateBackFromClickEventHandlers = new Action(() => garbage = 0);
            RoutedEventHandler AnimateBackButtonsAndBackground = new RoutedEventHandler((object sender2, RoutedEventArgs e2) =>
            {
                Storyboard ToPlayBack = new Storyboard();

                #region Create tanimButtonBack
                ThicknessAnimation tanimButtonBack = new ThicknessAnimation();
                tanimButtonBack.To = InitialMargins[0];
                tanimButtonBack.Duration = TimeSpan.FromMilliseconds(MilliDuration);
                tanimButtonBack.EasingFunction = new QuarticEase();
                tanimButtonBack.BeginTime = TimeSpan.FromMilliseconds(MilliBeginT);
                Storyboard.SetTarget(tanimButtonBack, button_Quests);
                Storyboard.SetTargetProperty(tanimButtonBack, new PropertyPath(MarginProperty));
                #endregion
                ToPlayBack.Children.Add(tanimButtonBack);

                #region Create tanims to move all other menu buttons back
                for (int i = 1; i < 4; i++)
                {
                    #region Create tanimMoveButtonBack
                    ThicknessAnimation tanimMoveButtonBack = new ThicknessAnimation();
                    tanimMoveButtonBack.To = InitialMargins[i];
                    tanimMoveButtonBack.Duration = TimeSpan.FromMilliseconds(MilliDuration);
                    tanimMoveButtonBack.EasingFunction = new CubicEase();
                    tanimMoveButtonBack.BeginTime = TimeSpan.FromMilliseconds(MilliBeginT);
                    Storyboard.SetTarget(tanimMoveButtonBack, secmenuGrid.Children[i]);
                    Storyboard.SetTargetProperty(tanimMoveButtonBack, new PropertyPath(MarginProperty));
                    #endregion
                    ToPlayBack.Children.Add(tanimMoveButtonBack);
                }
                #endregion

                ToPlayBack.Completed += delegate (object o, EventArgs ea)
                {
                    secmenuGrid.Background = OldBackgroundsecmenu;
                };

                ToPlayBack.Begin(this);
                RemoveAnimateBackFromClickEventHandlers();
            });
            RemoveAnimateBackFromClickEventHandlers = new Action(() => button_Quests.Click -= AnimateBackButtonsAndBackground);
            button_Quests.Click += AnimateBackButtonsAndBackground;
            #endregion

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
                    button_Redeem.Click += button_Redeem_Click;
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
            button_Quests.Click += button_Back_Quests_Click_Close;
            RoutedEventHandler ChangeToOldBackground_Click = new RoutedEventHandler((object sender2, RoutedEventArgs e2) => { });
            ChangeToOldBackground_Click = new RoutedEventHandler((object sender2, RoutedEventArgs e2) =>
            {
                button_Quests.Click -= ChangeToOldBackground_Click;
            });
            button_Quests.Click += ChangeToOldBackground_Click;
            ToPlay.Begin(this);
        }

        /// <summary>
        /// MUST BE TESTED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Redeem_Click(object sender, RoutedEventArgs e)
        {
            DockPanel Parent = (sender as Button).Parent as DockPanel;
            int index = (myStackPanel.Children[myStackPanel.Children.Count - 1] as StackPanel).Children.IndexOf(Parent);
            listQuestsStatus[index] = 2;
            Parent.Children.Remove(sender as Button);
            Parent.Children.Add(new TextBlock() { Text = "Redeemed " + lookup_Quests[index].Item2 + " portions!", Width = Parent.ActualWidth / 2, Height = Parent.ActualHeight, Background = new LinearGradientBrush(Color.FromArgb(255, 0, 200, 143), Color.FromArgb(255, 0, 162, 143), 90), Visibility = Visibility.Visible });
        }

        private void button_Back_Quests_Click_Close(object sender, RoutedEventArgs e)
        {
            InSecondaryMenu = false;

            Storyboard ToPlay = new Storyboard();
            //CLOSE QUESTS
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            myStackPanel.Background = myBrushes["myStackPanel.DefaultBackground"];
            myStackPanel.Visibility = Visibility.Collapsed;
            myGrid.Background = myBrushes["myGrid.DefaultBackground"];

            button_SecondaryMenu_Close.Visibility = Visibility.Visible;
            GraphicsForMotherBoard();
            curTab = 0;
            tabsDockPanel.Visibility = Visibility.Visible;

            button_Quests.Click -= button_Back_Quests_Click_Close;
            button_Quests.Click += button_Quests_Click_Open;

            const int UserChoiceDelayMilli = 2500;
            #region Create dtBackToTabs
            DispatcherTimer dtBackToTabs = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(UserChoiceDelayMilli) };
            EventHandler triggermenuchange = new EventHandler((object sender2, EventArgs e2) =>
            {
                if (!InSecondaryMenu)
                    button_SecondaryMenu_Close_Click(button_SecondaryMenu_Close, e);
                (sender2 as DispatcherTimer).Stop();
            });
            dtBackToTabs.Tick += triggermenuchange;
            #endregion
            
            ToPlay.Completed += delegate(object sender2, EventArgs e2)
            {
                dtBackToTabs.Start();
            };
            ToPlay.Begin(this);
            dtBackToTabs.Start();
        }

        private void button_SaveProgress_Click(object sender, RoutedEventArgs e)
        {
            BinaryWriter binaryWrite = new BinaryWriter(new FileStream(System.IO.Path.Combine(sGameFilesPath, sUsername + sAccountFileName), FileMode.Truncate), Encoding.UTF8);
            string[][] TabInfo = new string[2][];//[0] - tab names, [1] - tab texts
            TabInfo[0] = new string[texts_TabNames.Count]; TabInfo[1] = new string[texts_Tabs.Count];
            for (int i = 0; i < texts_Tabs.Count; i++)
            {
                TabInfo[0][i] = texts_TabNames[i].Text;
                TabInfo[1][i] = texts_Tabs[i].Text;
            }
            KSFileManagement.SaveProgress(binaryWrite, texts_Tabs.Count, TabInfo[0], TabInfo[1], NumRegisters, ALUSpec, ClockSpeedSpec, MemorySpec);
        }
        #endregion
        
        #region Tools Dockpanel
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
            tabsDockPanel.Children.RemoveAt(curTab);
            tabsDockPanel.Children[tabsDockPanel.Children.Count - 1].Visibility = Visibility.Visible;   //make visible the 'add new tab' button
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
            tabsDockPanel.Visibility = Visibility.Collapsed;
            (sender as Button).Click -= DockButton_Click_CodeManual_Open;
            (sender as Button).Click += DockButton_Click_CodeManual_Close;
        }

        private void DockButton_Click_CodeManual_Close(object sender, RoutedEventArgs e)
        {
            myStackPanel.Children.RemoveAt(myStackPanel.Children.Count - 1);
            texts_TabNames[curTab - 1].Visibility = Visibility.Visible;
            texts_Tabs[curTab - 1].Visibility = Visibility.Visible;
            tabsDockPanel.Visibility = Visibility.Visible;
            toolsDockPanel.Visibility = Visibility.Visible;
            (sender as Button).Click -= DockButton_Click_CodeManual_Close;
            (sender as Button).Click += DockButton_Click_CodeManual_Open;
        }
        #endregion

        #region Miscellaneous
        private void MainWindow_SizeChanged_ResizeElements(object sender, SizeChangedEventArgs e)
        {
            double AmountStickingOut = canvas_LoginDetails_Username.Width - 4 * myGrid.ColumnDefinitions[4].ActualWidth;
            canvas_LoginDetails_Username.Margin = new Thickness(myGrid.ColumnDefinitions[3].ActualWidth - AmountStickingOut / 2, 0, myGrid.ColumnDefinitions[3].ActualWidth - AmountStickingOut / 2, myGrid.RowDefinitions[3].ActualHeight - canvas_LoginDetails_Username.Height);
            canvas_LoginDetails_Password.Margin = new Thickness(canvas_LoginDetails_Username.Margin.Left, canvas_LoginDetails_Username.Margin.Bottom, canvas_LoginDetails_Username.Margin.Right, 0);

            Grid.SetColumn(text_LoginDetails_FormTitle, 5);
            text_LoginDetails_FormTitle.Margin = new Thickness(0, 24 * ActualHeight / 322, 0, 0);
            text_LoginDetails_FormTitle.Width = 2 * myGrid.ColumnDefinitions[4].ActualWidth;
            text_LoginDetails_FormTitle.Height = 26 * ActualHeight / 322;
            text_LoginDetails_FormTitle.TextAlignment = TextAlignment.Center;

            text_NoAccountFound.Width = 2.7 * myGrid.ColumnDefinitions[4].ActualWidth;
            text_NoAccountFound.Height = 26 * ActualHeight / 322;
            text_NoAccountFound.FontSize = ActualWidth / 100;
            text_NoAccountFound.Margin = new Thickness(0.65 * ActualWidth / 14, 17 * ActualHeight / 322, 0.65 * ActualWidth / 14, 8 * ActualHeight / 322);
            
            button_Go.Width = myGrid.ColumnDefinitions[5].ActualWidth * 3;
            button_Go.Height = myGrid.RowDefinitions[3].ActualHeight * 0.5;
            button_Go.Margin = new Thickness(myGrid.ColumnDefinitions[5].ActualWidth / 2, myGrid.RowDefinitions[5].ActualHeight * 0.5, myGrid.ColumnDefinitions[5].ActualWidth / 2, 0);

            button_NewAccount.Width = myGrid.ColumnDefinitions[5].ActualWidth * 2.5;
            button_NewAccount.Height = myGrid.RowDefinitions[3].ActualHeight * 0.4;
            button_NewAccount.Margin = new Thickness(myGrid.ColumnDefinitions[5].ActualWidth * 0.75, myGrid.RowDefinitions[3].ActualHeight * 0.2, myGrid.ColumnDefinitions[5].ActualWidth * 0.75, myGrid.RowDefinitions[3].ActualHeight * 0.4);

            myStackPanel.Width = ActualWidth * 6 / 7;
            myStackPanel.Height = ActualHeight * 5 / 6;
            toolsDockPanel.Width = 3.2 * (toolsDockPanel.Height = ActualHeight / 10);
            button_CodeManual.Height = button_LoadIntoMem.Height = button_DeleteTab.Height = toolsDockPanel.Height;
            button_CodeManual.Width = toolsDockPanel.Width * 0.375;
            button_LoadIntoMem.Width = button_DeleteTab.Width = toolsDockPanel.Width * 5 / 16;
            button_DeleteTab.FontSize = button_DeleteTab.Width / 4;
            tabsDockPanel.Width = ActualWidth;
            secmenuGrid.Width = tabsDockPanel.Width;
            secmenuGrid.Margin = new Thickness(-ActualWidth, secmenuGrid.Margin.Top, ActualWidth, secmenuGrid.Margin.Bottom);
            if (tabsDockPanel.Children.Count > 0)
            {
                (tabsDockPanel.Children[0] as Button).Width = ActualWidth / 7;
                for (int i = 1; i < tabsDockPanel.Children.Count; i++)
                    (tabsDockPanel.Children[i] as Button).Width = ActualWidth / 14;
            }
            button_SecondaryMenu_Open.Width = button_SecondaryMenu_Close.Width = 20;
            button_SecondaryMenu_Open.Margin = new Thickness(0, 0, ActualWidth - 35, 0);
            button_SecondaryMenu_Close.Margin = new Thickness(ActualWidth - 35, 0, 0, 0);
            if (button_SecondaryMenu_Open.Height >= myGrid.RowDefinitions[0].MyHeight())
                button_SecondaryMenu_Open.Height = button_SecondaryMenu_Close.Height = myGrid.RowDefinitions[0].MyHeight();
            //for (int curChild = 0; curChild < 4; curChild++)
            //{
            //    (secmenuGrid.Children[curChild] as Button).Margin = new Thickness(4 * secmenuGrid.ColumnDefinitions[curChild].MyWidth() / 103, 0, 4 * secmenuGrid.ColumnDefinitions[curChild].MyWidth() / 103, 0);
            //    secmenuRects[curChild].Width = (secmenuGrid.Children[curChild] as Button).Width = 95 * secmenuGrid.ColumnDefinitions[curChild].MyWidth() / 103;
            //}

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
            stackpanel_CMP.Width = 0.5 * text_ALU.Width;
            stackpanel_CMP.Height = 0.15 * text_ALU.Height;
            if (text_CMP != null)
            {
                text_CMP.Width = stackpanel_CMP.Width;
                text_CMP.Height = stackpanel_CMP.Height;
                text_CMP.FontSize = Math.Min(text_CMP.Width * 2.3, text_CMP.Height * 0.9);
            }

            gridProcToMem.Width = rect_AddressBusWire.Width = rect_DataBusWire.Width = ActualWidth / 14;
            gridProcToMem.Height = ActualHeight * 102 / 322;
            
            int button_QstSave_Width = 5;
            button_QstSave.Height = ActualHeight * 44 / 322;
            button_QstSave.Width = button_QstSave_Width;
            button_QstSave.Margin = new Thickness(ActualWidth / 14 - button_QstSave_Width, 0, 0, 0);
        }
        #endregion
    }
}
