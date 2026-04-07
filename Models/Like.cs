using DAL;
using Newtonsoft.Json;
using System;

namespace Models
{
    public class Like
    {
        public int UserId { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        [JsonIgnore]
        public User User => DB.Users.Get(UserId)?.Copy();
    }
}
