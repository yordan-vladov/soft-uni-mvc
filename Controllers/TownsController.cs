using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using soft_uni_mvc.Data;
using soft_uni_mvc.Models;

namespace soft_uni_mvc.Controllers;

public class TownsController : Controller
{
    private readonly SoftUniContext _context;

    public TownsController(SoftUniContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Towns.OrderBy(t => t.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var town = await _context.Towns.FirstOrDefaultAsync(t => t.TownId == id);
        if (town is null) return NotFound();

        return View(town);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Town town)
    {
        if (ModelState.IsValid)
        {
            _context.Add(town);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(town);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var town = await _context.Towns.FindAsync(id);
        if (town is null) return NotFound();

        return View(town);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TownId,Name")] Town town)
    {
        if (id != town.TownId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(town);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Towns.AnyAsync(t => t.TownId == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(town);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var town = await _context.Towns.FirstOrDefaultAsync(t => t.TownId == id);
        if (town is null) return NotFound();

        return View(town);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var town = await _context.Towns.FindAsync(id);
        if (town is not null) _context.Towns.Remove(town);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
