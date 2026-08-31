import { useEffect, useState, useCallback } from "react";
import { useAuth } from "../context/AuthContext";
import { orderMeal, getMyMeals, updateMeal, deleteMeal, getMyMealTotals } from "../api/meals";
import { Loading, ErrorAlert, SuccessAlert, EmptyState } from "../components/Feedback";
import { getErrorMessage } from "../api/client";

const emptyOrder = { breakfast: 0, lunch: 0, dinner: 0 };

export default function Meals() {
  const { member } = useAuth();
  const [meals, setMeals] = useState([]);
  const [totals, setTotals] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [order, setOrder] = useState(emptyOrder);
  const [submitting, setSubmitting] = useState(false);

  const [editingId, setEditingId] = useState(null);
  const [editValues, setEditValues] = useState(emptyOrder);
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [mealsRes, totalsRes] = await Promise.all([getMyMeals(), getMyMealTotals()]);
      setMeals(mealsRes.data);
      setTotals(totalsRes.data);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  async function handleOrder(e) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (order.breakfast === 0 && order.lunch === 0 && order.dinner === 0) {
      setError("Enter at least one meal to order.");
      return;
    }

    setSubmitting(true);
    try {
      await orderMeal(order);
      setSuccess("Meal order placed.");
      setOrder(emptyOrder);
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  function startEdit(meal) {
    setEditingId(meal.id);
    setEditValues({ breakfast: meal.breakfast, lunch: meal.lunch, dinner: meal.dinner });
    setError("");
    setSuccess("");
  }

  async function saveEdit(mealId) {
    setBusyId(mealId);
    setError("");
    try {
      await updateMeal(mealId, editValues);
      setSuccess("Meal record updated.");
      setEditingId(null);
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  async function handleDelete(mealId) {
    if (!window.confirm("Delete this meal record?")) return;
    setBusyId(mealId);
    setError("");
    try {
      await deleteMeal(mealId);
      setSuccess("Meal record deleted.");
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
          <h1>Meals</h1>
          <p>Order meals and manage your meal records.</p>
        </div>
      </div>

      <ErrorAlert message={error} />
      <SuccessAlert message={success} />

      <div className="card">
        <h2>Order meals</h2>
        <p className="field-hint" style={{ marginTop: -8, marginBottom: 16 }}>
          Breakfast ordered before 5:00 AM counts for today, otherwise it's for tomorrow. Lunch
          must be ordered before 11:00 AM and dinner before 8:00 PM. Maximum 10 per meal type.
        </p>
        <form onSubmit={handleOrder}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="breakfast">Breakfast</label>
              <input
                id="breakfast"
                type="number"
                min="0"
                max="10"
                value={order.breakfast}
                onChange={(e) => setOrder({ ...order, breakfast: Number(e.target.value) })}
              />
            </div>
            <div className="field">
              <label htmlFor="lunch">Lunch</label>
              <input
                id="lunch"
                type="number"
                min="0"
                max="10"
                value={order.lunch}
                onChange={(e) => setOrder({ ...order, lunch: Number(e.target.value) })}
              />
            </div>
            <div className="field">
              <label htmlFor="dinner">Dinner</label>
              <input
                id="dinner"
                type="number"
                min="0"
                max="10"
                value={order.dinner}
                onChange={(e) => setOrder({ ...order, dinner: Number(e.target.value) })}
              />
            </div>
          </div>
          <button className="btn" type="submit" disabled={submitting}>
            {submitting ? "Placing order..." : "Place order"}
          </button>
        </form>
      </div>

      {totals && (
        <div className="card">
          <h2>My meal totals (all time)</h2>
          <div className="stat-grid">
            <div className="stat">
              <div className="stat-label">Breakfast</div>
              <div className="stat-value">{totals.totalBreakfast}</div>
            </div>
            <div className="stat">
              <div className="stat-label">Lunch</div>
              <div className="stat-value">{totals.totalLunch}</div>
            </div>
            <div className="stat">
              <div className="stat-label">Dinner</div>
              <div className="stat-value">{totals.totalDinner}</div>
            </div>
            <div className="stat">
              <div className="stat-label">Grand total</div>
              <div className="stat-value">{totals.grandTotal}</div>
            </div>
          </div>
        </div>
      )}

      <div className="card">
        <h2>My meal history</h2>
        {loading ? (
          <Loading />
        ) : !totals || totals.grandTotal === 0 ? (
          <EmptyState title="No meal records yet" description="Place your first meal order above." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Breakfast(total)</th>
                  <th>Lunch(total)</th>
                  <th>Dinner(Total)</th>
                  <th>Total meal</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>{member.name}</td>
                  <td>{totals.totalBreakfast}</td>
                  <td>{totals.totalLunch}</td>
                  <td>{totals.totalDinner}</td>
                  <td>{totals.grandTotal}</td>
                </tr>
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
