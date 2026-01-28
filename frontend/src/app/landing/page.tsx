'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

export default function LandingPage() {
  const router = useRouter();

  useEffect(() => {
    // If user is already authenticated, redirect to gallery
    if (document.cookie.includes('guest_id=')) {
      router.replace('/');
    }
  }, [router]);

  return (
    <main className="min-h-screen flex flex-col justify-center items-center bg-[var(--dusty-blue-light)] p-4">
      <div className="max-w-md w-full bg-[var(--dusty-blue)] p-8 rounded-2xl shadow-lg text-center">
        <h1 className="text-3xl font-bold text-[var(--cream)] mb-4">
          Welcome to the Wedding Memories
        </h1>
        <p className="text-[var(--dusty-blue-light)] mb-6">
          It looks like you’re not registered yet. Please scan your invitation QR code to join the gallery.
        </p>
      </div>
    </main>
  );
}
