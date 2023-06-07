﻿using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Shocker.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage ="密碼為必填欄位")]
		[DataType(DataType.Password)]
		[StringLength(30, MinimumLength = 6,ErrorMessage ="密碼長度須為6~30之間")]
		public string Password { get; set; }
		[StringLength(30, MinimumLength = 5, ErrorMessage = "暱稱長度須為5~30之間")]
		public string NickName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        [Required(ErrorMessage = "Email為必填欄位")]
        [EmailAddress(ErrorMessage = "Email不符合格式")]
        public string Email { get; set; }
		public string Address { get; set; }
	}
}
