using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models.ViewModels.Cart
{
    public class CartVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get { return Quantity * Price; } }
        public string Image { get; set; }
        public string TimeDelivery { get; set; }
        public string DateDelivery { get; set; }
        public string CityDelivery { get; set; }
        public string AdressDelivery { get; set; }
        public string CardDeliveryText { get; set; }
        public string DeliveryPoshtaNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAdress { get; set; }
        public string Number { get; set; }
        public string Password { get; set; }
        public string DeliveryMethod { get; set; }
        public string Extra { get; set; }
    }
}