using System.ComponentModel.DataAnnotations;

namespace fitness_club.Entities
{
    public class TrainingSession
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string SessionName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string SessionType { get; set; } = string.Empty; // "Групповая" или "Индивидуальная"

        [MaxLength(50)]
        public string DifficultyLevel { get; set; } = string.Empty; // "Начинающий", "Средний", "Продвинутый"

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int MaxParticipants { get; set; }

        public int CurrentParticipants { get; set; }

        public decimal Price { get; set; }

        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [MaxLength(50)]
        public string FitnessGoal { get; set; } = string.Empty; // "Похудение", "Набор мышц", "Общая физподготовка"

        // Foreign key
        public int TrainerId { get; set; }

        // Navigation property
        public virtual Trainer? Trainer { get; set; }
    }
}
