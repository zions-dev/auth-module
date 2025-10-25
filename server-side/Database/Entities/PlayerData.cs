using System;

namespace GameServer.Database.Entities
{
    public class PlayerData
    {
        public int Id { get; set; }
        public string SocialClubName { get; set; }
        public ulong SocialClubId { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string ResetCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}