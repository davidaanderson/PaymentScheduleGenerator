using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaymentScheduleGenerator.Config;
using PaymentScheduleGenerator.Domain;

namespace PaymentScheduleGenerator.Controllers
{
    public class QuoteController : Controller
    {
        protected QuoteSettings QuoteSettings { get; }

        public QuoteController(IOptions<QuoteSettings> quoteSettings)
        {
            QuoteSettings = quoteSettings.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(CreatePaymentScheduleCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            var paymentSchedule = command.Execute(User, QuoteSettings);

            return View("PaymentSchedule", paymentSchedule);
        }
    }
}
