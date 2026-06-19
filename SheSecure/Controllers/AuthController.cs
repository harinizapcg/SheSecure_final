using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SheSecure.AuthService.DTOs;
using SheSecure.AuthService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SheSecure.AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            var userExists =
                await _userManager.FindByEmailAsync(model.Email);

            if (userExists != null)
            {
                return BadRequest("User already exists.");
            }

            var random = new Random();
            string userId;
            do
            {
                userId = random.Next(100000, 999999).ToString();
            } while (await _userManager.FindByIdAsync(userId) != null);

            ApplicationUser user = new()
            {
                Id = userId,
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // CREATE ROLE IF NOT EXISTS
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole(model.Role));
            }

            // ASSIGN ROLE
            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok("User registered successfully.");
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var user =
                await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var isPasswordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    model.Password);

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid email or password.");
            }

            var userRoles =
                await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),

                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            // ADD ROLE CLAIMS
            foreach (var role in userRoles)
            {
                authClaims.Add(
                    new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],

                audience: _configuration["Jwt:Audience"],

                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(
                        _configuration["Jwt:DurationInMinutes"])),

                claims: authClaims,

                signingCredentials:
                    new SigningCredentials(
                        authSigningKey,
                        SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token =
                    new JwtSecurityTokenHandler()
                        .WriteToken(token),

                expiration = token.ValidTo
            });
        }

        // GET USERS
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<object>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userList.Add(new
                {
                    id = u.Id,
                    email = u.Email,
                    fullName = u.FullName,
                    role = roles.FirstOrDefault() ?? "Employee"
                });
            }

            return Ok(userList);
        }

        // CHANGE ROLE
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!await _roleManager.RoleExistsAsync(dto.NewRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(dto.NewRole));
            }

            await _userManager.AddToRoleAsync(user, dto.NewRole);

            return Ok(new { message = "Role updated successfully." });
        }

        // DELETE USER
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to delete user.");
            }

            return Ok(new { message = "User deleted successfully." });
        }
    }
}