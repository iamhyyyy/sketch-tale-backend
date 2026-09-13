using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sketch_tale.Application.DTOs;

public abstract class BaseFieldDto
{
    [JsonPropertyOrder(-1)]
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdateBy { get; set; }
}
