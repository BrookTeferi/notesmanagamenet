using System.ComponentModel.DataAnnotations;

namespace NotesManagement.DTOs
{
    public class UpdateNoteDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}