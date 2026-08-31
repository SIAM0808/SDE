
namespace MessManagementSystem.Api.DTOs.MemberPayment;

public class CreateMemberPaymentRequest
{
    public int MemberId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }
}

