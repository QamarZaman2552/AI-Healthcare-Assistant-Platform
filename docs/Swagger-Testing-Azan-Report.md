# Swagger Testing Report — Azan (Admin + Doctors)

**Tester:** Azan  
**Date:** 27 September 2026  
**Branch:** `feature/doctor-patient-module` (at `425f82f`)  
**Scope:** Admin (3) + Doctors (15) — see note on the endpoint count below  
**Swagger URL:** `https://localhost:52878/swagger`  
**Screenshots:** `Swagger Screenshot/Azan/` (one PNG per test case; that folder is gitignored, so the PNGs are shared separately)

## Summary

| Total cases | ✅ Pass | ❌ Fail | ⚠️ Observation |
|---|---|---|---|
| 85 | 77 | 6 | 2 |

> **Endpoint count:** the division doc lists Doctors as 16, but `DoctorsController` has **15** endpoints (the ones listed in the doc add up to 15 as well). Total for Azan is **18**, not 19.

Every case was run through the real Swagger UI (Authorize → Try it out → Execute). Each screenshot has a banner at the top showing the case, the token used, and the expected vs. actual status.

## Checklist matrix

Legend: ✅ as expected · ❌ bug · ⚠️ works as coded but questionable · `N/A` not applicable (public endpoint / no input / no ID)

| # | Method | Endpoint | 200/201 | 401 no token | 403 wrong role | 400 bad input | 404 random ID | 409 duplicate |
|---|---|---|---|---|---|---|---|---|
| A01 | GET | `/api/Admin/dashboard` | ✅ 200 (admin) | ✅ 401 | ✅ 403 (patient)<br>✅ 403 (doctor) | N/A | N/A |  |
| A02 | GET | `/api/Admin/users` | ✅ 200 (admin) | ✅ 401 | ✅ 403 (patient)<br>✅ 403 (doctor) | N/A | N/A |  |
| A03 | GET | `/api/Admin/system-status` | ✅ 200 (admin) | ✅ 401 | ✅ 403 (patient)<br>✅ 403 (doctor) | N/A | N/A |  |
| D01 | GET | `/api/Doctors` | ✅ 200 (none)<br>✅ 200 (patient) | N/A (public) | N/A (public) | N/A | N/A |  |
| D02 | GET | `/api/Doctors/by-status` | ✅ 200 (admin) | ✅ 401 | ⚠️ 200 (patient) | N/A | N/A |  |
| D03 | GET | `/api/Doctors/{id}` | ✅ 200 (none) | N/A (public) | N/A (public) | N/A | ✅ 404 |  |
| D04 | GET | `/api/Doctors/user/{userId}` | ✅ 200 (doctor) | ✅ 401 | ⚠️ 200 (patient) | N/A | ✅ 404 |  |
| D05 | GET | `/api/Doctors/specialty/{specialtyId}` | ✅ 200 (none) | N/A (public) | N/A (public) | N/A | ✅ 404 |  |
| D06 | POST | `/api/Doctors` | ✅ 201 (admin) | ✅ 401 | ✅ 403 (patient) | ✅ 400<br>✅ 400 | ✅ 404 | ✅ 409 |
| D07 | PUT | `/api/Doctors/{id}` | ✅ 200 (doctor) | ✅ 401 | ✅ 403 (patient)<br>❌ 200 (doctor) | ✅ 400 | ✅ 404 |  |
| D08 | PATCH | `/api/Doctors/{id}/status` | ✅ 200 (admin)<br>✅ 200 (admin) | ✅ 401 | ✅ 403 (patient)<br>❌ 200 (doctor) | ✅ 400<br>❌ 200 | ✅ 404 |  |
| D09 | PATCH | `/api/Doctors/{id}/verification` | ✅ 200 (admin) | ✅ 401 | ✅ 403 (doctor)<br>✅ 403 (patient) | ✅ 400 | ✅ 404 |  |
| D10 | GET | `/api/Doctors/{id}/specialties` | ✅ 200 (none) | N/A (public) | N/A (public) | N/A | ✅ 404 |  |
| D11 | POST | `/api/Doctors/{id}/specialties` | ✅ 200 (doctor) | ✅ 401 | ✅ 403 (patient)<br>❌ 200 (doctor) | ✅ 400 | ✅ 404<br>✅ 404 | ✅ 409 |
| D12 | DELETE | `/api/Doctors/{id}/specialties/{specialtyId}` | ✅ 204 (doctor) | ✅ 401 | ✅ 403 (patient)<br>❌ 204 (doctor) | N/A | ✅ 404 |  |
| D13 | GET | `/api/Doctors/dashboard/stats` | ✅ 200 (doctor)<br>✅ 200 (admin) | ✅ 401 | ✅ 403 (patient)<br>✅ 403 (doctor) | ❌ 403 | ✅ 404 |  |
| D14 | GET | `/api/Doctors/{id}/appointments` | ✅ 200 (doctor) | ✅ 401 | ✅ 403 (patient)<br>✅ 403 (doctor) | ✅ 400 | ✅ 404 |  |
| D15 | PATCH | `/api/Doctors/profile` | ✅ 200 (doctor) | ✅ 401 | ✅ 403 (patient) | ✅ 400 | ✅ 404 |  |

## Findings

### ❌ BUG-1 (High, security): a Doctor can modify ANY other doctor's record (IDOR)

`PUT /api/Doctors/{id}`, `PATCH /api/Doctors/{id}/status`, `POST /api/Doctors/{id}/specialties` and `DELETE /api/Doctors/{id}/specialties/{specialtyId}` only check the role (`Doctor,Admin`). They never check that the doctor owns `{id}`. Logged in as `doctor@healthcare.com`, I could edit, deactivate, and change the specialties of the QA doctor account:

| Case | Expected | Actual | Screenshot |
|---|---|---|---|
| PUT `/api/Doctors/{id}` | 403 | 200 | `D07-04_PUT_Doctors-id_other-doctor.png` |
| PATCH `/api/Doctors/{id}/status` | 403 | 200 | `D08-04_PATCH_Doctors-id-status_other-doctor.png` |
| POST `/api/Doctors/{id}/specialties` | 403 | 200 | `D11-08_POST_Doctors-id-specialties_other-doctor.png` |
| DELETE `/api/Doctors/{id}/specialties/{specialtyId}` | 403 | 204 | `D12-05_DELETE_Doctors-id-specialties-specialtyId_other-doctor.png` |

The same controller already has the right guard: `CanAccessDoctorAsync` is used by `dashboard/stats` and `{id}/appointments`, which correctly return 403 for another doctor. The fix is to call the same check in these four actions. `PUT` also wipes fields that are left out (biography and clinic were reset to `null` in the screenshot).

### ❌ BUG-2 (Medium): `PATCH /api/Doctors/{id}/status` with an empty body `{}` deactivates the doctor

`UpdateDoctorStatusRequest.IsActive` is a plain `bool` with no `[Required]`, so a missing field becomes `false` and the doctor is silently deactivated (200 instead of 400). `UpdateDoctorVerificationRequest.IsVerified` has the same problem. Fix: make them `bool?` with `[Required]`. Screenshot: `D08-06_PATCH_Doctors-id-status_missing-field.png` (I re-activated the QA doctor right after, in case `D08-07`.)

### ❌ BUG-3 (Low): `GET /api/Doctors/dashboard/stats` — `id` is not validated

The action takes `Guid id` without `[FromQuery]`/`[Required]`, so calling it without `id` returns **403** for a Doctor (and 404 for an Admin) instead of 400. For a Doctor it would make more sense to resolve the doctor from the token, as `PATCH /profile` already does. Screenshot: `D13-06_GET_Doctors-dashboard-stats_missing-id.png`.

### ⚠️ OBS-1: `GET /api/Doctors/by-status` and `GET /api/Doctors/user/{userId}` have no role restriction

Any logged-in user, including a Patient, gets 200. So a patient can list inactive or unverified doctors. If `by-status` is meant as an admin tool, add `[Authorize(Roles = "Admin")]`. Screenshots: `D02-03_GET_Doctors-by-status_patient-role.png`, `D04-03_GET_Doctors-user-userId_patient-role.png`.

### ⚠️ OBS-2: `GET /api/Admin/system-status` always reports `aiAvailable: true`

`AdminService.GetSystemStatusAsync` hard-codes `AIAvailable = true`. I ran the API with a dummy Gemini key and it still said `true`. Also, `activeConnections` is actually `Users.Count()` (total users), not active connections.

### ⚠️ OBS-3: 401/403 responses have an empty body

Swagger documents `ApiErrorResponse` for 401/403, but the JWT middleware returns `content-length: 0`. The frontend won't get a `message` for these. This could be fixed with `JwtBearerEvents.OnChallenge` / `OnForbidden`.

### ⚠️ OBS-4: Registration does not create a Doctor row

The comment on `CreateDoctorRequest` says "a bare Doctor row is created automatically at registration", but after registering with `role: Doctor`, `GET /api/Doctors/user/{userId}` returns 404 until `POST /api/Doctors` is called. Either the comment or the register flow is out of date. Related (Qamar's area): `POST /api/Auth/register` accepts `role: "Admin"` from anyone, so public sign-up can create admins.

## Test environment notes

- The API would not start with the repo config as-is: `JwtSettings:Key` and `AISettings:ApiKey` are required, and the connection string points to `Server=localhost`, but this machine runs `localhost\SQLEXPRESS`. For this run I passed a random JWT key, a dummy AI key and the SQLEXPRESS connection string as environment variables. **No config files were changed.**
- Test data created in the local DB: one Doctor-role user `azan.qa.doctor…@healthcare.test` with a doctor profile (license `AZN-QA-…`) used for the cross-doctor tests, plus one leftover QA user and doctor profile from a first dry run. The seed doctor's Psychiatry specialty was removed and re-added (net no change).
- Swagger UI checks parameter types before sending, so an invalid `int`/`bool` in a query string (e.g. `page=abc`) can't be sent from Swagger. For GET endpoints without a body, the 400 column is N/A.

## All cases

| # | Case | Auth | Expected | Actual | Result | Screenshot |
|---|---|---|---|---|---|---|
| A01 | GET `/api/Admin/dashboard` — happy | admin | 200 | 200 | ✅ | `A01-01_GET_Admin-dashboard_happy.png` |
| A01 | GET `/api/Admin/dashboard` — no-token | none | 401 | 401 | ✅ | `A01-02_GET_Admin-dashboard_no-token.png` |
| A01 | GET `/api/Admin/dashboard` — wrong-role-patient | patient | 403 | 403 | ✅ | `A01-03_GET_Admin-dashboard_wrong-role-patient.png` |
| A01 | GET `/api/Admin/dashboard` — wrong-role-doctor | doctor | 403 | 403 | ✅ | `A01-04_GET_Admin-dashboard_wrong-role-doctor.png` |
| A02 | GET `/api/Admin/users` — happy | admin | 200 | 200 | ✅ | `A02-01_GET_Admin-users_happy.png` |
| A02 | GET `/api/Admin/users` — no-token | none | 401 | 401 | ✅ | `A02-02_GET_Admin-users_no-token.png` |
| A02 | GET `/api/Admin/users` — wrong-role-patient | patient | 403 | 403 | ✅ | `A02-03_GET_Admin-users_wrong-role-patient.png` |
| A02 | GET `/api/Admin/users` — wrong-role-doctor | doctor | 403 | 403 | ✅ | `A02-04_GET_Admin-users_wrong-role-doctor.png` |
| A03 | GET `/api/Admin/system-status` — happy | admin | 200 | 200 | ✅ | `A03-01_GET_Admin-system-status_happy.png` |
| A03 | GET `/api/Admin/system-status` — no-token | none | 401 | 401 | ✅ | `A03-02_GET_Admin-system-status_no-token.png` |
| A03 | GET `/api/Admin/system-status` — wrong-role-patient | patient | 403 | 403 | ✅ | `A03-03_GET_Admin-system-status_wrong-role-patient.png` |
| A03 | GET `/api/Admin/system-status` — wrong-role-doctor | doctor | 403 | 403 | ✅ | `A03-04_GET_Admin-system-status_wrong-role-doctor.png` |
| D01 | GET `/api/Doctors` — happy-anonymous | none | 200 | 200 | ✅ | `D01-01_GET_Doctors_happy-anonymous.png` |
| D01 | GET `/api/Doctors` — happy-search | patient | 200 | 200 | ✅ | `D01-02_GET_Doctors_happy-search.png` |
| D02 | GET `/api/Doctors/by-status` — happy | admin | 200 | 200 | ✅ | `D02-01_GET_Doctors-by-status_happy.png` |
| D02 | GET `/api/Doctors/by-status` — no-token | none | 401 | 401 | ✅ | `D02-02_GET_Doctors-by-status_no-token.png` |
| D02 | GET `/api/Doctors/by-status` — patient-role | patient | 403 | 200 | ⚠️ | `D02-03_GET_Doctors-by-status_patient-role.png` |
| D03 | GET `/api/Doctors/{id}` — happy-anonymous | none | 200 | 200 | ✅ | `D03-01_GET_Doctors-id_happy-anonymous.png` |
| D03 | GET `/api/Doctors/{id}` — not-found | none | 404 | 404 | ✅ | `D03-02_GET_Doctors-id_not-found.png` |
| D04 | GET `/api/Doctors/user/{userId}` — happy | doctor | 200 | 200 | ✅ | `D04-01_GET_Doctors-user-userId_happy.png` |
| D04 | GET `/api/Doctors/user/{userId}` — no-token | none | 401 | 401 | ✅ | `D04-02_GET_Doctors-user-userId_no-token.png` |
| D04 | GET `/api/Doctors/user/{userId}` — patient-role | patient | 403 | 200 | ⚠️ | `D04-03_GET_Doctors-user-userId_patient-role.png` |
| D04 | GET `/api/Doctors/user/{userId}` — not-found | admin | 404 | 404 | ✅ | `D04-04_GET_Doctors-user-userId_not-found.png` |
| D05 | GET `/api/Doctors/specialty/{specialtyId}` — happy-anonymous | none | 200 | 200 | ✅ | `D05-01_GET_Doctors-specialty-specialtyId_happy-anonymous.png` |
| D05 | GET `/api/Doctors/specialty/{specialtyId}` — not-found | none | 404 | 404 | ✅ | `D05-02_GET_Doctors-specialty-specialtyId_not-found.png` |
| D06 | POST `/api/Doctors` — no-token | none | 401 | 401 | ✅ | `D06-01_POST_Doctors_no-token.png` |
| D06 | POST `/api/Doctors` — wrong-role-patient | patient | 403 | 403 | ✅ | `D06-02_POST_Doctors_wrong-role-patient.png` |
| D06 | POST `/api/Doctors` — missing-license | admin | 400 | 400 | ✅ | `D06-03_POST_Doctors_missing-license.png` |
| D06 | POST `/api/Doctors` — missing-userId | admin | 400 | 400 | ✅ | `D06-04_POST_Doctors_missing-userId.png` |
| D06 | POST `/api/Doctors` — not-found-user | admin | 404 | 404 | ✅ | `D06-05_POST_Doctors_not-found-user.png` |
| D06 | POST `/api/Doctors` — happy | admin | 201 | 201 | ✅ | `D06-06_POST_Doctors_happy.png` |
| D06 | POST `/api/Doctors` — duplicate-conflict | admin | 409 | 409 | ✅ | `D06-07_POST_Doctors_duplicate-conflict.png` |
| D07 | PUT `/api/Doctors/{id}` — happy | doctor | 200 | 200 | ✅ | `D07-01_PUT_Doctors-id_happy.png` |
| D07 | PUT `/api/Doctors/{id}` — no-token | none | 401 | 401 | ✅ | `D07-02_PUT_Doctors-id_no-token.png` |
| D07 | PUT `/api/Doctors/{id}` — wrong-role-patient | patient | 403 | 403 | ✅ | `D07-03_PUT_Doctors-id_wrong-role-patient.png` |
| D07 | PUT `/api/Doctors/{id}` — other-doctor | doctor | 403 | 200 | ❌ | `D07-04_PUT_Doctors-id_other-doctor.png` |
| D07 | PUT `/api/Doctors/{id}` — missing-license | doctor | 400 | 400 | ✅ | `D07-05_PUT_Doctors-id_missing-license.png` |
| D07 | PUT `/api/Doctors/{id}` — not-found | admin | 404 | 404 | ✅ | `D07-06_PUT_Doctors-id_not-found.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — happy | admin | 200 | 200 | ✅ | `D08-01_PATCH_Doctors-id-status_happy.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — no-token | none | 401 | 401 | ✅ | `D08-02_PATCH_Doctors-id-status_no-token.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — wrong-role-patient | patient | 403 | 403 | ✅ | `D08-03_PATCH_Doctors-id-status_wrong-role-patient.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — other-doctor | doctor | 403 | 200 | ❌ | `D08-04_PATCH_Doctors-id-status_other-doctor.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — invalid-body | admin | 400 | 400 | ✅ | `D08-05_PATCH_Doctors-id-status_invalid-body.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — missing-field | admin | 400 | 200 | ❌ | `D08-06_PATCH_Doctors-id-status_missing-field.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — restore-active | admin | 200 | 200 | ✅ | `D08-07_PATCH_Doctors-id-status_restore-active.png` |
| D08 | PATCH `/api/Doctors/{id}/status` — not-found | admin | 404 | 404 | ✅ | `D08-08_PATCH_Doctors-id-status_not-found.png` |
| D09 | PATCH `/api/Doctors/{id}/verification` — happy | admin | 200 | 200 | ✅ | `D09-01_PATCH_Doctors-id-verification_happy.png` |
| D09 | PATCH `/api/Doctors/{id}/verification` — no-token | none | 401 | 401 | ✅ | `D09-02_PATCH_Doctors-id-verification_no-token.png` |
| D09 | PATCH `/api/Doctors/{id}/verification` — wrong-role-doctor | doctor | 403 | 403 | ✅ | `D09-03_PATCH_Doctors-id-verification_wrong-role-doctor.png` |
| D09 | PATCH `/api/Doctors/{id}/verification` — wrong-role-patient | patient | 403 | 403 | ✅ | `D09-04_PATCH_Doctors-id-verification_wrong-role-patient.png` |
| D09 | PATCH `/api/Doctors/{id}/verification` — invalid-body | admin | 400 | 400 | ✅ | `D09-05_PATCH_Doctors-id-verification_invalid-body.png` |
| D09 | PATCH `/api/Doctors/{id}/verification` — not-found | admin | 404 | 404 | ✅ | `D09-06_PATCH_Doctors-id-verification_not-found.png` |
| D10 | GET `/api/Doctors/{id}/specialties` — happy-anonymous | none | 200 | 200 | ✅ | `D10-01_GET_Doctors-id-specialties_happy-anonymous.png` |
| D10 | GET `/api/Doctors/{id}/specialties` — not-found | none | 404 | 404 | ✅ | `D10-02_GET_Doctors-id-specialties_not-found.png` |
| D12 | DELETE `/api/Doctors/{id}/specialties/{specialtyId}` — no-token | none | 401 | 401 | ✅ | `D12-01_DELETE_Doctors-id-specialties-specialtyId_no-token.png` |
| D12 | DELETE `/api/Doctors/{id}/specialties/{specialtyId}` — wrong-role-patient | patient | 403 | 403 | ✅ | `D12-02_DELETE_Doctors-id-specialties-specialtyId_wrong-role-patient.png` |
| D12 | DELETE `/api/Doctors/{id}/specialties/{specialtyId}` — happy | doctor | 204 | 204 | ✅ | `D12-03_DELETE_Doctors-id-specialties-specialtyId_happy.png` |
| D12 | DELETE `/api/Doctors/{id}/specialties/{specialtyId}` — not-found | doctor | 404 | 404 | ✅ | `D12-04_DELETE_Doctors-id-specialties-specialtyId_not-found.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — no-token | none | 401 | 401 | ✅ | `D11-01_POST_Doctors-id-specialties_no-token.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — wrong-role-patient | patient | 403 | 403 | ✅ | `D11-02_POST_Doctors-id-specialties_wrong-role-patient.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — missing-specialtyId | doctor | 400 | 400 | ✅ | `D11-03_POST_Doctors-id-specialties_missing-specialtyId.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — happy | doctor | 200 | 200 | ✅ | `D11-04_POST_Doctors-id-specialties_happy.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — duplicate-conflict | doctor | 409 | 409 | ✅ | `D11-05_POST_Doctors-id-specialties_duplicate-conflict.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — not-found-doctor | admin | 404 | 404 | ✅ | `D11-06_POST_Doctors-id-specialties_not-found-doctor.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — not-found-specialty | admin | 404 | 404 | ✅ | `D11-07_POST_Doctors-id-specialties_not-found-specialty.png` |
| D11 | POST `/api/Doctors/{id}/specialties` — other-doctor | doctor | 403 | 200 | ❌ | `D11-08_POST_Doctors-id-specialties_other-doctor.png` |
| D12 | DELETE `/api/Doctors/{id}/specialties/{specialtyId}` — other-doctor | doctor | 403 | 204 | ❌ | `D12-05_DELETE_Doctors-id-specialties-specialtyId_other-doctor.png` |
| D13 | GET `/api/Doctors/dashboard/stats` — happy-doctor | doctor | 200 | 200 | ✅ | `D13-01_GET_Doctors-dashboard-stats_happy-doctor.png` |
| D13 | GET `/api/Doctors/dashboard/stats` — happy-admin | admin | 200 | 200 | ✅ | `D13-02_GET_Doctors-dashboard-stats_happy-admin.png` |
| D13 | GET `/api/Doctors/dashboard/stats` — no-token | none | 401 | 401 | ✅ | `D13-03_GET_Doctors-dashboard-stats_no-token.png` |
| D13 | GET `/api/Doctors/dashboard/stats` — wrong-role-patient | patient | 403 | 403 | ✅ | `D13-04_GET_Doctors-dashboard-stats_wrong-role-patient.png` |
| D13 | GET `/api/Doctors/dashboard/stats` — other-doctor | doctor | 403 | 403 | ✅ | `D13-05_GET_Doctors-dashboard-stats_other-doctor.png` |
| D13 | GET `/api/Doctors/dashboard/stats` — missing-id | doctor | 400 | 403 | ❌ | `D13-06_GET_Doctors-dashboard-stats_missing-id.png` |
| D13 | GET `/api/Doctors/dashboard/stats` — not-found | admin | 404 | 404 | ✅ | `D13-07_GET_Doctors-dashboard-stats_not-found.png` |
| D14 | GET `/api/Doctors/{id}/appointments` — happy | doctor | 200 | 200 | ✅ | `D14-01_GET_Doctors-id-appointments_happy.png` |
| D14 | GET `/api/Doctors/{id}/appointments` — no-token | none | 401 | 401 | ✅ | `D14-02_GET_Doctors-id-appointments_no-token.png` |
| D14 | GET `/api/Doctors/{id}/appointments` — wrong-role-patient | patient | 403 | 403 | ✅ | `D14-03_GET_Doctors-id-appointments_wrong-role-patient.png` |
| D14 | GET `/api/Doctors/{id}/appointments` — other-doctor | doctor | 403 | 403 | ✅ | `D14-04_GET_Doctors-id-appointments_other-doctor.png` |
| D14 | GET `/api/Doctors/{id}/appointments` — invalid-date | doctor | 400 | 400 | ✅ | `D14-05_GET_Doctors-id-appointments_invalid-date.png` |
| D14 | GET `/api/Doctors/{id}/appointments` — not-found | admin | 404 | 404 | ✅ | `D14-06_GET_Doctors-id-appointments_not-found.png` |
| D15 | PATCH `/api/Doctors/profile` — happy | doctor | 200 | 200 | ✅ | `D15-01_PATCH_Doctors-profile_happy.png` |
| D15 | PATCH `/api/Doctors/profile` — no-token | none | 401 | 401 | ✅ | `D15-02_PATCH_Doctors-profile_no-token.png` |
| D15 | PATCH `/api/Doctors/profile` — wrong-role-patient | patient | 403 | 403 | ✅ | `D15-03_PATCH_Doctors-profile_wrong-role-patient.png` |
| D15 | PATCH `/api/Doctors/profile` — missing-license | doctor | 400 | 400 | ✅ | `D15-04_PATCH_Doctors-profile_missing-license.png` |
| D15 | PATCH `/api/Doctors/profile` — not-found-admin-no-profile | admin | 404 | 404 | ✅ | `D15-05_PATCH_Doctors-profile_not-found-admin-no-profile.png` |
