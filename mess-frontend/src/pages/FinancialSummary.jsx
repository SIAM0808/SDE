import { useEffect, useState, useCallback } from "react";
import { useAuth } from "../context/AuthContext";
import { getMessMembers } from "../api/mess";
import { getMemberFinancialSummary } from "../api/financial";
import { Loading, ErrorAlert } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

const MONTHS = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

export default function FinancialSummary() {
  const { member, isAdmin } = useAuth();
  const messId = member.messId;
  const now = new Date();

  const [members, setMembers] = useState([]);
  const [memberId, setMemberId] = useState(member.memberId);
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);

  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!isAdmin) return;
    getMessMembers(messId)
      .then((res) => setMembers(res.data))
      .catch(() => {});
  }, [isAdmin, messId]);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const res = await getMemberFinancialSummary({ messId, memberId, year, month });
      setSummary(res.data);
    } catch (err) {
      setError(getErrorMessage(err));
      setSummary(null);
    } finally {
      setLoading(false);
    }
  }, [messId, memberId, year, month]);

  useEffect(() => {
    load();
  }, [load]);

  const years = Array.from({ length: 5 }, (_, i) => now.getFullYear() - 2 + i);

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Financial summary</h1>
          <p>See a member's dues breakdown for a given month.</p>
        </div>
      </div>

      <div className="card">
        <div className="form-inline">
          {isAdmin && (
            <div className="field" style={{ marginBottom: 0 }}>
              <label htmlFor="member">Member</label>
              <select id="member" value={memberId} onChange={(e) => setMemberId(Number(e.target.value))}>
                {members.map((m) => (
                  <option key={m.id} value={m.id}>
                    {m.name}
                  </option>
                ))}
              </select>
            </div>
          )}
          <div className="field" style={{ marginBottom: 0 }}>
            <label htmlFor="month">Month</label>
            <select id="month" value={month} onChange={(e) => setMonth(Number(e.target.value))}>
              {MONTHS.map((m, i) => (
                <option key={m} value={i + 1}>
                  {m}
                </option>
              ))}
            </select>
          </div>
          <div className="field" style={{ marginBottom: 0 }}>
            <label htmlFor="year">Year</label>
            <select id="year" value={year} onChange={(e) => setYear(Number(e.target.value))}>
              {years.map((y) => (
                <option key={y} value={y}>
                  {y}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      <ErrorAlert message={error} />

      {loading ? (
        <Loading />
      ) : (
        summary && (
          <>
            <div className="card">
              <h2>{summary.memberName}'s summary</h2>
              <div className="stat-grid">
                <div className="stat">
                  <div className="stat-label">Paid in</div>
                  <div className="stat-value">{summary.givenMoney.toFixed(2)}</div>
                </div>
                <div className="stat">
                  <div className="stat-label">Meal cost</div>
                  <div className="stat-value">{summary.mealCost.toFixed(2)}</div>
                </div>
                <div className="stat">
                  <div className="stat-label">Total expense share</div>
                  <div className="stat-value">{summary.totalExpense.toFixed(2)}</div>
                </div>
                <div className="stat">
                  <div className="stat-label">Due</div>
                  <div className={`stat-value ${summary.due < 0 ? "due-negative" : "due-positive"}`}>
                    {summary.due.toFixed(2)}
                  </div>
                </div>
              </div>
            </div>

            <div className="card">
              <h2>Breakdown</h2>
              <div className="table-wrap">
                <table>
                  <tbody>
                    <tr>
                      <td>House rent share</td>
                      <td>{summary.houseRent.toFixed(2)}</td>
                    </tr>
                    <tr>
                      <td>Chief bill share</td>
                      <td>{summary.chiefBill.toFixed(2)}</td>
                    </tr>
                    <tr>
                      <td>Others share</td>
                      <td>{summary.othersBill.toFixed(2)}</td>
                    </tr>
                    <tr>
                      <td>Total meals this month</td>
                      <td>{summary.totalMeals}</td>
                    </tr>
                    <tr>
                      <td>Meal rate</td>
                      <td>{summary.mealRate.toFixed(2)}</td>
                    </tr>
                    <tr>
                      <td>Meal cost</td>
                      <td>{summary.mealCost.toFixed(2)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <p className="field-hint" style={{ marginTop: 16 }}>
                {summary.canOrderMeal
                  ? "Meal ordering is currently allowed for this member."
                  : "Meal ordering is currently blocked because the member's balance is negative."}
                {" "}
                {summary.canLeaveOrBeRemoved
                  ? "This member can leave or be removed since their balance is settled."
                  : "This member cannot leave or be removed until their balance is fully settled."}
              </p>
            </div>
          </>
        )
      )}
    </div>
  );
}
