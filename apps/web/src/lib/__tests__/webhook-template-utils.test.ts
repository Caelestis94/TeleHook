/**
 * Tests for webhook template utilities
 *
 * To run these tests, you'll need to set up a testing framework like Jest:
 * npm install --save-dev jest @types/jest ts-jest
 *
 * Example jest.config.js:
 * module.exports = {
 *   preset: 'ts-jest',
 *   testEnvironment: 'node',
 *   roots: ['<rootDir>/src'],
 *   testMatch: ['**/ //__tests__/**//*.test.ts'],

import {
  formatScribanTemplate,
  extractSimplifiedVariables,
  getAvailableVariables,
  getSampleData,
} from "../webhook-template-utils";

describe("webhook-template-utils", () => {
  describe("formatScribanTemplate", () => {
    it("should format simple Scriban template with proper indentation", () => {
      const input = `{{if user.name}}
Hello {{user.name}}
{{else}}
Hello Guest
{{end}}`;

      const expected = `{{if user.name}}
  Hello {{user.name}}
{{else}}
  Hello Guest
{{end}}`;

      expect(formatScribanTemplate(input)).toBe(expected);
    });

    it("should handle single-line control structures", () => {
      const input = `{{if user.active}}Active{{end}}`;
      const expected = `{{if user.active}}Active{{end}}`;

      expect(formatScribanTemplate(input)).toBe(expected);
    });

    it("should preserve text blocks without Scriban syntax", () => {
      const input = `This is plain text
    with indentation
that should be preserved`;

      expect(formatScribanTemplate(input)).toBe(input);
    });

    it("should handle nested control structures", () => {
      const input = `{{for user in users}}
{{if user.active}}
Name: {{user.name}}
{{end}}
{{end}}`;

      const expected = `{{for user in users}}
  {{if user.active}}
    Name: {{user.name}}
  {{end}}
{{end}}`;

      expect(formatScribanTemplate(input)).toBe(expected);
    });
  });

  describe("extractSimplifiedVariables", () => {
    it("should extract variables from simple object", () => {
      const obj = {
        name: "John",
        age: 30,
        active: true,
      };

      const result = extractSimplifiedVariables(obj);

      expect(result).toEqual([
        { name: "name", type: "string", isArray: false },
        { name: "age", type: "number", isArray: false },
        { name: "active", type: "boolean", isArray: false },
      ]);
    });

    it("should identify arrays and objects", () => {
      const obj = {
        users: [{ name: "John" }],
        settings: { theme: "dark" },
        count: 5,
      };

      const result = extractSimplifiedVariables(obj);

      expect(result).toEqual([
        { name: "users", type: "array", isArray: true },
        { name: "settings", type: "object", isArray: false },
        { name: "count", type: "number", isArray: false },
      ]);
    });

    it("should handle empty object", () => {
      const result = extractSimplifiedVariables({});
      expect(result).toEqual([]);
    });
  });

  describe("getAvailableVariables", () => {
    const mockPayloadSample =
      '{"user": {"name": "John", "id": 123}, "message": "Hello"}';
    const emptyPayloadSample = null;
    
    it("should return variables from valid payload", () => {
      const result = getAvailableVariables(mockPayloadSample);

      expect(result).toEqual([
        { name: "user", type: "object", isArray: false },
        { name: "message", type: "string", isArray: false },
      ]);
    });

    it("should return empty array for non-existent payload", () => {
      const result = getAvailableVariables(emptyPayloadSample);
      expect(result).toEqual([]);
    });

    it("should handle invalid JSON in payload sample", () => {
      const invalidPayloadSample = "invalid json";

      const result = getAvailableVariables(invalidPayloadSample);
      expect(result).toEqual([]);
    });
  });

  describe("getSampleData", () => {
    const payloadSample = '{"user": {"name": "John"}, "count": 5}';

    it("should return parsed sample data", () => {
      const result = getSampleData(payloadSample);

      expect(result).toEqual({
        user: { name: "John" },
        count: 5,
      });
    });

    it("should return empty object for non-existent schema", () => {
      const result = getSampleData(null);
      expect(result).toEqual({});
    });

    it("should return empty object for invalid JSON", () => {
      const invalidPayloadSample = "invalid json";

      const result = getSampleData(invalidPayloadSample);
      expect(result).toEqual({});
    });
  });
});
