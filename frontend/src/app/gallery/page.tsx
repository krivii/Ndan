'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Cookies from 'js-cookie';

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

  useEffect(() => {
    const cookie = Cookies.get('event_cookie');
    if (!cookie) {
      router.replace('/set-name');
      return;
    }

    fetch('/api/media')
      .then(res => res.json())
      .then(setMedia)
      .finally(() => setLoading(false));
  }, [router]);

  return (
    <main className="min-h-screen bg-[var(--dusty-blue-light)] text-[var(--text-dark)] p-2">
      <header className="text-center py-4">
        <h1 className="text-2xl font-semibold">Share Your Memories 💙</h1>
      </header>

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
