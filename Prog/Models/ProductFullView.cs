using System;
using System.Collections.Generic;

namespace Prog.Models;

public partial class ProductFullView
{
    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public DateOnly? ProductionDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? FarmerId { get; set; }

    public string? FarmerName { get; set; }

    public string? FarmName { get; set; }

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }
}
