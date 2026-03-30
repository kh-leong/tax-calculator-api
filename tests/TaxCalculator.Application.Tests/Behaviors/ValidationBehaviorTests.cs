using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using TaxCalculator.Application.Behaviors;
using TaxCalculator.Application.Common;

namespace TaxCalculator.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_PassesThrough()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(
            []);

        var called = false;
        Task<Result<string>> next(CancellationToken _ = default)
        {
            called = true;
            return Task.FromResult(Result<string>.Success("ok"));
        }

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidatorPasses_PassesThrough()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var called = false;
        Task<Result<string>> next(CancellationToken _ = default)
        {
            called = true;
            return Task.FromResult(Result<string>.Success("ok"));
        }

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidatorFails_ReturnsValidationError()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Field", "Bad value")]));

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        var called = false;
        Task<Result<string>> next(CancellationToken _ = default)
        {
            called = true;
            return Task.FromResult(Result<string>.Success("ok"));
        }

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("ValidationFailed");
        result.Error.Message.Should().Contain("Bad value");
        called.Should().BeFalse();
    }

    public record TestRequest : IRequest<Result<string>>;
}
