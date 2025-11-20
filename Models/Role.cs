using System.ComponentModel.DataAnnotations;

namespace MediaMatch.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Ex: "Admin", "Curador", "Membro"

        // Propriedade de Navegação
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}