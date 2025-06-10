using Microsoft.AspNetCore.Mvc;
using NotesManagement.DTOs;
using NotesManagement.Services;
using NotesManagement.Helpers;

namespace NotesManagement.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;

        public AuthController(AuthService authService, TokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _authService.Register(dto.Username, dto.Password);
            if (user == null)
                return BadRequest(new { message = "Username already taken" });

            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiryDate = _tokenService.GetRefreshTokenExpiry();

            await _authService.AddRefreshToken(refreshToken, expiryDate, user.Id);

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _authService.ValidateUser(dto.Username, dto.Password);
            if (user == null)
                return Unauthorized();

            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiryDate = _tokenService.GetRefreshTokenExpiry();

            await _authService.AddRefreshToken(refreshToken, expiryDate, user.Id);

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var refreshToken = await _authService.GetValidRefreshToken(dto.RefreshToken);
            if (refreshToken == null)
                return Unauthorized();

            await _authService.RevokeRefreshToken(dto.RefreshToken);

            var newAccessToken = _tokenService.GenerateAccessToken(refreshToken.UserId, refreshToken.User.Username);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var expiryDate = _tokenService.GetRefreshTokenExpiry();

            await _authService.AddRefreshToken(newRefreshToken, expiryDate, refreshToken.UserId);

            return Ok(new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}