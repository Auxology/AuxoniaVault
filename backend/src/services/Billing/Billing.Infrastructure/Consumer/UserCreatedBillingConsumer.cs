using Billing.Application.Abstractions.Database;
using Billing.Domain.ValueObjects;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Stripe;
using Customer = Billing.Domain.Aggregate.Customer.Customer;

namespace Billing.Infrastructure.Consumer;

public sealed class UserCreatedBillingConsumer(
    IStripeClient stripeClient,
    IBillingDbContext dbContext,
    ILogger<UserCreatedBillingConsumer> logger) : IConsumer<UserCreatedContract>
{
    public async Task Consume(ConsumeContext<UserCreatedContract> context)
    {
        logger.LogInformation("Received UserCreatedContract for User ID: {UserId}, Email: {Email}", context.Message.UserId, context.Message.Email);

        var message = context.Message;

        UserId typedUserId = UserId.UnsafeFromGuid(message.UserId);

        var existing = await dbContext.Customers.SingleOrDefaultAsync(c => c.UserId == typedUserId || c.StripeCustomerEmail == message.Email);

        if (existing is not null)
            return;

        var createOptions = new CustomerCreateOptions
        {
            Email = message.Email,
            Name = message.Name,
            Metadata = new Dictionary<string, string>
            {
                { "UserId", message.UserId.ToString() }
            }
        };

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = message.UserId.ToString()
        };

        var customer = await new CustomerService(stripeClient).CreateAsync(createOptions, requestOptions);

        var customerResult = Customer.Create
        (
            typedUserId,
            customer.Id,
            customer.Name,
            customer.Email
        );

        if (customerResult.IsFailure)
        {
            logger.LogError("Failed to create Customer aggregate for User ID: {UserId}. Errors: {Errors}", message.UserId, string.Join(", ", customerResult.Error));
            return;
        }

        await dbContext.Customers.AddAsync(customerResult.Value);

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created Stripe customer with ID: {CustomerId} for User ID: {UserId}", customer.Id, message.UserId);
    }
}
