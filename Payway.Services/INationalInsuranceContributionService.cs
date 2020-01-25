using System;
using System.Collections.Generic;
using System.Text;

namespace Payway.Services
{
    public interface INationalInsuranceContributionService
    {
        decimal NIContribution(decimal totalAmount);
    }
}
