import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AppointmentService } from '../../services/appointment.service';
import { AuthService } from '../../services/auth.service';
import { Appointment } from '../../models/appointment';

// Owner's view: loads the company's appointments and shows them in a table.
@Component({
  selector: 'app-appointments',
  imports: [DatePipe, RouterLink],
  templateUrl: './appointments.component.html'
})
export class AppointmentsComponent implements OnInit {
  appointments = signal<Appointment[]>([]);   // signals: async-set state the view renders (zoneless)
  loading = signal(true);

  constructor(
    private service: AppointmentService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.service.getAppointments().subscribe({
      next: data => {
        this.appointments.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  changeStatus(id: string, status: string): void {
    this.service.updateStatus(id, status).subscribe({
      next: () => this.load()   // re-fetch so the table reflects the new status
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  // Translate the API's enum name to a Portuguese label for display.
  private readonly statusLabels: Record<string, string> = {
    Pending: 'Pendente',
    Confirmed: 'Confirmado',
    Cancelled: 'Cancelado',
    Completed: 'Concluído'
  };

  statusLabel(status: string): string {
    return this.statusLabels[status] ?? status;
  }
}
