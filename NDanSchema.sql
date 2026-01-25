
CREATE TYPE media_type_enum AS ENUM ('image', 'video');

CREATE TABLE events (
    event_id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(255) NOT NULL,
    invite_token_hash   CHAR(64) NOT NULL UNIQUE,
    start_date_utc       TIMESTAMPTZ,
    end_date_utc         TIMESTAMPTZ,
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    created_utc          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX ix_events_invite_token ON events(invite_token_hash);
CREATE INDEX ix_events_active ON events(is_active);


CREATE TABLE guests (
    guest_id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id         UUID NOT NULL REFERENCES events(event_id) ON DELETE CASCADE,
    nickname         VARCHAR(100),
    created_utc       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);


CREATE UNIQUE INDEX ix_guests_event_token ON guests(event_id, invite_token);
CREATE INDEX ix_guests_event ON guests(event_id);

CREATE TABLE media (
    media_id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id             UUID NOT NULL REFERENCES events(event_id) ON DELETE CASCADE,
    guest_id             UUID REFERENCES guests(guest_id),
    
    media_type           media_type_enum NOT NULL,
    storage_key           VARCHAR(512) NOT NULL,
    thumbnail_key         VARCHAR(512), -- future use
    
    mime_type           VARCHAR(100),
    file_size_bytes         BIGINT,
    duration_seconds         DOUBLE PRECISION, -- only for videos
    
    created_utc             TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    processing_status       SMALLINT NOT NULL DEFAULT 2
    -- 0=uploaded, 1=processing, 2=ready, 3=failed
);

CREATE INDEX ix_media_event_id_created_at ON media(event_id, created_at DESC); 
CREATE INDEX ix_media_event_id_like_count ON media(event_id, like_count DESC); 


CREATE TABLE likes (
    like_id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    media_id        UUID NOT NULL REFERENCES media(media_id) ON DELETE CASCADE,
    guest_id        UUID NOT NULL REFERENCES guests(guest_id) ON DELETE CASCADE,
    created_utc       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);



CREATE UNIQUE INDEX ix_likes_unique ON likes(media_id, guest_id);
CREATE INDEX ix_likes_media_id ON likes(media_id);

