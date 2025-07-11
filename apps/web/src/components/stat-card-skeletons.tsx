import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

interface StatCardSkeletonsProps {
  /**
   * Number of skeleton cards to render
   * @default 4
   */
  count?: number;
  /**
   * Grid layout classes for responsive design
   * @default "grid grid-cols-1 md:grid-cols-4 gap-4"
   */
  gridClasses?: string;
  /**
   * Whether to show the trend indicator skeleton
   * @default false
   */
  showTrend?: boolean;
}

function SingleCardSkeleton({ showTrend }: { showTrend: boolean }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Skeleton className="h-5 w-5" />
            <Skeleton className="h-4 w-20" />
          </div>
          {showTrend && <Skeleton className="h-4 w-10" />}
        </CardTitle>
        <Skeleton className="h-3 w-32" />
      </CardHeader>
      <CardContent className="text-center">
        <Skeleton className="h-8 w-16 mx-auto mb-2" />
        <Skeleton className="h-3 w-24 mx-auto" />
      </CardContent>
    </Card>
  );
}

export function StatCardSkeletons({
  count = 4,
  gridClasses = "grid grid-cols-1 md:grid-cols-4 lg:grid-cols-5 gap-4",
  showTrend = false,
}: StatCardSkeletonsProps) {
  // If count is 1 and no grid classes, return a single card
  if (count === 1 && gridClasses === "") {
    return <SingleCardSkeleton showTrend={showTrend} />;
  }

  return (
    <div className={gridClasses}>
      {Array.from({ length: count }).map((_, index) => (
        <SingleCardSkeleton key={index} showTrend={showTrend} />
      ))}
    </div>
  );
}
