import { useState } from "react";
import { searchMess, sendJoinRequest } from "../api/mess";
import { ErrorAlert, SuccessAlert, EmptyState, Loading } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

export default function JoinMess() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState(null);
  const [searching, setSearching] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [requestingId, setRequestingId] = useState(null);

  async function handleSearch(e) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!query.trim()) {
      setError("Enter a mess name or mess code to search.");
      return;
    }

    setSearching(true);
    try {
      const res = await searchMess(query.trim());
      setResults(res.data);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSearching(false);
    }
  }

  async function handleJoin(messId) {
    setError("");
    setSuccess("");
    setRequestingId(messId);
    try {
      await sendJoinRequest(messId);
      setSuccess("Join request sent. Waiting for the mess admin to approve it.");
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setRequestingId(null);
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Find & join a mess</h1>
          <p>Search by mess name or mess code, then send a join request to the admin.</p>
        </div>
      </div>

      <div className="card">
        <ErrorAlert message={error} />
        <SuccessAlert message={success} />

        <form onSubmit={handleSearch} className="form-inline">
          <div className="field" style={{ flex: 1, marginBottom: 0 }}>
            <label htmlFor="query">Mess name or code</label>
            <input
              id="query"
              type="text"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="e.g. Green Road or 483920"
            />
          </div>
          <button className="btn" type="submit" disabled={searching}>
            {searching ? "Searching..." : "Search"}
          </button>
        </form>
      </div>

      {searching && <Loading label="Searching..." />}

      {!searching && results && results.length === 0 && (
        <EmptyState title="No mess found" description="Try a different name or code." />
      )}

      {!searching && results && results.length > 0 && (
        <div className="card">
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Mess code</th>
                  <th>Members</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {results.map((mess) => (
                  <tr key={mess.id}>
                    <td>{mess.name}</td>
                    <td>{mess.messCode}</td>
                    <td>{mess.memberCount}</td>
                    <td>
                      <button
                        className="btn btn-sm"
                        onClick={() => handleJoin(mess.id)}
                        disabled={requestingId === mess.id}
                      >
                        {requestingId === mess.id ? "Sending..." : "Send join request"}
                      </button>
                    </td>
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
