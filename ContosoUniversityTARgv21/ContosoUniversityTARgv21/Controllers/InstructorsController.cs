using ContosoUniversityTARgv21.Data;
using ContosoUniversityTARgv21.Models;
using ContosoUniversityTARgv21.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversityTARgv21.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorsController
            (
                SchoolContext context
            )
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int? id, int? courseId)
        {
            var vm = new InstructorIndexData();

            vm.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Enrollments)
                            .ThenInclude(i => i.Student)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Department)
                .AsNoTracking()
                .OrderBy(i =>i.LastName)
                .ToListAsync();

            if(id != null)
            {
                ViewData["InstructorId"] = id.Value;
                Instructor instructor = vm.Instructors
                    .Where(i => i.Id == id.Value).Single();
                vm.Courses = instructor.CourseAssignments
                    .Select(s => s.Course);
            }

            if(courseId != null)
            {
                ViewData["CourseId"] = courseId.Value;
                var selectedCourse = vm.Courses
                    .Where(x => x.CourseId == courseId)
                    .Single();

                await _context.Entry(selectedCourse)
                    .Collection(x => x.Enrollments)
                    .LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment)
                        .Reference(x => x.Student)
                        .LoadAsync();
                }
                vm.Enrollments = selectedCourse.Enrollments;
            }

            return View(vm);
        }
    }
}
