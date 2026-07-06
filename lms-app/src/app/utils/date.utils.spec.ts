import { describe, it, expect } from 'vitest';
import { nowIST, toISTISOString } from './date.utils';

describe('DateUtils', () => {
  describe('nowIST', () => {
    it('should return a valid ISO string', () => {
      const result = nowIST();
      expect(result).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/);
      
      const parsedDate = new Date(result);
      expect(isNaN(parsedDate.getTime())).toBeFalsy();
    });
  });

  describe('toISTISOString', () => {
    it('should return a string ending in +05:30', () => {
      const result = toISTISOString();
      expect(result.endsWith('+05:30')).toBeTruthy();
      
      const parsedDate = new Date(result);
      expect(isNaN(parsedDate.getTime())).toBeFalsy();
    });
  });
});
