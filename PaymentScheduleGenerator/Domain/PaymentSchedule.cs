using System.Collections.Generic;
using NodaMoney;

namespace PaymentScheduleGenerator.Domain
{
    public class PaymentSchedule
    {
        public PaymentSchedule(Money vehiclePrice, Money deposit, IList<Payment> payments)
        {
            VehiclePrice = vehiclePrice;
            Deposit = deposit;
            Payments = payments;
        }

        public Money VehiclePrice { get; protected set; }

        public Money Deposit { get; protected set; }

        public IList<Payment> Payments { get; protected set; }
    }
}