'use client';

import { useRef, useState } from 'react';

type SelectedFile = {
  file: File;
  error?: string;
};

const MAX_FILE_SIZE_MB = 50;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;

const ALLOWED_TYPES = {
  image: ['image/jpeg', 'image/png', 'image/webp', 'image/heic'],
  video: ['video/mp4', 'video/webm', 'video/quicktime']
};

export default function UploadPage() {
  const inputRef = useRef<HTMLInputElement>(null);
  const [files, setFiles] = useState<SelectedFile[]>([]);

  const validateFile = (file: File): string | null => {
    if (file.size > MAX_FILE_SIZE_BYTES) {
      return `File exceeds ${MAX_FILE_SIZE_MB} MB limit`;
    }

    const isImage = ALLOWED_TYPES.image.includes(file.type);
    const isVideo = ALLOWED_TYPES.video.includes(file.type);

    if (!isImage && !isVideo) {
      return 'Unsupported file type';
    }

    return null;
  };

const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
  if (!e.target.files) return;

  const selected: SelectedFile[] = Array.from(e.target.files).map(file => ({
    file,
    error: validateFile(file) || undefined,
  }));

  setFiles(selected);
};


  const formatSize = (bytes: number) => {
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  const getIcon = (file: File) => {
    if (file.type.startsWith('image/')) return '🖼️';
    if (file.type.startsWith('video/')) return '🎥';
    return '📄';
  };

  return (
    <main className="min-h-screen p-6 bg-gray-50 flex justify-center">
      <div className="w-full max-w-md bg-white rounded-2xl shadow p-6">
        <h1 className="text-2xl font-semibold mb-2 text-center">
          Upload media
        </h1>

        <p className="text-gray-600 mb-6 text-center">
          Photos and videos from your device
        </p>

        {/* Hidden file input */}
        <input
          ref={inputRef}
          type="file"
          multiple
          accept="image/*,video/*"
          onChange={onFileChange}
          className="hidden"
        />

        {/* Select files */}
        <button
          onClick={() => inputRef.current?.click()}
          className="w-full bg-blue-600 text-white py-3 rounded-full font-semibold"
        >
          Select files
        </button>

        {/* File list */}
        {files.length > 0 && (
          <div className="mt-6 space-y-3">
            {files.map(({ file, error }, idx) => (
              <div
                key={idx}
                className={`flex items-center justify-between rounded-lg border p-3 ${
                  error ? 'border-red-300 bg-red-50' : 'border-gray-200'
                }`}
              >
                <div className="flex items-center gap-3">
                  <span className="text-xl">{getIcon(file)}</span>

                  <div>
                    <p className="text-sm font-medium truncate max-w-[180px]">
                      {file.name}
                    </p>
                    <p className="text-xs text-gray-500">
                      {formatSize(file.size)}
                    </p>
                  </div>
                </div>

                {error && (
                  <span className="text-xs text-red-600 font-medium">
                    {error}
                  </span>
                )}
              </div>
            ))}
          </div>
        )}

        {/* Upload button (disabled until valid files exist) */}
        {files.some(f => !f.error) && (
          <button
            className="mt-6 w-full bg-green-600 text-white py-3 rounded-full font-semibold"
          >
            Upload
          </button>
        )}

        <p className="mt-4 text-xs text-gray-500 text-center">
          Max file size: {MAX_FILE_SIZE_MB} MB · Images & videos only
        </p>
      </div>
    </main>
  );
}
