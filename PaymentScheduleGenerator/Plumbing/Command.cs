using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using PaymentScheduleGenerator.Config;

namespace PaymentScheduleGenerator.Plumbing
{
    public abstract class Command<TReturn>
    {
        protected abstract IList<string> AuthorisedRoles { get; }

        protected abstract bool IsTargetObjectInUserScope(IPrincipal user);

        protected abstract bool IsCommandStateAcceptable(IPrincipal user);

        protected abstract TReturn PerformAction(QuoteSettings settings);

        public TReturn Execute(IPrincipal currentUser, QuoteSettings settings)
        {
            Authorise(currentUser);
            
            return PerformAction(settings);
        }
        
        private void Authorise(IPrincipal currentUser)
        {
            var userIsAuthorised = UserIsInAuthorisedRole(currentUser) 
                                   && IsTargetObjectInUserScope(currentUser) 
                                   && IsCommandStateAcceptable(currentUser);

            if (!userIsAuthorised)
            {
                throw new UserUnauthorisedException();
            }
        }

        private bool UserIsInAuthorisedRole(IPrincipal currentUser)
        {
            return !AuthorisedRoles.Any(currentUser.IsInRole);
        }
    }
}
