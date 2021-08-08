using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models {
    public class SQLEmployeeRepository : IEmployeeRepository {
        private AppDbContext context;
        private readonly ILogger<SQLEmployeeRepository> logger;

        public SQLEmployeeRepository(AppDbContext context, ILogger<SQLEmployeeRepository> logger) {
            this.context = context;
            this.logger = logger;
        }
        public Employee Add(Employee employee) {
            context.Employees.Add(employee);
            context.SaveChanges();
            return employee;
        }

        public Employee Delete(int id) {
            Employee employee = context.Employees.Find(id);
            if(employee != null) {
                context.Employees.Remove(employee);
                context.SaveChanges();
            }
            return employee;
        }

        public Employee GetEmployee(int Id) {
            logger.LogTrace("LogTrace Log");
            logger.LogDebug("LogDebug Log");
            logger.LogInformation("LogInformation Log");
            logger.LogWarning("LogWarning Log");
            logger.LogError("LogError Log");
            logger.LogCritical("LogCritical Log");
            return context.Employees.Find(Id);
        }

        public IEnumerable<Employee> GettAllEmployee() {
            return context.Employees;
        }

        public Employee Update(Employee employeeChanges) {
            var employee = context.Employees.Attach(employeeChanges);
            employee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return employeeChanges;
        }
    }
}
