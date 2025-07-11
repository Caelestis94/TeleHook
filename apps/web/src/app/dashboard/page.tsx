import { Metadata } from "next";
import { DashboardOverviewClient } from "./dashboard-page-client";

export const metadata: Metadata = {
  title: "Overview | TeleHook",
  description: "Monitor your webhooks status and performance",
};

export default function DashboardOverviewPage() {
  return <DashboardOverviewClient />;
}
