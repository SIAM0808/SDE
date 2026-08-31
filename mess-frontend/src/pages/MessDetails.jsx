import { useEffect, useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import {
  updateMess,
  getJoinRequests,
  approveJoinRequest,
  rejectJoinRequest,
  getMessMembers,
  removeMember,
  leaveMess,
  deleteMess,
} from "../api/mess";
import { Loading, ErrorAlert, SuccessAlert, EmptyState } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

export default function MessDetails() {
  const { member, isAdmin, refreshMember } = useAuth();
  const navigate = useNavigate();
  const messId = member.messId;

  const [members, setMembers] = useState([]);
  const [joinRequests, setJoinRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [nameInput, setNameInput] = useState(member.messName || "");
  const [savingName, setSavingName] = useState(false);
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const requests = [getMessMembers(messId)];
      if (isAdmin) requests.push(getJoinRequests(messId));

      const results = await Promise.all(requests);
      setMembers(results[0].data);
      if (isAdmin) setJoinRequests(results[1].data);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [messId, isAdmin]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleRename(e) {
    e.preventDefault();
    setError("");
    setSuccess("");
    if (!nameInput.trim()) {
      setError("Mess name cannot be empty.");
      return;
    }
    setSavingName(true);
    try {
      await updateMess(messId, nameInput.trim());
      await refreshMember();
      setSuccess("Mess name updated.");
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSavingName(false);
    }
  }

  async function handleApprove(requestId) {
    setError("");
    setSuccess("");
    setBusyId(requestId);
    try {
      await approveJoinRequest(messId, requestId);
      setSuccess("Join request approved.");
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  async function handleReject(requestId) {
    setError("");
    setSuccess("");
    setBusyId(requestId);
    try {
      await rejectJoinRequest(messId, requestId);
      setSuccess("Join request rejected.");
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  async function handleRemoveMember(memberId) {
    if (!window.confirm("Remove this member from the mess?")) return;
    setError("");
    setSuccess("");
    setBusyId(memberId);
    try {
      await removeMember(messId, memberId);
      setSuccess("Member removed.");
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  async function handleLeave() {
    if (!window.confirm("Leave this mess? You can only leave once your balance is fully settled.")) return;
    setError("");
    try {
      await leaveMess(messId);
      await refreshMember();
      navigate("/");
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function handleDeleteMess() {
    if (!window.confirm("Delete this mess permanently? This cannot be undone.")) return;
    setError("");
    try {
      await deleteMess(messId);
      await refreshMember();
      navigate("/");
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  const pendingRequests = joinRequests.filter((r) => r.status === "Pending");

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>{member.messName}</h1>
          <p>Mess code: {member.messCode || "—"}</p>
        </div>
      </div>

      <ErrorAlert message={error} />
      <SuccessAlert message={success} />

      {isAdmin && (
        <div className="card">
          <h2>Mess settings</h2>
          <form onSubmit={handleRename} className="form-inline">
            <div className="field" style={{ flex: 1, marginBottom: 0 }}>
              <label htmlFor="messName">Mess name</label>
              <input
                id="messName"
                type="text"
                value={nameInput}
                onChange={(e) => setNameInput(e.target.value)}
              />
            </div>
            <button className="btn" type="submit" disabled={savingName}>
              {savingName ? "Saving..." : "Save name"}
            </button>
          </form>
        </div>
      )}

      {loading ? (
        <Loading />
      ) : (
        <>
          {isAdmin && (
            <div className="card">
              <h2>Join requests</h2>
              {pendingRequests.length === 0 ? (
                <EmptyState title="No pending join requests" />
              ) : (
                <div className="table-wrap">
                  <table>
                    <thead>
                      <tr>
                        <th>Name</th>
                        <th>Email</th>
                        <th>Requested</th>
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      {pendingRequests.map((r) => (
                        <tr key={r.id}>
                          <td>{r.memberName}</td>
                          <td>{r.memberEmail}</td>
                          <td>{new Date(r.requestDate).toLocaleDateString()}</td>
                          <td>
                            <div className="btn-row">
                              <button
                                className="btn btn-sm"
                                disabled={busyId === r.id}
                                onClick={() => handleApprove(r.id)}
                              >
                                Approve
                              </button>
                              <button
                                className="btn btn-secondary btn-sm"
                                disabled={busyId === r.id}
                                onClick={() => handleReject(r.id)}
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
          )}

          <div className="card">
            <h2>Members ({members.length})</h2>
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Phone</th>
                    <th>Joined</th>
                    {isAdmin && <th></th>}
                  </tr>
                </thead>
                <tbody>
                  {members.map((m) => (
                    <tr key={m.id}>
                      <td>
                        {m.name} {m.id === member.memberId && "(you)"}
                      </td>
                      <td>{m.email}</td>
                      <td>{m.phone}</td>
                      <td>{new Date(m.joinDate).toLocaleDateString()}</td>
                      {isAdmin && (
                        <td>
                          {m.id !== member.memberId && (
                            <button
                              className="btn btn-danger btn-sm"
                              disabled={busyId === m.id}
                              onClick={() => handleRemoveMember(m.id)}
                            >
                              Remove
                            </button>
                          )}
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          <div className="card">
            <h2>{isAdmin ? "Danger zone" : "Leave mess"}</h2>
            {isAdmin ? (
              <>
                <p>
                  Deleting the mess is permanent and only possible once every member's balance is
                  settled.
                </p>
                <button className="btn btn-danger" onClick={handleDeleteMess}>
                  Delete mess
                </button>
              </>
            ) : (
              <>
                <p>You can leave once your financial balance with the mess is fully settled.</p>
                <button className="btn btn-danger" onClick={handleLeave}>
                  Leave mess
                </button>
              </>
            )}
          </div>
        </>
      )}
    </div>
  );
}
