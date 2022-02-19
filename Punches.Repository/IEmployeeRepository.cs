using Punches.Models;

namespace Punches.Repository;

public interface IEmployeeRepository
{
    IEnumerable<Employee> GetAll();
    void Set(IEnumerable<Employee> data);
    void Remove(string employeeId);
}