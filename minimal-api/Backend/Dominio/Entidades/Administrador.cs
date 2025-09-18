using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Dominio.Entidades
{
    public class Administrador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)] // Aumentado para acomodar o hash da senha
        public string Senha { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Perfil { get; set; } = string.Empty;
    }
}
