using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace EagleFitLogin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    //;000652482=9002? string read in from swiped ID card

    public partial class MainWindow : Window
    {
        private const int _lengthOfMemberID = 8;
        private const int SECOND = 1000;
        private const int MINUTE = 60000;
        private Timer clock = new Timer();
        //private Timer readyTimeout = new Timer();
        //private Timer memberIdTimer = new Timer();
        //private Timer infoTimeout = new Timer();
        //private Timer archiveTrigger = new Timer();
        
        private DBHandler data;
        private CardSwipeHandler card;
        private LoginLogoutHandler logging;
        private DatabaseSwitcher switcher;

        private DispatcherTimer Timer = new DispatcherTimer();

        //*******
        bool delay = false;
        //********
        bool isTimedOut_Ready = false;
        bool isTimedOut_Info = false;
        bool isFailure = false;
        bool isLogout = false;
        bool isCredit = false;
        bool isCancel = true;
        bool blink = false;
        bool isArchiving = false;
        string memberID = "";
        private string readyToSwipe = "Ready To Swipe Card";
        private string idleMessage = "System Is Idle";
        private string memberName;
        private DbList dbData;
        bool isGroupEx;
        

        public MainWindow()
        {

            ClassSelection cs = new ClassSelection();
            bool? dialogResult = cs.ShowDialog();
            switch (dialogResult)
            {
                case true:
                    //MessageBox.Show("Fast Fitness Selected!");
                    isGroupEx = false;
                    break;
                case false:
                    //MessageBox.Show("Group Exercise Selected!");
                    isGroupEx = true;
                    break;
            }
            data = new DBHandler();
            logging = new LoginLogoutHandler(data);
            switcher = new DatabaseSwitcher(data, ".myMemberSerial");
            memberName = idleMessage;
            double width = SystemParameters.PrimaryScreenWidth;
            double height = SystemParameters.PrimaryScreenHeight;
            
            if (height <= 600 || width <= 800)
            {
                //InitializeComponent2();// small
            }
            else
            {
                InitializeComponent();//large
            }

            card = new CardSwipeHandler();
            txtBx_MemberID.KeyDown += new KeyEventHandler(txtBx_MemberID_KeyDown);

            Timer.Tick += new EventHandler(Timer_Click);
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();

            clearInfo();
            txtBx_MemberID.Focus();

            dbData = switcher.openDatabaseData();
            switcher.openDatabase(1, "localhost", "blah");

            
        }


        

        private void Timer_Click(object sender, EventArgs e)
        {
            DateTime d;
            d = DateTime.Now;
            txtBlk_Clock.Text = d.ToString("hh:mm:ss tt");
        }



        private void txtBx_MemberID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string inputMemberID = card.HandleInput(txtBx_MemberID.Text);
                if (inputMemberID != string.Empty)
                {
                    txtBx_MemberID.Text = inputMemberID.ToString();
                    PerformLogging(inputMemberID);
                }
                else
                {
                    MessageBox.Show("Invalid ID");
                }
                txtBx_MemberID.MaxLength = _lengthOfMemberID;
            }
            else if (e.Key == Key.Oem1)
            {
                txtBx_MemberID.Text = "";
                txtBx_MemberID.MaxLength = _lengthOfMemberID + 2;
            }
            else
            {
                if (e.Key < Key.D0 || e.Key > Key.D9)
                {
                    if (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                    {
                        int isNumber = 0;
                        e.Handled = !int.TryParse(e.Key.ToString(), out isNumber);
                    }
                }
            }
        }



        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            string inputMemberID = card.HandleInput(txtBx_MemberID.Text);
            if (inputMemberID != string.Empty)
            {
                txtBx_MemberID.Text = inputMemberID.ToString();
                PerformLogging(inputMemberID);
            }
            else
            {
                MessageBox.Show("Invalid ID");
            }
        }



        public void resetState()
        {
            isTimedOut_Ready = true;
            isTimedOut_Info = false;
            isFailure = false;
            isLogout = false;
            isCredit = false;
            isCancel = true;
            blink = false;
            data = new DBHandler();
            logging = new LoginLogoutHandler(data);
            card = new CardSwipeHandler();
        }


        private void PerformLogging(string memberID)
        {
            
                
                int status;
                if (logging.getDataReady(memberID))
                {
                    //MessageBox.Show("2");
                    memberName = logging.memberName;
                    isCancel = false;
                    if (logging.isLoggedIn(memberID))
                    {

                        isLogout = false;
                        isCancel = true;// for display prior
                        fillMemberData(memberID);
                        isCancel = false;// ahh all better
                        if (true)
                        {
                            status = logging.isWarnToContinue();// once had params
                            handleCancelLogin(status);
                        }
                    }
                    if (!isCancel)
                    {
                        if (isGroupEx)//logging.isCredit())
                        {
                            //isGroupEx = true;
                            if (logging.isLoggedIn(memberID))
                            {
                                logging.logoutGroupExMember(memberID, logging.thisVisitsValue);//getVisitValue(memberID));
                                isLogout = true;
                            }
                            else
                            {
                                logging.loginGroupExMember(memberID);
                                isLogout = false;
                            }
                        }
                        else
                        {
                            //isGroupEx = false;
                            if (logging.isLoggedIn(memberID))
                            {
                                logging.logoutFastFitnessMember(memberID, logging.thisVisitsValue);
                                isLogout = true;
                            }
                            else
                            {
                                logging.loginFastFitnessMember(memberID);
                                isLogout = false;
                            }
                        }
                    }
                
                else
                {
                    isFailure = true;
                    isCancel = false;
                }

                if (!isFailure)
                {
                    fillMemberData(memberID);

                }
                else
                {
                    clearInfo();
                }
                prepPaintTimers();
            }// end is archive
                
                //clearInfo();
                resetState();
        }


       
        // works but not integrated yet
        private void handleCancelLogin(int status)
        {
            if (status == 0 || status == 1)
            {
                txtBx_MemberID.Text = "";

                if (MessageBox.Show("\n\nWould you really want to log out?\n", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    isCancel = false;
                    isLogout = true;
                }
                else
                {
                    isCancel = true;
                }
            }
            else
            {
                isCancel = false;
            }
        }
      

        private void prepPaintTimers()
        {
            isTimedOut_Ready = false;
            isTimedOut_Info = false;
            //readyTimeout.Stop();
            //infoTimeout.Stop();
            //readyTimeout.Start();
            //infoTimeout.Start();
            //memberIdTimer.Stop();
            //memberIdTimer.Start();
            //this.Invalidate(true);
        }
       
        private void clearInfo()
        {
            txtblk_TimeInValue.Text = "";
            txtblk_TimeOutValue.Text = "";
            txtblk_WorkoutLengthValue.Text = "";
            txtblk_TotalVisitsValue.Text = "";
            txtblk_MemberName.Text = readyToSwipe;
            txtblk_MemberID.Text = idleMessage;
            txtblk_MemberResult.Text = "";
        }


        // this is messy because of numerous edits... but it works fine.
        private void fillMemberData(string memberID)
        {
            memberName = logging.memberName;
            if (isCancel)
            {
                txtblk_TimeInValue.Text = logging.loginTime.ToString("hh:mm:ss tt");
                txtblk_TimeOutValue.Text = "";
                txtblk_WorkoutLengthValue.Text = logging.workoutLength.ToString() + " minutes";
                txtblk_MemberName.Text = "" + logging.memberName;
                txtblk_MemberID.Text = "" + memberID;
                txtblk_MemberResult.Text = "Logout Canceled";
                txtblk_TotalVisitsValue.Text = logging.visits.ToString();

            }
            else if (isLogout)
            {
                txtblk_TimeInValue.Text = logging.loginTime.ToString("hh:mm:ss tt");
                txtblk_TimeOutValue.Text = DateTime.Now.ToString("hh:mm:ss tt");
                txtblk_WorkoutLengthValue.Text = logging.workoutLength.ToString() + " minutes";
                txtblk_MemberName.Text = "" + logging.memberName;
                txtblk_MemberID.Text = "" + memberID;
                txtblk_MemberResult.Text = "Logout Successful";
                txtblk_TotalVisitsValue.Text = "" + (logging.visits + logging.thisVisitsValue);
                
            }
            else
            {
                txtblk_TimeInValue.Text = DateTime.Now.ToString("hh:mm:ss tt");
                txtblk_TimeOutValue.Text = "";
                txtblk_WorkoutLengthValue.Text = "";
                txtblk_MemberName.Text = "" + logging.memberName;
                txtblk_MemberID.Text = "" + memberID;
                txtblk_MemberResult.Text = "Login Successful";
                txtblk_TotalVisitsValue.Text = "" + (logging.visits + logging.thisVisitsValue);
                
            }
            lbox_AdditionalActivity.DataContext = logging.activities;
        }
    }
}
