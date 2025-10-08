using Billing.Domain.Errors;
using Billing.Domain.ValueObjects;
using Billing.SharedKernel;

namespace Billing.Domain.Aggregate.Customer;

public class Customer : Entity, IAggregateRoot
{
    public int Id { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public string StripeCustomerId { get; private set; }
    
    public string StripeCustomerName { get; private set; }
    
    public string StripeCustomerEmail { get; private set; }
    
    private Customer() { } // For EF Core
    
    private Customer(UserId userId, string stripeCustomerId, string stripeCustomerName, string stripeCustomerEmail)
    {
        UserId = userId;
        StripeCustomerId = stripeCustomerId;
        StripeCustomerName = stripeCustomerName;
        StripeCustomerEmail = stripeCustomerEmail;
    }
    
    public static Result<Customer> Create(UserId userId, string stripeCustomerId, string stripeCustomerName, string stripeCustomerEmail)
    {
        if (userId.IsEmpty())
            return Result.Failure<Customer>(CustomerErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            return Result.Failure<Customer>(CustomerErrors.StripeCustomerIdRequired);
        
        if (string.IsNullOrWhiteSpace(stripeCustomerName))
            return Result.Failure<Customer>( CustomerErrors.StripeCustomerNameRequired);
        
        if (string.IsNullOrWhiteSpace(stripeCustomerEmail))
            return Result.Failure<Customer>( CustomerErrors.StripeCustomerEmailRequired);
        
        var customer = new Customer(userId, stripeCustomerId, stripeCustomerName, stripeCustomerEmail);
        
        return Result.Success(customer);
    }
}