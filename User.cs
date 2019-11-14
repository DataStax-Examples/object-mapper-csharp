using System;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }

    public override string ToString()
    {
        return $"UserId: {UserId}, Name: {Name}, Age: {Age}";
    }
}