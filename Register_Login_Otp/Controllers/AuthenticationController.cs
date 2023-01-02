using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using Register_Login_Otp.Data;
using Register_Login_Otp.Model;
using System.Configuration;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Xml.Linq;

namespace Register_Login_Otp.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public IConfiguration _configuration;

        public AuthenticationController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterVm register)
        {
            if (register == null)
                return NotFound();

            DateTime currentTime = DateTime.Now;


            Register reg = new Register();
            reg.isVerified = false;
            reg.mailTime = currentTime.AddMinutes(5);
            reg.Name = register.Name;
            reg.Email = register.Email;
            reg.Password = register.Password;

            _context.Registers.Add(reg);
            _context.SaveChanges();



            var credentials = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            var callbackUrl = "https://localhost:7020/api/auth/verifiedLink?email=" + register.Email;
            MailMessage message = new MailMessage();
            message.From = new MailAddress(credentials.Email);
            message.To.Add(new MailAddress(register.Email));
            message.Subject = "Test";
            message.IsBodyHtml = true; //to make message body as html  
            message.Body = $"Click here to Verify your email <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

            SendMail(credentials, message);

            return Ok(new { message = "Sent Mail " });
        }
        





        [AllowAnonymous]
        [HttpGet]
        [Route("verifiedLink")]
        public async Task<IActionResult> VerifiedLink (string email)
        {

            var userDetails = _context.Registers.FirstOrDefault(i => i.Email == email);

            DateTime currentTime = DateTime.Now;


            if (userDetails == null)
            {
                return BadRequest();
            }

            else if (userDetails.mailTime <= currentTime)
                return BadRequest("Verification Time Expired ");

            else
            {
                userDetails.isVerified = true;
                _context.Registers.Update(userDetails);
                _context.SaveChanges();

            }
            return Ok("Verified User ");
        }



        [HttpPost]
        [Route("login")]

        public async Task<IActionResult> Login(LoginViewModel login)
        {

            if (login == null)
                return BadRequest();

            var UserDetails = _context.Registers.Where(i => i.Email == login.Email && i.Password == login.Password).FirstOrDefault();

            if (UserDetails != null && UserDetails.isVerified==true)
            {

                Random rnd = new Random();
                string randomNumber = (rnd.Next(100000, 999999)).ToString();


                DateTime currentTime = DateTime.Now;


                UserDetails.OTP = randomNumber;
                UserDetails.OtpTime = currentTime.AddMinutes(5);
                _context.Registers.Update(UserDetails);
                _context.SaveChanges();


                var credentials = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
                MailMessage message = new MailMessage();
                message.From = new MailAddress(credentials.Email);
                message.To.Add(new MailAddress(login.Email));
                message.Subject = "Test";
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = "Your Otp is " + randomNumber ;
                message.Priority = MailPriority.High;

                SendMail(credentials, message);
            }

            else
            {
                return BadRequest("Invalid Creadantials ");
            }

            return Ok("Please Verified your Otp confirmation");

        }

   


        [HttpPost]
        [Route("verifiedOTP")]

        public async Task<IActionResult> VerifiedOtp(Otp otp)
        {

            if (otp == null)
                return BadRequest();

            var OtpFromDb = _context.Registers.Where(i => i.OTP == otp.OTP).FirstOrDefault();

            DateTime currentTime = DateTime.Now;

            if (OtpFromDb.OTP == null)
                return BadRequest("Invalid Otp ");


            else if (OtpFromDb.OtpTime <= currentTime)
                return BadRequest("Verification Time Expired ");

            else

                return Ok("verification Successfully Completed");

        }




        [HttpPost]
        [Route("ReSendOtp")]
        public async Task<IActionResult> ReSendOtp(string email)
        {

            var UserDetails = _context.Registers.Where(i => i.Email == email).FirstOrDefault();

            Random rnd = new Random();
            string randomNumber = (rnd.Next(100000, 999999)).ToString();


            DateTime currentTime = DateTime.Now;


            UserDetails.OTP = randomNumber;
            UserDetails.OtpTime = currentTime.AddMinutes(5);
            _context.Registers.Update(UserDetails);
            _context.SaveChanges();


            var credentials = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
            MailMessage message = new MailMessage();
            message.From = new MailAddress(credentials.Email);
            message.To.Add(new MailAddress(email));
            message.Subject = "Test";
            message.IsBodyHtml = true; //to make message body as html  
            message.Body = "Your Otp is " + randomNumber;
            message.Priority = MailPriority.High;

            SendMail(credentials, message);

            return Ok("ReSend Otp Successfully");

        }



        private static void SendMail(EmailSettings credentials, MailMessage message)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {

                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(credentials.Email, credentials.Password);
                    smtp.Host = credentials.HOST;
                    smtp.Port = credentials.PORT;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}

