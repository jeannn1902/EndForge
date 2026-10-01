using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EndForge.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string HashPassword { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public DateTime UltimoAcceso { get; set; }

        // Navegación opcional
        public ICollection<ProgresoUsuarioDb>? Progresos { get; set; }

        public ICollection<EvaluacionDb>? Evaluaciones { get; set; }

        public ICollection<ConcesionXPDb>? ConcesionesXP { get; set; }
    }
}
