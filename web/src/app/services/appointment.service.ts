import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Appointment } from '../models/appointment';

// Reads the company's appointments (the owner's view).
@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly api = 'http://localhost:5069/api/appointments';

  constructor(private http: HttpClient) {}

  getAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(this.api);
  }

  // PATCH the status. Backend expects [FromBody] string -> body must be a JSON string ("Confirmed").
  updateStatus(id: string, status: string): Observable<void> {
    return this.http.patch<void>(
      `${this.api}/${id}/status`,
      JSON.stringify(status),
      { headers: { 'Content-Type': 'application/json' } }
    );
  }
}
