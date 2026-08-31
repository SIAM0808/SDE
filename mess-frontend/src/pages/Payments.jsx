import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { getMessMembers } from "../api/mess";
import { createMemberPayment } from "../api/payments";
import { Loading, ErrorAlert, SuccessAlert, EmptyState } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export default function Payments() {
  const { member } = useAuth();
  const messId = member.messId;

  const [members, setMembers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const [memberId, setMemberId] = useState("");
  const [amount, setAmount] = useState("");
  const [paymentDate, setPaymentDate] = useState(todayIso());

  // Session-only log of payments recorded in this browser session.
  // The API doesn't expose a GET endpoint for payment history.
  const [recentlyRecorded, setRecentlyRecorded] = useState([]);

  useEffect(() => {
    async function load() {
      setLoading(true);
      setError("");
      try {
        const res = await getMessMembers(messId);
        setMembers(res.data);
        if (res.data.length > 0) setMemberId(String(res.data[0].id));
      } catch (err) {
        setError(getErrorMessage(err));
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [messId]);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setSuccess("");

    const amountNum = Number(amount);
    if (!memberId) {
      setError("Choose a member.");
      return;
    }
    if (!amountNum || amountNum <= 0) {
      setError("Amount must be greater than zero.");
      return;
    }

    setSubmitting(true);
    try {
      const res = await createMemberPayment(messId, {
        memberId: Number(memberId),
        amount: amountNum,
        paymentDate,
      });
      const memberName = members.find((m) => m.id === Number(memberId))?.name || "Member";
      setRecentlyRecorded((prev) => [{ ...res.data.payment, memberName }, ...prev]);
      setSuccess("Payment recorded.");
      setAmount("");
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Member payments</h1>
          <p>Record money a member has paid into the mess.</p>
        </div>
      </div>

      <ErrorAlert message={error} />
      <SuccessAlert message={success} />

      <div className="card">
        <h2>Record a payment</h2>
        {loading ? (
          <Loading />
        ) : members.length === 0 ? (
          <EmptyState title="No members in this mess yet" />
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="form-grid">
              <div className="field">
                <label htmlFor="member">Member</label>
                <select id="member" value={memberId} onChange={(e) => setMemberId(e.target.value)}>
                  {members.map((m) => (
                    <option key={m.id} value={m.id}>
                      {m.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="amount">Amount</label>
                <input
                  id="amount"
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                />
              </div>
              <div className="field">
                <label htmlFor="paymentDate">Date</label>
                <input
                  id="paymentDate"
                  type="date"
                  value={paymentDate}
                  onChange={(e) => setPaymentDate(e.target.value)}
                />
              </div>
            </div>
            <button className="btn" type="submit" disabled={submitting}>
              {submitting ? "Recording..." : "Record payment"}
            </button>
          </form>
        )}
      </div>

      {recentlyRecorded.length > 0 && (
        <div className="card">
          <h2>Recorded this session</h2>
          <p className="field-hint" style={{ marginTop: -8, marginBottom: 16 }}>
            The API doesn't provide a payment history endpoint, so this list only reflects what
            you've recorded since opening this page.
          </p>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Member</th>
                  <th>Amount</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {recentlyRecorded.map((p) => (
                  <tr key={p.id}>
                    <td>{p.memberName}</td>
                    <td>{Number(p.amount).toFixed(2)}</td>
                    <td>{new Date(p.paymentDate).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
