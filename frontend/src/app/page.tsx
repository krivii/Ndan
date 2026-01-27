import { Camera, QrCode } from 'lucide-react';
import Logo from './components/Logo';
import InviteForm from './components/InviteForm';

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20">
      <div className="min-h-screen flex flex-col">
        <header className="p-6 md:p-8">
          <Logo />
        </header>

        <main className="flex-1 flex items-center justify-center px-6 pb-20">
          <div className="w-full max-w-lg space-y-8 text-center">
            <div className="space-y-4">
              <div className="w-20 h-20 bg-primary/10 rounded-full flex items-center justify-center mx-auto">
                <QrCode className="w-10 h-10 text-primary" />
              </div>
              <h2 className="text-4xl md:text-5xl font-serif font-bold text-text-dark">
                Join the Celebration
              </h2>
              <p className="text-lg text-primary-dark max-w-md mx-auto">
                Scan the QR code or enter your invite code to share photos and memories.
              </p>
            </div>

            <InviteForm />
            
            <div className="pt-8 border-t border-primary-light/30">
              <p className="text-sm text-primary-dark">
                Don't have a code? Ask the event host!
              </p>
            </div>
          </div>
        </main>

        <footer className="p-6 text-center text-sm text-primary-dark">
          <p>Made with love for your special day 💙</p>
        </footer>
      </div>
    </div>
  );
}