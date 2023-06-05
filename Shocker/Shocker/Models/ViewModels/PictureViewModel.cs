using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
    public class PictureViewModel
    {
        public string Id { get; set; }       
        public IFormFile Picture { get; set; }
    }
}
