'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { getSession } from '@/app/utils/session';
import { supabase } from '@/app/lib/supabase';
import { Upload, X, CheckCircle, AlertCircle, Loader2, ArrowLeft, Image as ImageIcon, Video } from 'lucide-react';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;
const BUCKET_NAME = 'NDan-media';
const MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB
const SUPPORTED_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'video/mp4', 'video/quicktime'];

type SelectedFile = {
  file: File;
  preview: string;
  mediaId?: string;
  storageKey?: string;
  thumbnailKey?: string;
  status: 'pending' | 'uploading' | 'generating-thumbnail' | 'success' | 'error';
  progress: number;
  error?: string;
};

export default function UploadPage() {
  const router = useRouter();
  const [files, setFiles] = useState<SelectedFile[]>([]);
  const [globalError, setGlobalError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    if (!mounted) return;

    const session = getSession();
    if (!session) {
      router.replace('/');
    }
  }, [mounted, router]);

  if (!mounted) {
    return null;
  }

  const session = getSession();

  if (!session) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20 flex items-center justify-center">
        <p className="text-primary-dark">Please join the event using the QR code.</p>
      </div>
    );
  }

  const { guestId, eventId } = session;

  // Generate thumbnail for images
  const generateImageThumbnail = (file: File): Promise<Blob> => {
    return new Promise((resolve, reject) => {
      const img = new Image();
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');

      img.onload = () => {
        const MAX_SIZE = 300;
        let width = img.width;
        let height = img.height;

        if (width > height) {
          if (width > MAX_SIZE) {
            height *= MAX_SIZE / width;
            width = MAX_SIZE;
          }
        } else {
          if (height > MAX_SIZE) {
            width *= MAX_SIZE / height;
            height = MAX_SIZE;
          }
        }

        canvas.width = width;
        canvas.height = height;
        ctx?.drawImage(img, 0, 0, width, height);

        canvas.toBlob(
          (blob) => {
            if (blob) {
              resolve(blob);
            } else {
              reject(new Error('Failed to create thumbnail'));
            }
          },
          'image/jpeg',
          0.7
        );
      };

      img.onerror = () => reject(new Error('Failed to load image'));
      img.src = URL.createObjectURL(file);
    });
  };

  // Generate thumbnail for videos
  const generateVideoThumbnail = (file: File): Promise<Blob> => {
    return new Promise((resolve, reject) => {
      const video = document.createElement('video');
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');

      video.preload = 'metadata';
      video.muted = true;
      video.playsInline = true;

      video.onloadedmetadata = () => {
        video.currentTime = Math.min(1, video.duration / 4); // Capture at 1 sec or 25% of video
      };

      video.onseeked = () => {
        const MAX_SIZE = 300;
        let width = video.videoWidth;
        let height = video.videoHeight;

        if (width > height) {
          if (width > MAX_SIZE) {
            height *= MAX_SIZE / width;
            width = MAX_SIZE;
          }
        } else {
          if (height > MAX_SIZE) {
            width *= MAX_SIZE / height;
            height = MAX_SIZE;
          }
        }

        canvas.width = width;
        canvas.height = height;
        ctx?.drawImage(video, 0, 0, width, height);

        canvas.toBlob(
          (blob) => {
            if (blob) {
              resolve(blob);
            } else {
              reject(new Error('Failed to create thumbnail'));
            }
          },
          'image/jpeg',
          0.7
        );

        URL.revokeObjectURL(video.src);
      };

      video.onerror = () => {
        URL.revokeObjectURL(video.src);
        reject(new Error('Failed to load video'));
      };

      video.src = URL.createObjectURL(file);
    });
  };

  const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = Array.from(e.target.files || []);
    const newFiles: SelectedFile[] = selectedFiles.map((file) => {
      let error: string | undefined;
      if (!SUPPORTED_TYPES.includes(file.type)) error = 'Unsupported file type';
      if (file.size > MAX_FILE_SIZE) error = 'File too large (max 50MB)';

      return {
        file,
        preview: URL.createObjectURL(file),
        status: 'pending',
        progress: 0,
        error,
      };
    });
    setFiles((prev) => [...prev, ...newFiles]);
    e.target.value = '';
  };

  const removeFile = (index: number) => {
    setFiles((prev) => {
      const updated = [...prev];
      URL.revokeObjectURL(updated[index].preview);
      updated.splice(index, 1);
      return updated;
    });
  };

  const uploadFile = async (fileObj: SelectedFile, index: number) => {
    try {
      // Update status to uploading
      setFiles((prev) => {
        const updated = [...prev];
        updated[index].status = 'uploading';
        updated[index].progress = 10;
        return updated;
      });

      // 1️⃣ Request upload slot from backend
      const slotRes = await fetch(`${API_BASE}/media/upload-slot`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
        guestId,
        fileName: fileObj.file.name,
        mimeType: fileObj.file.type,          // ← send this
      }),
      });

      if (!slotRes.ok) throw new Error('Failed to get upload slot');
      const slot = await slotRes.json();

      fileObj.mediaId = slot.mediaId;
      fileObj.storageKey = slot.storageKey;
      fileObj.thumbnailKey = slot.thumbnailKey;

      setFiles((prev) => {
        const updated = [...prev];
        updated[index].progress = 30;
        return updated;
      });

      // 2️⃣ Upload main file to Supabase
      const { error: uploadError } = await supabase.storage
        .from(BUCKET_NAME)
        .upload(slot.storageKey, fileObj.file, {
          cacheControl: '3600',
          upsert: false,
        });

      if (uploadError) throw uploadError;

      setFiles((prev) => {
        const updated = [...prev];
        updated[index].progress = 60;
        updated[index].status = 'generating-thumbnail';
        return updated;
      });

      // 3️⃣ Generate and upload thumbnail
      const isVideo = fileObj.file.type.startsWith('video');
      const thumbnailBlob = isVideo
        ? await generateVideoThumbnail(fileObj.file)
        : await generateImageThumbnail(fileObj.file);

      const { error: thumbError } = await supabase.storage
        .from(BUCKET_NAME)
        .upload(slot.thumbnailKey, thumbnailBlob, {
          cacheControl: '3600',
          upsert: false,
          contentType: 'image/jpeg',
        });

      if (thumbError) console.warn('Thumbnail upload failed:', thumbError);

      setFiles((prev) => {
        const updated = [...prev];
        updated[index].progress = 80;
        return updated;
      });

      // 4️⃣ Send metadata to backend
      const metaRes = await fetch(`${API_BASE}/media/metadata`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          eventId,
          guestId,
          fileName: fileObj.file.name,
          storageKey: slot.storageKey,
          thumbnailKey: slot.thumbnailKey,
          fileUrl: slot.storageKey,
          mimeType: fileObj.file.type,
          fileSizeBytes: fileObj.file.size,
          mediaType: isVideo ? 'video' : 'image',
        }),
      });

      if (!metaRes.ok) throw new Error('Failed to save metadata');

      setFiles((prev) => {
        const updated = [...prev];
        updated[index].status = 'success';
        updated[index].progress = 100;
        return updated;
      });
    } catch (err: unknown) {
      setFiles((prev) => {
        const updated = [...prev];
        updated[index].status = 'error';
        updated[index].error = (err as Error).message || 'Upload failed';
        return updated;
      });
    }
  };

  const startUpload = async () => {
    const validFiles = files.filter((f) => f.status === 'pending' && !f.error);
    if (validFiles.length === 0) return;

    setUploading(true);
    setGlobalError(null);

    try {
      for (let i = 0; i < files.length; i++) {
        if (files[i].status === 'pending' && !files[i].error) {
          await uploadFile(files[i], i);
        }
      }
    } catch (err) {
      setGlobalError('Some uploads failed. Please try again.');
    } finally {
      setUploading(false);
    }
  };

  const allSuccess = files.length > 0 && files.every((f) => f.status === 'success');
  const hasErrors = files.some((f) => f.status === 'error');

  return (
    <div className="min-h-screen bg-gradient-to-br from-cream via-soft-white to-primary-light/20">
      {/* Header */}
      <header className="sticky top-0 z-10 bg-white/80 backdrop-blur-sm border-b border-primary-light/30 p-4">
        <div className="max-w-4xl mx-auto flex items-center justify-between">
          <button
            onClick={() => router.push('/gallery')}
            className="flex items-center gap-2 text-primary hover:text-primary-dark transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
            <span className="font-medium">Back to Gallery</span>
          </button>
          <h1 className="text-xl font-serif font-bold text-text-dark">Upload Media</h1>
        </div>
      </header>

      <main className="max-w-4xl mx-auto p-6">
        {/* Upload Area */}
        <div className="bg-white rounded-2xl shadow-lg p-8 mb-6">
          <div className="mb-6">
            <label
              htmlFor="file-upload"
              className="flex flex-col items-center justify-center w-full h-48 border-2 border-dashed border-primary/30 rounded-xl cursor-pointer hover:border-primary/60 hover:bg-primary/5 transition-colors"
            >
              <div className="flex flex-col items-center justify-center pt-5 pb-6">
                <Upload className="w-12 h-12 text-primary mb-3" />
                <p className="mb-2 text-sm text-text-dark font-medium">
                  <span className="font-semibold">Click to upload</span> or drag and drop
                </p>
                <p className="text-xs text-primary-dark">
                  Images (JPEG, PNG, GIF, WebP) or Videos (MP4, MOV) up to 50MB
                </p>
              </div>
              <input
                id="file-upload"
                type="file"
                multiple
                accept={SUPPORTED_TYPES.join(',')}
                onChange={onFileChange}
                className="hidden"
                disabled={uploading}
              />
            </label>
          </div>

          {/* File List */}
          {files.length > 0 && (
            <div className="space-y-3">
              {files.map((fileObj, index) => (
                <div
                  key={index}
                  className="flex items-center gap-4 p-4 border border-primary-light/30 rounded-lg bg-gradient-to-r from-white to-primary-light/5"
                >
                  {/* Preview */}
                  <div className="flex-shrink-0 w-16 h-16 rounded-lg overflow-hidden bg-gray-100">
                    {fileObj.file.type.startsWith('video') ? (
                      <div className="w-full h-full flex items-center justify-center bg-gray-200">
                        <Video className="w-8 h-8 text-gray-500" />
                      </div>
                    ) : (
                      <img
                        src={fileObj.preview}
                        alt=""
                        className="w-full h-full object-cover"
                      />
                    )}
                  </div>

                  {/* File Info */}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-text-dark truncate">
                      {fileObj.file.name}
                    </p>
                    <p className="text-xs text-primary-dark">
                      {(fileObj.file.size / 1024 / 1024).toFixed(2)} MB
                    </p>

                    {/* Progress Bar */}
                    {(fileObj.status === 'uploading' || fileObj.status === 'generating-thumbnail') && (
                      <div className="mt-2">
                        <div className="w-full bg-gray-200 rounded-full h-1.5">
                          <div
                            className="bg-primary h-1.5 rounded-full transition-all duration-300"
                            style={{ width: `${fileObj.progress}%` }}
                          />
                        </div>
                        <p className="text-xs text-primary-dark mt-1">
                          {fileObj.status === 'generating-thumbnail'
                            ? 'Generating thumbnail...'
                            : `Uploading... ${fileObj.progress}%`}
                        </p>
                      </div>
                    )}

                    {/* Error */}
                    {fileObj.error && (
                      <p className="text-xs text-red-600 mt-1">{fileObj.error}</p>
                    )}
                  </div>

                  {/* Status Icon */}
                  <div className="flex-shrink-0">
                    {fileObj.status === 'success' && (
                      <CheckCircle className="w-6 h-6 text-green-600" />
                    )}
                    {fileObj.status === 'error' && (
                      <AlertCircle className="w-6 h-6 text-red-600" />
                    )}
                    {(fileObj.status === 'uploading' || fileObj.status === 'generating-thumbnail') && (
                      <Loader2 className="w-6 h-6 text-primary animate-spin" />
                    )}
                    {fileObj.status === 'pending' && !fileObj.error && !uploading && (
                      <button
                        onClick={() => removeFile(index)}
                        className="p-1 hover:bg-red-50 rounded-full transition-colors"
                      >
                        <X className="w-5 h-5 text-gray-500 hover:text-red-600" />
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Action Buttons */}
          {files.length > 0 && (
            <div className="mt-6 flex gap-3">
              {!allSuccess && (
                <>
                  <button
                    onClick={startUpload}
                    disabled={uploading || files.every((f) => f.status !== 'pending' || f.error)}
                    className="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-dark disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors font-medium shadow-md"
                  >
                    {uploading ? (
                      <>
                        <Loader2 className="w-5 h-5 animate-spin" />
                        <span>Uploading...</span>
                      </>
                    ) : (
                      <>
                        <Upload className="w-5 h-5" />
                        <span>Upload {files.filter((f) => f.status === 'pending' && !f.error).length} Files</span>
                      </>
                    )}
                  </button>
                  <button
                    onClick={() => {
                      files.forEach((f) => URL.revokeObjectURL(f.preview));
                      setFiles([]);
                    }}
                    disabled={uploading}
                    className="px-6 py-3 border border-primary text-primary rounded-lg hover:bg-primary-light/20 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium"
                  >
                    Clear All
                  </button>
                </>
              )}
              {allSuccess && (
                <button
                  onClick={() => router.push('/gallery')}
                  className="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium shadow-md"
                >
                  <CheckCircle className="w-5 h-5" />
                  <span>View in Gallery</span>
                </button>
              )}
            </div>
          )}

          {/* Global Error */}
          {globalError && (
            <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
              <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
              <p className="text-sm text-red-800">{globalError}</p>
            </div>
          )}

          {/* Success Message */}
          {allSuccess && (
            <div className="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg flex items-start gap-2">
              <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
              <p className="text-sm text-green-800">
                All files uploaded successfully! Thumbnails generated.
              </p>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
