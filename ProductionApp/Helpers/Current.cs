using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionApp.Controllers;

namespace ProductionApp.Helpers {
    public static class Current {

        /// <summary>
        ///     Shortcut to HttpContext.Current.
        /// </summary>
        public static HttpContext Context {
            get { return HttpContext.Current; }
        }

        /// <summary>
        ///     Shortcut to HttpContext.Current.Request.
        /// </summary>
        public static HttpRequest Request {
            get { return Context.Request; }
        }

        /// <summary>
        ///     Gets the controller for the current request; should be set during init of current request's controller.
        /// </summary>
        public static BaseController Controller {
            get { return Context.Items["Controller"] as BaseController; }
            set { Context.Items["Controller"] = value; }
        }
    }
}