/**
 * Searches an array of error messages for specific keywords and returns all matching errors.
 * This is useful for mapping one or more backend validation errors to a specific form field.
 * @param {string[]} errors - An array of error detail strings from the API.
 * @param {string | string[]} keywords - The keyword(s) to search for (e.g., "Name", ["BotToken", "Token"]). The search is case-insensitive.
 * @returns {string[]} An array of matching error messages. The array will be empty if no matches are found.
 */
export const getFieldErrors = (errors: string[] | undefined, keywords: string | string[]): string[] => {
  if (!errors || errors.length === 0) {
    return []; 
  }
  
  const keywordArray = Array.isArray(keywords) ? keywords : [keywords];
  const lowerCaseKeywords = keywordArray.map(k => k.toLowerCase());
  
  return errors.filter(error => 
    lowerCaseKeywords.some(keyword => error.toLowerCase().includes(keyword))
  );
};

/**
 * Humanizes backend field names to be more user-friendly.
 * @param {string} error - The error message to humanize.
 * @returns {string} The humanized error message.
 */
export const humanizeFieldName = (error: string): string => {
  const replacements: Record<string, string> = {
    'BotToken': 'Bot Token',
    'ChatId': 'Chat ID',
    'MessageTemplate': 'Message Template',
    'ParseMode': 'Parse Mode',
    'DisableWebPagePreview': 'Disable Web Page Preview',
    'DisableNotification': 'Disable Notification',
    'TopicId': 'Topic ID',
    'PayloadSample': 'Payload Sample',
  };

  let humanizedError = error;
  for (const [backend, human] of Object.entries(replacements)) {
    const regex = new RegExp(`\\b${backend}\\b`, 'gi');
    humanizedError = humanizedError.replace(regex, human);
  }
  
  return humanizedError;
};

/**
 * Prioritizes errors by showing "required" errors first and filtering out other errors when a required error exists.
 * @param {string[]} errors - Array of error messages for a single field.
 * @returns {string[]} Filtered and prioritized error messages.
 */
export const prioritizeErrors = (errors: string[]): string[] => {
  if (errors.length === 0) return errors;
  
  // Check if any error contains "required"
  const requiredErrors = errors.filter(error => 
    error.toLowerCase().includes('required') || 
    error.toLowerCase().includes('is required')
  );
  
  // If we have required errors, only show those
  if (requiredErrors.length > 0) {
    return requiredErrors;
  }
  
  // Otherwise return all errors
  return errors;
};

/**
 * Maps backend API errors to form field errors for easier consumption in forms.
 * @param {string[]} errors - An array of error detail strings from the API.
 * @param {Record<string, string | string[]>} fieldMap - A mapping of form field names to backend keywords.
 * @returns {Record<string, string[]>} An object with form field names as keys and arrays of error messages as values.
 */
export const mapApiErrorsToFields = (
  errors: string[] | undefined, 
  fieldMap: Record<string, string | string[]>
): Record<string, string[]> => {
  if (!errors || errors.length === 0) {
    return {};
  }

  const result: Record<string, string[]> = {};
  
  for (const [fieldName, keywords] of Object.entries(fieldMap)) {
    const fieldErrors = getFieldErrors(errors, keywords);
    if (fieldErrors.length > 0) {
      // Prioritize errors (required takes precedence)
      const prioritizedErrors = prioritizeErrors(fieldErrors);
      // Humanize field names in error messages
      const humanizedErrors = prioritizedErrors.map(humanizeFieldName);
      result[fieldName] = humanizedErrors;
    }
  }
  
  return result;
};