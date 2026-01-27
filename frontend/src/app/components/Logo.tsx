import { Camera } from 'lucide-react';

export default function Logo() {
  return (
    <div className="flex items-center gap-3">
      <div className="w-12 h-12 bg-gradient-to-br from-primary to-primary-dark rounded-full flex items-center justify-center shadow-lg">
        <Camera className="w-6 h-6 text-white" />
      </div>
      <div>
        <h1 className="text-2xl font-serif font-bold text-text-dark">
          Memories
        </h1>
        <p className="text-xs text-primary-dark font-sans">
          Share your moments
        </p>
      </div>
    </div>
  );
}