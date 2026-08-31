import { NavLink, Outlet } from "react-router-dom";
import { useState } from "react";
import { useAuth } from "../context/AuthContext";

const navLinkClass = ({ isActive }) => "sidebar-link" + (isActive ? " active" : "");

function NavLinks({ onNavigate }) {
  const { isInMess, isAdmin } = useAuth();

  return (
    <nav className="sidebar-nav" onClick={onNavigate}>
      <NavLink to="/" end className={navLinkClass}>
        Dashboard
      </NavLink>

      {!isInMess && (
        <>
          <NavLink to="/mess/create" className={navLinkClass}>
            Create a Mess
          </NavLink>
          <NavLink to="/mess/join" className={navLinkClass}>
            Find & Join a Mess
          </NavLink>
        </>
      )}

      {isInMess && (
        <>
          <NavLink to="/mess" className={navLinkClass}>
            My Mess
          </NavLink>
          <NavLink to="/meals" className={navLinkClass}>
            Meals
          </NavLink>
          <NavLink to="/financial" className={navLinkClass}>
            Financial Summary
          </NavLink>
          <NavLink to="/cash-transfers" className={navLinkClass}>
            Cash Transfers
          </NavLink>

          {isAdmin && (
            <>
              <NavLink to="/expenses" className={navLinkClass}>
                Expenses
              </NavLink>
              <NavLink to="/payments" className={navLinkClass}>
                Member Payments
              </NavLink>
            </>
          )}
        </>
      )}
    </nav>
  );
}

export default function Layout() {
  const { member, logout } = useAuth();
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          Mess Manager
          <span>Shared living, sorted</span>
        </div>
        <NavLinks />
        <div className="sidebar-footer">
          <div className="sidebar-user">
            <strong>{member?.name}</strong>
            {member?.messName || "Not in a mess yet"}
          </div>
          <button className="btn btn-secondary btn-sm" onClick={logout}>
            Log out
          </button>
        </div>
      </aside>

      <div className="main-area">
        <div className="topbar">
          <strong>Mess Manager</strong>
          <button className="btn btn-secondary btn-sm" onClick={() => setMobileOpen(true)}>
            Menu
          </button>
        </div>

        {mobileOpen && (
          <div className="mobile-nav" onClick={() => setMobileOpen(false)}>
            <div className="mobile-nav-panel" onClick={(e) => e.stopPropagation()}>
              <div className="sidebar-brand">Mess Manager</div>
              <NavLinks onNavigate={() => setMobileOpen(false)} />
              <div className="sidebar-footer">
                <div className="sidebar-user">
                  <strong>{member?.name}</strong>
                  {member?.messName || "Not in a mess yet"}
                </div>
                <button className="btn btn-secondary btn-sm" onClick={logout}>
                  Log out
                </button>
              </div>
            </div>
          </div>
        )}

        <Outlet />
      </div>
    </div>
  );
}
