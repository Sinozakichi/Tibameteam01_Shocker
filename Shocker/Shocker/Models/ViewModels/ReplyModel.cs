﻿namespace Shocker.Models.ViewModels
{
    public class ReplyModel
    {
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public string SellerAccount { get; set; }
        public string? Reply { get; set; }
    }
}
