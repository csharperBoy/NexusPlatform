using Core.Application.Abstractions.People;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.Employee
{
    public record CreateEmployeeCommand(
         string NationalCode,
     string FirstlName,
     string LastName,
     DateTime? BirthDate,
     string? BirthPlace,
     string? FatherName,
     Gender? Gender,

     string EmployeeCode,
     Guid EmploymentTypeId ,
     Guid EmploymentStatusId ,
     DateOnly StartDate ,
     DateOnly? EndDate 

) : IRequest<Result<Guid>>;


    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
    {
        private readonly IOrgChartInternalService _orgChartService;
        private readonly IPersonPublicService _personService;
        private readonly IEmployeeInternalService _employeeService;
        private readonly ILogger<CreateEmployeeCommandHandler> _logger;

        public CreateEmployeeCommandHandler(
            IOrgChartInternalService orgChartService,
            IPersonPublicService personService,
            IEmployeeInternalService employeeService,
            ILogger<CreateEmployeeCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
            _personService = personService;
            _employeeService = employeeService;
        }

        public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating employeeCode: {EmployeeCode}",
                    request.EmployeeCode);

                Guid personId = await _personService.CreatePersonAsync(
                    request.NationalCode,
                    request.FirstlName,
                    request.LastName,
                    request.BirthDate,
                    request.BirthPlace,
                    request.FatherName,
                    request.Gender

                  );

                Guid employeeId = await _employeeService.CreateEmployeeAsync(
                    request.EmployeeCode, personId, request.EmploymentTypeId, request.EmploymentStatusId, request.StartDate, request.EndDate);
                

                await _personService.SaveAsync();
                await _employeeService.SaveAsync();
                _logger.LogInformation(
                    "Employee created successfully: {employeeId} ({EmployeeCode})",
                    employeeId, request.EmployeeCode);

                return Result<Guid>.Ok(employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create Post: {EmployeeCode}",
                     request.EmployeeCode);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
