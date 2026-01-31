export default function RootPage() {
  return (
    <main className="min-h-screen flex items-center justify-center bg-[var(--dusty-blue-light)] p-6">
      <div className="max-w-md text-center bg-white p-8 rounded-2xl shadow">
        <h1 className="text-2xl font-semibold mb-4">
          Welcome 👋
        </h1>
        <p className="text-gray-600">
          This gallery is private.
        </p>
        <p className="text-gray-600 mt-2">
          Please scan the QR code provided at the event to access it.
        </p>
      </div>
    </main>
  );
}
