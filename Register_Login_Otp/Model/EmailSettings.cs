namespace Register_Login_Otp.Model
{
    public class EmailSettings
    {

        public int PORT { get; set; }
        public string HOST  { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string callbackUrl { get; set; }
        public string EncryptionKey  { get; set; }


    }
}
