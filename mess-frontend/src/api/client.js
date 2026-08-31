import axios from "axios";

// Base URL of the ASP.NET Core backend.
// Change this if your backend runs on a different port/host.
export const API_BASE_URL = "http://localhost:5279/api";

const client = axios.create({
  baseURL: API_BASE_URL,
});

// Attach the saved JWT token (if any) to every request.
client.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// If the token is invalid/expired, the API returns 401.
// Send the user back to the login page in that case.
client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem("token");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  }
);

// Helper to turn API error responses into a readable message.
export function getErrorMessage(error) {
  if (error.response && error.response.data) {
    if (typeof error.response.data === "string") return error.response.data;
    if (error.response.data.message) return error.response.data.message;
  }
  if (error.message) return error.message;
  return "Something went wrong. Please try again.";
}

export default client;
