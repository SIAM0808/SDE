import client from "./client";

// POST /api/meal
export function orderMeal({ breakfast, lunch, dinner }) {
  return client.post("/meal", { breakfast, lunch, dinner });
}

// GET /api/meal
export function getMyMeals() {
  return client.get("/meal");
}

// PUT /api/meal/{mealId}
export function updateMeal(mealId, { breakfast, lunch, dinner }) {
  return client.put(`/meal/${mealId}`, { breakfast, lunch, dinner });
}

// DELETE /api/meal/{mealId}
export function deleteMeal(mealId) {
  return client.delete(`/meal/${mealId}`);
}

// GET /api/meal/my-totals
export function getMyMealTotals() {
  return client.get("/meal/my-totals");
}

// GET /api/meal/mess-totals/{messId}
export function getMessMealTotals(messId) {
  return client.get(`/meal/mess-totals/${messId}`);
}
