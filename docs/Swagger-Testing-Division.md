# Swagger Testing Division — Qamar, Azan, Ahsan

**Total Endpoints:** 62 | **Swagger URL:** `https://localhost:7xxx/swagger`  
**Date:** 23 September 2026

---

## Qamar — 21 Endpoints

Focus: Auth flows, Patient management, Intake, Specialities

| Section | Endpoints |
|---------|-----------|
| **Auth** (2) | POST `/api/Auth/register`, POST `/api/Auth/login` |
| **Patients** (9) | POST `/api/Patients/register`, GET `/api/Patients`, GET `/api/Patients/{id}`, GET `/api/Patients/user/{userId}`, GET/POST/PUT `/api/Patients/{id}/profile`, GET `/api/Patients/dashboard/stats`, GET `/api/Patients/{id}/history` |
| **Patient Intakes** (5) | GET `/api/PatientIntakes/{id}`, GET `/api/PatientIntakes/patient/{patientId}`, GET `/api/PatientIntakes/appointment/{appointmentId}`, POST `/api/PatientIntakes`, PUT `/api/PatientIntakes/{id}/status` |
| **Specialities** (5) | GET/POST `/api/Specialities`, GET/PUT/DELETE `/api/Specialities/{id}` |

---

## Azan — 19 Endpoints

Focus: Admin dashboard, Doctor management, specialties assignment, verification

| Section | Endpoints |
|---------|-----------|
| **Admin** (3) | GET `/api/Admin/dashboard`, GET `/api/Admin/users`, GET `/api/Admin/system-status` |
| **Doctors** (16) | GET/POST `/api/Doctors`, GET `/api/Doctors/by-status`, GET/PUT `/api/Doctors/{id}`, GET `/api/Doctors/user/{userId}`, GET `/api/Doctors/specialty/{specialtyId}`, PATCH `/api/Doctors/{id}/status`, PATCH `/api/Doctors/{id}/verification`, GET/POST `/api/Doctors/{id}/specialties`, GET `/api/Doctors/dashboard/stats`, GET `/api/Doctors/{id}/appointments`, PATCH `/api/Doctors/profile`, DELETE `/api/Doctors/{id}/specialties/{specialtyId}` |

---

## Ahsan — 22 Endpoints

Focus: AI chat/symptom check, Appointment lifecycle, Availability & slots

| Section | Endpoints |
|---------|-----------|
| **AI** (6) | POST `/api/Ai/chat`, POST `/api/Ai/symptom-check`, GET `/api/Ai/conversations/patient/{id}`, GET `/api/Ai/conversations/{id}`, POST `/api/Ai/conversations`, GET `/api/Ai/health` |
| **Appointments** (8) | GET `/api/Appointments/{id}`, GET `/api/Appointments/patient/{patientId}`, GET `/api/Appointments/doctor/{doctorId}`, POST `/api/Appointments`, PUT `/api/Appointments/{id}/cancel`, PUT `/api/Appointments/{id}/reschedule`, PUT `/api/Appointments/{id}/status`, GET `/api/Appointments/doctor/{doctorId}/dashboard` |
| **Availability** (8) | GET `/api/Availability/search`, GET `/api/Availability/doctor/{doctorId}`, GET `/api/Availability/doctor/{doctorId}/day/{dayOfWeek}`, GET `/api/Availability/doctor/{doctorId}/slots/{date}`, GET `/api/Availability/{id}`, POST/PUT/DELETE `/api/Availability...` |

---

## Summary

| Member | Sections | Endpoints |
|--------|----------|-----------|
| **Qamar** | Auth, Patients, Patient Intakes, Specialities | **21** |
| **Azan** | Admin, Doctors | **19** |
| **Ahsan** | AI, Appointments, Availability | **22** |
| **Total** | 9 controllers | **62** |

## Testing Checklist (per endpoint)

- [ ] Happy path (valid request → 200/201)
- [ ] Unauthorized (no token → 401)
- [ ] Wrong role (e.g., Patient calling Admin → 403)
- [ ] Invalid input (missing field → 400)
- [ ] Not found (random GUID → 404)
- [ ] Screenshots saved to `Swagger Screenshot/` folder

## Seed Logins

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@healthcare.com | Admin@123 |
| Doctor | doctor@healthcare.com | Doctor@123 |
| Patient | patient@healthcare.com | Patient@123 |
