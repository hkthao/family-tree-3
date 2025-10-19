export interface SystemConfig {
  id: string;
  key: string;
  value?: any;
  valueType?: string; // e.g., 'string', 'int', 'bool', 'json'
  description?: string;
}