'use client';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

import { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Cookies from 'js-cookie';

export default function SetNamePage() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const eventToken = searchParams.get('event'); // token from URL query

  const [nickname, setNickname] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Guard: must have event token
  useEffect(() => {
    if (!eventToken) {
      router.replace('/'); // redirect to landing if no token
    }
  }, [eventToken, router]);

  const submit = async () => {
    const name = nickname.trim();

    if (!name) {
      setError('Please enter your name.');
      return;
    }
    if (name.length > 50) {
      setError('Name is too long.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // Call backend API
      const res = await fetch(`${API_BASE_URL}/guests`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nickname: name, eventToken })
      });

      if (!res.ok) {
        const msg = await res.json();
        throw new Error(msg?.message || 'Failed to create guest');
      }

      const data = await res.json();

      // Set session cookie for the guest
      Cookies.set(
        'event_session',
        JSON.stringify({
          guestId: data.guestId,
          eventToken
        }),
        {
          path: '/',
          expires: 30 // 30 days
        }
      );

      // Redirect to main gallery
    router.replace(`/gallery/${eventToken}`);
    } catch (err) {
      console.error(err);
      setError((err as Error).message || 'Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  if (!eventToken) return null; // prevent flicker

  return (
    <main className="min-h-screen flex items-center justify-center bg-[var(--dusty-blue-light)] p-6">
      <div className="w-full max-w-sm bg-white rounded-2xl shadow p-6 text-center">
        <h1 className="text-2xl font-semibold mb-2">What’s your name?</h1>
        <p className="text-gray-600 mb-6">
          This will be shown with your photos and messages.
        </p>

        <input
          type="text"
          value={nickname}
          onChange={(e) => setNickname(e.target.value)}
          placeholder="Your name"
          maxLength={50}
          className="w-full px-4 py-3 rounded-lg border focus:outline-none focus:ring-2 focus:ring-[var(--dusty-blue)]"
        />

        {error && <p className="text-red-600 text-sm mt-3">{error}</p>}

        <button
          onClick={submit}
          disabled={loading}
          className="mt-6 w-full bg-blue-600 text-white py-3 rounded-full font-semibold shadow-md disabled:opacity-60"
        >
          {loading ? 'Saving…' : 'Continue'}
        </button>
      </div>
    </main>
  );
}
