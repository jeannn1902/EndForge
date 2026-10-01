using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EndForge.Models
{
    public class EvaluacionDb
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        // Propiedades del JSON de evaluación
        public string? PracticaId { get; set; }

        public double Calificacion { get; set; }

        public DateTime FechaEvaluacion { get; set; }

        public bool Aprobada { get; set; }

        // Navegación
        public Usuario? Usuario { get; set; }
    }
}
