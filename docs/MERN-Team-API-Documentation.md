# MERN Team — API Documentation

## Base URL
```
https://localhost:5001
```

## Authentication
All protected endpoints require JWT token in header:
```
Authorization: Bearer <token>
```

---

## Priority 1 — CRITICAL

### 1. Login
```
POST /api/auth/login
```
**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```
**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "jwt_token_here",
    "user": {
      "id": "guid",
      "email": "string",
      "role": "Patient|Doctor|Admin",
      "firstName": "string",
      "lastName": "string"
    }
  }
}
```

---

### 2. Register
```
POST /api/auth/register
```
**Request:**
```json
{
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string",
  "role": "Patient|Doctor"
}
```

---

### 3. Book Appointment
```
POST /api/appointments
```
**Request:**
```json
{
  "patientId": "guid",
  "doctorId": "guid",
  "scheduledStart": "2026-09-20T10:00:00",
  "scheduledEnd": "2026-09-20T10:30:00",
  "reasonForVisit": "string"
}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "patientId": "guid",
    "patientName": "John Doe",
    "doctorId": "guid",
    "doctorName": "Dr. Smith",
    "status": "Pending",
    "scheduledStart": "2026-09-20T10:00:00",
    "scheduledEnd": "2026-09-20T10:30:00",
    "reasonForVisit": "string"
  }
}
```
**Errors:**
- `400` — Invalid slot / Past date
- `404` — Patient or Doctor not found
- `409` — Slot already taken / Doctor not available

---

### 4. Get All Doctors
```
GET /api/doctors?search=keyword&specialtyId=guid&page=1&pageSize=20
```
**Query Params:**
- `search` — Search by name
- `specialtyId` — Filter by specialty
- `page` — Page number (default: 1)
- `pageSize` — Items per page (default: 20)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userId": "guid",
        "fullName": "Dr. Smith",
        "email": "doctor@test.com",
        "licenseNumber": "ABC-123",
        "yearsOfExperience": 10,
        "consultationFee": 100.00,
        "clinicName": "City Clinic",
        "isActive": true,
        "isVerified": true,
        "specialties": [
          { "id": "guid", "name": "Cardiology" }
        ]
      }
    ],
    "totalCount": 50,
    "page": 1,
    "pageSize": 20
  }
}
```

---

### 5. Get Doctor by ID
```
GET /api/doctors/{id}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "fullName": "Dr. Smith",
    "email": "doctor@test.com",
    "licenseNumber": "ABC-123",
    "yearsOfExperience": 10,
    "consultationFee": 100.00,
    "biography": "string",
    "clinicName": "City Clinic",
    "clinicAddress": "123 Main St",
    "isActive": true,
    "isVerified": true,
    "specialties": [...]
  }
}
```

---

### 6. Get Doctor Availability
```
GET /api/availability/doctor/{doctorId}/slots/{date}
```
**Date format:** `YYYY-MM-DD`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "startTime": "09:00",
      "endTime": "09:30",
      "dayOfWeek": "Monday",
      "slotDurationMinutes": 30,
      "isActive": true,
      "isAvailable": true
    }
  ]
}
```

---

## Priority 2 — High

### 7. Get All Specialties
```
GET /api/specialties
```
**Response:**
```json
{
  "success": true,
  "data": [
    { "id": "guid", "name": "Cardiology", "description": "string" }
  ]
}
```

---

### 8. Patient Dashboard Stats
```
GET /api/patients/dashboard/stats?id={patientId}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "totalAppointments": 10,
    "upcomingAppointments": 3,
    "recentActivity": [
      {
        "appointmentId": "guid",
        "doctorName": "Dr. Smith",
        "status": "Confirmed",
        "scheduledStart": "2026-09-20T10:00:00",
        "type": "Appointment"
      }
    ]
  }
}
```

---

### 9. Doctor Dashboard Stats
```
GET /api/doctors/dashboard/stats?id={doctorId}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "totalAppointments": 50,
    "todayAppointments": 5,
    "upcomingAppointments": 8,
    "completedAppointments": 37,
    "recentAppointments": [
      {
        "id": "guid",
        "patientName": "John Doe",
        "status": "Confirmed",
        "scheduledStart": "2026-09-20T10:00:00",
        "type": "Appointment"
      }
    ]
  }
}
```

---

### 10. Doctor Appointments (by Date)
```
GET /api/doctors/{doctorId}/appointments?date=today
```
**Date:** `today` or `YYYY-MM-DD`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "patientName": "John Doe",
      "scheduledStart": "2026-09-20T10:00:00",
      "status": "Confirmed",
      "durationMinutes": 30
    }
  ]
}
```

---

### 11. Get Appointment by ID
```
GET /api/appointments/{id}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "patientId": "guid",
    "patientName": "John Doe",
    "doctorId": "guid",
    "doctorName": "Dr. Smith",
    "status": "Pending",
    "scheduledStart": "2026-09-20T10:00:00",
    "scheduledEnd": "2026-09-20T10:30:00",
    "reasonForVisit": "Checkup"
  }
}
```

---

### 12. Patient Appointment History
```
GET /api/appointments/patient/{patientId}
```
**Response:** Same as Get Appointment (array)

---

### 13. Update Appointment Status
```
PUT /api/appointments/{id}/status
```
**Request:**
```json
{
  "status": "Confirmed|Completed|Cancelled|NoShow"
}
```

---

### 14. Cancel Appointment
```
PUT /api/appointments/{id}/cancel
```
**Request:**
```json
{
  "cancellationReason": "string (optional)"
}
```

---

### 15. Reschedule Appointment
```
PUT /api/appointments/{id}/reschedule
```
**Request:**
```json
{
  "newStart": "2026-09-21T10:00:00",
  "newEnd": "2026-09-21T10:30:00"
}
```

---

### 16. Get Patient by ID
```
GET /api/patients/{id}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "userId": "guid",
    "fullName": "John Doe",
    "email": "patient@test.com",
    "phoneNumber": "1234567890",
    "medicalRecordNumber": "MRN-001",
    "profile": { ... }
  }
}
```

---

### 17. Update Doctor Profile
```
PATCH /api/doctors/profile
```
**Request:**
```json
{
  "licenseNumber": "ABC-123",
  "yearsOfExperience": 10,
  "biography": "string",
  "consultationFee": 100.00,
  "clinicName": "City Clinic",
  "clinicAddress": "123 Main St"
}
```

---

### 18. Update Doctor Availability
```
PUT /api/availability/{id}
```
**Request:**
```json
{
  "dayOfWeek": "Monday",
  "startTime": "09:00",
  "endTime": "17:00",
  "slotDurationMinutes": 30,
  "isActive": true
}
```

---

## Priority 3 — Medium

### 19. Get Patient Conversations
```
GET /api/ai/conversations/patient/{patientId}
```
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "title": "Chat about symptoms",
      "status": "Active",
      "startedAt": "2026-09-20T10:00:00",
      "messageCount": 5,
      "endedAt": null
    }
  ]
}
```

---

### 20. Create Conversation
```
POST /api/ai/conversations
```
**Request:**
```json
{
  "patientId": "guid",
  "appointmentId": "guid (optional)",
  "title": "New Chat"
}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "New Chat",
    "status": "Active",
    "startedAt": "2026-09-20T10:00:00",
    "messageCount": 0
  }
}
```

---

### 21. Patient Profile CRUD
```
GET    /api/patients/{id}/profile
POST   /api/patients/{id}/profile
PUT    /api/patients/{id}/profile
```
**Request Body:**
```json
{
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "bloodGroup": "O+",
  "heightCm": 175,
  "weightKg": 70,
  "addressLine1": "123 Main St",
  "city": "Lahore",
  "state": "Punjab",
  "postalCode": "54000",
  "country": "Pakistan",
  "emergencyContactName": "Jane Doe",
  "emergencyContactPhone": "03001234567",
  "allergies": "None",
  "chronicConditions": "None",
  "currentMedications": "None"
}
```

---

## Error Response Format
All errors return:
```json
{
  "success": false,
  "message": "Error description",
  "errors": [],
  "timestamp": "2026-09-20T10:00:00Z"
}
```

---

## Test Credentials
| Role | Email | Password |
|---|---|---|
| Admin | admin@healthcare.com | Admin@123 |
| Doctor | doctor@healthcare.com | Doctor@123 |
| Patient | patient@healthcare.com | Patient@123 |

---

## Swagger
```
https://localhost:5001/swagger
```

---

## Notes for MERN Team
1. All protected endpoints require `Authorization: Bearer <token>` header
2. Dates are in UTC format: `2026-09-20T10:00:00`
3. GUIDs are in format: `550e8400-e29b-41d4-a716-446655440000`
4. Status values: `Pending`, `Confirmed`, `Completed`, `Cancelled`, `NoShow`
5. Role values: `Patient`, `Doctor`, `Admin`
