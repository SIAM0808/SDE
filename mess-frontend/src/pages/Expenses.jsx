import { useEffect, useState, useCallback } from "react";
import { useAuth } from "../context/AuthContext";
import {
  createExpense,
  getExpenses,
  updateExpense,
  deleteExpense,
  getTotalCost,
} from "../api/expenses";
import { Loading, ErrorAlert, SuccessAlert, EmptyState } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

const CATEGORIES = ["Food", "HouseRent", "Chief", "Others"];

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

const emptyForm = { description: "", category: "Food", amount: "", expenseDate: todayIso() };

export default function Expenses() {
  const { member } = useAuth();
  const messId = member.messId;

  const [expenses, setExpenses] = useState([]);
  const [totals, setTotals] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [form, setForm] = useState(emptyForm);
  const [submitting, setSubmitting] = useState(false);

  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState(emptyForm);
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [expensesRes, totalsRes] = await Promise.all([
        getExpenses(messId),
        getTotalCost(messId),
      ]);
      setExpenses(expensesRes.data);
      setTotals(totalsRes.data);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [messId]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleCreate(e) {
    e.preventDefault();
    setError("");
    setSuccess("");

    const amount = Number(form.amount);
    if (!form.description.trim()) {
      setError("Description is required.");
      return;
    }
    if (!amount || amount <= 0) {
      setError("Amount must be greater than zero.");
      return;
    }

    setSubmitting(true);
    try {
      await createExpense(messId, {
        description: form.description.trim(),
        category: form.category,
        amount,
        expenseDate: form.expenseDate,
      });
      setSuccess("Expense added.");
      setForm(emptyForm);
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  function startEdit(expense) {
    setEditingId(expense.id);
    setEditForm({
      description: expense.description,
      category: expense.category,
      amount: expense.amount,
      expenseDate: expense.expenseDate.slice(0, 10),
    });
    setError("");
    setSuccess("");
  }

  async function saveEdit(expenseId) {
    setBusyId(expenseId);
    setError("");
    try {
      await updateExpense(messId, expenseId, {
        description: editForm.description.trim(),
        category: editForm.category,
        amount: Number(editForm.amount),
        expenseDate: editForm.expenseDate,
      });
      setSuccess("Expense updated.");
      setEditingId(null);
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  async function handleDelete(expenseId) {
    if (!window.confirm("Delete this expense?")) return;
    setBusyId(expenseId);
    setError("");
    try {
      await deleteExpense(messId, expenseId);
      setSuccess("Expense deleted.");
      await load();
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
          <h1>Expenses</h1>
          <p>Record and manage shared mess expenses.</p>
        </div>
      </div>

      <ErrorAlert message={error} />
      <SuccessAlert message={success} />

      <div className="card">
        <h2>Add an expense</h2>
        <form onSubmit={handleCreate}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="description">Description</label>
              <input
                id="description"
                type="text"
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                placeholder="e.g. Rice and vegetables"
              />
            </div>
            <div className="field">
              <label htmlFor="category">Category</label>
              <select
                id="category"
                value={form.category}
                onChange={(e) => setForm({ ...form, category: e.target.value })}
              >
                {CATEGORIES.map((c) => (
                  <option key={c} value={c}>
                    {c}
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
                value={form.amount}
                onChange={(e) => setForm({ ...form, amount: e.target.value })}
              />
            </div>
            <div className="field">
              <label htmlFor="expenseDate">Date</label>
              <input
                id="expenseDate"
                type="date"
                value={form.expenseDate}
                onChange={(e) => setForm({ ...form, expenseDate: e.target.value })}
              />
            </div>
          </div>
          <button className="btn" type="submit" disabled={submitting}>
            {submitting ? "Adding..." : "Add expense"}
          </button>
        </form>
      </div>

      {totals && (
        <div className="card">
          <h2>Totals by category</h2>
          <div className="stat-grid">
            <div className="stat">
              <div className="stat-label">Food</div>
              <div className="stat-value">{totals.food.toFixed(2)}</div>
            </div>
            <div className="stat">
              <div className="stat-label">House rent</div>
              <div className="stat-value">{totals.houseRent.toFixed(2)}</div>
            </div>
            <div className="stat">
              <div className="stat-label">Chief</div>
              <div className="stat-value">{totals.chief.toFixed(2)}</div>
            </div>
            <div className="stat">
              <div className="stat-label">Others</div>
              <div className="stat-value">{totals.others.toFixed(2)}</div>
            </div>
            <div className="stat">
              <div className="stat-label">Cash given to members</div>
              <div className="stat-value">{totals.memberCashTransfer.toFixed(2)}</div>
            </div>
            <div className="stat">
              <div className="stat-label">Grand total</div>
              <div className="stat-value">{totals.grandTotal.toFixed(2)}</div>
            </div>
          </div>
        </div>
      )}

      <div className="card">
        <h2>All expenses</h2>
        {loading ? (
          <Loading />
        ) : expenses.length === 0 ? (
          <EmptyState title="No expenses recorded yet" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Category</th>
                  <th>Amount</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {expenses.map((exp) => {
                  const isEditing = editingId === exp.id;
                  return (
                    <tr key={exp.id}>
                      {isEditing ? (
                        <>
                          <td>
                            <input
                              type="date"
                              style={{ width: 150 }}
                              value={editForm.expenseDate}
                              onChange={(e) =>
                                setEditForm({ ...editForm, expenseDate: e.target.value })
                              }
                            />
                          </td>
                          <td>
                            <input
                              type="text"
                              value={editForm.description}
                              onChange={(e) =>
                                setEditForm({ ...editForm, description: e.target.value })
                              }
                            />
                          </td>
                          <td>
                            <select
                              value={editForm.category}
                              onChange={(e) =>
                                setEditForm({ ...editForm, category: e.target.value })
                              }
                            >
                              {CATEGORIES.map((c) => (
                                <option key={c} value={c}>
                                  {c}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td>
                            <input
                              type="number"
                              min="0.01"
                              step="0.01"
                              style={{ width: 90 }}
                              value={editForm.amount}
                              onChange={(e) => setEditForm({ ...editForm, amount: e.target.value })}
                            />
                          </td>
                          <td>
                            <div className="btn-row">
                              <button
                                className="btn btn-sm"
                                disabled={busyId === exp.id}
                                onClick={() => saveEdit(exp.id)}
                              >
                                Save
                              </button>
                              <button
                                className="btn btn-secondary btn-sm"
                                onClick={() => setEditingId(null)}
                              >
                                Cancel
                              </button>
                            </div>
                          </td>
                        </>
                      ) : (
                        <>
                          <td>{new Date(exp.expenseDate).toLocaleDateString()}</td>
                          <td>{exp.description}</td>
                          <td>{exp.category}</td>
                          <td>{exp.amount.toFixed(2)}</td>
                          <td>
                            <div className="btn-row">
                              <button className="btn btn-secondary btn-sm" onClick={() => startEdit(exp)}>
                                Edit
                              </button>
                              <button
                                className="btn btn-danger btn-sm"
                                disabled={busyId === exp.id}
                                onClick={() => handleDelete(exp.id)}
                              >
                                Delete
                              </button>
                            </div>
                          </td>
                        </>
                      )}
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
