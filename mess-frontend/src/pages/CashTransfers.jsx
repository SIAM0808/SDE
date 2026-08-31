import { useEffect, useState, useCallback } from "react";
import { useAuth } from "../context/AuthContext";
import { getMessMembers } from "../api/mess";
import {
  createCashTransfer,
  getMyPendingTransfers,
  approveCashTransfer,
  rejectCashTransfer,
} from "../api/payments";
import { Loading, ErrorAlert, SuccessAlert, EmptyState } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export default function CashTransfers() {
  const { member, isAdmin } = useAuth();
  const messId = member.messId;

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // Admin: create transfer
  const [members, setMembers] = useState([]);
  const [memberId, setMemberId] = useState("");
  const [amount, setAmount] = useState("");
  const [transferDate, setTransferDate] = useState(todayIso());
  const [submitting, setSubmitting] = useState(false);
  const [loadingMembers, setLoadingMembers] = useState(isAdmin);

  // My pending transfers
  const [pending, setPending] = useState([]);
  const [loadingPending, setLoadingPending] = useState(true);
  const [busyId, setBusyId] = useState(null);

  const loadPending = useCallback(async () => {
    setLoadingPending(true);
    try {
      const res = await getMyPendingTransfers();
      setPending(res.data);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoadingPending(false);
    }
  }, []);

  useEffect(() => {
    loadPending();
  }, [loadPending]);

  useEffect(() => {
    if (!isAdmin) return;
    async function load() {
      setLoadingMembers(true);
      try {
        const res = await getMessMembers(messId);
        setMembers(res.data);
        if (res.data.length > 0) setMemberId(String(res.data[0].id));
      } catch (err) {
        setError(getErrorMessage(err));
      } finally {
        setLoadingMembers(false);
      }
    }
    load();
  }, [isAdmin, messId]);

  async function handleCreate(e) {
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
      await createCashTransfer(messId, { memberId: Number(memberId), amount: amountNum, transferDate });
      setSuccess("Cash transfer created. Waiting for the member to approve it.");
      setAmount("");
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  async function handleApprove(transferId) {
    setBusyId(transferId);
    setError("");
    setSuccess("");
    try {
      await approveCashTransfer(transferId);
      setSuccess("Cash transfer approved.");
      await loadPending();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  async function handleReject(transferId) {
    setBusyId(transferId);
    setError("");
    setSuccess("");
    try {
      await rejectCashTransfer(transferId);
      setSuccess("Cash transfer rejected.");
      await loadPending();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Cash transfers</h1>
          <p>Money the admin gives to a member, which the member must approve.</p>
        </div>
      </div>

      <ErrorAlert message={error} />
      <SuccessAlert message={success} />

      {isAdmin && (
        <div className="card">
          <h2>Give money to a member</h2>
          {loadingMembers ? (
            <Loading />
          ) : members.length === 0 ? (
            <EmptyState title="No members in this mess yet" />
          ) : (
            <form onSubmit={handleCreate}>
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
                  <label htmlFor="transferDate">Date</label>
                  <input
                    id="transferDate"
                    type="date"
                    value={transferDate}
                    onChange={(e) => setTransferDate(e.target.value)}
                  />
                </div>
              </div>
              <button className="btn" type="submit" disabled={submitting}>
                {submitting ? "Creating..." : "Create transfer"}
              </button>
            </form>
          )}
        </div>
      )}

      <div className="card">
        <h2>My pending transfers</h2>
        {loadingPending ? (
          <Loading />
        ) : pending.length === 0 ? (
          <EmptyState title="Nothing pending" description="You have no cash transfers waiting for approval." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Amount</th>
                  <th>Date</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {pending.map((t) => (
                  <tr key={t.id}>
                    <td>{Number(t.amount).toFixed(2)}</td>
                    <td>{new Date(t.transferDate).toLocaleDateString()}</td>
                    <td>
                      <div className="btn-row">
                        <button
                          className="btn btn-sm"
                          disabled={busyId === t.id}
                          onClick={() => handleApprove(t.id)}
                        >
                          Approve
                        </button>
                        <button
                          className="btn btn-secondary btn-sm"
                          disabled={busyId === t.id}
                          onClick={() => handleReject(t.id)}
                        >
                          Reject
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
