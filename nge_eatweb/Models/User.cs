using System;

namespace nge_eatweb.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }
}
