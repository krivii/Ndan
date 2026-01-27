'use client';

import { useState } from 'react';
import { ArrowRight, User } from 'lucide-react';
import Button from './Button';
import { generateFingerprint } from '../lib/fingerprint';

interface NicknameFormProps {
  eventId: string;
  eventName: string;
  onGuestCreated: (guestId: string) => void;
}

export default function NicknameForm({ eventId, eventName, onGuestCreated }: NicknameFormProps) {
  const [nickname, setNickname] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    if (!nickname.trim()) {
      setError('Please enter your name');
      return;
    }

    setLoading(true);

    try {
      const fingerprint = generateFingerprint();
      
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/guests`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          eventId,
          nickname: nickname.trim(),
          fingerprint,
        }),
      });

      if (!response.ok) {
        throw new Error('Failed to register');
      }

      const data = await response.json();
      onGuestCreated(data.id);
    } catch (err) {
      setError('Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20 flex items-center justify-center px-6">
      <div className="w-full max-w-md space-y-8 text-center">
        {/* Welcome Message */}
        <div className="space-y-4">
          <div className="w-20 h-20 bg-primary/10 rounded-full flex items-center justify-center mx-auto">
            <User className="w-10 h-10 text-primary" />
          </div>
          <div>
            <h1 className="text-3xl md:text-4xl font-serif font-bold text-text-dark">
              Welcome!
            </h1>
            <p className="text-lg text-primary-dark mt-2">
              You are joining <span className="font-semibold">{eventName}</span>
            </p>
          </div>
          <p className="text-primary-dark max-w-sm mx-auto">
            What should we call you? Your name will appear when you upload photos.
          </p>
        </div>

        {/* Nickname Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <label htmlFor="nickname" className="block text-sm font-medium text-text-dark">
              Your Name
            </label>
            <input
              id="nickname"
              type="text"
              value={nickname}
              onChange={(e) => setNickname(e.target.value)}
              placeholder="e.g., Sarah"
              maxLength={100}
              className="w-full px-4 py-3 text-center text-lg
                       bg-white border-2 border-primary-light rounded-xl
                       focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent
                       placeholder:text-primary-light/50
                       transition-all duration-200"
              disabled={loading}
              autoFocus
              autoComplete="off"
            />
          </div>

          {error && (
            <p className="text-sm text-red-600">{error}</p>
          )}

          <Button
            type="submit"
            variant="primary"
            size="lg"
            loading={loading}
            disabled={!nickname.trim()}
            className="w-full"
          >
            Continue to Gallery
            <ArrowRight className="w-5 h-5" />
          </Button>
          
          <p className="text-xs text-primary-dark">
            You can skip this, but your uploads will show as -Anonymous-
          </p>
        </form>

        {/* Skip Option */}
        <button
          onClick={() => onGuestCreated('')}
          disabled={loading}
          className="text-sm text-primary-dark hover:text-primary underline"
        >
          Continue without name
        </button>
      </div>
    </div>
  );
}