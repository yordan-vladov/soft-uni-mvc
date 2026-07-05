using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using soft_uni_mvc.Data;
using soft_uni_mvc.Models;

namespace soft_uni_mvc.Controllers;

public class DepartmentsController : Controller
{
    private readonly SoftUniContext _context;

    public DepartmentsController(SoftUniContext context)
    {
        _context = context;
    }

    private SelectList ManagerSelectList(int? selectedId = null)
    {
        var employees = _context.Employees
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName });
        return new SelectList(employees, "EmployeeId", "FullName", selectedId);
    }

    public async Task<IActionResult> Index()
    {
        var departments = _context.Departments.Include(d => d.Manager);
        return View(await departments.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var department = await _context.Departments.Include(d => d.Manager).FirstOrDefaultAsync(d => d.DepartmentId == id);
        if (department is null) return NotFound();

        return View(department);
    }

    public IActionResult Create()
    {
        ViewData["ManagerId"] = ManagerSelectList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,ManagerId")] Department department)
    {
        if (ModelState.IsValid)
        {
            _context.Add(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["ManagerId"] = ManagerSelectList(department.ManagerId);
        return View(department);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var department = await _context.Departments.FindAsync(id);
        if (department is null) return NotFound();

        ViewData["ManagerId"] = ManagerSelectList(department.ManagerId);
        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,Name,ManagerId")] Department department)
    {
        if (id != department.DepartmentId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Departments.AnyAsync(d => d.DepartmentId == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["ManagerId"] = ManagerSelectList(department.ManagerId);
        return View(department);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var department = await _context.Departments.Include(d => d.Manager).FirstOrDefaultAsync(d => d.DepartmentId == id);
        if (department is null) return NotFound();

        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department is not null) _context.Departments.Remove(department);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
