import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Professional, CreateProfessionalRequest, UpdateProfessionalRequest, ProfessionalStatus } from '../models/professional';

@Injectable({ providedIn: 'root' })
export class ProfessionalService {
  private readonly api = 'http://localhost:5069/api/professionals';

  constructor(private http: HttpClient) {}

  getProfessionals(status?: ProfessionalStatus): Observable<Professional[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<Professional[]>(this.api, { params });
  }

  getProfessionalById(id: string): Observable<Professional> {
    return this.http.get<Professional>(`${this.api}/${id}`);
  }

  createProfessional(request: CreateProfessionalRequest): Observable<Professional> {
    return this.http.post<Professional>(this.api, request);
  }

  updateProfessional(id: string, request: UpdateProfessionalRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.api}/${id}`, request);
  }
}
