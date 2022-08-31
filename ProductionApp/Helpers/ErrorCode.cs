using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Helpers
{
    public static class ErrorCode
    {
        public const int FOREIGN_KEY = -2;
        public const int DUPLICATE_ID = -1;
        public const int EntityValidation = -3;
    }
}