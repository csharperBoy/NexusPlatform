using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Base
{
    public enum Icon
    {
        [Description("fa-solid:circle-user")]
        User,
        [Description("md-account-circle")]
        Account,
        [Description("fa-solid:folder")]
        Folder
    }
}
