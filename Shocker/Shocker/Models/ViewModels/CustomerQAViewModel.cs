using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
    public class CustomerQAViewModel
    {

        [Required(ErrorMessage = "請選擇問題類型")]
        public int QuestionCategoryId { get; set; }
        public string? UserAccount { get; set; }
        [Required(ErrorMessage = "請在框框中輸入您遇到的問題")]

        public string? Description { get; set; }
    }
}
