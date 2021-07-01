using MVC_Store.Areas.Admin.Data;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Account;
using MVC_Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVC_Store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model); 
            }
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Пароль не совпадает");
                return View("CreateAccount", model);
            }

            using(Db db = new Db())
            {
                if(db.Users.Any(y => y.Number.Equals(model.Number)))
                {
                    ModelState.AddModelError("", $"Этот номер {model.Number} уже занят.");
                    model.Number = "";
                    return View("CreateAccount", model);
                }
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    Number = model.Number,
                    Password = model.Password
                };
                db.Users.Add(userDTO);

                db.SaveChanges();

                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };
                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            TempData["SM"] = "Вы теперь зарегестрированы и можете войти.";

            return RedirectToAction("Login");
        }


        [HttpGet]
        public ActionResult Login()
        {
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("user-profile");
            }

            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {


            if (!ModelState.IsValid)
            {
                return View(model);
            }
            bool isValid = false;

            using(Db db = new Db())
            {
                if (db.Users.Any(y => y.Number.Equals(model.Number) && y.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
                if (!isValid)
                {
                    ModelState.AddModelError("", "Неправильный номер или пароль.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Number, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Number, model.RememberMe));

                }
            }
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        [Authorize]
        public ActionResult UserNavPartial()
        {

            string userName = User.Identity.Name;

            UserNavPartialVM model;

            using(Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(y => y.Number == userName);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            return PartialView(model);
        }
        [Authorize]
        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {

            string userName = User.Identity.Name;

            UserProfileVM model;

            using(Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(y => y.Number == userName);

                model = new UserProfileVM(dto);

            }

            return View("UserProfile", model);
        }
        [Authorize]
        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {

            bool userNameIsChanged = false;

            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Пароли не совпадают.");
                    return View("UserProfile", model);
                }
            }

            using(Db db = new Db())
            {
                string userName = User.Identity.Name;

                if(userName != model.Number)
                {
                    userName = model.Number;
                    userNameIsChanged = true;
                }


                if(db.Users.Where(y => y.Id != model.Id).Any(y => y.Number== userName))
                {
                    ModelState.AddModelError("", $"Этот номер {model.Number} уже занят.");
                    model.Number = "";
                    return View("UserProfile", model);
                }

                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.Number= model.Number;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                db.SaveChanges();
            }

            TempData["SM"] = "Вы изменили свой профиль.";
            if (!userNameIsChanged)
                return View("UserPRofile", model);
            else
                return RedirectToAction("Logout");
        }
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();
            using(Db db = new Db())
            {
                UserDTO user = db.Users.FirstOrDefault(y => y.Number == User.Identity.Name);
                int userId = user.Id;
                List<OrderVM> orders = db.Orders.Where(y => y.UserId == userId).ToArray().Select(y => new OrderVM(y)).ToList();

                foreach(var order in orders)
                {
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsDto = db.OrderDetails.Where(y => y.OrderId == order.OrderId).ToList();

                    foreach(var orderDetails in orderDetailsDto)
                    {
                        ProductDTO product = db.Products.FirstOrDefault(y => y.Id == orderDetails.ProductId);

                        decimal price = product.Price;
                        string productName = product.Name;
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            

            return View(ordersForUser);
        }
    }
}