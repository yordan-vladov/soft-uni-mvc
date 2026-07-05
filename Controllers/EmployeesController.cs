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

    public async Task<IActionResult> Index(string? searchString, int? departmentId, string? sortOrder, int? pageNumber)
    {
        ViewData["CurrentSort"] = sortOrder;
        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentDepartment"] = departmentId;
        ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["JobTitleSortParam"] = sortOrder == "jobtitle" ? "jobtitle_desc" : "jobtitle";
        ViewData["DepartmentSortParam"] = sortOrder == "department" ? "department_desc" : "department";
        ViewData["HireDateSortParam"] = sortOrder == "hiredate" ? "hiredate_desc" : "hiredate";
        ViewData["SalarySortParam"] = sortOrder == "salary" ? "salary_desc" : "salary";

        ViewData["DepartmentFilter"] = new SelectList(_context.Departments.OrderBy(d => d.Name), "DepartmentId", "Name", departmentId);

        var employees = _context.Employees.Include(e => e.Department).Include(e => e.Manager).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            employees = employees.Where(e =>
                e.FirstName.Contains(searchString) ||
                e.LastName.Contains(searchString) ||
                e.MiddleName!.Contains(searchString) ||
                e.JobTitle.Contains(searchString));
        }

        if (departmentId.HasValue)
        {
            employees = employees.Where(e => e.DepartmentId == departmentId);
        }

        employees = sortOrder switch
        {
            "name_desc" => employees.OrderByDescending(e => e.FirstName).ThenByDescending(e => e.LastName),
            "jobtitle" => employees.OrderBy(e => e.JobTitle),
            "jobtitle_desc" => employees.OrderByDescending(e => e.JobTitle),
            "department" => employees.OrderBy(e => e.Department.Name),
            "department_desc" => employees.OrderByDescending(e => e.Department.Name),
            "hiredate" => employees.OrderBy(e => e.HireDate),
            "hiredate_desc" => employees.OrderByDescending(e => e.HireDate),
            "salary" => employees.OrderBy(e => e.Salary),
            "salary_desc" => employees.OrderByDescending(e => e.Salary),
            _ => employees.OrderBy(e => e.FirstName).ThenBy(e => e.LastName),
        };

        const int pageSize = 10;
        var paginatedEmployees = await PaginatedList<Employee>.CreateAsync(employees, pageNumber ?? 1, pageSize);
        return View(paginatedEmployees);
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
