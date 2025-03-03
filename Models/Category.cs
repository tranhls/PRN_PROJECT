using System;
using System.Collections.Generic;

namespace PRN222_Assm.Models;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
