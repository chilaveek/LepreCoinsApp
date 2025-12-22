using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class Family
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
