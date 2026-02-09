using Apolon_ADPC.Models;
using Apolon_ADPC.ORM.Core;
using Microsoft.AspNetCore.Mvc;

namespace Apolon_ADPC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly UnitOfWork _uow;

        public PrescriptionController(UnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: api/prescription
        [HttpGet]
        public IActionResult GetAll()
        {
            var prescriptions = _uow.Prescriptions.GetAll();
            return Ok(prescriptions);
        }

        // GET: api/prescription/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var prescription = _uow.Prescriptions.GetById(id);
            if (prescription == null) return NotFound();

            return Ok(prescription);
        }

        // POST: api/prescription
        [HttpPost]
        public IActionResult Create([FromBody] PrescriptionCU model)
        {
            if (model == null) return BadRequest();

            var patientExists = _uow.Patients.GetById(model.PatientId) != null;
            if (!patientExists)
                return BadRequest(new { message = "Patient does not exist" });

            var medicationExists = _uow.Medications.GetById(model.MedicationId) != null;
            if (!medicationExists)
                return BadRequest(new { message = "Medication does not exist" });

            var prescription = new Prescription
            {
                PatientId = model.PatientId,
                MedicationId = model.MedicationId,
                Dosage = model.Dosage,
                StartDate = DateTime.Now,
                EndDate = model.EndDate.HasValue && model.EndDate.Value.Date != DateTime.Today
                    ? model.EndDate.Value.Date
                    : null
            };

            _uow.Prescriptions.Insert(prescription);
            _uow.Commit();

            return Created("Prescription created", prescription);
        }

        // PUT: api/prescription/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PrescriptionCU model)
        {
            if (model == null) return BadRequest();

            var existing = _uow.Prescriptions.GetById(id);
            if (existing == null) return NotFound();

            if (model.PatientId > 0 && model.PatientId != existing.PatientId) //if provided
            {
                var patientExists = _uow.Patients.GetById(model.PatientId) != null; //check existance
                if (!patientExists)
                    return BadRequest(new { message = "Patient does not exist" });

                existing.PatientId = model.PatientId;
            }

            if (model.MedicationId > 0 && model.MedicationId != existing.MedicationId)
            {
                var medicationExists = _uow.Medications.GetById(model.MedicationId) != null;
                if (!medicationExists)
                    return BadRequest(new { message = "Medication does not exist" });

                existing.MedicationId = model.MedicationId;
            }

            if (!string.IsNullOrWhiteSpace(model.Dosage) && model.Dosage != "string")
                existing.Dosage = model.Dosage;

            if (model.EndDate.HasValue)
            {
                var endDate = model.EndDate.Value.Kind == DateTimeKind.Utc
                    ? model.EndDate.Value.ToLocalTime().Date
                    : model.EndDate.Value.Date;

                if (endDate != DateTime.Today)
                    existing.EndDate = endDate;
            }

            _uow.Prescriptions.Update(id, existing);
            _uow.Commit();

            return Ok(existing);
        }

        // DELETE: api/prescription/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _uow.Prescriptions.GetById(id);
            if (existing == null) return NotFound();

            _uow.Prescriptions.Delete(id);
            _uow.Commit();

            return NoContent();
        }
    }
}
