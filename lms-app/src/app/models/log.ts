export interface ActivityLogResponse {
  id: number;
  userId: number;
  userEmail: string;
  userName: string;
  activityType: string;
  description: string;
  timestamp: string;
}

export interface AuditLogResponse {
  id: number;
  userId: number;
  userEmail: string;
  userName: string;
  tableName: string;
  recordId: number;
  action: string;
  oldValues: string;
  newValues: string;
  timestamp: string;
}
