using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Helper
{
    public static class CacheKeyHelper
    {
        public static string PhoneBook_BaseChacheKey { get; set; } = "contact:phonebook";
        public static string PhoneBook_GetPhoneBookList { get; set; } = $"{PhoneBook_BaseChacheKey}:full-list";
    }
}
