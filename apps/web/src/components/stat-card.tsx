import { ReactNode } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { StatCardSkeletons } from "@/components/stat-card-skeletons";
import { cn } from "@/lib/utils";

export interface StatCardProps {
  /**
   * Main title of the stat card
   */
  title: string;

  /**
   * Grid classes for responsive design
   * @default "grid grid-cols-1 md:grid-cols-4 lg:grid-cols-5 gap-4"
   */
  gridClasses?: string;

  /**
   * Icon to display next to the title
   */
  icon: ReactNode;

  /**
   * Color class for the icon (e.g., "text-blue-600")
   */
  iconColor?: string;

  /**
   * The main value to display
   */
  value: string | number;

  /**
   * Description text that appears below the title in header
   */
  description?: string;

  /**
   * Subtitle text that appears below the value
   */
  subtitle?: string;

  /**
   * Optional trend indicator (e.g., percentage change)
   */
  trend?: ReactNode;

  /**
   * Size of the value text
   */
  valueSize?: "sm" | "md" | "lg" | "xl";

  /**
   * Loading state
   */
  isLoading?: boolean;

  /**
   * Additional className for the card
   */
  className?: string;

  /**
   * Click handler for the card
   */
  onClick?: () => void;
}

const valueSizeClasses = {
  sm: "text-xl",
  md: "text-2xl",
  lg: "text-3xl",
  xl: "text-4xl",
};

export function StatCard({
  title,
  icon,
  iconColor = "text-gray-600",
  value,
  description,
  subtitle,
  trend,
  valueSize = "lg",
  isLoading = false,
  className,
  gridClasses = "grid grid-cols-1 md:grid-cols-4 lg:grid-cols-5 gap-4",
  onClick,
}: StatCardProps) {
  if (isLoading) {
    return (
      <StatCardSkeletons
        count={1}
        gridClasses={gridClasses}
        showTrend={!!trend}
      />
    );
  }

  const formattedValue =
    typeof value === "number" ? value.toLocaleString() : value;

  return (
    <Card
      className={cn(
        "transition-all duration-200",
        onClick && "cursor-pointer hover:shadow-md hover:scale-[1.02]",
        className
      )}
      onClick={onClick}
    >
      <CardHeader className="pb-3">
        <CardTitle className="flex justify-between items-start">
          <div className="flex items-center gap-2">
            <div className={iconColor}>{icon}</div>
            <span className="font-medium">{title}</span>
          </div>
          {trend && <div className="flex-shrink-0">{trend}</div>}
        </CardTitle>
        {description && <CardDescription>{description}</CardDescription>}
      </CardHeader>

      <CardContent className="text-center pt-0">
        <div className={cn("font-bold mt-2", valueSizeClasses[valueSize])}>
          {formattedValue}
        </div>
        {subtitle && (
          <p className="text-xs text-muted-foreground mt-1">{subtitle}</p>
        )}
      </CardContent>
    </Card>
  );
}
