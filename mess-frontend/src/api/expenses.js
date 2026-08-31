import client from "./client";

// POST /api/expense/{messId}
export function createExpense(messId, { description, category, amount, expenseDate }) {
  return client.post(`/expense/${messId}`, { description, category, amount, expenseDate });
}

// GET /api/expense/{messId}
export function getExpenses(messId) {
  return client.get(`/expense/${messId}`);
}

// PUT /api/expense/{messId}/{expenseId}
export function updateExpense(messId, expenseId, { description, category, amount, expenseDate }) {
  return client.put(`/expense/${messId}/${expenseId}`, { description, category, amount, expenseDate });
}

// DELETE /api/expense/{messId}/{expenseId}
export function deleteExpense(messId, expenseId) {
  return client.delete(`/expense/${messId}/${expenseId}`);
}

// GET /api/expense/{messId}/total
export function getTotalCost(messId) {
  return client.get(`/expense/${messId}/total`);
}
