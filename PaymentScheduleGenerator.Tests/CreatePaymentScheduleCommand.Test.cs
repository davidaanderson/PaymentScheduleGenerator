using System;
using FizzWare.NBuilder;
using FluentValidation.TestHelper;
using NUnit.Framework;
using PaymentScheduleGenerator.Domain;

namespace PaymentScheduleGenerator.Tests
{ 
    [TestFixture]
    public class CreatePaymentScheduleCommandTests
    {
        [TestFixture]
        public class Validation
        {
            private CreatePaymentScheduleCommand.Validator _validator;

            [SetUp]
            public void SetUpValidator()
            {
                _validator = new CreatePaymentScheduleCommand.Validator();
            }

            [Test]
            public void ValidModel()
            {
                var command = Builder<CreatePaymentScheduleCommand>.CreateNew()
                    .With(c => c.VehiclePrice, 1500m)
                    .With(c => c.Deposit, 300m)
                    .With(c => c.DeliveryDate, DateTime.Today)
                    .With(c => c.TermInMonths, 12)
                    .Build();

                var result = _validator.TestValidate(command);
                result.ShouldNotHaveAnyValidationErrors();
            }

            [Test]
            public void VehiclePriceInvalidWhenZero()
            {
                var command = Builder<CreatePaymentScheduleCommand>.CreateNew()
                    .With(c => c.VehiclePrice, 0m)
                    .Build();

                var result = _validator.TestValidate(command);
                result.ShouldHaveValidationErrorFor(c => c.VehiclePrice);
            }

            [Test]
            public void DepositInvalidWhenLessThanFifteenPercent()
            {
                var command = Builder<CreatePaymentScheduleCommand>.CreateNew()
                    .With(c => c.VehiclePrice, 1000m)
                    .With(c => c.Deposit, 100m)
                    .Build();

                var result = _validator.TestValidate(command);
                result
                    .ShouldHaveValidationErrorFor(c => c.Deposit)
                    .WithErrorMessage("Deposit must be a minimum of 15% of the vehicle price.");
            }

            [Test]
            public void TermInMonthsInvalidWhenNotDefinedValue()
            {
                var command = Builder<CreatePaymentScheduleCommand>.CreateNew()
                    .With(c => c.TermInMonths, 1)
                    .Build();

                var result = _validator.TestValidate(command);
                result
                    .ShouldHaveValidationErrorFor(c => c.TermInMonths)
                    .WithErrorMessage("The term must be 12, 24 or 36 months.");
            }

            [Test]
            public void DeliveryDateInvalidWhenInPast()
            {
                var command = Builder<CreatePaymentScheduleCommand>.CreateNew()
                    .With(c => c.DeliveryDate, DateTime.Today.AddDays(-1))
                    .Build();

                var result = _validator.TestValidate(command);
                result
                    .ShouldHaveValidationErrorFor(c => c.DeliveryDate);
            }
        }
    }
}
