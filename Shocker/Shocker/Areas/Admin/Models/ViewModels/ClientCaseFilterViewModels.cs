namespace Shocker.Areas.Admin.Models.ViewModels
{
    public class ClientCaseFilterViewModels
    {
        public int CaseId { get; set; }
        public string CategoryName { get; set; }
        public string UserAccount { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? CloseDate { get; set; }
    }
}
