using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows;

using MySql.Data.MySqlClient;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Data.SqlClient;

namespace EagleFitLogin
{
    
    public class DBHandler
    {
        private String dbIP;
        private String dbName;
        private String dbUserName;
        private String dbPassword;
        private String connectionString;
        private MySqlConnection conn;
        private DateTime sdate;
        private DateTime edate;


        //
        //default contstructor
        //
        public DBHandler()
        {
            this.dbIP = "146.187.135.45";
            this.dbName = "EagleFit";
            this.dbUserName = "root";
            this.dbPassword = "#32-jDs7e*Q";

            connectToMySql();
        }

        //
        //constructor
        public DBHandler(String dbIP, String dbName, String dbUserName, String dbPassword)
        {
            this.dbIP = dbIP;
            this.dbName = dbName;
            this.dbUserName = dbUserName;
            this.dbPassword = dbPassword;
        }

        //
        //gets and sets
        //
        public String iP
        {
            get
            {
                return this.dbIP;
            }
            set
            {
                this.dbIP = value;
            }
        }

        public String nameOfDb
        {
            get
            {
                return this.dbName;
            }
            set
            {
                this.dbName = value;
            }
        }

        public String userName
        {
            get
            {
                return this.dbUserName;
            }
            set
            {
                this.dbUserName = value;
            }
        }

        public String pwd
        {
            get
            {
                return this.dbPassword;
            }
            set
            {
                this.dbPassword = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return this.sdate;
            }
            set
            {
                this.sdate = value;
            }
        }

        public DateTime EndDate
        {
            get
            {
                return this.edate;
            }
            set
            {
                this.edate = value;
            }
        }

        /*
         * Author: Russ Utt
         * Method Name: connectToMySql
         * Parameters: none
         * Output: void
         * Exception: MySqlException
         * Description: This method makes a connection string based on the 
         * object's values and attempts to make a connection to the database
         */
        public void connectToMySql()
        {
            String connectS = "server=" + this.dbIP + ";uid=" + this.dbUserName + ";pwd=" + this.dbPassword + ";database=" + this.dbName + ";";
            //this.connectionString = string.Format("server={0}; user id={1}; password={2}; database={3}; pooling=false", this.dbIP, this.dbUserName, this.dbPassword, this.dbName);

            try
            {
                conn = new MySqlConnection(connectS);

                conn.Open();
                //MessageBox.Show("inside connection try");
            }
            catch (MySqlException e)
            {
                MessageBox.Show("Error connecting to the server:" + e.Message + "\r\n\r\nMake sure your settings are correct.");
            }
        }//end connectToMySql

        /*
         * Author: Russ Utt
         * Method Name: disconnectMySql
         * Parameters: none
         * Output: void
         * Exception: MySqlException
         * Description: disconnects the connection to the database
         */
        private void disconnectMySql()
        {
            conn.Close();
        }//end disconnectMySql



        /*
         * Author: Russ Utt
         * Method Name: isSettingsCorrect
         * Parameters: a connection string
         * Output: bool (true if it is valid, false if not)
         * Exception: MySqlException
         * Description: This method test a possible connection string and 
         * verifys it is valid but trying to connect to the database
         */
        public bool isSettingsConnect(String connectionString)
        {
            bool connect = false;

            try
            {
                conn = new MySqlConnection(connectionString);
                conn.Open();
                connect = true;
            }
            catch (MySqlException)
            {
                connect = false;
            }
            finally
            {
                disconnectMySql();
            }
            return connect;
        }


       

       

        /*
         * Author: Russ Utt
         * Method Name: getMemberInfo
         * Parameters: member's id
         * Output: DataTable
         * Exception: MySqlException
         * Description: This method gets all information about a particular member
         */
        public DataTable getMemberInfo(String memID)
        {
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter("SELECT First_Name, Last_Name, FastFitness_totalVisits, GroupEx_totalVisits FROM Student_Info WHERE ID = '" + memID.Replace("'", "''") + "' ", conn);
                dataAdapter.Fill(dt);
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return dt;
        }//end getMemberInfo

        /*
         * Author: Russ Utt
         * Method Name: doesMemberExist
         * Parameters: member's id
         * Output: bool
         * Exception: MySqlException
         * Description: This method will check to see if an unarchived member with this particular member id exists in the
         * members table
         */
        public bool doesMemberExist(String memID)
        {
            DataTable dt = new DataTable();
            bool exists = false;
            connectToMySql();

            try
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter("SELECT * FROM Student_Info WHERE ID = '" + memID.Replace("'", "''") + "' ", conn);
                dataAdapter.Fill(dt);
                if (dt.Rows.Count == 1)
                {
                    exists = true;
                }
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return exists;
        }//doesMemberExist

        /*
         * Author: Russ Utt
         * Method Name: getLatestLoggingEntry
         * Parameters: member's id
         * Output: DataTable
         * Exception: MySqlException
         * Description: This method gets the latest visit entry for a particular user
         */
        public DataTable getLatestLoggingEntry(String memID)
        {
            int visitID = getMaxVisitID(memID);
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter("SELECT time_in, time_out, value FROM visits WHERE visit_id = " + visitID, conn);
                dataAdapter.Fill(dt);
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return dt;

        }


        //returns next available visit number
        public int getMaxVisit()
        {
            int visitID = 0;
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter("SELECT MAX(Visit_id) FROM FastFitness_Visits ", conn);
                dataAdapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    DataRow myRow = dt.Rows[0];
                    try
                    {
                        visitID = Int32.Parse(myRow[0].ToString());
                    }
                    catch (FormatException)
                    {
                        visitID = -1;
                    }

                }
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            visitID++;
            return visitID;
        }//getMaxVisit

        /*
        * Author: Joshua Montgomery
        * Method Name: getLatestTimein
        * Parameters: String 
        * Output: DateTime
        * Exception: Exception
        * Description:  gets the logout time of the latest
        */
        public DateTime getLatestTimeIn(String memberID)
        {
            MySqlDataReader reader;
            int visitID = getMaxVisitID(memberID);
            DateTime time_in = new DateTime();
            time_in = DateTime.MinValue;
            connectToMySql();
            String command = "SELECT Time_In, Time_Out FROM FastFitness_Visits WHERE visit_id = " + visitID;
            MySqlCommand c = new MySqlCommand(command, conn);
            reader = c.ExecuteReader();

            try
            {
                if (reader.Read())
                {
                    time_in = reader.GetDateTime(0);
                }
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
            finally
            {
                reader.Close();
                disconnectMySql();
            }
            return time_in;

        }

        /*
        * Author: Joshua Montgomery
        * Method Name: getLatestTimeOut
        * Parameters: String 
        * Output: DateTime
        * Exception: Exception
        * Description:  gets the logout time of the latest
        */
        public DateTime getLatestTimeOut(String memberID)
        {
            MySqlDataReader reader;
            int visitID = getMaxVisitID(memberID);
            DateTime time_out = new DateTime();
            time_out = DateTime.MinValue;
            connectToMySql();
            String command = "SELECT time_in, time_out FROM FastFitness_Visits WHERE visit_id = " + visitID;
            MySqlCommand c = new MySqlCommand(command, conn);
            reader = c.ExecuteReader();
            try
            {
                if (reader.Read())
                {
                    time_out = reader.GetDateTime(1);
                }
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
            finally
            {
                reader.Close();
                disconnectMySql();
            }
            return time_out;

        }



        /*
       * Author: Joshua Montgomery
       * Method Name: getTodaysVisitCount
       * Parameters: String 
       * Output: int
       * Exception: Exception
       * Description:  gets the total of earned visits for the current day
       */
        public int getTodaysVisitCount(String memberID)
        {
            MySqlDataReader reader;

            connectToMySql();

           
            
            String command = "SELECT Value FROM FastFitness_Visits WHERE ID = '" + memberID + "' AND time_in > curdate();";
            MySqlCommand c = new MySqlCommand(command, conn);
            reader = c.ExecuteReader();
            int value = 0;
            try
            {
                
                while (reader.Read())
                {
                    value += reader.GetInt32(0);
                }
                 
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                reader.Close();
                disconnectMySql();
            }
            return value;
        }

        /*
         * Author: Russ Utt
         * Method Name: getMaxVisitID
         * Parameters: member's id
         * Output: int
         * Exception: MySqlException
         * Description: This method gets the latest visit_id for a particular member
         */
        public int getMaxVisitID(String memID)
        {
            int visitID = 0;
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter("SELECT MAX(Visit_id) FROM FastFitness_Visits WHERE ID = '" + memID.Replace("'", "''") + "' ", conn);
                dataAdapter.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    DataRow myRow = dt.Rows[0];
                    try
                    {
                        visitID = Int32.Parse(myRow[0].ToString());
                    }
                    catch (FormatException)
                    {
                        visitID = -1;
                    }

                }
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return visitID;
        }//getMaxVisitID

        /*
        * Author: Russ Utt
        * Method Name: updateVisitTotal
        * Parameters: member id, value
        * Output: bool
        * Exception: MySqlException
        * Description: This method updates a particular member's total_current_visits by a certain value.
        */
        public void updateVisitTotal(String memID, int value)
        {
            connectToMySql();

            try
            {
                MySqlCommand command = new MySqlCommand("UPDATE Student_Info SET FastFitness_totalVisits = (FastFitness_totalVisits + " + value + " ) WHERE ID = '" + memID.Replace("'", "''") + "'", conn);
                command.ExecuteNonQuery();
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
        }

       

        /*
         * Author: Russ Utt
         * Method Name: isCreditMember
         * Parameters: member's id
         * Output: bool
         * Exception: MySqlException
         * Description: This method checks to see if a particual member is a credit member
         */
        public bool isCreditMember(String memID)
        {
            return true;
            /*
            bool isCreditMember = false;
            String categoryType = "";
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT cat_name FROM members WHERE member_id = '" + memID.Replace("'", "''") + "'", conn);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    DataRow myRow = dt.Rows[0];
                    categoryType = myRow[0].ToString();
                }
                if (categoryType == "credit")
                {
                    isCreditMember = true;
                }
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return isCreditMember;
             * */
        }//end isCreditMember

        /*
         * Author: Russ Utt
         * Method Name: closeFailedLogoutAndGiveVisitValue
         * Parameters: member's id
         * Output: bool
         * Exception: MySqlException
         * Description: This method closes out visits when a member forgets to logout.  It needs to check if the member is 
         * a credit member or not, if they are a credit member they get a zero visit, if they are not a credit member they get 
         * a visit
         */
        public bool closeFailedLogoutAndGiveVisitValue(String memID)
        {
            bool success = false;
            bool isCredit = isCreditMember(memID);
            int visitID = getMaxVisitID(memID);

            connectToMySql();

            try
            {
                if (isCredit)
                {
                    MySqlCommand command = new MySqlCommand("UPDATE visits SET time_out = '" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " 23:59:59', value = 0 WHERE member_id = '" + memID.Replace("'", "''") + "' AND visit_id = " + visitID, conn);
                    command.ExecuteNonQuery();
                    success = true;
                }
                else
                {
                    MySqlCommand command = new MySqlCommand("UPDATE visits SET time_out = '" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " 23:59:59', value = 1 WHERE member_id = '" + memID.Replace("'", "''") + "' AND visit_id = " + visitID, conn);
                    command.ExecuteNonQuery();
                    success = true;
                }

            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            updateVisitTotal(memID, 1);  //update the member's total_current_visits
            return success;
        }//closeFailedLogoutAndGiveVisitValue

        /*
         * Author: Russ Utt
         * Method Name: loginMember
         * Parameters: member id
         * Output: bool
         * Exception: MySqlException
         * Description: This method inserts a row into the visits table.  It puts '0001-01-01 00:00:00' in for the logout
         * (which will later be used to check for members who forgot to logout)
         */
        public bool groupExLogin(String memID)
        {
            bool success = false;
            String loginTime;
            loginTime = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;

            connectToMySql();

            try
            {
                MySqlCommand command = new MySqlCommand("INSERT INTO visits VALUES(null, '" + memID.Replace("'", "''") + "', '" + loginTime.Replace("'", "''") + "', '0001-01-01 00:00:00', 0, 0)", conn);
                command.ExecuteNonQuery();
                success = true;
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return success;
        }//end loginMember

        /*
         * Author: Russ Utt
         * Method Name: logoutMember
         * Parameters: member id, value
         * Output: void
         * Exception: MySqlException
         * Description: This method is used when a member logs out, it updates the time_out to the current time
         * and updates the value appropriatley
         */
        public bool groupExLogout(String memID, int value)
        {
            bool success = false;
            String logoutTime;
            logoutTime = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            int visitID = getMaxVisitID(memID);

            connectToMySql();

            try
            {
                MySqlCommand command = new MySqlCommand("UPDATE visits SET time_out = '" + logoutTime.Replace("'", "''") + "', value = " + value + " WHERE visit_id = " + visitID, conn);
                command.ExecuteNonQuery();
                success = true;
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            updateVisitTotal(memID, value); //update the member's total_current_visits
            return success;
        }//end logoutMember

        /*
         * Author: Jeremy Reineman
         * Method Name: fastFitnessLogin
         * Parameters: member id
         * Output: bool
         * Exception: MySqlException
         * Description: This method will insert a row into the fastfitness_visits table when a fastfitness member logins. 
         * 
         */
        public bool fastFitnessLogin(String memID)
        {
            int maxVisit = getMaxVisit();
            bool success = false;
            String loginTime;
            loginTime = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;

            connectToMySql();

            try
            {
                MySqlCommand command = new MySqlCommand("INSERT INTO FastFitness_Visits VALUES(  " + maxVisit + "  , '" + memID.Replace("'", "''") + "', '" + loginTime.Replace("'", "''") + "', '0001-01-01 00:00:00', " + 0 + ")", conn);
                command.ExecuteNonQuery();
                success = true;
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }

            return success;
        }

        /*
        * Author: Jeremy Reineman
        * Method Name: fastFitnessLogout
        * Parameters: member id
        * Output: bool
        * Exception: MySqlException
        * Description: This method logs out a fast fitness member.  
        */
        public bool fastFitnessLogout(String memID, int value)
        {
            bool success = false;
            String logoutTime;
            logoutTime = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            int visitID = getMaxVisitID(memID);

            connectToMySql();

            try
            {
                MySqlCommand command = new MySqlCommand("UPDATE FastFitness_Visits SET Time_Out = '" + logoutTime.Replace("'", "''") + "' , Value = " + value + " WHERE visit_id = " + visitID, conn);
                command.ExecuteNonQuery();
                success = true;
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            updateVisitTotal(memID, value);
            return success;
        }


        /*
         * Author: Russ Utt
         * Method Name: getZeroVisits
         * Parameters: member id
         * Output: DataTable
         * Exception: MySqlException
         * Description: This method gets all the zero visits for a particular member (checks to see if a member
         * has any visits that still have a time_out = '0001-01-01 00:00:00'
         */
        public DataTable getZeroVisits(String memID)
        {
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT Time_in, Time_out FROM FastFitness_Visits WHERE Time_Out = '0001-01-01 00:00:00' AND ID = '" + memID.Replace("'", "''") + "'  AND Time_In < curdate() ORDER BY Time_In DESC", conn);
                da.Fill(dt);
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return dt;
        }

        

        

        /*
         * Author: Russ Utt
         * Method Name: getFailedLogouts
         * Parameters: none
         * Output: DataTable
         * Exception: MySqlException
         * Description: This method gets a list of all "unarchived" missed logouts (where time_out =
         * '0001-01-01 00:00:00'
         */
        public DataTable getFailedLogouts()
        {
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT visit_id, member_id, cat_name FROM visits NATURAL JOIN members WHERE time_out = '0001-01-01 00:00:00' AND is_archived = 0", conn);
                da.Fill(dt);
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return dt;
        }//end getFailedLogouts

        /*
         * Author: Russ Utt
         * Method Name: getActivityID
         * Parameters: description of the activity
         * Output: void
         * Exception: MySqlException
         * Description: This will check for all un-archived failed logouts (anything with a logout
         * time of 0001-01-01 00:00:00) and closes them and gives a visit value of 1
         * if they are a credit member and a 0 if they are a credit member, I'm giving
         * a time_out of the current date and time of 23:59:59 */
        public void closeAllFailedLogouts()
        {
            DataTable dt = getFailedLogouts();
            if (dt.Rows.Count > 0)
            {
                connectToMySql();
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        if (dr[2].ToString() == "non-credit")
                        {
                            MySqlCommand command = new MySqlCommand("UPDATE visits SET time_out = '" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " 23:59:59', value = 1 WHERE visit_id = " + Int32.Parse(dr[0].ToString()), conn);
                            command.ExecuteNonQuery();
                            command = new MySqlCommand("UPDATE members SET total_current_visits = (total_current_visits + 1) WHERE member_id = '" + dr[1].ToString() + "'", conn);
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            MySqlCommand command = new MySqlCommand("UPDATE visits SET time_out = '" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " 23:59:59', value = 0 WHERE visit_id = " + Int32.Parse(dr[0].ToString()), conn);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException)
                    {

                    }
                }
                disconnectMySql();
            }
        }//end closeAllFailedLogouts

        
        

       

        /*
         * Author: Russ Utt
         * Method Name: getDefaultExpireDate
         * Parameters: none
         * Output: String
         * Exception: MySqlException
         * Description: This method gets the default expiration date that is set in the database
         */
        public String getDefaultExpireDate()
        {
            String expireDate = "";
            DateTime time = new DateTime();
            DataTable dt = new DataTable();
            connectToMySql();
            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT expiration_date FROM dates", conn);
                da.Fill(dt);
                //MessageBox.Show(dt.Rows.Count.ToString());
                if (dt.Rows.Count == 1)
                {
                    DataRow myRow = dt.Rows[0];
                    time = Convert.ToDateTime(myRow[0].ToString());
                    if (time <= System.DateTime.Now)
                    {
                        //setDefaultExpDate(System.DateTime.Now);
                        expireDate = System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day + " 23:59:59";
                    }
                    else
                    {
                        expireDate = time.Year + "-" + time.Month + "-" + time.Day + " 23:59:59";
                    }
                }
                else if (dt.Rows.Count == 0)
                {
                    //createDefaultExpirationDate(System.DateTime.Now);
                    expireDate = System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day + " 23:59:59";
                }

            }
            catch (MySqlException)
            {
                expireDate = System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day + " 23:59:59";
            }
            finally
            {
                disconnectMySql();
            }
            return expireDate;
        }

        

        /*
         * Author: Russ Utt
         * Method Name: getCurrentMinMinutesPerVisit
         * Parameters: none
         * Output: int
         * Exception: MySqlException
         * Description: This method gets the minimum number of minutes a credit member needs to workout in order 
         * to receive a visit
         */
        public int getCurrentMinMinutesPerVisit()
        {
            DataTable dt = new DataTable();
            int min = -1;
            connectToMySql();
            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT settings_value FROM settings WHERE settings_name = 'min_time_for_visit'", conn);
                da.Fill(dt);
                if (dt.Rows.Count == 1)
                {
                    DataRow myRow = dt.Rows[0];
                    min = Int32.Parse(myRow[0].ToString());
                }
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return min;
        }

        

         /*
         * Author: Russ Utt
         * Method Name: getAllCreditMemberInfo
         * Parameters: member id
         * Output: DataTable
         * Exception: MySqlException
         * Description: This method gets all member info for a particular member
         */
        public DataTable getAllMemberInfo(String memID)
        {
            DataTable dt = null;
            connectToMySql();

            try
            {
                dt = new DataTable();

                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM members WHERE member_id = '" + memID.Replace("'", "''") + "'", conn);
                da.Fill(dt);

            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return dt;
        }

        //Author: Joe Kearns
        public DataTable getVisitInfoForCompReport(String memID, DateTime dateFrom, DateTime dateTo)
        {
            String fromDate, toDate;
            fromDate = dateFrom.Year + "-" + dateFrom.Month + "-" + dateFrom.Day + " 00:00:00";
            toDate = dateTo.Year + "-" + dateTo.Month + "-" + dateTo.Day + " 23:59:59";

            DataTable dt = null;
            connectToMySql();

            try
            {

                dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT time_in, time_out, value FROM visits WHERE member_id = '" + memID.Replace("'", "''") + "' AND is_archived = 0 AND time_in BETWEEN '" + fromDate.Replace("'", "''") + "' AND '" + toDate.Replace("'", "''") + "'", conn);
                da.Fill(dt);
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }
            return dt;
        }

       
        /*
         * Author: Russ Utt
         * Method Name: doesUserExist
         * Parameters: user's name
         * Output: bool
         * Exception: MySqlException
         * Description: This method checks to see if a certain user exists in the database.
         */
        public bool doesUserExist(String userName)
        {
            int count = -1;
            String user = userName.ToUpper();
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM users WHERE UPPER(user_name) = '" + user.Replace("'", "''") + "'", conn);
                da.Fill(dt);
                count = dt.Rows.Count;
                if (count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException)
            {
                return true;
            }
            finally
            {
                disconnectMySql();
            }
        }

       

        /* The following 2 methods, ComputeHash and VerifyHash deal with encrypting
         * passwords.  The code was lifted from http://www.obviex.com/samples/hash.aspx
         * with very little modification by me.  The saltBytes passed into the 
         * ComputeHash method will be null as to let the method figure the 
         * size on the fly */
        public static string computeHash(string plainText, byte[] saltBytes)
        {
            if (saltBytes == null)
            {
                // Define min and max salt sizes.
                int minSaltSize = 4;
                int maxSaltSize = 8;

                // Generate a random number for the size of the salt.
                Random random = new Random();
                int saltSize = random.Next(minSaltSize, maxSaltSize);

                // Allocate a byte array, which will hold the salt.
                saltBytes = new byte[saltSize];

                // Initialize a random number generator.
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                // Fill the salt with cryptographically strong byte values.
                rng.GetNonZeroBytes(saltBytes);
            }
            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            // Because we support multiple hashing algorithms, we must define
            // hash object as a common (abstract) base class. We will specify the
            // actual hashing algorithm class later during object creation.
            HashAlgorithm hash;
            hash = new MD5CryptoServiceProvider();
            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length +
                                                saltBytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            // Convert result into a base64-encoded string.
            string hashValue = Convert.ToBase64String(hashWithSaltBytes);

            // Return the result.
            return hashValue;
        }

        public static bool verifyHash(string plainText, string hashValue)
        {
            // Convert base64-encoded hash value into a byte array.
            byte[] hashWithSaltBytes = Convert.FromBase64String(hashValue);

            // We must know size of hash (without salt).
            int hashSizeInBits, hashSizeInBytes;

            hashSizeInBits = 128;
            // Convert size of hash from bits to bytes.
            hashSizeInBytes = hashSizeInBits / 8;

            // Make sure that the specified hash value is long enough.
            if (hashWithSaltBytes.Length < hashSizeInBytes)
                return false;
            // Allocate array to hold original salt bytes retrieved from hash.
            byte[] saltBytes = new byte[hashWithSaltBytes.Length -
                                        hashSizeInBytes];

            // Copy salt from the end of the hash to the new array.
            for (int i = 0; i < saltBytes.Length; i++)
                saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

            // Compute a new hash string.
            string expectedHashString = computeHash(plainText, saltBytes);

            // If the computed hash matches the specified hash,
            // the plain text value must be correct.
            return (hashValue == expectedHashString);
        }

       

             
        

        
        /*
          * Author: Russ Utt
          * Method Name: isAdminPasswordCorrect
          * Parameters: password
          * Output: void
          * Exception: MySqlException
          * Description: This method checks to see if the password given is the correct administrator password
          */
        public bool isAdminPasswordCorrect(String pass)
        {
            bool correct = false;
            String password = "";
            DataTable dt = new DataTable();
            connectToMySql();

            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM users WHERE user_role = 'admin';", conn);
                da.Fill(dt);
                if (dt.Rows.Count == 1)
                {
                    DataRow myRow = dt.Rows[0];

                    password = myRow["password"].ToString();

                    correct = true;
                }
            }
            catch (MySqlException)
            {

            }
            finally
            {
                disconnectMySql();
            }


            return (verifyHash(pass, password));


        }
    
    }//endclass
}//endnamespace
