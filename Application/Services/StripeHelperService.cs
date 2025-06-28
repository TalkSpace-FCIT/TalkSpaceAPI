using Application.Abstractions;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StripeHelperService:IStripeHelperService
    {
        public StripeHelperService() { }

        public Event PrepareStripeEvent(string json, string signature, string endpointSecret)
        {
            return EventUtility.ConstructEvent(json, signature, endpointSecret);
        }
    }
}
