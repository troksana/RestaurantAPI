using FluentValidation;
using RestaurantAPI.Entities;
using System;
using System.Linq;

namespace RestaurantAPI.Models.Validators
{
    public class RestaurantQueryValidator : AbstractValidator<RestaurantQuery>
    {
        private int[] allowedPagesSizes = new[] {5,10,15 };
        private string[] allowedSortByColumnsNames =
                {nameof(Restaurant.Name), nameof(Restaurant.Category),nameof(Restaurant.Description),};

        public RestaurantQueryValidator()
        {
            RuleFor(r => r.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(r => r.PageSize).Custom((value, context) =>
            {
                if (!allowedPagesSizes.Contains(value))
                {
                    context.AddFailure("PageSize", $"PageSize must be [{string.Join(",", allowedPagesSizes)}]");
                }
            });

            RuleFor(r => r.SortBy).Must(value => string.IsNullOrEmpty(value) || allowedSortByColumnsNames.Contains(value))
                .WithMessage($"SortBy is optional, or must be in [{string.Join(",", allowedSortByColumnsNames)}]");
        }
    }
}
