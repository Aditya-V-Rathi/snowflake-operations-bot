import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuditLog {
  id: number;
  slackUserId: string;
  slackUsername: string;
  command: string;
  parameters: string;
  status: string;
  message: string;
  timestamp: string;
}

export interface ManagedUser {
  id: number;
  username: string;
  role: string;
  createdBy: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private baseUrl = 'http://localhost:5170/api/audit';

  constructor(private http: HttpClient) {}

  getLogs(): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.baseUrl}/logs`);
  }

  getManagedUsers(): Observable<ManagedUser[]> {
    return this.http.get<ManagedUser[]>(`${this.baseUrl}/users`);
  }
}