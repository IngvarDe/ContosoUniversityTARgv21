using ContosoUniversityTARgv21.Data;
using ContosoUniversityTARgv21.Models;
using ContosoUniversityTARgv21.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorToUpdate = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (await TryUpdateModelAsync<Instructor>
                (
                    instructorToUpdate,
                    "",
                    i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment
                ))
            {
                if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                {
                    instructorToUpdate.OfficeAssignment = null;
                }
                UpdateInstructorCourses(selectedCourses, instructorToUpdate);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem presists, " +
                        " see your system administrator."
                        );
                }

                return RedirectToAction(nameof(Index));
            }

            UpdateInstructorCourses(selectedCourses, instructorToUpdate);
            PopulateAssignedCourseData(instructorToUpdate);

            return View(instructorToUpdate);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Instructor instructor = await _context.Instructors
                .Include(i => i.CourseAssignments)
                .SingleAsync(i => i.Id == id);

            var dep = await _context.Departments
                .Where(d => d.InstructorId == id)
                .ToListAsync();
            dep.ForEach(d => d.InstructorId = null);

            _context.Instructors.Remove(instructor);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            PopulateAssignedCourseData(instructor);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new CourseAssignment
                    {
                        InstructorId = instructor.Id,
                        CourseId = int.Parse(course)
                    };
                    instructor.CourseAssignments.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }
            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.CourseAssignments
                .Select(c => c.Course.CourseId));

            foreach (var course in _context.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseId.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseId))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment
                        {
                            InstructorId = instructorToUpdate.Id,
                            CourseId = course.CourseId,
                        });
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseId))
                    {
                        CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments
                            .FirstOrDefault(c => c.CourseId == course.CourseId);
                        _context.Remove(courseToRemove);
                    }
                }
            }
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Courses;
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments
                .Select(c => c.CourseId));
            var vm = new List<AssignedCourseData>();

            foreach (var course in allCourses)
            {
                vm.Add(new AssignedCourseData
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseId)
                });
            }
            ViewData["Courses"] = vm;
        }
    }
}
