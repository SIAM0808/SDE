import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { getMemberFinancialSummary } from "../api/financial";
import { getMyMealTotals } from "../api/meals";
import { Loading, ErrorAlert } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

export default function Dashboard() {
  const { member, isInMess, isAdmin } = useAuth();
  const [summary, setSummary] = useState(null);
  const [mealTotals, setMealTotals] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(isInMess);

  useEffect(() => {
    if (!isInMess) return;

    async function load() {
      setLoading(true);
      setError("");
      const now = new Date();
      try {
        const [summaryRes, totalsRes] = await Promise.all([
          getMemberFinancialSummary({
            messId: member.messId,
            memberId: member.memberId,
            year: now.getFullYear(),
            month: now.getMonth() + 1,
          }),
          getMyMealTotals(),
        ]);
        setSummary(summaryRes.data);
        setMealTotals(totalsRes.data);
      } catch (err) {
        setError(getErrorMessage(err));
      } finally {
        setLoading(false);
      }
    }

    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isInMess]);

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Welcome, {member?.name}</h1>
          <p>
            {isInMess
              ? `Member of ${member.messName}${isAdmin ? " · you are the admin" : ""}`
              : "You are not part of a mess yet."}
          </p>
        </div>
      </div>

      {!isInMess && (
        <div className="card">
          <h2>Get started</h2>
          <p>
            You need to be part of a mess to order meals and track expenses. Create a new mess or
            join an existing one using its name or mess code.
          </p>
          <div className="btn-row">
            <Link className="btn" to="/mess/create">
              Create a mess
            </Link>
            <Link className="btn btn-secondary" to="/mess/join">
              Find & join a mess
            </Link>
          </div>
        </div>
      )}

      {isInMess && (
        <>
          <ErrorAlert message={error} />

          {loading ? (
            <Loading />
          ) : (
            <>
              <div className="card">
                <h2>This month at a glance</h2>
                {summary && (
                  <div className="stat-grid">
                    <div className="stat">
                      <div className="stat-label">Paid in</div>
                      <div className="stat-value">{summary.givenMoney.toFixed(2)}</div>
                    </div>
                    <div className="stat">
                      <div className="stat-label">Total expense share</div>
                      <div className="stat-value">{summary.totalExpense.toFixed(2)}</div>
                    </div>
                    <div className="stat">
                      <div className="stat-label">Meals this month</div>
                      <div className="stat-value">{summary.totalMeals}</div>
                    </div>
                    <div className="stat">
                      <div className="stat-label">Due</div>
                      <div className={`stat-value ${summary.due < 0 ? "due-negative" : "due-positive"}`}>
                        {summary.due.toFixed(2)}
                      </div>
                    </div>
                  </div>
                )}
                <p className="field-hint" style={{ marginTop: "16px" }}>
                  <Link to="/financial">View the full financial summary →</Link>
                </p>
              </div>

              <div className="card">
                <h2>Meal totals (all time)</h2>
                {mealTotals && (
                  <div className="stat-grid">
                    <div className="stat">
                      <div className="stat-label">Breakfast</div>
                      <div className="stat-value">{mealTotals.totalBreakfast}</div>
                    </div>
                    <div className="stat">
                      <div className="stat-label">Lunch</div>
                      <div className="stat-value">{mealTotals.totalLunch}</div>
                    </div>
                    <div className="stat">
                      <div className="stat-label">Dinner</div>
                      <div className="stat-value">{mealTotals.totalDinner}</div>
                    </div>
                    <div className="stat">
                      <div className="stat-label">Grand total</div>
                      <div className="stat-value">{mealTotals.grandTotal}</div>
                    </div>
                  </div>
                )}
                <p className="field-hint" style={{ marginTop: "16px" }}>
                  <Link to="/meals">Order or manage meals →</Link>
                </p>
              </div>
            </>
          )}
        </>
      )}
    </div>
  );
}
