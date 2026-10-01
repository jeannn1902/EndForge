using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EndForge.Models
{
    public class ConcesionXPDb
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        public string Clave { get; set; } = string.Empty;

        public int CantidadXP { get; set; }

        public DateTime FechaUtc { get; set; }

        public string? PracticaId { get; set; }

        public string? TemaId { get; set; }

        // Navegación
        public Usuario? Usuario { get; set; }
    }
}
