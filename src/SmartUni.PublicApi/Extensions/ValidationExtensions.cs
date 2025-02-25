using FluentValidation;

namespace SmartUni.PublicApi.Extensions
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string>
            CustomEmailAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.NotEmpty().WithMessage("Email is required")
                .MaximumLength(50).WithMessage("TEmail must not exceed 50 characters")
                .EmailAddress().WithMessage("Email is not valid");
        }

        public static IRuleBuilderOptions<T, string>
            PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.NotEmpty()
                .NotNull().WithMessage("Phone number is required.")
                .MinimumLength(6).WithMessage("Phone number must not be less than 6 characters.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");
        }
    }
}