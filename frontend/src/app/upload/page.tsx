'use client';

import { useRef, useState } from 'react';

export default function UploadPage() {
  const inputRef = useRef<HTMLInputElement>(null);
  const [files, setFiles] = useState<File[]>([]);

  const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;
    setFiles(Array.from(e.target.files));
  };

  return (
    <main className="min-h-screen p-6 bg-gray-50 flex items-center justify-center">
      <div className="w-full max-w-md bg-white rounded-2xl shadow p-6 text-center">
        <h1 className="text-2xl font-semibold mb-2">
          Upload photos or videos
        </h1>

        <p className="text-gray-600 mb-6">
          Select media files from your device to share with the event.
        </p>

        {/* Hidden input */}
        <input
          ref={inputRef}
          type="file"
          multiple
          accept="image/*,video/*"
          onChange={onFileChange}
          className="hidden"
        />

        {/* Select files button */}
        <button
          onClick={() => inputRef.current?.click()}
          className="w-full bg-blue-600 text-white py-3 rounded-full font-semibold"
        >
          Select files
        </button>

        {files.length > 0 && (
          <>
            <p className="text-sm text-gray-600 mt-4">
              {files.length} file{files.length > 1 ? 's' : ''} selected
            </p>

            <button
              className="mt-4 w-full bg-green-600 text-white py-3 rounded-full font-semibold"
            >
              Upload
            </button>
          </>
        )}
      </div>
    </main>
  );
}
