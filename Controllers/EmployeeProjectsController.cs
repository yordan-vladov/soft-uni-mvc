using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using soft_uni_mvc.Data;
using soft_uni_mvc.Models;

namespace soft_uni_mvc.Controllers;

public class EmployeeProjectsController : Controller
{
    private readonly SoftUniContext _context;

    public EmployeeProjectsController(SoftUniContext context)
    {
        _context = context;
    }

    private void PopulateDropdowns(int? employeeId = null, int? projectId = null)
    {
        var employees = _context.Employees
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName });
        ViewData["EmployeeId"] = new SelectList(employees, "EmployeeId", "FullName", employeeId);
        ViewData["ProjectId"] = new SelectList(_context.Projects.OrderBy(p => p.Name), "ProjectId", "Name", projectId);
    }

    public async Task<IActionResult> Index()
    {
        var assignments = _context.EmployeeProjects.Include(ep => ep.Employee).Include(ep => ep.Project);
        return View(await assignments.ToListAsync());
    }

    public async Task<IActionResult> Details(int? employeeId, int? projectId)
    {
        if (employeeId is null || projectId is null) return NotFound();

        var assignment = await _context.EmployeeProjects
            .Include(ep => ep.Employee)
            .Include(ep => ep.Project)
            .FirstOrDefaultAsync(ep => ep.EmployeeId == employeeId && ep.ProjectId == projectId);
        if (assignment is null) return NotFound();

        return View(assignment);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EmployeeId,ProjectId")] EmployeeProject employeeProject)
    {
        if (await _context.EmployeeProjects.AnyAsync(ep => ep.EmployeeId == employeeProject.EmployeeId && ep.ProjectId == employeeProject.ProjectId))
        {
            ModelState.AddModelError(string.Empty, "This employee is already assigned to this project.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(employeeProject);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(employeeProject.EmployeeId, employeeProject.ProjectId);
        return View(employeeProject);
    }

    public async Task<IActionResult> Delete(int? employeeId, int? projectId)
    {
        if (employeeId is null || projectId is null) return NotFound();

        var assignment = await _context.EmployeeProjects
            .Include(ep => ep.Employee)
            .Include(ep => ep.Project)
            .FirstOrDefaultAsync(ep => ep.EmployeeId == employeeId && ep.ProjectId == projectId);
        if (assignment is null) return NotFound();

        return View(assignment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int employeeId, int projectId)
    {
        var assignment = await _context.EmployeeProjects.FindAsync(employeeId, projectId);
        if (assignment is not null) _context.EmployeeProjects.Remove(assignment);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
