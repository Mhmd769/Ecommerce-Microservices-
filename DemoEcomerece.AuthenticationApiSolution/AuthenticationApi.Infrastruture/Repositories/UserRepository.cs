using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Infrastruture.Data;
using eCommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApi.Infrastruture.Repositories
{
    public class UserRepository(AuthenticationDBContext context, IConfiguration config) : IUser
    {
        private async Task<AppUser> GetUserByEmail(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user is null ? null! : user!;
        }
        public async Task<GetUserDTO> GetUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user is not null ? new GetUserDTO(user.Id,
                user.Name!,
                user.TelephoneNumber!,
                user.Address!,
                user.Email!,
                user.Role!):null!;
        }

        public async Task<Response> Login(LoginDTO loginDTO)
        {
            var getUser = await GetUserByEmail(loginDTO.Email);
            if (getUser is null)
                return new Response(false, "Invaild Credentials");
            bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password,getUser.Password);
            if (!verifyPassword)
                return new Response(false, "password didn`t match ");
            string token = GenerateToken(getUser);
            return new Response(true, token);

        }

        private string GenerateToken(AppUser user)
        {
            var keyString = config["Authentication:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT Secret Key is missing!");

            var key = Encoding.UTF8.GetBytes(keyString);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email ?? "")
            };

            if (!string.IsNullOrEmpty(user.Role) && !user.Role.Equals("string", StringComparison.OrdinalIgnoreCase))
                claims.Add(new(ClaimTypes.Role, user.Role));

            var token = new JwtSecurityToken(
                 issuer: config["Authentication:Issuer"],  // Fixed Typo
                 audience: config["Authentication:Audience"],
                 claims: claims,
                 expires: DateTime.UtcNow.AddHours(1),
                 signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<Response> Register(AppUserDTO appUserDTO)
        {
            var getuser = await GetUserByEmail(appUserDTO.Email);
            if (getuser is not null)
                return new Response(false, $"you cannot use this email for this registration");
            var result = context.Users.Add(new AppUser()
            {
                Name = appUserDTO.Name,
                Email = appUserDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(appUserDTO.Password),
                TelephoneNumber = appUserDTO.TelephoneNumber,
                Address = appUserDTO.Address,
                Role = appUserDTO.Role
            });
            await context.SaveChangesAsync();
            return result.Entity.Id > 0 ? new Response(true, $"User {appUserDTO.Name} registration done Successfully") :
                new Response(false, "Invaild data provided");
        }
    }
}
