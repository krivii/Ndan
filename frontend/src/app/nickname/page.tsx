'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';

export default function NicknamePage() {
  const router = useRouter();
  const [nickname, setNickname] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    if (!nickname) return alert('Enter your nickname');

    setLoading(true);

    try {
      const res = await fetch('/api/set-nickname', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nickname }),
      });

      if (res.ok) {
        router.push('/'); // go to gallery
      } else {
        alert('Failed to set nickname');
      }
    } catch (err) {
      console.error(err);
      alert('Network error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen flex flex-col justify-center items-center bg-[var(--dusty-blue-light)] p-4">
      <div className="max-w-md w-full bg-[var(--dusty-blue)] p-8 rounded-2xl shadow-lg text-center">
        <h1 className="text-2xl font-bold text-[var(--cream)] mb-4">
          Enter Your Nickname
        </h1>
        <input
          type="text"
          value={nickname}
          onChange={(e) => setNickname(e.target.value)}
          placeholder="Your nickname"
          className="w-full px-4 py-3 mb-4 rounded-lg focus:outline-none focus:ring-2 focus:ring-[var(--rose)]"
        />
        <button
          onClick={handleSubmit}
          className="bg-[var(--rose)] text-white px-6 py-3 rounded-full shadow-lg text-lg"
          disabled={loading}
        >
          {loading ? 'Saving...' : 'Proceed to Gallery'}
        </button>
      </div>
    </main>
  );
}
