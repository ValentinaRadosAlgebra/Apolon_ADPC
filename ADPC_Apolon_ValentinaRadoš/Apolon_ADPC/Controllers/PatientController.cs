using Apolon_ADPC.Models;
using Apolon_ADPC.ORM.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace Apolon_ADPC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly UnitOfWork _uow;

        public PatientController(UnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: api/patient
        [HttpGet]
        public IActionResult GetAll()
        {
            var patients = _uow.Patients.GetAll();

            foreach (var patient in patients)
            {
                patient.Checkups = _uow.Checkups.GetAll(p => p.PatientId == patient.Id);
                patient.Prescriptions = _uow.Prescriptions.GetAll(p => p.PatientId == patient.Id);
            }// manual lazy loading, can cause the N+1 query problem

            return Ok(patients);
        }

        // GET: api/patient/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var patient = _uow.Patients.GetById(id);
            if (patient == null) return NotFound();

            // manual eager - loaded explicitly before returning
            _uow.LoadNavigation(patient, nameof(Patient.Checkups));
            _uow.LoadNavigation(patient, nameof(Patient.Prescriptions));

            return Ok(patient);
        }

        // POST: api/patient
        [HttpPost]
        public IActionResult Create([FromBody] PatientCU model)
        {
            if (model == null) return BadRequest();

            if (!string.IsNullOrEmpty(model.Email)) //check duplicates if not ull
            {
                var exists = _uow.Patients.GetAll(p => p.Email == model.Email).Any();
                if (exists)
                    return BadRequest(new { message = "Email already exists" });
            }

            var patient = new Patient
            {
                Name = model.Name,
                Surname = model.Surname,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = model.Address,
                Phone = string.IsNullOrWhiteSpace(model.Phone) || model.Phone == "string" ? null : model.Phone,
                Email = string.IsNullOrWhiteSpace(model.Email) || model.Email == "string" ? null : model.Email,
                EmergencyContact = string.IsNullOrWhiteSpace(model.EmergencyContact) || model.EmergencyContact == "string" ? null : model.EmergencyContact,
                ProfileCreated = DateTime.Now
            };

            _uow.Patients.Insert(patient);
            _uow.Commit();

            return Created("Patient created", patient);
        }

        // PUT: api/patient/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PatientCU model)
        {
            if (model == null) return BadRequest();

            var existing = _uow.Patients.GetById(id);
            if (existing == null) return NotFound();

            if (!string.IsNullOrEmpty(model.Email) && model.Email != existing.Email)
            {
                var exists = _uow.Patients.GetAll(p => p.Email == model.Email).Any();
                if (exists)
                    return BadRequest(new { message = "Email already exists" });
            }

            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != "string")
                existing.Name = model.Name;

            if (!string.IsNullOrWhiteSpace(model.Surname) && model.Surname != "string")
                existing.Surname = model.Surname;


            var birthDate = model.DateOfBirth.Kind == DateTimeKind.Utc
                ? model.DateOfBirth.ToLocalTime().Date //normalize the date so that it matches your local date context and remove time info
                : model.DateOfBirth.Date; 

            if (birthDate != DateTime.Today)
                existing.DateOfBirth = birthDate;

            if (!string.IsNullOrWhiteSpace(model.Gender) && model.Gender != "string")
                existing.Gender = model.Gender;

            if (!string.IsNullOrWhiteSpace(model.Address) && model.Address != "string")
                existing.Address = model.Address;

            if (!string.IsNullOrWhiteSpace(model.Phone) && model.Phone != "string")
                existing.Phone = model.Phone;

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != "string")
                existing.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.EmergencyContact) && model.EmergencyContact != "string")
                existing.EmergencyContact = model.EmergencyContact;

            _uow.Patients.Update(id, existing);
            _uow.Commit();

            return Ok(existing);
        }

        // DELETE: api/patient/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _uow.Patients.GetById(id);
            if (existing == null) return NotFound();

            _uow.Checkups.DeleteWhere($"patient_id={id}");
            _uow.Prescriptions.DeleteWhere($"patient_id={id}");

            _uow.Patients.Delete(id);

            _uow.Commit();

            return NoContent();
        }
    }
}

