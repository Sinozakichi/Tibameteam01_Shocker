using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
    public class ReturnreasonViewModel
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [MinLength(5,ErrorMessage ="評論不可少於5個字")]
        public string ReturnReason { get; set; }
    }
}
