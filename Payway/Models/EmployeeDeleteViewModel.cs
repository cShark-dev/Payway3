using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payway.Models
{
    public class EmployeeDeleteViewModel
    {
        public int Id { get; set; }         //We only need an ID to delete the record, because the ID is the unique identifier
        public string FullName { get; set; }
    }
}
