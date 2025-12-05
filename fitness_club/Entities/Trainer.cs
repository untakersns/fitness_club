using System.ComponentModel.DataAnnotations;

namespace fitness_club.Entities
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Certification { get; set; } = string.Empty;

        public int YearsOfExperience { get; set; }

        public decimal Rating { get; set; }

        public string Bio { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public DateTime HireDate { get; set; }

        // Navigation property
        public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
    }
}