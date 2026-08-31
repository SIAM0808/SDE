import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createMess } from "../api/mess";
import { useAuth } from "../context/AuthContext";
import { ErrorAlert } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

export default function CreateMess() {
  const { refreshMember } = useAuth();
  const navigate = useNavigate();

  const [name, setName] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");

    if (!name.trim()) {
      setError("Please enter a mess name.");
      return;
    }

    setSubmitting(true);
    try {
      await createMess(name.trim());
      await refreshMember();
      navigate("/");
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
          <h1>Create a mess</h1>
          <p>You'll become the admin and get a mess code others can use to join.</p>
        </div>
      </div>

      <div className="card" style={{ maxWidth: 480 }}>
        <ErrorAlert message={error} />
        <form onSubmit={handleSubmit} noValidate>
          <div className="field">
            <label htmlFor="messName">Mess name</label>
            <input
              id="messName"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Green Road Mess"
            />
          </div>
          <button className="btn" type="submit" disabled={submitting}>
            {submitting ? "Creating..." : "Create mess"}
          </button>
        </form>
      </div>
    </div>
  );
}
