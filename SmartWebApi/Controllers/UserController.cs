using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace AspnetUserApi
{
    // Model reprezentujący użytkownika
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    // DbContext – konfiguracja bazy danych
    public class UserDbContext : DbContext
    {
        private readonly string _connectionString;

        // Konstruktor używany przez DI, konfiguracja odbywa się z poziomu Startup/Program.cs
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        // Alternatywny konstruktor przyjmujący connection string
        public UserDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Jeśli nie został skonfigurowany DbContextOptions, używamy connection string
        protected async override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionString))
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }

        public DbSet<User> Users { get; set; }
    }

    #region Repozytorium

    // Interfejs repozytorium użytkownika
    public interface IUserRepository
    {
        void Add(User user);
        User Get(int id);
        IEnumerable<User> ListAll();
        void Update(User user);
        void Delete(int id);
    }

    // Implementacja repozytorium użytkownika
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _dbContext;
        public UserRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async void Add(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }
        public User Get(int id)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Id == id);
        }
        public IEnumerable<User> ListAll()
        {
            return _dbContext.Users.ToList();
        }
        public async void Update(User user)
        {
            _dbContext.Users.Update(user);
            _dbContext.SaveChanges();
        }
        public async void Delete(int id)
        {
            var user = _dbContext.Users.Find(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
            }
        }
    }

    #endregion

    #region Serwis

    // Interfejs serwisu użytkownika
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(int id);
        void AddUser(User user);
        void UpdateUser(User user);
        bool DeleteUser(int id);
    }

    // Implementacja serwisu użytkownika – wstrzykuje IUserRepository
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public IEnumerable<User> GetAllUsers() => _userRepository.ListAll();
        public User GetUserById(int id) => _userRepository.Get(id);
        public async void AddUser(User user) => _userRepository.Add(user);
        public async void UpdateUser(User user) => _userRepository.Update(user);
        public bool DeleteUser(int id)
        {
            var user = _userRepository.Get(id);
            if (user == null)
                return false;
            _userRepository.Delete(id);
            return true;
        }
    }

    #endregion

    #region Kontroler

    // Kontroler z wstrzykniętym IUserService
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers() =>
            Ok(_userService.GetAllUsers());
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
                return NotFound($"User with ID {id} not found.");
            return Ok(user);
        }
        [HttpPost]
        public ActionResult<User> AddUser([FromBody] User newUser)
        {
            _userService.AddUser(newUser);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.Id)
                return BadRequest();
            _userService.UpdateUser(user);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var success = _userService.DeleteUser(id);
            if (!success)
                return NotFound($"User with ID {id} not found.");
            return NoContent();
        }

        [HttpGet]
        [Route("admin-only")]
        [RoleAuthorize("Admin")]
        public IActionResult GetAdminData()
        {
            return Ok("Only admin can see this message");
        }
    }

    #endregion

    // Program – konfiguracja aplikacji, Autofac oraz rejestracja zależności
    public class Program
    {
        public async static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Użycie Autofac jako kontenera DI
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Rejestracja kontrolerów
            builder.Services.AddControllers();

            // Rejestracja EF Core przy użyciu PostgreSQL
            // Upewnij się, że w appsettings.json masz klucz "DefaultConnection" z poprawnym connection stringiem
            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Konfiguracja EF Core (przykład z bazą InMemory, można użyć UseNpgsql dla PostgreSQL)
            // builder.Services.AddDbContext<UserDbContext>(options =>
            //     options.UseInMemoryDatabase("UserDb"));

            // Przeniesienie rejestracji usług do Autofac
            // var containerBuilder = new ContainerBuilder();
            // containerBuilder.Populate(builder.Services);

            // Rejestracja repozytorium i serwisu w Autofac
            // containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            // containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();

            // builder.Host.ConfigureContainer<ContainerBuilder>(cb => {
            builder.Host.ConfigureContainer<ContainerBuilder>(cb =>
                {
                    cb.Populate(builder.Services);
                    cb.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
                    cb.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
                });
            // });

            var app = builder.Build();

            // Konfiguracja middleware
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}