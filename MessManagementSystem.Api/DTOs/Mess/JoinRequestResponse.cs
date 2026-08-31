namespace MessManagementSystem.Api.DTOs.Mess;

public class JoinRequestResponse
{
    public int Id { get; set; }

    public int MessId { get; set; }

    public int MemberId { get; set; }

    public string MemberName { get; set; } = string.Empty;

    public string MemberEmail { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; }
}