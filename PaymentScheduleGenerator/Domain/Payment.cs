using NodaMoney;
using NodaTime;

namespace PaymentScheduleGenerator.Domain
{
    public class Payment
    {
        public Payment(LocalDate dueDate, Money amount)
        {
            DueDate = dueDate;
            Amount = amount;
        }

        public LocalDate DueDate { get; protected set; }

        public Money Amount { get; protected set; }
    }
}