using EmployeeManagementSys.BL.Utils;
using EmployeeManagementSys.DL;


namespace EmployeeManagementSys.BL
{
    public class AttendanceManager : IAttendanceManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CheckInDtoValidator _validator;

        public AttendanceManager(IUnitOfWork unitOfWork, CheckInDtoValidator validator)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<APIResult<CheckInResponseDto>> CheckInAsync(CheckInDto checkInDto, string userRole, Guid callerId)
        {
            if (userRole != "Employee")
            {
                return new APIResult<CheckInResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only employees can check in." } }
                };
            }

            // Ownership: an employee may only check IN THEMSELVES. Trust the
            // caller's identity from the token, never the EmployeeId in the
            // request body (which previously allowed check-in spoofing).
            checkInDto.EmployeeId = callerId;

            var validationResult = await _validator.ValidateAsync(checkInDto);
            if (!validationResult.IsValid)
            {
                return new APIResult<CheckInResponseDto>
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(e => new APIError { Code = "ValidationError", Message = e.ErrorMessage }).ToArray()
                };
            }

            var now = DateTime.UtcNow;
            TimeZoneInfo egyptTimeZone;
            try
            {
                egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to UTC+2 if time zone is not found (manual offset)
                egyptTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                    "Egypt Custom Time",
                    TimeSpan.FromHours(2),
                    "Egypt Standard Time",
                    "Egypt Standard Time"
                );
            }
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(now, egyptTimeZone);
            var checkInTime = localTime.TimeOfDay;

            if (checkInTime < new TimeSpan(7, 30, 0) || checkInTime > new TimeSpan(9, 0, 0))
            {
                return new APIResult<CheckInResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "TimeRestriction", Message = "Check-in is only allowed between 7:30 AM and 9:00 AM." } }
                };
            }

            if (await _unitOfWork.AttendanceRepository.HasCheckedInTodayAsync(checkInDto.EmployeeId))
            {
                return new APIResult<CheckInResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "DuplicateCheckIn", Message = "You have already checked in today." } },
                    Data = new CheckInResponseDto { Success = false, Message = "Duplicate check-in attempt" }
                };
            }

            var attendance = new Attendance
            {
                EmployeeId = checkInDto.EmployeeId,
                CheckInDate = localTime.Date,
                CheckInTime = checkInTime,
                CreatedDate = now
            };

            var addedAttendance = await _unitOfWork.AttendanceRepository.AddAsync(attendance);
            await _unitOfWork.SaveChangesAsync();

            var employee = await _unitOfWork.EmployeeRepository.GetByIDAsync(checkInDto.EmployeeId);
            var attendanceDto = new AttendanceDto
            {
                AttendanceId = addedAttendance.AttendanceId,
                EmployeeId = addedAttendance.EmployeeId,
                EmployeeFullName = employee?.FullName,
                EmployeeEmail= employee?.Email,
                CheckInDate = addedAttendance.CheckInDate,
                CheckInTime = addedAttendance.CheckInTime,
                CheckInDateString = addedAttendance.CheckInDateString,
                CheckInTimeString = addedAttendance.CheckInTimeString,
                IsOnTime = addedAttendance.IsOnTime,
                Status = addedAttendance.Status,
                StatusDisplayName = AttendanceExtensions.GetStatusDisplayName(addedAttendance)
            };

            return new APIResult<CheckInResponseDto>
            {
                Success = true,
                Data = new CheckInResponseDto
                {
                    Success = true,
                    Message = "Check-in successful",
                    Attendance = attendanceDto,
                    CheckInTime = checkInTime,
                    CheckInDate = localTime.Date
                }
            };
        }
        public async Task<APIResult<IEnumerable<AttendanceListDto>>> GetWeeklyAttendanceAsync(Guid employeeId, string userRole, Guid callerId)
        {
            // Admin may view any employee's attendance; an Employee may view
            // only their OWN. Closes the IDOR where any employee could read
            // another's attendance by passing a foreign employeeId.
            if (userRole != "Admin" && userRole != "Employee")
            {
                return new APIResult<IEnumerable<AttendanceListDto>>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Not authorized to view attendance." } }
                };
            }
            if (userRole == "Employee" && callerId != employeeId)
            {
                return new APIResult<IEnumerable<AttendanceListDto>>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Employees can only view their own attendance." } }
                };
            }

            var weekStartDate = DateTime.UtcNow.StartOfWeek(DayOfWeek.Monday);
            var attendances = await _unitOfWork.AttendanceRepository.GetWeeklyAttendanceAsync(employeeId, weekStartDate);

            var tasks = attendances.Select(async a => new AttendanceListDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = (await _unitOfWork.EmployeeRepository.GetByIDAsync(a.EmployeeId))?.FullName,
                CheckInDate = a.CheckInDate,
                CheckInTime = a.CheckInTime,
                IsOnTime = a.IsOnTime,
                Status = a.Status,
                StatusDisplayName = AttendanceExtensions.GetStatusDisplayName(a)
            });
            var dtoList = (await Task.WhenAll(tasks)).ToList();

            return new APIResult<IEnumerable<AttendanceListDto>>
            {
                Success = true,
                Data = dtoList
            };
        }
        public async Task<APIResult<PagedList<AttendanceListDto>>> GetPaginatedAttendanceAsync(AttendanceQueryParams queryParams, string userRole)
        {
            if (userRole != "Admin")
            {
                return new APIResult<PagedList<AttendanceListDto>>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only admins can view paginated attendance." } }
                };
            }

            var (attendances, totalCount) = await _unitOfWork.AttendanceRepository.GetPaginatedAttendanceAsync(queryParams);

            var tasks = attendances.Items.Select(async a => new AttendanceListDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = queryParams.IncludeEmployee
                    ? (await _unitOfWork.EmployeeRepository.GetByIDAsync(a.EmployeeId))?.FullName
                    : null,
                CheckInDate = a.CheckInDate,
                CheckInTime = a.CheckInTime,
                IsOnTime = a.IsOnTime,
                Status = a.Status,
                StatusDisplayName = AttendanceExtensions.GetStatusDisplayName(a)
            });
            var dtoList = (await Task.WhenAll(tasks)).ToList();

            var pagedDtoList = new PagedList<AttendanceListDto>(dtoList, totalCount, queryParams.PageNumber, queryParams.PageSize);

            return new APIResult<PagedList<AttendanceListDto>>
            {
                Success = true,
                Data = pagedDtoList
            };
        }
        public async Task<APIResult<IEnumerable<AttendanceListDto>>> GetDailyAttendanceListAsync(string userRole)
        {
            if (userRole != "Admin")
            {
                return new APIResult<IEnumerable<AttendanceListDto>>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only admins can view daily attendance." } }
                };
            }

            var today = DateTime.UtcNow.Date;
            var queryParams = new AttendanceQueryParams
            {
                FromDate = today,
                ToDate = today,
                PageNumber = 1,
                PageSize = int.MaxValue // Retrieve all records for the day
            };
            var (attendances, _) = await _unitOfWork.AttendanceRepository.GetPaginatedAttendanceAsync(queryParams);

            var tasks = attendances.Items.Select(async a => new AttendanceListDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = (await _unitOfWork.EmployeeRepository.GetByIDAsync(a.EmployeeId))?.FullName,
                CheckInDate = a.CheckInDate,
                CheckInTime = a.CheckInTime,
                IsOnTime = a.IsOnTime,
                Status = a.Status,
                StatusDisplayName = AttendanceExtensions.GetStatusDisplayName(a)
            });
            var dtoList = (await Task.WhenAll(tasks)).ToList();

            return new APIResult<IEnumerable<AttendanceListDto>>
            {
                Success = true,
                Data = dtoList
            };
        }

        //public async Task<APIResult<Dictionary<Guid, double>>> GetWeeklyWorkingHoursAsync(string userRole)
        //{
        //    if (userRole != "Admin")
        //    {
        //        return new APIResult<Dictionary<Guid, double>>
        //        {
        //            Success = false,
        //            Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only admins can view weekly working hours." } }
        //        };
        //    }

        //    var weekStartDate = DateTime.UtcNow.StartOfWeek(DayOfWeek.Monday);
        //    var queryParams = new EmployeeQueryParams { PageNumber = 1, PageSize = int.MaxValue };
        //    var employees = await _unitOfWork.EmployeeRepository.GetPaginatedEmployeesAsync(queryParams);
        //    var result = new Dictionary<Guid, double>();

        //    foreach (var employee in employees)
        //    {
        //        var attendances = await _unitOfWork.AttendanceRepository.GetWeeklyAttendanceAsync(employee.Id, weekStartDate);
        //        var totalHours = attendances.Sum(a => a.WorkingHours ?? 0);
        //        result[employee.Id] = totalHours;
        //    }

        //    return new APIResult<Dictionary<Guid, double>>
        //    {
        //        Success = true,
        //        Data = result
        //    };
        //}

        public async Task<APIResult<IEnumerable<AttendanceListDto>>> GetMonthlyAttendanceAsync(Guid employeeId, int year, int month, string userRole, Guid callerId)
        {
            // Admin may view any employee's attendance; an Employee may view
            // only their OWN.
            if (userRole != "Admin" && userRole != "Employee")
            {
                return new APIResult<IEnumerable<AttendanceListDto>>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Not authorized to view attendance." } }
                };
            }
            if (userRole == "Employee" && callerId != employeeId)
            {
                return new APIResult<IEnumerable<AttendanceListDto>>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Employees can only view their own attendance." } }
                };
            }

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var attendances = await _unitOfWork.AttendanceRepository.GetMonthlyAttendanceAsync(employeeId, year, month);

            var tasks = attendances.Select(async a => new AttendanceListDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = (await _unitOfWork.EmployeeRepository.GetByIDAsync(a.EmployeeId))?.FullName,
                CheckInDate = a.CheckInDate,
                CheckInTime = a.CheckInTime,
                IsOnTime = a.IsOnTime,
                Status = a.Status,
                StatusDisplayName = AttendanceExtensions.GetStatusDisplayName(a)
            });
            var dtoList = (await Task.WhenAll(tasks)).ToList();

            return new APIResult<IEnumerable<AttendanceListDto>>
            {
                Success = true,
                Data = dtoList
            };
        }

        public async Task<APIResult<byte[]>> GetMonthlyAttendanceReportCsvAsync(Guid employeeId, int year, int month, string userRole, Guid callerId)
        {
            // Reuse the monthly query — it carries the same Admin→any / Employee→own
            // authorization, so the report can never expose data the JSON endpoint wouldn't.
            var data = await GetMonthlyAttendanceAsync(employeeId, year, month, userRole, callerId);
            if (!data.Success)
            {
                return new APIResult<byte[]> { Success = false, Errors = data.Errors };
            }

            var lines = new List<string>
            {
                CsvExport.Row("Employee", "Date", "Check-In Time", "On Time", "Status")
            };
            foreach (var a in data.Data!)
            {
                lines.Add(CsvExport.Row(
                    a.EmployeeFullName,
                    a.CheckInDate.ToString("yyyy-MM-dd"),
                    a.CheckInTime.ToString(@"hh\:mm"),
                    a.IsOnTime ? "Yes" : "No",
                    a.StatusDisplayName));
            }

            return new APIResult<byte[]> { Success = true, Data = CsvExport.ToUtf8Bytes(lines) };
        }
    }

   

   
}