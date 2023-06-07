using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Shocker.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        [Required]     
        [MinLength(6)]
        [MaxLength(30)]
        public string Password { get; set; }
		[MinLength(5)]
		[MaxLength(30)]
		public string NickName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }
		[MaxLength(50)]
		public string Address { get; set; }
	}
}
