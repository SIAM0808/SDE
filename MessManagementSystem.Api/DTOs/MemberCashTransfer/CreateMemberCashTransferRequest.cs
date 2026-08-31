
namespace MessManagementSystem.Api.DTOs.MemberCashTransfer;

public class CreateMemberCashTransferRequest
{
    public int MemberId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransferDate { get; set; }
}
