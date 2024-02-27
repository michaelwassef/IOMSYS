namespace IOMSYS.Models
{
    public class UserPermissionsAssignmentViewModel
    {
        public IEnumerable<UsersModel> Users { get; set; }
        public IEnumerable<PermissionModel> Permissions { get; set; }
        public int SelectedUserId { get; set; }
        public List<int> SelectedPermissionsIds { get; set; } = new List<int>();
    }
}
