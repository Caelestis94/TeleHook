export interface VariableInfo {
  name: string;
  type: string;
  isArray: boolean;
}

/**
 * Enhanced Scriban formatter function based on official language specification
 */
export function formatScribanTemplate(template: string): string {
  let indentLevel = 0;
  const indentSize = 2;

  const lines = template.split("\n");
  const formattedLines: string[] = [];

  for (const line of lines) {
    const trimmedLine = line.trim();

    // Preserve empty lines
    if (!trimmedLine) {
      formattedLines.push("");
      continue;
    }

    // Handle text blocks (non-code blocks)
    if (!trimmedLine.includes("{{") && !trimmedLine.includes("}}")) {
      if (indentLevel > 0) {
        // Inside control structures - apply current indentation
        const indent = " ".repeat(indentLevel * indentSize);
        formattedLines.push(indent + trimmedLine);
      } else {
        // Outside control structures - preserve original spacing
        formattedLines.push(line);
      }
      continue;
    }

    // Check if this is a single-line control structure (has both opening and {{ end }} on same line)
    const isSingleLineControl = trimmedLine.match(
      /{{\s*(?:if|for|while|case)\b.*}}\s*.*{{\s*(?:else\b.*}}.*)?{{\s*end\s*}}/
    );

    if (isSingleLineControl) {
      // Single-line control structure - apply current indentation and don't change indent level
      const indent = " ".repeat(indentLevel * indentSize);
      formattedLines.push(indent + trimmedLine);
      continue;
    }

    // Check for statements that decrease indentation BEFORE adding the line
    // Only for multi-line control structures
    if (trimmedLine.match(/^{{\s*(?:end|else(?:\s+if)?|when)\s*}}$/)) {
      indentLevel = Math.max(0, indentLevel - 1);
    }

    // Apply indentation
    const indent = " ".repeat(indentLevel * indentSize);
    formattedLines.push(indent + trimmedLine);

    // Check for statements that increase indentation AFTER adding the line
    // Only for multi-line control structures (those that don't end with {{ end }} on same line)
    if (
      trimmedLine.match(/^{{\s*(?:if|for|while|case|tablerow|func)\b/) &&
      !trimmedLine.includes("{{ end }}")
    ) {
      indentLevel++;
    }

    // Handle else and else if - they stay at current level but content after should be indented
    if (
      trimmedLine.match(/^{{\s*else(?:\s+if\b.*?)?\s*}}$/) &&
      !isSingleLineControl
    ) {
      indentLevel++;
    }
  }

  return formattedLines.join("\n");
}

/**
 * Extract simplified variables from a JSON object for template autocomplete
 */
export function extractSimplifiedVariables(obj: unknown): VariableInfo[] {
  const variables: VariableInfo[] = [];

  for (const [key, value] of Object.entries(obj)) {
    if (Array.isArray(value)) {
      variables.push({
        name: key,
        type: "array",
        isArray: true,
      });
    } else if (value && typeof value === "object") {
      variables.push({
        name: key,
        type: "object",
        isArray: false,
      });
    } else {
      variables.push({
        name: key,
        type: typeof value,
        isArray: false,
      });
    }
  }

  return variables;
}

/**
 * Get available variables from a schema by parsing its payload sample
 */
export function getAvailableVariables(payloadSample: string): VariableInfo[] {
  if (!payloadSample) return [];

  try {
    const parsedPayloadSample = JSON.parse(payloadSample);
    return extractSimplifiedVariables(parsedPayloadSample);
  } catch {
    return [];
  }
}

/**
 * Get sample data from a schema for template preview
 */
export function getSampleData(payloadSample: string): Record<string, unknown> {
  if (!payloadSample) return {};

  try {
    return JSON.parse(payloadSample);
  } catch {
    return {};
  }
}
