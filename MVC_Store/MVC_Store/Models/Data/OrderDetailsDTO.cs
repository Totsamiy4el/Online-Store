using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVC_Store.Models.Data
{
    [Table("tblOrderDetails")]
    public class OrderDetailsDTO
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string TimeDelivery { get; set; }
        public string DateDelivery { get; set; }
        public string CityDelivery { get; set; }
        public string AdressDelivery { get; set; }
        public string CardDeliveryText { get; set; }
        public string DeliveryPoshtaNumber { get; set; }
        public string DeliveryMethod { get; set; }
        [ForeignKey("OrderId")]
        public virtual OrderDTO Orders { get; set; }

        [ForeignKey("UserId")]
        public virtual UserDTO Users { get; set; }
        [ForeignKey("ProductId")]
        public virtual ProductDTO Products { get; set; }

    }
}