using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models.ViewModel
{
    public class ChangePasswordViewModel
    {
        public int id { get; set; } 
        public string? OldPassword { get; set; }
        [Required]
        [MinLength(8)]
        public string? NewPassword { get; set; }
        [Compare("NewPassword")]
        public string? ConfirmPassword { get; set; }

    }
}
