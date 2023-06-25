using Microsoft.AspNetCore.Identity;

namespace MyMobAPI.DTO.DTO
{
    public class UserDTO : IdentityUser
    {
        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string Role { get; set; }

        public string TeamName { get; set; }

        public string LongText { get; set; }
    }
}
