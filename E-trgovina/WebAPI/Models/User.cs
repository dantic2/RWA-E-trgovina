using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PwdHash { get; set; } = null!;

    public string PwdSalt { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }

    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; }


    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
