using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_6___Group_4___CSCN73060_SEC_1.Data;
using Project_6___Group_4___CSCN73060_SEC_1.Models;

namespace Project_6___Group_4___CSCN73060_SEC_1.Controllers
{
    public class BuildsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BuildsController> _logger;

        public BuildsController(ApplicationDbContext context, ILogger<BuildsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Builds
        public async Task<IActionResult> Index()
        {
            var builds = await _context.Builds
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Memory)
                .Include(b => b.Storage)
                .Include(b => b.Case)
                .Include(b => b.PowerSupply)
                .Include(b => b.CpuCooler)
                .OrderByDescending(b => b.UpdatedAt)
                .ToListAsync();

            return View(builds);
        }

        // GET: Builds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var build = await _context.Builds
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Memory)
                .Include(b => b.Storage)
                .Include(b => b.Case)
                .Include(b => b.PowerSupply)
                .Include(b => b.CpuCooler)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (build == null)
            {
                return NotFound();
            }

            return View(build);
        }

        // GET: Builds/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Builds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CpuId,GpuId,MotherboardId,MemoryId,StorageId,CaseId,PowerSupplyId,CpuCoolerId")] Build build)
        {
            if (ModelState.IsValid)
            {
                build.CreatedAt = DateTime.UtcNow;
                build.UpdatedAt = DateTime.UtcNow;
                _context.Add(build);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(build);
        }

        // GET: Builds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var build = await _context.Builds.FindAsync(id);
            if (build == null)
            {
                return NotFound();
            }
            return View(build);
        }

        // POST: Builds/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CpuId,GpuId,MotherboardId,MemoryId,StorageId,CaseId,PowerSupplyId,CpuCoolerId,CreatedAt")] Build build)
        {
            if (id != build.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    build.UpdatedAt = DateTime.UtcNow;
                    _context.Update(build);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildExists(build.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(build);
        }

        // GET: Builds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var build = await _context.Builds
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Memory)
                .Include(b => b.Storage)
                .Include(b => b.Case)
                .Include(b => b.PowerSupply)
                .Include(b => b.CpuCooler)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (build == null)
            {
                return NotFound();
            }

            return View(build);
        }

        // POST: Builds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var build = await _context.Builds.FindAsync(id);
            if (build != null)
            {
                _context.Builds.Remove(build);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BuildExists(int id)
        {
            return _context.Builds.Any(e => e.Id == id);
        }
    }
}
