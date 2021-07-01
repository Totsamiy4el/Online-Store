using MVC_Store.Areas.Admin.Data;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            List<CategoryVM> categoryVMList;


            using(Db db = new Db()){
                categoryVMList = db.Categories.ToArray().OrderBy(y => y.Sorting).Select(y => new CategoryVM(y)).ToList();
            }
            return PartialView("_CategoryMenuPartial", categoryVMList);
        } 

        public ActionResult Category(string name)
        {
            List<ProductVM> productVMList;

            using(Db db = new Db())
            {
                CategoryDTO categoryDTO = db.Categories.Where(y => y.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                productVMList = db.Products.ToArray().Where(y => y.CategoryId == catId).Select(y => new ProductVM(y)).ToList();

                var productCat = db.Products.Where(y => y.CategoryId == catId).FirstOrDefault();

                if(productCat == null)
                {
                    var catName = db.Categories.Where(y => y.Slug == name).Select(y => y.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }

            return View(productVMList);
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            ProductDTO dto;
            ProductVM model;

            int id = 0;

            using(Db db = new Db())
            {
                if(!db.Products.Any(y => y.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                dto = db.Products.Where(y => y.Slug == name).FirstOrDefault();

                id = dto.Id;

                model = new ProductVM(dto);

            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));

            return View("ProductDetails", model);
        }
    }
}