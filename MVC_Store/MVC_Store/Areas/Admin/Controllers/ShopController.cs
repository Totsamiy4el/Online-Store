using MVC_Store.Areas.Admin.Data;
using MVC_Store.Areas.Admin.Models.ViewModels.Shop;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MVC_Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            return View(categoryVMList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;

            using (Db db = new Db())
            {
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                CategoryDTO dto = new CategoryDTO();

                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();
            }

            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;

                CategoryDTO dto;

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }


        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                CategoryDTO dto = db.Categories.Find(id);

                db.Categories.Remove(dto);

                db.SaveChanges();
            }

            TempData["SM"] = "Вы удалили категорию.";

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";
                CategoryDTO dto = db.Categories.Find(id);

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();
            }
            return "ok";
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Названия продукта уже занято.");
                    return View(model);
                }
            }
            int id;
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO(); product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.OldPrice = 0m;
                product.CategoryId = model.CategoryId;
                product.Extra = " ";

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            TempData["SM"] = "Вы добавили продукт!";


            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }
            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }
            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }
            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }
            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            if (file != null && file.ContentLength > 0)
            {

                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Изображение не загружено.Выберете верный формат файла.");
                        return View(model);
                    }
                }

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(500, 500).Crop(1, 1);
                img.Save(path2);

            }

            return RedirectToAction("AddProduct");
        }


        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVM> listOfProductVM;

            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name"); ViewBag.SelectedCat = catId.ToString();
            }
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 10);
            ViewBag.onePageOfProducts = onePageOfProducts;

            return View(listOfProductVM);
        }


        public ActionResult EditProduct(int id)
        {
            ProductVM model;


            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content("Продукт не найден.");
                }

                model = new ProductVM(dto);

                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fileName => Path.GetFileName(fileName));
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            int id = model.Id;

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory
                   .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                   .Select(fn => Path.GetFileName(fn));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Это имя продукта занято!");
                    return View(model);
                }
            }
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            TempData["SM"] = "Вы изменили продукт!";

            if (file != null && file.ContentLength > 0)
            {

                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
              ext != "image/jpeg" &&
              ext != "image/pjpeg" &&
              ext != "image/gif" &&
              ext != "image/x-png" &&
              ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Изображение не загружено.Выберете правильный формат файла.");
                        return View(model);
                    }
                }
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }
                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }
                string imageName = file.FileName; using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(500, 500).Crop(1, 1);
                img.Save(path2);

            }



            return RedirectToAction("EditProduct");
        }


        public ActionResult DeleteProduct(int id)
        {
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];

                if (file != null && file.ContentLength > 0)
                {
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(500, 500).Crop(1, 1);
                    img.Save(path2);
                }


            }

        }

        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);


            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }
        public ActionResult AddSale(ProductVM model)
        {
            int id = model.Id;

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);


                dto.Price = model.Price;
                dto.OldPrice = model.OldPrice;

                db.SaveChanges();
            }
            return View();
        }

        public ActionResult Orders()
        {
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                List<OrderVM> orders = db.Orders.ToArray().Select(y => new OrderVM(y)).ToList();
                foreach (var order in orders)
                {
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    string ExtraComments = "";
                    
                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(y => y.OrderId == order.OrderId).ToList();

                    UserDTO user = db.Users.FirstOrDefault(y => y.Id == order.UserId);

                    string username = user.Number;
                    foreach (var orderDetails in orderDetailsList)
                    {
                        ProductDTO product = db.Products.FirstOrDefault(y => y.Id == orderDetails.ProductId);

                        decimal price = product.Price;

                        string productName = product.Name;

                        ExtraComments += product.Extra;

                        productAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    OrderDetailsDTO details = db.OrderDetails.FirstOrDefault(y => y.OrderId == order.OrderId);

                    string time = details.TimeDelivery;
                    string date = details.DateDelivery;
                    string city = details.CityDelivery;
                    string adres = details.AdressDelivery;
                    string card = details.CardDeliveryText;
                    string poshta = details.DeliveryPoshtaNumber;
                    string delivery = details.DeliveryMethod;
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt,
                        TimeDelivery = time,
                        DateDelivery = date,
                        CityDelivery = city,
                        AdressDelivery = adres,
                        CardDeliveryText = card,
                        DeliveryPoshtaNumber = poshta,
                        DeliveryMethod = delivery,
                        Extra = ExtraComments
                    });

                }
            }

            return View(ordersForAdmin);
        }

    }
}