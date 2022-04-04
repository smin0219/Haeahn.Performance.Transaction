using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{ 
    public enum EventType
    {
        Opened,
        Closed,
        Created,
        Added,
        Deleted,
        Modified,
        Sync
    }
    public enum ViewType
    {
        View3D,
        Section,
        Sheet,
        Drafting,
        Normal
    }

    public enum LoginResult
    {
        LoginSsoSucceeded,
        LoginSsoFailed,

        HUserLoginSucceeded,
        HUserLoginFailed,

        UuidLoginSucceeded,
        UuidLoginFailed,

        Succeeded,
        Cancelled,
    }

    public enum CommandType
    {
        Default = 0,
        PreselectElements = 1   // 레빗 객체 선택 동작 수반하는 명령
    }
}
