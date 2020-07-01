using System;

namespace DataAccess.Models.Enum
{
    public enum AccessLevel : byte
    {
        None = 0,
        Create = 1,
        Read = 2,
        Update = 4,
        Delete = 8,
        Owner = 15
    }
}
