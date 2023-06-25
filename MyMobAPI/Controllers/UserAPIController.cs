using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMobAPI.DTO.DTO;
using MyMobAPI.SQLite.Context;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyMobAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        private readonly UserStore<UserDTO, IdentityRole, MyMobAPIDBSQlite> _userStore;
        public readonly UserManager<UserDTO> _userManager;
        private readonly SignInManager<UserDTO> _signInManager;
        private readonly ILogger<UserAPIController> _logger;

        public UserAPIController(MyMobAPIDBSQlite db, UserManager<UserDTO> userManager, SignInManager<UserDTO> signInManager,
            ILogger<UserAPIController> logger)
        {
            _userStore = new UserStore<UserDTO, IdentityRole, MyMobAPIDBSQlite>(db);
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<UserDTO>> GetAllUsers() =>
            await _userStore.Users.ToArrayAsync();

        [HttpGet("user")]
        public async Task<UserDTO> GetUser(string userName) =>
            GetAllUsers().Result.FirstOrDefault(us => us.UserName == userName);

        [HttpGet("register")]
        public async Task<string> Register(string username, string password, string email, string comp, string fn, string sn)
        {
            _logger.BeginScope($"Регистрация пользователя {username}");
            if (await GetUser(username) == null)
            {
                var user = new UserDTO
                {
                    UserName = username,
                    SecondName = sn,
                    FirstName = fn,
                    Email = email,
                    Role = comp,
                };

                var registration_result = await _userManager.CreateAsync(user, password);

                if (registration_result.Succeeded)
                {
                    _logger.LogInformation("Пользователь {0} успешно зарегестрирован", username);

                    return (await GetUser(username)).Id;
                }
                else
                {
                    _logger.LogInformation("Пользователь {0} успешно не зарегестрирован", username);

                    return null;
                }
            }
            else
                return null;
        }

        [HttpGet("login")]
        public async Task<bool> Login(string username, string password)
        {
            var login_result = await _signInManager.PasswordSignInAsync(
                username,
                password,
                true,
                true
            );
            
            _logger.LogInformation($"Пользователь {username} {(login_result.Succeeded ? "" : "не") } успешно зашёл");

            return login_result.Succeeded;
        }

        [HttpGet("edit")]
        public async Task Edit(string username, string email, string text, string comp, string phone)
        {
            _logger.LogInformation("Пользователь {0} успешно редактируется", username);

            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                if (!string.IsNullOrEmpty(email))
                    user.Email = email;

                if (!string.IsNullOrEmpty(text))
                    user.LongText = text;

                if (!string.IsNullOrEmpty(comp))
                    user.Role = comp;

                if (!string.IsNullOrEmpty(phone))
                    user.PhoneNumber = phone;

                var result = await _userManager.UpdateAsync(user);
            }
        }
    }
}
