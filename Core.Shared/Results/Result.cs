using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Results
{
    public record Result(bool Succeeded, string? Error = null)
    {
        public static Result Ok() => new(true);
        public static Result Fail(string error) => new(false, error);
    }

    public record Result<T>(bool Succeeded, T? Data = default, string? Error = null) : Result(Succeeded, Error)
    {
        public static Result<T> Ok(T data) => new(true, data);
        public static Result<T> Fail(string error) => new(false, default, error);
    }
}
