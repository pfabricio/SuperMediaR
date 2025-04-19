using System;

namespace SuperMediaR.Core.Authorization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute
{
    public string Role { get; }

    public AuthorizeAttribute(string role)
    {
        Role = role;
    }
}