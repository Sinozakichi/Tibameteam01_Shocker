using Microsoft.Build.Framework;

namespace Shocker.Areas.Admin.Models.ViewModels
{
    public class ClientCaseViewModels
    {
        public int CaseId { get; set; }
        public string AdminAccount { get; set; }
        [Required]
        public string Reply { get; set; }
    }
}
