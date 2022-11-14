namespace ContosoUniversityTARgv21.Models
{
    public class CourseAssignment
    {
        public int InstructorId { get; set; }
        public int CourseID { get; set; }
        public Instructor Instructor { get; set; }
        public Course Course { get; set; }
    }
}