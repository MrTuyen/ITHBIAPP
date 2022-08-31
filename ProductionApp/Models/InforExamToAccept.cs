using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class InforExamToAccept
    {
        public string Emp_ID { get; set; }
        public int ExamID { get; set; }
        public int CourseID { get; set; }
        public string ExamName { get; set; }
        public string CourseName { get; set; }
        public double Point { get; set; }
        public string State { get; set; }
        public int Time { get; set; }
        public string Date { get; set; }
        public int QuestionNumber { get; set; }
    }
}