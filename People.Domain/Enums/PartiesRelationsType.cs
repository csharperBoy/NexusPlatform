using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Enums
{
    public enum PartiesRelationsType : byte
    {
        Owner = 0, // مالک
        LegalRepresentative = 1, // نماینده قانونی
        Parent = 2, // والدین
        Child = 3, // فرزند
        Sibling = 4, // خواهر/برادر
        Spouse = 5, // همسر
        Colleague = 6, // همکار
        Friend = 7, // دوست
        Other = 8, // سایر


    }
}
