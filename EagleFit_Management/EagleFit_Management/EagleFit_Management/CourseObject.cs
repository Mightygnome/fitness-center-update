/*
 EagleFit_Management: CourseObject.cs
  
 Fall 2013 - Winter 2014
 
 Description: This class creates a course object. 
              This class is used by the AddMultipleUsersAtOnce class to read in the course listing from the database.
              Once a course object is created, the course is then added to a List holding Courses (List<CourseObject>)
              that is used to populate the combo-box to choose which course to enroll a student into.
 
 Linked .cs/xaml Files: AddMultipleUsersAtOnce.xaml.cs,  
 
 Created By: Sarah Henderson
 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleFit_Management
{
    public class CourseObject
    {
        /*
         create table Courses(
	        course_ID INT(3) NOT NULL,
	        course_Name VARCHAR(25) NOT NULL,
	        Start_Time TIME,
	        End_Time TIME,
	        Section_Num INT(1) NOT NULL,
	        Quarter_End_Date DATE NOT NULL default '0000-00-00',
	        Credits INT(1),
	        Quarter VARCHAR(6) NOT NULL,
	        Year INT(4) NOT NULL,
	        PRIMARY KEY (course_ID, Section_Num, Quarter_End_Date)	
        );
         */

        private int cId;        
        private String courseName;
        private String sTime;
        private String eTime;
        private int section;
        private String QED;
        private int credits;
        private String quarter;
        private int Year;

        public CourseObject(int course_Id, String c_Name, String startTime, String endTime, int secNum, String quarterED, int credits1, String qtr, int yr)
        {
             cId = course_Id;
             courseName = c_Name;
             sTime = startTime;
             eTime = endTime;
             section = secNum;
             QED = quarterED;             
             credits = credits1;
             quarter = qtr;
             Year = yr;            
        }


        public int getCId()
        {
            return cId; 
        }


        public String getCourseName()
        {
            return courseName;
        }

        public String getSTime()
        {
            return sTime;
        }

        public String getETime()
        {
            return eTime;
        }


        public int getSection()
        {
            return section;
        }


        public String getQED()
        {
            return QED;
        }


        public int getCredits()
        {
            return credits;
        }

        public String getQtr()
        {
            return quarter;
        }

        public int getYear()
        {
            return Year;
        }


    }//end class
}//end namespace
