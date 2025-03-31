using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PRN222_Assm.Helper;
using PRN222_Assm.Models;

namespace PRN222_Assm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DBContext");
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            builder.Services.AddDbContext<PrnassmContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            builder.Services.AddScoped<SendMail>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Studdent", policy => policy.RequireRole("Studdent"));
                options.AddPolicy("Tearcher", policy => policy.RequireRole("Tearcher"));
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            });

            var app = builder.Build();

            // create admin if not exits
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<PrnassmContext>();

                //  
                var adminUser = context.Accounts.FirstOrDefault(a => a.Email == "admin@gmail.com" && a.Role == 3);
                if (adminUser == null)
                {
                    //  
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword("1234");

                    var newAdmin = new Account
                    {
                        Email = "admin@gmail.com",
                        Password = hashedPassword,
                        Name = "Admin",
                        Role = 3,
                        Code = "admin001",
                        Phone = "1234567890",
                        Address = "Admin Address",
                        Dob = DateOnly.FromDateTime(DateTime.Now)
                    };

                    // Th m user v o database
                    context.Accounts.Add(newAdmin);
                    context.SaveChanges();
                }
            }



            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.MapHub<SignalRHub>("/signalrHub");

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
