using System.ComponentModel.DataAnnotations;

namespace ContosoUniversityTARgv21.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }

        [DisplayFormat(NullDisplayText = "No Grade")]
        public Grade? Grade { get; set; }

        public Course Course { get; set; }
        public Student Student { get; set; }
    }

    public enum Grade
    {
        A, B, C, D, E, F
    }
}