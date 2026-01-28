export function generateFingerprint(): string {
  if (typeof window === 'undefined') return '';
  
  const components = [
    navigator.userAgent,
    navigator.language,
    screen.width + 'x' + screen.height,
    new Date().getTimezoneOffset(),
    navigator.hardwareConcurrency || '',
  ];
  
  const fingerprint = components.join('|');
  
  // Simple hash function
  let hash = 0;
  for (let i = 0; i < fingerprint.length; i++) {
    const char = fingerprint.charCodeAt(i);
    hash = ((hash << 5) - hash) + char;
    hash = hash & hash; // Convert to 32bit integer
  }
  
  return Math.abs(hash).toString(36);
}

export function getStoredGuestId(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('guestId');
}

export function setStoredGuestId(guestId: string): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem('guestId', guestId);
}

export function clearStoredGuestId(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem('guestId');
}