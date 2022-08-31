using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Helpers;
using ProductionApp.Models;
namespace ProductionApp.Controllers {
    public class SharedController:BaseController {
        public ActionResult RenderHeader() {


            return PartialView("_Header");
        }

        public ActionResult RenderMobileMenu() {
            if(userLogin != null) {
                var cates = db.TBL_CATEGORIES.Where(m => m.CA_STATUS == true && m.CA_URL.Length > 0);
                var pages = db.TBL_PERMISSION.Where(p => p.USERNAME == userLogin.Username).Select(m => m.TBL_CATEGORIES).Where(m => m.CA_STATUS == true).OrderBy(p => p.CA_ID).ToList();
                var pagesParent = db.TBL_CATEGORIES.Where(m => m.CA_PARENT == 0).OrderBy(p => p.CA_ID).ToList();
                bool isPer = false;
                bool isCheck = false;
                if(Request.Url != null && (Request.Url.AbsolutePath != "" &&
                                           !Request.Url.AbsolutePath.ToLower().Contains("/account".ToLower()) &&
                                           !Request.Url.AbsolutePath.ToLower().Contains("/Notification".ToLower()) &&
                                           !Request.Url.AbsolutePath.ToLower().Contains("/display".ToLower()) &&
                                           Request.Url.AbsolutePath.ToLower().Replace("/" ,"") != "")) {
                    if(Request.Url != null)
                    {
                        var controller = Request.Url.AbsolutePath.Split()[0];

                        foreach(var item in cates) {
                            //if(Request.Url.AbsolutePath.ToLower().Contains(item.CA_URL.ToLower())) {
                            if(item.CA_URL.ToLower().Contains(controller.ToLower())) {
                                isCheck = true;
                                break;
                            }
                        }
                        if(isCheck)
                            foreach(var item in pages) {
                                if(item.CA_URL != null && item.CA_URL.ToLower().Contains(controller.ToLower())) {
                                    isPer = true;
                                    break;
                                }
                            }


                        if(isCheck  && isPer == false) {
                            Session["permission"] = "0";

                        }
                    }
                }
                ViewBag.Pages = pages;
                ViewBag.PagesParent = pagesParent;
            }
            return PartialView("_SMenu");


        }


    }
}