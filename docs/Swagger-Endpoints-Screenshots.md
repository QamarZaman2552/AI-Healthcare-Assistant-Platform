# Swagger API Endpoints — Screenshots

AI Healthcare Assistant & Smart Appointment Platform — Swagger UI endpoint screenshots.

**Swagger URL:** `https://localhost:7xxx/swagger` (local)  
**Date:** 22 September 2026  
**Auth:** JWT Bearer (lock icon = secured endpoint)

---

## 1. Admin

![Admin Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225435.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Admin/dashboard` | Gets the administrator dashboard with statistics. |
| GET | `/api/Admin/users` | Gets all users in the system. |
| GET | `/api/Admin/system-status` | Checks database and AI service health. |

---

## 2. AI

![AI Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225639.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Ai/chat` | Sends a message to the AI healthcare assistant with automatic patient context. |
| POST | `/api/Ai/symptom-check` | Performs an AI-powered symptom assessment. |
| GET | `/api/Ai/conversations/patient/{id}` | Gets all conversations for the authenticated patient. |
| GET | `/api/Ai/conversations/{id}` | Gets a specific conversation with all messages. |
| POST | `/api/Ai/conversations` | Creates a new conversation for a patient. |
| GET | `/api/Ai/health` | Checks whether the AI provider is available. |

---

## 3. Auth

![Auth Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225707.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/register` | Registers a new user account. |
| POST | `/api/Auth/login` | Authenticates a user and returns a JWT token. |

---

## 4. Doctors (Part 1)

![Doctors Endpoints Part 1](../Swagger%20Screenshot/Screenshot%202026-09-22%20225721.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Doctors` | Gets a paginated list of doctors. |
| POST | `/api/Doctors` | Creates a doctor. |
| GET | `/api/Doctors/by-status` | Gets doctors filtered by active and verification status. |
| GET | `/api/Doctors/{id}` | Gets a doctor by ID. |
| PUT | `/api/Doctors/{id}` | Updates doctor information. |
| GET | `/api/Doctors/user/{userId}` | Gets a doctor by user ID. |
| GET | `/api/Doctors/specialty/{specialtyId}` | Gets all doctors belonging to a specialty. |
| PATCH | `/api/Doctors/{id}/status` | Updates doctor active status. |
| PATCH | `/api/Doctors/{id}/verification` | Updates doctor verification status. |
| GET | `/api/Doctors/{id}/specialties` | Gets all specialties assigned to a doctor. |
| POST | `/api/Doctors/{id}/specialties` | Assigns a specialty to a doctor. |
| GET | `/api/Doctors/dashboard/stats` | Gets doctor dashboard stats. |

---

## 5. Doctors (Part 2)

![Doctors Endpoints Part 2](../Swagger%20Screenshot/Screenshot%202026-09-22%20225733.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Doctors/{id}/specialties` | Assigns a specialty to a doctor. |
| GET | `/api/Doctors/dashboard/stats` | Gets doctor dashboard stats. |
| GET | `/api/Doctors/{id}/appointments` | Gets doctor appointments by doctor ID for a specific date. |
| PATCH | `/api/Doctors/profile` | Updates doctor profile information. |
| DELETE | `/api/Doctors/{id}/specialties/{specialtyId}` | Removes a specialty from a doctor. |

---

## 6. Patients

![Patients Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225828.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Patients/register` | Registers the current user as a patient. |
| GET | `/api/Patients` | Gets all patients. |
| GET | `/api/Patients/{id}` | Gets a patient by ID. |
| GET | `/api/Patients/user/{userId}` | Gets a patient by user ID. |
| GET | `/api/Patients/{id}/profile` | Gets a patient's profile. |
| POST | `/api/Patients/{id}/profile` | Creates a patient's profile. |
| PUT | `/api/Patients/{id}/profile` | Updates a patient's profile. |
| GET | `/api/Patients/dashboard/stats` | Gets patient dashboard stats. |
| GET | `/api/Patients/{id}/history` | Gets patient appointment history. |

---

## 7. Appointments

![Appointments Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225849.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Appointments/{id}` | Gets an appointment by its unique identifier. |
| GET | `/api/Appointments/patient/{patientId}` | Gets all appointments for a patient. |
| GET | `/api/Appointments/doctor/{doctorId}` | Gets all appointments for a doctor. |
| POST | `/api/Appointments` | Creates a new appointment. |
| PUT | `/api/Appointments/{id}/cancel` | Cancels an existing appointment. |
| PUT | `/api/Appointments/{id}/reschedule` | Reschedules an existing appointment. |
| PUT | `/api/Appointments/{id}/status` | Updates the status of an appointment. |
| GET | `/api/Appointments/doctor/{doctorId}/dashboard` | Gets doctor appointments filtered by status or date range. |

---

## 8. Specialities

![Specialities Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225926.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Specialities` | Gets all medical specialties. |
| POST | `/api/Specialities` | Creates a new medical specialty. |
| GET | `/api/Specialities/{id}` | Gets a specialty by ID. |
| PUT | `/api/Specialities/{id}` | Updates an existing medical specialty. |
| DELETE | `/api/Specialities/{id}` | Deletes a medical specialty. |

---

## 9. Availability

![Availability Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225936.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Availability/search` | Searches available doctors for a specialty on a specific date. |
| GET | `/api/Availability/doctor/{doctorId}` | Gets all availability records for a doctor. |
| GET | `/api/Availability/doctor/{doctorId}/day/{dayOfWeek}` | Gets doctor availability for a specific day. |
| GET | `/api/Availability/doctor/{doctorId}/slots/{date}` | Gets available time slots for a doctor on a specific date. |
| GET | `/api/Availability/{id}` | Gets an availability record by ID. |
| PUT | `/api/Availability/{id}` | Updates an existing availability record. |
| DELETE | `/api/Availability/{id}` | Deletes an availability record. |
| POST | `/api/Availability` | Creates a new doctor availability record. |

---

## 10. Patient Intakes

![Patient Intakes Endpoints](../Swagger%20Screenshot/Screenshot%202026-09-22%20225949.png)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/PatientIntakes/{id}` | Gets a patient intake by ID. |
| GET | `/api/PatientIntakes/patient/{patientId}` | Gets all intake records for a patient. |
| GET | `/api/PatientIntakes/appointment/{appointmentId}` | Gets the intake associated with an appointment. |
| POST | `/api/PatientIntakes` | Creates a new patient intake. |
| PUT | `/api/PatientIntakes/{id}/status` | Updates the status of a patient intake. |

---

## Endpoint Summary

| Section | Endpoints |
|---------|-----------|
| Admin | 3 |
| AI | 6 |
| Auth | 2 |
| Doctors | 16 |
| Patients | 9 |
| Appointments | 8 |
| Specialities | 5 |
| Availability | 8 |
| Patient Intakes | 5 |
| **Total** | **62** |
