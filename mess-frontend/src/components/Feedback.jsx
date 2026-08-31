// Small, reusable presentational components used across pages.

export function Loading({ label = "Loading..." }) {
  return <div className="loading-state">{label}</div>;
}

export function ErrorAlert({ message }) {
  if (!message) return null;
  return <div className="alert alert-error">{message}</div>;
}

export function SuccessAlert({ message }) {
  if (!message) return null;
  return <div className="alert alert-success">{message}</div>;
}

export function InfoAlert({ message }) {
  if (!message) return null;
  return <div className="alert alert-info">{message}</div>;
}

export function EmptyState({ title, description }) {
  return (
    <div className="empty-state">
      <h3>{title}</h3>
      {description && <p>{description}</p>}
    </div>
  );
}

export function StatusBadge({ status }) {
  const key = (status || "").toLowerCase();
  const className =
    key === "approved" || key === "success"
      ? "badge badge-approved"
      : key === "rejected"
      ? "badge badge-rejected"
      : "badge badge-pending";
  return <span className={className}>{status}</span>;
}
