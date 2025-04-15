
using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddDbContext<Context>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            //be able to inject JWTService class inside ourcontrollers.
            builder.Services.AddScoped<JWTService>();

            //Defining our IdentityCore Service
            builder.Services.AddIdentityCore<User>(options =>
            {
                //Password Congiguration
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                //For Email Confirmation.
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddRoles<IdentityRole>()  //to be able to add roles like (Admin/User)
                .AddRoleManager<RoleManager<IdentityRole>>() // to be able to make use of RoleManager like (Allows you to manage roles (create, delete, assign roles) using the RoleManager.)
                .AddEntityFrameworkStores<Context>() //Providing our Context like(Tells Identity to save all user/role data to your database using the given DbContext (Context).)
                .AddSignInManager<SignInManager<User>>() //Make use of singing manager like (Allows you to sign users in and out using the SignInManager.)
                .AddUserManager<UserManager<User>>() //make use of userManger to create Users like (Gives you full control to create, update, delete users using UserManager.)
                .AddDefaultTokenProviders(); // to be able to create tokens for email confirmation like (Adds support for generating tokens needed for: 1Email confirmation,2Password reset,3Two-factor authentication)

            //be able to authenticate users using JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // validate the token based on the key we have provided  inside appsetting.development.json JWT:key
                        ValidateIssuerSigningKey = true,

                        //the issuer signing key based on the JWT:key.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:key"])),
                        
                        //the issuer which in here is the api project url we are using
                        ValidIssuer = builder.Configuration["JWT:Issuer"],

                        //validate the issuer (who ever is issuing the JWT)
                        ValidateIssuer = true,

                        // dont validate the audience (angular side)
                        ValidateAudience = false,
                    };
                });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            //adding UseAuthentication() into our pipeline and this should come before UseAuthorization().
            //authentication varifies the identity of a user or service, and authorization determines their access rights.
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
