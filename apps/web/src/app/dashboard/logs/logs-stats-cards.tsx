import { StatCardSkeletons } from "@/components/stat-card-skeletons";
import { CheckCircle, Clock, AlertCircle, Calendar } from "lucide-react";
import type { WebhookLog } from "@/types/log";
import { StatCard } from "@/components/stat-card";

interface LogsStatsCardsProps {
  logs: WebhookLog[];
  isLoading?: boolean;
}

export function LogsStatsCards({
  logs,
  isLoading = false,
}: LogsStatsCardsProps) {
  if (isLoading) {
    return <StatCardSkeletons count={4} />;
  }

  const successfulLogs = logs.filter((log) => log.responseStatusCode === 200);
  const failedLogs = logs.filter((log) => log.responseStatusCode !== 200);

  const successRate =
    logs.length > 0
      ? Math.round((successfulLogs.length / logs.length) * 100)
      : 0;

  const avgProcessingTime =
    logs.length > 0
      ? Math.round(
          logs.reduce((sum, log) => sum + log.processingTimeMs, 0) / logs.length
        )
      : 0;

  const stats = [
    {
      title: "Success Rate",
      description: "Across all filtered results",
      value: `${successRate}%`,
      icon: <CheckCircle className="w-5 h-5" />,
      iconColor: "text-green-600",
    },
    {
      title: "Avg Processing",
      description: "Across all filtered results",
      value: `${avgProcessingTime}ms`,
      icon: <Clock className="w-5 h-5" />,
      iconColor: "text-blue-600",
    },
    {
      title: "Failed Requests",
      description: "Total with errors",
      value: failedLogs.length,
      icon: <AlertCircle className="w-5 h-5" />,
      iconColor: "text-yellow-600",
    },
    {
      title: "Total Requests",
      description: "Matching current filters",
      value: logs.length,
      icon: <Calendar className="w-5 h-5" />,
      iconColor: "text-purple-600",
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
      {stats.map((stat) => (
        <StatCard
          key={stat.title}
          title={stat.title}
          description={stat.description}
          value={stat.value}
          icon={stat.icon}
          iconColor={stat.iconColor}
        />
      ))}
    </div>
  );
}
