import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LucideIcon } from "lucide-react";

interface HelpStep {
  title: string;
  description: string;
}

interface HelpCardProps {
  icon: LucideIcon;
  title: string;
  steps: HelpStep[];
  borderColor?: string;
  iconColor?: string;
  stepIconBg?: string;
  stepIconColor?: string;
}

export function HelpCard({
  icon: Icon,
  title,
  steps,
  borderColor = "border-l-blue-500",
  iconColor = "text-blue-600 dark:text-blue-400",
  stepIconBg = "bg-blue-100 dark:bg-blue-900/20",
  stepIconColor = "text-blue-600 dark:text-blue-400",
}: HelpCardProps) {
  return (
    <Card className={`border-l-4 ${borderColor}`}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Icon className={`w-5 h-5 ${iconColor}`} />
          <span>{title}</span>
        </CardTitle>
        <hr />
      </CardHeader>
      <CardContent>
        <div className="grid gap-4 md:grid-cols-2">
          {steps.map((step, index) => (
            <div key={index}>
              <h4 className="font-medium mb-2 text-sm flex items-center gap-2">
                <span
                  className={`w-6 h-6 rounded-full ${stepIconBg} ${stepIconColor} text-xs flex items-center justify-center font-semibold`}
                >
                  {index + 1}
                </span>
                {step.title}
              </h4>
              <div
                className="text-sm text-muted-foreground"
                dangerouslySetInnerHTML={{ __html: step.description }}
              />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
