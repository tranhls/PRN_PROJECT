using System;
using System.Collections.Generic;

namespace PRN222_Assm.Models;

public partial class Score
{
    public int Id { get; set; }

    public double? Pt1 { get; set; }

    public double? Pt2 { get; set; }

    public double? Quiz { get; set; }

    public double? Assignment { get; set; }

    public double? Final { get; set; }

    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
}
