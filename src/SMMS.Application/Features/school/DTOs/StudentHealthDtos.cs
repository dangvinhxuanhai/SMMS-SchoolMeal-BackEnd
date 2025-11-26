
namespace SMMS.Application.Features.school.DTOs
{
    public class StudentBMIResultDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string AcademicYear { get; set; }
        public decimal HeightCm { get; set; }
        public decimal WeightKg { get; set; }
        public decimal BMI { get; set; }
        public string BMIStatus { get; set; }
        public DateTime RecordAt { get; set; }
    }
}
