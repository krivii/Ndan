'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { ArrowRight, AlertCircle } from 'lucide-react';
import Button from './Button';

export default function InviteForm() {
  const router = useRouter();
  const [token, setToken] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const formatToken = (value: string) => {
    // Remove all non-alphanumeric characters
    const cleaned = value.replace(/[^A-Z0-9]/gi, '').toUpperCase();
    
    // Add dashes every 4 characters
    const parts = [];
    for (let i = 0; i < cleaned.length && i < 12; i += 4) {
      parts.push(cleaned.substring(i, i + 4));
    }
    
    return parts.join('-');
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatToken(e.target.value);
    setToken(formatted);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    const cleanToken = token.replace(/-/g, '');
    if (cleanToken.length !== 12) {
      setError('Please enter a complete invite code');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/events/validate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ inviteToken: token }),
      });

      if (!response.ok) {
        throw new Error('Invalid invite code');
      }

      const data = await response.json();
      
      // Store event info
      if (typeof window !== 'undefined') {
        sessionStorage.setItem('eventId', data.id);
        sessionStorage.setItem('eventName', data.name);
      }
      
      // Redirect to event page
      router.push(`/event/${data.id}`);
    } catch (err) {
      setError('Invalid invite code. Please check and try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="w-full max-w-md space-y-4">
      <div className="space-y-2">
        <label 
          htmlFor="invite-token" 
          className="block text-sm font-medium text-text-dark"
        >
          Enter your invite code
        </label>
        <input
          id="invite-token"
          type="text"
          value={token}
          onChange={handleChange}
          placeholder="XXXX-XXXX-XXXX"
          maxLength={14}
          className="w-full px-4 py-3 text-center text-lg font-mono tracking-wider
                     bg-white border-2 border-primary-light rounded-xl
                     focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent
                     placeholder:text-primary-light/50 placeholder:font-sans
                     transition-all duration-200"
          disabled={loading}
          autoComplete="off"
          autoCapitalize="characters"
        />
        <p className="text-xs text-primary-dark text-center">
          Ask the host for your invite code
        </p>
      </div>

      {error && (
        <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded-lg">
          <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0" />
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      <Button
        type="submit"
        variant="primary"
        size="lg"
        loading={loading}
        disabled={token.replace(/-/g, '').length !== 12}
        className="w-full"
      >
        Continue
        <ArrowRight className="w-5 h-5" />
      </Button>
    </form>
  );
}