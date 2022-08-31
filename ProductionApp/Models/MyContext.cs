using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using ProductionApp.Helpers;

namespace ProductionApp.Models
{
    public class MyContext : ProductionAppEntities
    {
        public MyContext()
        {
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 180;
        }

        //public override int SaveChanges()
        //{
        //    //try
        //    //{
        //        return base.SaveChanges();
        ////    }
        ////    catch (System.Data.Entity.Validation.DbEntityValidationException e)
        ////    {

        ////        Utilities.WriteLogException(e, "Error code using by Admin EntityValidation: -3");
        ////        return -3;
        ////    }
        ////    catch (DbUpdateException ex)
        ////    {

        ////        if (null == ex.InnerException) return -1;
        ////        var innerException = ex.InnerException.InnerException as System.Data.SqlClient.SqlException;
        ////        if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
        ////        {
        ////            Utilities.WriteLogException(ex, "Mycontext:DbUpdateException | Error code using by Admin DUPLICATE_ID: -1");
        ////            return -1;
        ////        }

        ////        if (innerException != null && innerException.Number == 547)
        ////        {
        ////            Utilities.WriteLogException(ex, "Mycontext:DbUpdateException | Error code using by Admin FOREIGN_KEY: -2");
        ////            return -2;
        ////        }

        ////        Utilities.WriteLogException(ex, "Mycontext:DbUpdateException | Other error");
        ////        return -99;
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        Utilities.WriteLogException(e, "MyContext Class");
        ////        throw;
        ////    }

        //}

    }
}