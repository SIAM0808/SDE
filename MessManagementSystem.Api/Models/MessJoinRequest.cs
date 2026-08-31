namespace MessManagementSystem.Api.Models;

public class MessJoinRequest
{
    public int Id { get; set; }

    public int MessId { get; set; }

    public int MemberId { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime RequestDate { get; set; }

    // Navigation properties
    public Mess? Mess { get; set; }

    public Member? Member { get; set; }
}