using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payway.Entity;
using Payway.Models;
using Payway.Services;
using RotativaCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payway.Controllers
{
   // [Authorize(Roles = "Admin, Manager")]
    public class PayController : Controller
    {                
        private readonly IPayComputationService _payComputationService;
        private readonly IEmployeeService _employeeService;
        private readonly ITaxService _taxService;
        private readonly INationalInsuranceContributionService _nationalInsuranceContributionService;
        private decimal overtimeHrs;
        private decimal contractualEarnings;
        private decimal overtimeEarnings;
        private decimal totalEarnings;
        private decimal tax;
        private decimal unionFee;
        private decimal studentLoan;
        private decimal nationalInsurance;
        private decimal totalDeduction;

        public PayController(IPayComputationService payComputationService, 
                            IEmployeeService employeeService, 
                            ITaxService taxService,
                            INationalInsuranceContributionService nationalInsuranceContributionService)              //We need to inject the employee here, inject the IEmployeeService
        {
            _payComputationService = payComputationService;
            _employeeService = employeeService;
            _taxService = taxService;
            _nationalInsuranceContributionService = nationalInsuranceContributionService;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var payRecords = _payComputationService.GetAll().Select(pay => new PaymentRecordIndexViewModel
            {
                Id = pay.Id,
                EmployeeId = pay.EmployeeId,
                FullName = pay.FullName,
                PayDate = pay.PayDate,
                PayMonth = pay.PayMonth,
                TaxYearId = pay.TaxYearId,
                Year = _payComputationService.GetTaxYearById(pay.TaxYearId).YearOfTax,                 //We need to write a method in the PayComputationService and pass this id to that method
                TotalEarnings = pay.TotalEarnings,
                TotalDeduction = pay.TotalDeduction,
                NetPayment = pay.NetPayment,
                Employee = pay.Employee,
            });
            return View(payRecords);          //We need a view model for this index, right click models add new model
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()               //We need the create action method, we start with the GET version
        {
            ViewBag.employees = _employeeService.GetallEmployeesForPayroll();                   //This will render a selectable drop down list of all employees, we need to return the collection of employees as a select list item type
            ViewBag.taxYears = _payComputationService.GetAllTaxYear();      //Retrieves all the tax years and passes that to the Viewbag
            var model = new PaymentRecordCreateViewModel();
            return View(model);      //We need a view model, go to model and add a class, call the class PaymentRecordCreateViewModel
        }

        // The POST version of the Create method begins here

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PaymentRecordCreateViewModel model)
        {
            if (ModelState.IsValid)         //First we need to check if the model state is valid, if it's valid we can create a new payment record

            {
                var payrecord = new PaymentRecord()
                {
                    Id = model.Id,                           //Then we need to a our mapping
                    EmployeeId = model.EmployeeId,
                    FullName = _employeeService.GetById(model.EmployeeId).FullName,
                    NiNo = _employeeService.GetById(model.EmployeeId).NationalInsuranceNo,
                    PayDate = model.PayDate,
                    PayMonth = model.PayMonth,
                    TaxYearId = model.TaxYearId,
                    TaxCode = model.TaxCode,
                    HourlyRate = model.HourlyRate,
                    HoursWorked = model.HoursWorked,
                    ContractualHours =  model.ContractualHours,
                    OvertimeHours = overtimeHrs = _payComputationService.OvertimeHours(model.HoursWorked, model.ContractualHours),                    //We have a method to compute the overtime hours, this method takes 2 args
                    ContractualEarnings = contractualEarnings = _payComputationService.ContractualEarnings(model.ContractualHours, model.HoursWorked,model.HourlyRate),
                    OvertimeEarnings = overtimeEarnings =  _payComputationService.OvertimeEarnings(_payComputationService.OvertimeRate(model.HourlyRate), overtimeHrs),                //This method takes 2 args, the first arg is passed in using a method
                    TotalEarnings = totalEarnings =  _payComputationService.TotalEarnings(overtimeEarnings, contractualEarnings),
                    Tax = tax = _taxService.TaxAmount(totalEarnings),                             //The tax comes from the tax service so we need to inject that interface into the constructor
                    UnionFee = unionFee = _employeeService.UnionFees(model.EmployeeId),
                    SLC = studentLoan = _employeeService.StudentLoanRepaymentAmount(model.EmployeeId, totalEarnings),             //This method takes in 2 args, the id and total amount earned
                    NIC = nationalInsurance =  _nationalInsuranceContributionService.NIContribution(totalEarnings),                                                //We have a service for the NIC, let's inject the interface within the controller
                    TotalDeduction = totalDeduction = _payComputationService.TotalDeduction(tax, nationalInsurance, studentLoan, unionFee),                           //Total deduction method takes 4 args
                    NetPayment = _payComputationService.NetPay(totalEarnings, totalDeduction)                
                };
                await _payComputationService.CreateAsync(payrecord);                   //We are now ready to create a payment, and then we need to pass in payrecord, 
                return RedirectToAction(nameof(Index));                                                            //once we create this we are going to return to index, redirecttoAction

            }

            ViewBag.employees = _employeeService.GetallEmployeesForPayroll();                   //IF the model state fails, we still want the drop down list of all employees, This will render a selectable drop down list of all employees, we need to return the collection of employees as a select list item type
            ViewBag.taxYears = _payComputationService.GetAllTaxYear();      //AND all the tax years together with the view, this create is async so the method Iactionresult needs to be async
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Detail(int id)             //We need to create a detail view model, go to models folder and add a class named "PaymentRecordDetailViewModel"
        {

            var paymentRecord = _payComputationService.GetById(id);          //In order to display the payment records we need to retrieve the payment record by ID
            if (paymentRecord == null)
            {
                return NotFound();
            }

            var model = new PaymentRecordDetailviewModel                                        //Next we need our model
            {

                Id = paymentRecord.Id,
                EmployeeId = paymentRecord.EmployeeId,
                FullName = paymentRecord.FullName,
                NiNo = paymentRecord.NiNo,
                PayDate = paymentRecord.PayDate,
                PayMonth = paymentRecord.PayMonth,
                TaxYearId = paymentRecord.TaxYearId,                    
                Year = _payComputationService.GetTaxYearById(paymentRecord.TaxYearId).YearOfTax,
                TaxCode = paymentRecord.TaxCode, 
                HourlyRate = paymentRecord.HourlyRate,
                HoursWorked = paymentRecord.HoursWorked,
                ContractualHours = paymentRecord.ContractualHours,
                OvertimeHours = paymentRecord.OvertimeHours,
                OvertimeRate = _payComputationService.OvertimeRate(paymentRecord.HourlyRate),
                ContractualEarnings = paymentRecord.ContractualEarnings,
                OvertimeEarnings = paymentRecord.OvertimeEarnings,
                Tax = paymentRecord.Tax, 
                NIC = paymentRecord.NIC,
                UnionFee = paymentRecord.UnionFee,
                SLC = paymentRecord.SLC,
                TotalEarnings = paymentRecord.TotalEarnings,
                TotalDeduction = paymentRecord.TotalDeduction,
                Employee = paymentRecord.Employee,                  //This employee is equal to paymentRecord.Employee
                TaxYear = paymentRecord.TaxYear,
                NetPayment = paymentRecord.NetPayment,
            };
            return View(model);
        }
        [HttpGet]                           //The payslip action method is going to be the same as the details action method, we are using the same detail view model as well 
        public IActionResult Payslip(int id)             //We need to create a detail view model, go to models folder and add a class named "PaymentRecordDetailViewModel"
        {

            var paymentRecord = _payComputationService.GetById(id);          //In order to display the payment records we need to retrieve the payment record by ID
            if (paymentRecord == null)
            {
                return NotFound();
            }

            var model = new PaymentRecordDetailviewModel                                        //Next we need our model
            {

                Id = paymentRecord.Id,
                EmployeeId = paymentRecord.EmployeeId,
                FullName = paymentRecord.FullName,
                NiNo = paymentRecord.NiNo,
                PayDate = paymentRecord.PayDate,
                PayMonth = paymentRecord.PayMonth,
                TaxYearId = paymentRecord.TaxYearId,
                Year = _payComputationService.GetTaxYearById(paymentRecord.TaxYearId).YearOfTax,
                TaxCode = paymentRecord.TaxCode,
                HourlyRate = paymentRecord.HourlyRate,
                HoursWorked = paymentRecord.HoursWorked,
                ContractualHours = paymentRecord.ContractualHours,
                OvertimeHours = paymentRecord.OvertimeHours,
                OvertimeRate = _payComputationService.OvertimeRate(paymentRecord.HourlyRate),
                ContractualEarnings = paymentRecord.ContractualEarnings,
                OvertimeEarnings = paymentRecord.OvertimeEarnings,
                Tax = paymentRecord.Tax,
                NIC = paymentRecord.NIC,
                UnionFee = paymentRecord.UnionFee,
                SLC = paymentRecord.SLC,
                TotalEarnings = paymentRecord.TotalEarnings,
                TotalDeduction = paymentRecord.TotalDeduction,
                Employee = paymentRecord.Employee,                  //This employee is equal to paymentRecord.Employee
                TaxYear = paymentRecord.TaxYear,
                NetPayment = paymentRecord.NetPayment,
            };
            return View(model);
        }

        
        public IActionResult GeneratePayslipPdf(int id)
        {
            var payslip = new ActionAsPdf("Payslip", new { id = id })
            {
                FileName = "payslip.pdf"
            };
            return payslip;
        }

    }
}
