export interface SystemConfig {
  key: string;
  value: any;
  type: string; // e.g., 'string', 'boolean', 'integer', 'enum', 'json'
  description?: string;
  isReadOnly: boolean;
  options?: { value: string; text: string }[]; // For enum types
}