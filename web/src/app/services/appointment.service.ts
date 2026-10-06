import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/env.production';
import { Appointment, CreateAppointmentRequest } from '../models/appointment';

// Reads and manages the company's appointments (the owner's view).
@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly api = `${environment.apiUrl}/appointments`;

  constructor(private http: HttpClient) {}

  getAppointments(date?: string): Observable<Appointment[]> {
    let url = this.api;
    if (date) {
      url = `${this.api}?date=${date}`;
    }
    return this.http.get<Appointment[]>(url);
  }

  createAppointment(request: CreateAppointmentRequest): Observable<Appointment> {
    return this.http.post<Appointment>(this.api, request);
  }

  // PATCH the status. Backend expects [FromBody] string -> body must be a JSON string ("Confirmed").
  updateStatus(id: string, status: string): Observable<void> {
    return this.http.patch<void>(
      `${this.api}/${id}/status`,
      JSON.stringify(status),
      { headers: { 'Content-Type': 'application/json' } }
    );
  }

  getAppointmentsByCustomer(phone: string): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.api}/customer?phone=${encodeURIComponent(phone)}`);
  }
}
