using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConsultorioApi.Models;
using ConsultorioApi.Services;

namespace ConsultorioApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly JwtSettings _jwtSettings;

        public AuthController(UsuarioService usuarioService, IOptions<JwtSettings> jwtSettings)
        {
            _usuarioService = usuarioService;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo) ||
                string.IsNullOrWhiteSpace(request.Contrasena) ||
                string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new { error = "Correo, contraseña y nombre son obligatorios" });
            }

            if (await _usuarioService.ExisteCorreo(request.Correo))
                return BadRequest(new { error = "Ya existe una cuenta con ese correo" });

            var usuario = new Usuario
            {
                Correo = request.Correo,
                Contrasena = request.Contrasena,
                Nombre = request.Nombre,
                Rol = "admin"
            };

            await _usuarioService.Crear(usuario);

            var token = GenerarToken(usuario);
            return StatusCode(201, new { mensaje = "Cuenta creada exitosamente", token, nombre = usuario.Nombre });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contrasena))
                return BadRequest(new { error = "Correo y contraseña son obligatorios" });

            var usuario = await _usuarioService.ObtenerPorCorreo(request.Correo);
            if (usuario == null || !_usuarioService.VerificarContrasena(request.Contrasena, usuario.Contrasena))
                return Unauthorized(new { error = "Correo o contraseña incorrectos" });

            var token = GenerarToken(usuario);
            return Ok(new { token, nombre = usuario.Nombre });
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet("perfil")]
        public IActionResult Perfil()
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            return Ok(new { correo });
        }

        private string GenerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegistroRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}
