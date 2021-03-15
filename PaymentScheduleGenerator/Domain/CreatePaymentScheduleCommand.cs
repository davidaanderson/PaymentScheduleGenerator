using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentValidation;
using PaymentScheduleGenerator.Config;
using PaymentScheduleGenerator.Plumbing;

namespace PaymentScheduleGenerator.Domain
{
    public class CreatePaymentScheduleCommand : Command<PaymentSchedule>
    {
        public class Validator : AbstractValidator<CreatePaymentScheduleCommand>
        {
            public Validator()
            {
                RuleFor(x => x.VehiclePrice)
                    .GreaterThan(0);
                
                RuleFor(x => x.TermInMonths)
                    .Must(term => new [] { 12, 24, 36 }.Contains(term))
                    .WithMessage("The term must be 12, 24 or 36 months.");
                
                RuleFor(x => x.Deposit)
                    .Must((model, deposit) => model.VehiclePrice * 0.15m <= deposit)
                    .WithMessage("Deposit must be a minimum of 15% of the vehicle price.");

                RuleFor(x => x.DeliveryDate)
                    .GreaterThanOrEqualTo(DateTime.Today);
            }
        }
        
        protected override IList<string> AuthorisedRoles => new List<string>();

        protected override bool IsTargetObjectInUserScope(IPrincipal user)
        {
            return true;
        }

        protected override bool IsCommandStateAcceptable(IPrincipal user)
        {
            return true;
        }

        protected override PaymentSchedule PerformAction(QuoteSettings settings)
        {
            var quote = new Quote(
                VehiclePrice,
                Deposit,
                settings.ArrangementFee,
                settings.CompletionFee,
                DeliveryDate,
                TermInMonths);
            return quote.CalculatePaymentSchedule();
        }

        public decimal VehiclePrice { get; set; }
        
        public decimal Deposit { get; set; }

        public int TermInMonths { get; set; }
        
        public DateTime DeliveryDate { get; set; }
    }
}
