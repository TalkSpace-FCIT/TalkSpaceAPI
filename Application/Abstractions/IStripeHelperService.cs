using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IStripeHelperService
    {
        Event PrepareStripeEvent(string json, string secret,string endpointsecret);
    }
}
