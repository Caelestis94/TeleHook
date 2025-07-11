import { ReactNode } from "react";

interface WebhookConfigLayoutProps {
  children: ReactNode;
}

export default function WebhookConfigLayout({
  children,
}: WebhookConfigLayoutProps) {
  return <div className="container max-w-7xl mx-auto">{children}</div>;
}
