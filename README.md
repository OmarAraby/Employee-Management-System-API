# Employee Management System API Documentation

Welcome to the **Employee Management System API**, a powerful backend service built with **ASP.NET Core** for managing employee attendance, profiles, digital signatures, and authentication. This API supports two user roles: **Admin** and **Employee**, and features role-based access control via JWT authentication.

---

## 📄 Table of Contents

- [Overview](#overview)
- [Folder Structure](#folder-structure)
- [Authentication](#authentication)
- [Base URL](#base-url)
- [Endpoints](#endpoints)
  - [Authentication Endpoints](#authentication-endpoints)
  - [Attendance Endpoints](#attendance-endpoints)
  - [Employee Endpoints](#employee-endpoints)
  - [Signature Endpoints](#signature-endpoints)
- [Data Models](#data-models)
- [Error Handling](#error-handling)
- [Usage Examples](#usage-examples)
- [Contributing](#contributing)
- [License](#license)

    

---

##  Overview

This RESTful API provides functionality for:

- Managing employee records
    
- Tracking and reporting attendance
    
- Handling digital signature uploads
    
- Secure login/logout and password reset
    

All endpoints are secured and require a **valid JWT token** for access unless specified.

---

## Folder Structure

```
EmployeeManagementSys/
├── API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── AttendanceController.cs
│   │   ├── EmployeesController.cs
│   │   ├── SignatureController.cs
│   ├── HandleFiles/
│   │   ├── IFileService.cs
│   │   ├── FileService.cs
│
├── BL/
│   ├── Managers/
│   │   ├── AuthenticationManager.cs
│   │   ├── AttendanceManager.cs
│   │   ├── EmployeeManager.cs
│   │   ├── SignatureManager.cs
│   ├── Interfaces/
│   │   ├── IAuthenticationManager.cs
│   │   ├── IAttendanceManager.cs
│   │   ├── IEmployeeManager.cs
│   │   ├── ISignatureManager.cs
│   ├── Dtos/
│   │   ├── LoginDto.cs
│   │   ├── ResetPasswordDto.cs
│   │   ├── RefreshTokenDto.cs
│   │   ├── CheckInDto.cs
│   │   ├── CreateEmployeeDto.cs
│   │   ├── EmployeeDto.cs
│   │   ├── AttendanceListDto.cs
│   │   ├── SignatureDto.cs
│   ├── Utils/
│
├── DL/
│   ├── Models/
│   │   ├── Attendance.cs
│   │   ├── Employee.cs
│   │   ├── Signature.cs
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── IEmployeeRepository.cs
│   │   │   ├── IAttendanceRepository.cs
│   │   │   ├── ISignatureRepository.cs
│   │   ├── Implementations/
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── EmployeeRepository.cs
│   │   │   ├── AttendanceRepository.cs
│   │   │   ├── SignatureRepository.cs
│   ├── QueryParams/
│   │   ├── AttendanceQueryParams.cs
│   │   ├── EmployeeQueryParams.cs
│   ├── Helpers/
│   │   ├── PagedList.cs
│
├── Data/
│   ├── EmployeeManagementSysDbContext.cs
├── Program.cs
├── appsettings.json
```

---

## Authentication

- **JWT-based Authentication** with roles (`Admin`, `Employee`)
    
- **Authorization header required** for all secure endpoints
    

```http
Authorization: Bearer <your-token>
```

---

## Base URL

```
http://localhost:7185/api/
```

---

## Endpoints

### 🔑 Authentication Endpoints

- `/auth/login` — User login
- `/auth/reset-password` — Reset password (authenticated)
    

### Attendance Endpoints

- `/attendance/check-in` — Employee daily check-in
    
- `/attendance` — Get paginated attendance records (Admin only)
    
- `/attendance/weekly/{employeeId}` — Weekly records for a specific employee
    
- `/attendance/daily` — Today’s check-ins (Admin only)
    
- `/attendance/monthly/{employeeId}` — Monthly records
    

### Employee Endpoints

- `POST /employee` — Add new employee (Admin only)
    
- `PUT /employee/{id}` — Update employee (Admin only)
    
- `DELETE /employee/{id}` — Delete employee (Admin only)
    
- `GET /employee` — Paginated employees list (Admin only)
    
- `GET /employee/profile/{employeeId}` — Employee profile view
    

### Signature Endpoints

- `POST /signatures/upload/{empId}` — Upload signature
    
- `GET /signatures/{empId}` — Get employee’s signature(s)
    

---

## Data Models

Defined in `BL/Dtos`. Examples:

- `LoginDto`, `CreateEmployeeDto`, `EmployeeDto`
    
- `CheckInDto`, `AttendanceListDto`
    
- `SignatureDto`
    

---

## Error Handling

All responses follow this standard structure:

```json
{
  "success": false,
  "errors": [
    {
      "code": "InvalidCredentials",
      "message": "Invalid username or password."
    }
  ]
}
```

---

## Usage Examples

### Login

```bash
curl -X POST "http://localhost:7185/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "aly.ghazal", "password": "password123"}'
```

### Check-In

```bash
curl -X POST "http://localhost:7185/api/attendance/check-in" \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{"employeeId": "<guid>"}'
```

### Upload Signature

```bash
curl -X POST "http://localhost:7185/api/signatures/upload/<employeeId>" \
  -H "Authorization: Bearer <your-token>" \
  -F "file=@signature.png"
```

---



