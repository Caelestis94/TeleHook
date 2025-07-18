import { DateFormatPattern, HelpData } from ".";

export const helpData: HelpData = {
  basics: [
    {
      title: "Variables",
      description: "Basic variable output and fallbacks",
      examples: [
        { code: "{{ variable_name }}", description: "Output a variable" },
        {
          code: '{{ variable ?? "default" }}',
          description: "Variable with fallback",
        },
      ],
    },
    {
      title: "Conditionals",
      description: "If/else statements",
      examples: [
        {
          code: `{{ if condition }}
  True content
{{ else }}
  False content
{{ end }}`,
          description: "If/else statement",
        },
      ],
    },
    {
      title: "Loops",
      description: "Iterate through arrays and collections",
      examples: [
        {
          code: `{{ for item in array }}
  {{ item.property }}
{{ end }}`,
          description: "Loop through array",
        },
        {
          code: `{{ for item, index in array }}
  {{ index }}: {{ item }}
{{ end }}`,
          description: "Loop with index",
        },
      ],
    },
    {
      title: "Object Properties",
      description: "Access object properties and nested data",
      examples: [
        { code: "{{ user.name }}", description: "Access object property" },
        { code: '{{ user["name"] }}', description: "Access with brackets" },
      ],
    },
    {
      title: "Array Access",
      description: "Access array elements by index",
      examples: [
        { code: "{{ items[0] }}", description: "First array element" },
        {
          code: "{{ items[0].title }}",
          description: "Property of array element",
        },
      ],
    },
  ],
  strings: [
    {
      title: "Case Conversion",
      description: "Convert text case",
      examples: [
        { code: '{{ "hello" | string.upcase }}', description: "HELLO" },
        { code: '{{ "HELLO" | string.downcase }}', description: "hello" },
        {
          code: '{{ "hello world" | string.capitalizewords }}',
          description: "Hello World",
        },
      ],
    },
    {
      title: "Text Manipulation",
      description: "Modify and format text",
      examples: [
        {
          code: "{{ text | string.truncate 50 }}",
          description: "Truncate to 50 characters",
        },
        {
          code: '{{ "old text" | string.replace "old" "new" }}',
          description: "Replace text",
        },
        {
          code: '{{ " text " | string.strip }}',
          description: "Remove whitespace",
        },
      ],
    },
    {
      title: "String Testing",
      description: "Test string contents and properties",
      examples: [
        {
          code: '{{ text | string.contains "search" }}',
          description: "Check if contains",
        },
        {
          code: '{{ text | string.starts_with "prefix" }}',
          description: "Check if starts with",
        },
        { code: "{{ text | string.size }}", description: "Get string length" },
      ],
    },
    {
      title: "Splitting & Joining",
      description: "Split strings into arrays and join arrays",
      examples: [
        {
          code: '{{ "a,b,c" | string.split "," }}',
          description: "Split into array",
        },
        {
          code: '{{ text | string.append " suffix" }}',
          description: "Add suffix",
        },
      ],
    },
  ],
  dates: [
    {
      title: "Current Date",
      description: "Access current date and time",
      examples: [
        { code: "{{ date.now }}", description: "Current date/time" },
        { code: "{{ date.now.year }}", description: "Current year" },
        { code: "{{ date.now.month }}", description: "Current month" },
      ],
    },
    {
      title: "Date Formatting",
      description: "Format dates for display",
      examples: [
        {
          code: '{{ date.now | date.to_string "%Y-%m-%d" }}',
          description: "2025-06-27",
        },
        {
          code: '{{ date.now | date.to_string "%b %d, %Y" }}',
          description: "Jun 27, 2025",
        },
        {
          code: '{{ date.now | date.to_string "%A, %B %d" }}',
          description: "Thursday, June 27",
        },
      ],
    },
    {
      title: "Date Arithmetic",
      description: "Add or subtract time from dates",
      examples: [
        { code: "{{ date.now | date.add_days 7 }}", description: "Add 7 days" },
        {
          code: "{{ date.now | date.add_months -1 }}",
          description: "Subtract 1 month",
        },
        {
          code: "{{ date.now | date.add_hours 5 }}",
          description: "Add 5 hours",
        },
      ],
    },
  ],
  arrays: [
    {
      title: "Array Info",
      description: "Get information about arrays",
      examples: [
        { code: "{{ items | array.size }}", description: "Number of items" },
        { code: "{{ items | array.first }}", description: "First element" },
        { code: "{{ items | array.last }}", description: "Last element" },
      ],
    },
    {
      title: "Array Processing",
      description: "Transform and sort arrays",
      examples: [
        { code: "{{ items | array.reverse }}", description: "Reverse order" },
        { code: "{{ items | array.sort }}", description: "Sort elements" },
        { code: "{{ items | array.uniq }}", description: "Remove duplicates" },
      ],
    },
    {
      title: "Array Manipulation",
      description: "Slice and combine arrays",
      examples: [
        {
          code: "{{ items | array.limit 5 }}",
          description: "Take first 5 items",
        },
        {
          code: "{{ items | array.offset 2 }}",
          description: "Skip first 2 items",
        },
        {
          code: '{{ items | array.join ", " }}',
          description: "Join with commas",
        },
      ],
    },
    {
      title: "Object Arrays",
      description: "Work with arrays of objects",
      examples: [
        {
          code: '{{ users | array.map "name" }}',
          description: "Extract property from objects",
        },
        {
          code: '{{ products | array.sort "price" }}',
          description: "Sort by property",
        },
      ],
    },
  ],
  examples: [
    {
      title: "User Notification",
      description: "User registration notification",
      examples: [
        {
          code: `🎉 Welcome {{ user.name | string.capitalizewords }}!
Your account {{ user.email | string.downcase }} has been activated.
Joined on {{ user.created_at | date.to_string "%B %d, %Y" }}`,
          description: "User registration notification",
        },
      ],
    },
    {
      title: "Status Update",
      description: "Build/deployment status",
      examples: [
        {
          code: `{{ if status == "success" }}✅{{ else }}❌{{ end }} {{ project_name | string.upcase }}
{{ description | string.truncate 100 }}
{{ if errors | array.size > 0 }}
Errors: {{ errors | array.join ", " }}
{{ end }}`,
          description: "Build/deployment status",
        },
      ],
    },
    {
      title: "List Processing",
      description: "Activity feed with limit",
      examples: [
        {
          code: `📋 Recent Activities ({{ activities | array.size }} total):
{{ for activity in activities | array.limit 5 }}
• {{ activity.type | string.capitalizewords }}: {{ activity.description }}
  {{ activity.timestamp | date.to_string "%m/%d %H:%M" }}
{{ end }}`,
          description: "Activity feed with limit",
        },
      ],
    },
    {
      title: "Conditional Content",
      description: "Conditional list display",
      examples: [
        {
          code: `{{ if comments | array.size > 0 }}
💬 Comments:
{{ for comment in comments }}
{{ comment.author }}: {{ comment.text | string.truncate 50 }}
{{ end }}
{{ else }}
No comments yet.
{{ end }}`,
          description: "Conditional list display",
        },
      ],
    },
    {
      title: "Data Summary",
      description: "Analytics summary with filtering",
      examples: [
        {
          code: `📊 Report Summary:
Total Users: {{ users | array.size }}
Active Users: {{ users | array.filter @user.active | array.size }}
Top Countries: {{ users | array.map "country" | array.uniq | array.limit 3 | array.join ", " }}
Generated: {{ date.now | date.to_string "%Y-%m-%d %H:%M" }}`,
          description: "Analytics summary with filtering",
        },
      ],
    },
    {
      title: "Order Confirmation",
      description: "E-commerce order summary",
      examples: [
        {
          code: `🛍️ Order #{{ order.id }} Confirmed!
Hello {{ customer.name }}, your order for {{ order.items | array.size }} items, totaling $ {{ order.total | math.format "N2" }}, has been successfully placed.
Expected delivery: {{ order.delivery_date | date.to_string "%A, %B %d" }}`,
          description: "E-commerce order summary",
        },
      ],
    },
    {
      title: "Event Reminder",
      description: "Automated event reminder",
      examples: [
        {
          code: `🗓️ Reminder: {{ event.name }}
Just a friendly reminder that "{{ event.name }}" is happening in {{ event.start_time | date.now | date.timespan.total_hours | math.round }} hours.
Location: {{ event.location | string.truncate 25 }}
See you there!`,
          description: "Automated event reminder",
        },
      ],
    },
    {
      title: "System Alert",
      description: "Multi-level system alert",
      examples: [
        {
          code: `{{- case alert.level -}}
{{- when "critical" -}}
🔥 CRITICAL ALERT
{{- when "warning" -}}
⚠️ Warning
{{- else -}}
ℹ️ Info
{{- end -}}
Service: {{ alert.service | string.upcase }}
Message: {{ alert.message }}
Time: {{ alert.timestamp | date.to_string "%H:%M:%S" }}`,
          description: "Multi-level system alert",
        },
      ],
    },
  ],
};

export const dateFormatPatterns: DateFormatPattern[] = [
  { pattern: "%Y", description: "4-digit year (2025)" },
  { pattern: "%m", description: "Month (01-12)" },
  { pattern: "%d", description: "Day (01-31)" },
  { pattern: "%H", description: "Hour 24h (00-23)" },
  { pattern: "%M", description: "Minutes (00-59)" },
  { pattern: "%b", description: "Short month (Jan)" },
  { pattern: "%B", description: "Full month (January)" },
];

export const tabOptions = [
  { value: "basics", label: "Basics" },
  { value: "strings", label: "Strings" },
  { value: "dates", label: "Dates" },
  { value: "arrays", label: "Arrays" },
  { value: "examples", label: "Examples" },
] as const;
