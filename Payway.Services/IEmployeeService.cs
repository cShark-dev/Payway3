using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Payway.Entity;

namespace Payway.Services
{
    public interface IEmployeeService
    {
        Task CreateAsync(Employee newEmployee);
        Employee GetById(int employeeId);
        Task UpdateAsync(Employee employee);
        Task UpdateAsync(int id);
        Task Delete(int employeeId);

        
    }
}
