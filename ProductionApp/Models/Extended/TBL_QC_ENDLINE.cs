using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Production.Models
{
    [MetadataType(typeof(TBL_QC_ENDLINEMetadata))]
    public partial class TBL_QC_ENDLINE
    {

    }
    public class TBL_QC_ENDLINEMetadata
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide QC Staff ID")]
        public Nullable<long> QC_STAFF_ID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide WORKER ID")]
        public Nullable<long> WORKER_ID { get; set; }
    }
}