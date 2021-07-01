using MVC_Store.Areas.Admin.Data;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Ваша корзина пуста";
                return View();
            }
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;
            CartVM cartVM = new CartVM();

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            CartVM model = new CartVM();

            int qty = 0;

            decimal price = 0m;

            if (Session["cart"] != null)
            {
                var list = (List<CartVM>)Session["cart"];
                foreach (var item in list)
                {
                    qty += item.Quantity;

                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                model.Quantity = 0;
                model.Price = 0m;
            }

            return PartialView("_CartPartial", model);
        }

        public ActionResult AddToCartCard()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();
            int id = 23;
            using (Db db = new Db())
            {
                ProductDTO product = db.Products.Find(id);
                cart.Add(new CartVM()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 1,
                    Price = product.Price,
                    Image = product.ImageName
                }
);
            }
            Session["cart"] = cart;

            return RedirectToAction("");
        }
        public ActionResult AddToCartPartial(int id)
        {

            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                ProductDTO product = db.Products.Find(id);

                var productInCart = cart.FirstOrDefault(y => y.ProductId == id);

                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName,
                        Extra = product.Extra
                    }
                    );
                }
                else
                {
                    productInCart.Quantity++;
                }
            }
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            Session["cart"] = cart;



            return PartialView("_AddToCartPartial", model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(y => y.ProductId == productId);

                model.Quantity++;

                var result = new { qty = model.Quantity, price = model.Price 
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }



        }
        public ActionResult DecrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(y => y.ProductId == productId);

                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(y => y.ProductId == productId);
                cart.Remove(model);
            }

        }
        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;



            return PartialView();
        }

        [HttpPost]
        public void PlaceOrder(string date, string time, string adress, string city, string poshta, string cardText, string name, string namelast, string numberPhone, string email, string passwordAcc, string deliv)
        {
            if (numberPhone == null)
            {

                List<CartVM> cart = Session["cart"] as List<CartVM>;

                string userName = User.Identity.Name;

                int orderId = 0;

                using (Db db = new Db())
                {
                    OrderDTO orderDTO = new OrderDTO();

                    var q = db.Users.FirstOrDefault(y => y.Number == userName);
                    int userId = q.Id;
                    orderDTO.UserId = userId;
                    orderDTO.CreatedAt = DateTime.Now;

                    db.Orders.Add(orderDTO);
                    db.SaveChanges();

                    orderId = orderDTO.OrderId;

                    OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                    foreach (var item in cart)
                    {

                        orderDetailsDTO.ProductId = item.ProductId;
                        orderDetailsDTO.Quantity = item.Quantity;
                        orderDetailsDTO.OrderId = orderId;
                        orderDetailsDTO.UserId = userId;
                        orderDetailsDTO.TimeDelivery = time;
                        orderDetailsDTO.DateDelivery = date;
                        orderDetailsDTO.CityDelivery = city;
                        orderDetailsDTO.AdressDelivery = adress;
                        orderDetailsDTO.DeliveryPoshtaNumber = poshta;
                        orderDetailsDTO.CardDeliveryText = cardText;
                        orderDetailsDTO.DeliveryMethod = deliv;
                        db.OrderDetails.Add(orderDetailsDTO);
                        db.SaveChanges();


                    }


                    

                }



                Session["cart"] = null;

            }
            else
            {
                using (Db db = new Db())
                {
                    if (db.Users.Any(y => y.Number.Equals(numberPhone)))
                    {

                        numberPhone = "";

                    }
                    else
                    {
                        UserDTO userDTO = new UserDTO()
                        {
                            FirstName = name,
                            LastName = namelast,
                            EmailAdress = email,
                            Number = numberPhone,
                            Password = passwordAcc
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

                        List<CartVM> cart = Session["cart"] as List<CartVM>;

                        string userName = User.Identity.Name;

                        int orderId = 0;

                        OrderDTO orderDTO = new OrderDTO();


                        int userId = id;
                        orderDTO.UserId = userId;
                        orderDTO.CreatedAt = DateTime.Now;

                        db.Orders.Add(orderDTO);
                        db.SaveChanges();

                        orderId = orderDTO.OrderId;

                        OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                        foreach (var item in cart)
                        {

                            orderDetailsDTO.ProductId = item.ProductId;
                            orderDetailsDTO.Quantity = item.Quantity;
                            orderDetailsDTO.OrderId = orderId;
                            orderDetailsDTO.UserId = userId;
                            orderDetailsDTO.TimeDelivery = time;
                            orderDetailsDTO.DateDelivery = date;
                            orderDetailsDTO.CityDelivery = city;
                            orderDetailsDTO.AdressDelivery = adress;
                            orderDetailsDTO.DeliveryPoshtaNumber = poshta;
                            orderDetailsDTO.CardDeliveryText = cardText;
                            orderDetailsDTO.DeliveryMethod = deliv;
                            db.OrderDetails.Add(orderDetailsDTO);
                            db.SaveChanges();
                        }

                        







                        Session["cart"] = null;

                    }
                }
            }

        }

        public bool Check(string numberShoto)
        {
            using (Db db = new Db())
            {

                bool CheckNumber = db.Users.Any(y => y.Number.Equals(numberShoto));

                return CheckNumber;

            }

        }
    }

}