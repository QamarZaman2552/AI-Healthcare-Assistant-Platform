using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Ai
{
    public class AISymptomCheckRequestDto
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(4000)]
        public string Symptoms { get; set; } = string.Empty;

        [Range(1, 120)]
        public int Age { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(2000)]
        public string? MedicalHistory { get; set; }

        [MaxLength(2000)]
        public string? CurrentMedications { get; set; }

        [MaxLength(2000)]
        public string? Allergies { get; set; }
    }
}
