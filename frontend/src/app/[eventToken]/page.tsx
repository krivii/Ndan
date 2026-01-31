'use client';

import { useEffect } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { getSession } from '@/app/utils/session';

const VALID_TOKEN = 'abcd1234'; // or fetch from backend

export default function EventTokenPage() {
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    const session = getSession();
    if (session?.guestId && session.eventToken === VALID_TOKEN) {
      // Session exists → go directly to gallery
      router.replace('/gallery');
      return;
    }

    // Extract token from URL
    const segments = pathname.split('/').filter(Boolean);
    const tokenFromUrl = segments[0];

    if (!tokenFromUrl || tokenFromUrl !== VALID_TOKEN) {
      router.replace('/'); // invalid token → landing page
      return;
    }

    // No session yet → redirect to registration
    router.replace(`/set-name?event=${VALID_TOKEN}`);
  }, [pathname, router]);

  return (
    <main className="min-h-screen flex items-center justify-center bg-gray-50">
      <p className="text-gray-600">Redirecting…</p>
    </main>
  );
}
