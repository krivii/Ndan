import { NextRequest, NextResponse } from 'next/server';
import { v4 as uuidv4 } from 'uuid';

// Mock DB for now
const EVENT_TOKEN = 'wedding2026';

export async function POST(req: NextRequest) {
  const { nickname } = await req.json();

  if (!nickname) return NextResponse.json({ error: 'Missing nickname' }, { status: 400 });

  // Simulate creating a guest
  const guestId = uuidv4();

  // TODO: replace with real DB insertion:
  // Guests.Add({ GuestId: guestId, EventToken: EVENT_TOKEN, Nickname: nickname })

  return NextResponse.json({
    guestId,
    eventToken: EVENT_TOKEN
  });
}
