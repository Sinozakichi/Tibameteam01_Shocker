using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Shocker.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Password { get; set; }
        public string NickName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(50)]
		public string Address { get; set; }
	}
}
