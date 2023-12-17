using System.ComponentModel.DataAnnotations;

namespace GDMan.Core.Models;

public enum ContentType
{
    [Display(Description = "application/json")]
    Json,
}