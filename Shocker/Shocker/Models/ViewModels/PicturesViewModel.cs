namespace Shocker.Models.ViewModels
{
    public class PicturesViewModel
    {
        public IFormFile Picture { get; set; }
        public int ProductId { get; set; }
        public string? Description { get; set; }
    }
}
