using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using aspnetcoreoauth.Models;
using Microsoft.AspNetCore.Authorization;

namespace aspnetcoreoauth.Controllers
{
    [Authorize]
    public class SampleTestEntitiesController : Controller
    {
        private readonly ISampleEntityService _service;
        private readonly ILogger<Controller> _logger;
        public SampleTestEntitiesController(ILogger<Controller> logger, ISampleEntityService service)
        {
            _logger = logger;
            _service = service;
        }

        // GET: SampleTestEntities
        public async Task<IActionResult> Index()
        {
            _logger?.LogInformation("List SampleTestEntities action called");
            return View(await _service.GetSampleTestEntityList(e => e != null));
        }

        // GET: SampleTestEntities/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SampleTestEntity = await _service.GetSampleTestEntity(id);
            if (SampleTestEntity == null)
            {
                return NotFound();
            }

            return View(SampleTestEntity);
        }

        // GET: SampleTestEntities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SampleTestEntities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,HardProperty")] SampleTestEntity SampleTestEntity)
        {
            if (ModelState.IsValid)
            {
                await _service.AddSampleTestEntity(SampleTestEntity);
                return RedirectToAction(nameof(Index));
            }
            return View(SampleTestEntity);
        }

        // GET: SampleTestEntities/Edit/1
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SampleTestEntity = await _service.GetSampleTestEntity(id);
            if (SampleTestEntity == null)
            {
                return NotFound();
            }
            return View(SampleTestEntity);
        }

        // POST: SampleTestEntities/Edit/1
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,HardProperty")] SampleTestEntity SampleTestEntity)
        {
            if (id != SampleTestEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _service.UpdateSampleTestEntity(SampleTestEntity);
                return RedirectToAction(nameof(Index));
            }
            return View(SampleTestEntity);
        }

        // GET: SampleTestEntities/Delete/1
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var SampleTestEntity = await _service.GetSampleTestEntity(id);
            if (SampleTestEntity == null)
            {
                return NotFound();
            }

            return View(SampleTestEntity);
        }

        // POST: SampleTestEntities/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteSampleTestEntity(id);
            return RedirectToAction(nameof(Index));
        }

    }
}