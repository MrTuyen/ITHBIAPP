using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class DetailQuestion
    {
        public int QuestionID { get; set; }
        public string ContentQues { get; set; }
        public int AnswerID { get; set; }
        public string ContentAns { get; set; }
        public int State { get; set; }

    }
}