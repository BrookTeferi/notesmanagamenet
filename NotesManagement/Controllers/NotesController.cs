using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesManagement.Models;
using NotesManagement.Data;
using System.Security.Claims;
using NotesManagement.DTOs;
using Microsoft.EntityFrameworkCore;

namespace NotesManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly UserDbContext _context;

        public NotesController(UserDbContext context)
        {
            _context = context;
        }

        // GET /api/notes
        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .ToListAsync();

            var response = notes.Select(n => new NoteResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                UserId = n.UserId
            });

            return Ok(response);
        }

        // POST /api/notes
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _context.Notes.AddAsync(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt,
                UserId = note.UserId
            });
        }

        // GET /api/notes/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetNoteById(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User ID Claim: {userIdClaim}");

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "You are not authenticated" });
            }

            var note = await _context.Notes
                .Where(n => n.UserId == userId && n.Id == id)
                .FirstOrDefaultAsync();

            Console.WriteLine($"Looking for Note ID: {id}, User ID: {userId}");

            if (note == null)
            {
                return NotFound(new { error = $"No note found with ID: {userId}" });
            }

            var response = new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt,
                UserId = note.UserId
            };

            return Ok(response);
        }
    }
}