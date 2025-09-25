using System.ComponentModel.DataAnnotations;

namespace Dndcs2.dtos;

public class MetaDtoObject
{
    [MaxLength(200)]
    public string CreatedBy { get; private set; }
    public DateTime CreateDate { get; private set; }
    [MaxLength(200)]
    public string UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool Enabled { get; private set; }

    public MetaDtoObject(string createdBy, DateTime createDate, string updatedBy, DateTime updatedDate, bool enabled)
    {
        CreatedBy = createdBy;
        CreateDate = createDate;
        UpdatedBy = updatedBy;
        UpdatedDate = updatedDate;
        Enabled = enabled;
    }
    
}