using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Ai
{
    public class AISymptomCheckRequestDto
    {
        public Guid? PatientId { get; set; }

        [Required(ErrorMessage = "Symptoms are required.")]
        [MinLength(2, ErrorMessage = "Symptoms must be at least 2 characters.")]
        [MaxLength(4000, ErrorMessage = "Symptoms cannot exceed 4000 characters.")]
        public string Symptoms { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int Age { get; set; }

        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters.")]
        public string? Gender { get; set; }

        [MaxLength(2000, ErrorMessage = "Medical history cannot exceed 2000 characters.")]
        public string? MedicalHistory { get; set; }

        [MaxLength(2000, ErrorMessage = "Current medications cannot exceed 2000 characters.")]
        public string? CurrentMedications { get; set; }

        [MaxLength(2000, ErrorMessage = "Allergies cannot exceed 2000 characters.")]
        public string? Allergies { get; set; }
    }
}
