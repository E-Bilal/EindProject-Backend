namespace BackendAPI.Configurations
{
    public class MailSettings
    {
        public string? Displayname { get; set; }
        public string? From { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool UseSSL { get; set; }

    }
}
