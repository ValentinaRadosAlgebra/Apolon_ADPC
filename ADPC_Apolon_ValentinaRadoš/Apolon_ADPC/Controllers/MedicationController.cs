using Apolon_ADPC.Models;
using Apolon_ADPC.ORM.Core;
using Microsoft.AspNetCore.Mvc;

namespace Apolon_ADPC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        private readonly UnitOfWork _uow;

        public MedicationController(UnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: api/medication
        [HttpGet]
        public IActionResult GetAll()
        {
            var medications = _uow.Medications.GetAll();
            return Ok(medications);
        }

        // GET: api/medication/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var medication = _uow.Medications.GetById(id);
            if (medication == null) return NotFound();

            return Ok(medication);
        }

        // POST: api/medication
        [HttpPost]
        public IActionResult Create([FromBody] MedicationCU model)
        {
            if (model == null) return BadRequest();

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var exists = _uow.Medications
                    .GetAll($"name='{model.Name}'")
                    .Any();

                if (exists)
                    return BadRequest(new { message = "Medication already exists" });
            }

            var medication = new Medication
            {
                Name = model.Name,
                Description = string.IsNullOrWhiteSpace(model.Description) || model.Description == "string"
                    ? null
                    : model.Description,
                Manufacturer = string.IsNullOrWhiteSpace(model.Manufacturer) || model.Manufacturer == "string"
                    ? null
                    : model.Manufacturer
            };

            _uow.Medications.Insert(medication);
            _uow.Commit();

            return Created("Medication created", medication);
        }

        // PUT: api/medication/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] MedicationCU model)
        {
            if (model == null) return BadRequest();

            var existing = _uow.Medications.GetById(id);
            if (existing == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var exists = _uow.Medications
                    .GetAll($"name='{model.Name}'")
                    .Any();

                if (exists)
                    return BadRequest(new { message = "Medication already exists" });
            }

            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != "string")
                existing.Name = model.Name;

            if (!string.IsNullOrWhiteSpace(model.Description) && model.Description != "string")
                existing.Description = model.Description;

            if (!string.IsNullOrWhiteSpace(model.Manufacturer) && model.Manufacturer != "string")
                existing.Manufacturer = model.Manufacturer;

            _uow.Medications.Update(id, existing);
            _uow.Commit();

            return Ok(existing);
        }

        // DELETE: api/medication/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _uow.Medications.GetById(id);
            if (existing == null) return NotFound();

            var isInPrescription = _uow.Prescriptions.GetAll($"medication_id={id}").Any();
            if (isInPrescription)
                return BadRequest(new { message = "Cannot delete medication. It is referenced in a prescription." });

            _uow.Medications.Delete(id);
            _uow.Commit();

            return NoContent();
        }
    }
}
