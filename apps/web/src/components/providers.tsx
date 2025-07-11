"use client";

import { ThemeProvider } from "@/components/theme-provider";
import AuthProvider from "@/components/auth-provider";
import { ErrorProvider } from "@/contexts/error-context";
import { GlobalErrorDialog } from "@/components/global-error-dialog";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@/lib/query-client";

interface ProvidersProps {
  children: React.ReactNode;
}

export function Providers({ children }: ProvidersProps) {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ErrorProvider>
          <ThemeProvider
            attribute="class"
            defaultTheme="system"
            enableSystem
            disableTransitionOnChange
          >
            {children}
            <GlobalErrorDialog />
          </ThemeProvider>
        </ErrorProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}
