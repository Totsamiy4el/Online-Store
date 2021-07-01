using MVC_Store.Areas.Admin.Data;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            List<PageVM> pageList;

            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            return View(pageList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db =new Db())
            {
                string slug;

                PagesDTO dto = new PagesDTO();

                dto.Title = model.Title.ToUpper();

                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "Это название уже занято.");
                    return View(model);
                }

                else if(db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "Это короткое название уже занято.");
                    return View(model);

                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                db.Pages.Add(dto);
                db.SaveChanges();
            }
            TempData["SM"] = "Вы добавили новую страницу!";

            return RedirectToAction("Index");
            
        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                if(dto == null)
                {
                    return Content("Страница не найдена.");
                }

                model = new PageVM(dto);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using(Db db = new Db())
            {
                int id = model.Id;

                string slug = "home";

                PagesDTO dto = db.Pages.Find(id);

                dto.Title = model.Title;

                if(model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                if(db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "Название уже занято.");
                    return View(model);
                }
                else if(db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Это короткое название уже занято.");
                    return View(model);
                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                
                db.SaveChanges();
            }

            TempData["SM"] = "Вы изменили страницу.";
            

            return RedirectToAction("EditPage");
        }

        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            PageVM model;

            using(Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                if(dto == null)
                {
                    return Content("Страница не найдена.");
                }

                model = new PageVM(dto);

            }

            return View(model);
        }
        
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using(Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                db.Pages.Remove(dto);

                db.SaveChanges();
            }

            TempData["SM"] = "Вы удалили страницу.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using(Db db = new Db())
            {
                int count = 1;

                PagesDTO dto;

                foreach(var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVM model;
            using(Db db =new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                model = new SidebarVM(dto);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                dto.Body = model.Body;

                db.SaveChanges();
            }

            TempData["SM"] = "Вы изменили боковую панель";


                return RedirectToAction("EditSidebar");
        }
    }
}