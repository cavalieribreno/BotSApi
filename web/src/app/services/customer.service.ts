import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/env.production';
import { Customer, CreateCustomerRequest } from '../models/customer';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly api = `${environment.apiUrl}/customers`;

  constructor(private http: HttpClient) {}

  getCustomers(): Observable<Customer[]> {
    return this.http.get<Customer[]>(this.api);
  }

  getCustomerById(id: string): Observable<Customer> {
    return this.http.get<Customer>(`${this.api}/${id}`);
  }

  createCustomer(request: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<Customer>(this.api, request);
  }
}
