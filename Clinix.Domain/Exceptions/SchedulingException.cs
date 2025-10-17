using System;

namespace Clinix.Domain.Exceptions;

public class SchedulingException : Exception
    {
    public SchedulingException(string message) : base(message) { }
    }
