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
    <main className="min-h-screen p-6">
      <h1 className="text-2xl font-bold mb-4">Gallery</h1>
      {/* Render media items here */}
    </main>
  );
}
