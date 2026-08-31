import client from "./client";

// GET /api/financial/member-summary?messId=&memberId=&year=&month=
export function getMemberFinancialSummary({ messId, memberId, year, month }) {
  return client.get("/financial/member-summary", {
    params: { messId, memberId, year, month },
  });
}
