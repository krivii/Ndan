// utils/session.ts
import Cookies from 'js-cookie';

export interface EventSession {
  guestId: string;
  eventToken: string;
  eventId: string;
}

const COOKIE_NAME = 'event_session';

export function getSession(): EventSession | null {
  const cookie = Cookies.get(COOKIE_NAME);
  if (!cookie) return null;

  try {
    const decoded = decodeURIComponent(cookie);
    const session = JSON.parse(decoded) as EventSession;
    if (!session.guestId || !session.eventToken || !session.eventId) return null;
    return session;
  } catch {
    return null;
  }
}

export function setSession(session: EventSession, days = 30) {
  Cookies.set(COOKIE_NAME, JSON.stringify(session), { path: '/', expires: days });
}

export function clearSession() {
  Cookies.remove(COOKIE_NAME, { path: '/' });
}
