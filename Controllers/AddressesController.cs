using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using soft_uni_mvc.Data;
using soft_uni_mvc.Models;

namespace soft_uni_mvc.Controllers;

public class AddressesController : Controller
{
    private readonly SoftUniContext _context;

    public AddressesController(SoftUniContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var addresses = _context.Addresses.Include(a => a.Town);
        return View(await addresses.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var address = await _context.Addresses.Include(a => a.Town).FirstOrDefaultAsync(a => a.AddressId == id);
        if (address is null) return NotFound();

        return View(address);
    }

    public IActionResult Create()
    {
        ViewData["TownId"] = new SelectList(_context.Towns.OrderBy(t => t.Name), "TownId", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AddressText,TownId")] Address address)
    {
        if (ModelState.IsValid)
        {
            _context.Add(address);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["TownId"] = new SelectList(_context.Towns.OrderBy(t => t.Name), "TownId", "Name", address.TownId);
        return View(address);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var address = await _context.Addresses.FindAsync(id);
        if (address is null) return NotFound();

        ViewData["TownId"] = new SelectList(_context.Towns.OrderBy(t => t.Name), "TownId", "Name", address.TownId);
        return View(address);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("AddressId,AddressText,TownId")] Address address)
    {
        if (id != address.AddressId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(address);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Addresses.AnyAsync(a => a.AddressId == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["TownId"] = new SelectList(_context.Towns.OrderBy(t => t.Name), "TownId", "Name", address.TownId);
        return View(address);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var address = await _context.Addresses.Include(a => a.Town).FirstOrDefaultAsync(a => a.AddressId == id);
        if (address is null) return NotFound();

        return View(address);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address is not null) _context.Addresses.Remove(address);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
