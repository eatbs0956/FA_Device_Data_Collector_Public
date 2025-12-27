import { localStg } from '@/utils/storage';

/** Get token */
export function getToken() {
  return localStg.get('token') || '';
}

/** Clear auth storage */
export function clearAuthStorage() {
  localStg.remove('token');
  localStg.remove('refreshToken');
  localStg.remove('tokenExpireTime');
}

/**
 * Calculate token expiry time (next midnight, minimum 2 hours)
 * @returns ISO 8601 timestamp string
 */
export function calculateTokenExpiry(): string {
  const now = new Date();
  const nextMidnight = new Date(now);
  nextMidnight.setHours(24, 0, 0, 0); // Next midnight

  const hoursUntilMidnight = (nextMidnight.getTime() - now.getTime()) / (1000 * 60 * 60);

  // If less than 2 hours until midnight, use 2 hours from now
  if (hoursUntilMidnight < 2) {
    const twoHoursLater = new Date(now.getTime() + 2 * 60 * 60 * 1000);
    return twoHoursLater.toISOString();
  }

  return nextMidnight.toISOString();
}

/**
 * Check if token is expired
 * @returns true if expired, false otherwise
 */
export function isTokenExpired(): boolean {
  const expireTime = localStg.get('tokenExpireTime');
  if (!expireTime) {
    return true;
  }

  const expireDate = new Date(expireTime);
  const now = new Date();

  return now >= expireDate;
}
