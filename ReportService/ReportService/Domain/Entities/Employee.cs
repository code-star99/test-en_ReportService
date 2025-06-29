using System;
using System.Globalization;

namespace ReportService.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Inn { get; private set; }
        public string Department { get; private set; }
        public decimal Salary { get; private set; }
        public string EmployeeCode { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Employee() { }
        public Employee(string name, string inn, string department)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Inn = inn ?? throw new ArgumentNullException(nameof(inn));
            Department = department ?? throw new ArgumentNullException(nameof(department));
            Salary = 0;
            EmployeeCode = string.Empty;
        }

        public void UpdateSalary(decimal newSalary)
        {
            if (newSalary < 0) throw new ArgumentException("Salary cannot be negative");
            Salary = Math.Round(newSalary, 2);
        }

        public void UpdateEmployeeCode(string employeeCode)
        {
            EmployeeCode = employeeCode?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        public void TransferToDepartment(string newDepartment)
        {
            Department = newDepartment ?? throw new ArgumentNullException(nameof(newDepartment));
        }
    }
}
