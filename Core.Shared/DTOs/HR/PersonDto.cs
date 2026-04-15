using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.HR
{
    public class PersonDto
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string? NationalCode { get; private set; } = null!;
        public string? FullName { get; private set; } = null!;
        public Gender? Gender { get; private set; } = null;
        public int Age { get; private set; } = 0;
    }
}
