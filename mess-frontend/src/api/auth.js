import client from "./client";

// POST /api/auth/register
export function register({ name, phone, email, password }) {
  return client.post("/auth/register", { name, phone, email, password });
}

// POST /api/auth/login -> { message, token }
export function login({ email, password }) {
  return client.post("/auth/login", { email, password });
}
