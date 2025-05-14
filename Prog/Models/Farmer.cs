using System;
using System.Collections.Generic;

namespace Prog.Models;

public partial class Farmer
{
    public int FarmerId { get; set; }

    public int? UserId { get; set; }

    public string FarmerName { get; set; } = null!;

    public string? FarmName { get; set; }

    public string? Location { get; set; }

    public string? ContactNumber { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual User? User { get; set; }
}
