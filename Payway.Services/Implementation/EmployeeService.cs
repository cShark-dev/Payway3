using Microsoft.AspNetCore.Mvc.Rendering;
using Payway.Entity;
using Payway.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payway.Services.Implementation
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private decimal studentLoanAmount;
        //private decimal fee;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task CreateAsync(Employee newEmployee)
        {
            await _context.Employees.AddAsync(newEmployee);
            await _context.SaveChangesAsync();
        }
        public Employee GetById(int employeeId) => _context.Employees.Where(e => e.Id == employeeId).FirstOrDefault(); //Will retrieve an employee who's ID is equal to the ID passed into to this method


        public async Task Delete(int employeeId)
        {
            var employee = GetById(employeeId);
            _context.Remove(employee);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Employee> GetAll() => _context.Employees;

        public async Task UpdateAsync(Employee employee)
        {
            _context.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id)
        {
            var employee = GetById(id);
            _context.Update(employee);
            await _context.SaveChangesAsync();
        }

        public decimal StudentLoanRepaymentAmount(int id, decimal totalAmount)
        {
            var employee = GetById(id);                                //Step 1: Retrieve employee by ID, check if employee is paying studentLoan
            if (employee.StudentLoan == StudentLoan.Yes && totalAmount > 1750 && totalAmount < 2000)
            {
                studentLoanAmount = 15m;
            }
            else if (employee.StudentLoan == StudentLoan.Yes && totalAmount >= 2000 && totalAmount < 2250)
            {
                studentLoanAmount = 38m;
            }
            else if (employee.StudentLoan == StudentLoan.Yes && totalAmount >= 2250 && totalAmount < 2500)
            {
                studentLoanAmount = 60m;
            }
            else if (employee.StudentLoan == StudentLoan.Yes && totalAmount >= 2500)
            {
                studentLoanAmount = 83m;
            }
            else
            {
                studentLoanAmount = 0m;
            }
            return studentLoanAmount;
        }

        public decimal UnionFees(int id)
        {
            var employee = GetById(id);                        //Step 1: Check if the employee is a union member, get employee by ID
            var fee = employee.UnionMember == UnionMember.Yes ? 10m : 0m;
            return fee;
        }

        public IEnumerable<SelectListItem> GetallEmployeesForPayroll()
        {
            return GetAll().Select(emp => new SelectListItem()                    //We already have a collection of Employees the GetAll in this file, we need to project that to a new form which is a select list item
            {
                Text = emp.FullName,
                Value = emp.Id.ToString()

            });
            
        }
    }
}
