import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../services/appointment.service';
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
  error = signal('');

  constructor(private service: AppointmentService) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set('');
    this.service.getAppointments().subscribe({
      next: data => {
        this.appointments.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Não foi possível carregar os agendamentos.');
        this.loading.set(false);
      }
    });
  }

  changeStatus(id: string, status: string): void {
    this.service.updateStatus(id, status).subscribe({
      next: () => this.load(),   // re-fetch so the table reflects the new status
      error: () => this.error.set('Não foi possível atualizar o status. Tente de novo.')
    });
  }

  // Cancel needs a confirmation (destructive action).
  cancel(id: string): void {
    if (confirm('Cancelar este agendamento?')) {
      this.changeStatus(id, 'Cancelled');
    }
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

  // Count for the summary stats at the top.
  count(status: string): number {
    return this.appointments().filter(a => a.status === status).length;
  }
}
