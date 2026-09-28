using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto model);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto model);
    }
}
