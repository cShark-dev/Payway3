using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Payway.Entity;
using Payway.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payway.Services.Implementation
{
    public class PayComputationService : IPayComputationService
    {

        private decimal contractualEarnings;
        private decimal overtimeHours;
        private readonly ApplicationDbContext context;

        public PayComputationService(ApplicationDbContext _context)
        {
            _context = context;

        }
        
      

        public decimal ContractualEarnings(decimal contractualHours, decimal hoursWorked, decimal hourlyRate)
        {
            if (hoursWorked < contractualHours)
            {
                contractualEarnings = hoursWorked * hourlyRate;
            }
            else
            {
                contractualEarnings = contractualHours * hourlyRate;
            }
            return contractualEarnings;
        }

        public async Task CreateAsync(PaymentRecord paymentRecord)
        {
            await context.PaymentRecords.AddAsync(paymentRecord);
            await context.SaveChangesAsync();
        }

        public IEnumerable<PaymentRecord> GetAll() => context.PaymentRecords.OrderBy(p => p.EmployeeId);

        public IEnumerable<SelectListItem> GetAllTaxYear()
        {
            var allTaxYear = context.TaxYears.Select(taxYears => new SelectListItem 
            {
                Text = taxYears.YearOfTax,
                Value = taxYears.Id.ToString()
            });
            return allTaxYear;          //will give a list of all the tax years and then pass this to a viewbag and then render this to a select list in the view
        }

        public PaymentRecord GetById(int id) => context.PaymentRecords.Where(pay => pay.Id == id).FirstOrDefault();

        public decimal NetPay(decimal totalEarnings, decimal totalDeduction) => totalEarnings - totalDeduction;


        public decimal OvertimeEarnings(decimal overtimeRate, decimal overtimeHours) => overtimeHours * overtimeRate;


        public decimal OvertimeHours(decimal hoursWorked, decimal contractualHours)
        {
            if (hoursWorked <= contractualHours)
            {
                overtimeHours = 0.0m;
            }
            else if (hoursWorked > contractualHours)
            {
                overtimeHours = hoursWorked - contractualHours;
            }
            return overtimeHours;
        }

        public decimal OvertimeRate(decimal hourlyRate) => hourlyRate * 1.5m;


        public decimal TotalDeduction(decimal tax, decimal nic, decimal studentLoanRepayment, decimal unionFees)
         => tax + nic + studentLoanRepayment + unionFees;


        public decimal TotalEarnings(decimal overtimeEarnings, decimal contractualEarnings)
         => overtimeEarnings + contractualEarnings;
    }
}
