import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

interface DashboardChartCardSkeletonProps {
  /**
   * Number of skeleton chart cards to render
   * @default 4
   */
  count?: number;
  /**
   * Grid layout classes for responsive design
   * @default "grid grid-cols-1 lg:grid-cols-2 gap-6"
   */
  gridClasses?: string;
  /**
   * Height of the chart area
   * @default "h-[250px]"
   */
  chartHeight?: string;
}

function SingleChartCardSkeleton({ chartHeight }: { chartHeight: string }) {
  return (
    <Card className="pt-0">
      <CardHeader className="flex items-center gap-2 space-y-0 border-b py-5 sm:flex-row">
        <div className="grid flex-1 gap-1">
          <CardTitle className="flex flex-row items-center gap-1">
            <Skeleton className="h-5 w-5" />
            <Skeleton className="h-5 w-32" />
          </CardTitle>
          <CardDescription>
            <Skeleton className="h-4 w-64" />
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent className="px-2 pt-4 sm:px-6 sm:pt-6">
        <div
          className={`${chartHeight} w-full flex items-center justify-center`}
        >
          <div className="w-full h-full bg-muted rounded-lg animate-pulse flex items-center justify-center">
            <div className="w-16 h-16 bg-muted-foreground/20 rounded-lg animate-pulse" />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export function DashboardChartCardSkeleton({
  count = 4,
  gridClasses = "grid grid-cols-1 lg:grid-cols-2 gap-6",
  chartHeight = "h-[250px]",
}: DashboardChartCardSkeletonProps) {
  // If count is 1 and no grid classes, return a single card
  if (count === 1 && gridClasses === "") {
    return <SingleChartCardSkeleton chartHeight={chartHeight} />;
  }

  return (
    <div className={gridClasses}>
      {Array.from({ length: count }).map((_, index) => (
        <SingleChartCardSkeleton key={index} chartHeight={chartHeight} />
      ))}
    </div>
  );
}
