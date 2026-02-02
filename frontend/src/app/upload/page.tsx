'use client';

import { useEffect, useRef, useState } from 'react';
import { getSession } from '@/app/utils/session';
import { createClient } from '@supabase/supabase-js';
import Button from '../components/Button';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;
const SUPABASE_URL = process.env.NEXT_PUBLIC_SUPABASE_URL!;
const SUPABASE_KEY = process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY!;
const supabase = createClient(SUPABASE_URL, SUPABASE_KEY);
const MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB
const SUPPORTED_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'video/mp4', 'video/quicktime'];

type SelectedFile = {
  file: File;
  mediaId?: string;
  storageKey?: string;
  thumbnailKey?: string;
  status: 'pending' | 'uploading' | 'success' | 'error';
  progress: number;
  error?: string;
};



export default function UploadPage() {
  const [files, setFiles] = useState<SelectedFile[]>([]);
  const [globalError, setGlobalError] = useState<string | null>(null);

  const [mounted, setMounted] = useState(false);

useEffect(() => {
  setMounted(true);
}, []);

if (!mounted) {
  return null; // Render nothing on server
}

  // Session check (can still early return for UI)
  const session = getSession();

  if (!session) {
    return (
      <main className="min-h-screen flex items-center justify-center bg-gray-50">
        <p className="text-gray-600">
          Please join the event using the QR code.
        </p>
      </main>
    );
  }

    const { guestId, eventToken } = session;

  const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = Array.from(e.target.files || []);
    const newFiles: SelectedFile[] = selectedFiles.map((file) => {
      let error: string | undefined;
      if (!SUPPORTED_TYPES.includes(file.type)) error = 'Unsupported file type';
      if (file.size > MAX_FILE_SIZE) error = 'File too large (max 50MB)';
      return { file, status: 'pending', progress: 0, error };
    });
    setFiles((prev) => [...prev, ...newFiles]);
  };

  const uploadFile = async (fileObj: SelectedFile) => {
    if (!session) {
      fileObj.status = 'error';
      fileObj.error = 'No session. Cannot upload.';
      setFiles([...files]);
      return;
    }

    try {
      fileObj.status = 'uploading';
      setFiles([...files]);

      // 1️⃣ Request upload slot from backend
      const res = await fetch(`${API_BASE}/media/upload-slot`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ guestId: session.guestId, fileName: fileObj.file.name })
      });

      if (!res.ok) throw new Error('Failed to get upload slot');
      const slot = await res.json();

      fileObj.mediaId = slot.mediaId;
      fileObj.storageKey = slot.storageKey;
      fileObj.thumbnailKey = slot.thumbnailKey;

      // 2️⃣ Upload file directly to Supabase
    const { data, error: uploadError } = await supabase.storage
    .from('NDan-media') // your bucket name
    .upload(slot.storageKey!, fileObj.file, {
        cacheControl: '3600',
        upsert: false
    });

    if (uploadError) throw uploadError;

      // 3️⃣ Send metadata to backend
      const metaRes = await fetch(`${API_BASE}/media/metadata`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          mediaId: slot.mediaId,
          eventId: session.eventId,
          guestId: session.guestId,
          mediaType: fileObj.file.type.startsWith('video') ? 'Video' : 'Image',
          storageKey: slot.storageKey,
          thumbnailKey: slot.thumbnailKey,
          mimeType: fileObj.file.type,
          fileSizeBytes: fileObj.file.size
        })
      });

      if (!metaRes.ok) throw new Error('Failed to save metadata');

      fileObj.status = 'success';
      fileObj.progress = 100;
      setFiles([...files]);
    } catch (err: unknown) {
      fileObj.status = 'error';
      fileObj.error = (err as Error).message || 'Upload failed';
      setFiles([...files]);
    }
  };

  const startUpload = () => {
    files
      .filter((f) => f.status === 'pending' && !f.error)
      .forEach((f) => uploadFile(f));
  };

return (
    <main className="min-h-screen p-6 bg-gray-50 flex flex-col items-center">
      <div className="w-full max-w-md bg-white rounded-2xl shadow p-6 flex flex-col items-center">
        <h1 className="text-2xl font-semibold mb-4">Uploaded Media</h1>

        <input
        type="file"
        multiple
        accept={SUPPORTED_TYPES.join(',')}
        onChange={onFileChange}
        className="w-full mb-4 px-4 py-3 rounded-lg border border-gray-300 bg-white cursor-pointer text-gray-700"
        />


        <Button
        onClick={() => {
            console.log('Start Upload button clicked!');
            console.log('Files to upload:', files);
            startUpload(); // call your actual upload function
        }}
        size="md"
        >
        Start Upload
        </Button>

        {files.length > 0 && (
        <ul className="mt-4 w-full space-y-2">
            {files.map((f, i) => (
            <li key={i} className="p-2 border rounded-lg flex justify-between items-center">
                {/* Access name and size from the nested file object */}
                <span>{f.file.name} ({Math.round(f.file.size / 1024)} KB)</span>

                <span>
                {f.status === 'uploading' && <span>Uploading… {f.progress}%</span>}
                {f.status === 'success' && <span className="text-green-600">✓</span>}
                {f.status === 'error' && <span className="text-red-600">✗ {f.error}</span>}
                </span>
            </li>
            ))}
        </ul>
        )}
      </div>
    </main>
  );
}
