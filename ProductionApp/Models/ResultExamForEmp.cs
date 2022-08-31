using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class ResultExamForEmp
    {
        public string EmployeeName { get; set; }
        public string ExamName { get; set; }
        public string CourseName { get; set; }
        public string Time { get; set; }
        public string Score { get; set; }
        public string QuestionNumber { get; set; }
        public double Rate { get; set; }
        public int CorrectAnswers { get; set; }
    }
}