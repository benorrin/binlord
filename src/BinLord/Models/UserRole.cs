namespace BinLord.Models;

public enum UserRole
{
    /// <summary>Can view bin schedules and history, nothing else.</summary>
    Viewer,

    /// <summary>Can view and modify bin schedules and history.</summary>
    Editor,

    /// <summary>Can do everything, including managing settings and users.</summary>
    Admin,
}
