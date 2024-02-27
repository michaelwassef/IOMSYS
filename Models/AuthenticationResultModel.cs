namespace IOMSYS.Models
{
    public class AuthenticationResultModel
    {
        public bool IsAuthenticated { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public int UserTypeId { get; set; }
    }
}
