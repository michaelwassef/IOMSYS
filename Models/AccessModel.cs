namespace IOMSYS.Models
{
    public class AccessModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int UserTypeId { get; set; }
        public bool KeepLoggedIn { get; set; }
    }
}
