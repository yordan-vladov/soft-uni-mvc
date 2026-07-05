using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using soft_uni_mvc.Data;
using soft_uni_mvc.Models;

namespace soft_uni_mvc.Controllers;

public class EmployeesController : Controller
{
    private readonly SoftUniContext _context;

    public EmployeesController(SoftUniContext context)
    {
        _context = context;
    }

    private void PopulateDropdowns(Employee? employee = null)
    {
        ViewData["DepartmentId"] = new SelectList(_context.Departments.OrderBy(d => d.Name), "DepartmentId", "Name", employee?.DepartmentId);

        var employees = _context.Employees
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName });
        ViewData["ManagerId"] = new SelectList(employees, "EmployeeId", "FullName", employee?.ManagerId);

        ViewData["AddressId"] = new SelectList(_context.Addresses.OrderBy(a => a.AddressText), "AddressId", "AddressText", employee?.AddressId);
    }

    public async Task<IActionResult> Index()
    {
        var employees = _context.Employees.Include(e => e.Department).Include(e => e.Manager);
        return View(await employees.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.Address).ThenInclude(a => a!.Town)
            .Include(e => e.EmployeeProjects).ThenInclude(ep => ep.Project)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee is null) return NotFound();

        return View(employee);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,MiddleName,JobTitle,DepartmentId,ManagerId,HireDate,Salary,AddressId")] Employee employee)
    {
        if (ModelState.IsValid)
        {
            _context.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(employee);
        return View(employee);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var employee = await _context.Employees.FindAsync(id);
        if (employee is null) return NotFound();

        PopulateDropdowns(employee);
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FirstName,LastName,MiddleName,JobTitle,DepartmentId,ManagerId,HireDate,Salary,AddressId")] Employee employee)
    {
        if (id != employee.EmployeeId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Employees.AnyAsync(e => e.EmployeeId == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(employee);
        return View(employee);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee is null) return NotFound();

        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is not null) _context.Employees.Remove(employee);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
