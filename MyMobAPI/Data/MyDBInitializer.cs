using Castle.Core.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMobAPI.DTO.DTO;
using MyMobAPI.SQLite.Context;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyMobAPI.Data
{
    public class MyDBInitializer
    {
        private readonly MyMobAPIDBSQlite _db;
        private readonly ILogger<MyDBInitializer> _logger;
        private readonly UserManager<UserDTO> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private const string ADMIN = "Administrator";
        private const string USER = "USER";

        public MyDBInitializer(MyMobAPIDBSQlite db,
            ILogger<MyDBInitializer> logger,
            UserManager<UserDTO> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Initialize()
        {
            var timer = Stopwatch.StartNew();
            _logger.LogInformation("Инициализация базы данных...");

            //_db.Database.EnsureDeleted();
            //_db.Database.EnsureCreated();

            var db = _db.Database;

            if (db.GetPendingMigrations().Any())
            {
                _logger.LogInformation("Выполнение миграций...");
                db.Migrate();
                _logger.LogInformation("Выполнение миграций выполнено успешно");
            }
            else
                _logger.LogInformation("База данных находится в актуальной версии ({0:0.0###} c)",
                    timer.Elapsed.TotalSeconds);

            try
            {
                //InitializeProducts();
                await InitializeIdentityAsync();

            }
            catch (Exception error)
            {
                _logger.LogError(error, "Ошибка при выполнении инициализации БД :(");
            }

            _logger.LogInformation("Инициализация БД выполнена успешно {0}",
                timer.Elapsed.TotalSeconds);
        }

        private async Task InitializeIdentityAsync()
        {
            var timer = Stopwatch.StartNew();
            _logger.LogInformation("Инициализация системы Identity...");

            await CheckRole(ADMIN);
            await CheckRole(USER);

            if (await _userManager.FindByNameAsync(ADMIN) is null)
            {
                _logger.LogInformation("Отсутствует учётная запись администратора");
                var admin = new UserDTO
                {
                    UserName = ADMIN,
                    FirstName = ADMIN,
                    SecondName = "Adminych",
                    Role = ADMIN,
                    TeamName = "Админы",
                    LongText = "Я админ)",
                };

                var creation_result = await _userManager.CreateAsync(admin, $"{ADMIN}123&");
                if (creation_result.Succeeded)
                {
                    _logger.LogInformation("Учётная запись администратора создана успешно.");
                    await _userManager.AddToRoleAsync(admin, ADMIN);
                    _logger.LogInformation("Учётная запись администратора наделена ролью {0}", ADMIN);
                }
                else
                {
                    var errors = creation_result.Errors.Select(e => e.Description);
                    throw new InvalidOperationException($"Ошибка при создании учётной записи администратора:( ({string.Join(",", errors)})");
                }
            }

            _logger.LogInformation("Инициализация системы Identity завершена успешно за {0:0.0##}с", timer.Elapsed.Seconds);
        }

        private async Task CheckRole(string RoleName)
        {
            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                _logger.LogInformation("Роль {0} отсуствует. Создаю...");
                await _roleManager.CreateAsync(new IdentityRole { Name = RoleName });
                _logger.LogInformation("Роль {0} создана успешно");
            }
        }
    }
}
