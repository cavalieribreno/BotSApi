import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BusinessHours } from '../models/business-hours';

// Reads and updates the company's operating hours schedule.
@Injectable({ providedIn: 'root' })
export class BusinessHoursService {
  private readonly api = 'http://localhost:5069/api/business-hours';

  constructor(private http: HttpClient) {}

  getBusinessHours(): Observable<BusinessHours[]> {
    return this.http.get<BusinessHours[]>(this.api);
  }

  updateBusinessHours(hours: BusinessHours[]): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(this.api, hours);
  }
}
