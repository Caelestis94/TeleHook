import { Button } from "@/components/ui/button";
import {
  Breadcrumb,
  BreadcrumbList,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbSeparator,
  BreadcrumbPage,
} from "@/components/ui/breadcrumb";
import { LucideIcon, RefreshCw, Plus } from "lucide-react";

interface ActionButton {
  label: string;
  icon?: LucideIcon;
  onClick: () => void;
  variant?:
    | "default"
    | "outline"
    | "destructive"
    | "secondary"
    | "ghost"
    | "link";
  disabled?: boolean;
}

interface BreadcrumbItemProps {
  label: string;
  href?: string;
}

interface PageHeaderProps {
  title: string;
  description: string;
  icon: LucideIcon;
  iconBgColor?: string;
  iconColor?: string;
  actions?: ActionButton[];
  refreshAction?: {
    onClick: () => void;
    disabled?: boolean;
  };
  createAction?: {
    label?: string;
    onClick: () => void;
    disabled?: boolean;
  };
  breadcrumbItems?: BreadcrumbItemProps[];
}

export function PageHeader({
  title,
  description,
  icon: Icon,
  iconBgColor = "bg-blue-100 dark:bg-blue-900/20",
  iconColor = "text-blue-600 dark:text-blue-400",
  actions = [],
  refreshAction,
  createAction,
  breadcrumbItems,
}: PageHeaderProps) {
  return (
    <div className="space-y-4">
      {/* Breadcrumbs */}
      {breadcrumbItems && breadcrumbItems.length > 0 && (
        <Breadcrumb>
          <BreadcrumbList>
            {breadcrumbItems.map((item, index) => {
              const isLast = index === breadcrumbItems.length - 1;
              
              return (
                <div key={index} className="flex items-center">
                  <BreadcrumbItem>
                    {isLast ? (
                      <BreadcrumbPage>{item.label}</BreadcrumbPage>
                    ) : (
                      <BreadcrumbLink href={item.href!}>{item.label}</BreadcrumbLink>
                    )}
                  </BreadcrumbItem>
                  {!isLast && <BreadcrumbSeparator />}
                </div>
              );
            })}
          </BreadcrumbList>
        </Breadcrumb>
      )}

      {/* Main Header */}
      <div className="flex flex-col gap-4 lg:flex-row lg:gap-0 items-start lg:items-center justify-between">
        <div className="flex items-center gap-3">
          <div className={`p-2 rounded-lg ${iconBgColor}`}>
            <Icon className={`w-6 h-6 ${iconColor}`} />
          </div>
          <div>
            <h2 className="text-3xl font-bold tracking-tight">{title}</h2>
            <p className="text-muted-foreground">{description}</p>
          </div>
        </div>
        <div className="flex gap-2">
          {refreshAction && (
            <Button
              variant="outline"
              onClick={refreshAction.onClick}
              disabled={refreshAction.disabled}
            >
              <RefreshCw className="w-4 h-4 mr-2" />
              Refresh
            </Button>
          )}
          {actions.map((action, index) => (
            <Button
              key={index}
              variant={action.variant || "outline"}
              onClick={action.onClick}
              disabled={action.disabled}
            >
              {action.icon && <action.icon className="w-4 h-4 mr-2" />}
              {action.label}
            </Button>
          ))}
          {createAction && (
            <Button disabled={createAction.disabled} onClick={createAction.onClick}>
              <Plus className="w-4 h-4 mr-2" />
              {createAction.label || "Add"}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}