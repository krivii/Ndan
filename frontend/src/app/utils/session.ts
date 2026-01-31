// utils/session.ts
import Cookies from 'js-cookie';

export interface EventSession {
  guestId: string;
  eventToken: string;
}

export function getSession(): EventSession | null {
  const cookie = Cookies.get('event_session');
  if (!cookie) return null;

  try {
    const session = JSON.parse(cookie) as EventSession;
    if (!session.guestId || !session.eventToken) return null;
    return session;
  } catch {
    return null;
  }
}
