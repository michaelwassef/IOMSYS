namespace IOMSYS.Models
{
    public class PermissionModel
    {
        public int PermissionId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string DisplayPermissionName { get; set; } 
    }
}
