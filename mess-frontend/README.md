# Mess Manager — Frontend

A React frontend for the MessManagementSystem ASP.NET Core API.

## Stack

- React + Vite
- Plain CSS (no UI framework)
- react-router-dom for routing
- axios for API calls

## 1. Backend setup (required)

The backend currently has **no CORS policy**, so the browser will block every request
from the React dev server unless you add one. In `Program.cs`, add a CORS policy and
enable it **before** `app.UseAuthentication()`:

```csharp
// After builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ... after var app = builder.Build();
app.UseCors("Frontend"); // must be before UseAuthentication()
app.UseAuthentication();
app.UseAuthorization();
```

Then run the API as usual (`dotnet run`, default `http://localhost:5279`).

## 2. Frontend setup

```bash
npm install
npm run dev
```

The app will run at `http://localhost:5173`.

If your API runs on a different host/port, update `API_BASE_URL` in
`src/api/client.js`.

## Notes on the API surface

- Real registration/login only goes through `/api/auth/register` and `/api/auth/login`.
  The legacy, unauthenticated `/api/members` CRUD endpoints are not used by this
  frontend — accounts created through them can't log in (their password is never
  hashed), and they bypass all mess/admin authorization rules.
- There is no GET endpoint for member payment history, only `POST /api/memberpayment/{messId}`.
  The Payments page therefore only shows payments recorded during the current
  browser session, not a persisted history.
- Whether the logged-in member is a mess admin isn't returned by `/api/members/me`,
  so the frontend infers it by cross-checking `adminMemberId` from
  `GET /api/mess/search`.

## Project structure

```
src/
  api/          axios client + one file per backend resource
  context/      AuthContext (login state, current member, mess membership)
  components/   Layout/sidebar, ProtectedRoute, shared feedback UI
  pages/        one file per screen
```
