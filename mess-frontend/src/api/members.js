import client from "./client";

// GET /api/members/me -> current logged in member + mess info
export function getCurrentMember() {
  return client.get("/members/me");
}
