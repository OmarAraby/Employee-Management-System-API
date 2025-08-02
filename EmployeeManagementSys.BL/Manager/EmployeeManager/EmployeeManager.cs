using EmployeeManagementSys.DL;
using Microsoft.AspNetCore.Identity;


namespace EmployeeManagementSys.BL
{
    public class EmployeeManager : IEmployeeManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CreateEmployeeValidator _validator;
        private readonly UserManager<Employee> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;



        public EmployeeManager(IUnitOfWork unitOfWork, CreateEmployeeValidator validator, UserManager<Employee> userManager, RoleManager<IdentityRole<Guid>> roleManager = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));

        }


        public async Task<APIResult<EmployeeDto>> AddEmployeeAsync(CreateEmployeeDto createDto, string userRole)
        {
            if (userRole != "Admin")
            {
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only admins can add employees." } }
                };
            }

            var validationResult = await _validator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(e => new APIError { Code = "ValidationError", Message = e.ErrorMessage }).ToArray()
                };
            }

            // Check if email already exists
            var existingEmployee = await _userManager.FindByEmailAsync(createDto.FirstName.ToLower() + "." + createDto.LastName.ToLower() + "@company.com");
            if (existingEmployee != null)
            {
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "DuplicateEmail", Message = "An employee with this email already exists." } }
                };
            }

            var employee = new Employee
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                NationalId = createDto.NationalId,
                Age = createDto.Age,
                UserName = createDto.FirstName.ToLower() + "." + createDto.LastName.ToLower(),
                Email = createDto.FirstName.ToLower() + "." + createDto.LastName.ToLower() + "@company.com",
                CreatedDate = DateTime.UtcNow,
                RequiresPasswordReset = true //  new employees must reset password
            };

            // Use UserManager instead of repository directly
            string defaultPassword = GenerateDefaultPassword(employee);
            var result = await _userManager.CreateAsync(employee, defaultPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new APIError { Code = "CreateFailed", Message = e.Description }).ToArray();
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Assign role using UserManager
            await _userManager.AddToRoleAsync(employee, "Employee");



            return new APIResult<EmployeeDto>
            {
                Success = true,
                Data = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber,
                    NationalId = employee.NationalId,
                    Age = employee.Age,
                    Status = employee.Status,
                    StatusDisplayName = employee.StatusDisplayName,
                    CreatedDate = employee.CreatedDate,
                    Email = employee.Email, 
                    //UserName = employee.UserName 
                }
            };
        }

        public async Task<APIResult<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto updateDto, string userRole)
        {
            if (userRole != "Admin")
            {
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only admins can update employees." } }
                };
            }

            var employee = await _unitOfWork.EmployeeRepository.GetByIDAsync(updateDto.Id);
            if (employee == null)
            {
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "NotFound", Message = "Employee not found." } }
                };
            }

            employee.FirstName = updateDto.FirstName;
            employee.LastName = updateDto.LastName;
            employee.NationalId = updateDto.NationalId;
            employee.Age = updateDto.Age;
            employee.UpdatedDate = DateTime.UtcNow;

            var updatedEmployee = await _unitOfWork.EmployeeRepository.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return new APIResult<EmployeeDto>
            {
                Success = true,
                Data = new EmployeeDto
                {
                    Id = updatedEmployee.Id,
                    FirstName = updatedEmployee.FirstName,
                    LastName = updatedEmployee.LastName,
                    FullName = updatedEmployee.FullName,
                    PhoneNumber = updatedEmployee.PhoneNumber,
                    NationalId = updatedEmployee.NationalId,
                    Age = updatedEmployee.Age,
                    Status = updatedEmployee.Status,
                    StatusDisplayName = updatedEmployee.StatusDisplayName,
                    UpdatedDate = updatedEmployee.UpdatedDate
                }
            };
        }

        public async Task<APIResult<bool>> DeleteEmployeeAsync(Guid id, string userRole)
        {
            if (userRole != "Admin")
            {
                return new APIResult<bool>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only admins can delete employees." } }
                };
            }

            var success = await _unitOfWork.EmployeeRepository.DeleteAsync(id);
            if (!success)
            {
                return new APIResult<bool>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "DeleteFailed", Message = "Failed to delete employee." } }
                };
            }

            await _unitOfWork.SaveChangesAsync();
            return new APIResult<bool>
            {
                Success = true,
                Data = true
            };
        }

        public async Task<APIResult<PagedList<EmployeeListDto>>> GetPaginatedEmployeesAsync(EmployeeQueryParams queryParams)
        {
            var pagedEmployees = await _unitOfWork.EmployeeRepository.GetPaginatedEmployeesAsync(queryParams);
            var dtoItems = pagedEmployees.Items.Select(e => new EmployeeListDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email ?? "",
                PhoneNumber = e.PhoneNumber ?? "",
                NationalId = e.NationalId ?? "",
                Age = e.Age,
                Status = e.Status,
                StatusDisplayName = e.StatusDisplayName ?? e.Status.ToString(),
                CreatedDate = e.CreatedDate,

                IsActive = e.IsActive
            }).ToList();

            var pagedDtoList = new PagedList<EmployeeListDto>(dtoItems, pagedEmployees.TotalCount, pagedEmployees.PageNumber, pagedEmployees.PageSize);
            return new APIResult<PagedList<EmployeeListDto>>
            {
                Success = true,
                Data = pagedDtoList
            };
        }
        public async Task<APIResult<EmployeeStatsDto>> GetEmployeeStatisticsAsync()
        {
            var totalEmployees = await _unitOfWork.EmployeeRepository.GetTotalEmployeesAsync();
            var activeEmployees = await _unitOfWork.EmployeeRepository.GetActiveEmployeesCountAsync();
            var inactiveEmployees = totalEmployees - activeEmployees;
            var suspendedEmployees = 0; // Placeholder
            var newEmployeesThisMonth = await Task.FromResult(0); // Placeholder

            return new APIResult<EmployeeStatsDto>
            {
                Success = true,
                Data = new EmployeeStatsDto
                {
                    TotalEmployees = totalEmployees,
                    ActiveEmployees = activeEmployees,
                    InactiveEmployees = inactiveEmployees,
                    SuspendedEmployees = suspendedEmployees,
                    NewEmployeesThisMonth = newEmployeesThisMonth
                }
            };
        }

        public async Task<APIResult<EmployeeDto>> GetEmployeeProfileAsync(Guid employeeId, string userRole)
        {
            if (userRole != "Employee")
            {
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only employees can view their profile." } }
                };
            }

            var employee = await _unitOfWork.EmployeeRepository.GetByIDAsync(employeeId);
            if (employee == null)
            {
                return new APIResult<EmployeeDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "NotFound", Message = "Employee not found." } }
                };
            }

            return new APIResult<EmployeeDto>
            {
                Success = true,
                Data = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber,
                    NationalId = employee.NationalId,
                    Email = employee.Email ?? string.Empty, 
                    Age = employee.Age,
                    Status = employee.Status,
                    StatusDisplayName = employee.StatusDisplayName,
                    Signature = employee.Signature?.FilePath, 
                    CreatedDate = employee.CreatedDate,
                    UpdatedDate = employee.UpdatedDate
                }
            };
        }


        //  generating default passwords
        private string GenerateDefaultPassword(Employee employee)
        {
          
            // Example: FirstName + "@" + last 4 digits of NationalId
            string nationalIdSuffix = employee.NationalId.Length >= 4
                ? employee.NationalId.Substring(employee.NationalId.Length - 4)
                : employee.NationalId;

            return $"{employee.FirstName}@{nationalIdSuffix}";

        }
    }

}