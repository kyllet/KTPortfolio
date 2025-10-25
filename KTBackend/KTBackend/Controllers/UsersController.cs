using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KTBackend.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KTBackend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController: ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;

        public UsersController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Register(UserCredentialsDTO userCredentialsDTO)
        {
            var user = new IdentityUser
            {
                UserName = userCredentialsDTO.Email,
                Email = userCredentialsDTO.Email
            };
            var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);

            if (result.Succeeded)
            {
                return await BuildToken(user);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Login(UserCredentialsDTO userCredentialsDTO)
        {
            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);

            if (user is null)
            {
                var errors = BuildIncorrectLoginErrorMessage();
                return BadRequest(errors);
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, userCredentialsDTO.Password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await BuildToken(user);
            }
            else
            {
                var errors = BuildIncorrectLoginErrorMessage();
                return BadRequest(errors);
            }
        }

        private IEnumerable<IdentityError> BuildIncorrectLoginErrorMessage()
        {
            var identityError = new IdentityError()
            {
                Description = "Incorrect login"
            };
            var errors = new List<IdentityError>();
            errors.Add(identityError);
            return errors;
        }
        private async Task<AuthenticationResponseDTO> BuildToken(IdentityUser user)
        {
            var claims = new List<Claim> {
                new Claim("email", user.Email!),
                new Claim("myclaim", "claim!")
            };
            var claimsDB = await userManager.GetClaimsAsync(user);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtkey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiration, signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponseDTO
            {
                Token = token,
                Expiration = expiration
            };
        }
    }
}
