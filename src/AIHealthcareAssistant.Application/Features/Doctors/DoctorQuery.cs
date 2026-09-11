namespace AIHealthcareAssistant.Application.Features.Doctors;

/// <summary>
/// Filter/pagination options for listing doctors. All filters are optional and combine with AND.
/// </summary>
public class DoctorQuery
{
    public Guid? SpecialtyId { get; set; }
    public string? Search { get; set; }
    public bool? AvailableOnly { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }

    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => value
        };
    }
}
