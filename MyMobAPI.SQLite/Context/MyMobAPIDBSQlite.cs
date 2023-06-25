using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMobAPI.DTO.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMobAPI.SQLite.Context
{
    public class MyMobAPIDBSQlite : IdentityDbContext<UserDTO>
    {
        public DbSet<UserDTO> Users { get; set; }
        public MyMobAPIDBSQlite(DbContextOptions<MyMobAPIDBSQlite> options) : base(options) { }
    }
}
