﻿namespace IOMSYS.Models
{
    public class BranchesModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LandlinePhone { get; set; }
        public string? Address { get; set; }
        public byte[]? BranchLogo { get; set; }
        public int? BranchMangerId { get; set; }
        public string? Password { get; set; }
        public int SupplierId { get; set; }
        public int CustomerId { get; set; }
    }
}
