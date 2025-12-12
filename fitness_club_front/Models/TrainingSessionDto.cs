namespace fitness_club_front.Models
{
    public class TrainingSessionDto
    {
        public int Id { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string FitnessGoal { get; set; } = string.Empty;
        public int TrainerId { get; set; }
        public TrainerDto? Trainer { get; set; }
    }
}