using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class DataTicketOrderFabricToPrint
    {
        //listSourceDataToPrint.Columns.Add("ID_TICKET_DETAIL", typeof(int));
        //        listSourceDataToPrint.Columns.Add("ID_TICKET", typeof(int));
        //        listSourceDataToPrint.Columns.Add("ORDER_IN_LIST", typeof(int));

        //        listSourceDataToPrint.Columns.Add("CODE_FABRIC", typeof(string));
        //        listSourceDataToPrint.Columns.Add("NOTE_DETAIL", typeof(string));
        //        listSourceDataToPrint.Columns.Add("NUMBER_REQUEST", typeof(double));

        //        listSourceDataToPrint.Columns.Add("WO", typeof(string));
        //        listSourceDataToPrint.Columns.Add("ASSORTMENT", typeof(string));
        //        listSourceDataToPrint.Columns.Add("NOTE_TTS", typeof(string));

        //        listSourceDataToPrint.Columns.Add("DATE_CREATE", typeof(DateTime));

        public int ID_TICKET_DETAIL { get; set; }
        public int ID_TICKET { get; set; }
        public int ORDER_IN_LIST { get; set; }

        public string CODE_FABRIC { get; set; }
        public string NOTE_DETAIL { get; set; }
        public double NUMBER_REQUEST { get; set; }

        public string WO { get; set; }
        public string ASSORTMENT { get; set; }
        public string NOTE_TTS { get; set; }

        public string TICKET_NUMBER { get; set; }
        public DateTime DATE_CREATE { get; set;}
    }
}