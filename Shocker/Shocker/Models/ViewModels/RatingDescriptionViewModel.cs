using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
    public class RatingDescriptionViewModel
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        [MinLength(5,ErrorMessage ="評價不能低於5個字")]
        public string Description { get; set; }
    }
}

