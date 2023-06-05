using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; }
		public string Address { get; set; }
	}
}
