namespace IOMSYS.Models
{
    public class UsersModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
        public int UserTypeId { get; set; }
        public string UserTypeName { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<UserTypesModel> UserTypes { get; set; }
    }
}
