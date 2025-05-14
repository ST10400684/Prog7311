using System;
using System.Collections.Generic;

namespace Prog.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int FarmerId { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly ProductionDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual Farmer Farmer { get; set; } = null!;
}
