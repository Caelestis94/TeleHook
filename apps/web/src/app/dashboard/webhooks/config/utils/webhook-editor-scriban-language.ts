import { languages, Thenable } from "monaco-editor";

// Define Scriban language configuration
interface Variable {
  name: string;
  type: string;
  isArray: boolean;
}

interface SchemaStructure {
  [key: string]: {
    type: string;
    isArray: boolean;
    properties?: SchemaStructure;
    itemProperties?: SchemaStructure;
  };
}

export const scribanLanguageConfig: languages.LanguageConfiguration = {
  brackets: [
    ["{", "}"],
    ["{{", "}}"],
    ["(", ")"],
    ["[", "]"],
  ],
  autoClosingPairs: [
    { open: "{", close: "}" },
    { open: "{{", close: "}}" },
    { open: "(", close: ")" },
    { open: "[", close: "]" },
    { open: '"', close: '"' },
    { open: "'", close: "'" },
  ],
  surroundingPairs: [
    { open: "{", close: "}" },
    { open: "{{", close: "}}" },
    { open: "(", close: ")" },
    { open: "[", close: "]" },
    { open: '"', close: '"' },
    { open: "'", close: "'" },
  ],
  comments: {
    blockComment: ["{{#", "}}"],
  },
  // Indentation rules based on official Scriban language specification
  indentationRules: {
    increaseIndentPattern:
      /^\s*{{\s*(if|for|while|case|tablerow|func)\b(?!.*{{\s*end\s*}})/,
    decreaseIndentPattern: /^\s*{{\s*(end|else(?:\s+if)?|when)\s*}}$/,
  },
  // Folding rules
  folding: {
    markers: {
      start: /{{\s*(if|for|while|case|tablerow|func)\b(?!.*{{\s*end\s*}})/,
      end: /{{\s*end\s*}}/,
    },
  },
};

// Define Scriban syntax highlighting
export const scribanTokensProvider:
  | languages.IMonarchLanguage
  | Thenable<languages.IMonarchLanguage> = {
  tokenizer: {
    root: [
      // Comments
      [/\{\{#.*?\}\}/, "comment.scriban"],

      // Template delimiters
      [/\{\{/, { token: "delimiter.scriban", next: "@scribanExpression" }],

      // Regular text
      [/[^{]+/, "text"],
      [/./, "text"],
    ],

    scribanExpression: [
      // End delimiter
      [/\}\}/, { token: "delimiter.scriban", next: "@pop" }],

      // Keywords based on official Scriban language specification
      [
        /\b(if|else|end|for|in|while|break|continue|capture|case|when|with|tablerow|assign|echo|include|raw|comment|func|ret)\b/,
        "keyword.scriban",
      ],

      // Operators
      [/(\|\||\&\&|==|!=|<=|>=|<|>|\+|\-|\*|\/|%|\||!)/, "operator.scriban"],

      // Filters (after pipe)
      [/\|\s*([a-zA-Z_][a-zA-Z0-9_]*)/, "function.scriban"],

      // Built-in functions
      [/\b(date|string|math|array|object|html|url|regex)\b/, "type.scriban"],

      // Properties and methods
      [/\.([a-zA-Z_][a-zA-Z0-9_]*)/, "property.scriban"],

      // Variables
      [/[a-zA-Z_][a-zA-Z0-9_]*/, "variable.scriban"],

      // Numbers
      [/\d+(\.\d+)?/, "number.scriban"],

      // Strings
      [/"([^"\\]|\\.)*$/, "string.invalid.scriban"],
      [/"/, "string.scriban", "@string"],
      [/'([^'\\]|\\.)*$/, "string.invalid.scriban"],
      [/'/, "string.scriban", "@stringApostrophe"],

      // Array/object access
      [/\[/, "delimiter.square.scriban"],
      [/\]/, "delimiter.square.scriban"],

      // Parentheses
      [/\(/, "delimiter.parenthesis.scriban"],
      [/\)/, "delimiter.parenthesis.scriban"],

      // Whitespace
      [/[ \t\r\n]+/, "white"],

      // Everything else
      [/./, "text"],
    ],

    string: [
      [/[^\\"]+/, "string.scriban"],
      [/\\./, "string.escape.scriban"],
      [/"/, "string.scriban", "@pop"],
    ],

    stringApostrophe: [
      [/[^\\']+/, "string.scriban"],
      [/\\./, "string.escape.scriban"],
      [/'/, "string.scriban", "@pop"],
    ],
  },
};

// Register Scriban language with Monaco
export function registerScribanLanguage(
  monaco: typeof import("monaco-editor")
) {
  // Register the language
  monaco.languages.register({ id: "scriban" });

  // Set language configuration
  monaco.languages.setLanguageConfiguration("scriban", scribanLanguageConfig);

  // Set tokens provider for syntax highlighting
  monaco.languages.setMonarchTokensProvider("scriban", scribanTokensProvider);
}

// Build nested schema structure from sample data for dot notation autocomplete
function buildSchemaStructure(
  obj: unknown,
  maxDepth = 4,
  currentDepth = 0
): SchemaStructure {
  if (currentDepth >= maxDepth || !obj || typeof obj !== "object") {
    return {};
  }

  const structure: SchemaStructure = {};

  for (const [key, value] of Object.entries(obj)) {
    if (Array.isArray(value)) {
      structure[key] = {
        type: "array",
        isArray: true,
        itemProperties:
          value.length > 0 &&
          typeof value[0] === "object" &&
          !Array.isArray(value[0])
            ? buildSchemaStructure(value[0], maxDepth, currentDepth + 1)
            : {},
      };
    } else if (value && typeof value === "object") {
      structure[key] = {
        type: "object",
        isArray: false,
        properties: buildSchemaStructure(value, maxDepth, currentDepth + 1),
      };
    } else {
      structure[key] = {
        type: typeof value,
        isArray: false,
      };
    }
  }

  return structure;
}

// Parse dot notation path and return available properties
function getPropertiesForPath(
  path: string,
  schemaStructure: SchemaStructure
): string[] {
  const parts = path.split(".");
  let current = schemaStructure;

  for (let i = 0; i < parts.length - 1; i++) {
    const part = parts[i];

    // Handle array indexing like "embeds[0]" or "embeds[*]"
    const arrayMatch = part.match(/^([^[]+)\[([^\]]*)\]$/);
    if (arrayMatch) {
      const arrayName = arrayMatch[1];
      if (current[arrayName]?.isArray && current[arrayName].itemProperties) {
        current = current[arrayName].itemProperties!;
        continue;
      }
    }

    // Handle regular property access
    if (current[part]) {
      if (current[part].properties) {
        current = current[part].properties!;
      } else if (current[part].isArray && current[part].itemProperties) {
        // If accessing an array without indexing, show item properties
        current = current[part].itemProperties!;
      } else {
        return []; // End of traversable path
      }
    } else {
      return []; // Property doesn't exist
    }
  }

  return Object.keys(current);
}

// Get type information for a specific property path
function getPropertyType(
  path: string,
  schemaStructure: SchemaStructure
): { type: string; isArray: boolean } | null {
  const parts = path.split(".");
  let current = schemaStructure;

  for (let i = 0; i < parts.length; i++) {
    const part = parts[i];

    // Handle array indexing
    const arrayMatch = part.match(/^([^[]+)\[([^\]]*)\]$/);
    if (arrayMatch) {
      const arrayName = arrayMatch[1];
      if (current[arrayName]?.isArray && current[arrayName].itemProperties) {
        if (i === parts.length - 1) {
          // This is the final part, return the item type
          return { type: "object", isArray: false };
        }
        current = current[arrayName].itemProperties!;
        continue;
      }
      return null;
    }

    if (current[part]) {
      if (i === parts.length - 1) {
        // Final part, return its type
        return { type: current[part].type, isArray: current[part].isArray };
      }

      if (current[part].properties) {
        current = current[part].properties!;
      } else if (current[part].isArray && current[part].itemProperties) {
        current = current[part].itemProperties!;
      } else {
        return null;
      }
    } else {
      return null;
    }
  }

  return null;
}

// Detect if we're inside a for loop and extract loop variables
function getLoopVariables(content: string, position: number): string[] {
  const textBeforeCursor = content.substring(0, position);
  const loopVariables: string[] = [];

  // Find all for loops that might contain the cursor
  const forLoopPattern =
    /{{\s*for\s+(\w+)(?:\s*,\s*(\w+))?\s+in\s+([^}]+)\s*}}/g;
  let match;

  while ((match = forLoopPattern.exec(textBeforeCursor)) !== null) {
    const loopStart = match.index + match[0].length;

    // Check if cursor is after this for loop (and potentially before its {{ end }})
    if (loopStart < position) {
      // Find the corresponding {{ end }}
      const remainingText = content.substring(loopStart);
      const endMatch = remainingText.match(/{{\s*end\s*}}/);

      if (!endMatch || loopStart + (endMatch.index || 0) > position) {
        // We're inside this loop
        loopVariables.push(match[1]); // Loop item variable
        if (match[2]) {
          loopVariables.push(match[2]); // Loop index variable (if present)
        }
      }
    }
  }

  return loopVariables;
}

export function createScribanCompletionProvider(
  monaco: typeof import("monaco-editor"),
  variables: Variable[] = [],
  sampleData: unknown = {}
) {
  return monaco.languages.registerCompletionItemProvider("scriban", {
    triggerCharacters: [".", " ", "{", "}"], // Trigger on dot, space, and braces
    provideCompletionItems: (model, position) => {
      const word = model.getWordUntilPosition(position);

      // Check if we're inside {{ }} delimiters
      const lineContent = model.getLineContent(position.lineNumber);
      const beforeCursor = lineContent.substring(0, position.column - 1);
      const afterCursor = lineContent.substring(position.column - 1);

      // Find the last {{ and first }} relative to cursor
      const lastOpenBrace = beforeCursor.lastIndexOf("{{");
      const lastCloseBrace = beforeCursor.lastIndexOf("}}");
      const nextCloseBrace = afterCursor.indexOf("}}");

      // We're inside {{ }} if we found {{ after the last }} and there's a }} ahead
      const isInsideBraces =
        lastOpenBrace > lastCloseBrace && nextCloseBrace !== -1;

      // Build schema structure for dot notation autocomplete
      const schemaStructure = buildSchemaStructure(sampleData);

      // Get loop variables if we're inside a for loop
      const fullText = model.getValue();
      const offset = model.getOffsetAt(position);
      const loopVariables = getLoopVariables(fullText, offset);

      // Check for dot notation context
      const expressionStart = Math.max(lastOpenBrace + 2, 0);
      const expressionText = beforeCursor.substring(expressionStart).trim();

      // Look for patterns like:
      // - "variable." (just typed a dot - show all properties)
      // - "array[0]." (array access with dot - show all properties)
      // - "parent.child." (nested property with dot - show all properties)
      // - "variable.prop" (partial property name being typed - filter properties)
      const justTypedDot = expressionText.match(
        /([a-zA-Z_][a-zA-Z0-9_]*(?:\[[^\]]*\])?(?:\.[a-zA-Z_][a-zA-Z0-9_]*(?:\[[^\]]*\])?)*)\.\s*$/
      );
      const typingProperty = expressionText.match(
        /([a-zA-Z_][a-zA-Z0-9_]*(?:\[[^\]]*\])?(?:\.[a-zA-Z_][a-zA-Z0-9_]*(?:\[[^\]]*\])?)*\.)([a-zA-Z_][a-zA-Z0-9_]*)$/
      );

      // Calculate proper range for property completions
      let range;
      if (typingProperty) {
        // User is typing a property name - replace from start of property name
        const partialPropertyLength = typingProperty[2].length;
        range = {
          startLineNumber: position.lineNumber,
          endLineNumber: position.lineNumber,
          startColumn: position.column - partialPropertyLength,
          endColumn: position.column,
        };
      } else {
        // Default range
        range = {
          startLineNumber: position.lineNumber,
          endLineNumber: position.lineNumber,
          startColumn: word.startColumn,
          endColumn: word.endColumn,
        };
      }

      let suggestions: languages.CompletionItem[] = [];

      if (isInsideBraces && (justTypedDot || typingProperty)) {
        // We're in dot notation context - show properties
        const basePath = justTypedDot
          ? justTypedDot[1]
          : typingProperty
          ? typingProperty[1].slice(0, -1)
          : "";
        const partialProperty = typingProperty ? typingProperty[2] : ""; // What user is currently typing

        // Check if this is a loop variable
        const baseVariable = basePath.split(".")[0].split("[")[0];
        const isLoopVariable = loopVariables.includes(baseVariable);

        let availableProperties: string[] = [];

        if (isLoopVariable) {
          // For loop variables, we need to infer the structure from the array they're iterating over
          // Find the array this loop variable comes from by looking at the for loop
          const loopArrayMatch = fullText.match(
            new RegExp(
              `\\{\\{\\s*for\\s+${baseVariable}(?:\\s*,\\s*\\w+)?\\s+in\\s+([^}]+)\\s*\\}\\}`
            )
          );

          if (loopArrayMatch) {
            const arrayPath = loopArrayMatch[1].trim();

            // Look for array item properties in schema structure
            const arrayDef = schemaStructure[arrayPath];

            if (arrayDef?.itemProperties) {
              availableProperties = Object.keys(arrayDef.itemProperties);
            } else {
              // Fallback: try to get properties directly from the sample data
              if (
                sampleData &&
                sampleData[arrayPath] &&
                Array.isArray(sampleData[arrayPath]) &&
                sampleData[arrayPath].length > 0
              ) {
                const firstItem = sampleData[arrayPath][0];
                availableProperties = Object.keys(firstItem);
              }
            }
          }
        } else {
          // Regular property path resolution
          availableProperties = getPropertiesForPath(
            basePath + ".",
            schemaStructure
          );
        }

        // Filter properties based on partial input (if any)
        const filteredProperties = partialProperty
          ? availableProperties.filter((prop) =>
              prop.toLowerCase().startsWith(partialProperty.toLowerCase())
            )
          : availableProperties; // Show all if just typed dot

        suggestions = filteredProperties.map((prop) => {
          const fullPath = isLoopVariable
            ? `${baseVariable}.${prop}`
            : `${basePath}.${prop}`;

          let propType: { type: string; isArray: boolean } | string | null =
            null;
          if (isLoopVariable) {
            // For loop variables, try to get type from array item properties
            const loopArrayMatch = fullText.match(
              new RegExp(
                `\\{\\{\\s*for\\s+${baseVariable}(?:\\s*,\\s*\\w+)?\\s+in\\s+([^}]+)\\s*\\}\\}`
              )
            );
            if (loopArrayMatch) {
              const arrayPath = loopArrayMatch[1].trim();
              const arrayDef = schemaStructure[arrayPath];
              if (arrayDef?.itemProperties?.[prop]) {
                const itemProp = arrayDef.itemProperties[prop];
                propType = { type: itemProp.type, isArray: itemProp.isArray };
              } else {
                propType = { type: "string", isArray: false }; // Default
              }
            }
          } else {
            propType = getPropertyType(fullPath, schemaStructure);
          }

          const isArray =
            propType && typeof propType === "object" ? propType.isArray : false;
          const typeStr =
            propType && typeof propType === "object"
              ? propType.type
              : propType || "unknown";
          const documentation = `Property: ${prop}${isArray ? " (array)" : ""}${
            typeStr ? ` - ${typeStr}` : ""
          }`;

          return {
            label: prop,
            kind: isArray
              ? monaco.languages.CompletionItemKind.Field
              : monaco.languages.CompletionItemKind.Property,
            insertText: prop,
            documentation,
            range,
            sortText: "0" + prop,
          };
        });

        // Add array indexing suggestions for arrays
        const currentPropType = isLoopVariable
          ? { type: "object", isArray: false } // Loop variables are typically objects
          : getPropertyType(basePath, schemaStructure);

        if (currentPropType?.isArray) {
          suggestions.push(
            {
              label: "[0]",
              kind: monaco.languages.CompletionItemKind.Operator,
              insertText: "[0]",
              documentation: "Access first array element",
              range,
              sortText: "1[0]",
            },
            {
              label: "[i]",
              kind: monaco.languages.CompletionItemKind.Snippet,
              insertText: "[${1:index}]",
              insertTextRules:
                monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              documentation: "Access array element by index",
              range,
              sortText: "1[i]",
            }
          );
        }

        // Add for loop suggestions when dealing with arrays
        if (currentPropType?.isArray && !isInsideBraces) {
          suggestions.push({
            label: `for loop over ${basePath}`,
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: `{{ for item in ${basePath} }}\n\${1:content with {{ item.property }}}\n{{ end }}`,
            insertTextRules:
              monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            documentation: `Create a for loop to iterate over ${basePath}`,
            range,
            sortText: "3for_" + basePath,
          });
        }
      } else {
        // Regular autocomplete (variables, keywords, functions)
        suggestions = [
          // Loop variables (highest priority if inside a loop)
          ...loopVariables.map((loopVar) => ({
            label: isInsideBraces ? loopVar : `{{ ${loopVar} }}`,
            kind: monaco.languages.CompletionItemKind.Variable,
            insertText: isInsideBraces ? loopVar : `{{ ${loopVar} }}`,
            documentation: `Loop variable: ${loopVar}`,
            range,
            sortText: "00" + loopVar, // Highest priority
          })),

          // Variables from schema
          ...variables.map((variable) => ({
            label: isInsideBraces ? variable.name : `{{ ${variable.name} }}`,
            kind: monaco.languages.CompletionItemKind.Variable,
            insertText: isInsideBraces
              ? variable.name
              : `{{ ${variable.name} }}`,
            documentation: `${
              variable.isArray
                ? "Array"
                : variable.type.charAt(0).toUpperCase() + variable.type.slice(1)
            } variable: ${variable.name}`,
            range,
            sortText: "0" + variable.name,
          })),

          // Scriban keywords (only when inside braces) - based on official documentation
          ...(isInsideBraces
            ? [
                "if",
                "else",
                "end",
                "for",
                "in",
                "while",
                "break",
                "continue",
                "case",
                "when",
                "func",
                "ret",
              ].map((keyword) => ({
                label: keyword,
                kind: monaco.languages.CompletionItemKind.Keyword,
                insertText: keyword,
                documentation: `Scriban keyword: ${keyword}`,
                range,
                sortText: "1" + keyword,
              }))
            : []),

          // Scriban functions (only when inside braces) - Based on official documentation
          ...(isInsideBraces
            ? [
                // Date functions
                {
                  name: "date.now",
                  desc: "Current date/time - usage: date.now",
                },
                {
                  name: "date.add_days",
                  desc: "Add days to date - usage: date.add_days 7",
                },
                {
                  name: "date.add_months",
                  desc: "Add months to date - usage: date.add_months 1",
                },
                {
                  name: "date.add_years",
                  desc: "Add years to date - usage: date.add_years 1",
                },
                {
                  name: "date.add_hours",
                  desc: "Add hours to date - usage: date.add_hours -5",
                },
                {
                  name: "date.add_minutes",
                  desc: "Add minutes to date - usage: date.add_minutes 30",
                },
                {
                  name: "date.add_seconds",
                  desc: "Add seconds to date - usage: date.add_seconds 15",
                },
                {
                  name: "date.parse",
                  desc: 'Parse date string - usage: date.parse "2016/01/05"',
                },
                {
                  name: "date.to_string",
                  desc: 'Format date - usage: date.to_string "%d %b %Y"',
                },

                // String functions
                { name: "string.upcase", desc: "Convert to uppercase" },
                { name: "string.downcase", desc: "Convert to lowercase" },
                { name: "string.capitalize", desc: "Capitalize first letter" },
                {
                  name: "string.capitalizewords",
                  desc: "Capitalize each word",
                },
                {
                  name: "string.truncate",
                  desc: "Truncate string - usage: string.truncate 50",
                },
                {
                  name: "string.replace",
                  desc: 'Replace text - usage: string.replace "old" "new"',
                },
                {
                  name: "string.append",
                  desc: 'Append text - usage: string.append " suffix"',
                },
                {
                  name: "string.prepend",
                  desc: 'Prepend text - usage: string.prepend "prefix "',
                },
                {
                  name: "string.strip",
                  desc: "Remove whitespace from both ends",
                },
                { name: "string.lstrip", desc: "Remove whitespace from left" },
                { name: "string.rstrip", desc: "Remove whitespace from right" },
                { name: "string.size", desc: "Get string length" },
                {
                  name: "string.contains",
                  desc: 'Check if contains text - usage: string.contains "search"',
                },
                {
                  name: "string.starts_with",
                  desc: 'Check if starts with - usage: string.starts_with "prefix"',
                },
                {
                  name: "string.ends_with",
                  desc: 'Check if ends with - usage: string.ends_with "suffix"',
                },
                {
                  name: "string.split",
                  desc: 'Split string - usage: string.split ","',
                },
                { name: "string.empty", desc: "Check if string is empty" },

                // Math functions
                {
                  name: "math.round",
                  desc: "Round number - usage: math.round (for no decimals)",
                },
                { name: "math.ceil", desc: "Round up to nearest integer" },
                { name: "math.floor", desc: "Round down to nearest integer" },
                { name: "math.abs", desc: "Get absolute value" },
                {
                  name: "math.format",
                  desc: 'Format number - usage: math.format "C2" (currency)',
                },

                // Array functions
                { name: "array.size", desc: "Get array length" },
                { name: "array.first", desc: "Get first element" },
                { name: "array.last", desc: "Get last element" },
                { name: "array.reverse", desc: "Reverse array order" },
                {
                  name: "array.sort",
                  desc: 'Sort array - usage: array.sort "property_name"',
                },
                { name: "array.uniq", desc: "Get unique elements" },
                {
                  name: "array.contains",
                  desc: "Check if contains item - usage: array.contains item",
                },
                {
                  name: "array.add",
                  desc: "Add item to array - usage: array.add value",
                },
                {
                  name: "array.join",
                  desc: 'Join array elements - usage: array.join "|"',
                },
                {
                  name: "array.map",
                  desc: 'Extract property from objects - usage: array.map "property"',
                },
                {
                  name: "array.filter",
                  desc: "Filter array - usage: array.filter @function",
                },
                {
                  name: "array.limit",
                  desc: "Limit array size - usage: array.limit 10",
                },
                {
                  name: "array.offset",
                  desc: "Skip elements - usage: array.offset 5",
                },
                { name: "array.compact", desc: "Remove null values" },
                {
                  name: "array.concat",
                  desc: "Concatenate arrays - usage: array.concat other_array",
                },
                {
                  name: "array.cycle",
                  desc: 'Cycle through values - usage: array.cycle ["a", "b", "c"]',
                },
                {
                  name: "array.any",
                  desc: "Check if any match - usage: array.any @function",
                },
                {
                  name: "array.each",
                  desc: "Transform each element - usage: array.each @function",
                },

                // HTML functions
                { name: "html.strip", desc: "Remove HTML tags" },
                { name: "html.escape", desc: "Escape HTML characters" },
                {
                  name: "html.newline_to_br",
                  desc: "Convert newlines to <br> tags",
                },
                { name: "html.url_encode", desc: "URL encode string" },
                { name: "html.url_escape", desc: "URL escape string" },

                // Object functions
                { name: "object.keys", desc: "Get object property names" },
                { name: "object.values", desc: "Get object property values" },
                { name: "object.size", desc: "Get number of properties" },
                {
                  name: "object.has_key",
                  desc: 'Check if has property - usage: object.has_key "prop"',
                },

                // Regex functions
                {
                  name: "regex.match",
                  desc: "Match regex pattern - usage: regex.match pattern",
                },
                {
                  name: "regex.replace",
                  desc: "Replace with regex - usage: regex.replace pattern replacement",
                },
                {
                  name: "regex.split",
                  desc: "Split with regex - usage: regex.split pattern",
                },
              ].map((func) => ({
                label: func.name,
                kind: monaco.languages.CompletionItemKind.Function,
                insertText: func.name,
                documentation: `Scriban function: ${func.desc}`,
                range,
                sortText: "2" + func.name,
              }))
            : []),

          // Template snippets (only when outside braces)
          ...(!isInsideBraces
            ? [
                {
                  label: "if statement",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText:
                    "{{ if ${1:condition} }}\n${2:content}\n{{ end }}",
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation: "Scriban if statement",
                  range,
                  sortText: "3if",
                },
                {
                  label: "for loop",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText:
                    "{{ for ${1:item} in ${2:array} }}\n${3:content}\n{{ end }}",
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation: "Scriban for loop",
                  range,
                  sortText: "3for",
                },
                {
                  label: "variable with fallback",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText: '{{ ${1:variable} ?? "${2:default_value}" }}',
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation: "Variable with fallback value",
                  range,
                  sortText: "3fallback",
                },
                {
                  label: "current date formatted",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText: '{{ date.now | date.to_string "${1:%d %b %Y}" }}',
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation:
                    "Current date formatted (correct Scriban syntax)",
                  range,
                  sortText: "3date",
                },
                {
                  label: "string truncate",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText: "{{ ${1:variable} | string.truncate ${2:50} }}",
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation: "Truncate string to specified length",
                  range,
                  sortText: "3truncate",
                },
                {
                  label: "string replace",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText:
                    '{{ ${1:variable} | string.replace "${2:old}" "${3:new}" }}',
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation: "Replace text in string",
                  range,
                  sortText: "3replace",
                },
                {
                  label: "array join",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText: '{{ ${1:array} | array.join "${2:|}" }}',
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation: "Join array elements with delimiter",
                  range,
                  sortText: "3join",
                },
                {
                  label: "conditional text",
                  kind: monaco.languages.CompletionItemKind.Snippet,
                  insertText:
                    "{{ if ${1:condition} }}${2:true_text}{{ else }}${3:false_text}{{ end }}",
                  insertTextRules:
                    monaco.languages.CompletionItemInsertTextRule
                      .InsertAsSnippet,
                  documentation: "Conditional text with if/else",
                  range,
                  sortText: "3conditional",
                },
              ]
            : []),
        ];
      }

      return { suggestions };
    },
  });
}
