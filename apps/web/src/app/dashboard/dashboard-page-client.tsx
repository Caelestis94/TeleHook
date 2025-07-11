"use client";

import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import {
  BarChart,
  Bar,
  Cell,
  XAxis,
  YAxis,
  Area,
  AreaChart,
  CartesianGrid,
  LabelList,
} from "recharts";
import {
  BarChart3,
  ArrowDownUp,
  BarChart2,
  Clock,
  TrendingUp,
} from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useOverviewStats } from "@/hooks/queries";
import {
  DashboardChartCardSkeleton,
  DashboardStatsCards,
} from "@/app/dashboard/";
import { PageHeader } from "@/components/layout";
import { Separator } from "@/components/ui/separator";
import { handleError } from "@/lib/error-handling";
import { useEffect } from "react";

const chartConfig = {
  requests: {
    label: "Requests",
    color: "var(--color-chart-1)",
  },
  successRate: {
    label: "Success Rate %",
    color: "var(--color-chart-2)",
  },
  avgProcessingTime: {
    label: "Avg Processing (ms)",
    color: "var(--color-chart-3)",
  },
  totalRequests: {
    label: "Total Requests",
    color: "var(--color-chart-4)",
  },
} satisfies ChartConfig;

export function DashboardOverviewClient() {
  const {
    data: stats,
    isLoading,
    refetch,
    isRefetching,
    isError,
    error,
  } = useOverviewStats();

  // Format date for display
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric" });
  };

  useEffect(() => {
    if (isError && error) {
      handleError(error);
    }
  }, [isError, error]);

  const handleRefresh = () => {
    refetch();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader
        title="Overview"
        description="Monitor your webhook performance and activity"
        icon={BarChart2}
        iconColor="text-orange-600 dark:text-orange-400"
        iconBgColor="bg-orange-100 dark:bg-orange-900/20"
        refreshAction={{
          onClick: handleRefresh,
          disabled: isLoading || isRefetching,
        }}
        breadcrumbItems={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Overview" },
        ]}
      />

      {/* Summary Stats Cards */}
      <DashboardStatsCards stats={stats || null} isLoading={isLoading} />
      <Separator className="my-4" />

      {/* Charts Row */}
      {isLoading ? (
        <DashboardChartCardSkeleton count={2} />
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Daily Requests Trend */}
          <Card className="pt-0">
            <CardHeader className="flex items-center gap-2 space-y-0 border-b py-5 sm:flex-row">
              <div className="grid flex-1 gap-1">
                <CardTitle className="flex flex-row items-center gap-1">
                  <BarChart3 className="w-5 h-5"></BarChart3>
                  Daily Requests
                </CardTitle>
                <CardDescription>
                  Showing daily request trends for the last 30 days
                </CardDescription>
              </div>
            </CardHeader>
            <CardContent className="px-2 pt-4 sm:px-6 sm:pt-6">
              {!stats || stats.dailyTrend.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16 px-8 text-center">
                  <div className="relative w-32 h-16 mb-6 opacity-20">
                    <svg viewBox="0 0 100 40" className="w-full h-full">
                      <path
                        d="M0,30 Q15,20 25,25 T50,15 T75,20 T100,10"
                        stroke="#3b82f6"
                        strokeWidth="2"
                        fill="none"
                      />
                      <path
                        d="M0,30 Q15,20 25,25 T50,15 T75,20 T100,10 L100,40 L0,40 Z"
                        fill="url(#gradient)"
                        opacity="0.3"
                      />
                      <defs>
                        <linearGradient
                          id="gradient"
                          x1="0%"
                          y1="0%"
                          x2="0%"
                          y2="100%"
                        >
                          <stop offset="0%" stopColor="#3b82f6" />
                          <stop
                            offset="100%"
                            stopColor="#3b82f6"
                            stopOpacity="0"
                          />
                        </linearGradient>
                      </defs>
                    </svg>
                  </div>
                  <h3>Waiting for your first webhook</h3>
                  <p>
                    Daily request trends will appear here once you start sending
                    webhooks
                  </p>
                </div>
              ) : (
                <ChartContainer
                  config={chartConfig}
                  className="h-[200px] w-full"
                >
                  <AreaChart data={stats.dailyTrend}>
                    <defs>
                      <linearGradient
                        id="fillRequest"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="5%"
                          stopColor="var(--color-chart-1)"
                          stopOpacity={0.8}
                        />
                        <stop
                          offset="95%"
                          stopColor="var(--color-chart-1)"
                          stopOpacity={0.1}
                        />
                      </linearGradient>
                    </defs>
                    <CartesianGrid vertical={false} />
                    <XAxis
                      dataKey="date"
                      tickFormatter={formatDate}
                      tickMargin={8}
                      minTickGap={32}
                      axisLine={false}
                      tickLine={false}
                    />
                    <ChartTooltip
                      content={<ChartTooltipContent indicator="dot" />}
                      labelFormatter={(value) => `Date: ${formatDate(value)}`}
                    />
                    <Area
                      type="natural"
                      dataKey="requests"
                      stroke="var(--color-chart-1)"
                      strokeWidth={2}
                      fill="url(#fillRequest)"
                      stackId="a"
                    />
                  </AreaChart>
                </ChartContainer>
              )}
            </CardContent>
          </Card>

          {/* Success Rate Trend */}
          <Card className="pt-0">
            <CardHeader className="flex items-center gap-2 space-y-0 border-b py-5 sm:flex-row">
              <div className="grid flex-1 gap-1">
                <CardTitle className="flex flex-row items-center gap-1">
                  <TrendingUp className="w-5 h-5" />
                  Success Rate Trend
                </CardTitle>

                <CardDescription>
                  Showing success rate trends for the last 30 days
                </CardDescription>
              </div>
            </CardHeader>
            <CardContent className="px-2 pt-4 sm:px-6 sm:pt-6">
              {!stats || stats.dailyTrend.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16 px-8 text-center">
                  <div className="flex items-end gap-1 mb-6 opacity-20 h-16">
                    {[85, 90, 75, 95, 80, 88, 92, 78, 85, 90].map(
                      (height, i) => (
                        <div
                          key={i}
                          className="w-2 bg-blue-500 rounded-t"
                          style={{ height: `${(height / 100) * 64}px` }}
                        />
                      )
                    )}
                  </div>
                  <h3>Success rate tracking ready</h3>
                  <p>
                    Monitor webhook reliability and response rates over time
                  </p>
                </div>
              ) : (
                <ChartContainer
                  config={chartConfig}
                  className="aspect auto h-[250px] w-full"
                >
                  <BarChart
                    accessibilityLayer
                    data={stats.dailyTrend}
                    margin={{
                      left: 12,
                      right: 12,
                    }}
                  >
                    <CartesianGrid vertical={false} />
                    <XAxis
                      dataKey="date"
                      tickLine={false}
                      tickMargin={8}
                      minTickGap={32}
                      tickFormatter={formatDate}
                      axisLine={false}
                    />
                    <YAxis
                      max={100}
                      tickLine={false}
                      axisLine={false}
                      tickFormatter={(value) => `${value}%`}
                    ></YAxis>

                    <ChartTooltip
                      content={<ChartTooltipContent />}
                      labelFormatter={(value) => `Date: ${formatDate(value)}`}
                      formatter={(value) => [
                        `${Math.round(Number(value))}%`,
                        "Success Rate",
                      ]}
                    />
                    <Bar
                      dataKey="successRate"
                      fill="var(--color-chart-2)"
                      radius={4}
                    ></Bar>
                  </BarChart>
                </ChartContainer>
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {/* Bottom Row */}
      {isLoading ? (
        <DashboardChartCardSkeleton count={2} />
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Top Endpoints */}
          <Card className="pt-0">
            <CardHeader className="flex items-center gap-2 space-y-0 border-b py-5 sm:flex-row">
              <div className="grid flex-1 gap-1">
                <CardTitle className="flex flex-row items-center gap-1">
                  <ArrowDownUp className="w-4 h-4" />
                  Most Active Webhooks
                </CardTitle>

                <CardDescription>
                  Showing the {stats?.topWebhooks.length || 0} most active
                  webhooks in the last 30 days
                </CardDescription>
              </div>
            </CardHeader>
            <CardContent className="px-2 pt-4 sm:px-6 sm:pt-6">
              {!stats || stats.topWebhooks.length === 0 ? (
                <div className="flex flex-col items-center justify-center text-center py-16 px-8">
                  <div className="flex flex-col items-start gap-2 mb-6 opacity-20 h-16">
                    <div className="w-12 h-4 bg-blue-500 rounded"></div>
                    <div className="w-8 h-4 bg-blue-500 rounded"></div>
                    <div className="w-4 h-4 bg-blue-500 rounded"></div>
                  </div>
                  <h3>No endpoints active yet</h3>
                  <p>Your busiest webhook endpoints will be ranked here</p>
                </div>
              ) : (
                <ChartContainer
                  config={chartConfig}
                  className="aspect-auto h-[250px] w-full"
                >
                  <BarChart
                    accessibilityLayer
                    data={stats.topWebhooks}
                    layout="horizontal"
                    margin={{
                      top: 20,
                    }}
                  >
                    <CartesianGrid vertical={false} />
                    <XAxis
                      dataKey="webhookName"
                      tickLine={false}
                      tickMargin={10}
                      axisLine={false}
                    />
                    <ChartTooltip
                      cursor={false}
                      content={
                        <ChartTooltipContent
                          hideLabel
                          hideIndicator
                          formatter={(value) => {
                            return <p>Total Requests: {value}</p>;
                          }}
                        />
                      }
                    />
                    <Bar dataKey="totalRequests" radius={4}>
                      {stats.topWebhooks.map((entry, index) => (
                        <Cell
                          key={`cell-${index}`}
                          fill={`var(--color-chart-${index + 1})`}
                        />
                      ))}
                      <LabelList
                        position="top"
                        offset={12}
                        className="fill-foreground"
                        fontSize={12}
                      />
                    </Bar>
                  </BarChart>
                </ChartContainer>
              )}
            </CardContent>
          </Card>

          {/* Processing Time Trend */}
          <Card className="pt-0">
            <CardHeader className="flex items-center gap-2 space-y-0 border-b py-5 sm:flex-row">
              <div className="grid flex-1 gap-1">
                <CardTitle className="flex flex-row items-center gap-1">
                  <Clock className="w-5 h-5"></Clock>Average Daily Processing
                  Time
                </CardTitle>
                <CardDescription>
                  Showing average processing time per day for the last 30 days
                </CardDescription>
              </div>
            </CardHeader>
            <CardContent className="px-2 pt-4 sm:px-6 sm:pt-6">
              {!stats || stats.dailyTrend.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16 px-8 text-center">
                  <div className="relative w-32 h-16 mb-6 opacity-20">
                    <svg viewBox="0 0 100 40" className="w-full h-full">
                      <path
                        d="M0,25 Q20,22 35,24 T65,18 T85,20 T100,15"
                        stroke="#3b82f6"
                        strokeWidth="2"
                        fill="none"
                      />
                      <path
                        d="M0,25 Q20,22 35,24 T65,18 T85,20 T100,15 L100,40 L0,40 Z"
                        fill="url(#gradient2)"
                        opacity="0.3"
                      />
                    </svg>
                  </div>
                  <h3>Performance insights coming soon</h3>
                  <p>Average response times will be tracked here</p>
                </div>
              ) : (
                <ChartContainer
                  config={chartConfig}
                  className="aspect-auto h-[250px] w-full"
                >
                  <AreaChart data={stats.dailyTrend}>
                    <defs>
                      <linearGradient
                        id="fillProcessingTime"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="5%"
                          stopColor="var(--color-chart-3)"
                          stopOpacity={0.8}
                        />
                        <stop
                          offset="95%"
                          stopColor="var(--color-chart-3)"
                          stopOpacity={0.1}
                        />
                      </linearGradient>
                    </defs>
                    <CartesianGrid vertical={false} />
                    <XAxis
                      dataKey="date"
                      tickFormatter={formatDate}
                      axisLine={false}
                      tickLine={false}
                    />
                    <YAxis
                      axisLine={false}
                      tickLine={false}
                      tickFormatter={(value) => `${value}ms`}
                    />
                    <ChartTooltip
                      content={<ChartTooltipContent />}
                      labelFormatter={(value) => `Date: ${formatDate(value)}`}
                      formatter={(value) => [
                        `${Math.round(Number(value))}ms`,
                        " Avg Processing Time",
                      ]}
                    />
                    <Area
                      type="natural"
                      dataKey="avgProcessingTime"
                      stroke="var(--color-chart-3)"
                      strokeWidth={2}
                      fill="url(#fillProcessingTime)"
                      stackId="a"
                    />
                  </AreaChart>
                </ChartContainer>
              )}
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
