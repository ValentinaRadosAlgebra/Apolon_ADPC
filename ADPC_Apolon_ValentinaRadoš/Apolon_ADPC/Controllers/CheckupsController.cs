using Apolon_ADPC.Models;
using Apolon_ADPC.ORM.Core;
using Microsoft.AspNetCore.Mvc;

namespace Apolon_ADPC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckupsController : ControllerBase
    {
        private readonly UnitOfWork _uow;

        public CheckupsController(UnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: api/checkups
        [HttpGet]
        public IActionResult GetAll()
        {
            var checkups = _uow.Checkups.GetAll();
            return Ok(checkups);
        }

        // GET: api/checkups/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var checkup = _uow.Checkups.GetById(id);
            if (checkup == null) return NotFound();

            return Ok(checkup);
        }

        // POST: api/checkups
        [HttpPost]
        public IActionResult Create([FromBody] CheckupCU model)
        {
            if (model == null) return BadRequest();

            var patientExists = _uow.Patients.GetById(model.PatientId) != null;
            if (!patientExists)
                return BadRequest(new { message = "Patient does not exist" });

            if (!Enum.TryParse<CheckupType>(model.Type, ignoreCase: true, out var type))
            {
                return BadRequest(new { message = "Invalid checkup type" });
            }

            var checkup = new Checkups
            {
                PatientId = model.PatientId,
                Type = type, //no Previous and if not type givenBadRequest
                CheckupDate = DateTime.Now,
                Notes = string.IsNullOrWhiteSpace(model.Notes) || model.Notes == "string"
                    ? null
                    : model.Notes,
                Diagnosis = string.IsNullOrWhiteSpace(model.Diagnosis) || model.Diagnosis == "string"
                    ? null
                    : model.Diagnosis
            };

            _uow.Checkups.Insert(checkup);
            _uow.Commit();

            return Created("Checkup created", checkup);
        }

        // PUT: api/checkups/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] CheckupCU model)
        {
            if (model == null) return BadRequest();

            var existing = _uow.Checkups.GetById(id);
            if (existing == null) return NotFound();

            if (model.PatientId > 0 && model.PatientId != existing.PatientId)
            {
                var patientExists = _uow.Patients.GetById(model.PatientId) != null;
                if (!patientExists)
                    return BadRequest(new { message = "Patient does not exist" });

                existing.PatientId = model.PatientId;
            }

            if (!string.IsNullOrWhiteSpace(model.Type) && model.Type != "string") { 
                if (!Enum.TryParse<CheckupType>(model.Type, ignoreCase: true, out var type))
                {
                    return BadRequest(new { message = "Invalid checkup type" });
                }

                if (type != existing.Type)
                {
                    existing.Type = type;
                } 
            }

            if (!string.IsNullOrWhiteSpace(model.Notes) && model.Notes != "string")
                existing.Notes = model.Notes;

            if (!string.IsNullOrWhiteSpace(model.Diagnosis) && model.Diagnosis != "string")
                existing.Diagnosis = model.Diagnosis;

            _uow.Checkups.Update(id, existing);
            _uow.Commit();

            return Ok(existing);
        }

        // DELETE: api/checkups/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _uow.Checkups.GetById(id);
            if (existing == null) return NotFound();

            _uow.Checkups.Delete(id);
            _uow.Commit();

            return NoContent();
        }
    }
}
