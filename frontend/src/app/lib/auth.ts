export function isGuestAuthenticated(): boolean {
  if (typeof window === 'undefined') return false;
  // Simulate session cookie check
  return document.cookie.includes('guest_id=');
}
