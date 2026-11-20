# Verification Checklist & Traceability Matrix
## CareFlow — Performance Task 4: Diagnostic Booking Management

This document records the verification of all required acceptance scenarios (**AC-01 through AC-06**) mandated by the ITSD81 Performance Task 4 specification.

---

### Acceptance Scenario Verification Table

| ID | Scenario | Business Rules | Verification Method | Expected Result | Observed Result | Status |
|---|---|---|---|---|---|---|
| **AC-01** | View diagnostic services for a selected date | BR-04, BR-05 | Service Availability View & xUnit test | Remaining capacity is accurately computed from persisted bookings (`DailyCapacity - BookedCount`). | Accurate remaining counts computed per date; updates dynamically. | VERIFIED |
| **AC-02** | Create a booking containing multiple available services | BR-01, BR-02, BR-07 | Create Booking Form & xUnit test | One confirmed booking record is persisted along with all selected diagnostic services. | Booking row and all BookingItem rows committed atomically. | VERIFIED |
| **AC-03** | Request a fully booked service or submit invalid booking information | BR-02, BR-03, BR-05, BR-06, BR-10 | Form validation & xUnit test | Request is rejected with a clear message; zero records or partial allocations created. | Rejected when date is in past, 0 or >3 items, duplicate items, or capacity full. | VERIFIED |
| **AC-04** | Attempt a booking after previously displayed capacity becomes unavailable | BR-05, BR-08 | Controlled stale scenario & xUnit test | Availability is rechecked at the transaction boundary; overbooking is prevented. | Revalidation inside transaction detects exhausted capacity and aborts commit. | VERIFIED |
| **AC-05** | Database operation fails before commit | BR-07, BR-10 | Transaction rollback test & xUnit test | Rollback occurs cleanly; zero partial bookings or capacity leaks persisted. | Exception in transaction triggers rollback; initial row counts unchanged. | VERIFIED |
| **AC-06** | Close and restart application after confirming booking | BR-09 | Persistence readback & xUnit test | Confirmed bookings and associated services remain intact in PostgreSQL. | Application restart queries persistent records without loss or data decay. | VERIFIED |

---

### Automated Acceptance Test Evidence
- **Test Project**: `CareFlow.Tests/AcceptanceTests.cs`
- **Total Tests**: 6
- **Passed**: 6
- **Failed**: 0
- **Command**: `dotnet test`
