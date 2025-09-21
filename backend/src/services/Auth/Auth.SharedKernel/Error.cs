namespace Auth.SharedKernel;

public readonly record struct Error(string Code, string Description, ErrorType Type)
{
    public static Error None => new(Code: string.Empty, Description: string.Empty, ErrorType.Failure);

    public static Error NullValue => new
    (
        Code: "General.NullValue",
        Description: "A null value was provided.",
        ErrorType.Failure
    );
    
    public static Error Failure(string code, string description) 
        => new(code, description, ErrorType.Failure);
    
    public static Error Validation(string code, string description) 
        => new(code, description, ErrorType.Validation);
    
    public static Error Problem(string code, string description)
        => new(code, description, ErrorType.Problem);
    
    public static Error NotFound(string code, string description) 
        => new(code, description, ErrorType.NotFound);
    
    public static Error Conflict(string code, string description) 
        => new(code, description, ErrorType.Conflict);
}