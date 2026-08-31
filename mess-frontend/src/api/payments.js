import client from "./client";

// POST /api/memberpayment/{messId}
export function createMemberPayment(messId, { memberId, amount, paymentDate }) {
  return client.post(`/memberpayment/${messId}`, { memberId, amount, paymentDate });
}

// POST /api/membercashtransfer/{messId}
export function createCashTransfer(messId, { memberId, amount, transferDate }) {
  return client.post(`/membercashtransfer/${messId}`, { memberId, amount, transferDate });
}

// GET /api/membercashtransfer/my-pending
export function getMyPendingTransfers() {
  return client.get("/membercashtransfer/my-pending");
}

// POST /api/membercashtransfer/{transferId}/approve
export function approveCashTransfer(transferId) {
  return client.post(`/membercashtransfer/${transferId}/approve`);
}

// POST /api/membercashtransfer/{transferId}/reject
export function rejectCashTransfer(transferId) {
  return client.post(`/membercashtransfer/${transferId}/reject`);
}
