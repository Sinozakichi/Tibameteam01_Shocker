﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Shocker.Models
{
    public partial class Pictures
    {
        public string PictureId { get; set; }
        public int ProductId { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }

        public virtual Products Product { get; set; }
    }
}