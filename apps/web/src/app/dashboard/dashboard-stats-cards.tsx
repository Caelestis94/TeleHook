import { StatCard } from "@/components/stat-card";
import { StatCardSkeletons } from "@/components/stat-card-skeletons";
import { useMediaQuery } from "@/hooks/useMediaQuery";
import {
  Activity,
  CheckCircle,
  XCircle,
  Clock,
  Calendar,
  TrendingUp,
  TrendingDown,
} from "lucide-react";
import type { WebhookStatsOverview } from "@/types/stats";

interface DashboardStatsCardsProps {
  stats: WebhookStatsOverview | null;
  isLoading?: boolean;
}

// Helper function to get trend indicator
const getTrendIndicator = (
  current: number,
  previous: number,
  lowerIsBetter = false
) => {
  if (previous === 0) return null;
  const change = ((current - previous) / previous) * 100;
  const absChange = Math.abs(change);

  // Determine if this is an improvement or regression
  const isImprovement = lowerIsBetter ? change < -5 : change > 5;
  const isRegression = lowerIsBetter ? change > 5 : change < -5;

  if (isImprovement) {
    return (
      <div className="flex items-center gap-1 text-green-600">
        {lowerIsBetter ? (
          <TrendingDown className="w-3 h-3" />
        ) : (
          <TrendingUp className="w-3 h-3" />
        )}
        <span className="text-xs">{absChange.toFixed(0)}%</span>
      </div>
    );
  } else if (isRegression) {
    return (
      <div className="flex items-center gap-1 text-red-600">
        {lowerIsBetter ? (
          <TrendingUp className="w-3 h-3" />
        ) : (
          <TrendingDown className="w-3 h-3" />
        )}
        <span className="text-xs">{absChange.toFixed(0)}%</span>
      </div>
    );
  }
  return null;
};

// Helper function to get comparison data
const getComparisonData = (stats: WebhookStatsOverview | null) => {
  if (!stats || stats.dailyTrend.length < 2) return null;

  const recent = stats.dailyTrend.slice(-2); // Last 2 days
  if (recent.length < 2) return null;

  const [previous, current] = recent;
  return {
    requests: { current: current.requests, previous: previous.requests },
    successRate: {
      current: current.successRate,
      previous: previous.successRate,
    },
    avgProcessingTime: {
      current: current.avgProcessingTime,
      previous: previous.avgProcessingTime,
    },
  };
};

export function DashboardStatsCards({
  stats,
  isLoading = false,
}: DashboardStatsCardsProps) {
  const isMobile = useMediaQuery("(max-width: 768px)");
  const isTablet = useMediaQuery("(max-width: 1350px)");

  if (isLoading) {
    return (
      <StatCardSkeletons
        count={5}
        gridClasses={"grid grid-cols-1 md:grid-cols-4 lg:grid-cols-5 gap-4"}
      />
    );
  }

  if (!stats) {
    return null;
  }

  const comparison = getComparisonData(stats);

  const dashboardStats = [
    {
      title: "Total Requests",
      icon: <Activity className="w-5 h-5" />,
      iconColor: "text-blue-600",
      value: stats.summary.totalRequests,
      subtitle: stats.period,
      trend: comparison
        ? getTrendIndicator(
            comparison.requests.current,
            comparison.requests.previous
          )
        : null,
    },
    {
      title: "Success Rate",
      icon: <CheckCircle className="w-5 h-5" />,
      iconColor: "text-green-600",
      value: `${Math.round(stats.summary.successRate)}%`,
      subtitle: `${
        stats.summary.totalRequests - stats.summary.failedRequests
      } successful`,
      trend: comparison
        ? getTrendIndicator(
            comparison.successRate.current,
            comparison.successRate.previous
          )
        : null,
    },
    {
      title: "Failed Requests",
      icon: <XCircle className="w-5 h-5" />,
      iconColor: "text-red-600",
      value: stats.summary.failedRequests,
      subtitle:
        stats.summary.failedRequests > 0 ? "Needs attention" : "All good!",
    },
    {
      title: "Avg Processing Time",
      icon: <Clock className="w-5 h-5" />,
      iconColor: "text-purple-600",
      value: `${Math.round(stats.summary.avgProcessingTime)}ms`,
      subtitle: "Response time",
      trend: comparison
        ? getTrendIndicator(
            comparison.avgProcessingTime.current,
            comparison.avgProcessingTime.previous,
            true
          )
        : null,
    },
    {
      title: "Today",
      icon: <Calendar className="w-5 h-5" />,
      iconColor: "text-orange-600",
      value: stats.summary.todayRequests,
      subtitle: "Requests today",
    },
  ];

  // Dynamic grid classes based on screen size
  const gridClasses = isMobile
    ? "grid-cols-1 gap-3"
    : isTablet
    ? "grid-cols-2 gap-4"
    : "grid-cols-5 gap-4";

  return (
    <div className={`grid mb-4 ${gridClasses}`}>
      {dashboardStats.map((stat) => (
        <StatCard
          key={stat.title}
          title={stat.title}
          icon={stat.icon}
          iconColor={stat.iconColor}
          value={stat.value}
          subtitle={stat.subtitle}
          trend={stat.trend}
        />
      ))}
    </div>
  );
}
