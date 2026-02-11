'use client';

import { useEffect, useState, useCallback, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { getSession } from '@/app/utils/session';
import { Camera, Loader2, AlertCircle, Upload } from 'lucide-react';
import MediaUploader from '@/app/components/MediaUploader';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;
const SUPABASE_URL = process.env.NEXT_PUBLIC_SUPABASE_URL;
const BUCKET_NAME = 'NDan-media';
const PAGE_SIZE = 20;

type MediaListItem = {
  id: string;
  key: string;
  likes: number;
  type?: string;
};

type GalleryItem = {
  id: string;
  src: string;
  mediaType: 'image' | 'video';
  likes: number;
};

export default function GalleryPage() {
  const router = useRouter();
  
  const [allMedia, setAllMedia] = useState<MediaListItem[]>([]);
  const [visible, setVisible] = useState<GalleryItem[]>([]);
  const [page, setPage] = useState(0);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState('');
  const [eventId, setEventId] = useState<string | null>(null);
  const [guestId, setGuestId] = useState<string | null>(null);
  
  const observerTarget = useRef<HTMLDivElement>(null);

  // Validate session and get event ID
  useEffect(() => {
    const session = getSession();
    
    if (!session) {
      console.log('No session - redirecting');
      router.replace('/');
      return;
    }

    console.log('Session found:', session);
    setEventId(session.eventId);
    setGuestId(session.guestId);
    fetchMedia(session.eventId);
  }, [router]);

  // Fetch all media from backend
  const fetchMedia = async (eventId: string) => {
    try {
      setLoading(true);
      console.log('Fetching from:', `${API_BASE}/media/event/${eventId}`);
      
      const res = await fetch(`${API_BASE}/media/event/${eventId}`);
      
      if (!res.ok) {
        throw new Error(`HTTP ${res.status}`);
      }
      
      const data: MediaListItem[] = await res.json();
      console.log('Fetched media:', data);
      
      setAllMedia(data);
      setError('');
    } catch (err) {
      console.error('Error fetching media:', err);
      setError('Failed to load gallery');
    } finally {
      setLoading(false);
    }
  };

  // Convert storage keys to public URLs
  const mapToGallery = (items: MediaListItem[]): GalleryItem[] => {
    return items
      .filter(m => m.key)
      .map(m => ({
        id: m.id,
        src: `${SUPABASE_URL}/storage/v1/object/public/${BUCKET_NAME}/${m.key}`,
        mediaType: (m.type === 'video' || m.key.includes('.mp4') || m.key.includes('.mov')) 
          ? 'video' 
          : 'image',
        likes: m.likes || 0
      }));
  };

  // Load next page
  const loadMore = useCallback(() => {
    if (loadingMore || !hasMore || allMedia.length === 0) return;

    setLoadingMore(true);

    const start = page * PAGE_SIZE;
    const end = start + PAGE_SIZE;
    const chunk = allMedia.slice(start, end);

    if (chunk.length === 0) {
      setHasMore(false);
      setLoadingMore(false);
      return;
    }

    const galleryItems = mapToGallery(chunk);
    setVisible(prev => [...prev, ...galleryItems]);
    setPage(p => p + 1);
    
    if (end >= allMedia.length) {
      setHasMore(false);
    }
    
    setLoadingMore(false);
  }, [allMedia, page, loadingMore, hasMore]);

  // Initial load
  useEffect(() => {
    if (allMedia.length > 0 && visible.length === 0 && !loading) {
      loadMore();
    }
  }, [allMedia, visible.length, loading, loadMore]);

  // Intersection Observer for infinite scroll
  useEffect(() => {
    const observer = new IntersectionObserver(
      entries => {
        if (entries[0].isIntersecting && hasMore && !loadingMore) {
          loadMore();
        }
      },
      { threshold: 0.5 }
    );

    const currentTarget = observerTarget.current;
    if (currentTarget) {
      observer.observe(currentTarget);
    }

    return () => {
      if (currentTarget) {
        observer.unobserve(currentTarget);
      }
    };
  }, [hasMore, loadingMore, loadMore]);

  // Refresh gallery after upload
  const handleUploadComplete = () => {
    if (eventId) {
      setPage(0);
      setVisible([]);
      setHasMore(true);
      fetchMedia(eventId);
    }
  };

  // Navigate to upload page
  const goToUpload = () => {
    router.push('/upload');
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20 flex items-center justify-center">
        <div className="text-center space-y-4">
          <Loader2 className="w-12 h-12 animate-spin text-primary mx-auto" />
          <p className="text-primary-dark">Loading gallery...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20 flex items-center justify-center px-6">
        <div className="text-center space-y-4 max-w-md">
          <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto">
            <AlertCircle className="w-8 h-8 text-red-500" />
          </div>
          <h1 className="text-2xl font-serif font-bold text-text-dark">Error Loading Gallery</h1>
          <p className="text-primary-dark">{error}</p>
          <button
            onClick={() => {
              const session = getSession();
              if (session) {
                setError('');
                fetchMedia(session.eventId);
              }
            }}
            className="text-primary hover:text-primary-dark underline"
          >
            Try again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20">
      {/* Header */}
      <header className="sticky top-0 z-10 bg-white/80 backdrop-blur-sm border-b border-primary-light/30 p-4">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Camera className="w-6 h-6 text-primary" />
            <h1 className="text-xl font-serif font-bold text-text-dark">
              Gallery
            </h1>
          </div>
          <div className="flex items-center gap-4">
            <div className="text-sm text-primary-dark">
              {allMedia.length} {allMedia.length === 1 ? 'photo' : 'photos'}
            </div>
            <button
              onClick={goToUpload}
              className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors shadow-md"
            >
              <Upload className="w-4 h-4" />
              <span className="font-medium">Upload</span>
            </button>
          </div>
        </div>
      </header>

      {/* Gallery Grid */}
      <main className="max-w-7xl mx-auto p-4">
        {visible.length === 0 && !loading ? (
          <div className="text-center py-16 space-y-4">
            <div className="w-20 h-20 bg-primary/10 rounded-full flex items-center justify-center mx-auto">
              <Camera className="w-10 h-10 text-primary" />
            </div>
            <h2 className="text-2xl font-serif font-bold text-text-dark">
              No photos yet
            </h2>
            <p className="text-primary-dark">
              Be the first to share a moment!
            </p>
            <button
              onClick={goToUpload}
              className="mt-4 inline-flex items-center gap-2 px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors shadow-md"
            >
              <Upload className="w-5 h-5" />
              <span className="font-medium">Upload Photos</span>
            </button>
          </div>
        ) : (
          <>
            <div className="grid gap-3 grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
              {visible.map((item) => (
                <div
                  key={item.id}
                  className="aspect-square overflow-hidden rounded-xl bg-white shadow-md hover:shadow-lg transition-shadow duration-200 cursor-pointer group relative"
                >
                  {item.mediaType === 'image' ? (
                    <img
                      src={item.src}
                      alt=""
                      className="h-full w-full object-cover group-hover:scale-105 transition-transform duration-200"
                      loading="lazy"
                      onError={(e) => {
                        console.error('Image failed to load:', item.src);
                        e.currentTarget.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="100" height="100"%3E%3Crect fill="%23ddd" width="100" height="100"/%3E%3Ctext fill="%23999" x="50%25" y="50%25" text-anchor="middle" dy=".3em"%3E❌%3C/text%3E%3C/svg%3E';
                      }}
                    />
                  ) : (
                    <video
                      src={item.src}
                      className="h-full w-full object-cover"
                      preload="metadata"
                      muted
                      playsInline
                      onError={(e) => {
                        console.error('Video failed to load:', item.src);
                      }}
                    />
                  )}
                  
                  {/* Like count overlay */}
                  {item.likes > 0 && (
                    <div className="absolute bottom-2 right-2 bg-black/60 text-white text-xs px-2 py-1 rounded-full flex items-center gap-1">
                      <span>❤️</span>
                      <span>{item.likes}</span>
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Infinite scroll trigger */}
            <div ref={observerTarget} className="h-20 flex items-center justify-center">
              {loadingMore && (
                <div className="flex items-center gap-2 text-primary-dark">
                  <Loader2 className="w-5 h-5 animate-spin" />
                  <span>Loading more...</span>
                </div>
              )}
              {!hasMore && visible.length > 0 && (
                <p className="text-primary-dark text-sm">Youve seen it all! 🎉</p>
              )}
            </div>
          </>
        )}
      </main>
    </div>
  );
}
