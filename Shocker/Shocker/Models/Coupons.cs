﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Shocker.Models
{
    public partial class Coupons
    {
        public Coupons()
        {
            OrderDetails = new HashSet<OrderDetails>();
        }

        public int CouponId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string HolderAccount { get; set; }
        public int ProductCategoryId { get; set; }
        public decimal Discount { get; set; }
        public string Status { get; set; }
        public string PublisherAccount { get; set; }

        public virtual Users HolderAccountNavigation { get; set; }
        public virtual ProductCategories ProductCategory { get; set; }
        public virtual Users PublisherAccountNavigation { get; set; }
        public virtual Status StatusNavigation { get; set; }
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}