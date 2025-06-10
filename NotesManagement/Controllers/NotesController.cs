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
                return Unauthorized(new { error = "You are not authenticated" });

            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (!notes.Any())
            {
                return Ok(new { message = "You have no notes yet." });
            }

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
                return BadRequest(new { error = "Invalid request data", details = ModelState });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "You are not authenticated" });

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

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "You are not authenticated" });

            var note = await _context.Notes
                .Where(n => n.UserId == userId && n.Id == id)
                .FirstOrDefaultAsync();

            if (note == null)
            {
                return NotFound(new { error = $"No note found with ID: {id}" });
            }

            return Ok(new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt,
                UserId = note.UserId
            });
        }

        // PUT /api/notes/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateNote(Guid id, [FromBody] UpdateNoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid request data", details = ModelState });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "You are not authenticated" });

            var note = await _context.Notes
                .Where(n => n.UserId == userId && n.Id == id)
                .FirstOrDefaultAsync();

            if (note == null)
                return NotFound(new { error = $"Note with ID: {id} not found." });

            note.Title = dto.Title;
            note.Content = dto.Content;
            note.UpdatedAt = DateTime.UtcNow;

            _context.Notes.Update(note);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Note updated successfully", noteId = id });
        }

        // DELETE /api/notes/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "You are not authenticated" });

            var note = await _context.Notes
                .Where(n => n.UserId == userId && n.Id == id)
                .FirstOrDefaultAsync();

            if (note == null)
                return NotFound(new { error = $"Note with ID: {id} not found." });

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Note deleted successfully", noteId = id });
        }
    }
}