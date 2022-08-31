using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Helpers
{
    public class EnumHelper
    {
        public enum Enum_Action
        {
            Cancel = 1,
            Call = 2,
            CPSend = 3,
            SPSend = 4,
            Complete = 5
        }

        public enum Manager_Action
        {
            None = 1,
            Approve = 2,
            Reject = 3
        }

        public enum HR_Action
        {
            None = 1,
            Approve = 2,
            Reject = 3
        }

        public enum Action
        {
            None = 1,
            Processing = 2,
            Approve = 3,
            Reject = 4
        }

        public enum Recut_Action_Click
        {
            QASew = 1,
            Manager = 2,
            Plan = 3,
            CCDFabric = 4,
            Warehouse = 5,
            CCD = 6,
            QACut = 7,
            Production = 8
        }

        public enum View_Type
        {
            Weekly = 1,
            Daily = 2,
            Style = 3,
            Fabric = 4
        }
    }
}