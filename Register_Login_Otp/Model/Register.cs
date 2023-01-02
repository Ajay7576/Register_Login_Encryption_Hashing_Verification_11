using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Register_Login_Otp.Model
{
    public class Register 
    {
        public Register()
        {
           isVerified= false;
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    
        public string? OTP { get; set; }

        public DateTime mailTime  { get; set; }

        public bool isVerified  { get; set; }

        public DateTime OtpTime  { get; set; }


    }
}
