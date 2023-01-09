using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Ocsp;
using Register_Login_Otp.Data;
using Register_Login_Otp.Model;
using System.Configuration;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
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


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var allUser = await _context.Registers.ToListAsync();
            return Ok(allUser);
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterVm register)
        {
            if (register == null)
                return NotFound(new {message="Please enter a Valid Details"});

            var checkUser=await _context.Registers.FirstOrDefaultAsync(i=>i.Email==register.Email);

            if (checkUser == null)
            {

                const int keySize = 64;
                const int iterations = 350000;

                HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

                byte[] salt = RandomNumberGenerator.GetBytes(keySize);
                string saltString = Convert.ToBase64String(salt);


                var hash = Rfc2898DeriveBytes.Pbkdf2(
                        Encoding.UTF8.GetBytes(register.Password),
                        salt,
                        iterations,
                        hashAlgorithm,
                        keySize);

                string hashPassword = Convert.ToBase64String(hash);




                DateTime currentTime = DateTime.Now;
                Register reg = new Register();
                reg.isVerified = false;
                reg.mailTime = currentTime.AddMinutes(5);
                reg.Name = register.Name;
                reg.Email = register.Email;
                reg.Password = hashPassword;

                _context.Registers.Add(reg);
                _context.SaveChanges();

                EncryptMail(register.Email);
            }
            else
            {
                EncryptMail(checkUser.Email);
            }

            return Ok(new { message = "Check your Mail And verify your account " });
        }




        [HttpPost]
        [Route("verifiedLink")]
        public async Task<IActionResult> VerifiedLink(string email)
        {
            // decrypt Email

            email = decrypyt(email);

            var userDetails = await _context.Registers.FirstOrDefaultAsync(i => i.Email == email);

            DateTime currentTime = DateTime.Now;


            if (userDetails == null)
            {
                return BadRequest(new {message="Email Does Not exist"});
            }

            else if (userDetails.mailTime <= currentTime)

                return BadRequest(new {message= "Email Verification Time is Expired"});

            else
            {
                userDetails.isVerified = true;
                _context.Registers.Update(userDetails);
                _context.SaveChanges();

            }
            return Ok(new {message= "Verified User"});
        }



        [HttpPost]
        [Route("ReVerifiedLink")]
        public async Task<IActionResult> ReVerifiedLink(string email)
        {
            email = decrypyt(email);
            var userDetails = await _context.Registers.FirstOrDefaultAsync(i => i.Email == email);

            DateTime currentTime = DateTime.Now;
            userDetails.mailTime = currentTime.AddMinutes(5);

            _context.Registers.Update(userDetails);
            _context.SaveChanges();

            EncryptMail(email);
            return Ok(new {message= "Check your Mail And Verify Your Account " });
        }





        [HttpPost]
        [Route("login")]

        public async Task<IActionResult> Login(LoginViewModel login)
        {

            if (login == null)
                return BadRequest(new {message="Invalid login details"});

            var UserDetails = await _context.Registers.FirstOrDefaultAsync(i => i.Email == login.Email);


            if (UserDetails != null && UserDetails.isVerified == true)
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
                message.Body = "Your Otp is " + randomNumber;
                message.Priority = MailPriority.High;

                SendMail(credentials, message);

            }

            else
            {
                return BadRequest(new {message= "Please Verify Your Email" });
            }

            return Ok( new { login,message = "Otp Send Your Mail Please check It" });

        }




        [HttpPost]
        [Route("verifiedOTP")]

        public async Task<IActionResult> VerifiedOtp(Otp otp)
        {

            if (otp == null)
                return BadRequest();

            var OtpFromDb = await _context.Registers.FirstOrDefaultAsync(i => i.OTP == otp.OTP.ToString());

            DateTime currentTime = DateTime.Now;

            if (OtpFromDb == null)
                return BadRequest(new {message= "Invalid Otp " });


            else if (OtpFromDb.OtpTime <= currentTime)
            return BadRequest(new { message = " Otp Verification Time is Expired" });

            else

                return Ok(new {message= "verification Successfully Completed" });

        }




        [HttpPost]
        [Route("ReSendOtp")]
        public async Task<IActionResult> ReSendOtp(string email)
        {

            var UserDetails = await _context.Registers.FirstOrDefaultAsync(i => i.Email == email);

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

            return Ok(new {message= "Otp Send Your Mail Please check it" });

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


        private void EncryptMail(string email)
        {
            // Encrypt Email
            var credentials = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
            byte[] clearBytes = Encoding.Unicode.GetBytes(email);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(credentials.EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    string encryptedMail = Convert.ToBase64String(ms.ToArray());

                    var callbackUrl = credentials.callbackUrl + encryptedMail;
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(credentials.Email);
                    message.To.Add(new MailAddress(email));
                    message.Subject = "Test";
                    message.IsBodyHtml = true; //to make message body as html  
                    message.Body = $"Click here to Verify your email <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

                    SendMail(credentials, message);
                }
            }
        }



        private string decrypyt(string email)
        {
            var credentials = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            email = email.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(email);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(credentials.EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);


                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    email = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return email;
        }



    }
}

