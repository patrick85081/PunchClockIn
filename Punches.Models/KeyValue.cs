
using System.ComponentModel.DataAnnotations;

namespace Punches.Models;

public class KeyValue
{
    [Key]
    public string Key { get; set; }
    public string Value { get; set; }
    public string Note { get; set; }
}