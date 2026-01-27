'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Loader2 } from 'lucide-react';
import { generateFingerprint, getStoredGuestId, setStoredGuestId } from '@/app/lib/fingerprint';
import NicknameForm from '@/app/components/NicknameForm';

export default function InviteLinkPage() {
  const params = useParams();
  const router = useRouter();
  const token = params.token as string;
  
  const [loading, setLoading] = useState(true);
  const [eventId, setEventId] = useState<string | null>(null);
  const [eventName, setEventName] = useState<string>('');
  const [needsNickname, setNeedsNickname] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    validateAndCheckGuest();
  }, []);

const validateAndCheckGuest = async () => {
  try {
    // 1. Validate invite token
    const validateRes = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/events/validate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ inviteToken: token }),
    });

    if (!validateRes.ok) {
    throw new Error('Invalid invite link');
    }

    const eventData = await validateRes.json();
    setEventId(eventData.id);
    setEventName(eventData.name);

    // 2. Check if guest already exists
    const fingerprint = generateFingerprint();
    const storedGuestId = getStoredGuestId();

    // Try to find existing guest
    let existingGuestId: string | null = storedGuestId;
    
    if (!existingGuestId) {
    // Check by fingerprint
    const checkRes = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/guests/find?eventId=${eventData.id}&fingerprint=${fingerprint}`
    );
    
    if (checkRes.ok) {
        const guestData = await checkRes.json();
        existingGuestId = guestData.id;
        
        // Only store if we have a valid ID
        if (existingGuestId) {
        setStoredGuestId(existingGuestId);
        }
    }
    }

    if (existingGuestId) {
    // Guest exists - go directly to gallery
    router.push(`/event/${eventData.id}`);
    } else {
    // New guest - need nickname
    setNeedsNickname(true);
    setLoading(false);
    }
} catch (err) {
    setError('Invalid or expired invite link');
    setLoading(false);
}
};

  const handleGuestCreated = (guestId: string) => {
    setStoredGuestId(guestId);
    router.push(`/event/${eventId}`);
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20 flex items-center justify-center">
        <div className="text-center space-y-4">
          <Loader2 className="w-12 h-12 animate-spin text-primary mx-auto" />
          <p className="text-primary-dark">Loading...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20 flex items-center justify-center px-6">
        <div className="text-center space-y-4 max-w-md">
          <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto">
            <span className="text-3xl">❌</span>
          </div>
          <h1 className="text-2xl font-serif font-bold text-text-dark">Invalid Link</h1>
          <p className="text-primary-dark">{error}</p>
        </div>
      </div>
    );
  }

  if (needsNickname) {
    return (
      <NicknameForm 
        eventId={eventId!} 
        eventName={eventName}
        onGuestCreated={handleGuestCreated}
      />
    );
  }

  return null;
}