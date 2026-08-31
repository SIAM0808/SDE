import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Layout from "./components/Layout";

import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import CreateMess from "./pages/CreateMess";
import JoinMess from "./pages/JoinMess";
import MessDetails from "./pages/MessDetails";
import Meals from "./pages/Meals";
import Expenses from "./pages/Expenses";
import Payments from "./pages/Payments";
import CashTransfers from "./pages/CashTransfers";
import FinancialSummary from "./pages/FinancialSummary";

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          <Route
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route path="/" element={<Dashboard />} />
            <Route path="/mess/create" element={<CreateMess />} />
            <Route path="/mess/join" element={<JoinMess />} />

            <Route
              path="/mess"
              element={
                <ProtectedRoute requireMess>
                  <MessDetails />
                </ProtectedRoute>
              }
            />
            <Route
              path="/meals"
              element={
                <ProtectedRoute requireMess>
                  <Meals />
                </ProtectedRoute>
              }
            />
            <Route
              path="/financial"
              element={
                <ProtectedRoute requireMess>
                  <FinancialSummary />
                </ProtectedRoute>
              }
            />
            <Route
              path="/cash-transfers"
              element={
                <ProtectedRoute requireMess>
                  <CashTransfers />
                </ProtectedRoute>
              }
            />
            <Route
              path="/expenses"
              element={
                <ProtectedRoute requireMess requireAdmin>
                  <Expenses />
                </ProtectedRoute>
              }
            />
            <Route
              path="/payments"
              element={
                <ProtectedRoute requireMess requireAdmin>
                  <Payments />
                </ProtectedRoute>
              }
            />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
