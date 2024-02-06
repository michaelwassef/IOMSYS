namespace IOMSYS.Models
{
    public class UserBranchAssignmentViewModel
    {
        public IEnumerable<UsersModel> Users { get; set; }
        public IEnumerable<BranchesModel> Branches { get; set; }
        public int SelectedUserId { get; set; }
        public List<int> SelectedBranchIds { get; set; } = new List<int>();
    }
}
