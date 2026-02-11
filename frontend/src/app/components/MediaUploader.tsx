'use client';

import { useState, useRef } from 'react';
import { Camera, Upload, X, Check, AlertCircle, Image as ImageIcon, Video } from 'lucide-react';
import Button from './Button';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;
const SUPABASE_URL = process.env.NEXT_PUBLIC_SUPABASE_URL;
const SUPABASE_ANON_KEY = process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY;
const BUCKET_NAME = 'wedding-media';

interface MediaUploaderProps {
  eventId: string;
  guestId: string | null;
  onUploadComplete?: () => void;
}

interface UploadProgress {
  file: File;
  progress: number;
  status: 'pending' | 'uploading' | 'saving' | 'done' | 'error';
  error?: string;
  mediaId?: string;
  preview?: string;
}

export default function MediaUploader({ 
  eventId, 
  guestId,
  onUploadComplete 
}: MediaUploaderProps) {
  const [uploads, setUploads] = useState<UploadProgress[]>([]);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    // Validate files
    const validFiles = files.filter(file => {
      const maxSize = 50 * 1024 * 1024; // 50MB
      const allowedTypes = [
        'image/jpeg',
        'image/png',
        'image/gif',
        'image/webp',
        'video/mp4',
        'video/quicktime',
      ];

      if (file.size > maxSize) {
        alert(`${file.name} is too large (max 50MB)`);
        return false;
      }

      if (!allowedTypes.includes(file.type)) {
        alert(`${file.name} is not a supported file type`);
        return false;
      }

      return true;
    });

    if (validFiles.length === 0) return;

    // Create upload items with previews
    const newUploads: UploadProgress[] = validFiles.map(file => {
      const preview = URL.createObjectURL(file);
      return {
        file,
        progress: 0,
        status: 'pending',
        preview
      };
    });

    setUploads(prev => [...prev, ...newUploads]);
    uploadFiles(newUploads);

    // Reset file input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const uploadFiles = async (filesToUpload: UploadProgress[]) => {
    setIsUploading(true);

    // Upload files in parallel (max 3 at a time)
    const maxConcurrent = 3;
    const queue = [...filesToUpload];
    const inProgress = new Set<Promise<void>>();

    while (queue.length > 0 || inProgress.size > 0) {
      // Start new uploads if below max concurrent
      while (inProgress.size < maxConcurrent && queue.length > 0) {
        const uploadItem = queue.shift()!;
        const uploadPromise = uploadSingleFile(uploadItem).finally(() => {
          inProgress.delete(uploadPromise);
        });
        inProgress.add(uploadPromise);
      }

      // Wait for at least one to complete
      if (inProgress.size > 0) {
        await Promise.race(inProgress);
      }
    }

    setIsUploading(false);
    onUploadComplete?.();
  };

  const uploadSingleFile = async (uploadItem: UploadProgress) => {
    const index = uploads.findIndex(u => u.file === uploadItem.file);

    try {
      // Update status to uploading
      updateUpload(index, { status: 'uploading', progress: 0 });

      // 1. Generate storage key
      const timestamp = Date.now();
      const fileExt = uploadItem.file.name.split('.').pop() || 'jpg';
      const storageKey = `${eventId}/${timestamp}.${fileExt}`;

      // 2. Determine media type
      const mediaType = uploadItem.file.type.startsWith('image/') ? 'image' : 'video';

      // 3. Upload to Supabase Storage
      const formData = new FormData();
      formData.append('file', uploadItem.file);

      const uploadResponse = await fetch(
        `${SUPABASE_URL}/storage/v1/object/${BUCKET_NAME}/${storageKey}`,
        {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${SUPABASE_ANON_KEY}`,
            'apikey': SUPABASE_ANON_KEY!,
          },
          body: uploadItem.file,
        }
      );

      if (!uploadResponse.ok) {
        const errorText = await uploadResponse.text();
        throw new Error(`Upload failed: ${errorText}`);
      }

      updateUpload(index, { progress: 80 });

      // 4. Save metadata to backend
      updateUpload(index, { status: 'saving' });

      const metadataResponse = await fetch(`${API_BASE}/media/metadata`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          eventId,
          guestId,
          storageKey,
          mediaType,
          mimeType: uploadItem.file.type,
          fileSizeBytes: uploadItem.file.size,
        }),
      });

      if (!metadataResponse.ok) {
        // Rollback: delete uploaded file
        await fetch(
          `${SUPABASE_URL}/storage/v1/object/${BUCKET_NAME}/${storageKey}`,
          {
            method: 'DELETE',
            headers: {
              'Authorization': `Bearer ${SUPABASE_ANON_KEY}`,
              'apikey': SUPABASE_ANON_KEY!,
            },
          }
        );
        throw new Error('Failed to save metadata');
      }

      const metadata = await metadataResponse.json();

      // Success
      updateUpload(index, {
        status: 'done',
        progress: 100,
        mediaId: metadata.id
      });

      // Clean up preview after 3 seconds
      setTimeout(() => {
        if (uploadItem.preview) {
          URL.revokeObjectURL(uploadItem.preview);
        }
      }, 3000);

    } catch (error) {
      console.error('Upload error:', error);
      updateUpload(index, {
        status: 'error',
        error: error instanceof Error ? error.message : 'Upload failed'
      });
    }
  };

  const updateUpload = (index: number, updates: Partial<UploadProgress>) => {
    setUploads(prev => prev.map((upload, i) => 
      i === index ? { ...upload, ...updates } : upload
    ));
  };

  const removeUpload = (index: number) => {
    const upload = uploads[index];
    if (upload.preview) {
      URL.revokeObjectURL(upload.preview);
    }
    setUploads(prev => prev.filter((_, i) => i !== index));
  };

  const clearCompleted = () => {
    uploads.forEach(upload => {
      if (upload.status === 'done' && upload.preview) {
        URL.revokeObjectURL(upload.preview);
      }
    });
    setUploads(prev => prev.filter(u => u.status !== 'done'));
  };

  const retryUpload = (index: number) => {
    const upload = uploads[index];
    if (upload.status === 'error') {
      updateUpload(index, { status: 'pending', error: undefined, progress: 0 });
      uploadFiles([upload]);
    }
  };

  return (
    <div className="space-y-4">
      {/* Upload Button */}
      <div className="bg-white rounded-xl p-4 shadow-md border border-primary-light/30">
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept="image/*,video/*"
          onChange={handleFileSelect}
          disabled={isUploading}
          className="hidden"
        />
        <Button
          type="button"
          variant="primary"
          size="lg"
          disabled={isUploading}
          className="w-full"
          onClick={() => fileInputRef.current?.click()}
        >
          <Camera className="w-5 h-5" />
          Upload Photos & Videos
        </Button>
        <p className="text-xs text-primary-dark text-center mt-2">
          Tap to select from your device or camera
        </p>
      </div>

      {/* Upload Progress List */}
      {uploads.length > 0 && (
        <div className="bg-white rounded-xl p-4 shadow-md border border-primary-light/30 space-y-3">
          <div className="flex justify-between items-center">
            <h3 className="text-sm font-medium text-text-dark">
              Uploads ({uploads.filter(u => u.status === 'done').length}/{uploads.length})
            </h3>
            {uploads.some(u => u.status === 'done') && (
              <button
                onClick={clearCompleted}
                className="text-xs text-primary hover:text-primary-dark transition-colors"
              >
                Clear completed
              </button>
            )}
          </div>

          <div className="space-y-2 max-h-96 overflow-y-auto">
            {uploads.map((upload, index) => (
              <div
                key={index}
                className="bg-cream/50 rounded-lg p-3 border border-primary-light/20"
              >
                <div className="flex items-start gap-3">
                  {/* Preview Thumbnail */}
                  <div className="flex-shrink-0 w-16 h-16 rounded-lg overflow-hidden bg-gray-200">
                    {upload.preview && (
                      upload.file.type.startsWith('image/') ? (
                        <img
                          src={upload.preview}
                          alt=""
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center bg-gray-800">
                          <Video className="w-8 h-8 text-white" />
                        </div>
                      )
                    )}
                  </div>

                  {/* Upload Info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-text-dark truncate">
                          {upload.file.name}
                        </p>
                        <p className="text-xs text-primary-dark">
                          {(upload.file.size / 1024 / 1024).toFixed(2)} MB
                        </p>
                      </div>

                      {/* Status Icon */}
                      <div className="flex-shrink-0">
                        {upload.status === 'done' && (
                          <div className="w-6 h-6 bg-green-100 rounded-full flex items-center justify-center">
                            <Check className="w-4 h-4 text-green-600" />
                          </div>
                        )}
                        {upload.status === 'error' && (
                          <div className="w-6 h-6 bg-red-100 rounded-full flex items-center justify-center">
                            <AlertCircle className="w-4 h-4 text-red-600" />
                          </div>
                        )}
                        {(upload.status === 'uploading' || upload.status === 'saving') && (
                          <div className="w-6 h-6 bg-primary/10 rounded-full flex items-center justify-center">
                            <Upload className="w-4 h-4 text-primary animate-pulse" />
                          </div>
                        )}
                      </div>
                    </div>

                    {/* Progress Bar */}
                    {(upload.status === 'uploading' || upload.status === 'saving') && (
                      <div className="mt-2">
                        <div className="flex justify-between text-xs mb-1">
                          <span className="text-primary-dark">
                            {upload.status === 'uploading' ? 'Uploading...' : 'Saving...'}
                          </span>
                          <span className="text-primary-dark font-medium">
                            {Math.round(upload.progress)}%
                          </span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-1.5 overflow-hidden">
                          <div
                            className="bg-primary h-1.5 rounded-full transition-all duration-300 ease-out"
                            style={{ width: `${upload.progress}%` }}
                          />
                        </div>
                      </div>
                    )}

                    {/* Error Message */}
                    {upload.status === 'error' && (
                      <div className="mt-2 space-y-1">
                        <p className="text-xs text-red-600">{upload.error}</p>
                        <button
                          onClick={() => retryUpload(index)}
                          className="text-xs text-primary hover:text-primary-dark underline"
                        >
                          Retry
                        </button>
                      </div>
                    )}
                  </div>

                  {/* Remove Button */}
                  <button
                    onClick={() => removeUpload(index)}
                    disabled={upload.status === 'uploading' || upload.status === 'saving'}
                    className="flex-shrink-0 p-1 hover:bg-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    title="Remove"
                  >
                    <X className="w-4 h-4 text-gray-500" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}