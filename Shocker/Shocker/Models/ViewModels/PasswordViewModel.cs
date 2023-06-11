using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
	public class PasswordViewModel
	{
		public string Id { get; set; }
		[Required(ErrorMessage = "密碼為必填欄位")]
		[DataType(DataType.Password)]
		[StringLength(30, MinimumLength = 6, ErrorMessage = "密碼長度須為6~30之間")]
		public string Password { get; set; }
	}
}
