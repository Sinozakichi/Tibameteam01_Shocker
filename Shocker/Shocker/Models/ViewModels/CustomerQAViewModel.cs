﻿using System.ComponentModel.DataAnnotations;

namespace Shocker.Models.ViewModels
{
    public class CustomerQAViewModel
    {
        public int CaseId { get; set; }

        [Required(ErrorMessage = "請選擇問題類型")]
        public int QuestionCategoryId { get; set; }
        public string? UserAccount { get; set; }

        public string? AdminAccount { get; set; }
        [Required(ErrorMessage = "請在框框中輸入您遇到的問題")]

        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? CloseDate { get; set; }
        public string? Reply { get; set; }
    }
}
