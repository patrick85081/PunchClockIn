using Punches.Models;

namespace Punches.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly DataContext dataContext;
    public EmployeeRepository(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    public IEnumerable<Employee> GetAll()
    {
        return dataContext.Employee
            .FindAll()
            .OrderBy(e => e.Index);
    }

    public void Set(IEnumerable<Employee> data)
    {
        dataContext.Employee.Upsert(data);
    }

    public void Remove(string employeeId)
    {
        dataContext.Employee.Delete(employeeId);
    }
}