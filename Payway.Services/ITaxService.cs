using System;
using System.Collections.Generic;
using System.Text;

namespace Payway.Services
{
    public interface ITaxService
    {
        decimal TaxAmount(decimal totalAmount);     //the tax amount depends on the totalAmount we earn
    }
}
