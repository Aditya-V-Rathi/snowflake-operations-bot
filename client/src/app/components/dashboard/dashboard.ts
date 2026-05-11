import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { AuditService, AuditLog, ManagedUser } from '../../services/audit';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
  standalone: false
})
export class DashboardComponent implements OnInit, OnDestroy {
  // Tab management
  activeTab: 'logs' | 'users' = 'logs';

  // Logs tab
  logs: AuditLog[] = [];
  filteredLogs: AuditLog[] = [];
  isLoading = true;
  autoRefresh = true;
  private refreshInterval: any;

  // Filters
  selectedStatus = '';
  selectedCommand = '';

  // Stats
  totalLogs = 0;
  successCount = 0;
  failedCount = 0;
  lastActivity = 'No activity yet';

  // Users tab
  managedUsers: ManagedUser[] = [];
  isLoadingUsers = false;

  constructor(private auditService: AuditService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.fetchLogs();
    this.fetchManagedUsers();
    this.refreshInterval = setInterval(() => {
      if (this.activeTab === 'logs') {
        this.fetchLogs();
      } else {
        this.fetchManagedUsers();
      }
    }, 60000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  switchTab(tab: 'logs' | 'users'): void {
    this.activeTab = tab;
    this.cdr.detectChanges();
  }

  // Logs methods
  fetchLogs(): void {
    this.auditService.getLogs().subscribe({
      next: (data) => {
        this.logs = data;
        this.applyFilters();

        // Compute stats
        this.totalLogs = this.logs.length;
        this.successCount = this.logs.filter(l => l.status === 'success').length;
        this.failedCount = this.logs.filter(l => l.status === 'failed').length;
        this.lastActivity = this.logs.length > 0
          ? new Date(this.logs[0].timestamp).toLocaleString()
          : 'No activity yet';

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching logs', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    this.filteredLogs = this.logs.filter(log => {
      const statusMatch = this.selectedStatus ? log.status === this.selectedStatus : true;
      const commandMatch = this.selectedCommand ? log.command === this.selectedCommand : true;
      return statusMatch && commandMatch;
    });
    this.cdr.detectChanges();
  }

  toggleAutoRefresh(): void {
    this.autoRefresh = !this.autoRefresh;
    if (this.autoRefresh) {
      this.refreshInterval = setInterval(() => {
        if (this.activeTab === 'logs') {
          this.fetchLogs();
        } else {
          this.fetchManagedUsers();
        }
      }, 60000);
    } else {
      clearInterval(this.refreshInterval);
    }
    this.cdr.detectChanges();
  }

  resetFilters(): void {
    this.selectedStatus = '';
    this.selectedCommand = '';
    this.applyFilters();
  }

  // Users methods
  fetchManagedUsers(): void {
    this.isLoadingUsers = true;
    this.auditService.getManagedUsers().subscribe({
      next: (data) => {
        this.managedUsers = data;
        this.isLoadingUsers = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching managed users', err);
        this.isLoadingUsers = false;
        this.cdr.detectChanges();
      }
    });
  }
}