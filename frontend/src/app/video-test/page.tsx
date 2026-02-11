'use client';

import { useEffect, useState } from 'react';
import { supabase } from '@/app/lib/supabase';

const TEST_VIDEO_KEY = '375b46fc-8287-48cb-b1b0-e76b8c7d706f.mp4'; // replace with your video key in the bucket

export default function TestVideoPage() {
  const [videoUrl, setVideoUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchVideoUrl = async () => {
      try {
        const { data, error } = await supabase.storage
          .from('NDan-media') // your bucket name
          .createSignedUrl(TEST_VIDEO_KEY, 60 * 60); // 1 hour

        if (error || !data?.signedUrl) {
          console.error('Error fetching video URL:', error);
          setError('Failed to load video.');
          return;
        }

        setVideoUrl(data.signedUrl);
      } catch (err) {
        console.error(err);
        setError('Unexpected error.');
      }
    };

    fetchVideoUrl();
  }, []);

  return (
    <main className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <div className="w-full max-w-lg">
        <h1 className="text-2xl font-semibold mb-4">Test Video Playback</h1>

        {error && <p className="text-red-600">{error}</p>}

        {videoUrl ? (
          <video
            src={videoUrl}
            controls
            className="w-full rounded-lg bg-black"
            preload="metadata"
            crossOrigin="anonymous"
          >
            Your browser does not support the video tag.
          </video>
        ) : (
          <p>Loading video…</p>
        )}
      </div>
    </main>
  );
}
