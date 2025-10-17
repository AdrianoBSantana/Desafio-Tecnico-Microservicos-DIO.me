using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Shared.Models;
using Shared.Services;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService; // Injeta o serviço compartilhado

        public AuthController(IConfiguration configuration, JwtService jwtService)
        {
            _configuration = configuration;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Autentica um usuário e retorna um token JWT se as credenciais forem válidas.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // ATENÇÃO: Autenticação simulada com usuários em memória.
            // Em produção, substitua por um sistema de identidade real.
            var user = GetHardcodedUsers().FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            // Usa o serviço compartilhado para gerar o token
            var token = _jwtService.GenerateToken(user);
            
            return Ok(new LoginResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                Username = user.Username
            });
        }

        // Método auxiliar que retorna uma lista fixa de usuários para simulação.
        private List<User> GetHardcodedUsers()
        {
            return new List<User>
            {
                new User { Id = 1, Username = "admin", Password = "admin123", Role = UserRoles.Admin },
                new User { Id = 2, Username = "vendas", Password = "vendas123", Role = UserRoles.Vendas },
                new User { Id = 3, Username = "estoque", Password = "estoque123", Role = UserRoles.Estoque }
            };
        }
    }
}
