import { createContext, useContext, useEffect, useState, useCallback } from "react";
import { login as loginApi, register as registerApi } from "../api/auth";
import { getCurrentMember } from "../api/members";
import { searchMess } from "../api/mess";
import { getErrorMessage } from "../api/client";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem("token"));
  // member: { memberId, name, email, messId, messName, messCode, isAdmin }
  const [member, setMember] = useState(null);
  const [loading, setLoading] = useState(true);

  // Load the current member's profile whenever we have a token.
  // GET /members/me doesn't tell us whether the member is the mess admin,
  // so we cross-check against /mess/search (which does return adminMemberId).
  const refreshMember = useCallback(async () => {
    if (!localStorage.getItem("token")) {
      setMember(null);
      return;
    }
    try {
      const res = await getCurrentMember();
      const info = res.data;

      let isAdmin = false;
      let messCode = null;

      if (info.messId && info.messName) {
        try {
          const searchRes = await searchMess(info.messName);
          const match = searchRes.data.find((m) => m.id === info.messId);
          if (match) {
            isAdmin = match.adminMemberId === info.memberId;
            messCode = match.messCode;
          }
        } catch {
          // Non-critical: admin status just stays unknown/false.
        }
      }

      setMember({ ...info, isAdmin, messCode });
    } catch {
      setMember(null);
    }
  }, []);

  useEffect(() => {
    (async () => {
      setLoading(true);
      await refreshMember();
      setLoading(false);
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function login(email, password) {
    try {
      const res = await loginApi({ email, password });
      localStorage.setItem("token", res.data.token);
      setToken(res.data.token);
      await refreshMember();
      return { success: true };
    } catch (err) {
      return { success: false, message: getErrorMessage(err) };
    }
  }

  async function register(data) {
    try {
      await registerApi(data);
      return { success: true };
    } catch (err) {
      return { success: false, message: getErrorMessage(err) };
    }
  }

  function logout() {
    localStorage.removeItem("token");
    setToken(null);
    setMember(null);
  }

  const value = {
    token,
    member,
    loading,
    isAuthenticated: !!token,
    isInMess: !!(member && member.messId),
    isAdmin: !!(member && member.isAdmin),
    login,
    register,
    logout,
    refreshMember,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
