using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using aspnetcoreoauth.Models;

namespace aspnetcoreoauth.Controllers
{
    public class EThorTestEntitiesController : Controller
    {
        private readonly IEThorEntityService _service;
        private readonly ILogger<Controller> _logger;
        public EThorTestEntitiesController(ILogger<Controller> logger, IEThorEntityService service)
        {
            _logger = logger;
            _service = service;
        }

        // GET: EThorTestEntities
        public async Task<IActionResult> Index()
        {
            _logger?.LogInformation("List EThorTestEntities action called");
            return View(await _service.GetEThorTestEntityList(e => e != null));
        }

        // GET: EThorTestEntities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eThorTestEntity = await _service.GetEThorTestEntity(id);
            if (eThorTestEntity == null)
            {
                return NotFound();
            }

            return View(eThorTestEntity);
        }

        // GET: EThorTestEntities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EThorTestEntities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,HardProperty")] EThorTestEntity eThorTestEntity)
        {
            if (ModelState.IsValid)
            {
                await _service.AddEThorTestEntity(eThorTestEntity);
                return RedirectToAction(nameof(Index));
            }
            return View(eThorTestEntity);
        }

        // GET: EThorTestEntities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eThorTestEntity = await _service.GetEThorTestEntity(id);
            if (eThorTestEntity == null)
            {
                return NotFound();
            }
            return View(eThorTestEntity);
        }

        // POST: EThorTestEntities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,HardProperty")] EThorTestEntity eThorTestEntity)
        {
            if (id != eThorTestEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _service.UpdateEThorTestEntity(eThorTestEntity);
                return RedirectToAction(nameof(Index));
            }
            return View(eThorTestEntity);
        }

        // GET: EThorTestEntities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eThorTestEntity = await _service.GetEThorTestEntity(id);
            if (eThorTestEntity == null)
            {
                return NotFound();
            }

            return View(eThorTestEntity);
        }

        // POST: EThorTestEntities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteEThorTestEntity(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
