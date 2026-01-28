'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

type MediaItem = {
  mediaId: string;
  storageUrl: string;
  mediaType: 'Image' | 'Video';
  createdUtc: string;
  likeCount: number;
};

export default function GalleryPage() {
  const router = useRouter();
  const [media, setMedia] = useState<MediaItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Redirect to landing page if not authenticated
  useEffect(() => {
    if (!document.cookie.includes('guest_id=')) {
      router.replace('/landing');
    }cd de
  }, [router]);

  // Fetch media (for now static demo)
  useEffect(() => {
    fetch('/api/media') // Replace with real API later
      .then(res => res.json())
      .then(setMedia)
      .finally(() => setLoading(false));
  }, []);

  return (
    <main className="min-h-screen bg-[var(--dusty-blue-light)] text-[var(--text-dark)] p-2">
      <header className="text-center py-4">
        <h1 className="text-2xl font-semibold">Share Your Memories 💙</h1>
        <p className="text-sm opacity-80">
          Upload photos, videos, and voice messages from today
        </p>
      </header>

      {/* Upload Button */}
      <div className="fixed bottom-4 left-0 right-0 flex justify-center">
        <button className="bg-[var(--rose)] text-white px-6 py-3 rounded-full shadow-lg text-lg">
          Upload
        </button>
      </div>

      {/* Media Grid */}
      {loading && <p className="text-center mt-10">Loading memories…</p>}

      <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-2 pb-24">
        {media.map(item => (
          <div key={item.mediaId} className="bg-white rounded-lg overflow-hidden shadow">
            {item.mediaType === 'Image' ? (
              <img src={item.storageUrl} className="w-full h-40 object-cover" />
            ) : (
              <video src={item.storageUrl} className="w-full h-40 object-cover" controls />
            )}
            <div className="p-1 text-xs flex justify-between">
              <span>❤️ {item.likeCount}</span>
              <span>{new Date(item.createdUtc).toLocaleTimeString()}</span>
            </div>
          </div>
        ))}
      </div>
    </main>
  );
}
