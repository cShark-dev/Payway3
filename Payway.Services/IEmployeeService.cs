using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payway.Entity;

namespace Payway.Services
{
    public interface IEmployeeService   
    {
        Task CreateAsync(Employee newEmployee);   //to create an employee, we create a new employee called newEmployee
        Employee GetById(int employeeId);  //A method to Retrieve employees by ID, we pass in the employee ID
        Task UpdateAsync(Employee employee); //A method to update the employees record We need to pass in an employee
        Task UpdateAsync(int id); //Update an employee by ID, the method signature is different, a method can have the same name as long they have different signatures
        Task Delete(int employeeId); //A method to delete, you usually delete using a unique identifier, we 
        decimal UnionFees(int id); //Retrieve a union fee, first pass the employee id t ocheck if the employee is a union member, if member then a union fee will apply to this employee 
        decimal StudentLoanRepaymentAmount(int id, decimal totalAmount); //We check if the employee is paying back student loan, we need to know the total amount the employee has earned
        IEnumerable<Employee> GetAll(); //A method to return a list of all the employees, an IEnumerable of employees, a collection of employees that we call GetAll
        IEnumerable<SelectListItem> GetallEmployeesForPayroll();

    }
}
