
namespace Shocker.Models
{
	public class ApiResultModel
	{
		public bool Status { get; set; }
		public object Data { get; set; }
		public string ErrorMessage { get; set; }
	}
}
