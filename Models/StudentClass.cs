using System;
using System.Collections.Generic;

namespace PRN222_Assm.Models;

public partial class StudentClass
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int? StudentId { get; set; }

    public int? ScoreId { get; set; }

    public double? Total { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Score? Score { get; set; }

    public virtual Account? Student { get; set; }
}
