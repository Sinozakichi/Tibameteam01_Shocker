using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
    public class CustomerQAViewModel
    {
        [Range(1,10,ErrorMessage="請選擇問題類型")]
        [Required]
        public int QuestionCategoryId { get; set; }
        public string? UserAccount { get; set; }

        [Required(ErrorMessage = "請在框中輸入您遇到的問題,不可空白")]
        public string Description { get; set; }

        
    }
}
