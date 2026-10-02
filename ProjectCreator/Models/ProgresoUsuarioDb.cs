using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EndForge.Models
{
    public class ProgresoUsuarioDb
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        // Ejemplos de propiedades que venían en JSON
        public string? PracticaId { get; set; }

        public string? RutaProyecto { get; set; }

        public int PuntosXP { get; set; }

        public DateTime FechaUltimaActualizacion { get; set; }

        // Navegación
        public Usuario? Usuario { get; set; }
    }
}
