using System;
using System.Collections.Generic;

namespace soft_uni_mvc.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public int ManagerId { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Employee Manager { get; set; } = null!;
}
