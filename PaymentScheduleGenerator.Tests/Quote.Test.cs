using System;
using System.Linq;
using System.Runtime.InteropServices;
using FizzWare.NBuilder;
using FluentAssertions;
using NodaMoney;
using NodaTime;
using NUnit.Framework;
using PaymentScheduleGenerator.Domain;

namespace PaymentScheduleGenerator.Tests
{
    [TestFixture]
    public class QuoteTests
    {
        [TestFixture]
        public class CalculateTests
        {
            [TestCase(12)]
            [TestCase(24)]
            [TestCase(36)]
            public void CorrectNumberOfMonthsAreUsedInPaymentScheduleForGivenTerm(int expected)
            {
                // arrange
                var quote = Builder<Quote>.CreateNew()
                    .With(bo => bo.TermInMonths, expected)
                    .Build();
                
                // act
                var actual = quote.CalculatePaymentSchedule();

                // assert
                actual.Payments.Should().HaveCount(expected);
            }

            [Test]
            public void EveryPaymentDateIsAMonday()
            {
                // arrange
                var quote = Builder<Quote>.CreateNew()
                    .With(bo => bo.TermInMonths, 12)
                    .Build();
                
                // act
                var actual = quote.CalculatePaymentSchedule();

                // assert
                actual.Payments.All(p => p.DueDate.DayOfWeek == IsoDayOfWeek.Monday).Should().BeTrue();
            }
            
            [TestCase("2021-03-10", "2021-04-05", Description = "Standard use case")]
            [TestCase("2021-02-28", "2021-03-01", Description = "Next day is first Monday of next month")]
            [TestCase("2021-03-29", "2021-04-05", Description = "Delivery date is already a Monday")]
            public void FirstPaymentDateIsFirstMondayOfNextMonth(string deliveryDateAsString, string expectedFirstPaymentDateAsString)
            {
                // arrange
                var deliveryDate = LocalDate.FromDateTime(DateTime.Parse(deliveryDateAsString));
                var expectedFirstPaymentDate = LocalDate.FromDateTime(DateTime.Parse(expectedFirstPaymentDateAsString));

                var quote = Builder<Quote>.CreateNew()
                    .With(bo => bo.TermInMonths, 12)
                    .With(bo => bo.DeliveryDate, deliveryDate)
                    .Build();

                // act
                var actual = quote.CalculatePaymentSchedule();

                // assert
                actual.Payments.First().DueDate.Should().Be(expectedFirstPaymentDate);
            }

            [Test]
            public void FirstPaymentContainsArrangementFee()
            {
                // arrange
                var quote = Builder<Quote>.CreateNew()
                    .With(bo => bo.TermInMonths, 12)
                    .With(bo => bo.DeliveryDate, new LocalDate(2021, 1, 1))
                    .With(bo => bo.VehiclePrice, Money.PoundSterling(1200))
                    .With(bo => bo.Deposit, Money.PoundSterling(0))
                    .With(bo => bo.ArrangementFee, Money.PoundSterling(100))
                    .With(bo => bo.CompletionFee, Money.PoundSterling(0))
                    .Build();

                // act
                var actual = quote.CalculatePaymentSchedule();

                // assert
                actual.Payments.First().Amount.Should().Be(new Money(200m));
            }
            
            [Test]
            public void FinalPaymentContainsCompletionFee()
            {
                // arrange
                var quote = Builder<Quote>.CreateNew()
                    .With(bo => bo.TermInMonths, 12)
                    .With(bo => bo.DeliveryDate, new LocalDate(2021, 1, 1))
                    .With(bo => bo.VehiclePrice, Money.PoundSterling(1200))
                    .With(bo => bo.Deposit, Money.PoundSterling(0))
                    .With(bo => bo.ArrangementFee, Money.PoundSterling(0))
                    .With(bo => bo.CompletionFee, Money.PoundSterling(100))
                    .Build();

                // act
                var actual = quote.CalculatePaymentSchedule();

                // assert
                actual.Payments.Last().Amount.Should().Be(new Money(200m));
            }

            [Test]
            public void DepositIsSubtractedFromPaymentSchedule()
            {
                // arrange
                var quote = Builder<Quote>.CreateNew()
                    .With(bo => bo.TermInMonths, 12)
                    .With(bo => bo.DeliveryDate, new LocalDate(2021, 1, 1))
                    .With(bo => bo.VehiclePrice, Money.PoundSterling(2400))
                    .With(bo => bo.Deposit, Money.PoundSterling(1200))
                    .With(bo => bo.ArrangementFee, Money.PoundSterling(0))
                    .With(bo => bo.CompletionFee, Money.PoundSterling(0))
                    .Build();

                // act
                var actual = quote.CalculatePaymentSchedule();

                // assert
                actual.Payments.Sum(p => p.Amount.Amount).Should().Be(1200m);
            }

            [TestCase(800, 12, 66.67, 66.63)]
            [TestCase(1200, 12, 100, 100)]
            public void MonthlyPaymentIsCorrect(double vehiclePrice, int term, double expectedMonthlyPayment, double expectedFinalPayment)
            {
                // arrange
                var quote = Builder<Quote>.CreateNew()
                    .With(bo => bo.TermInMonths, term)
                    .With(bo => bo.DeliveryDate, new LocalDate(2021, 1, 1))
                    .With(bo => bo.VehiclePrice, new Money(vehiclePrice))
                    .With(bo => bo.Deposit, Money.PoundSterling(0))
                    .With(bo => bo.ArrangementFee, Money.PoundSterling(0))
                    .With(bo => bo.CompletionFee, Money.PoundSterling(0))
                    .Build();

                // act
                var actual = quote.CalculatePaymentSchedule();

                // assert
                var finalMonthlyPayment = actual.Payments.Last();
                finalMonthlyPayment.Amount.Should().Be(Money.PoundSterling(expectedFinalPayment));
                
                var allOtherMonthlyPayments = actual.Payments.Take(actual.Payments.Count - 1);
                foreach (var monthlyPayment in allOtherMonthlyPayments)
                {
                    monthlyPayment.Amount.Should().Be(Money.PoundSterling(expectedMonthlyPayment));
                }
            }
        }
    }
}