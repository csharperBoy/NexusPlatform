using Core.Shared.Results;
using MediatR;
using Sample.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Commands
{
    public record SampleApiCommand(string property1, string property2)
      : IRequest<Result<SampleApiResponse>>;

}
