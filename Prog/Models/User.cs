using System;
using System.Collections.Generic;

namespace Prog.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string? Email { get; set; }

    public string UserRole { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<Farmer> Farmers { get; set; } = new List<Farmer>();
}
