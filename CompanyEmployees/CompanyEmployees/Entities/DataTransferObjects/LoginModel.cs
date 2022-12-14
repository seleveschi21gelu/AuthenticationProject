using System.ComponentModel.DataAnnotations;

namespace CompanyEmployees.Entities.DataTransferObjects
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }

        public string? ClientURI { get; set; }
        
        public string? RefreshToken { get; set; }
        
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
