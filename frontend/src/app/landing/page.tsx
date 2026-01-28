'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Cookies from 'js-cookie'; 

export default function LandingPage() {
  const router = useRouter();

  useEffect(() => {
    const cookie = Cookies.get('event_cookie');

    if (cookie) {
      try {
        const parsed = JSON.parse(cookie);
        if (parsed.guestId && parsed.eventToken) {
          // Cookie valid → go to gallery
          router.replace('/gallery');
          return;
        }
      } catch {}
    }

    // No valid cookie → go to set-name
    router.replace('/set-name');
  }, [router]);

  return (
    <main className="min-h-screen flex justify-center items-center bg-[var(--dusty-blue-light)]">
      <p>Checking your session...</p>
    </main>
  );
}
