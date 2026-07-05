using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using soft_uni_mvc.Data;
using soft_uni_mvc.Models;

namespace soft_uni_mvc.Controllers;

public class ProjectsController : Controller
{
    private readonly SoftUniContext _context;

    public ProjectsController(SoftUniContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Projects.OrderBy(p => p.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var project = await _context.Projects
            .Include(p => p.EmployeeProjects).ThenInclude(ep => ep.Employee)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
        if (project is null) return NotFound();

        return View(project);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate")] Project project)
    {
        if (ModelState.IsValid)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var project = await _context.Projects.FindAsync(id);
        if (project is null) return NotFound();

        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProjectId,Name,Description,StartDate,EndDate")] Project project)
    {
        if (id != project.ProjectId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Projects.AnyAsync(p => p.ProjectId == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
        if (project is null) return NotFound();

        return View(project);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project is not null) _context.Projects.Remove(project);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
