using MVC_Store.Areas.Admin.Data;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            if (page == "")
                page = "home";

            PageVM model;
            PagesDTO dto;

            using (Db db = new Db())
            {
                if (!db.Pages.Any(y => y.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }

            using (Db db = new Db()) {
                dto = db.Pages.Where(y => y.Slug == page).FirstOrDefault();
            }

            ViewBag.PageTitle = dto.Title;

            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Да";
            }
            else
            {
                ViewBag.Sidebar = "Нет";
            }

            model = new PageVM(dto);

            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            List<PageVM> pageVMList;

            using(Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(y => y.Sorting).Where(y => y.Slug != "home").Select(y => new PageVM(y)).ToList();
            }


            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            SidebarVM model;

            using(Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                model = new SidebarVM(dto);
            }


            return PartialView("_SidebarPartial",model);
        }
    }
}