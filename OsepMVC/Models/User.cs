using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OsepMVC.Models
{
    public class User
    {
        public User()
        {
            Events = new HashSet<Event>();
            Votes = new HashSet<Vote>();
            Comments = new HashSet<Comment>(); 
            Participations = new HashSet<EventParticipation>(); 
        }

        public int UserId { get; set; }

        [Required(ErrorMessage = "İsim gereklidir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyisim gereklidir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Rol gereklidir.")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Üniversite gereklidir.")]
        public string University { get; set; }

        public bool IsAdmin { get; set; }

        public ICollection<Event> Events { get; set; }
        public ICollection<Vote> Votes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<EventParticipation> Participations { get; set; }
    }
}
