
using System.ComponentModel.DataAnnotations;

namespace Punches.Models;

public class Employee : ICloneable
{
    [Key]
    public string Id => $"{ChineseName}{EnglishName}";
    public string Department { get; set; }
    public string ChineseName { get; set; }
    public string EnglishName { get; set; }
    public string Ext { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string SkypeId { get; set; }
    public int Index { get; set; }

    public Employee Clone()
    {
        return this.MemberwiseClone() as Employee;
    }
    object ICloneable.Clone()
    {
        return Clone();
    }
}