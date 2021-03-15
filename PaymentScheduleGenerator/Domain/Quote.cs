using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodaMoney;
using NodaMoney.Extensions;
using NodaTime;

namespace PaymentScheduleGenerator.Domain
{
    public class Quote
    {
        public Quote(
            decimal vehiclePrice,
            decimal deposit,
            decimal arrangementFee,
            decimal completionFee,
            DateTime deliveryDate,
            int termInMonths)
        {
            VehiclePrice = vehiclePrice;
            Deposit = deposit;
            ArrangementFee = arrangementFee;
            CompletionFee = completionFee;
            DeliveryDate = LocalDate.FromDateTime(deliveryDate);
            TermInMonths = termInMonths;
        }

        public Quote()
        {
        }

        public Money VehiclePrice { get; protected set; }

        public Money Deposit { get; protected set; }

        public Money ArrangementFee { get; protected set; }

        public Money CompletionFee { get; protected set; }

        public LocalDate DeliveryDate { get; protected set; }

        public int TermInMonths { get; protected set; }

        public PaymentSchedule CalculatePaymentSchedule()
        {
            var payments = CalculatePayments();
            return new PaymentSchedule(VehiclePrice, Deposit, payments);
        }

        private IList<Payment> CalculatePayments()
        {
            var loanValue = VehiclePrice - Deposit;

            var monthlyPaymentValues = loanValue.SafeDivide(TermInMonths).ToImmutableArray();
            var firstPaymentDate = GetNextPaymentDate(DeliveryDate);
            var nextPaymentDate = GetNextPaymentDate(firstPaymentDate);
            
            var payments = new List<Payment>();
            payments.Add(GetFirstPayment(monthlyPaymentValues.First(), firstPaymentDate));

            for (int paymentMonth = 1; paymentMonth < TermInMonths - 1; paymentMonth++)
            {
                payments.Add(new Payment(nextPaymentDate, monthlyPaymentValues[paymentMonth]));
                nextPaymentDate = GetNextPaymentDate(nextPaymentDate);
            }

            payments.Add(GetFinalPayment(monthlyPaymentValues.Last(), nextPaymentDate));

            return payments;
        }

        private Payment GetFirstPayment(Money firstPayment, LocalDate dueDate)
        {
            var firstPaymentWithFees = firstPayment + ArrangementFee;
            return new Payment(dueDate, firstPaymentWithFees);
        }

        private Payment GetFinalPayment(Money finalPayment, LocalDate dueDate)
        {
            var finalPaymentWithFees = finalPayment + CompletionFee;
            return new Payment(dueDate, finalPaymentWithFees);
        }

        private static LocalDate GetNextPaymentDate(LocalDate currentDate)
        {
            var firstDayOfCurrentMonth = currentDate.ToYearMonth().OnDayOfMonth(1);
            var firstDayOfNextMonth = firstDayOfCurrentMonth.Plus(Period.FromMonths(1));

            if (firstDayOfNextMonth.DayOfWeek == IsoDayOfWeek.Monday)
            {
                return firstDayOfNextMonth;
            }

            return firstDayOfNextMonth.Next(IsoDayOfWeek.Monday);
        }
    }
}
