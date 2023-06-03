namespace Shocker.Models.ViewModels
{
    public class RegisterViewModel
    {
        public string Id { get; set; }
        public string AccountType { get; set; }
        public string Password { get; set; }
        public string? NickName { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime RegisterDate { get; set; }
        public string? PicturePath { get; set; }
        public string? AboutSeller { get; set; }

    }
}
