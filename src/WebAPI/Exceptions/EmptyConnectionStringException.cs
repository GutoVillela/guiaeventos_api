namespace WebAPI.Exceptions;

public class EmptyConnectionStringException : ApplicationException
{
    public EmptyConnectionStringException() : base("The connection string can not be empty.")
    {
    }
}