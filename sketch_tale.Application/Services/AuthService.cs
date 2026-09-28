using Microsoft.AspNetCore.Identity;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto model)
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            return result.Succeeded;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!result.Succeeded) return null;

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty
            };
        }
    }
}
