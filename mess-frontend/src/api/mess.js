import client from "./client";

// POST /api/mess
export function createMess(name) {
  return client.post("/mess", { name });
}

// GET /api/mess/search?query=
export function searchMess(query) {
  return client.get("/mess/search", { params: { query } });
}

// PUT /api/mess/{messId}
export function updateMess(messId, name) {
  return client.put(`/mess/${messId}`, { name });
}

// POST /api/mess/join
export function sendJoinRequest(messId) {
  return client.post("/mess/join", { messId });
}

// GET /api/mess/{messId}/join-requests
export function getJoinRequests(messId) {
  return client.get(`/mess/${messId}/join-requests`);
}

// POST /api/mess/{messId}/join-requests/approve
export function approveJoinRequest(messId, requestId) {
  return client.post(`/mess/${messId}/join-requests/approve`, { requestId });
}

// POST /api/mess/{messId}/join-requests/reject
export function rejectJoinRequest(messId, requestId) {
  return client.post(`/mess/${messId}/join-requests/reject`, { requestId });
}

// GET /api/mess/{messId}/members
export function getMessMembers(messId) {
  return client.get(`/mess/${messId}/members`);
}

// DELETE /api/mess/{messId}/members/{memberId}
export function removeMember(messId, memberId) {
  return client.delete(`/mess/${messId}/members/${memberId}`);
}

// POST /api/mess/{messId}/leave
export function leaveMess(messId) {
  return client.post(`/mess/${messId}/leave`);
}

// DELETE /api/mess/{messId}
export function deleteMess(messId) {
  return client.delete(`/mess/${messId}`);
}
