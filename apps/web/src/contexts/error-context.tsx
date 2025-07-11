"use client";
import {
  createContext,
  useContext,
  useState,
  ReactNode,
  useEffect,
} from "react";
import { setApiKeyErrorCallback } from "@/lib/error-handling";

interface ErrorContextType {
  isApiKeyErrorVisible: boolean;
  hideApiKeyError: () => void;
}

const ErrorContext = createContext<ErrorContextType | undefined>(undefined);

export function ErrorProvider({ children }: { children: ReactNode }) {
  const [isApiKeyErrorVisible, setIsApiKeyErrorVisible] = useState(false);

  const showApiKeyError = () => {
    setIsApiKeyErrorVisible(true);
  };
  const hideApiKeyError = () => setIsApiKeyErrorVisible(false);

  useEffect(() => {
    setApiKeyErrorCallback(showApiKeyError);
  }, []);

  useEffect(() => {}, [isApiKeyErrorVisible]);

  return (
    <ErrorContext.Provider value={{ isApiKeyErrorVisible, hideApiKeyError }}>
      {children}
    </ErrorContext.Provider>
  );
}

export function useError() {
  const context = useContext(ErrorContext);
  if (!context) {
    throw new Error("useError must be used within ErrorProvider");
  }
  return context;
}
