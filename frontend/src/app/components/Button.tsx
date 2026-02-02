'use client';

import React from 'react';
import { Loader2 } from 'lucide-react';
import { cn } from '../lib/utils';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  children: React.ReactNode;
}

export default function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  children,
  disabled,
  className,
  ...props
}: ButtonProps) {
  const baseStyles =
    'font-medium rounded-full transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed inline-flex items-center justify-center';

  const variants = {
    primary: 'bg-blue-600 hover:bg-blue-700 text-white shadow-md hover:shadow-lg active:scale-95',
    secondary: 'bg-orange-500 hover:bg-orange-600 text-white shadow-md hover:shadow-lg active:scale-95',
    outline: 'border-2 border-blue-600 text-blue-600 hover:bg-blue-600 hover:text-white active:scale-95',
  };

  const sizes = {
    sm: 'px-4 py-2 text-sm gap-1.5',
    md: 'px-6 py-3 text-base gap-2',
    lg: 'px-8 py-4 text-lg gap-2',
  };

  return (
    <button
      disabled={disabled || loading}
      className={cn(baseStyles, variants[variant], sizes[size], className)}
      {...props}
    >
      {loading ? (
        <>
          <Loader2 className="w-4 h-4 animate-spin mr-2" />
          <span>Loading...</span>
        </>
      ) : (
        children
      )}
    </button>
  );
}
