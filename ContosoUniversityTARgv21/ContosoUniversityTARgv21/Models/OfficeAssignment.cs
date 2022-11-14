using System.ComponentModel.DataAnnotations;

namespace ContosoUniversityTARgv21.Models
{
    public class OfficeAssignment
    {
        [Key]
        public int InstructorID { get; set; }

        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; }

        public Instructor Intsructor { get; set; }
    }
}