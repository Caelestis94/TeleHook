import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export const convertUTCToLocalDate = (utcDateString: string) => {
  const utcDate = new Date(utcDateString + "Z");
  return utcDate.toLocaleDateString();
};

export const convertUTCToLocalDateTime = (utcDateString: string) => {
  const utcDate = new Date(utcDateString + "Z");
  return utcDate.toLocaleString();
};

export const formatTimeRemaining = (milliseconds: number) => {
  if (milliseconds <= 0) return "Expired";

  const totalSeconds = Math.floor(milliseconds / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
};

export const formatMillisecondsToDateTime = (milliseconds: number) => {
  const date = new Date(milliseconds + "Z");
  return date.toLocaleString();
};

export const parseJsonSafely = (str: string) => {
  try {
    return JSON.parse(str);
  } catch {
    return null;
  }
};
