using ContosoUniversityTARgv21.Data;
using ContosoUniversityTARgv21.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ContosoUniversityTARgv21.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly SchoolContext _context;

        public DepartmentsController
            (
                SchoolContext context
            )
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _context.Departments
                .Include(d => d.Administrator)
                .ToListAsync();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dep = await _context.Departments
                .Include(d => d.Administrator)
                .FirstOrDefaultAsync(m => m.DepartmentID == id);

            if (dep == null)
            {
                return NotFound();
            }

            return View(dep);
        }


        public IActionResult Create()
        {
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FirstMidName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department dep)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dep);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["InstructorID"] = new SelectList(_context.Instructors,
                "ID", "FirstMidName", dep.InstructorID);
            return View(dep);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dep = await _context.Departments.FindAsync(id);

            if (dep == null)
            {
                return NotFound();
            }

            ViewData["InstructorID"] = new SelectList(_context.Instructors,
                "ID", "FirstMidName", dep.InstructorID);

            return View(dep);
        }

    }
}
