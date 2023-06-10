
using System.ComponentModel.DataAnnotations;
namespace Shocker.Areas.Admin.Models.ViewModels
{
    public class ClientCaseViewModels
    {
        public int CaseId { get; set; }
        
        [Required(ErrorMessage ="請回覆內容,不能回復空白")]
        public string Reply { get; set; }
    }
}
