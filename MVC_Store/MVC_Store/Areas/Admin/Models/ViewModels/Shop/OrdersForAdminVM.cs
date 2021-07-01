using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        public int OrderNumber { get; set; }
        public string UserName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQty { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeDelivery { get; set; }
        public string DateDelivery { get; set; }
        public string CityDelivery { get; set; }
        public string AdressDelivery { get; set; }
        public string CardDeliveryText { get; set; }
        public string DeliveryPoshtaNumber { get; set; }
        public string DeliveryMethod { get; set; }
        public string Extra { get; set; }

    }
}