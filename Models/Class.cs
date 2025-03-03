using System;
using System.Collections.Generic;

namespace PRN222_Assm.Models;

public partial class Class
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public int? Subject { get; set; }

    public int? Tearcher { get; set; }

    public int? SemesterId { get; set; }

    public virtual Semmester? Semester { get; set; }

    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();

    public virtual Subject? SubjectNavigation { get; set; }

    public virtual Account? TearcherNavigation { get; set; }
}
