using System;

namespace RestAPI.Models.SubModels;

public class GrowthVelocityResult
{
    public string? Period { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public VelocityDetail? Weight { get; set; }
    public VelocityDetail? Height { get; set; }
    public VelocityDetail? HeadCircumference { get; set; }

}
