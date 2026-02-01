'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { getSession } from '@/app/utils/session';


export default function GalleryPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkSession = () => {
      const session = getSession();

      if (!session) {
        // No valid cookie → redirect to set-name
        router.replace('/set-name');
        return;
      }

      // If session exists → continue loading gallery
      // Wrap setState in a timeout to avoid React sync warning
      setTimeout(() => setLoading(false), 0);
    };

    checkSession();
  }, [router]);

  if (loading) {
    return (
      <main className="min-h-screen flex items-center justify-center bg-gray-50">
        <p className="text-gray-600">Loading gallery…</p>
      </main>
    );
  }

return (
    <div className="min-h-screen bg-red-100">
      <div className="fixed top-0 left-0 right-0 z-50 bg-black p-4 flex justify-between items-center">
        <h1 className="text-white text-lg">Gallery</h1>

        <button
          onClick={() => router.push('/upload')}
          className="bg-yellow-400 text-black px-6 py-3 rounded-lg text-lg font-bold"
        >
          UPLOAD
        </button>
      </div>

      <div className="pt-24 p-6">
        <p>Gallery content placeholder</p>
      </div>
    </div>
  );
}
