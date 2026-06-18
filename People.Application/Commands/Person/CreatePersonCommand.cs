using Core.Application.Abstractions.People;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using People.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Application.Commands.Person
{
    public record CreatePersonCommand(
        string NationalCode ,
     string FirstlName ,
     string LastName ,
     DateTime? BirthDate ,
     string? BirthPlace ,
     string? FatherName ,
     Gender? Gender 
  ) : IRequest<Result<Guid>>;

    public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Result<Guid>>
    {
        private readonly IPersonPublicService _personService;
        private readonly ILogger<CreatePersonCommandHandler> _logger;

        public CreatePersonCommandHandler(
            IPersonPublicService personService,
            ILogger<CreatePersonCommandHandler> logger)
        {
            _personService = personService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating Person: {NationalCode}",
                    request.NationalCode);

                Guid personId = await _personService.CreatePersonAsync(
                      request.NationalCode,
                      request.FirstlName,
                      request.LastName,
                      request.BirthDate ,
                      request.BirthPlace ,
                      request.FatherName ,
                      request.Gender
                      
                    );
                

                await _personService.SaveAsync();
                _logger.LogInformation(
                    "Person created successfully: {NationalCode}",
                     request.NationalCode);

                return Result<Guid>.Ok(personId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create person: {NationalCode}",
                     request.NationalCode);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
