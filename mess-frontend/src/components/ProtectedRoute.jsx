import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { Loading } from "./Feedback";

export default function ProtectedRoute({ children, requireMess = false, requireAdmin = false }) {
  const { isAuthenticated, loading, isInMess, isAdmin } = useAuth();

  if (loading) {
    return <Loading label="Checking your session..." />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requireMess && !isInMess) {
    return <Navigate to="/" replace />;
  }

  if (requireAdmin && !isAdmin) {
    return <Navigate to="/" replace />;
  }

  return children;
}
